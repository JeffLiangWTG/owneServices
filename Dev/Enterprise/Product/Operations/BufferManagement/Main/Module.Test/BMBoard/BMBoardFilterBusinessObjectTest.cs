using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMBoardFilterBusinessObject))]
	class BMBoardFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNameFilter()
		{
			var board = Factory.New<BMBoard>();
			board.MB_Name = "Mai Bored";

			var bizo = GetNewFilterStripBusinessObject();
			var nameFilter = (ModuleTextFilter)bizo["Name"];

			nameFilter.IsActive = true;
			nameFilter.Property = "Mai Bored";
			AssertEquals(board, Factory.LoadTop1<BMBoard>(bizo.Filter));

			nameFilter.Property = "Your board";
			AssertEquals(null, Factory.LoadTop1<BMBoard>(bizo.Filter));
		}

		public void TestDescriptionFilter()
		{
			var board = Factory.New<BMBoard>();
			board.MB_Description = "Mai Bored";

			var bizo = GetNewFilterStripBusinessObject();
			var descFilter = (ModuleTextFilter)bizo["Description"];

			descFilter.IsActive = true;
			descFilter.Property = "Mai Bored";
			AssertEquals(board, Factory.LoadTop1<BMBoard>(bizo.Filter));

			descFilter.Property = "Your board";
			AssertEquals(null, Factory.LoadTop1<BMBoard>(bizo.Filter));
		}

		public void TestPublishedFilter()
		{
			var board = Factory.New<BMBoard>();
			board.MB_IsPublished = true;

			var bizo = GetNewFilterStripBusinessObject();
			var descFilter = (ModuleFlagsFilter)bizo["Published"];

			descFilter.IsActive = true;
			descFilter.Property0 = true;
			AssertEquals(board, Factory.LoadTop1<BMBoard>(bizo.Filter));

			descFilter.Property1 = true;
			AssertEquals(null, Factory.LoadTop1<BMBoard>(bizo.Filter));
		}

		public void TestGlobalFilter()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");

			var myGlobalBoard = BMSTestHelper.CreateBoard(system, "My global board");
			myGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myGlobalBoard.IsGlobal = true;

			var myNonGlobalBoard = BMSTestHelper.CreateBoard(system, "My non-global board");
			myNonGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myNonGlobalBoard.IsGlobal = false;

			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			var otherDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GB_HomeBranch = otherBranch.PK;
			otherStaff.GS_GE_HomeDepartment = otherDepartment.PK;

			Factory.Save();

			BMBoard otherStaffGlobalBoard;
			BMBoard otherStaffNonGlobalBoard;
			using (Env.SetTemporaryUserContext(otherStaff.PK.ToGuid(), otherBranch.PK.ToGuid(), otherDepartment.PK.ToGuid()))
			{
				otherStaffGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's global board");
				otherStaffGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffGlobalBoard.IsGlobal = true;

				otherStaffNonGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's non-global board");
				otherStaffNonGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffNonGlobalBoard.IsGlobal = false;

				Factory.Save();
			}

			var bizo = GetNewFilterStripBusinessObject();
			var globalStatusFilter = (ModuleTextFilter)bizo["Global"];

			globalStatusFilter.IsActive = true;
			globalStatusFilter.Property = GlobalStatusList.Codes.Global;
			var boards = Factory.Load<BMBoard>(bizo.Filter);
			CombineAssertions("Should only contain global boards.", () =>
			{
				AssertCollectionContains(myGlobalBoard, boards);
				AssertCollectionNotContains(myNonGlobalBoard, boards);
				AssertCollectionContains(otherStaffGlobalBoard, boards);
				AssertCollectionNotContains(otherStaffNonGlobalBoard, boards);
			});

			globalStatusFilter.IsActive = true;
			globalStatusFilter.Property = GlobalStatusList.Codes.CurrentCompany;
			boards = Factory.Load<BMBoard>(bizo.Filter);
			CombineAssertions("Should only contain non-global boards belonging to the current company.", () =>
			{
				AssertCollectionNotContains(myGlobalBoard, boards);
				AssertCollectionContains(myNonGlobalBoard, boards);
				AssertCollectionNotContains(otherStaffGlobalBoard, boards);
				AssertCollectionNotContains(otherStaffNonGlobalBoard, boards);
			});

			globalStatusFilter.IsActive = true;
			globalStatusFilter.Property = GlobalStatusList.Codes.All;
			boards = Factory.Load<BMBoard>(bizo.Filter);
			CombineAssertions("Should contain global boards and non-global boards belonging to the current company.", () =>
			{
				AssertCollectionContains(myGlobalBoard, boards);
				AssertCollectionContains(myNonGlobalBoard, boards);
				AssertCollectionContains(otherStaffGlobalBoard, boards);
				AssertCollectionNotContains(otherStaffNonGlobalBoard, boards);
			});

			globalStatusFilter.IsActive = false;
			boards = Factory.Load<BMBoard>(bizo.Filter);
			CombineAssertions("Should contain global boards and non-global boards belonging to the current company by default.", () =>
			{
				AssertCollectionContains(myGlobalBoard, boards);
				AssertCollectionContains(myNonGlobalBoard, boards);
				AssertCollectionContains(otherStaffGlobalBoard, boards);
				AssertCollectionNotContains(otherStaffNonGlobalBoard, boards);
			});
		}

		public void TestSystemFilter()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_Description = "Mai Bored";

			var bizo = GetNewFilterStripBusinessObject();
			var systemFilter = (ModuleGuidFilter)bizo["System"];

			systemFilter.IsActive = true;
			systemFilter.Property = system.PK;
			AssertEquals(board, Factory.LoadTop1<BMBoard>(bizo.Filter));

			systemFilter.Property = ZGuid.NewZGuid();
			AssertEquals(null, Factory.LoadTop1<BMBoard>(bizo.Filter));
		}

		public void TestReleaseGroupFilter()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_Description = "Mai Bored";
			board.MB_GG_ReleaseGroup = Factory.New<GlbGroup>().PK;

			var bizo = GetNewFilterStripBusinessObject();
			var releaseGroupFilter = (ModuleGuidFilter)bizo["ReleaseGroup"];

			releaseGroupFilter.IsActive = true;
			releaseGroupFilter.Property = board.MB_GG_ReleaseGroup;
			AssertEquals(board, Factory.LoadTop1<BMBoard>(bizo.Filter));

			releaseGroupFilter.Property = ZGuid.NewZGuid();
			AssertEquals(null, Factory.LoadTop1<BMBoard>(bizo.Filter));
		}

		public void TestOwnerFilter()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_Description = "Mai Bored";
			board.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;

			var bizo = GetNewFilterStripBusinessObject();
			var ownerFilter = (ModuleNkFilter)bizo["Owner"];

			ownerFilter.IsActive = true;
			ownerFilter.Property = GlbStaff.CurrentUser.GS_Code;
			AssertEquals(board, Factory.LoadTop1<BMBoard>(bizo.Filter));

			ownerFilter.Property = "ZZZ";
			AssertEquals(null, Factory.LoadTop1<BMBoard>(bizo.Filter));
		}

		public void TestFilter_WhenBoardsAreGlobal()
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

			BMBoard otherStaffPublishedGlobalBoard;
			BMBoard otherStaffNonPublishedGlobalBoard;

			using (Env.SetTemporaryUserContext(otherStaff.PK.ToGuid(), otherBranch.PK.ToGuid(), otherDepartment.PK.ToGuid()))
			{
				otherStaffPublishedGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's published global board");
				otherStaffPublishedGlobalBoard.MB_IsPublished = true;
				otherStaffPublishedGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffPublishedGlobalBoard.IsGlobal = true;

				otherStaffNonPublishedGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's non-published global board");
				otherStaffNonPublishedGlobalBoard.MB_IsPublished = false;
				otherStaffNonPublishedGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffNonPublishedGlobalBoard.IsGlobal = true;

				Factory.Save();
			}

			var bizo = GetNewFilterStripBusinessObject();
			var boards = Factory.Load<BMBoard>(bizo.Filter);

			CombineAssertions("Should contain all global boards.", () =>
			{
				AssertCollectionContains(myPublishedGlobalBoard, boards);
				AssertCollectionContains(myNonPublishedGlobalBoard, boards);
				AssertCollectionContains(otherStaffPublishedGlobalBoard, boards);
				AssertCollectionContains(otherStaffNonPublishedGlobalBoard, boards);
			});
		}

		public void TestFilter_WhenBoardsAreNonGlobal()
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

			BMBoard otherStaffPublishedNonGlobalBoard;
			BMBoard otherStaffNonPublishedNonGlobalBoard;

			using (Env.SetTemporaryUserContext(otherStaff.PK.ToGuid(), otherBranch.PK.ToGuid(), otherDepartment.PK.ToGuid()))
			{
				otherStaffPublishedNonGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's published non-global board");
				otherStaffPublishedNonGlobalBoard.MB_IsPublished = true;
				otherStaffPublishedNonGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffPublishedNonGlobalBoard.IsGlobal = false;

				otherStaffNonPublishedNonGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's non-published non-global board");
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

			var bizo = GetNewFilterStripBusinessObject();
			var boards = Factory.Load<BMBoard>(bizo.Filter);

			CombineAssertions("Should only contain the boards that are configured to be visible in current company context.", () =>
			{
				AssertCollectionContains(myPublishedNonGlobalBoard, boards);
				AssertCollectionContains(myNonPublishedNonGlobalBoard, boards);
				AssertCollectionNotContains(otherStaffPublishedNonGlobalBoard, boards);
				AssertCollectionNotContains(otherStaffNonPublishedNonGlobalBoard, boards);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMBoardFilterBusinessObject();
		}
	}
}
