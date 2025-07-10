using Enterprise.DocumentEngine.MacroValueProviders.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Test.ValueProviders.Macros
{
	[TestedType(typeof(StaffLoginNamesRelatedToAllTasks))]
	sealed class StaffLoginNamesRelatedToAllTasksTest : ValueProviderWithLoadControlFactoryTest<StaffLoginNamesRelatedToAllTasks>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<StaffLoginNamesRelatedToAllTasks()>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<StaffLoginNamesRelatedToAllTasks  ( )  >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<StaffLoginNamesRelatedToAllTasks  (  asdf )  >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<StaffLoginNamesRelatedToAllTasks  (  as.df )  >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<StaffLoginNamesRelatedToAllTasks(  pk,  abc)>", Passes.FirstPass));
		}

		public override void TestReplacement()
		{
			var workflowProvider = Factory.New<DummyWithWorkflow>();
			var docDataProvider = new DummyWrapper(workflowProvider, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<StaffLoginNamesRelatedToAllTasks ()>", Report));

			var staffXYZ = Factory.NewWithValidTestData<GlbStaff>();
			staffXYZ.GS_Code = "XYZ";
			staffXYZ.GS_LoginName = "Staff.XYZ";
			var task = workflowProvider.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staffXYZ.GS_Code;
			Factory.Save();

			AssertEquals("Staff.XYZ", ValueProviderToTest.GetReplacement("<StaffLoginNamesRelatedToAllTasks ()>", Report));

			var staffABC = Factory.NewWithValidTestData<GlbStaff>();
			staffABC.GS_Code = "ABC";
			staffABC.GS_LoginName = "Staff.ABC";

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resouceCapability = Factory.New<GlbResourceCapabilityPivot>();
			resouceCapability.G5_G4_Capability = capability.PK;
			task.P9_G4_RequiredCapability = capability.PK;
			resouceCapability.G5_GS_Resource = staffABC.PK;
			Factory.Save();
			AssertEquals("Staff.ABC, Staff.XYZ", ValueProviderToTest.GetReplacement("<StaffLoginNamesRelatedToAllTasks ()>", Report));

			var resouceCapability2 = Factory.New<GlbResourceCapabilityPivot>();
			resouceCapability2.G5_G4_Capability = capability.PK;
			task.P9_G4_RequiredCapability = capability.PK;
			resouceCapability2.G5_GS_Resource = staffXYZ.PK;
			Factory.Save();
			AssertEquals("Staff.ABC, Staff.XYZ", ValueProviderToTest.GetReplacement("<StaffLoginNamesRelatedToAllTasks ()>", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var workflowProvider = Factory.New<DummyWithWorkflow>();
			var docDataProvider = new DummyWrapper(workflowProvider, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);

			var staffHNY = Factory.NewWithValidTestData<GlbStaff>();
			staffHNY.GS_Code = "HNY";
			staffHNY.GS_LoginName = "Hunter.Yang";
			var task = workflowProvider.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staffHNY.GS_Code;
			Factory.Save();
		}

		public void TestReplacement_ErrorMessages()
		{
			// Business Object is not applicable for looking up related staff login names.
			object value = null;
			var docDataProvider = new DummyWrapper(null, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);

			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			AssertEquals("Value should be null", null, value);
			AssertNoExceptionThrown(() =>
			{
				value = ValueProviderToTest.GetReplacement("<StaffLoginNamesRelatedToAllTasks ()>", Report);
			});
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("Value should be null", null, value);
			Assert("report.ErrorManager.ToString()", Report.ErrorManager.ToString().Contains("Business Object is not applicable for looking up related staff login names."));
			Report.ErrorManager.ClearErrors();

			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			docDataProvider = new DummyWrapper(menuItem, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			AssertEquals("Value should be null", null, value);
			AssertNoExceptionThrown(() =>
			{
				value = ValueProviderToTest.GetReplacement("<StaffLoginNamesRelatedToAllTasks ()>", Report);
			});
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("Value should be null", null, value);
			Assert("report.ErrorManager.ToString()", Report.ErrorManager.ToString().Contains("Business Object is not applicable for looking up related staff login names."));
			Report.ErrorManager.ClearErrors();
		}
	}
}
