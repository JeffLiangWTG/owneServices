using System;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class SynchronisationDirectorTest : TestCaseWithFactoryAndMocks
	{
		[ExpectNoExceptions]
		public void TestUpdateProgress_ShouldNotifyProgressForm()
		{
			var searcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher.Object;
			var director = new SynchronisationDirector(Factory);
			var progressForm = new Mock<IProgressForm>();
			director.Progress = progressForm.Object;

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Smaug";
			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("Smaug");

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Dragons";
			var directoryEntry2 = DummyDirectoryEntryWrapper.CreateGroup("Dragons");

			Factory.Save();

			searcher.Setup(s => s.FindUser("Smaug", string.Empty)).Returns(directoryEntry1);
			searcher.Setup(s => s.FindGroup("Dragons", string.Empty)).Returns(directoryEntry2);

			director.Synchronise();

			progressForm.VerifySet(p => p.Status = "Gathering records to sync", Times.Once);
			progressForm.VerifySet(p => p.Status = "Synchronizing 1 user(s)", Times.Once);
			progressForm.VerifySet(p => p.PercentComplete = 0);

			progressForm.VerifySet(p => p.PercentComplete = 50);
			progressForm.VerifySet(p => p.Status = "Synchronizing 1 group(s)", Times.Once);
			progressForm.VerifySet(p => p.PercentComplete = 100);

			progressForm.Verify(p => p.Dispose(), Times.Once);
		}

		public void TestEnableIntegration_ExceptionThrown_ShouldNotifyComplete()
		{
			var searcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher.Object;
			var director = new SynchronisationDirector(Factory);
			var progressForm = new Mock<IProgressForm>();
			director.Progress = progressForm.Object;

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Smaug";
			Factory.Save();
			Assert(staff.GS_IsActive);

			searcher.Setup(s => s.FindUser("Smaug", string.Empty)).Throws(new Exception());
			searcher.Setup(s => s.FindUser("sysadmin", string.Empty)).Throws(new Exception());

			AssertExceptionThrown<Exception>(() => director.Synchronise());

			progressForm.VerifySet(p => p.PercentComplete = 100, Times.Once);
		}

		public void TestSynchronisationShouldFailWhenNoStaffSecurityRight()
		{
			var staffWithRight = Helper.CreateStaffWithSecurityRights(true, "StaffViewHomeAddressDetails", "GroupsModify");
			var staffWithNoRight = Helper.CreateStaffWithSecurityRights(false, "StaffViewHomeAddressDetails", "GroupsModify");
			Factory.Save();

			var searcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher.Object;
			var director = new SynchronisationDirector(Factory);

			searcher.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser("Dummy"));
			searcher.Setup(s => s.FindGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateGroup("Gummy"));

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithNoRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>(() => director.Synchronise());
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown(() => director.Synchronise());
			}
		}

		public void TestSynchronise_WithPreferredSyncMode_ADIsMaster()
		{
			var searcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher.Object;
			var director = new SynchronisationDirector(Factory);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
			directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime()); //ensure CW1 to be the latest

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
			Factory.Save();

			searcher.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);

			director.Synchronise(preferredSyncMode: Enterprise.Integration.SyncMode.ADIsMaster);

			//CW1 is latest, but Sync should use the preferred sync mode which is AD -> CW1
			Assert(staff.GS_IsActive);
			Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("lord.sauron", staff.GS_LoginName);
			AssertEquals("lord.sauron", directoryEntry.GetValue(GlbStaffSchema.GS_LoginName));
		}

		public void TestSynchronise_WithPreferredSyncMode_EnterpriseIsMaster()
		{
			var searcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher.Object;
			var director = new SynchronisationDirector(Factory);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
			directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime()); //ensure AD to be the latest

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
			Factory.Save();

			searcher.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);
			searcher.Setup(s => s.FindUser(staff.GS_LoginName, string.Empty)).Returns(directoryEntry);
			searcher.Setup(s => s.FindUserByCommonName(staff.GS_LoginName, string.Empty)).Returns(directoryEntry);

			director.Synchronise(preferredSyncMode: Enterprise.Integration.SyncMode.EnterpriseIsMaster);

			//AD is latest, but Sync should use the preferred sync mode which is CW1 -> AD
			Assert(staff.GS_IsActive);
			Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("sauron", staff.GS_LoginName);
			AssertEquals("sauron", directoryEntry.GetValue(GlbStaffSchema.GS_LoginName));
		}
	}
}
