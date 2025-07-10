using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE456;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class IE456ProcessorTest : DeltaIEBaseProcessorTest<CC456BType, IE456Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE456ResponseMessage.json");

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE456ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE456ResponseMessageWithOnlyFunctionalError.json");

		protected override ZString GetMessageTextForFees() => ZString.Empty;

		protected override ZString GetExpectedMessageInterpretation()
		{
			return new ZString(
				@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Functional rejection<br><strong>LRN: </strong>WTLDFRFRM0000000001<br><strong>CRN: </strong>CRN099999999<br><strong>MRN: </strong>MRN099999999<br><strong>Rejection Type: </strong>415<br><strong>Rejection Date Time: </strong>2021-05-01T12:34:56Z<br><strong>Rejection Reason: </strong>Reason</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""100px"">Sequence Number</td><td width=""100px"">Error Code</td><td width=""75px"">Field Code</td><td width=""100px"">Error Reason</td><td width=""75px"">Path</td><td width=""135px"">Remarks</td></tr><tr><td>1</td><td>99</td><td>type</td><td>BER0071</td><td>consignmentHeaderMasterLevel<wbr>.consignmentHouseLevel[0]<wbr>.transportDocument<wbr>.type</td><td>The Declaration date is not valid.</td></tr></table></p>");
		}

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef()
		{
			return new ZString(
				@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Functional rejection</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""center""><td width=""100px"">Sequence Number</td><td width=""100px"">Error Code</td><td width=""75px"">Field Code</td><td width=""100px"">Error Reason</td><td width=""75px"">Path</td><td width=""135px"">Remarks</td></tr><tr><td>1</td><td>99</td><td>type</td><td>BER0071</td><td>consignmentHeaderMasterLevel<wbr>/consignmentHouseLevel[0]<wbr>/transportDocument<wbr>/type</td><td>The Declaration date is not valid.</td></tr></table></p>");
		}

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => "CRN099999999";

		protected override ZString GetExpectedMRN() => "MRN099999999";

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.Error;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.FunctionalRejection;

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 2;

		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.DeclarationRejected;

		protected override ZString GetExpectedCESLogInfo() => "CES 2024-01-08 00:00:00";

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE456ResponseMessageForMissingField.json");
	}
}
