using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE431;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestDate(2025,1,2)]
	sealed class IE431MessagePrettierTest : DeltaIEMessagePrettierTest<CC431BType, IE431MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>Status: </strong>Timer Expired<br><strong>LRN: </strong>WTLDFRFRM0000000001</p><strong><p style=\"font-size: 120%\">Timer Expiry for Supplementary Declaration</p></strong><p style=\"font-size: 120%\"><strong>Declaration Start Date: </strong>2025-01-01<br><strong>Declaration Expiry Date: </strong>2025-01-31<br><strong>Timer Expiry information: </strong>Supplementary declaration must be lodged within the specified timeframe.</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE431ResponseMessage.json");
	}
}
