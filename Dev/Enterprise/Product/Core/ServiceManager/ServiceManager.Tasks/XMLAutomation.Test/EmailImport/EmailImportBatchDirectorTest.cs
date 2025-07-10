using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class EmailImportBatchDirectorTest : TestCaseWithFactory
	{
		public void TestRun()
		{
			EmailImportBatchDirectorForTest processor = new EmailImportBatchDirectorForTest(null);
			processor.Run();
			AssertEquals(-1, processor.Notify.AsString.IndexOf("Execute has been called."));

			string tempDirectory = Env.TempPath;
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDirectory);

			processor.Run();
			AssertEquals(true, processor.Notify.AsString.IndexOf("Execute has been called.") > -1);
		}

		class LocalCartageBookingEmailReaderForTest : LocalCartageBookingEmailReader
		{
			public LocalCartageBookingEmailReaderForTest(StringRegistryItem path, NotificationBuffer notify)
				: base(path, notify)
			{
			}

			protected override void Execute(CancellationToken token)
			{
				base.Execute(token);
				Buffer.Notify(new InfoNotification("Execute has been called."));
			}
		}

		class EmailImportBatchDirectorForTest : EmailImportBatchDirector
		{
			public EmailImportBatchDirectorForTest(INotifications notify) : base(notify) { }
			protected override LocalCartageBookingEmailReader GetNewEmailReader()
			{
				return new LocalCartageBookingEmailReaderForTest(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, Notify);
			}
		}
	}
}
