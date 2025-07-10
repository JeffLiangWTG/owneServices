using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.MFI.AutoeDocAllocation.Testing
{
	public abstract class DocumentAllocatorTest : TestCaseWithFactory
	{
		public void TestAllocateFiles()
		{
			using (var testFileName = TempFile.New())
			{
				var testFile = new FileInfo(testFileName.Filename);
				string testDirectory = testFile.Directory.ToString();
				MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory = testDirectory;
				MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory = testDirectory;
				GlbGroup postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup = postmasterGroup.PK.ToGuid();
				var notify = new NotificationBuffer();
				var allocator = GetDocumentAllocator();
				allocator.AllocateFiles(notify);
				Assert("Logger should be populating", notify.AsString.Length > 0);
				Assert("Temp file should be found in processing directory", notify.AsString.Contains("1 file found for automatic eDoc allocation"));
				Assert("Temp file should be rejected, incorrect naming convention", notify.AsString.Contains("has been rejected as it does not match the required naming convention."));
				Assert("Rejected file should be emailed to notification group", notify.AsString.Contains("1 rejected file has been emailed to the notification group"));
				AssertSpecificAllocationResult(notify);
			}
		}

		protected abstract DocumentAllocator GetDocumentAllocator();
		protected abstract void AssertSpecificAllocationResult(NotificationBuffer notify);
	}
}
