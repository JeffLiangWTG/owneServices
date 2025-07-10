using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public class ExitControlReportController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.EU.ExitControlReport;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.ExitControlReport;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusExitReport);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EuExitControlReport;

		protected override SecurityCheckpoint CheckPointForNew => null;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EuExitControlReport;

		protected override SecurityCheckpoint CheckPointForDelete => null;

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Does not support Delete functionality");
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Does not support New functionality");
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			IZForm result;
			var topLevelBusinessObject = GetTopLevelBusinessObject(sourceEntity);
			if (topLevelBusinessObject is null)
			{
				throw new ModuleGuiNotSupportedException("No support for CusExitReport not attached to Declaration or Shipment or CusExitHeader");
			}
			else
			{
				result = ExitControlControllerParentFormHelper.ShowLoadedFormSelectExitControlAndAddToRecent(this, topLevelBusinessObject, sourceEntity);
			}

			SelectExitReportsTabPage((ZForm)result);
			LastShownForm = result;
			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotImplementedException(); // All forms shown must be proxied via JobDeclarationController or ShipmentController or CusExitHeaderController
		}

		BusinessObject GetTopLevelBusinessObject(IBusiness businessEntity)
		{
			var header = GetExitHeader(businessEntity);
			if (header?.Parent is BusinessObject businessObject)
			{
				return businessObject;
			}
			else
			{
				return header;
			}
		}

		CusExitHeader GetExitHeader(IBusiness businessEntity) => businessEntity is CusExitHeader exitHeader ? exitHeader : businessEntity is CusExitReport exitReport ? exitReport.Header : null;

		void SelectExitReportsTabPage(ZForm shownForm)
		{
			var exitControlUserControl = shownForm.FindSingleOrDefault<ExitControlUserControl>();
			if (exitControlUserControl != null)
			{
				exitControlUserControl.Focus();
				exitControlUserControl.ExitControlTabControl.SelectTab(nameof(ExitControlUserControl.ReportsTabPage));
			}
		}
	}
}
