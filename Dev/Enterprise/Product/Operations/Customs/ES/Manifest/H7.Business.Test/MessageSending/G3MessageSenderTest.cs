using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseMessageSendingObject;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3MessageSenderTest : TestCaseWithFactory
	{
		[TestDate(2022, 3, 12, 4, 0, 0)]
		public void TestSendMessageSuccessfully()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = false;

			header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES1230789654", "ES");
			(header as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 2);
			Factory.Save();

			var messageSender = new G3MessageSender(sendingObjectParent);
			var messagesSent = messageSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("Message can be sent successfully", 1, messagesSent);

				AssertEquals("Number of message sent", 1, header.Messages.Count);
				AssertEquals("Header Message Status should not be updated", string.Empty, header.AMA_MessageStatus);
				AssertEquals("Message Type", G3MessageTypes.Codes.G3Declaration, ((Enterprise.Messaging.Business.EDIMessage)header.Messages.FirstOrDefault()).EM_MessageType);

				AssertEquals("G3 LRN of bill which is sent should be updated", "2212307896540000000002", bill1.G3LocalReferenceNumber);
				AssertEquals("Bill Message Status of bill which is sent should be SNT", LogicalStatusList.Codes.Sent, bill1.ABL_MessageStatus);

				AssertEquals("G3 LRN of bill which is not sent should be updated", string.Empty, bill2.G3LocalReferenceNumber);
				AssertEquals("Bill Message Status of bill which is not sent should not be updated", string.Empty, bill2.ABL_MessageStatus);
			});
		}

		[TestDate(2022, 3, 12, 4, 0, 0)]
		public void TestSendMessageSuccessfully_LRNWithNIF()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = false;

			header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "ES1230789654", "ES");
			(header as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 2);
			Factory.Save();

			var messageSender = new G3MessageSender(sendingObjectParent);
			var messagesSent = messageSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("Message can be sent successfully", 1, messagesSent);

				AssertEquals("Number of message sent", 1, header.Messages.Count);
				AssertEquals("Header Message Status should not be updated", string.Empty, header.AMA_MessageStatus);
				AssertEquals("Message Type", G3MessageTypes.Codes.G3Declaration, ((Enterprise.Messaging.Business.EDIMessage)header.Messages.FirstOrDefault()).EM_MessageType);

				AssertEquals("G3 LRN of bill which is sent should be updated", "2212307896540000000002", bill1.G3LocalReferenceNumber);
				AssertEquals("Bill Message Status of bill which is sent should be SNT", LogicalStatusList.Codes.Sent, bill1.ABL_MessageStatus);

				AssertEquals("G3 LRN of bill which is not sent should be updated", string.Empty, bill2.G3LocalReferenceNumber);
				AssertEquals("Bill Message Status of bill which is not sent should not be updated", string.Empty, bill2.ABL_MessageStatus);
			});
		}

		[TestDate(2022, 3, 12, 4, 0, 0)]
		public void TestSendMessageSuccessfully_LRNWithCorrectEoriWhenMultiples()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = false;

			var organization = header.Branch.OrgProxy;

			var customsCode = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789A", "AU");
			customsCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES1230789654", "ES");
			customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);

			(header as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 2);
			Factory.Save();

			var messageSender = new G3MessageSender(sendingObjectParent);
			var messagesSent = messageSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("Message can be sent successfully", 1, messagesSent);

				AssertEquals("Number of message sent", 1, header.Messages.Count);
				AssertEquals("Header Message Status should not be updated", string.Empty, header.AMA_MessageStatus);
				AssertEquals("Message Type", G3MessageTypes.Codes.G3Declaration, ((Enterprise.Messaging.Business.EDIMessage)header.Messages.FirstOrDefault()).EM_MessageType);

				AssertEquals("G3 LRN of bill which is sent should be updated", "2212307896540000000002", bill1.G3LocalReferenceNumber);
				AssertEquals("Bill Message Status of bill which is sent should be SNT", LogicalStatusList.Codes.Sent, bill1.ABL_MessageStatus);

				AssertEquals("G3 LRN of bill which is not sent should be updated", string.Empty, bill2.G3LocalReferenceNumber);
				AssertEquals("Bill Message Status of bill which is not sent should not be updated", string.Empty, bill2.ABL_MessageStatus);
			});
		}

		public void TestUpdateMessageContentOnPreviewMessageEditor()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill1 = header.Bills.AddNew();

			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			var sendingObject = sendingObjectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;

			header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES1230789654", "ES");
			(header as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 2);
			Factory.Save();

			sendingObject.PreviewMessage += new MessagePreviewEventHandler(OnMessageCreated);
			var messageSender = new G3MessageSender(sendingObjectParent);
			messageSender.Send();

			AssertEquals("Tested Message Content", ((Enterprise.Messaging.Business.EDIMessage)header.Messages.FirstOrDefault()).EM_MessageText);

			void OnMessageCreated(MessageEventArgs args)
			{
				args.MessageText = "Tested Message Content";
			}
		}

		public void TestG3LRNNotUpdatedWhenItFailsToSendMessage()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObjectParent = new G3MessageSendingObjectParent(header);
			sendingObjectParent.SendingObjectsCollection[0].Action = "ABC";
			sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			sendingObjectParent.SendingObjectsCollection[1].ShouldSend = false;

			var messageSender = new G3MessageSender(sendingObjectParent);
			var messagesSent = messageSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("Message cannot be sent without LRN", 0, messagesSent);
				AssertEquals("G3 LRN of bill 1 should be updated", string.Empty, bill1.G3LocalReferenceNumber);
				AssertEquals("G3 LRN of bill 2 should be updated", string.Empty, bill2.G3LocalReferenceNumber);
			});

			ErrorReporter.Clear();
		}

		[TestDate(2025, 3, 12, 4, 0, 0)]
		public void TestSendMessageWhenMultipleChunks_SendsMessageSuccessfully()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			for (int i = 0; i < 3; i++)
			{
				header.Bills.AddNew();
			}

			var sendingObjectParent = new G3MessageSendingObjectParent(header);

			header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES1230789654", "ES");
			Factory.Save();

			var messageSender = new G3MessageSender(sendingObjectParent);
			SetG3MessageSenderChunkSize(messageSender, 1);
			var messagesSent = messageSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("Messages can be sent successfully", 3, messagesSent);
				AssertEquals("There are 3 different G3 LRNs", 3, header.Bills.Select(x => x.G3LocalReferenceNumber).Distinct().Count());
			});
		}

		public void TestSendMessageWhenMultipleChunks_FailsToSendMessage()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			for (int i = 0; i < 3; i++)
			{
				header.Bills.AddNew();
			}

			var sendingObjectParent = new G3MessageSendingObjectParent(header);

			Factory.Save();

			var messageSender = new G3MessageSender(sendingObjectParent);
			SetG3MessageSenderChunkSize(messageSender, 1);
			var messagesSent = messageSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("The number of messages successfully sent is 0", 0, messagesSent);
				AssertEquals("Report the error only once", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Exception thrown when trying to send declaration", ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		void SetG3MessageSenderChunkSize(G3MessageSender messageSender, int newChunkSize)
		{
			var fieldInfo = typeof(G3MessageSender).GetField("chunkSize", BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo != null)
			{
				fieldInfo.SetValue(messageSender, newChunkSize);
			}
		}
	}
}
