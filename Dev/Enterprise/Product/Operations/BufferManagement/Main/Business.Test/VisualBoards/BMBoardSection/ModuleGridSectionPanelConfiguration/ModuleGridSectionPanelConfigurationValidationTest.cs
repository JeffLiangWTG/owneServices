using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Test
{
	class ModuleGridSectionPanelConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPanelNameOverrideShouldBeDefinedWhenOverridden()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var config = (ModuleGridSectionConfiguration)modSection.Configuration;
			var panelConfig = config.PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			panelConfig.SectionNameIsOverridden = true;
			panelConfig.SectionNameOverride = ZString.Empty;
			AssertHasError(panelConfig.SectionNameOverrideInfo, "Custom panel name cannot be empty when overriding panel name.");

			panelConfig.SectionNameOverride = "   ";
			AssertHasError(panelConfig.SectionNameOverrideInfo, "Custom panel name cannot be empty when overriding panel name.");

			panelConfig.SectionNameOverride = "I'll override you!";
			AssertNoNotifications(config.SectionNameOverrideInfo);

			panelConfig.SectionNameOverride = ZString.Empty;
			AssertHasError(panelConfig.SectionNameOverrideInfo, "Custom panel name cannot be empty when overriding panel name.");
		}

		public void TestShouldDisplayErrorWhenSequenceIsOutsideRange()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			board.MB_IsPublished = false;
			board.MB_GG_ReleaseGroup = ZGuid.Empty;
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var panelConfig = (modSection.Configuration as ModuleGridSectionConfiguration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			panelConfig.Sequence = 0;
			AssertHasError(panelConfig.SequenceInfo, "Please enter a 'Sequence' within the range 1 to 100.");

			panelConfig.Sequence = 101;
			AssertHasError(panelConfig.SequenceInfo, "Please enter a 'Sequence' within the range 1 to 100.");

			panelConfig.Sequence = 10;
			AssertNoNotifications(panelConfig.SequenceInfo);
		}

		public void TestShouldDisplayErrorWhenSequenceIsDuplicate()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			board.MB_IsPublished = false;
			board.MB_GG_ReleaseGroup = ZGuid.Empty;
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);

			var panelConfig1 = (modSection.Configuration as ModuleGridSectionConfiguration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;
			var panelConfig2 = new ModuleGridSectionPanelConfiguration(modSection);
			(modSection.Configuration as ModuleGridSectionConfiguration).PanelConfigurations.Add(panelConfig2);

			panelConfig1.Sequence = 3;
			panelConfig2.Sequence = 3;
			AssertNoNotifications(panelConfig1.SequenceInfo);
			AssertHasError(panelConfig2.SequenceInfo, "The Sequence has been duplicated and must be unique.");

			panelConfig2.Sequence = 1;
			AssertNoNotifications(panelConfig1.SequenceInfo);
			AssertNoNotifications(panelConfig2.SequenceInfo);
		}

		public void TestShouldDisplayWarningOrErrorWhenFilterLayoutIsNotPublished()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			board.MB_IsPublished = false;
			board.MB_GG_ReleaseGroup = ZGuid.Empty;
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var panelConfig = (modSection.Configuration as ModuleGridSectionConfiguration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			var filterPublished = Factory.New<StmModuleFilter>();
			filterPublished.S9_ModuleID = ModuleIDs.WorkItem.Name;
			filterPublished.S9_FilterName = "Published filter";
			filterPublished.S9_IsPublished = true;
			filterPublished.S9_GC = ZGuid.Empty;

			panelConfig.FilterLayout = filterPublished.PK;
			AssertNoNotifications("There should not be any warnings or errors when the layout is published", panelConfig.FilterLayoutInfo);

			var filterNonPublished = Factory.New<StmModuleFilter>();
			filterNonPublished.S9_ModuleID = ModuleIDs.WorkItem.Name;
			filterNonPublished.S9_FilterName = "Non published filter";
			filterNonPublished.S9_IsPublished = false;

			panelConfig.FilterLayout = filterNonPublished.PK;
			AssertHasWarning("There should be a warning when the board is not visible for other users, and the layout is not published", panelConfig.FilterLayoutInfo, "This layout is not published, and will not be applied when other users open this visual board.");

			board.MB_IsPublished = true;
			panelConfig.FilterLayout = filterNonPublished.PK;
			AssertHasError("There should be an error when the board is published, but the layout is not published", panelConfig.FilterLayoutInfo, "This layout is not published, and therefore cannot be applied to a published visual board.");

			board.MB_IsPublished = false;
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);
			board.MB_GG_ReleaseGroup = releaseGroup.PK;
			panelConfig.FilterLayout = filterNonPublished.PK;
			AssertHasError("There should be an error when the board is associated with a release group, but the layout is not published", panelConfig.FilterLayoutInfo, "This layout is not published, and therefore cannot be applied to a visual board associated with a release group.");

			panelConfig.FilterLayout = ZGuid.Invalid;
			AssertHasError("There should be an error when an invalid guid is passed", panelConfig.FilterLayoutInfo, "The layout cannot be loaded.");
		}

		public void TestShouldNotDisplayWarningOrError_WhenFilterLayoutIsPublishedToAllCompanies()
		{
			var filterPublishedToAllCompanies = Factory.New<StmModuleFilter>();
			filterPublishedToAllCompanies.S9_ModuleID = ModuleIDs.WorkItem.Name;
			filterPublishedToAllCompanies.S9_FilterName = "Published to all companies filter";
			filterPublishedToAllCompanies.S9_IsPublished = true;
			filterPublishedToAllCompanies.S9_GC = ZGuid.Empty;

			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var panelConfig = (modSection.Configuration as ModuleGridSectionConfiguration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			board.IsGlobal = false;
			panelConfig.FilterLayout = filterPublishedToAllCompanies.PK;
			AssertNoNotifications("There should not be any warnings or errors when the layout is published to all companies, and the board is not global.", panelConfig.FilterLayoutInfo);

			board.IsGlobal = true;
			panelConfig.FilterLayout = filterPublishedToAllCompanies.PK;
			AssertNoNotifications("There should not be any warnings or errors when the layout is published to all companies, and the board is global.", panelConfig.FilterLayoutInfo);
		}

		public void TestShouldDisplayError_WhenFilterLayoutIsPublishedToOneCompany()
		{
			var filterPublishedToOneCompany = Factory.New<StmModuleFilter>();
			filterPublishedToOneCompany.S9_ModuleID = ModuleIDs.WorkItem.Name;
			filterPublishedToOneCompany.S9_FilterName = "Published to one company filter";
			filterPublishedToOneCompany.S9_IsPublished = true;

			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var panelConfig = (modSection.Configuration as ModuleGridSectionConfiguration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			filterPublishedToOneCompany.S9_GC = GlbCompany.CurrentCompany.PK;
			board.IsGlobal = false;
			panelConfig.FilterLayout = filterPublishedToOneCompany.PK;
			AssertEquals(board.MB_GC_Company, filterPublishedToOneCompany.S9_GC);
			AssertNoNotifications("There should not be any warnings or errors when the layout is published to one company, and the board is only visible to the same company.", panelConfig.FilterLayoutInfo);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			filterPublishedToOneCompany.S9_GC = company.PK;
			board.IsGlobal = false;
			panelConfig.FilterLayout = filterPublishedToOneCompany.PK;
			AssertNotEquals(board.MB_GC_Company, filterPublishedToOneCompany.S9_GC);
			AssertHasError(
				"There should be an error when the layout is published to one company, but the board is only visible to another company.",
				panelConfig.FilterLayoutInfo,
				$"This layout is not published for company {GlbCompany.CurrentCompany.CompanyName}, and therefore cannot be applied to a company specific visual board.");

			filterPublishedToOneCompany.S9_GC = GlbCompany.CurrentCompany.PK;
			board.IsGlobal = true;
			panelConfig.FilterLayout = filterPublishedToOneCompany.PK;
			AssertHasError(
				"There should be an error when the layout is published to one company, but the board is global.",
				panelConfig.FilterLayoutInfo,
				"This layout is not published for all companies, and therefore cannot be applied to a global visual board.");
		}

		public void TestShouldDisplayWarningOrError_WhenFilterLayoutIsNotPublished()
		{
			var filterNonPublished = Factory.New<StmModuleFilter>();
			filterNonPublished.S9_ModuleID = ModuleIDs.WorkItem.Name;
			filterNonPublished.S9_FilterName = "Non published filter";
			filterNonPublished.S9_IsPublished = false;

			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			board.MB_IsPublished = false;
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var panelConfig = (modSection.Configuration as ModuleGridSectionConfiguration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration;

			filterNonPublished.S9_GC = GlbCompany.CurrentCompany.PK;
			board.IsGlobal = false;
			panelConfig.FilterLayout = filterNonPublished.PK;
			AssertEquals(board.MB_GC_Company, filterNonPublished.S9_GC);
			AssertHasWarning(
				"There should be a warning when the board is not visible for other users, and the layout is not published.",
				panelConfig.FilterLayoutInfo,
				"This layout is not published, and will not be applied when other users open this visual board.");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			filterNonPublished.S9_GC = company.PK;
			board.IsGlobal = false;
			panelConfig.FilterLayout = filterNonPublished.PK;
			AssertNotEquals(board.MB_GC_Company, filterNonPublished.S9_GC);
			AssertHasError(
				"There should be an error when the layout is not published, and can be used in one company context, but the board is only visible to another company.",
				panelConfig.FilterLayoutInfo,
				$"This layout is not published for company {GlbCompany.CurrentCompany.CompanyName}, and therefore cannot be applied to a company specific visual board.");

			filterNonPublished.S9_GC = GlbCompany.CurrentCompany.PK;
			board.IsGlobal = true;
			panelConfig.FilterLayout = filterNonPublished.PK;
			AssertHasError(
				"There should be an error when the layout is not published, but the board is global.",
				panelConfig.FilterLayoutInfo,
				"This layout is not published for all companies, and therefore cannot be applied to a global visual board.");
		}
	}
}
