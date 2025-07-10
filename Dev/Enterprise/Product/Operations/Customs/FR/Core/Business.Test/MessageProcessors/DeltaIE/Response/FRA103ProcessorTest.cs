using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.FRA103;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRA103ProcessorTest : DeltaIEBaseProcessorTest<FRA103AType, FRA103Processor>
	{
		protected override ZString GetEventReference() => "FRA103";

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 2;

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedMessageInterpretation() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>01FRAB1234567890R1<br><strong>MRN: </strong>01FRAB1234567890A1<br><strong>Customs request reference: </strong>FR456789<br><strong>Timer request instruction start date: </strong>2023-08-21T17:15:13<br><strong>Initial timer request instruction expiry date: </strong>2023-08-21T19:15:13<br><strong>New timer request instruction expiry date: </strong>2023-08-22T20:15:13<br><strong>Extension Information: </strong>extension info</p>");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>Customs request reference: </strong>FR456789<br><strong>Timer request instruction start date: </strong>2023-08-21T17:15:13<br><strong>Initial timer request instruction expiry date: </strong>2023-08-21T19:15:13<br><strong>New timer request instruction expiry date: </strong>2023-08-22T20:15:13<br><strong>Extension Information: </strong>extension info</p>");

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.ValidationRequestExtension;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA103ResponseMessage.json");

		protected override ZString GetMessageTextForFees() => ZString.Empty;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA103ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA103ResponseMessageWithoutImportOrExportOperation.json");

		protected override ZString GetExpectedEntryHeaderEntryStatus() => ZString.Empty;

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_FRA103ResponseMessageForMissingField.json");
	}
}
