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
	[TestedType(typeof(FTAAmendmentDetails))]
	sealed class FTAAmendmentDetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new FTAAmendmentDetails(new ImportFTAAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>()), Factory, ZString.Empty, entry.PK, ZString.Empty, null);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFTAAmendmentDetailsBy105MessageData()
		{
			var amendmentDetails = entry.SubsequentMessageDetails.FTAAmendments[0];
			AssertEquals("신청사유", amendmentDetails.AmendmentReason);
			AssertEquals("UXX", amendmentDetails.AmendmentType);
			AssertEquals(2, amendmentDetails.SequenceNo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFTAAmendmentDetailsBy106MessageData()
		{
			var message106 = entry.Messages.AddNew();
			message106.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message106.EM_MessageType = ElectronicDocumentTypeList.Codes._106;
			message106.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message106.EM_ApplicationReference = message105.EM_MessageNum;
			message106.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;

			var fileReader = new TestFileReader(typeof(FTAAmendmentDetailsTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, "GOVCBR106_ANT.xml");
			message106.EM_MessageData = testMsgFile;
			Factory.Save();

			var amendmentDetails = entry.SubsequentMessageDetails.FTAAmendments[0];
			AssertEquals(new ZDateTime("2014-05-06"), amendmentDetails.DecisionDate);
			AssertEquals("승인통보", amendmentDetails.NoticeTypeDescription);
		}

		public void TestFTAAmendmentDetailsByDB()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_FTARelationArticleCode = "3";

			entry.CH_CEI_Instruction = instruction.PK;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SC);
			entryNum.CE_IssueDate = new ZDateTime("2024-01-01");
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;

			var amendmentDetails = entry.SubsequentMessageDetails;
			AssertEquals("AAC", amendmentDetails.FTAMessageStatus);
			AssertEquals(new ZDateTime("2024-01-01"), amendmentDetails.FTAAcceptedDate);
			AssertEquals("법 제9조제2항", amendmentDetails.LawCodeDescription);
		}

		public void TestMessageStatus()
		{
			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_ApplicationReference = message105.EM_MessageNum;
			messageR99.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR99.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;

			var amendDetail = entry.SubsequentMessageDetails.FTAAmendments[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendDetail.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.AmendmentAccepted, amendDetail.MessageStatusDescription);

			var factory = new BusinessObjectFactory();
			entry = factory.Load<CusEntryHeader>(entry.PK);
			message105 = factory.Load<EDIMessage>(message105.PK);

			messageR99 = entry.Messages.AddNew();
			messageR99.EM_ApplicationReference = message105.EM_MessageNum;
			messageR99.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR99.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;

			var message106 = entry.Messages.AddNew();
			message106.EM_ApplicationReference = message105.EM_MessageNum;
			message106.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message106.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message106.EM_MessageType = ElectronicDocumentTypeList.Codes._106;
			message106.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;

			amendDetail = entry.SubsequentMessageDetails.FTAAmendments[0];
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendDetail.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.AmendmentAccepted, amendDetail.MessageStatusDescription);
			AssertEquals(CustomsEntryStatusTypeList.Descriptions.ANT, amendDetail.NoticeTypeDescription);
		}
		public void TestFTAAmendmentDetailsResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(FTAAmendmentDetails), nameof(FTAAmendmentDetails.DecisionDate), false, attribute => attribute.Caption == "Review Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(FTAAmendmentDetails), nameof(FTAAmendmentDetails.NoticeTypeDescription), false, attribute => attribute.Caption == "Review Result Description");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(FTAAmendmentDetails), nameof(FTAAmendmentDetails.NoticeTypeDescription), false, attribute => attribute.ShortCaption == "Review Result Desc.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(FTAAmendmentDetails), nameof(FTAAmendmentDetails.SequenceNo), false, attribute => attribute.Caption == "Version No.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SC);
			entryNum.CE_EntryLineReference = "1";

			message105 = entry.Messages.AddNew();
			message105.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message105.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message105.EM_MessageType = ElectronicDocumentTypeList.Codes._105;
			message105.EM_SystemCreateUser = "ORG";
			message105.EM_ApplicationReference = "2";
			var fileReader = new TestFileReader(typeof(FTAAmendmentDetailsTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestOutGoingFilesPath, "GOVCBR105.xml");
			message105.EM_MessageData = testMsgFile;
			Factory.Save();
		}
		CusEntryHeader entry;
		EDIMessage message105;

		const string TestOutGoingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
		const string TestImcomingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
