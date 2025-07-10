using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA101;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class FRA101ProcessorTest : DeltaIEBaseProcessorTest<FRA101AType, FRA101Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA101ResponseMessage.json");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>LRN: </strong>0000008412<br><strong>CRN: </strong>25FRD0000006221CR7<br><strong>MRN: </strong>25FRD2300006221MR0<br><strong>Declaration Type: </strong>IM<br><strong>Additional Declaration Type: </strong>A<br><strong>State: </strong>PAIEMENTAUCOMPTANT<br><strong>State Date Time: </strong>2025-03-27T11:59:10<br><strong>Previous State: </strong>LIBERE<br><strong>Event: </strong>PAIEMENT_COMPTANT_EN_ATTENTE</p>");

		protected override ZString GetExpectedLRN() => "0000008412";

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.ToNotifyAPaymentOrAnInsufficientCredit;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA101ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA101ResponseMessageWithoutOperation.json");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>State: </strong>PAIEMENTAUCOMPTANT<br><strong>State Date Time: </strong>2025-03-27T11:59:10<br><strong>Previous State: </strong>LIBERE<br><strong>Event: </strong>PAIEMENT_COMPTANT_EN_ATTENTE</p>");

		protected override ZString GetMessageTextForFees() => ZString.Empty;

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 2;

		protected override ZString GetEventReference() => "FRA101";

		protected override ZString GetExpectedEntryHeaderEntryStatus() => ZString.Empty;

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA101ResponseMessageForMissingField.json");
	}
}
