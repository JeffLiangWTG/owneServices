using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(EUManifestMessageSender))]
	sealed class EUManifestMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendInvalidationRequest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var messageSender = new EUManifestMessageSender(manifestHeader);

			messageSender.SendMessage(MessageTypes.Codes.Q04);
			AssertMessageStatusAndEDIMessageDetails(manifestHeader, MessageTypes.Codes.Q04);
		}

		public void TestAdditionalInformationRequest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.RegistrationNumber = "AA";
			var messageSender = new EUManifestMessageSender(manifestHeader);

			messageSender.SendMessage(MessageTypes.Codes.R02);
			AssertMessageStatusAndEDIMessageDetails(manifestHeader, MessageTypes.Codes.R02);
		}

		public void TestSendMessage_ShouldFail_WhenManifestHasAwaitingStatus()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			var messageSender = new EUManifestMessageSender(manifestHeader);

			var sendMessageResult = messageSender.SendMessage(MessageTypes.Codes.R02);

			AssertEquals("There are messages waiting for a response. You are unable to send a message until a valid response is received. If a response has been received, exit the job, then re-open to refresh the status", sendMessageResult);
			AssertEquals(0, manifestHeader.Messages.Count);
		}

		public void TestSendAndAmendManifestMenuItem_ShouldGenerateMessageWhenCapable()
		{
			var specificCircumstanceList = new EUICS2SpecificCircumstanceList().GetAllCodes();

			foreach (var specificCircumstance in specificCircumstanceList)
			{
				TestSendFillingMessage(specificCircumstance);
				if (specificCircumstance != EUICS2SpecificCircumstanceList.Codes.F25)
				{
					AssertSendAmendManifest(specificCircumstance);
				}
			}
		}

		public void TestSendWithMultipleItems()
		{
			AsycudaManifestHeader CreateHeaderAndSendMessages(string messageTypes)
			{
				var manifestHeader = Factory.New<AsycudaManifestHeader>();
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;
				manifestHeader.RegistrationDate = ZDateTime.Now;
				manifestHeader.RegistrationNumber = "Test";

				var bill1 = manifestHeader.Bills.AddNew();
				bill1.ABL_BillNumber = "HB001";
				bill1.TransportDocumentType = "TD1";

				var hrcmScreeningOnBill1 = bill1.BillScreenings.AddNew();
				hrcmScreeningOnBill1.ASR_Result = "B01";

				var bill2 = manifestHeader.Bills.AddNew();
				bill2.ABL_BillNumber = "HB002";
				bill2.TransportDocumentType = "TD2";

				var hrcmScreeningOnBill2 = bill2.BillScreenings.AddNew();
				hrcmScreeningOnBill2.ASR_Result = "B02";

				var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
				requestHeader1.EUS_Identifier = "A70";
				requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

				var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
				requestHeader2.EUS_Identifier = "B90";
				requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

				var requestHeader3 = manifestHeader.RequestHeaders.AddNew();
				requestHeader3.EUS_Identifier = "C58";
				requestHeader3.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

				requestHeader1.EUS_HouseBillNumber = "HB001";
				requestHeader2.EUS_HouseBillNumber = "HB002";

				Factory.Save();

				var amendedItemsHeader = new ICS2AmendedItemsHeader(messageTypes, manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
				amendedItemsHeader.AmendedItems[0].IsSelected = true;
				amendedItemsHeader.AmendedItems[1].IsSelected = false;
				amendedItemsHeader.AmendedItems[2].IsSelected = true;

				var messageSender = new EUManifestMessageSender(manifestHeader);
				messageSender.SendAmendmentMessage(amendedItemsHeader);

				return manifestHeader;
			}

			CombineAssertions(() =>
			{
				var manifestHeader = CreateHeaderAndSendMessages(MessageTypes.Codes.A22);
				var requestHeader1 = manifestHeader.RequestHeaders[0];
				var requestHeader2 = manifestHeader.RequestHeaders[1];
				var requestHeader3 = manifestHeader.RequestHeaders[2];

				AssertEquals("Should not update the status for unselected request headers.", string.Empty, requestHeader2.EUS_Status);
				AssertEquals("Should update the status to SNT for A22 messages.", MessageStatusCodeList.Codes.Sent, requestHeader1.EUS_Status);
				AssertEquals("Should update the status to SNT for A22 messages.", MessageStatusCodeList.Codes.Sent, requestHeader3.EUS_Status);

				AssertEquals("Should generate 2 new messages from these two selected request headers.", 2, manifestHeader.Messages.Count);
			});

			CombineAssertions(() =>
			{
				var manifestHeader = CreateHeaderAndSendMessages(MessageTypes.Codes.R02);
				var requestHeader1 = manifestHeader.RequestHeaders[0];
				var requestHeader2 = manifestHeader.RequestHeaders[1];
				var requestHeader3 = manifestHeader.RequestHeaders[2];

				AssertEquals("Should not update the status for unselected request headers.", string.Empty, requestHeader2.EUS_Status);
				AssertEquals("Should update the status to AWA for R02 messages.", MessageStatusCodeList.Codes.Awaiting, requestHeader1.EUS_Status);
				AssertEquals("Should update the status to AWA for R02 messages.", MessageStatusCodeList.Codes.Awaiting, requestHeader3.EUS_Status);

				var message = manifestHeader.Messages.SingleOrDefault() as EDIMessage;
				AssertNotNull("Should generate 1 new message which combine these two selected request headers.", message);

				var text = message.EM_MessageText;
				AssertContains("<referralRequestReference>A70</referralRequestReference>", text);
				AssertContains("<referralRequestReference>C58</referralRequestReference>", text);
			});

			CombineAssertions(() =>
			{
				var manifestHeader = CreateHeaderAndSendMessages(MessageTypes.Codes.R03);
				var requestHeader1 = manifestHeader.RequestHeaders[0];
				var requestHeader2 = manifestHeader.RequestHeaders[1];
				var requestHeader3 = manifestHeader.RequestHeaders[2];

				AssertEquals("Should not update the status for unselected request headers.", string.Empty, requestHeader2.EUS_Status);
				AssertEquals("Should update the status to AWA for R03 messages.", MessageStatusCodeList.Codes.Awaiting, requestHeader1.EUS_Status);
				AssertEquals("Should update the status to AWA for R03 messages.", MessageStatusCodeList.Codes.Awaiting, requestHeader3.EUS_Status);

				var message = manifestHeader.Messages.SingleOrDefault() as EDIMessage;
				AssertNotNull("Should generate 1 new message which combine these two selected request headers.", message);

				var text = message.EM_MessageText;
				AssertContains("Should output it as the requestHeader1 links to a bill.", "<referralRequestReference>A70</referralRequestReference>", text);
				AssertNotContains("Should not output it as the requestHeader2 is not selected.", "<referralRequestReference>B90</referralRequestReference>", text);
				AssertNotContains("Should not output it as the requestHeader3 doesn't link to a bill.", "<referralRequestReference>C58</referralRequestReference>", text);
			});
		}

		void TestSendFillingMessage(string selectedSpecificCircumstance)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.SpecificCircumstanceIndicator = selectedSpecificCircumstance;

			var messageSender = new EUManifestMessageSender(manifestHeader);
			var messageType = GetMessageTypeFromSender(manifestHeader, isAmending: false);

			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, "TestReferenceNumber");
			Factory.Save();
			var oldLRN = manifestHeader.LocalReferenceNumber;

			var lrnQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, manifestHeader.AMA_RN_NKCountry);
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, manifestHeader.TableName);
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, manifestHeader.PK);

			AssertEquals("Pre: 1 CusEntryNum in db.", 1, Factory.Load<CusEntryNumber>(lrnQuery).Length);

			var messageBuilderSupported = EUICS2MessageBuilderLoader.Instance.GetMessageBuilder(messageType, manifestHeader, null) != null;

			if (messageBuilderSupported)
			{
				messageSender.SendFilingMessage();
				AssertMessageStatusAndEDIMessageDetails(manifestHeader, messageType);

				manifestHeader.Reload();
				AssertNotEquals("New LRN should be generated when FXX message was sent.", oldLRN, manifestHeader.LocalReferenceNumber);

				var cusEntryNumInDB = Factory.Load<CusEntryNumber>(new ZQuery(lrnQuery));
				CombineAssertions("New LRN should be saved into new CusEntryNumber", () =>
				{
					AssertEquals("Count", 2, cusEntryNumInDB.Length);
					Assert("old", cusEntryNumInDB.Any(x => x.CE_EntryNum == oldLRN));
					Assert("new", cusEntryNumInDB.Any(x => x.CE_EntryNum == manifestHeader.LocalReferenceNumber));
				});

				var message = (EDIMessage)manifestHeader.Messages.Single();
				AssertContains("New LRN should be exported to message text", manifestHeader.LocalReferenceNumber, message.EM_MessageText);
			}
			else
			{
				AssertEquals($"Invalid EU ICS2 Message Builder for code: {messageType}", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestSendManifestFailed_RollbackLRN()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeaderForTesting>();
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;
			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, "TestReferenceNumber");

			Factory.Save();

			var lrnQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, manifestHeader.AMA_RN_NKCountry);
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, manifestHeader.TableName);
			lrnQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, manifestHeader.PK);

			AssertEquals("Pre: 1 CusEntryNum in db.", 1, Factory.Load<CusEntryNumber>(lrnQuery).Length);

			var messageSender = new EUManifestMessageSender(manifestHeader);

			AssertNoExceptionThrown("Expected message sending failure.", () => messageSender.SendFilingMessage());

			Factory.Save();
			manifestHeader.Reload();

			AssertEquals("LRN should be rolled back when message sending failure.", "TestReferenceNumber", manifestHeader.LocalReferenceNumber);
			AssertEquals("No CusEntryNum record should be created.", 1, Factory.Load<CusEntryNumber>(lrnQuery).Length);
		}

		void AssertSendAmendManifest(string selectedSpecificCircumstance)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.SpecificCircumstanceIndicator = selectedSpecificCircumstance;
			manifestHeader.RegistrationDate = ZDateTime.Now;
			manifestHeader.RegistrationNumber = "Test";

			Factory.Save();

			var messageSender = new EUManifestMessageSender(manifestHeader);
			var messageType = GetMessageTypeFromSender(manifestHeader, isAmending: true);

			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "A70";
			requestHeader.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
			var amendedItem = amendedItemsHeader.AmendedItems[0];
			amendedItem.IsSelected = true;

			var messageBuilderSupported = EUICS2MessageBuilderLoader.Instance.GetMessageBuilder(messageType, manifestHeader, new[] { amendedItem }) != null;

			if (messageBuilderSupported)
			{
				messageSender.SendAmendmentMessage(amendedItemsHeader);
				AssertMessageStatusAndEDIMessageDetails(manifestHeader, messageType);
			}
			else
			{
				AssertEquals($"Invalid EU ICS2 Message Builder for code: {messageType}", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		void AssertMessageStatusAndEDIMessageDetails(AsycudaManifestHeader manifestHeader, string expectedMessageType)
		{
			CombineAssertions(() =>
			{
				var message = (EDIMessage)manifestHeader.Messages.Single();
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IC2, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", expectedMessageType, message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertStartsWith("EM_MessageText", $@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3{expectedMessageType}", message.EM_MessageText);
			});
		}

		string GetMessageTypeFromSender(AsycudaManifestHeader manifestHeader, bool isAmending)
		{
			return manifestHeader.GetCustomsMessageType(isAmending);
		}

		sealed class AsycudaManifestHeaderForTesting : AsycudaManifestHeader
		{
			public AsycudaManifestHeaderForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();

				foreach (var message in Messages)
				{
					if (message is ICS2OutboundEDIMessage ics2OutboundMessage)
					{
						ics2OutboundMessage.Saving += (m) =>
						{
							var testInnerException = new ZDataException(new Exception(), null, Db.Connection);
							testInnerException.SetFriendlyMessageForTest("TestProcessWithSaveExceptionHandling_NoInnerSqlException");
							throw new ZSaveException(testInnerException, Factory);
						};
					}
				}
			}
		}
	}
}
