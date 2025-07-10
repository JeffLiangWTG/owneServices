using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE456;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class IE456MessagePrettierTest : DeltaIEMessagePrettierTest<CC456BType, IE456MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Functional rejection<br><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>CRN099999999<br><strong>MRN: </strong>MRN099999999<br><strong>Rejection Type: </strong>415<br><strong>Rejection Date Time: </strong>2021-05-01T12:34:56Z<br><strong>Rejection Reason: </strong>Reason</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""100px"">Sequence Number</td><td width=""100px"">Error Code</td><td width=""75px"">Field Code</td><td width=""100px"">Error Reason</td><td width=""75px"">Path</td><td width=""135px"">Remarks</td></tr><tr><td>1</td><td>99</td><td>type</td><td>BER0071</td><td>consignmentHeaderMasterLevel<wbr>.consignmentHouseLevel[0]<wbr>.transportDocument<wbr>.type</td><td>The Declaration date is not valid.</td></tr></table></p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE456ResponseMessage.json");
	}
}
