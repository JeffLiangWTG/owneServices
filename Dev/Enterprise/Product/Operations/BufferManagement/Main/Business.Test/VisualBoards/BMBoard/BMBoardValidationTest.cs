using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateName()
		{
			var board = Factory.New<BMSystem>().Boards.AddNew();
			board.Validation.ValidateAll();

			AssertHasError(board.MB_NameInfo, "Please enter a Name.");
		}

		public void TestMBNameUniqueToSystem()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory);
			var system2 = BMSTestHelper.CreateSystem(Factory);

			var board1 = BMSTestHelper.CreateBoard(system1, "Name1");
			board1.Validation.ValidateMB_Name();
			Factory.Save();
			AssertNoError(board1.MB_NameInfo, "The Name has been duplicated and must be unique.");

			var board2 = BMSTestHelper.CreateBoard(system1, "Name1");
			board2.Validation.ValidateMB_Name();
			AssertHasError(board2.MB_NameInfo, "The Name has been duplicated and must be unique.");

			var board3 = BMSTestHelper.CreateBoard(system2, "Name1");
			board3.Validation.ValidateMB_Name();
			AssertNoError(board3.MB_NameInfo, "The Name has been duplicated and must be unique.");

			var board4 = BMSTestHelper.CreateBoard(system1, "name1");
			board4.Validation.ValidateMB_Name();
			AssertHasError(board4.MB_NameInfo, "The Name has been duplicated and must be unique.");
		}

		public void TestValidateIsPublished()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardPublish.IsAllowed = false;

				var system = BMSTestHelper.CreateSystem(Factory, "WKI");
				var board = BMSTestHelper.CreateBoard(system);
				board.MB_IsPublished = false;
				AssertNoErrors(board.MB_IsPublishedInfo);

				Factory.Save();

				board.MB_IsPublished = true;
				AssertHasError(board.MB_IsPublishedInfo, "You do not have permission to publish Visual Boards.");

				Env.Security.BMBoardPublish.IsAllowed = true;

				board.Validation.ValidateAll();
				AssertNoErrors(board.MB_IsPublishedInfo);
			}
		}

		public void TestValidateIsPublished_ErrorWhenHasChangesAndPublished()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var board = BMSTestHelper.CreateBoard(system);
			board.MB_IsPublished = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardPublish.IsAllowed = false;

				AssertNoErrors(board.MB_IsPublishedInfo);

				board.MB_Description = "Dummy Board";

				board.Validation.ValidateAll();
				AssertNoErrors(board.MB_IsPublishedInfo);

				board.MB_IsPublished = false;
				AssertNoErrors(board.MB_IsPublishedInfo);

				Factory.Save();

				board.MB_IsPublished = true;
				AssertHasError(board.MB_IsPublishedInfo, "You do not have permission to publish Visual Boards.");
			}
		}

		public void TestValidateIsPublished_ErrorWhenIsNotInDatabaseAndPublished()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardPublish.IsAllowed = false;

				var board = BMSTestHelper.CreateBoard(system);
				board.MB_GG_ReleaseGroup = Factory.New<GlbGroup>().PK;
				board.MB_IsPublished = true;

				board.Validation.ValidateAll();
				AssertHasError(board.MB_IsPublishedInfo, "You do not have permission to publish Visual Boards.");

				board.MB_IsPublished = false;

				board.Validation.ValidateAll();
				AssertNoErrors(board.MB_IsPublishedInfo);
			}
		}

		[ExpectNoExceptions]
		public void TestNullSystem()
		{
			var board = Factory.NewWithValidTestData<BMBoard>();
			board.MB_FS_System = ZGuid.NewZGuid();
			board.Validation.ValidateMB_Name();
		}

		public void TestMustEitherPublishBoardOrAssignToTeamOrIndividual()
		{
			var board = Factory.New<BMSystem>().Boards.AddNew();
			board.MB_GS_NKStaffCode = ZString.Empty;

			board.MB_IsPublished = true;
			AssertNoError(board.MB_IsPublishedInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertNoError(board.MB_GG_ReleaseGroupInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertNoError(board.MB_GS_NKStaffCodeInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");

			board.MB_IsPublished = false;
			AssertHasError(board.MB_IsPublishedInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertHasError(board.MB_GG_ReleaseGroupInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertHasError(board.MB_GS_NKStaffCodeInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");

			board.MB_GG_ReleaseGroup = Factory.New<GlbGroup>().PK;
			AssertNoError(board.MB_IsPublishedInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertNoError(board.MB_GG_ReleaseGroupInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertNoError(board.MB_GS_NKStaffCodeInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");

			board.MB_GG_ReleaseGroup = ZGuid.Empty;
			AssertHasError(board.MB_IsPublishedInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertHasError(board.MB_GG_ReleaseGroupInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertHasError(board.MB_GS_NKStaffCodeInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");

			board.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			AssertNoError(board.MB_IsPublishedInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertNoError(board.MB_GG_ReleaseGroupInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertNoError(board.MB_GS_NKStaffCodeInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");

			board.MB_GS_NKStaffCode = ZString.Empty;
			AssertHasError(board.MB_IsPublishedInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertHasError(board.MB_GG_ReleaseGroupInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
			AssertHasError(board.MB_GS_NKStaffCodeInfo, "This board must either be Published, have a Release Group specified, or an Owner specified.");
		}
	}
}
