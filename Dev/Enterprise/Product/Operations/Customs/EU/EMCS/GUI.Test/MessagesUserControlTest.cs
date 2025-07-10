using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	public class MessagesUserControlTest : TestCaseWithFactory
	{
		public void TestMessageGrid()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			using (var control = new MessagesUserControl())
			{
				control.SetDataBinding(declaration.Messages, ZString.Empty);
				var messagesGrid = control.MessagesGrid;
				CombineAssertions(() =>
				{
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_MessageDateTime).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_SystemCreateUser).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_SystemCreateTimeUtc).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_SendOrReceiveHumanReadable).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_MessageType).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_MessageSubType).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_ApplicationReference).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_MessageNum).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_Status).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_InterchangeNumber).IsReadOnly);
					AssertEquals(true, messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_InterchangeStatus).IsReadOnly);
				});
			}
		}

		public void TestInterpretedMessageTextWebBrowser()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var message = Factory.New<EDIMessage>();
			message.EM_LinkedObject = declaration;

			using (var control = new MessagesUserControl())
			{
				control.SetDataBinding(declaration.Messages, ZString.Empty);
				var interpretedTextBox = control.InterpretedMessageTextBox;
				var interpretedWebBrowser = control.InterpretedMessageWebBrowser;

				CombineAssertions(() =>
				{
					AssertType<ZWebBrowser>(interpretedWebBrowser);

					var expectedText = "<html></head><body><div>Some text here</div></body></html>";
					message.EM_MessageInterpretation = expectedText;
					AssertEquals(expectedText, interpretedWebBrowser.DocumentText);

					message.EM_MessageInterpretation = ZString.Empty;
					AssertEquals(ZString.Empty, interpretedWebBrowser.DocumentText);
				});
			}
		}
	}
}

