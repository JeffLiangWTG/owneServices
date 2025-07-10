using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class BMBoardSectionViewModelTest : BMSTestCaseWithFactory
	{
		#region Acceptability Bands

		public void TestSectionSubHeading_ShouldNotQuerySecondaryServerExcessively()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			section.SectionConfiguration.CellsPerSubsection = 0; // This section just displays acceptability bands

			BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule, AcceptabilityBandShowOnOption.Both);
			BMSTestHelper.AddAcceptabilityBandToSection(section, config.RedRule, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var results = viewModel.GetAcceptabilityBandResults(Factory);
			viewModel.RefreshAcceptabilityBandSubHeading(results);

			var dataProvider = (DummySecondaryServerConnectionProvider)SecondaryServerConnectionProviderProvider.GetProvider();
			AssertEquals(1, dataProvider.GetNewConnectionWrapperExecutedCount);
		}

		#endregion

		#region Filters

		#region TaskFilter and WorkflowFilter

		public void TestSetupProperties_WithFilterWithCountrySpecificModule_WhenModuleNotAvailableInCurrentContext_ShouldNotThrowExceptions_WorkflowFilters()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "DUM");
			var section = config.BufferSection;
			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(section.WorkflowFilter, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNoExceptionThrown("Creating a view model when this filter exists should not throw exceptions. SAD!", () => BMSTestHelper.CreateViewModel(section));
			}
		}

		public void TestSetupProperties_WithFilterWithCountrySpecificModule_WhenModuleNotAvailableInCurrentContext_ShouldNotThrowExceptions_TaskFilters()
		{
			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "DUM");
			var section = config.BufferSection;
			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(section.TaskFilter, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNoExceptionThrown("Creating a view model when this filter exists should not throw exceptions. SAD!", () => BMSTestHelper.CreateViewModel(section));
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.DisableAcceptabilityBandResultCache();
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}

	class BMBoardSectionViewModelNonTransactionedTest : NonTransactionedTestCase
	{
		public void TestCalculateWithInvalidAcceptabilityBandSQL()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "");
			band.BAB_Name = "Defect Backlog Improvement ESRVRG (30 days)";
			band.BAB_SqlText = @"
				SELECT convert(decimal(10,3), 'Nope')  as Value,
				null as Component,
				null as ReleaseGroup";

			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false).ProcessHeaders.AddNew();
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "dearmom@family.com";

			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			Factory.Save();

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());

			band.BAB_Type = AcceptabilityBandTypes.Codes.Aggregate;
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);
			sectionViewModel.GetAcceptabilityBandResults(Factory);

			AssertEquals("1 message sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Recipients Count", 1, createdEmail.Recipients.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Recipient", "dearmom@family.com", createdEmail.Recipients[0].Email);
				AssertEquals("Subject", "Acceptability Band SQL Error", createdEmail.Subject);
				AssertContains("Body", "Acceptability Band SQL is invalid. The status calculation produced an error.", createdEmail.Body);
				AssertContains("Body", "The acceptability band: ", createdEmail.Body);
				AssertContains("Body", "Release Group PK:", createdEmail.Body);
				AssertContains("Body", "SQL:", createdEmail.Body);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.DisableAcceptabilityBandResultCache();
		}
	}
}

