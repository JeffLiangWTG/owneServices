using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ErrorReporting.Business;
using Enterprise.ErrorReporting.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ErrorReporting.Module
{
	public class ErrorReportDetailsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ErrorDetailsForm((StmErrorReport)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ErrorReportDetails; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmErrorReport); }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.ErrorReporting;
			}
		}

		#region Hacks Be Here

		// Hack:
		// If you go to the Error Reports module and open an item, you get the View form.
		// If you then copy the URL, you get a URL for an Edit form. Editing an StmErrorReport
		// is not allowed/supported/etc.
		// This makes the URL show the View form instead of the Edit form.
		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return ShowViewForm(sourceEntity);
		}

		#endregion
	}
}
