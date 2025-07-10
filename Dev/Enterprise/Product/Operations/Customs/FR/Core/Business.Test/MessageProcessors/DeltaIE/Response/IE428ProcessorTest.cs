using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IE428ProcessorTest : DeltaIEBaseProcessorTest<CC428BType, IE428Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessage.json");

		protected override ZString GetExpectedMessageInterpretation() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>ACCEPTE<br><strong>Status Date: </strong>2023-05-11T11:54:37<br><strong>Acceptance Date Time: </strong>2023-05-10T11:54:37<br><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>23FRD0000001228CR8<br><strong>MRN: </strong>23FRD2300001228MR1</p>");

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => "23FRD0000001228CR8";

		protected override ZString GetExpectedMRN() => "23FRD2300001228MR1";

		protected override ZDateTime GetExpectedIssueDate() => new ZDateTime(2023, 05, 11, 11, 54, 37);

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.DeclarationAcceptance;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessageWithoutImportOperationAndDeclarationStatus.json");

		protected override ZString GetMessageTextForFees() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessageWithTaxes.json");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>");

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 3;

		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.DeclarationAcceptedMrnAllocated;

		protected override ZString GetExpectedCESLogInfo() => "CES 2023-05-11 11:54:37";

		protected override ZString GetExpectedEntryNumber() => GetExpectedCRN();

		protected override ZDateTime GetExpectedCustomsEntryIssueDate() => new ZDateTime(2023, 05, 10, 11, 54, 37);

		public void TestUpdateEntryLine()
		{
			var entryHeader = GetEntryHeader();

			var entryLine1 = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var processor = GetDeltaIEBaseProcessor();
			var message = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("CL_ConfirmedCustomsValue should be updated by `CustomsValue` from message.", 1179m, entryLine1.CL_ConfirmedCustomsValue);
			AssertEquals("CL_ConfirmedStatisticalValue should be updated by `StatisticalValue` from message.", 1500m, entryLine1.CL_ConfirmedStatisticalValue);
			AssertEquals("CL_ConfirmedValueForVAT should be updated by `VATBase` from message.", 632m, entryLine1.CL_ConfirmedValueForVAT);
		}

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE428ResponseMessageForMissingField.json");
	}
}
