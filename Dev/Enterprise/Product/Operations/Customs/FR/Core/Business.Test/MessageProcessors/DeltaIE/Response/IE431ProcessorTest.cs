using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE431;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	[TestDate(2023, 05, 11, 11, 54, 37)]
	public class IE431ProcessorTest : DeltaIEBaseProcessorTest<CC431BType, IE431Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE431ResponseMessage.json");

		protected override ZString GetExpectedMessageInterpretation() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>Status: </strong>Timer Expired<br><strong>Status Date: </strong>2023-05-11<br><strong>LRN: </strong>WTLDFRFRM0000000001</p><strong><p style=\"font-size: 120%\">Timer Expiry for Supplementary Declaration</p></strong><p style=\"font-size: 120%\"><strong>Declaration Start Date: </strong>2025-01-01<br><strong>Declaration Expiry Date: </strong>2025-01-31<br><strong>Timer Expiry information: </strong>Supplementary declaration must be lodged within the specified timeframe.</p>");

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.TimerExpirySupplementaryDeclaration;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE431ResponseMessage.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE431ResponseMessageWithoutLRN.json");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Timer Expired<br><strong>Status Date: </strong>2023-05-11</p><strong><p style=""font-size: 120%"">Timer Expiry for Supplementary Declaration</p></strong><p style=""font-size: 120%""><strong>Declaration Start Date: </strong>2025-01-01<br><strong>Declaration Expiry Date: </strong>2025-01-31<br><strong>Timer Expiry information: </strong>Supplementary declaration must be lodged within the specified timeframe.</p>");

		protected override ZString GetMessageTextForFees() => ZString.Empty;

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 2;

		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.TimerExpired;

		protected override ZString GetExpectedCESLogInfo() => "CES 2023-05-11 11:54:37";

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE431ResponseMessageForMissingField.json");
	}
}
