using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;
using WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkRoleVMs;

namespace WalkingTec.Mvvm.Mvc.Admin.Controllers
{
    [Area("_Admin")]
    [ActionDescription("MenuKey.RoleManagement")]
    public class FrameworkRoleController : BaseController
    {
        #region 查询
        [ActionDescription("Sys.Search")]
        public ActionResult Index()
        {
            var vm = Wtm.CreateVM<FrameworkRoleListVM>();
            return PartialView(vm);
        }
        [ActionDescription("Sys.Search")]
        [HttpPost]
        public string Search(FrameworkRoleSearcher searcher)
        {
            var vm = Wtm.CreateVM<FrameworkRoleListVM>(passInit: true);
            if (ModelState.IsValid)
            {
                vm.Searcher = searcher;
                return vm.GetJson(false);
            }
            else
            {
                return vm.GetError();
            }
        }
        #endregion

        #region 新建
        [ActionDescription("Sys.Create")]
        public ActionResult Create()
        {
            var vm = Wtm.CreateVM<FrameworkRoleVM>();
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.Create")]
        public ActionResult Create(FrameworkRoleVM vm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(vm);
            }
            else
            {
                vm.DoAdd();
                return FFResult().CloseDialog().RefreshGrid();
            }
        }
        #endregion

        #region 修改
        [ActionDescription("Sys.Edit")]
        public ActionResult Edit(Guid id)
        {
            var vm = Wtm.CreateVM<FrameworkRoleVM>(id);
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.Edit")]
        [ValidateFormItemOnly]
        public ActionResult Edit(FrameworkRoleVM vm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(vm);
            }
            else
            {
                vm.DoEdit();
                return FFResult().CloseDialog().RefreshGrid();
            }
        }
        #endregion

        #region 删除
        [ActionDescription("Sys.Delete")]
        public ActionResult Delete(Guid id)
        {
            var vm = Wtm.CreateVM<FrameworkRoleVM>(id);
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.Delete")]
        public async Task<ActionResult> Delete(Guid id, IFormCollection noUse)
        {
            var vm = Wtm.CreateVM<FrameworkRoleVM>(id);
            vm.DoDelete();
            if (!ModelState.IsValid)
            {
                var userids = DC.Set<FrameworkUserRole>().Where(x => DC.Set<FrameworkRole>().Where(y => y.ID == id).Select(y => y.RoleCode).FirstOrDefault() == x.RoleCode).Select(x => x.UserCode).ToArray();
                await Wtm.RemoveUserCache(userids);
                return PartialView(vm);
            }
            else
            {
                return FFResult().CloseDialog().RefreshGrid();
            }
        }
        #endregion

        #region 批量删除
        [HttpPost]
        [ActionDescription("Sys.BatchDelete")]
        public ActionResult BatchDelete(Guid[] ids)
        {
            var vm = Wtm.CreateVM<FrameworkRoleBatchVM>(Ids: ids);
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.BatchDelete")]
        public async Task<ActionResult> DoBatchDelete(FrameworkRoleBatchVM vm, IFormCollection noUse)
        {
            if (!ModelState.IsValid || !vm.DoBatchDelete())
            {
                return PartialView("BatchDelete", vm);
            }
            else
            {
                List<Guid?> roleids = new List<Guid?>();
                foreach (var item in vm?.Ids)
                {
                    roleids.Add(Guid.Parse(item));
                }
                var userids = DC.Set<FrameworkUserRole>().Where(x => DC.Set<FrameworkRole>().Where(y => roleids.Contains(y.ID)).Select(y => y.RoleCode).Contains(x.RoleCode)).Select(x => x.UserCode).ToArray();
                await Wtm.RemoveUserCache(userids);
                return FFResult().CloseDialog().RefreshGrid();
            }
        }
        #endregion

        #region 导入
        [ActionDescription("Sys.Import")]
        public ActionResult Import()
        {
            var vm = Wtm.CreateVM<FrameworkRoleImportVM>();
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.Import")]
        public ActionResult Import(FrameworkRoleImportVM vm, IFormCollection nouse)
        {
            if (vm.ErrorListVM.EntityList.Count > 0 || !vm.BatchSaveData())
            {
                return PartialView(vm);
            }
            else
            {
                return FFResult().CloseDialog().RefreshGrid().Alert(Localizer["Sys.ImportSuccess", vm.EntityList.Count.ToString()]);
            }
        }
        #endregion

        #region 详细
        [ActionDescription("Sys.Details")]
        public PartialViewResult Details(Guid id)
        {
            var role = Wtm.CreateVM<FrameworkRoleMDVM>(id);
            role.ListVM.SearcherMode = ListVMSearchModeEnum.Custom1;
            return PartialView(role);
        }
        #endregion

        #region 页面权限
        [ActionDescription("_Admin.PageFunction")]
        public PartialViewResult PageFunction(Guid id)
        {
            var role = Wtm.CreateVM<FrameworkRoleMDVM>(id);
            role.ListVM.SearcherMode = ListVMSearchModeEnum.Custom2;
            return PartialView(role);
        }

        [ActionDescription("_Admin.PageFunction")]
        [HttpPost]
        public async Task<ActionResult> PageFunction(FrameworkRoleMDVM vm, IFormCollection noUse)
        {
            await vm.DoChangeAsync();
            return FFResult().CloseDialog().Alert(Localizer["Sys.OprationSuccess"]);
        }
        #endregion

        [ActionDescription("Sys.Export")]
        [HttpPost]
        public IActionResult ExportExcel(FrameworkRoleListVM vm)
        {
            return vm.GetExportData();
        }
    }
}