using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Testing
{
	public abstract class AccountsExportDirectorTest : TestCaseWithFactory
	{
		public void TestExecuteWithSuccess()
		{
			SetupEnvironment();
			TempDirectory.DeleteDirectory(ExportDirectory);
			SetupNewInvoices();
			Factory.Save();

			ExecuteExportDirector();
			AssertNotificationsWhenSuccess();
			TempDirectory.DeleteDirectory(ExportDirectory);
		}

		public void TestExecute_InvalidRegistryPath()
		{
			SetupEnvironment();
			TempDirectory.DeleteDirectory(tempPath);
			SetInvalidDirectory();
			SetupNewInvoices();
			Factory.Save();

			Assert("Empty folder", TempDirInfo.GetFiles().Length == 0);
			ExecuteExportDirector();
			AsserFileCreated();
			TempDirectory.DeleteDirectory(tempPath);
		}

		public void TestInvalidExecuteWithNoInvoices()
		{
			SetupEnvironment();
			TempDirectory.DeleteDirectory(ExportDirectory);
			Factory.Save();

			Assert("Empty folder", DirInfo.GetFiles().Length == 0);
			ExecuteExportDirector();
			Assert("Empty file created.", NotifyBuffer.AsString.Contains("No transactions in batch to export."));
			AssertNotificationsWhenFailure();
			TempDirectory.DeleteDirectory(ExportDirectory);
		}

		protected virtual void AssertNotificationsWhenSuccess()
		{
			Assert("Not Empty folder", DirInfo.GetFiles().Length > 0);
			Assert("1 files created", NotifyBuffer.AsString.Contains("File created: \"Invoice_"));
		}

		protected virtual void AssertNotificationsWhenFailure()
		{
			Assert("Empty folder", DirInfo.GetFiles().Length == 0);
			Assert("No file created.", NotifyBuffer.AsString.Contains("File created: none."));
		}

		protected NotificationBuffer NotifyBuffer
		{
			get { return notifyBuffer ?? (notifyBuffer = new NotificationBuffer()); }
		}
		NotificationBuffer notifyBuffer;

		protected DirectoryInfo DirInfo
		{
			get { return dirInfo ?? (dirInfo = new DirectoryInfo(ExportDirectory)); }
		}
		DirectoryInfo dirInfo;

		protected DirectoryInfo TempDirInfo
		{
			get { return tempDirInfo ?? (tempDirInfo = new DirectoryInfo(Env.TempPath)); }
		}
		DirectoryInfo tempDirInfo;

		protected ZString lastMessage
		{
			get { return UnitTestUserNotification.Instance.LastMessage.Text; }
		}

		protected ZString tempPath = Env.TempPath;
		protected abstract void SetupEnvironment();
		protected abstract void SetupNewInvoices();
		protected abstract void ExecuteExportDirector();
		protected abstract void SetInvalidDirectory();
		protected abstract void AsserFileCreated();
		protected abstract ZString ExportDirectory { get; }
	}
}
