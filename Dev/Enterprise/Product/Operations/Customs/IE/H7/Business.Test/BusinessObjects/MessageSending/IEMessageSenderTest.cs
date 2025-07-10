using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class IEMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendMessage_NewEDIMessageCreated()
		{
			AssertNewMessageCreated(AISOutgoingMessageTypeList.Codes.CustomsDeclaration);
			AssertNewMessageCreated(AISOutgoingMessageTypeList.Codes.CustomsDeclaration, isV2: false);
			AssertNewMessageCreated(AISOutgoingMessageTypeList.Codes.InvalidationRequest);
			AssertNewMessageCreated(AISOutgoingMessageTypeList.Codes.InvalidationRequest, isV2: false);
			AssertNewMessageCreated(AISOutgoingMessageTypeList.Codes.AmendmentRequest);
			AssertNewMessageCreated(AISOutgoingMessageTypeList.Codes.AmendmentRequest, isV2: false);

			void AssertNewMessageCreated(string messageType, bool isV2 = true)
			{
				var (company, branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
				var manifestHeader = Factory.New<AsycudaManifestHeader>();
				manifestHeader.AMA_ApplicationCode = isV2 ? ApplicationCodeTypeList.Codes.EuH7V2 : ApplicationCodeTypeList.Codes.EuH7V1;
				manifestHeader.AMA_GB = branch.PK;
				var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
				var bill = manifestHeader.Bills.AddNew();
				Assert("Pre-condition : Bill doesn't has EDIMessage on it", !bill.Messages.Any());

				var messageSendingObject = new MessageSendingObject(bill);
				messageSendingObject.Action = messageType;
				var sender = new IEMessageSender(messageSendingObject);
				var message = sender.Send();

				AssertCollectionContains("A new EDIMessage created for bill", message, bill.Messages);
				CombineAssertions($"message details {messageType} {manifestHeader.AMA_ApplicationCode}", () =>
				{
					AssertEquals("EM_GB", branch.PK, message.EM_GB);
					AssertEquals("EM_GP", companyCredential.PK, message.EM_GP);
					AssertEquals("Application Code", "IEI", message.EM_ApplicationCode);
					AssertEquals("Receive Transmit", "TRX", message.EM_ReceiveTransmit);
					AssertEquals("Message Type", messageType, message.EM_MessageType);
					AssertEquals("Status", "QUE", message.EM_Status);
					AssertNotNullOrEmpty("Text", message.EM_MessageText);
					AssertEquals("Bill Message Status", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
				});
			}
		}

		[TestDate(2024, 02, 04)]
		public void TestLRNGeneration_LV2()
		{
			AssertLRNGeneration(AISOutgoingMessageTypeList.Codes.CustomsDeclaration, generateNewLRN: true);
			AssertLRNGeneration(AISOutgoingMessageTypeList.Codes.InvalidationRequest, generateNewLRN: false);
			AssertLRNGeneration(AISOutgoingMessageTypeList.Codes.AmendmentRequest, generateNewLRN: false);
		}

		[TestDate(2024, 02, 04)]
		public void TestLRNGeneration_LV1()
		{
			AssertLRNGeneration(AISOutgoingMessageTypeList.Codes.CustomsDeclaration, generateNewLRN: true, isV2: false);
			AssertLRNGeneration(AISOutgoingMessageTypeList.Codes.InvalidationRequest, generateNewLRN: false, isV2: false);
			AssertLRNGeneration(AISOutgoingMessageTypeList.Codes.AmendmentRequest, generateNewLRN: false, isV2: false);
		}

		void AssertLRNGeneration(string messageType, bool generateNewLRN, bool isV2 = true)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = isV2 ? ApplicationCodeTypeList.Codes.EuH7V2 : ApplicationCodeTypeList.Codes.EuH7V1;
			var bill = manifestHeader.Bills.AddNew();
			bill.LocalReferenceNumber = "OriginalLRN";

			var messageSendingObject = new MessageSendingObject(bill);
			messageSendingObject.Action = messageType;
			var sender = new IEMessageSender(messageSendingObject);
			sender.Send();
			Factory.Save();

			if (generateNewLRN)
			{
				AssertEquals("EDIDATBNE2400000001V01", bill.LocalReferenceNumber);
			}
			else
			{
				AssertEquals("OriginalLRN", bill.LocalReferenceNumber);
			}
		}

		public void TestSendIM432_NewEDIMessageCreated()
		{
			var (company, branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_GB = branch.PK;
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var bill = manifestHeader.Bills.AddNew();
			Assert("Pre-condition : Bill doesn't has EDIMessage on it", !bill.Messages.Any());

			var messageSendingObject = new MessageSendingObject(bill);
			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.PresentationNotification;
			var sender = new IEMessageSender(messageSendingObject);
			var message = sender.Send();

			AssertCollectionContains("A new EDIMessage created for bill", message, bill.Messages);
			CombineAssertions("message details", () =>
			{
				AssertEquals("EM_GB", branch.PK, message.EM_GB);
				AssertEquals("EM_GP", companyCredential.PK, message.EM_GP);
				AssertEquals("Application Code", "IEI", message.EM_ApplicationCode);
				AssertEquals("Receive Transmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("Message Type", "432", message.EM_MessageType);
				AssertEquals("Status", "QUE", message.EM_Status);
				AssertEquals("Bill Message Status", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
			});
		}

		public void TestCreateV1IM432MessageBuilder()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = "LV1";
			var bill = manifestHeader.Bills.AddNew();
			var messageSendingObject = new MessageSendingObject(bill);
			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.PresentationNotification;
			var sender = new IEMessageSenderForTest(messageSendingObject);
			var builder = sender.CreateMessageBuilder_Exposed();
			AssertType<CargoWise.Customs.IE.MessageContracts.MessageBuilders.AIS.H7V1.IM432MessageBuilder>("Should use V1 builder when application code is LV1", builder);
		}

		public void TestSendRF415_NewEDIMessageCreated()
		{
			var (company, branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_GB = branch.PK;
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var bill = manifestHeader.Bills.AddNew();
			Assert("Pre-condition : Bill doesn't has EDIMessage on it", !bill.Messages.Any());

			var messageSendingObject = new RF415MessageSendingObject(bill);
			var sender = new IEMessageSender(messageSendingObject);
			var message = sender.Send();

			AssertCollectionContains("A new EDIMessage created for bill", message, bill.Messages);
			CombineAssertions("message details", () =>
			{
				AssertEquals("EM_GB", branch.PK, message.EM_GB);
				AssertEquals("EM_GP", companyCredential.PK, message.EM_GP);
				AssertEquals("Application Code", "IEI", message.EM_ApplicationCode);
				AssertEquals("Receive Transmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("Message Type", "F15", message.EM_MessageType);
				AssertEquals("Status", "QUE", message.EM_Status);
				AssertEquals("Bill Message Status", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
			});
		}

		public void TestSendIM483_NewEDIMessageCreated()
		{
			var (company, branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.EuH7V2;
			manifestHeader.AMA_GB = branch.PK;
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var bill = manifestHeader.Bills.AddNew();

			var eDoc1 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "Supp1.pdf", "CIV");
			var eDoc2 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "Supp2.pdf", "CIV");
			var eDoc3 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "Supp3.pdf", "CIV");

			var requestedDocument1 = bill.RequestedDocuments.AddNew();
			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;

			var requestedDocument2 = bill.RequestedDocuments.AddNew();
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;

			var uploadDocumentsAction = new UploadDocumentsSendingAction(bill);
			var supportingDocument1 = uploadDocumentsAction.AddInfoCollection[0].EDocsCollection.AddNew();
			supportingDocument1.EDoc = eDoc1.UniqueKey;
			var supportingDocument2 = uploadDocumentsAction.AddInfoCollection[0].EDocsCollection.AddNew();
			supportingDocument2.EDoc = eDoc2.UniqueKey;
			var supportingDocument3 = uploadDocumentsAction.AddInfoCollection[1].EDocsCollection.AddNew();
			supportingDocument3.EDoc = eDoc3.UniqueKey;

			var sender = new IEMessageSender(uploadDocumentsAction);
			var message = sender.Send();

			AssertCollectionContains("A new EDIMessage created for bill", message, bill.Messages);
			CombineAssertions("message details", () =>
			{
				AssertEquals("EM_GB", branch.PK, message.EM_GB);
				AssertEquals("EM_GP", companyCredential.PK, message.EM_GP);
				AssertEquals("Application Code", "IEI", message.EM_ApplicationCode);
				AssertEquals("Receive Transmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("Message Type", "483", message.EM_MessageType);
				AssertEquals("Status", "QUE", message.EM_Status);
				AssertEquals("Bill Message Status", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
				AssertContainsExactElementsInAnyOrder("MessageAttachmentContents", new[] { eDoc1.UniqueKey, eDoc2.UniqueKey, eDoc3.UniqueKey }, message.MessageAttachments.Select(x => x.EG_StorageDocsGuid));
			});
		}

		public void TestCreateMessageBuilder_IM483()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";

			var bill = header.Bills.AddNew();
			var uploadDocumentsAction = new UploadDocumentsSendingAction(bill);

			var sender = new IEMessageSenderForTest(uploadDocumentsAction);

			var builder = sender.CreateMessageBuilder_Exposed();
			AssertType<CargoWise.Customs.IE.MessageContracts.AIS.IM483MessageBuilder>("Should use V2 builder when application code is LV2", builder);

			header.AMA_ApplicationCode = "LV1";
			builder = sender.CreateMessageBuilder_Exposed();
			AssertType<CargoWise.Customs.IE.MessageContracts.MessageBuilders.AIS.H7V1.IM483MessageBuilder>("Should use V1 builder when application code is LV1", builder);
		}

		public void TestCreateMessageBuilder_RF415()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";

			var bill = header.Bills.AddNew();
			var uploadDocumentsAction = new RF415MessageSendingObject(bill);

			var sender = new IEMessageSenderForTest(uploadDocumentsAction);

			var builder = sender.CreateMessageBuilder_Exposed();
			AssertType<CargoWise.Customs.IE.MessageContracts.AIS.RF415MessageBuilder>("Should use V2 builder when application code is LV2", builder);

			header.AMA_ApplicationCode = "LV1";
			builder = sender.CreateMessageBuilder_Exposed();
			AssertType<CargoWise.Customs.IE.MessageContracts.MessageBuilders.AIS.H7V1.Rf415MessageBuilder>("Should use V1 builder when application code is LV1", builder);
		}
	}

	class IEMessageSenderForTest : IEMessageSender
	{
		public IEMessageSenderForTest(IH7MessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		public IXmlMessageBuilder CreateMessageBuilder_Exposed()
		{
			return CreateMessageBuilder(null);
		}
	}
}
