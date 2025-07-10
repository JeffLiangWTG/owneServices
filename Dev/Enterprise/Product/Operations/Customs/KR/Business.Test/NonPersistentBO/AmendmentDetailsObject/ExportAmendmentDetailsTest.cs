using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportAmendmentDetails))]
	sealed class ExportAmendmentDetailsTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportAmendmentDetailsWhenReceiveR20Message()
		{
			entry.EntryNumber = "6N00220000052X";
			entry.CusEntryNumber.CE_EntryType = "EXP";

			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = "R20";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = message5AS.EM_MessageNum;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Common\Incoming\GOVCBRR20_5AS.xml"));
			incomingMessage.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));

			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry.Reload();
			incomingMessage.Reload();
			var amendmentDetails = entry.ExportAmendmentDetailsCollection[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, amendmentDetails.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, amendmentDetails.MessageOrEntryStatus);
			EDIMessage lastOutgoingMessage = (EDIMessage)entry.Messages.LastOutgoingMessage;
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, lastOutgoingMessage.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentRejected, lastOutgoingMessage.MessageOrEntryStatus);
			AssertEquals(true, amendmentDetails.IsRejected);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportAmendmentDetailsWhenReceive5AFMessage()
		{
			entry.EntryNumber = "6N00220000051X";
			entry.CusEntryNumber.CE_EntryType = "EXP";

			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = "5AF";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = message5AS.EM_MessageNum;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Incoming\GOVCBR5AF_5AS.xml"));
			incomingMessage.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));

			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry.Reload();
			incomingMessage.Reload();
			var amendmentDetails = entry.ExportAmendmentDetailsCollection[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendmentDetails.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendmentDetails.MessageOrEntryStatus);
			EDIMessage lastOutgoingMessage = (EDIMessage)entry.Messages.LastOutgoingMessage;
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, lastOutgoingMessage.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, lastOutgoingMessage.MessageOrEntryStatus);
			AssertEquals(false, amendmentDetails.IsRejected);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportAmendmentDetailsWhenReceive5DTMessage()
		{
			entry.EntryNumber = "6N00220000051X";
			entry.CusEntryNumber.CE_EntryType = "EXP";

			var incomingMessage5AF = entry.Messages.AddNew();
			incomingMessage5AF.EM_MessageType = "5AF";
			incomingMessage5AF.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage5AF.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage5AF.EM_ApplicationReference = message5AS.EM_MessageNum;
			var fileStream5AF = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Incoming\GOVCBR5AF_5AS.xml"));
			incomingMessage5AF.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream5AF));

			var incomingMessage5DT = entry.Messages.AddNew();
			incomingMessage5DT.EM_MessageType = "5DT";
			incomingMessage5DT.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage5DT.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage5DT.EM_ApplicationReference = message5AS.EM_MessageNum;
			var fileStream5DT = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Incoming\GOVCBR5DT_Status_ANT.xml"));
			incomingMessage5DT.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream5DT));

			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			entry.Reload();
			incomingMessage5AF.Reload();
			incomingMessage5DT.Reload();
			var amendmentDetails = entry.ExportAmendmentDetailsCollection[0];
			EDIMessage lastOutgoingMessage = (EDIMessage)entry.Messages.LastOutgoingMessage;
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, lastOutgoingMessage.MessageStatus);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, lastOutgoingMessage.MessageOrEntryStatus);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, incomingMessage5DT.EM_MessageOwner);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendmentDetails.MessageStatus);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, amendmentDetails.MessageOrEntryStatus);
			AssertEquals(CustomsEntryStatusTypeList.Descriptions.ANT, amendmentDetails.StatusDescription);
			AssertEquals(ExportNotificationTypeList.Codes._05, amendmentDetails.NoticeType);
			AssertEquals(ExportNotificationTypeList.Descriptions._05, amendmentDetails.NoticeDescription);
			AssertEquals(false, amendmentDetails.IsRejected);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportAmendmentDetailsBy5ASMessageData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "040", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			entry.EntryNumber = "1234520100523X";
			entry.CusEntryNumber.CE_EntryType = "EXP";

			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Outgoing\GOVCBR5AS_Test.xml"));
			message5AS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));

			Factory.Save();
			var amendmentDetails = entry.ExportAmendmentDetailsCollection[0];
			AssertEquals(new ZDateTime("2021-02-02"), amendmentDetails.SubmissionDate);
			AssertEquals("11", amendmentDetails.ReasonCode);
			AssertEquals("D", amendmentDetails.FaultParty);
			AssertEquals("C", amendmentDetails.AmendmentType);
			AssertEquals("기간연장", amendmentDetails.AmendmentTypeDescription);
			AssertEquals("화주업무 오류", amendmentDetails.AmendReasonDescription);

			AssertEquals("12345-20-100523X", amendmentDetails.FormattedExportDeclarationNumber);
			AssertEquals("040", amendmentDetails.DeclarationCustomsOffice);
			AssertEquals("인천세관", amendmentDetails.DeclarationCustomsOfficeDescription);
			AssertEquals("15", amendmentDetails.DeclarationCustomsDivision);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportAmendmentDetailsBy5DTMessageData()
		{
			entry.EntryNumber = "000000000000000";
			entry.CusEntryNumber.CE_EntryType = "EXP";

			var message5DT = entry.Messages.AddNew();
			message5DT.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5DT.EM_MessageType = ElectronicDocumentTypeList.Codes._5DT;
			message5DT.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5DT.EM_ApplicationReference = message5AS.EM_MessageNum;
			message5DT.EM_LinkedObject = entry;
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Incoming\GOVCBR5DT_Test.xml"));
			message5DT.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			Factory.Save();

			var amendmentDetails = entry.ExportAmendmentDetailsCollection[0];
			AssertEquals(new ZDateTime("2014-05-06"), amendmentDetails.DecisionDate);
			AssertEquals("00000000000000", amendmentDetails.ApprovalNo);
			AssertEquals("AVC010", amendmentDetails.CustomsOfficerID);
			AssertEquals("담당자명", amendmentDetails.CustomsOfficerName);
		}

		public void TestMessageStatusAndMessageOrEntryStatus()
		{
			var message5AF = entry.Messages.AddNew();
			message5AF.EM_ApplicationReference = message5AS.EM_MessageNum;
			message5AF.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5AF.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5AF.EM_MessageType = "5AF";

			var exportAmendDetails = entry.ExportAmendmentDetailsCollection[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, exportAmendDetails.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, exportAmendDetails.MessageOrEntryStatus);

			var factory = new BusinessObjectFactory();
			entry = factory.Load<CusEntryHeader>(entry.PK);
			message5AS = factory.Load<EDIMessage>(message5AS.PK);

			message5AF = entry.Messages.AddNew();
			message5AF.EM_ApplicationReference = message5AS.EM_MessageNum;
			message5AF.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5AF.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5AF.EM_MessageType = "5AF";

			var message5DT = entry.Messages.AddNew();
			message5DT.EM_ApplicationReference = message5AS.EM_MessageNum;
			message5DT.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5DT.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5DT.EM_MessageType = "5DT";
			message5DT.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;

			exportAmendDetails = entry.ExportAmendmentDetailsCollection[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, exportAmendDetails.MessageStatus);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, exportAmendDetails.MessageOrEntryStatus);
			AssertEquals(CustomsEntryStatusTypeList.Descriptions.ANT, exportAmendDetails.StatusDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportAmendmentItems()
		{
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Outgoing\GOVCBR5AS_Amend.xml"));
			message5AS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			Factory.Save();

			var amendmentDetails = entry.ExportAmendmentDetailsCollection[0];
			var headerAmendedItems = amendmentDetails.HeaderAmendedItems;
			AssertEquals(4, headerAmendedItems.Count);
			AssertAmendItems(headerAmendedItems[0], ZString.Empty, "A704", "000", "00", "52349", "52420");
			AssertAmendItems(headerAmendedItems[1], ZString.Empty, "A706", "000", "00", "48473.2", "48575.26");
			AssertAmendItems(headerAmendedItems[2], ZString.Empty, "AA02", "000", "00", "16960.5", "17136");
			AssertAmendItems(headerAmendedItems[3], ZString.Empty, "AA04", "000", "00", "4160", "4192");

			var lineAmendedItems = amendmentDetails.LineAmendedItems;
			AssertEquals(29, lineAmendedItems.Count);
			AssertAmendItems(lineAmendedItems[0], "03", "B201", "006", "00", "2044", "2054");
			AssertAmendItems(lineAmendedItems[1], "03", "B201", "008", "00", "1842.8", "1991.6");
			AssertAmendItems(lineAmendedItems[2], "03", "B301", "001", "00", "688209", "688233");
			AssertAmendItems(lineAmendedItems[3], "03", "B301", "002", "00", "1137319", "1137353");
			AssertAmendItems(lineAmendedItems[4], "03", "B301", "003", "00", "13857781", "13858209");
			AssertAmendItems(lineAmendedItems[5], "03", "B301", "004", "00", "2642077", "2642159");
			AssertAmendItems(lineAmendedItems[6], "03", "B301", "005", "00", "1644737", "1644788");
			AssertAmendItems(lineAmendedItems[7], "03", "B301", "006", "00", "3637084", "3652012");
			AssertAmendItems(lineAmendedItems[8], "03", "B301", "007", "00", "12663305", "12663697");
			AssertAmendItems(lineAmendedItems[9], "03", "B301", "008", "00", "11956419", "12060959");
			AssertAmendItems(lineAmendedItems[10], "03", "B301", "009", "00", "4732409", "4732556");
			AssertAmendItems(lineAmendedItems[11], "03", "B301", "010", "00", "682392", "682412");
			AssertAmendItems(lineAmendedItems[12], "03", "B301", "011", "00", "426932", "426946");
			AssertAmendItems(lineAmendedItems[13], "03", "B301", "012", "00", "755879", "755902");
			AssertAmendItems(lineAmendedItems[14], "03", "B301", "013", "00", "1718459", "1718582");
			AssertAmendItems(lineAmendedItems[15], "03", "B601", "006", "00", "240", "241");
			AssertAmendItems(lineAmendedItems[16], "03", "B601", "008", "00", "2300", "2331");
			AssertAmendItems(lineAmendedItems[17], "01", "C101", "008", "07", "", "KIMCHI RAMEN (120G X 5)X8/CTN");
			AssertAmendItems(lineAmendedItems[18], "03", "C101", "013", "02", "OCEAN FREIGHT : USD 688.00INSURANCE PREMIUM : USD 44.20", "OCEAN FREIGHT : USD 688.00INSURANCE PREMIUM : USD 44.26");
			AssertAmendItems(lineAmendedItems[19], "03", "C201", "006", "01", "30", "31");
			AssertAmendItems(lineAmendedItems[20], "03", "C201", "008", "01", "50", "40");
			AssertAmendItems(lineAmendedItems[21], "01", "C201", "008", "07", "", "41");
			AssertAmendItems(lineAmendedItems[22], "01", "C202", "008", "07", "", "CT");
			AssertAmendItems(lineAmendedItems[23], "01", "C203", "008", "07", "", "10.3");
			AssertAmendItems(lineAmendedItems[24], "03", "C203", "013", "02", "732.2", "732.26");
			AssertAmendItems(lineAmendedItems[25], "03", "C204", "006", "01", "381", "393.7");
			AssertAmendItems(lineAmendedItems[26], "03", "C204", "008", "01", "1665", "1332");
			AssertAmendItems(lineAmendedItems[27], "01", "C204", "008", "07", "", "422.3");
			AssertAmendItems(lineAmendedItems[28], "03", "C204", "013", "02", "732.2", "732.26");

			var allAmendItems = amendmentDetails.AllAmendedItems;
			AssertEquals(33, allAmendItems.Count);

			AssertAmendItems(allAmendItems[0], ZString.Empty, "A704", "000", "00", "52349", "52420");
			AssertAmendItems(allAmendItems[3], ZString.Empty, "AA04", "000", "00", "4160", "4192");
			AssertAmendItems(allAmendItems[4], "03", "B201", "006", "00", "2044", "2054");
			AssertAmendItems(allAmendItems[32], "03", "C204", "013", "02", "732.2", "732.26");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIDs()
		{
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Outgoing\GOVCBR5AS_Add.xml"));
			message5AS.SetEM_MessageTextOrDataSource(new CargoWise.IO.Shim.SubStreamableStream(fileStream));
			Factory.Save();

			var amendmentDetails = entry.ExportAmendmentDetailsCollection[0];
			var lineAmendedItems = amendmentDetails.LineAmendedItems;

			AssertEquals("Entry Line : 001", lineAmendedItems[0].ID);
			AssertEquals("Entry Line : 001", lineAmendedItems[1].ID);
			AssertEquals("Container : 1", lineAmendedItems[2].ID);
			AssertEquals("Entry Line : 001, Vehicle No. : 9999999999", lineAmendedItems[3].ID);
			AssertEquals("Entry Line : 001, Req. Doc. : 01", lineAmendedItems[4].ID);
		}

		public void TestExportAmendmentDetailsResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ExportAmendmentDetails), nameof(ExportAmendmentDetails.MessageStatus), false, attribute => attribute.Caption == "Message Status");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ExportAmendmentDetails), nameof(ExportAmendmentDetails.NoticeType), false, attribute => attribute.Caption == "Review Result");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ExportAmendmentDetails), nameof(ExportAmendmentDetails.NoticeDescription), false, attribute => attribute.Caption == "Review Result Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ExportAmendmentDetails), nameof(ExportAmendmentDetails.DecisionDate), false, attribute => attribute.Caption == "Review Date");
		}
		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_VersionID = 1;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ORG";
			staff.GS_LoginName = "Origin";
			staff.GS_EmailAddress = "OriginalSender@wisetechglobal.com";
			message5AS = entry.Messages.AddNew();
			message5AS.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5AS.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5AS.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
			message5AS.EM_SystemCreateUser = "ORG";
			message5AS.EM_ApplicationReference = "2";
			var fileReader = new TestFileReader(typeof(ExportAmendmentDetailsTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestFilesPath, "GOVCBR5AS_Empty.xml");
			message5AS.EM_MessageData = testMsgFile;
			Factory.Save();
		}
		CusEntryHeader entry;
		EDIMessage message5AS;
		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing";

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new ExportAmendmentDetails(new Export5ASHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>()), Factory, entry.PK);
		}

		void AssertAmendItems(Export5ASItemWrapper amendmentDetail, string changeReasonCode, string amendID, string entryLineNo, string lineDetailNo, string beforeValue, string afterValue)
		{
			AssertEquals(changeReasonCode, amendmentDetail.AmendmentItem.LineAmendType);
			AssertEquals(amendID, amendmentDetail.AmendmentItem.AmendDataItemID);
			AssertEquals(entryLineNo, amendmentDetail.AmendmentItem.EntryLineNo);
			AssertEquals(lineDetailNo, amendmentDetail.AmendmentItem.LineDetailNo);
			AssertEquals(beforeValue, amendmentDetail.AmendmentItem.BeforeDescription);
			AssertEquals(afterValue, amendmentDetail.AmendmentItem.AfterDescription);
		}
	}
}
