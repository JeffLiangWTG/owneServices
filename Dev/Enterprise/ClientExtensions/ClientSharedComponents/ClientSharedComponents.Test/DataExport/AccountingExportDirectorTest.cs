using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.ClientSharedComponents.Testing
{
	public abstract class AccountingExportDirectorTest : TestCaseWithFactory
	{
		public void TestExecuteWithSuccess()
		{
			SetupEnvironment();
			TempDirectory.DeleteDirectory(ExportDirectory);
			SetupNewInvoices();
			Factory.Save();

			Assert("Empty folder", DirInfo.GetFiles().Length == 0);
			ExecuteExportDirector();
			Assert("Process has run", ExecuteHasBeenRun);
			Assert("Email sent", Env.OutgoingMailManager.EmailsCreated.Count >= 1);
			AssertNotificationsWhenSuccess();
			TempDirectory.DeleteDirectory(ExportDirectory);
		}

		public void TestInvalidExecuteWithNoInvoices()
		{
			SetupEnvironment();
			TempDirectory.DeleteDirectory(ExportDirectory);
			Factory.Save();

			Assert("Empty folder", DirInfo.GetFiles().Length == 0);
			ExecuteExportDirector();
			Assert("Process has run", ExecuteHasBeenRun);
			Assert("Email sent", Env.OutgoingMailManager.EmailsCreated.Count >= 1);
			Assert("Empty file created.", NotifyBuffer.AsString.Contains("Error: No account transaction files were created for batch"));
			AssertNotificationsWhenFailure();
			TempDirectory.DeleteDirectory(ExportDirectory);
		}

		public void TestExecute_InvalidRegistryPath()
		{
			SetupEnvironment();
			TempDirectory.DeleteDirectory(tempPath);
			SetInvalidDirectory();
			SetupNewInvoices();
			Factory.Save();

			ExecuteExportDirector();
			AsserFileCreated();
			TempDirectory.DeleteDirectory(tempPath);
		}

		protected virtual void AssertNotificationsCommon()
		{
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

		protected ZString tempPath = Env.TempPath;
		protected abstract void SetupEnvironment();
		protected abstract void SetupNewInvoices();
		protected abstract void ExecuteExportDirector();
		protected abstract void SetInvalidDirectory();
		protected abstract void AsserFileCreated();
		protected abstract bool ExecuteHasBeenRun { get; }
		protected abstract ZString ExportDirectory { get; }
	}
}
