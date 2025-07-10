using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.MFI.AutoeDocAllocation.Testing
{
	public class AutoeDocAllocationValidateEnvironmentTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			AssertEquals("Environment should not be valid", false, AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory));
			using (var testFileName = TempFile.New())
			{
				var testFile = new FileInfo(testFileName.Filename);
				string testDirectory = testFile.Directory.ToString();
				MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory = testDirectory;
				MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory = testDirectory;
				var postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				var staff = postmasterGroup.Staff.AddNew();
				staff.GS_EmailAddress = "blah@blah.com";
				staff.GS_Code = "ZA";
				MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup = postmasterGroup.PK.ToGuid();
				AssertEquals("Environment should now be valid", true, AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory));
			}
		}

		public void TestErrorNotification()
		{
			using (var testFileName = TempFile.New())
			{
				var testFile = new FileInfo(testFileName.Filename);
				string testDirectory = testFile.Directory.ToString();
				//Source Directory
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				string sourceDirectoryError = "MFI AutoeDoc Allocation source directory registry value is missing or directory is not valid:";
				Assert(Notify.AsString.IndexOf(sourceDirectoryError) > 0);
				MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory = "xxx//yyy";
				Notify.Clear();
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				Assert(Notify.AsString.IndexOf(sourceDirectoryError) > 0);
				MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory = testDirectory;
				Notify.Clear();
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				AssertEquals("Source Directory validation should be OK now", -1, Notify.AsString.IndexOf(sourceDirectoryError));
				//Hold Directory
				Notify.Clear();
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				string holdDirectoryError = "MFI AutoeDoc Allocation hold directory registry value is missing or directory is not valid:";
				Assert(Notify.AsString.IndexOf(holdDirectoryError) > 0);
				MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory = "xxx//yyy";
				Notify.Clear();
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				Assert(Notify.AsString.IndexOf(holdDirectoryError) > 0);
				MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory = testDirectory;
				Notify.Clear();
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				AssertEquals("Hold Directory validation should be OK now", -1, Notify.AsString.IndexOf(holdDirectoryError));
				//Notification Group
				string hotificationGroupError = "Auto eDoc Allocation notification group has not been set in registry or is invalid.";
				MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup = Guid.NewGuid();
				Notify.Clear();
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				Assert(Notify.AsString.IndexOf(hotificationGroupError) > 0);
				var postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				var staff = postmasterGroup.Staff.AddNew();
				staff.GS_EmailAddress = "blah@blah.com";
				staff.GS_Code = "ZA";
				MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup = postmasterGroup.PK.ToGuid();
				Notify.Clear();
				AutoeDocAllocationValidateEnvironment.Validate(Notify, Factory);
				AssertEquals("Hold Directory validation should be OK now", -1, Notify.AsString.IndexOf(hotificationGroupError));
			}
		}

		protected NotificationBuffer Notify = new NotificationBuffer();
	}
}
