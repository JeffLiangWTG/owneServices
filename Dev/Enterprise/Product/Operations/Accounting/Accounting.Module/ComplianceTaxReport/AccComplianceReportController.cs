using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccComplianceReportController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccComplianceReportController()
			: base()
		{ }

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccComplianceReportForm(businessEntity as AccComplianceReport);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccComplianceReport; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccComplianceReport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccComplianceReport); }
		}

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DeleteComplianceReport; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EditComplianceReport; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewComplianceReport; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ComplianceReports; }
		}

		public IZForm ShowNewForm(ZString type)
		{
			var complianceReport = (AccComplianceReport)GetNewBusinessEntityInLocalFactory();
			complianceReport.ACR_ReportType = type;
			return ShowFormForNewEntity(complianceReport);
		}

		#endregion
	}
}

