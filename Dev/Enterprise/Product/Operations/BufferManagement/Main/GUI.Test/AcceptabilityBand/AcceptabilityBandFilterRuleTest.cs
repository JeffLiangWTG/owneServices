using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
namespace Enterprise.BufferManagement.GUI.Test
{
	public class AcceptabilityBandFilterRuleTest : BMSTestCaseWithFactory
	{
		#region Aggregate

		public void TestAggregationAcceptabilityBand_BlankFilter()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "COL", "Colonel");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JHS", "John Hannibal Smith", capability);

			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var acceptabilityBand = CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Boop", type: AcceptabilityBandTypes.Codes.Aggregate);
			acceptabilityBand.BAB_SqlText =
@"SELECT COALESCE(Value, 0) Value, FC_PK Component, FH_GG_ReleaseGroup ReleaseGroup 
FROM dbo.BMComponent
LEFT JOIN (
SELECT COUNT(*) Value, FH_FC_CurrentComponent, FH_GG_ReleaseGroup
FROM dbo.ProcessHeader
GROUP BY FH_GG_ReleaseGroup, FH_FC_CurrentComponent
) headersCount ON FH_FC_CurrentComponent = FC_PK";

			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 0", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 0 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 10);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 10", BMConstants.CautionBoardColor,
				"Status: Caution", @"Caution: Boop: 10 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 12", BMConstants.GoodBoardColor,
				"Status: Good", @"Good: Boop: 12 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 14", BMConstants.ExcellentBoardColor,
				"Status: Excellent", @"Excellent: Boop: 14 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 4);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 18", BMConstants.GoodBoardColor,
				"Status: Good", @"Good: Boop: 18 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 20", BMConstants.CautionBoardColor,
				"Status: Caution", @"Caution: Boop: 20 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 22", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 22 (target is between 10 and 21)");
		}

		public void TestAggregationAcceptabilityBand()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "COL", "Colonel");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JHS", "John Hannibal Smith", capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!";
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2);
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "I love it when a plan comes together!";
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3);
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Boop", type: AcceptabilityBandTypes.Codes.Aggregate);
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			band.BAB_SqlText =
@"SELECT COALESCE(Value, 0) Value, FC_PK Component, FH_GG_ReleaseGroup ReleaseGroup 
FROM dbo.BMComponent
LEFT JOIN (
SELECT COUNT(*) Value, FH_FC_CurrentComponent, FH_GG_ReleaseGroup
FROM Workflows
GROUP BY FH_GG_ReleaseGroup, FH_FC_CurrentComponent
) headersCount ON FH_FC_CurrentComponent = FC_PK";

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 2", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 2 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 10);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 2", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 2 (target is between 10 and 21)");
		}

		#endregion

		#region Count

		public void TestAcceptabilityBandFilterRule_BlankFilter()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "INV", "Investor");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TBP", "T. Boone Pickens", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var acceptabilityBand = CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Boop", type: AcceptabilityBandTypes.Codes.Count);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 0", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 0 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, numberOfWorkflows: 10, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 10", BMConstants.CautionBoardColor,
				"Status: Caution", @"Caution: Boop: 10 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, numberOfWorkflows: 2, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 12", BMConstants.GoodBoardColor,
				"Status: Good", @"Good: Boop: 12 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, numberOfWorkflows: 2, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 14", BMConstants.ExcellentBoardColor,
				"Status: Excellent", @"Excellent: Boop: 14 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, numberOfWorkflows: 4, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 18", BMConstants.GoodBoardColor,
				"Status: Good", @"Good: Boop: 18 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, numberOfWorkflows: 2, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 20", BMConstants.CautionBoardColor,
				"Status: Caution", @"Caution: Boop: 20 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, numberOfWorkflows: 2, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 22", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 22 (target is between 10 and 21)");
		}

		public void TestAcceptabilityBandFilterRule_Filter()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "TSM", "Tea Sommelier");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MRT", "Mister Tea", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "ATM";
			releaseGroup.GG_Desc = "The A-Team";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!";
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2);
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "I pity the fool who does not drink tea!";
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3);
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Boop", type: AcceptabilityBandTypes.Codes.Count);
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 2", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 2 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 10);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Boop: 2", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Boop: 2 (target is between 10 and 21)");
		}

		#region Number As Percentage (NUP) filters

		public void TestCalculateStatus_NUP_WithBufferZoneAndCustomSQL_ShouldNotThrowDuplicateParameterException()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "LBF", "Louis Balfour");

			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_CompletionStatement = "niiiice";
			BMSTestHelper.CreateTask(workflow, staff.GS_Code);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, overrideChannels: true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 20, 33, 100, 100, 100, "Jazz Club", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);
			band.BAB_IsActive = true;
			band.BAB_FiltersByReleaseGroup = true;
			band.BAB_FiltersBySection = false;

			FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.BufferZone,
					FilterStripValueSetter = f =>
					{
						((ModuleNumberRangeFilter)f).Property1 = 0.0;
						((ModuleNumberRangeFilter)f).Property2 = 1.0;
					},
				},
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Custom SQL Filter",
					FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = @"FH_CompletionStatement = 'niiiice'",
				});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Jazz Club: 0", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Jazz Club: 0 (target is between 10 and 100)");
		}

		public void TestAcceptabilityBandFilterRule_NUPFilter_NoSuperset_NoSubset_ShouldReturnEverything()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "AWE", "Exhibiting naive childlike awe at everything");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BRL", "Brilliant Kid", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2);
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Tornadoes, they're brilliant!";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3);
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Whiskas", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);
			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Whiskas: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Whiskas: 100 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, 10);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Whiskas: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Whiskas: 100 (target is between 10 and 21)");
		}

		public void TestAcceptabilityBandFilterRule_NUPFilter_NoSuperset_WithSubset()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "WEA", "Reporting the weather");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "POU", "Poula Fisch", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2);
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Ooh, suit you sir!";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3);
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Scorchio", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Scorchio: 66.67", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Scorchio: 66.67 (target is between 10 and 21)");

			CreateWorkflows(config.Buffer, numberOfWorkflows: 10, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Scorchio: 15.38", BMConstants.ExcellentBoardColor,
				"Status: Excellent", @"Excellent: Scorchio: 15.38 (target is between 10 and 21)");
		}

		[TestDate(2016, 4, 20)]
		public void TestAcceptabilityBandFilterRule_NUPFilter_WithSuperset_WithSubset()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "FOO", "Football commentator");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RON", "Ron Manager", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			BMSTestHelper.CreateReleaseGroup(config.System, releaseGroup);
			var section = config.BufferSection;
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 20);
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 19);
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2);
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Surely this workflow will be ignored!";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 20);
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3);
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Jumpers for Goalposts", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);

			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = "Today",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Jumpers for Goalposts: 50", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Jumpers for Goalposts: 50 (target is between 0 and 0)");

			workflow1.FH_CompletionStatement = "Take the pressure down! 'Cause I can feel it, it's a-risin' like a storm...";
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Jumpers for Goalposts: 0", BMConstants.ExcellentBoardColor,
				"Status: Excellent", @"Excellent: Jumpers for Goalposts: 0 (target is between 0 and 0)");
		}

		[TestDate(2016, 4, 20)]
		public void TestAcceptabilityBandFilterRule_NUPFilter_WithSuperset_NoSubset()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "USA", "American");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "EDW", "Ed Winchester", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			BMSTestHelper.CreateReleaseGroup(config.System, releaseGroup);
			var section = config.BufferSection;
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 20);
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 19);
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2);
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "You ain't seen me, right?";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 20);
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3);
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Small Boys in the Park", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);

			FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = "Today",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Small Boys in the Park: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Small Boys in the Park: 100 (target is between 0 and 0)");

			workflow1.FH_CompletionStatement = "Hi! I'm Ed Winchester!";
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Small Boys in the Park: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Small Boys in the Park: 100 (target is between 0 and 0)");
		}

		[TestDate(2016, 4, 20)]
		public void TestAcceptabilityBandFilterRule_NUPFilter_WithSuperset_WithForceSeek()
		{
			using (var efSettings = TestEntityFrameworkSettings.Get())
			{
				efSettings.DefaultToForceSeek = true;
				var capability = BMSTestHelper.CreateCapability(Factory, "FOO", "Football commentator");
				var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RON", "Ron Manager", capability);

				var releaseGroup = Factory.New<GlbGroup>();
				releaseGroup.GG_Code = "FST";
				releaseGroup.GG_Desc = "Please release me, let me go!";

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
				BMSTestHelper.CreateReleaseGroup(config.System, releaseGroup);
				var section = config.BufferSection;
				section.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;

				var workflow1 = jobHeader.ProcessHeaders[0];
				workflow1.FH_CompletionStatement = "Look out!";
				workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
				workflow1.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 20);
				workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
				var task1 = BMSTestHelper.CreateTask(workflow1);
				task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
				var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0,
					"Test AB", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);

				FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
					new FilterStripsTestHelper.FilterStripDefinition
					{
						FilterStripName = ProcessHeader.ModuleFilterConstants.CurrentComponent,
						FilterStripValueSetter = f => ((CurrentComponentFilter)f).Property = config.Buffer.PK,
						ComparisonOperatorSetter = f => ((CurrentComponentFilter)f).ComparisonOperator = CurrentComponentFilter.ComparisonConstants.Exact,
					});

				BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

				Factory.Save();

				AssertAcceptabilityBandSubheadingControls(config, "Test AB: 100", BMConstants.HighRiskBoardColor,
					"Status: High Risk", @"High Risk: Test AB: 100 (target is between 0 and 0)");
			}
		}

		#endregion

		#region Planned Duration as Percentage (DUP) filters

		public void TestCalculateStatus_DUP_WithBufferZoneAndCustomSQL_ShouldNotThrowDuplicateParameterException()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "LBF", "Louis Balfour");

			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_CompletionStatement = "niiiice";
			BMSTestHelper.CreateTask(workflow, staff.GS_Code);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, overrideChannels: true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 20, 33, 100, 100, 100, "Jazz Club", type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);
			band.BAB_IsActive = true;
			band.BAB_FiltersByReleaseGroup = true;
			band.BAB_FiltersBySection = false;

			FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.BufferZone,
				FilterStripValueSetter = f =>
				{
					((ModuleNumberRangeFilter)f).Property1 = 0.0;
					((ModuleNumberRangeFilter)f).Property2 = 1.0;
				},
			},
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = @"FH_CompletionStatement = 'niiiice'",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Jazz Club: 0", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Jazz Club: 0 (target is between 10 and 100)");
		}

		public void TestAcceptabilityBandFilterRule_DUPFilter_NoSuperset_NoSubset_ShouldReturnEverything()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "ANC", "Anchorman");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PPP", "Poutremos Poutra-Poutremos", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Boutros!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_PlannedDurationInMinutes = 60;
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 60);

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Boutros!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_PlannedDurationInMinutes = 180;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 180);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Ghali!";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_PlannedDurationInMinutes = 60;
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3, staff.GS_Code, 60);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Ethethethetheth", type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);
			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertEquals(90, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(270, workflow2.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow3.FH_PlannedDurationInMinutes);

			AssertAcceptabilityBandSubheadingControls(config, "Ethethethetheth: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Ethethethetheth: 100 (target is between 0 and 0)");

			workflow3.FH_CompletionStatement = "Boutros!";
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Ethethethetheth: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Ethethethetheth: 100 (target is between 0 and 0)");
		}

		public void TestAcceptabilityBandFilterRule_DUPFilter_NoSuperset_WithSubset()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "ANC", "Anchorman");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PPP", "Poutremos Poutra-Poutremos", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;
			config.Buffer.FC_BufferTimespanInMinutes = 60;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Boutros!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_PlannedDurationInMinutes = 60;
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 60);

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Boutros!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_PlannedDurationInMinutes = 180;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 180);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Ghali!";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_PlannedDurationInMinutes = 60;
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3, staff.GS_Code, 60);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(
				config.Buffer,
				cautionMin: 5,
				goodMin: 10,
				excellentMin: 13,
				excellentMax: 20,
				goodMax: 25,
				cautionMax: 30,
				name: "Bono Estente",
				type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage
			);

			var def1 =
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.PlannedDuration,
					FilterStripValueSetter = f => {
						var filter = (ModuleDurationFilter)f;
						filter.MinDurationMinutes = 60;
						filter.MaxDurationMinutes = 120;
						filter.Scope = "Between";
						filter.IsActive = true;
					},
				};
			var def2 =
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
					FilterStripValueSetter = f => { ((ModuleFlagsFilter)f).Property1 = true; }, // workflow
				};
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule, def1, def2);

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertEquals(90, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(270, workflow2.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow3.FH_PlannedDurationInMinutes);

			// Calculation: ((workflow1 + workflow3).FH_PlannedDurationInMinutes / ((workflow1 + workflow2 + workflow3).FH_PlannedDurationInMinutes)) * 100 == 40
			AssertAcceptabilityBandSubheadingControls(config, "Bono Estente: 40", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Bono Estente: 40 (target is between 5 and 30)");

			workflow1.FH_PlannedDurationInMinutes = 270;
			task1.P9_EstDuration = new CargoWise.Types.ZInt(270).GetDateTimeFromMinutes();
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Bono Estente: 14.29", BMConstants.ExcellentBoardColor,
				"Status: Excellent", @"Excellent: Bono Estente: 14.29 (target is between 5 and 30)");
		}

		public void TestAcceptabilityBandFilterRule_DUPFilter_WithSuperset_WithSubset()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "FOP", "Football player");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CHW", "Chris Waddle", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;
			config.Buffer.FC_BufferTimespanInMinutes = 60;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Boutros!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_PlannedDurationInMinutes = 60;
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 60);

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Boutros!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_PlannedDurationInMinutes = 180;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 180);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Ghali!";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_PlannedDurationInMinutes = 60;
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3, staff.GS_Code, 60);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			var band = BMSTestHelper.CreateAcceptabilityBand(
				config.Buffer,
				cautionMin: 5,
				goodMin: 10,
				excellentMin: 15,
				excellentMax: 20,
				goodMax: 24,
				cautionMax: 30,
				name: "Chris Waddle",
				type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage
			);

			var def1 =
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.PlannedDuration,
					FilterStripValueSetter = f => {
						var filter = (ModuleDurationFilter)f;
						filter.MinDurationMinutes = 60;
						filter.MaxDurationMinutes = 120;	
						filter.Scope = "Between";
						filter.IsActive = true;
					},
				};
			var def2 =
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
					FilterStripValueSetter = f => { ((ModuleFlagsFilter)f).Property1 = true; }, // workflow
				};
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule, def1, def2);

			FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Boutros!",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertEquals(90, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(270, workflow2.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow3.FH_PlannedDurationInMinutes);

			// Calculation: (workflow1.FH_PlannedDurationInMinutes / ((workflow1 + workflow2).FH_PlannedDurationInMinutes)) * 100 == 25
			AssertAcceptabilityBandSubheadingControls(config, "Chris Waddle: 25", BMConstants.CautionBoardColor,
				"Status: Caution", @"Caution: Chris Waddle: 25 (target is between 5 and 30)");

			workflow2.FH_PlannedDurationInMinutes = 60;
			task2.P9_EstDuration = new CargoWise.Types.ZInt(60).GetDateTimeFromMinutes();
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Chris Waddle: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Chris Waddle: 100 (target is between 5 and 30)");
		}

		public void TestAcceptabilityBandFilterRule_DUPFilter_WithSuperset_NoSubset()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "TAI", "Tailor");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "KEN", "Ken Kenneth", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var section = config.BufferSection;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Boutros!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_PlannedDurationInMinutes = 60;
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 60);

			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Boutros!";
			workflow2.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow2.FH_PlannedDurationInMinutes = 180;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 180);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Ghali!";
			workflow3.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow3.FH_PlannedDurationInMinutes = 60;
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task3 = BMSTestHelper.CreateTask(workflow3, staff.GS_Code, 60);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);

			var band = BMSTestHelper.CreateAcceptabilityBand(
				config.Buffer,
				cautionMin: 1,
				goodMin: 2,
				excellentMin: 3,
				excellentMax: 4,
				goodMax: 5,
				cautionMax: 6,
				"Cielyn Gizmo",
				type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage
			);

			FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Boutros!",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			AssertEquals(90, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(270, workflow2.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow3.FH_PlannedDurationInMinutes);

			AssertAcceptabilityBandSubheadingControls(config, "Cielyn Gizmo: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Cielyn Gizmo: 100 (target is between 1 and 6)");

			workflow2.FH_PlannedDurationInMinutes = 60;
			task2.P9_EstDuration = new CargoWise.Types.ZInt(60).GetDateTimeFromMinutes();
			Factory.Save();

			AssertAcceptabilityBandSubheadingControls(config, "Cielyn Gizmo: 100", BMConstants.HighRiskBoardColor,
				"Status: High Risk", @"High Risk: Cielyn Gizmo: 100 (target is between 1 and 6)");
		}

		#endregion

		#endregion

		#region Assertions
		public static void AssertAcceptabilityBandSubheadingControls(AcceptabilityBandTestConfig config, string nameAndResultLabel, Color backColour, string subHeadingText, string subHeadingMouseOverText)
		{
			using (BMSGUITestCase.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
					AssertEquals(1, tiles.Length);
					AssertEquals(nameAndResultLabel, tiles[0].NameAndResultLabel.Text);
					AssertEquals(backColour, tiles[0].BackColor);

					var components = form.FindAll<BMComponentControl>().ToArray();
					AssertEquals(1, components.Length);
					AssertEquals(subHeadingText, components[0].ViewModel.SubHeadingAppearance.SectionSubHeading);
					AssertEquals(subHeadingMouseOverText, components[0].ViewModel.SubHeadingAppearance.SectionSubHeadingDetailText);
				});
			}
		}

		#endregion
	}
}

