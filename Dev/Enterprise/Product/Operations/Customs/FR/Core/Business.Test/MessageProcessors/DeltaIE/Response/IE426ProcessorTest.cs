using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IE426ProcessorTest : DeltaIEBaseProcessorTest<CC426BType, IE426Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE426ResponseMessage.json");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>ANTICIPATED<br><strong>Status Date: </strong>2023-05-11T11:54:37<br><strong>Registration Date Time: </strong>2023-05-10T12:09:14<br><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>23FRD0000001228CR8<br><strong>Presentation Notification Due Date: </strong>2023-07-10T13:34:22<br><strong>Estimated Presentation Notification Date: </strong>2023-08-10T15:18:53</p><style> table, th, td {border: 1px solid black; border-collapse: collapse;} th, td { padding: 10px; text-align: left;}</style><table><tr><td rowspan=""6"" colspan=""1""><strong><p style=""font-size: 120%"">Remarks</p></strong></td><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">123</td></tr><tr><td rowspan=""1"" colspan=""1"">Code</td><td rowspan=""1"" colspan=""1"">ASL</td></tr><tr><td rowspan=""1"" colspan=""1"">Reason</td><td rowspan=""1"" colspan=""1"">ASL-Motif</td></tr><tr><td rowspan=""1"" colspan=""1"">Sequence Number</td><td rowspan=""1"" colspan=""1"">1234</td></tr><tr><td rowspan=""1"" colspan=""1"">Code</td><td rowspan=""1"" colspan=""1"">PSL</td></tr><tr><td rowspan=""1"" colspan=""1"">Reason</td><td rowspan=""1"" colspan=""1"">PSL-Motif</td></tr><table>");

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => "23FRD0000001228CR8";

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZDateTime GetExpectedIssueDate() => new ZDateTime(2023, 05, 11, 11, 54, 37);

		protected override ZDateTime GetMRNExpectedIssueDate() => ZDateTime.Empty;

		protected override ZDateTime GetCRNExpectedExpiryDate() => new ZDateTime(2023, 07, 10, 13, 34, 22);

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.PrelodgedDeclarationAcceptance;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE426ResponseMessage.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE426ResponseMessageWithoutImportOperationAndDeclarationStatus.json");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>ANTICIPATED<br><strong>Status Date: </strong>2023-05-11T11:54:37</p>");

		protected override ZString GetMessageTextForFees() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE426ResponseMessageWithTaxes.json");

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 3;

		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered;

		protected override ZString GetExpectedCESLogInfo() => "CES 2023-05-11 11:54:37";

		protected override ZString GetExpectedEntryNumber() => GetExpectedCRN();

		protected override ZDateTime GetExpectedCustomsEntryIssueDate() => new ZDateTime(2023, 05, 10, 12, 09, 14);

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE426ResponseMessageForMissingField.json");
	}
}
