using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS016;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class IETS016MessagePrettierTest : PNTSMessagePrettierTest<Iets016, IETS016MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Functional rejection<br><strong>LRN: </strong>IETS115INVALIDMESSAGE<br><strong>Notification Date: </strong>1/05/2021 12:34:56 PM<br><strong>Business Validation: </strong>115</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""100px"">Sequence Number</td><td width=""100px"">Error Pointer</td><td width=""100px"">Error Code</td><td width=""100px"">Error Reason</td><td width=""135px"">Remarks</td></tr><tr><td>1</td><td>dateAndTimeOfPresentationOfTheGoods</td><td>99</td><td>BER0069</td><td>The Date and time of presentation of the goods is not valid.</td></tr><tr><td>2</td><td>declarationDate</td><td>99</td><td>BER0071</td><td>The Declaration date is not valid.</td></tr></table></p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PNTS_IETS016ResponseMessage.xml");
	}
}
