using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA102;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class FRA102ProcessorTest : DeltaIEBaseProcessorTest<FRA102AType, FRA102Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA102ResponseMessage.json");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>01FRAB1234567890R1<br><strong>MRN: </strong>01FRAB1234567890A1<br><strong>Operator request reference: </strong>ABC123<br><strong>Customs request reference: </strong>FR456789<br><strong>Request Registration date and time: </strong>2023-06-21T12:15:30</p>");

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.RegistrationOfTheInvalidationOrAmendment;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA102ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA102ResponseMessageWithoutImportOrExportOperation.json");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Operator request reference: </strong>ABC123<br><strong>Customs request reference: </strong>FR456789<br><strong>Request Registration date and time: </strong>2023-06-21T12:15:30</p>");

		protected override ZString GetMessageTextForFees() => ZString.Empty;

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 2;

		protected override ZString GetEventReference() => "FRA102";

		protected override ZString GetExpectedEntryHeaderEntryStatus() => ZString.Empty;

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA102ResponseMessageForMissingField.json");
	}
}
