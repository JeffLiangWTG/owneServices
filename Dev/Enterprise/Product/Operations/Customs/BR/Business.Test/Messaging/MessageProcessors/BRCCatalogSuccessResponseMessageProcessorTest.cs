using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCCatalogSuccessResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCCatalogSuccessResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CAT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "SUC" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCCatalogSuccessResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "Goods Catalog Success Response", processor.MessageFriendlyName);
		}

		public void TestProcessResponseMessageInvalidJson()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"[]";

			CombineAssertions(() =>
			{
				AssertExceptionThrown<JsonException>(() => ExecuteMessageProcessor(responseMessage));
				AssertEquals("CGC_AuthorityIdentifier not changed", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion not changed", "9", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Active, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		public void TestProcessResponseMessageWithoutSeq()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageWithoutSeq;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", $"Message #1: Message deserialization was failed or tag 'seq' not found or is empty.", logger.LogMessages.ToString());

				AssertEquals("CGC_AuthorityIdentifier not changed", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion not changed", "9", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Active, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		public void TestProcessResponseMessageWithoutCode()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageWithoutCode;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertContains("Logger", $"Message #1: Message deserialization was failed, tag 'codigo' not found or is empty.", logger.LogMessages.ToString());

				AssertEquals("CGC_AuthorityIdentifier not changed", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion not changed", "9", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Active, goodsCatalog.CGC_CustomsStatus);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageWithoutVersion()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageWithoutVersion;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CGC_AuthorityIdentifier not changed", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion not changed", "9", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		public void TestProcessResponseMessageWithoutSequenceMatch()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage.Replace("\"seq\": 1", "\"seq\": 5");
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Unable to locate the related outgoing message for CAT message #1", logger.LogMessages.ToString());

				AssertEquals("CGC_AuthorityIdentifier not changed", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion not changed", "9", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Active, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		public void TestProcessResponseMessageNoSuccess()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageNoSucess;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertNotEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);

				AssertEquals("CGC_AuthorityIdentifier not changed", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion not changed", "9", goodsCatalog.CGC_AuthorityVersion);
				Assert("Log AutoEvents.MessageRejected Created", goodsCatalog.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Rejected, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Active, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageSuccessUpdateVersion()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CGC_AuthorityIdentifier not updated", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion updated", "1", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageSuccessUpdateVersionWithForeignOperatorAccepted()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = "BR";
			foreignOperator.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CGC_AuthorityIdentifier not updated", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion updated", "1", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageSuccessUpdateVersionWithForeignOperatorNotAccepted()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = "BR";
			foreignOperator.CGI_CustomsStatus = "";
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CGC_AuthorityIdentifier not updated", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion updated", "1", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Rejected, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);
				Assert("Log AutoEvents.MessageRejected Created", goodsCatalog.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MessageRejected.Code));

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageSuccessUpdateIdentifier()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.CGC_AuthorityIdentifier = ZString.Empty;
			goodsCatalog.CGC_MessageStatus = ZString.Empty;
			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = "BR";

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success);
			responseMessage.EM_MessageText = JsonMessage;

			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XXX";

			var password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			requestMessage.EM_GP = password.PK;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CGC_AuthorityIdentifier updated", "1", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion updated", "1", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 1, password.PK);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageSuccessUpdateIdentifierWithoutForeignOperator()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.CGC_AuthorityIdentifier = ZString.Empty;
			goodsCatalog.CGC_MessageStatus = ZString.Empty;

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success);
			responseMessage.EM_MessageText = JsonMessage;

			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XXX";

			var password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			requestMessage.EM_GP = password.PK;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				AssertEquals("CGC_AuthorityIdentifier updated", "1", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion updated", "1", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageDifferentCodeAndAuthorityIdentifier()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.CGC_AuthorityIdentifier = "55";
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertContains("Logger", $"Message #1: Tag 'codigo' is different than Authority Identifier.", logger.LogMessages.ToString());

				AssertEquals("CGC_AuthorityIdentifier not updated", "55", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion updated", "1", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		public void TestProcessResponseMessageNullSeq()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageNullSeq;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", $"Message #1: Message deserialization was failed or tag 'seq' not found or is empty.", logger.LogMessages.ToString());

				AssertEquals("CGC_AuthorityIdentifier not changed", "8", goodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion not changed", "9", goodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Active, goodsCatalog.CGC_CustomsStatus);

				AssertLinkMessageSent(goodsCatalog, 0);
			});
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProcessResponseMessageUpdateAuthorityStatus()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			AssertProcessResponseMessageSetAuthorityStatus(string.Empty);

			goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.Logs.AddNew(Events.MessageSent, ActionList.Codes.CreateDraft, ZDateTimeOffset.Now.AddDays(-4));
			AssertProcessResponseMessageSetAuthorityStatus(GoodsCatalogStatusTypeList.Codes.Draft);

			goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.Logs.AddNew(Events.MessageSent, ActionList.Codes.Deactivate, ZDateTimeOffset.Now.AddMinutes(-3));
			AssertProcessResponseMessageSetAuthorityStatus(GoodsCatalogStatusTypeList.Codes.Inactive);

			goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.Logs.AddNew(Events.MessageSent, ActionList.Codes.Activate, ZDateTimeOffset.Now.AddMinutes(-2));
			AssertProcessResponseMessageSetAuthorityStatus(GoodsCatalogStatusTypeList.Codes.Active);

			goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.Logs.AddNew(Events.MessageSent, ActionList.Codes.UpdateDraft, ZDateTimeOffset.Now.AddMinutes(-1));
			AssertProcessResponseMessageSetAuthorityStatus(string.Empty);

			goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.Logs.AddNew(Events.MessageSent, ActionList.Codes.CreateNewVersion, ZDateTimeOffset.Now);
			AssertProcessResponseMessageSetAuthorityStatus(string.Empty);

			void AssertProcessResponseMessageSetAuthorityStatus(string expectedAuthorityStatus)
			{
				var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
				responseMessage.EM_MessageText = JsonMessage;
				Factory.Save();

				ExecuteMessageProcessor(responseMessage);
				AssertEquals("UpdateCustomStatusOnSaving Suspended", true, goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);

				Factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("EM_GB", goodsCatalog.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
					AssertEquals("EM_LinkTable", "CusGoodsCatalog", responseMessage.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessage.EM_LinkUniqueID);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, responseMessage.EM_MessageType);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

					AssertEquals("CGC_AuthorityStatus updated", expectedAuthorityStatus, goodsCatalog.CGC_AuthorityStatus);
					AssertEquals("CGC_MessageStatus", BRMessageStatusList.Codes.Accepted, goodsCatalog.CGC_MessageStatus);
					AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Accepted, goodsCatalog.CGC_CustomsStatus);
					AssertEquals("UpdateCustomStatusOnSaving not Suspended", false, goodsCatalog.IsUpdateCustomStatusOnSavingSuspended);

					AssertLinkMessageSent(goodsCatalog, 0);
				});
			}
		}

		public void TestProcessResponseMessage_NotAllForeignOperatorProcessed()
		{
			var goodsCatalog = CreateGoodsCatalog(Factory);
			goodsCatalog.ForeignOperators.AddNew().CGI_Reference = "US";
			goodsCatalog.ForeignOperators.AddNew().CGI_Reference = "ZA";
			Factory.Save();

			var responseMessageCAT = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessageCAT.EM_MessageText = JsonMessage;
			BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link).ResponseMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, requestMessageSubType: EDIMessageSubTypeList.Codes.Link);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<MessageProcessLockException>("MessageProcessLockException thrown to postpone",
					$"Message #{responseMessageCAT.EM_MessageNum} postponed: Catalog {goodsCatalog.CGC_CatalogCode} has CAT-LIN message waiting response.", () =>
					{
						var logger = ExecuteMessageProcessor(responseMessageCAT);
						AssertEquals("Logger", $"Warning: \tMessage #{responseMessageCAT.EM_MessageNum} postponed: Catalog {goodsCatalog.CGC_CatalogCode}, has CAT-LIN message waiting response.\r\n", logger.LogMessages.ToString());
					});
				AssertEquals("EM_LinkUniqueID", goodsCatalog.PK, responseMessageCAT.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", goodsCatalog.TableName, responseMessageCAT.EM_LinkTable);
				AssertEquals("CGC_MessageStatus NOT updated", BRMessageStatusList.Codes.AwaitingResponse, goodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", CustomsPostedStatusList.Codes.Active, goodsCatalog.CGC_CustomsStatus);
			});
		}

		public static CusGoodsCatalog CreateGoodsCatalog(BusinessObjectFactory factory)
		{
			var goodsCatalog = factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_CatalogCode = "C123";
			goodsCatalog.CGC_AuthorityIdentifier = "8";
			goodsCatalog.CGC_AuthorityVersion = "9";
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;

			return goodsCatalog;
		}

		void AssertLinkMessageSent(CusGoodsCatalog goodsCatalog, int countOfLinkMessages, ZGuid? expectedEM_GP = null)
		{
			var newOriMessages = goodsCatalog.Messages.OfType<BREDIMessage>()
				.Where(x => x.EM_MessageType == MessageTypeList.Codes.CAT && x.EM_MessageSubType == EDIMessageSubTypeList.Codes.Original && x.EM_Status == EDIMessage.Status.Queued).ToList();
			AssertEquals($"No new CAT|ORI Messages sent", 0, newOriMessages.Count);

			var linkMessages = goodsCatalog.Messages.OfType<BREDIMessage>()
				.Where(x => x.EM_MessageType == MessageTypeList.Codes.CAT && x.EM_MessageSubType == EDIMessageSubTypeList.Codes.Link).ToList();
			AssertEquals($"{countOfLinkMessages} CAT|LIN Messages sent", countOfLinkMessages, linkMessages.Count);

			linkMessages.ForEach((message) =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_GP", expectedEM_GP ?? ZGuid.Empty, message.EM_GP);
			});
		}

		const string JsonMessage = @"{
    ""seq"": 1,
    ""codigo"": ""1"",
    ""erros"": [
      """"
    ],
    ""sucesso"": true,
    ""versao"": ""1""
  }";

		const string JsonMessageWithoutSeq = @"{
    ""codigo"": ""1"",
    ""erros"": [
      """"
    ],
    ""sucesso"": true,
    ""versao"": ""1""
  }";

		const string JsonMessageWithoutCode = @"{
    ""seq"": 1,
    ""erros"": [
      """"
    ],
    ""sucesso"": true,
    ""versao"": ""4""
  }";

		const string JsonMessageWithoutVersion = @"{
    ""seq"": 1,
    ""codigo"": ""1"",
    ""erros"": [
      """"
    ],
    ""sucesso"": true
  }";

		const string JsonMessageNoSucess = @"{
    ""seq"": 1,
    ""codigo"": ""1"",
    ""erros"": [
      """"
    ],
    ""sucesso"": false,
    ""versao"": ""4""
  }";

		const string JsonMessageNullSeq = @"{
    ""seq"": null,
    ""codigo"": ""0"",
    ""erros"": [
      ""Erro Fabricante/produtor não encontrado no produto.  ""
    ],
    ""sucesso"": false,
    ""versao"": null
  }";
	}
}
