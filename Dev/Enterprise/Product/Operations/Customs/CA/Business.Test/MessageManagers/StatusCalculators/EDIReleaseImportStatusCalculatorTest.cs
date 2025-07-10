using CargoWise.Types;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(EDIReleaseImportStatusCalculator))]
	sealed class EDIReleaseImportStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public void TestIsShipmentCleared()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_MessageType = Business.MessageTypeList.Codes.EDIRelease;
			var calculator = new EDIReleaseImportStatusCalculator();
			header.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver;
			Assert("IsShipmentCleared", EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
			header.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			Assert("IsShipmentCleared", EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
			header.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired;
			Assert("IsShipmentCleared", EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
			header.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.ManualRelease;
			Assert("IsShipmentCleared", EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
			header.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.Cancelled;
			Assert("IsShipmentCleared", !EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
			header.CH_EntryStatus = EDIReleaseImportEntryStatusList.Codes.MultipleCargoControlNumber;
			var helper = new DeclarationTestHelper(Factory, true);
			var releaseStatus = new ReleaseStatus((EDIReleaseMessage)helper.GetEDIReleaseResponseMessage("37132536987", "1"));
			declaration.ReleaseStatuses.Add(releaseStatus);
			Assert("IsShipmentCleared", EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "ccn";
			declaration.ReleaseStatuses.Load();
			Assert("IsShipmentCleared", !EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
			declaration.JE_MessageType = Business.JobMessageTypeList.Codes.LowValueShipments;
			Assert("IsShipmentCleared", EDIReleaseImportStatusCalculator.IsShipmentCleared(declaration));
		}

		public void TestMainEntryStatusCodesAreSame()
		{
			AssertEquals("Clear", EntryStatusList.Codes.Clear, EDIReleaseImportEntryStatusList.Codes.GoodsReleased);
			AssertEquals("Error", EntryStatusList.Codes.Error, EDIReleaseImportEntryStatusList.Codes.Error);
			AssertEquals("Cancelled", EntryStatusList.Codes.Cancelled, EDIReleaseImportEntryStatusList.Codes.Cancelled);
		}

		#region TestCalculatedJobStatus

		public override void TestCalculatedJobStatus()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var header = jobDeclaration.CustomsEntryHeaders.AddNew();
			AssertEquals("No status", ZString.Empty, calculator.CalculatedJobStatus(header));

			const string messageWithSyntaxError = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:200902131330:203'GIS+14'ERP+2:237:28'UNT+5+1'";
			var ediMessage = AddEDIMessage(header, "50", ZDateTime.Now);
			ediMessage.EM_MessageText = messageWithSyntaxError;
			AssertEquals("SyntaxError", EDIReleaseImportEntryStatusList.Codes.SyntaxError, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "60", ProcessingIndicatorCodedList.ErrorMessage);
			AssertEquals("Error", EDIReleaseImportEntryStatusList.Codes.Error, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "70", ProcessingIndicatorCodedList.MessageContentAccepted);
			AssertEquals("MessageContentAccepted", EDIReleaseImportEntryStatusList.Codes.MessageContentAccepted, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "100", ProcessingIndicatorCodedList.GoodsReleased);
			AssertEquals("GoodsReleased", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "110", ProcessingIndicatorCodedList.GoodsRequiredForExamination);
			AssertEquals("GoodsRequiredForExamination", EDIReleaseImportEntryStatusList.Codes.GoodsRequiredForExamination, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "120", ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced);
			AssertEquals("Y51ReleaseDocumentsRequired", EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "130", ProcessingIndicatorCodedList.GoodsDetained);
			AssertEquals("GoodsDetained", EDIReleaseImportEntryStatusList.Codes.GoodsDetained, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "140", ProcessingIndicatorCodedList.GoodsMayMoveUnderCustomsTransfer);
			AssertEquals("GoodsMayMove", EDIReleaseImportEntryStatusList.Codes.GoodsMayMove, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "150", ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival);
			AssertEquals("DeclarationAccepted", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "160", ProcessingIndicatorCodedList.Transit);
			AssertEquals("Transit", EDIReleaseImportEntryStatusList.Codes.Transit, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "170", ProcessingIndicatorCodedList.Import);
			AssertEquals("AuthorisedToDeliver", EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "180", ProcessingIndicatorCodedList.TransactionAwaitingProcessing);
			AssertEquals("AwaitingCustomsProcessing", EDIReleaseImportEntryStatusList.Codes.AwaitingCustomsProcessing, calculator.CalculatedJobStatus(header));

			ediMessage = AddEDIMessage(header, "190", ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival);
			ediMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			AssertEquals("Cancelled", EDIReleaseImportEntryStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(header));
		}

		public void TestCalculatedJobStatusWithMultipleCCNs()
		{
			const string ccn1 = "CCN1";
			const string ccn2 = "CCN2";
			var jobDeclaration = Factory.New<JobDeclaration>();
			var header = jobDeclaration.CustomsEntryHeaders.AddNew();
			header.CH_MessageType = Business.MessageTypeList.Codes.EDIRelease;
			jobDeclaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = ccn1;
			jobDeclaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = ccn2;
			AssertEquals("No status", ZString.Empty, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "70", ProcessingIndicatorCodedList.MessageContentAccepted, ccn1);
			AddEDIMessage(header, "80", ProcessingIndicatorCodedList.MessageContentAccepted, ccn2);
			jobDeclaration.ReleaseStatuses.Load();
			AssertEquals("MessageContentAccepted", EDIReleaseImportEntryStatusList.Codes.MessageContentAccepted, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "90", ProcessingIndicatorCodedList.GoodsReleased, ZDateTime.Now.AddDays(1), ccn1);
			AddEDIMessage(header, "100", ProcessingIndicatorCodedList.GoodsRequiredForExamination, ZDateTime.Now.AddDays(1), ccn2);
			jobDeclaration.ReleaseStatuses.Load();
			AssertEquals("MultipleCargoControlNumber", EDIReleaseImportEntryStatusList.Codes.MultipleCargoControlNumber, calculator.CalculatedJobStatus(header));

			AddEDIMessage(header, "110", ProcessingIndicatorCodedList.GoodsReleased, ZDateTime.Now.AddDays(3), ccn2);
			jobDeclaration.ReleaseStatuses.Load();
			AssertEquals("GoodsReleased", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, calculator.CalculatedJobStatus(header));

			var ediMessage = AddEDIMessage(header, "120", ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival, ZDateTime.Now.AddDays(4));
			ediMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			jobDeclaration.ReleaseStatuses.Load();
			AssertEquals("Cancelled", EDIReleaseImportEntryStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(header));
		}

		public void TestGetMessageSubTypeCore()
		{
			var messageStatus = MessageStatusList.Codes.AcknowledgedDelete;
			var processingIndicator = ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival;
			var calculator = new EDIReleaseImportStatusCalculator();
			var messageSubType = calculator.GetMessageSubType(messageStatus, processingIndicator);
			AssertEquals("messageSubType should be CNL", "CNL", messageSubType);

			messageStatus = MessageStatusList.Codes.ErrorDelete;
			processingIndicator = ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival;
			messageSubType = calculator.GetMessageSubType(messageStatus, processingIndicator);
			AssertEquals("messageSubType should be ORG", "ORG", messageSubType);
		}

		EDIMessage AddEDIMessage(CusEntryHeader header, string messageNum, ProcessingIndicatorCodedList indicator, string ccn = "")
		{
			return AddEDIMessage(header, messageNum, indicator, ZDateTime.Now, ccn);
		}

		EDIMessage AddEDIMessage(CusEntryHeader header, string messageNum, ProcessingIndicatorCodedList indicator, ZDateTime createTime, string ccn = "")
		{
			var result = AddEDIMessage(header, messageNum, createTime);
			result.EM_MessageText = string.Format(@"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:200902131330:203'GIS+{0}'RFF+XC:{1}'UNT+5+1'", indicator, ccn);
			result.SetSystemDefinedValue(EDIMessage.Schema.CargoControlNumber, new ZString(ccn));
			return result;
		}

		EDIMessage AddEDIMessage(CusEntryHeader header, string messageNum, ZDateTime createTime)
		{
			var result = header.Messages.AddNew(typeof(EDIReleaseMessage));
			result.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			result.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			result.EM_MessageNum = messageNum;
			result.EM_SystemCreateTimeUtc = createTime;
			return (EDIMessage)result;
		}

		#endregion

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", Business.MessageTypeList.Descriptions.EDIRelease, calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new EDIReleaseImportStatusCalculator();
		}
	}
}
