using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccComplianceReportController))]
	public class AccComplianceReportControllerTest : ZSingletonControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.AccComplianceReport, Controller.ModuleID);
		}

		public void TestCheckpoints()
		{
			var bizO = (AccComplianceReport)GetBusinessObjectThatIsInTheDatabase();
			AssertEquals("View", Env.Security.ComplianceReports, Controller.GetCheckPointForView(bizO));
			AssertEquals("New", Env.Security.NewComplianceReport, Controller.GetCheckPointForNew(bizO));
			AssertEquals("Edit", Env.Security.EditComplianceReport, Controller.GetCheckPointForEdit(bizO));
			AssertEquals("Delete", Env.Security.DeleteComplianceReport, Controller.GetCheckPointForDelete(bizO));
		}

		public void TestShowNewForm()
		{
			var reportController = (AccComplianceReportController)Controller;
			var complianceReport = (AccComplianceReport)reportController.ShowNewForm("PTR").BusinessEntityForPersistingForm;

			AssertEquals("PTR", complianceReport.ACR_ReportType);
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccComplianceReport;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.New<AccComplianceReport>();
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(AccComplianceReport);
		}

		#endregion
	}
}

