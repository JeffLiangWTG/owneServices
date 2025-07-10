using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE917;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IE917MessagePrettierTest : DeltaIEMessagePrettierTest<CC917BType, IE917MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Message was technically rejected by customs</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""left""><td colspan=""6"">MRN# MRN099999999</td></tr><tr align=""center""><td width=""75px"">Line Number</td><td width=""75px"">Column Number</td><td width=""75px"">Pointer</td><td width=""100px"">Error Code</td><td width=""135px"">Error Text</td><td width=""100px"">Original Attribute Value</td></tr><tr><td>0</td><td>0</td><td>error/points.here</td><td>57</td><td>errorText</td><td>originalAttributeValue</td></tr></table></p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ResponseMessage.json");

		public void TestMessageHasNoXmlErrors()
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ResponseMessage_NoXmlErrors.json");
			var messageObject = new IE917MessageDataObject(message);
			var expectedMessageInterpretation = new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Message was technically rejected by customs</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""left""><td colspan=""6"">MRN# MRN099999999</td></tr></table></p>");
			AssertEquals("Message should be human readable when no errors.", expectedMessageInterpretation, messageObject.Prettier.GetMessageInterpretation());
		}
	}
}
