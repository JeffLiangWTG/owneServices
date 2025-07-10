using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using EDIMessage = Enterprise.Customs.IE.Business.EDIMessage;
using JobDeclaration = Enterprise.Customs.IE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestMessageTypeWithDescriptionColumn()
		{
			var dec = Factory.New<JobDeclaration>();

			using (var form = new ZForm(dec))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(dec, ".");
				form.Show();

				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");

				CombineAssertions(() =>
				{
					var columnStyleInfo = messagesGrid.GetColumnStyle(nameof(EDIMessage.MessageTypeWithDescription));
					AssertNotNull("User control should have MessageTypeWithDescription column", columnStyleInfo);
					AssertEquals("ColumnStyleType", typeof(ZTextBoxColumnStyle), columnStyleInfo.ColumnStyleType);
					AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(272), columnStyleInfo.Width);
					AssertEquals("IsVisible", false, columnStyleInfo.IsVisible);
				});
			}
		}

		public void TestInterpretedMessageTextWebBrowserCotainsText_WhenOutgoingMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_CEI_Instruction = instruction.PK;

			var message = Factory.New<AESOutboundEDIMessage>();
			message.EM_LinkedObject = header;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var messagesTabUserControl = (MessagesTabUserControl)userControl.BaseMessageUserControl.HostedControl;
				var interpretedMessageTextBox = messagesTabUserControl.Controls.Find("InterpretedMessageTextBox", true).SingleOrDefault() as ZTextBox;

				var interpretedMessageTextWebBrowser = messagesTabUserControl.Controls.Find("InterpretedMessageTextWebBrowser", true).SingleOrDefault() as ZWebBrowser;

				message.EM_MessageInterpretation = "<html></head><body><div>Some text here</div></body></html>";

				const string expected = @"<html>
							</head>
							<body>
								<pre>&lt;html&gt;&lt;/head&gt;&lt;body&gt;&lt;div&gt;Some text here&lt;/div&gt;&lt;/body&gt;&lt;/html&gt;<pre>
							</body>
						</html>";

				AssertEquals(expected, interpretedMessageTextWebBrowser.DocumentText.Trim());
			}
		}

		public void TestInterpretedMessageTextWebBrowserCotainsText_WhenIncomingMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_CEI_Instruction = instruction.PK;

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = header;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var messagesTabUserControl = (MessagesTabUserControl)userControl.BaseMessageUserControl.HostedControl;
				var interpretedMessageTextBox = messagesTabUserControl.Controls.Find("InterpretedMessageTextBox", true).SingleOrDefault() as ZTextBox;

				var interpretedMessageTextWebBrowser = messagesTabUserControl.Controls.Find("InterpretedMessageTextWebBrowser", true).SingleOrDefault() as ZWebBrowser;

				message.EM_MessageInterpretation = "<html></head><body><div>Some text here</div></body></html>";

				const string expected = @"<html></head><body><div>Some text here</div></body></html>";

				AssertEquals(expected, interpretedMessageTextWebBrowser.DocumentText.Trim());
			}
		}
	}
}
