using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();
		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);
		protected override IBusinessObjectCollection GetNewGridCollection() => new EU.Business.Declaration.JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

#if DEBUG
		#region CopyAndSubmitToCustomsMenuItem

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (Env.CurrentUser.IsDeveloper)
			{
				var newCopyAndSubmitToCustomsMenuItemName = ResString.GetMultilingualString("6AE91AFF-5ABC-4AB2-914B-AF598367F5D6", "Copy and Submit To Customs");
				var newCopyAndSubmitToCustomsMenuItem = new ZMenuItem(newCopyAndSubmitToCustomsMenuItemName, (s, e) => ShowCopyAndSubmitToCustomsForm());
				newCopyAndSubmitToCustomsMenuItem.Name = newCopyAndSubmitToCustomsMenuItemName;
				result.Insert(0, newCopyAndSubmitToCustomsMenuItem);
			}

			return result.ToArray();
		}

		void ShowCopyAndSubmitToCustomsForm()
		{
			var declarations = GetSelectedElements();
			if (declarations.FirstOrDefault() is JobDeclaration jobDeclarationOriginal)
			{
				CopyAndSendToCustomsForm.ShowForm(jobDeclarationOriginal);
			}
		}
		#endregion
#endif
	}
}
