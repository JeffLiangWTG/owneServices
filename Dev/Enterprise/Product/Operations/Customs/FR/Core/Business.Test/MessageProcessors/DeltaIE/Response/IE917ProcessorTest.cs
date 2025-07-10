using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE917;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class IE917ProcessorTest : DeltaIEBaseProcessorTest<CC917BType, IE917Processor>
	{
		public void TestAbleToProcessMessageWithMultipleNamesForSameProperty()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.Add(entryHeader);

			var processor = GetDeltaIEBaseProcessor();

			CombineAssertions(() =>
			{
				var importOperationMessage = GetDeltaIEFREDIMessageWithMessageText(GetImportOperationMessageText());
				entryHeader.CorrelationID = "WTLDFRFRM0000000001";
				entryHeader.CH_Status = MessageStatusCodeList.Codes.NACK;
				Factory.Save();
				processor.ProcessMessage(importOperationMessage);
				AssertEquals("Should be able to process message with field `Operation` named as `ImportOperation`.", EDIMessageStatusList.Codes.ProcessedOK, importOperationMessage.EM_Status);
				AssertEquals("EntryStatus should be updated if LRN is read properly from ImportOperation in the response messages.", MessageStatusCodeList.Codes.Error, entryHeader.CH_Status);

				entryHeader.CorrelationID = "WRONGLRN";
				Factory.Save();
				processor.ProcessMessage(importOperationMessage);
				AssertEquals("Should not be able to process message in case LRN does not match.", EDIMessageStatusList.Codes.Discarded, importOperationMessage.EM_Status);

				var exportOperationMessage = GetDeltaIEFREDIMessageWithMessageText(GetExportOperationMessageText());
				entryHeader.CorrelationID = "WTLDFRFRM0000000002";
				entryHeader.CH_Status = MessageStatusCodeList.Codes.NACK;
				Factory.Save();
				processor.ProcessMessage(exportOperationMessage);
				AssertEquals("Should be able to process message with field `Operation` named as `ExportOperation`.", EDIMessageStatusList.Codes.ProcessedOK, exportOperationMessage.EM_Status);
				AssertEquals("EntryStatus should be updated if LRN is read properly from ExportOperation in the response messages.", MessageStatusCodeList.Codes.Error, entryHeader.CH_Status);

				entryHeader.CorrelationID = "WRONGLRN";
				Factory.Save();
				processor.ProcessMessage(exportOperationMessage);
				AssertEquals("Should not be able to process message in case LRN does not match.", EDIMessageStatusList.Codes.Discarded, exportOperationMessage.EM_Status);

				var importOrExportOperationMessage = GetDeltaIEFREDIMessageWithMessageText(GetImportOrExportOperationMessageText());
				entryHeader.CorrelationID = "WTLDFRFRM0000000003";
				entryHeader.CH_Status = MessageStatusCodeList.Codes.NACK;
				Factory.Save();
				processor.ProcessMessage(importOrExportOperationMessage);
				AssertEquals("Should be able to process message with field `Operation` named as `ImportOrExportOperation`.", EDIMessageStatusList.Codes.ProcessedOK, importOrExportOperationMessage.EM_Status);
				AssertEquals("EntryStatus should be updated if LRN is read properly from ImportOrExportOperation in the response messages.", MessageStatusCodeList.Codes.Error, entryHeader.CH_Status);

				entryHeader.CorrelationID = "WRONGLRN";
				Factory.Save();
				processor.ProcessMessage(importOrExportOperationMessage);
				AssertEquals("Should not be able to process message in case LRN does not match.", EDIMessageStatusList.Codes.Discarded, importOrExportOperationMessage.EM_Status);
			});
		}

		string GetImportOrExportOperationMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ImportOrExportOperationResponseMessage.json");

		string GetExportOperationMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ExportOperationResponseMessage.json");

		string GetImportOperationMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ImportOperationResponseMessage.json");

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.Error;

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 2;

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedMessageInterpretation()
		{
			return new ZString(
				@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Message was technically rejected by customs</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""left""><td colspan=""6"">MRN# MRN099999999</td></tr><tr align=""center""><td width=""75px"">Line Number</td><td width=""75px"">Column Number</td><td width=""75px"">Pointer</td><td width=""100px"">Error Code</td><td width=""135px"">Error Text</td><td width=""100px"">Original Attribute Value</td></tr><tr><td>0</td><td>0</td><td>error/points.here</td><td>57</td><td>errorText</td><td>originalAttributeValue</td></tr></table></p>");
		}

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef()
		{
			return new ZString(
				@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Status: </strong>Message was technically rejected by customs</p><p><strong>Functional Errors: </strong><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr align=""left""><td colspan=""6"">MRN# N/A</td></tr><tr align=""center""><td width=""75px"">Line Number</td><td width=""75px"">Column Number</td><td width=""75px"">Pointer</td><td width=""100px"">Error Code</td><td width=""135px"">Error Text</td><td width=""100px"">Original Attribute Value</td></tr><tr><td>0</td><td>0</td><td>error/points.here</td><td>57</td><td>errorText</td><td>originalAttributeValue</td></tr></table></p>");
		}

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.TechnicalRejection;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ResponseMessage.json");

		protected override ZString GetMessageTextForFees() => ZString.Empty;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ResponseMessageWithoutCRNAndMRN.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ResponseMessageWithoutImportOperation.json");

		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => ZString.Empty;

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE917ResponseMessageForMissingField.json");
	}
}
