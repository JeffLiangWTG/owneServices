using System.Diagnostics;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class NotifierTest : TestCase
	{
		public void TestShowError()
		{
			var mocker = new MockRepository(MockBehavior.Default);
			var services = new MoqMockServiceContainer(mocker);
			var configuration = new MockConfiguration();
			configuration.Services = services;
			configuration.SetApplicationName("-App-");
			var notifier = new Notifier(configuration);

			using (var form = new Form())
			{
				services.MessageBoxForTest.Setup(m => m.Show(null, "e1", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.None);
				services.MessageBoxForTest.Setup(m => m.Show(null, "e2", "c1", MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.None);
				services.MessageBoxForTest.Setup(m => m.Show(form, "e3", "c2", MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.None);
				notifier.ShowError("e1");
				notifier.ShowError("e2", "c1");
				notifier.ShowError(form, "e3", "c2");
				mocker.VerifyAll();

				configuration.UILevel = UILevel.AutomatedWithNoUI;
				services.EventLogForTest.Setup(m => m.WriteEntry("-App-", "e4", EventLogEntryType.Error));
				notifier.ShowError(form, "e4", "c3");
				mocker.VerifyAll();
			}

			Assert("Mocks verified successfully.", true);
		}
	}
}
