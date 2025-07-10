using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using ProcessJobHeaderProvider = Enterprise.MasterFiles.Business.ProcessJobHeaderProvider;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardMenuItemProviderTest : BMSTestCaseWithFactory
	{
		public void TestGetMenuItems_WhenNoBufferManagementSystemForWorkflowType_ShouldReturnNoMenuItems()
		{
			EnableBMSInRegistry();

			const string workflowType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;

			using (var module = GetNewModule(ModuleIDs.SalesEnquiry))
			{
				var provider = new VisualBoardMenuItemProvider();

				AssertEquals("Pre-condition: PAVE is not enabled for Sales Enquiry module", false, ProcessJobHeaderProvider.SupportsPAVE(workflowType, Factory));
				AssertContainsExactElementsInAnyOrder("Should be no menu items returned by the provider since the module doesn't support PAVE", Array.Empty<string>(), provider.GetMenuItems(module).Select(i => i.Text));

				BMSTestHelper.CreateSystem(Factory, workflowType);
				Factory.Save();

				AssertContainsExactElementsInAnyOrder("Now that PAVE has been enabled for Sales Enquiry module, a Visual Boards menu item should be returned", new[] { "Visual Boards" }, provider.GetMenuItems(module).Select(i => i.Text));
			}
		}

		public void TestGetVisualBoardMenuItem_ShouldArrangeByReleaseGroupType_ThenByReleaseGroupAlphabetically_ThenByBoardNameAlphabetically()
		{
			var system2 = BMSTestHelper.CreateSystem(Factory, "INQ");
			system2.FS_Name = "System 2";
			Factory.Save(); // so that they're in the database in non-alpha order

			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system1.FS_Name = "System 1";

			var group2 = BMSTestHelper.CreateGroup(Factory, "GR2", "Group 2");
			var group3 = BMSTestHelper.CreateGroup(Factory, "GR3", "Group 3");
			var group4 = BMSTestHelper.CreateGroup(Factory, "GR4", "Group 4");
			Factory.Save(); // so that they're in the database in non-alpha order

			var group1 = BMSTestHelper.CreateGroup(Factory, "GR1", "Group 1");

			var me = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "STA", "Miss Rona");
			me.Groups.Add(group1);
			me.Groups.Add(group2);

			var otherStaff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "OTH", "Useless");

			// Boards in my release group

			var myReleaseGroupBoard2 = BMSTestHelper.CreateBoard(system2, "Board for my release group 2");
			myReleaseGroupBoard2.MB_GG_ReleaseGroup = group1.PK;
			myReleaseGroupBoard2.MB_IsPublished = true;
			Factory.Save(); // so that they're in the database in non-alpha order

			var myReleaseGroupBoard1 = BMSTestHelper.CreateBoard(system1, "Board for my release group 1");
			myReleaseGroupBoard1.MB_GG_ReleaseGroup = group1.PK;
			myReleaseGroupBoard1.MB_IsPublished = true;

			var myReleaseGroupBoard3 = BMSTestHelper.CreateBoard(system2, "Board for my release group 3");
			myReleaseGroupBoard3.MB_GG_ReleaseGroup = group2.PK;
			myReleaseGroupBoard3.MB_IsPublished = true;

			var myReleaseGroupBoard4 = BMSTestHelper.CreateBoard(system1, "Board for my release group 4");
			myReleaseGroupBoard4.MB_GG_ReleaseGroup = group2.PK;
			myReleaseGroupBoard4.MB_IsPublished = true;

			var myReleaseGroupBoardNonPublishedMine = BMSTestHelper.CreateBoard(system1, "My non-published board for my release group");
			myReleaseGroupBoardNonPublishedMine.MB_GG_ReleaseGroup = group1.PK;
			myReleaseGroupBoardNonPublishedMine.MB_IsPublished = false;
			myReleaseGroupBoardNonPublishedMine.MB_GS_NKStaffCode = me.GS_Code;

			var myReleaseGroupBoardNonPublishedOtherStaff = BMSTestHelper.CreateBoard(system1, "Someone else's non-published board for my release group");
			myReleaseGroupBoardNonPublishedOtherStaff.MB_GG_ReleaseGroup = group1.PK;
			myReleaseGroupBoardNonPublishedOtherStaff.MB_IsPublished = false;
			myReleaseGroupBoardNonPublishedOtherStaff.MB_GS_NKStaffCode = otherStaff.GS_Code;

			// Boards in other release groups

			var otherReleaseGroupBoard2 = BMSTestHelper.CreateBoard(system2, "Board for other release group 2");
			otherReleaseGroupBoard2.MB_GG_ReleaseGroup = group3.PK;
			otherReleaseGroupBoard2.MB_IsPublished = true;
			Factory.Save(); // so that they're in the database in non-alpha order

			var otherReleaseGroupBoard1 = BMSTestHelper.CreateBoard(system1, "Board for other release group 1");
			otherReleaseGroupBoard1.MB_GG_ReleaseGroup = group3.PK;
			otherReleaseGroupBoard1.MB_IsPublished = true;

			var otherReleaseGroupBoard3 = BMSTestHelper.CreateBoard(system1, "Board for other release group 3");
			otherReleaseGroupBoard3.MB_GG_ReleaseGroup = group4.PK;
			otherReleaseGroupBoard3.MB_IsPublished = true;

			var otherReleaseGroupBoard4 = BMSTestHelper.CreateBoard(system1, "Board for other release group 4");
			otherReleaseGroupBoard4.MB_GG_ReleaseGroup = group4.PK;
			otherReleaseGroupBoard4.MB_IsPublished = true;

			var otherReleaseGroupBoardNonPublishedMine = BMSTestHelper.CreateBoard(system1, "My non-published board for other release group");
			otherReleaseGroupBoardNonPublishedMine.MB_GG_ReleaseGroup = group3.PK;
			otherReleaseGroupBoardNonPublishedMine.MB_IsPublished = false;
			otherReleaseGroupBoardNonPublishedMine.MB_GS_NKStaffCode = me.GS_Code;

			var otherReleaseGroupBoardNonPublishedOtherStaff = BMSTestHelper.CreateBoard(system1, "Someone else's non-published board for other release group");
			otherReleaseGroupBoardNonPublishedOtherStaff.MB_GG_ReleaseGroup = group3.PK;
			otherReleaseGroupBoardNonPublishedOtherStaff.MB_IsPublished = false;
			otherReleaseGroupBoardNonPublishedOtherStaff.MB_GS_NKStaffCode = otherStaff.GS_Code;

			// Boards not associated with a release group

			var noReleaseGroupBoard2 = BMSTestHelper.CreateBoard(system2, "Board not associated with a release group 2");
			noReleaseGroupBoard2.MB_IsPublished = true;
			Factory.Save(); // so that they're in the database in non-alpha order

			var noReleaseGroupBoard1 = BMSTestHelper.CreateBoard(system2, "Board not associated with a release group 1");
			noReleaseGroupBoard2.MB_IsPublished = true;

			var noReleaseGroupBoardNonPublishedMine = BMSTestHelper.CreateBoard(system1, "My non-published board with no release group");
			noReleaseGroupBoardNonPublishedMine.MB_IsPublished = false;
			noReleaseGroupBoardNonPublishedMine.MB_GS_NKStaffCode = me.GS_Code;

			var noReleaseGroupBoardNonPublishedOtherStaff = BMSTestHelper.CreateBoard(system1, "Someone else's non-published board with no release group");
			noReleaseGroupBoardNonPublishedOtherStaff.MB_IsPublished = false;
			noReleaseGroupBoardNonPublishedOtherStaff.MB_GS_NKStaffCode = otherStaff.GS_Code;

			Factory.Save();

			using (Env.SetTemporaryUserContext(me.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var module = GetNewModule(ModuleIDs.ProcessHeader))
			{
				var provider = new VisualBoardMenuItemProvider();

				AssertEquals(0, provider.GetMenuItems(module).Count());

				Env.Security.VisualBoards.IsAllowed = true;
				var menuItem = provider.GetMenuItems(module).ElementAt(0);
				menuItem.OnPopup_Exposed();

				BMSFormTestHelper.AssertMenuItems(menuItem, VisualBoardMenuItemProvider.SearchItemText, "-", "Boards for my Release Groups", "Boards for other Release Groups", "Boards not associated with a Release Group");

				// Boards for my Release Groups

				var myReleaseGroupsMenu = BMSFormTestHelper.GetChildMenuItem(menuItem, "Boards for my Release Groups");
				BMSFormTestHelper.AssertMenuItems(myReleaseGroupsMenu, "Group 1", "Group 2");

				var subMenu = BMSFormTestHelper.GetChildMenuItem(myReleaseGroupsMenu, "Group 1");
				BMSFormTestHelper.AssertMenuItems(subMenu, "Board for my release group 1", "Board for my release group 2", "My non-published board for my release group", "Someone else's non-published board for my release group");

				subMenu = BMSFormTestHelper.GetChildMenuItem(myReleaseGroupsMenu, "Group 2");
				BMSFormTestHelper.AssertMenuItems(subMenu, "Board for my release group 3", "Board for my release group 4");

				// Boards for other Release Groups

				var otherReleaseGroupsMenu = BMSFormTestHelper.GetChildMenuItem(menuItem, "Boards for other Release Groups");
				BMSFormTestHelper.AssertMenuItems(otherReleaseGroupsMenu, "Group 3", "Group 4");

				subMenu = BMSFormTestHelper.GetChildMenuItem(otherReleaseGroupsMenu, "Group 3");
				BMSFormTestHelper.AssertMenuItems(subMenu, "Board for other release group 1", "Board for other release group 2", "My non-published board for other release group");

				subMenu = BMSFormTestHelper.GetChildMenuItem(otherReleaseGroupsMenu, "Group 4");
				BMSFormTestHelper.AssertMenuItems(subMenu, "Board for other release group 3", "Board for other release group 4");

				// Boards not associated with a Release Group

				var nonReleaseGroupsMenu = BMSFormTestHelper.GetChildMenuItem(menuItem, "Boards not associated with a Release Group");
				BMSFormTestHelper.AssertMenuItems(nonReleaseGroupsMenu, "My non-published board with no release group (private)", "Board not associated with a release group 1", "Board not associated with a release group 2");
			}
		}

		public void TestGetVisualBoardMenuItem_ShouldIncludeSlideshowMenuItems()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system1.FS_Name = "Mai System";

			var board1_1 = system1.Boards.AddNew();
			board1_1.MB_Name = "Board 1";
			var board1_2 = system1.Boards.AddNew();
			board1_2.MB_Name = "Board 2";

			var system2 = Factory.NewWithValidTestData<BMSystem>();
			system2.FS_Name = "Yaw System";
			var board2_1 = system2.Boards.AddNew();
			board2_1.MB_Name = "Board 2 System 2";

			var slideShow1 = Factory.New<BMBoardSlideshow>();
			slideShow1.MD_Name = "Test slideshow 1";
			slideShow1.BoardPivots.AddNew().MC_MB_Board = board1_1.PK;

			var slideShow2 = Factory.New<BMBoardSlideshow>();
			slideShow2.MD_Name = "Test slideshow 2";
			slideShow2.BoardPivots.AddNew().MC_MB_Board = board2_1.PK;

			var slideShow3 = Factory.New<BMBoardSlideshow>();
			slideShow3.MD_Name = "Test slideshow 3";
			slideShow3.BoardPivots.AddNew().MC_MB_Board = board1_2.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var module = GetNewModule(ModuleIDs.Organisation))
			{
				Env.Security.VisualBoards.IsAllowed = true;

				var provider = new VisualBoardMenuItemProvider();
				var menuItems = provider.GetMenuItems(module).ToArray();
				AssertEquals(1, menuItems.Length);

				var rootMenuItem = menuItems.Single();
				rootMenuItem.OnPopup_Exposed();

				BMSFormTestHelper.AssertMenuItems(rootMenuItem, VisualBoardMenuItemProvider.SearchItemText, "-", "Boards not associated with a Release Group", "-", "Slide Shows");

				var boardMenu = BMSFormTestHelper.GetChildMenuItem(rootMenuItem, "Boards not associated with a Release Group");
				BMSFormTestHelper.AssertMenuItems(boardMenu, "Board 1", "Board 2", "Board 2 System 2");

				var slideShowMenu = BMSFormTestHelper.GetChildMenuItem(rootMenuItem, "Slide Shows");
				BMSFormTestHelper.AssertMenuItems(slideShowMenu, "Test slideshow 1", "Test slideshow 2", "Test slideshow 3");
			}
		}

		public void TestGetVisualBoardMenuItem_WhenNoBoardsDefined()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system1.FS_Name = "Mai System";

			Factory.Save();

			using (var module = GetNewModule(ModuleIDs.Organisation))
			{
				var provider = new VisualBoardMenuItemProvider();

				var menuItem = provider.GetMenuItems(module).Single();
				menuItem.OnPopup_Exposed();
				BMSFormTestHelper.AssertMenuItems(menuItem); // should be empty.
			}

			var board = system1.Boards.AddNew();
			board.MB_Name = "Board 1";
			Factory.Save();

			using (var module = GetNewModule(ModuleIDs.Organisation))
			{
				var provider = new VisualBoardMenuItemProvider();

				var menuItem = provider.GetMenuItems(module).Single();
				menuItem.OnPopup_Exposed();
				BMSFormTestHelper.AssertMenuItems(menuItem, VisualBoardMenuItemProvider.SearchItemText, "-", "Boards not associated with a Release Group");
			}
		}

		public void TestGetMenuItemForJobWorkflowsModule_ShouldReturnSameMenuAsAnyBufferManagementEnabledModule()
		{
			TestGetMenuItemForModule_ShouldReturnSameMenuAsAnyBufferManagementEnabledModule(ModuleIDs.ProcessHeader);
		}

		public void TestGetMenuItemForProcessTasksModule_ShouldReturnSameMenuAsAnyBufferManagementEnabledModule()
		{
			TestGetMenuItemForModule_ShouldReturnSameMenuAsAnyBufferManagementEnabledModule(ModuleIDs.ProcessTasks);
		}

		void TestGetMenuItemForModule_ShouldReturnSameMenuAsAnyBufferManagementEnabledModule(ModuleIdentifier moduleId)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var board1 = system.Boards.AddNew();
			board1.MB_Name = "Board 1";
			var board2 = system.Boards.AddNew();
			board2.MB_Name = "Board 2";

			Factory.Save();

			using (var module = GetNewModule(moduleId))
			{
				var provider = new VisualBoardMenuItemProvider();
				var menuItem = provider.GetMenuItems(module).Single();
				menuItem.OnPopup_Exposed();
				BMSFormTestHelper.AssertMenuItems(menuItem, VisualBoardMenuItemProvider.SearchItemText, "-", "Boards not associated with a Release Group");

				var boardMenuItem = BMSFormTestHelper.GetChildMenuItem(menuItem, "Boards not associated with a Release Group");
				BMSFormTestHelper.AssertMenuItems(boardMenuItem, "Board 1", "Board 2");
			}

			using (var module = (ZFilterGridModule)ZFilterModule.GetZFilterModule(ModuleIDs.SalesEnquiry))
			{
				var provider = new VisualBoardMenuItemProvider();
				var menuItem = provider.GetMenuItems(module).Single();
				menuItem.OnPopup_Exposed();
				BMSFormTestHelper.AssertMenuItems(menuItem, VisualBoardMenuItemProvider.SearchItemText, "-", "Boards not associated with a Release Group");

				var boardMenuItem = BMSFormTestHelper.GetChildMenuItem(menuItem, "Boards not associated with a Release Group");
				BMSFormTestHelper.AssertMenuItems(boardMenuItem, "Board 1", "Board 2");
			}
		}

		public void TestTryGetButtonDetail()
		{
			var system = CreateSystem("ORG");
			system.Boards.AddNew();

			Factory.Save();

			using (var module = GetNewModule(ModuleIDs.ProcessHeader))
			{
				var provider = new VisualBoardMenuItemProvider();

				var menuItems = provider.GetMenuItems(module).ToArray();
				AssertEquals(1, menuItems.Length);
				AssertEquals(VisualBoardMenuItemProvider.VisualBoardsMenuItemName, menuItems[0].Name);

				IconTypes buttonImage = IconTypes.None, buttonImageActive = IconTypes.None;
				var buttonToolTip = string.Empty;

				((IFilterGridTopLevelMenuItemProvider)provider).TryGetButtonDetail(menuItems[0], ref buttonImage, ref buttonImageActive, ref buttonToolTip);

				AssertEquals(IconTypes.VisualBoards, buttonImage);
				AssertEquals(IconTypes.VisualBoards, buttonImageActive);
				AssertEquals("Displays a Buffer Management Visual Board, identifying tasks and workflows in a visual management dashboard.", buttonToolTip);
			}
		}

		public void TestGetVisualBoardMenuItem_WhenBoardsAreGlobal()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";

			var myPublishedGlobalBoard = BMSTestHelper.CreateBoard(system, "My published global board");
			myPublishedGlobalBoard.MB_IsPublished = true;
			myPublishedGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myPublishedGlobalBoard.IsGlobal = true;

			var myNonPublishedGlobalBoard = BMSTestHelper.CreateBoard(system, "My non-published global board");
			myNonPublishedGlobalBoard.MB_IsPublished = false;
			myNonPublishedGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myNonPublishedGlobalBoard.IsGlobal = true;

			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			var otherDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GB_HomeBranch = otherBranch.PK;
			otherStaff.GS_GE_HomeDepartment = otherDepartment.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(otherStaff.PK.ToGuid(), otherBranch.PK.ToGuid(), otherDepartment.PK.ToGuid()))
			{
				var otherStaffPublishedGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's published global board");
				otherStaffPublishedGlobalBoard.MB_IsPublished = true;
				otherStaffPublishedGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffPublishedGlobalBoard.IsGlobal = true;

				var otherStaffNonPublishedGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's non-published global board");
				otherStaffNonPublishedGlobalBoard.MB_IsPublished = false;
				otherStaffNonPublishedGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffNonPublishedGlobalBoard.IsGlobal = true;

				Factory.Save();
			}

			using (var module = GetNewModule(ModuleIDs.ProcessHeader))
			{
				Env.Security.VisualBoards.IsAllowed = true;

				var provider = new VisualBoardMenuItemProvider();
				var menuItem = provider.GetMenuItems(module).ElementAt(0);
				menuItem.OnPopup_Exposed();

				BMSFormTestHelper.AssertMenuItems(menuItem, VisualBoardMenuItemProvider.SearchItemText, "-", "Boards not associated with a Release Group");

				var nonReleaseGroupsMenu = BMSFormTestHelper.GetChildMenuItem(menuItem, "Boards not associated with a Release Group");
				BMSFormTestHelper.AssertMenuItems(nonReleaseGroupsMenu, "My non-published global board (private)", "My published global board", "Someone else's published global board");
			}
		}

		public void TestGetVisualBoardMenuItem_WhenBoardsAreNonGlobal()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";

			var myPublishedNonGlobalBoard = BMSTestHelper.CreateBoard(system, "My published non-global board");
			myPublishedNonGlobalBoard.MB_IsPublished = true;
			myPublishedNonGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myPublishedNonGlobalBoard.IsGlobal = false;

			var myNonPublishedNonGlobalBoard = BMSTestHelper.CreateBoard(system, "My non-published non-global board");
			myNonPublishedNonGlobalBoard.MB_IsPublished = false;
			myNonPublishedNonGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myNonPublishedNonGlobalBoard.IsGlobal = false;

			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			var otherDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GB_HomeBranch = otherBranch.PK;
			otherStaff.GS_GE_HomeDepartment = otherDepartment.PK;

			Factory.Save();

			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			CombineAssertions("Make sure the boards are configured to be visible only in current company context.", () =>
			{
				AssertEquals(myPublishedNonGlobalBoard.MB_GC_Company, currentCompanyPK);
				AssertEquals(myNonPublishedNonGlobalBoard.MB_GC_Company, currentCompanyPK);
			});

			using (Env.SetTemporaryUserContext(otherStaff.PK.ToGuid(), otherBranch.PK.ToGuid(), otherDepartment.PK.ToGuid()))
			{
				var otherStaffPublishedNonGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's published non-global board");
				otherStaffPublishedNonGlobalBoard.MB_IsPublished = true;
				otherStaffPublishedNonGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffPublishedNonGlobalBoard.IsGlobal = false;

				var otherStaffNonPublishedNonGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's non-published non-global board");
				otherStaffNonPublishedNonGlobalBoard.MB_IsPublished = false;
				otherStaffNonPublishedNonGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffNonPublishedNonGlobalBoard.IsGlobal = false;

				Factory.Save();

				CombineAssertions("Make sure the boards are configured to be visible only in other company context.", () =>
				{
					AssertNotEquals(currentCompanyPK, otherBranch.GB_GC);
					AssertEquals(otherStaffPublishedNonGlobalBoard.MB_GC_Company, otherBranch.GB_GC);
					AssertEquals(otherStaffNonPublishedNonGlobalBoard.MB_GC_Company, otherBranch.GB_GC);
				});
			}

			using (var module = GetNewModule(ModuleIDs.ProcessHeader))
			{
				Env.Security.VisualBoards.IsAllowed = true;

				var provider = new VisualBoardMenuItemProvider();
				var menuItem = provider.GetMenuItems(module).ElementAt(0);
				menuItem.OnPopup_Exposed();

				BMSFormTestHelper.AssertMenuItems(menuItem, VisualBoardMenuItemProvider.SearchItemText, "-", "Boards not associated with a Release Group");

				var nonReleaseGroupsMenu = BMSFormTestHelper.GetChildMenuItem(menuItem, "Boards not associated with a Release Group");
				BMSFormTestHelper.AssertMenuItems(nonReleaseGroupsMenu, "My non-published non-global board (private)", "My published non-global board");
			}
		}

		#region Performance

		public void TestDbHits_WithManyReleaseGroups()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			for (var i = 0; i < 10; i++)
			{
				var group = BMSTestHelper.CreateGroup(Factory, "GR" + i, "Group " + i);
				BMSTestHelper.CreateReleaseGroup(system, group);
				var board = BMSTestHelper.CreateBoard(system, "Board " + i);
				board.MB_GG_ReleaseGroup = group.PK;
			}

			Factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ BMBoardSchema.Constants.TableName, 0 },
				{ BMBoardSlideshowSchema.Constants.TableName, 0 },
				{ GlbGroupSchema.Constants.TableName, 0 },
			};
			ZFormModaliser.ShowDialogsInTest = true;

			using (var module = new DummyWithWorkflowModule())
			using (AssertMaxDbHitsForAllFactories("There should no longer be any hits on the relevant tables, because we've replaced the Factory.Loads with one quick query. SAD!", expectedHits, ignoreUnspecified: true, includeFactoryPredicate: BMSTestHelper.IsPAVEFactory))
			using (TestConnection.TrackExecutedCommands())
			using (module.ShowPopup())
			{
				Application.DoEvents();

				var rootMenuItem = GetContextMenuItem(module);
				rootMenuItem.OnPopup_Exposed();
				Application.DoEvents();

				AssertVisualBoardCommandExecutedCount("The query should run instead of using Factory.Load. SAD!", 1);
			}
		}

		public void TestLoadMenuItems_ShouldNotHappenUntilMenuItemClicked()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			for (var i = 0; i < 10; i++)
			{
				var group = BMSTestHelper.CreateGroup(Factory, "GR" + i, "Group " + i);
				BMSTestHelper.CreateReleaseGroup(system, group);
				var board = BMSTestHelper.CreateBoard(system, "Board " + i);
				board.MB_GG_ReleaseGroup = group.PK;
			}

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			using (var module = new DummyWithWorkflowModule())
			{
				IZForm popup = null;
				var expectedHitsForShowingModule = new Dictionary<string, int>
				{
					{ BMBoardSchema.Constants.TableName, 0 },
					{ BMSystemSchema.Constants.TableName, 2 }, // BMSystem is an uber factory table so this is sort of okay to do each time the module loads.
					{ GlbGroupSchema.Constants.TableName, 0 },
				};

				try
				{
					ZMenuItem rootMenuItem;

					using (AssertMaxDbHitsForAllFactories("Just showing the module shouldn't load all the boards, only the systems to see if we need a menu item at all. SAD!", expectedHitsForShowingModule, ignoreUnspecified: true, includeFactoryPredicate: BMSTestHelper.IsPAVEFactory))
					using (TestConnection.TrackExecutedCommands())
					{
						popup = module.ShowPopup();
						Application.DoEvents();

						AssertVisualBoardCommandExecutedCount("The query should not be executed just by opening the module. SAD!", 0);

						rootMenuItem = GetContextMenuItem(module);
					}

					var expectedHitsForClickingMenuItem = new Dictionary<string, int>
					{
						{ BMBoardSchema.Constants.TableName, 0 },
						{ BMBoardSlideshowSchema.Constants.TableName, 0 },
						{ BMSystemSchema.Constants.TableName, 0 },
						{ GlbGroupSchema.Constants.TableName, 0 },
					};

					using (AssertMaxDbHitsForAllFactories("Clicking the menu item should no longer load any boards, slideshows, or release groups because we have an efficient query to do that. SAD!", expectedHitsForClickingMenuItem, ignoreUnspecified: true, includeFactoryPredicate: BMSTestHelper.IsPAVEFactory))
					using (TestConnection.TrackExecutedCommands())
					{
						rootMenuItem.OnPopup_Exposed();
						Application.DoEvents();

						AssertVisualBoardCommandExecutedCount("The query should run instead of using Factory.Load. SAD!", 1);
					}
				}
				finally
				{
					popup?.Dispose();
				}
			}
		}

		public void TestClickContextMenuItemTwice_ShouldOnlyQueryDatabaseOnce()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSTestHelper.CreateBoard(system);
			Factory.Save();

			using (var module = new DummyWithWorkflowModule())
			using (module.ShowPopup())
			{
				Application.DoEvents();

				var contextMenuItem = GetContextMenuItem(module);

				AssertMenuItemEffectOnDatabaseQueries("The query should run instead of using Factory.Load. SAD!", 1, () => SimulateContextMenuItemClick(contextMenuItem));
				AssertMenuItemEffectOnDatabaseQueries("Clicking the menu item a second time shouldn't query the database again. SAD!", 0, () => SimulateContextMenuItemClick(contextMenuItem));
			}
		}

		public void TestClickToolStripButtonTwice_ShouldOnlyQueryDatabaseOnce()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSTestHelper.CreateBoard(system);
			Factory.Save();

			using (var module = new DummyWithWorkflowModule())
			using (var popup = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var button = GetToolStripButton(popup);

				AssertMenuItemEffectOnDatabaseQueries("The query should run instead of using Factory.Load. SAD!", 1, () => SimulateToolStripButtonClick(button));
				AssertMenuItemEffectOnDatabaseQueries("Clicking the button a second time shouldn't query the database again. SAD!", 0, () => SimulateToolStripButtonClick(button));
			}
		}

		public void TestClickContextMenuItem_ThenToolStripButton_ShouldShareDataAndNotQueryTwice()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSTestHelper.CreateBoard(system);
			Factory.Save();

			using (var module = new DummyWithWorkflowModule())
			using (var popup = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var contextMenuItem = GetContextMenuItem(module);
				AssertMenuItemEffectOnDatabaseQueries("The query should run instead of using Factory.Load. SAD!", 1, () => SimulateContextMenuItemClick(contextMenuItem));

				var button = GetToolStripButton(popup);
				AssertMenuItemEffectOnDatabaseQueries("Clicking the tool strip button shouldn't query the database if the context menu item has already been clicked. SAD!", 0, () => SimulateToolStripButtonClick(button));
			}
		}

		public void TestClickToolStripButton_ThenContextMenuItem_ShouldShareDataAndNotQueryTwice()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			BMSTestHelper.CreateBoard(system);
			Factory.Save();

			using (var module = new DummyWithWorkflowModule())
			using (var popup = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var button = GetToolStripButton(popup);
				AssertMenuItemEffectOnDatabaseQueries("The query should run instead of using Factory.Load. SAD!", 1, () => SimulateToolStripButtonClick(button));

				var contextMenuItem = GetContextMenuItem(module);
				AssertMenuItemEffectOnDatabaseQueries("Clicking the context menu item shouldn't query the database if the tool strip button has already been clicked. SAD!", 0, () => SimulateContextMenuItemClick(contextMenuItem));
			}
		}

		#endregion

		#region Assertions

		void AssertMenuItemEffectOnDatabaseQueries(string message, int expectedExecutions, Action actionToPerform)
		{
			var expectedHitsForClickingMenuItem = new Dictionary<string, int>
			{
				{ BMBoardSchema.Constants.TableName, 0 },
				{ BMBoardSlideshowSchema.Constants.TableName, 0 },
				{ BMSystemSchema.Constants.TableName, 0 },
				{ GlbGroupSchema.Constants.TableName, 0 },
			};

			using (AssertMaxDbHitsForAllFactories("Clicking the menu item should never cause db hits. SAD!", expectedHitsForClickingMenuItem, ignoreUnspecified: true, includeFactoryPredicate: BMSTestHelper.IsPAVEFactory))
			using (TestConnection.TrackExecutedCommands())
			{
				actionToPerform.Invoke();

				AssertVisualBoardCommandExecutedCount(message, expectedExecutions);
			}
		}

		void AssertVisualBoardCommandExecutedCount(string message, int expectedExecutions)
		{
			var executedMenuCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("VisualBoardMenuItemProvider.GetVisualBoardMenuItems"));
			AssertEquals(message, expectedExecutions, executedMenuCommands.Count());
		}

		#endregion

		#region Implementation

		static ZFilterGridModule GetNewModule(ModuleIdentifier id)
		{
			return (ZFilterGridModule)ZFilterModule.GetZFilterModule(id);
		}

		static ZMenuItem GetContextMenuItem(DummyFilterGridModule module)
		{
			return (ZMenuItem)module.ContextMenuExposed.Single(x => x.Name == VisualBoardMenuItemProvider.VisualBoardsMenuItemName);
		}

		static ZToolStripSplitButton GetToolStripButton(ZForm popup)
		{
			var toolstrip = popup.FindSingle<KToolStrip>("Toolstrip");

			for (var i = 0; i < toolstrip.Items.Count; i++)
			{
				if (toolstrip.Items[i].Text == "Visual Boards")
				{
					return (ZToolStripSplitButton)toolstrip.Items[i];
				}
			}

			return null;
		}

		static void SimulateToolStripButtonClick(ToolStripDropDownItem button)
		{
			try
			{
				button.ShowDropDown();
				Application.DoEvents();
			}
			finally
			{
				button.HideDropDown();
			}
		}

		static void SimulateContextMenuItemClick(ZMenuItem menuItem)
		{
			menuItem.OnPopup_Exposed();
			Application.DoEvents();
		}

		#endregion
	}
}
