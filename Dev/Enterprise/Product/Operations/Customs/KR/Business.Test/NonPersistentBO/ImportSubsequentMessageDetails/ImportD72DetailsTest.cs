using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportD72Details))]
	public class ImportD72DetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportD72Details(Factory, Factory.New<EDIMessage>());
		}

		public void TestFullData()
		{
			var details = new ImportD72Details(Factory, messageD72);
			AssertEquals(1, details.SequenceNo);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, details.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, details.MessageStatusDescription);
			AssertEquals(new ZDateTime("2024-01-01"), details.AcceptedDate);
			AssertEquals(new ZDateTime("2022-01-12"), details.DecisionDate);
			AssertEquals("승인통보", details.NoticeTypeDescription);
			AssertEquals(new ZDateTime("2021-03-31"), details.BeforeReExportScheduledDate);
			AssertEquals(new ZDateTime("2022-01-12"), details.AfterReExportScheduledDate);
			AssertEquals("주문 수집 일정 변동에 따른 재수출기한 연장 신청", details.ReasonDescription);
		}

		public void TestNoExistR43()
		{
			entry.Messages.RemoveAndDelete(messageR43);
			var details = new ImportD72Details(Factory, messageD72);
			AssertEquals(1, details.SequenceNo);
			AssertEquals("OAC", details.MessageStatus);
			AssertEquals("접수통보", details.MessageStatusDescription);
			AssertEquals(new ZDateTime("2024-01-01"), details.AcceptedDate);
			AssertEquals(ZDateTime.Empty, details.DecisionDate);
			AssertEquals(ZString.Empty, details.NoticeTypeDescription);
			AssertEquals(new ZDateTime("2021-03-31"), details.BeforeReExportScheduledDate);
			AssertEquals(new ZDateTime("2021-04-01"), details.AfterReExportScheduledDate);
			AssertEquals("주문 수집 일정 변동에 따른 재수출기한 연장 신청", details.ReasonDescription);
		}

		public void TestNoExistR43AndR99()
		{
			entry.Messages.RemoveAndDelete(messageR99);
			entry.Messages.RemoveAndDelete(messageR43);
			var details = new ImportD72Details(Factory, messageD72);
			AssertEquals(1, details.SequenceNo);
			AssertEquals(ZString.Empty, details.MessageStatus);
			AssertEquals(ZString.Empty, details.MessageStatusDescription);
			AssertEquals(ZDateTime.Empty, details.AcceptedDate);
			AssertEquals(ZDateTime.Empty, details.DecisionDate);
			AssertEquals(ZString.Empty, details.NoticeTypeDescription);
			AssertEquals(new ZDateTime("2021-03-31"), details.BeforeReExportScheduledDate);
			AssertEquals(new ZDateTime("2021-04-01"), details.AfterReExportScheduledDate);
			AssertEquals("주문 수집 일정 변동에 따른 재수출기한 연장 신청", details.ReasonDescription);
		}
		public void TestImportD72DetailsResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ImportD72Details), nameof(ImportD72Details.DecisionDate), false, attribute => attribute.Caption == "Review Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ImportD72Details), nameof(ImportD72Details.NoticeTypeDescription), false, attribute => attribute.Caption == "Review Result Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ImportD72Details), nameof(ImportD72Details.SequenceNo), false, attribute => attribute.Caption == "Version No.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			messageD72 = entry.Messages.AddNew();
			messageD72.EM_MessageNum = "1";
			messageD72.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			messageD72.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageD72.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			messageD72.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestOutGoingFilesPath, "GOVCBRD72_Result_D1.xml");
			messageD72.EM_MessageData = testMsgFile;

			messageR99 = entry.Messages.AddNew();
			messageR99.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR99.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_ApplicationReference = messageD72.EM_MessageNum;
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime("2024-01-01");
			testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, "GOVCBRR99_D72.xml");
			messageR99.EM_MessageData = testMsgFile;

			messageR43 = entry.Messages.AddNew();
			messageR43.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR43.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR43.EM_MessageType = ElectronicDocumentTypeList.Codes._R43;
			messageR43.EM_ApplicationReference = messageD72.EM_MessageNum;
			messageR43.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			messageR43.EM_MessageSubType = ElectronicDocumentTypeList.Codes._D72;
			testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, "GOVCBRR43_Result_C.xml");
			messageR43.EM_MessageData = testMsgFile;
			Factory.Save();
		}
		CusEntryHeader entry;
		EDIMessage messageD72;
		EDIMessage messageR99;
		EDIMessage messageR43;

		const string TestOutGoingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
		const string TestImcomingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
