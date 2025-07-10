using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardFilterForCurrentUserProviderTest : BMSTestCaseWithFactory
	{
		public void TestGetQuery()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);
			var system = Factory.NewWithValidTestData<BMSystem>();

			var myPrivateBoard = system.Boards.AddNew();
			myPrivateBoard.MB_Name = "myPrivateBoard";
			myPrivateBoard.MB_IsPublished = false;
			myPrivateBoard.MB_GS_NKStaffCode = staff.GS_Code;

			var otherPrivateBoard = system.Boards.AddNew();
			otherPrivateBoard.MB_Name = "otherPrivateBoard";
			otherPrivateBoard.MB_IsPublished = false;
			otherPrivateBoard.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var myReleaseGroupBoard = system.Boards.AddNew();
			myReleaseGroupBoard.MB_Name = "myReleaseGroupBoard";
			myReleaseGroupBoard.MB_GG_ReleaseGroup = group.PK;
			myReleaseGroupBoard.MB_IsPublished = false;

			var otherReleaseGroupBoard = system.Boards.AddNew();
			otherReleaseGroupBoard.MB_Name = "otherReleaseGroupBoard";
			otherReleaseGroupBoard.MB_GG_ReleaseGroup = Factory.New<GlbGroup>().PK;
			otherReleaseGroupBoard.MB_IsPublished = false;

			var publishedBoard = system.Boards.AddNew();
			publishedBoard.MB_Name = "publishedBoard";
			publishedBoard.MB_IsPublished = true;

			var anotherSystem = Factory.NewWithValidTestData<BMSystem>();
			var boardInOtherSystem = anotherSystem.Boards.AddNew();
			boardInOtherSystem.MB_Name = "boardInOtherSystem";
			boardInOtherSystem.MB_IsPublished = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var boards = Factory.Load<BMBoard>(BMBoardFilterForCurrentUserProvider.GetQuery(new[] { system }));
				AssertCollectionContains("Should contain private current user's boards", myPrivateBoard, boards);
				AssertCollectionContains("Should contain boards for current user's release group", myReleaseGroupBoard, boards);
				AssertCollectionContains("Should contain published boards", publishedBoard, boards);
				AssertEquals(3, boards.Length);
			}
		}
	}
}
