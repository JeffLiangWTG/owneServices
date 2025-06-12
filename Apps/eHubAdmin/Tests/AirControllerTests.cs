using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubArchiveOnline;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubAdmin.Controllers;
using eServices.eHubAdmin.ViewModels.Messages;
using NUnit.Framework;
using Rhino.Mocks;
using Query = eServices.eHubAdmin.ViewModels.Air.Query;

namespace eServices.eHubAdmin.Tests
{
	public class AirControllerTests
	{
		IDbTransaction stubTransaction;
		IDbConnection stubConnection;
		TestDbSet<eHubClient> testClients;
		TestDbSet<eHubInboxMessage> testInboxMessages;
		TestDbSet<eHubOutboxMessage> testOutboxMessages;
		TestDbSet<eHubInboxMessageArchive> testInboxMessageArchives;
		TestDbSet<eHubOutboxMessageArchive> testOutboxMessageArchives;
		TestDbSet<eHubSubscriptionType> testSubscriptionTypes;
		TestDbSet<eHubSubscriptionValue> testSubscriptionValues;
		TestDbSet<eHubArchiveMessage> testArchiveMessages;
		TestDbSet<eHubError> testErrors;
		eHubTransactionsContext stubeHubTransactionsContext;
		eHubArchiveOnlineContext stubeHubArchiveOnlineSecondaryContext;
		AirController airController;

