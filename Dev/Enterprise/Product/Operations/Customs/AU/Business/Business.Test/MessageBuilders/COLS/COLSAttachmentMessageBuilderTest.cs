using System.Linq;
using System.Text;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSAttachmentMessageBuilderTest : COLSMessageBuilderTest
	{
		public override void TestCreateNewMessage()
		{
			var fileContent = Encoding.UTF8.GetBytes("Test Document Content for Declaration");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var doc = declaration.DocManagerInfo.AddFileOrDocument(fileContent, "File.txt", "txt");
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "XXX456789YYYYMMDDSS9999999";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var entryNumObject = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_EntryNum = "LRN8888888";
			entryNumObject.CE_Category = "CUS";

			var cusStorageDocPivot = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot.CSD_DocType = "INVOICE";
			cusStorageDocPivot.CSD_Description = "XYZ123";
			cusStorageDocPivot.CSD_StorageDocReference = doc.UniqueKey;
			var messageBuilder = new COLSAttachmentMessageBuilder(colsHeader, cusStorageDocPivot, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: true);
			var message = messageBuilder.CreateNewMessage();

			CombineAssertions(() =>
			{
				AssertEquals("Message type", AUCOLSMessageTypeList.Codes.AddAttachment, message.EM_MessageType);
				AssertEquals("Message EM_LinkUniqueID", cusStorageDocPivot.PK, message.EM_LinkUniqueID);
				AssertEquals("Message EM_LinkTable", cusStorageDocPivot.TableName, message.EM_LinkTable);
				AssertEquals("Message EM_ApplicationReference", "LRN8888888", message.EM_ApplicationReference);
				AssertEquals("Message EM_MessageText", "{\"file\":\"File.txt\",\"docType\":\"INVOICE\",\"docReference\":\"XYZ123\",\"lastDoc\":true}", message.EM_MessageText);
			});
		}

		public void TestAdditionalProcessing_EM_Status() => CombineAssertions(() =>
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cusEntryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
			var colsHeader1 = Factory.New<QuarantineColsHeader>();
			colsHeader1.QCH_CH_CusEntryHeader = cusEntryHeader1.PK;
			var cusStorageDocPivot1 = colsHeader1.EDocPivotCollection.AddNew();
			var messageBuilder = new COLSAttachmentMessageBuilder(colsHeader1, cusStorageDocPivot1, isPartOfOtherMessage: true, isFirstAttachment: true, isLastAttachment: true);
			var message = messageBuilder.CreateNewMessage();
			AssertEquals("EM_Status = 'PND' when isPartOfOtherMessage = True", "PND", message.EM_Status);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cusEntryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
			var colsHeader2 = Factory.New<QuarantineColsHeader>();
			colsHeader2.QCH_CH_CusEntryHeader = cusEntryHeader2.PK;
			var cusStorageDocPivot2 = colsHeader2.EDocPivotCollection.AddNew();
			messageBuilder = new COLSAttachmentMessageBuilder(colsHeader2, cusStorageDocPivot2, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: true);
			message = messageBuilder.CreateNewMessage();
			AssertEquals("EM_Status = 'QUE' when isPartOfOtherMessage = False and isFirstAttachment = True", "QUE", message.EM_Status);

			messageBuilder = new COLSAttachmentMessageBuilder(colsHeader2, cusStorageDocPivot2, isPartOfOtherMessage: false, isFirstAttachment: false, isLastAttachment: true);
			message = messageBuilder.CreateNewMessage();
			AssertEquals("EM_Status = 'PND' when isPartOfOtherMessage = False and isFirstAttachment = False", "PND", message.EM_Status);
		});

		public void TestAdditionalProcessing_EM_MessageSubType() => CombineAssertions(() =>
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var cusStorageDocPivot = colsHeader.EDocPivotCollection.AddNew();
			var messageBuilder = new COLSAttachmentMessageBuilder(colsHeader, cusStorageDocPivot, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: true);
			var message = messageBuilder.CreateNewMessage();
			AssertEquals("EM_MessageSubType = 'ATT' for last attachment", "LST", message.EM_MessageSubType);

			messageBuilder = new COLSAttachmentMessageBuilder(colsHeader, cusStorageDocPivot, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: false);
			message = messageBuilder.CreateNewMessage();
			AssertEquals("EM_MessageSubType = 'ATT' for non-last attachment", "ATT", message.EM_MessageSubType);
		});

		public void TestAdditionalProcessing_CSD_MessageStatus() => CombineAssertions(() =>
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var cusStorageDocPivot = colsHeader.EDocPivotCollection.AddNew();
			var messageBuilder = new COLSAttachmentMessageBuilder(colsHeader, cusStorageDocPivot, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: true);
			_ = messageBuilder.CreateNewMessage();
			AssertEquals("CSD_MessageStatus = 'ALS' for last attachment", "ALS", cusStorageDocPivot.CSD_MessageStatus);

			messageBuilder = new COLSAttachmentMessageBuilder(colsHeader, cusStorageDocPivot, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: false);
			_ = messageBuilder.CreateNewMessage();
			AssertEquals("CSD_MessageStatus = 'ADS' for non-last attachment", "ADS", cusStorageDocPivot.CSD_MessageStatus);
		});

		public void TestAdditionalProcessing_MessageAttachments() => CombineAssertions(() =>
		{
			var fileContent = Encoding.UTF8.GetBytes("Test Document Content for Declaration");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var doc = declaration.DocManagerInfo.AddFileOrDocument(fileContent, "File.txt", "txt");
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var cusStorageDocPivot = colsHeader.EDocPivotCollection.AddNew();
			cusStorageDocPivot.CSD_DocType = "INVOICE";
			cusStorageDocPivot.CSD_Description = "XYZ123";
			cusStorageDocPivot.CSD_StorageDocReference = doc.UniqueKey;
			var messageBuilder = new COLSAttachmentMessageBuilder(colsHeader, cusStorageDocPivot, isPartOfOtherMessage: false, isFirstAttachment: true, isLastAttachment: true);
			AssertEquals("Prerequisite: initially colsHeader.Messages is empty", 0, colsHeader.Messages.Count);
			var message = messageBuilder.CreateNewMessage();
			var attachment = (EDIMessageAttach)message.MessageAttachments.Single();
			AssertEquals("EG_FileName", "File.txt", attachment.EG_FileName);
			AssertEquals("EG_StorageDocsGuid", doc.UniqueKey, attachment.EG_StorageDocsGuid);
		});
	}
}
