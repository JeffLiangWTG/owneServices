using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA101;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class FRA101MessagePrettierTest : DeltaIEMessagePrettierTest<FRA101AType, FRA101MessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>LRN: </strong>0000008412<br><strong>CRN: </strong>25FRD0000006221CR7<br><strong>MRN: </strong>25FRD2300006221MR0<br><strong>Declaration Type: </strong>IM<br><strong>Additional Declaration Type: </strong>A<br><strong>State: </strong>PAIEMENTAUCOMPTANT<br><strong>State Date Time: </strong>2025-03-27T11:59:10<br><strong>Previous State: </strong>LIBERE<br><strong>Event: </strong>PAIEMENT_COMPTANT_EN_ATTENTE</p>");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA101ResponseMessage.json");
	}
}