		[SetUp]
		public void Setup()
		{
			var mockRequest = MockRepository.GenerateMock<HttpRequestBase>();
			var mockResponse = MockRepository.GenerateMock<HttpResponseBase>();
			var mockHttpSessionStateBase = MockRepository.GenerateMock<HttpSessionStateBase>();
			var mockHttpContext = MockRepository.GenerateMock<HttpContextBase>();
			mockHttpContext.Stub(x => x.Request).Return(mockRequest);
			mockHttpContext.Stub(x => x.Response).Return(mockResponse);
			mockHttpContext.Stub(x => x.Session).Return(mockHttpSessionStateBase);

			stubTransaction = MockRepository.GenerateMock<IDbTransaction>();
			stubConnection = MockRepository.GenerateMock<IDbConnection>();
			testClients = new TestDbSet<eHubClient>();
			testInboxMessages = new TestDbSet<eHubInboxMessage>();
			testOutboxMessages = new TestDbSet<eHubOutboxMessage>();
			testInboxMessageArchives = new TestDbSet<eHubInboxMessageArchive>();
			testOutboxMessageArchives = new TestDbSet<eHubOutboxMessageArchive>();
			testSubscriptionTypes = new TestDbSet<eHubSubscriptionType>();
			testSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			testArchiveMessages = new TestDbSet<eHubArchiveMessage>();
			testErrors = new TestDbSet<eHubError>();

			stubeHubTransactionsContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			stubeHubTransactionsContext.Stub(x => x.BeginTransaction(Arg.Is(IsolationLevel.ReadUncommitted))).Return(stubTransaction);
			stubeHubTransactionsContext.Stub(x => x.Connection).Return(stubConnection);
			stubeHubTransactionsContext.Stub(x => x.CommandTimeout).PropertyBehavior();
			stubeHubTransactionsContext.Stub(x => x.eHubClients).Return(testClients);
			stubeHubTransactionsContext.Stub(x => x.eHubInboxMessages).Return(testInboxMessages);
			stubeHubTransactionsContext.Stub(x => x.eHubOutboxMessages).Return(testOutboxMessages);
			stubeHubTransactionsContext.Stub(x => x.eHubInboxMessageArchives).Return(testInboxMessageArchives);
			stubeHubTransactionsContext.Stub(x => x.eHubOutboxMessageArchives).Return(testOutboxMessageArchives);
			stubeHubTransactionsContext.Stub(x => x.eHubSubscriptionTypes).Return(testSubscriptionTypes);
			stubeHubTransactionsContext.Stub(x => x.eHubSubscriptionValues).Return(testSubscriptionValues);
			stubeHubTransactionsContext.Stub(x => x.eHubErrors).Return(testErrors);
			stubeHubTransactionsContext.Stub(x => x.Connection).Return(stubConnection);

			stubeHubArchiveOnlineSecondaryContext = MockRepository.GenerateMock<eHubArchiveOnlineContext>();
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.BeginTransaction(Arg.Is(IsolationLevel.ReadUncommitted))).Return(stubTransaction);
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.Connection).Return(stubConnection);
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.CommandTimeout).PropertyBehavior();
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.eHubArchiveMessages).Return(testArchiveMessages);
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.Connection).Return(stubConnection);

			airController = MockRepository.GeneratePartialMock<AirController>(stubeHubTransactionsContext, stubeHubArchiveOnlineSecondaryContext);
			airController.ControllerContext = new ControllerContext(mockHttpContext, new RouteData(), airController);
		}

		[Test]
		public void AirController_List()
		{
			var awb_st = new Guid("13eabe7d-4cb4-46b8-9028-05280272bd57");
			var awb_type = testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = awb_st, ST_ID = "AIRAWB" });
			testSubscriptionValues.AddRange(new[]
			{
				new eHubSubscriptionValue { eHubSubscriptionType = awb_type, SV_SubscribedUTC = new DateTime(2021, 1, 1, 1, 0, 0), SV_Value = "12345678901-2021-01-01T01:00:00.0000000Z", SV_Reference = "00000000-0000-0000-0000-000000000000" },
				new eHubSubscriptionValue { eHubSubscriptionType = awb_type, SV_SubscribedUTC = new DateTime(2021, 1, 1, 2, 0, 0), SV_Value = "12345678901-2021-01-01T02:00:00.0000000Z", SV_Reference = "11111111-0000-0000-0000-000000000000" },
				new eHubSubscriptionValue { eHubSubscriptionType = awb_type, SV_SubscribedUTC = new DateTime(2021, 1, 1, 3, 0, 0), SV_Value = "09876543210-2021-01-01T03:00:00.0000000Z", SV_Reference = "22222222-0000-0000-0000-000000000000" },
				new eHubSubscriptionValue { eHubSubscriptionType = awb_type, SV_SubscribedUTC = new DateTime(2021, 1, 1, 4, 0, 0), SV_Value = "09876543210-2021-01-01T04:00:00.0000000Z", SV_Reference = "33333333-0000-0000-0000-000000000000" },
			});

			var client1 = new eHubClient { CC_ID = "TSTAAA111" };
			var client2 = new eHubClient { CC_ID = "TSTAAA222" };
			var provider1 = new eHubClient { CC_ID = "PROVIDER1" };

			testInboxMessages.AddRange(new[]
			{
				new eHubInboxMessage { EI_PK = new Guid("00000000-0000-0000-0000-000000000000"), EI_MessageTrackingID = "00000000-0000-0000-0000-000000000000", eHubClient_Sender = client1, eHubClient_Recipient = provider1 },
				new eHubInboxMessage { EI_PK = new Guid("11111111-0000-0000-0000-000000000000"), EI_MessageTrackingID = "11111111-0000-0000-0000-000000000000", eHubClient_Sender = provider1, eHubClient_Recipient = client1 },
				new eHubInboxMessage { EI_PK = new Guid("22222222-0000-0000-0000-000000000000"), EI_MessageTrackingID = "22222222-0000-0000-0000-000000000000", eHubClient_Sender = client2, eHubClient_Recipient = provider1, EI_InsertUTC = DateTime.UtcNow.AddMinutes(-30) },
				new eHubInboxMessage { EI_PK = new Guid("44444444-0000-0000-0000-000000000000"), EI_MessageTrackingID = "22222222-0000-0000-0000-000000000000", eHubClient_Sender = client2, eHubClient_Recipient = provider1, EI_InsertUTC = DateTime.UtcNow.AddMinutes(-20) },
			});
			testArchiveMessages.AddRange(new[]
			{
				new eHubArchiveMessage { AM_PK = new Guid("33333333-0000-0000-0000-000000000000"), AM_InboxMessageTrackingID = new Guid("33333333-0000-0000-0000-000000000000"), eHubClient_SenderInbox = provider1, eHubClient_RecipientInbox = client2, AM_ReceivedFromSenderUTC = DateTime.UtcNow.AddMinutes(-5) },
				new eHubArchiveMessage { AM_PK = new Guid("55555555-0000-0000-0000-000000000000"), AM_InboxMessageTrackingID = new Guid("33333333-0000-0000-0000-000000000000"), eHubClient_SenderInbox = provider1, eHubClient_RecipientInbox = client2, AM_ReceivedFromSenderUTC = DateTime.UtcNow.AddMinutes(-10) }
			});

			// Results from Inbox only
			var query1 = new Query { Waybill = "12345678901" };
			Assert.That(query1.TimeZoneIdIANA, Is.EqualTo("Australia/Sydney"), "Should have default value");
			var result1 = airController.List(query1, null) as PartialViewResult;
			CollectionAssert.AreEqual(new[] { "11111111-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000" }, ((List<InboxMessage>)result1.Model).Select(i => (i.PK ?? i.AM_PK).ToString()));

			// Results from Inbox and Archive
			var query2 = new Query { Waybill = "09876543210" };
			var result2 = airController.List(query2, null) as PartialViewResult;
			CollectionAssert.AreEqual(new[] { "33333333-0000-0000-0000-000000000000", "55555555-0000-0000-0000-000000000000", "44444444-0000-0000-0000-000000000000", "22222222-0000-0000-0000-000000000000" }, ((List<InboxMessage>)result2.Model).Select(i => (i.PK ?? i.AM_PK).ToString()));

			// No results
			var query3 = new Query { Waybill = "99999999999" };
			var result3 = airController.List(query3, null) as PartialViewResult;
			CollectionAssert.IsEmpty((List<InboxMessage>)result3.Model);
		}

		[Test]
		public void AirController_Query_ST_ExpiryDays()
		{
			var query = new Query();
			var awb_st = new Guid("13eabe7d-4cb4-46b8-9028-05280272bd57");

			var result = airController.Query(query) as ViewResult;
			Assert.AreEqual(null, result.ViewBag.ST_ExpiryDays);

			testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = awb_st, ST_ID = "AIRAWB" });
			result = airController.Query(query) as ViewResult;
			Assert.AreEqual(null, result.ViewBag.ST_ExpiryDays);

			testSubscriptionTypes.Remove(testSubscriptionTypes.First());
			testSubscriptionTypes.Add(new eHubSubscriptionType { ST_PK = awb_st, ST_ID = "AIRAWB", ST_ExpiryDays = 180 });
			result = airController.Query(query) as ViewResult;
			Assert.AreEqual(180, result.ViewBag.ST_ExpiryDays);
		}
	}
}
