using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestInterpretedMessageTextWebBrowserCotainsText_WhenOutgoingMessage()
		{
			var exitControlLayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new ExitControlLayoutProviderForReportsMessagesTest()) }
			};

			using (ObjectFactory.Substitute("ExitControlLayoutProviders", exitControlLayoutProviders))
			{
				var report = Factory.NewWithValidTestData<CusExitReport>();
				var message = report.Messages.AddNew();
				message.EM_LinkedObject = report;
				message.IsTransmitMessage = true;

				userControl.SetDataBinding(report, "");

				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.SetDataBinding(report.Messages, "");
				messagesGrid.Select(0);

				var interpretedMessageTextBox = userControl.Controls.Find("InterpretedMessageTextBox", true).SingleOrDefault() as ZTextBox;
				var interpretedMessageTextWebBrowser = userControl.Controls.Find("InterpretedMessageTextWebBrowser", true).SingleOrDefault() as ZWebBrowser;

				interpretedMessageTextBox.Text = "<html></head><body><div>Some text here</div></body></html>";

				const string expected = @"<html><head></head><body><pre>&lt;html&gt;&lt;/head&gt;&lt;body&gt;&lt;div&gt;Some text here&lt;/div&gt;&lt;/body&gt;&lt;/html&gt;<pre></body></html>";

				AssertEquals(expected, interpretedMessageTextWebBrowser.DocumentText.Trim());
			}
		}

		[RequiresSTA]
		public void TestInterpretedMessageTextWebBrowserCotainsText_WhenIncomingMessage()
		{
			var report = Factory.NewWithValidTestData<CusExitReport>();
			var message = report.Messages.AddNew();
			message.EM_LinkedObject = report;
			message.IsTransmitMessage = false;

			userControl.SetDataBinding(report, "");

			var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
			messagesGrid.SetDataBinding(report.Messages, "");
			messagesGrid.Select(0);

			var interpretedMessageTextBox = userControl.Controls.Find("InterpretedMessageTextBox", true).SingleOrDefault() as ZTextBox;
			var interpretedMessageTextWebBrowser = userControl.Controls.Find("InterpretedMessageTextWebBrowser", true).SingleOrDefault() as ZWebBrowser;

			interpretedMessageTextBox.Text = "<html></head><body><div>Some text here</div></body></html>";

			const string expected = @"<html></head><body><div>Some text here</div></body></html>";

			AssertEquals(expected, interpretedMessageTextWebBrowser.DocumentText.Trim());
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new MessagesTabUserControl();
		}
		MessagesTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		sealed class ExitControlLayoutProviderForReportsMessagesTest : ExitControlLayoutProvider, EU.ExitControl.GUI.IExitControlLayoutProvider
		{
			BaseMessagesTabUserControl EU.ExitControl.GUI.IExitControlLayoutProvider.CreateReportsMessagesUserControl() => new MessagesTabUserControl();
		}
	}
}
