using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubArchiveOnline;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubAdmin.Controllers;
using eServices.eHubAdmin.Helpers;
using eServices.eHubAdmin.ViewModels.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rhino.Mocks;

namespace eServices.eHubAdmin.Tests
{
	[TestClass]
	public class MessagesControllerTests
	{
		IDbTransaction stubTransaction;
		IDbConnection stubConnection;
		eHubTransactionsContext stubeHubTransactionsContext;
		eHubArchiveOnlineContext stubeHubArchiveOnlineSecondaryContext;
		TestDbSet<eHubClient> testClients;
		TestDbSet<eHubInboxMessageDisplay> testInboxMessageDisplays;
		TestDbSet<eHubOutboxMessageDisplay> testOutboxMessageDisplays;
		TestDbSet<eHubInboxMessageArchive> testInboxMessageArchives;
		TestDbSet<eHubOutboxMessageArchive> testOutboxMessageArchives;
		TestDbSet<eHubArchiveMessage> testArchiveSecondaryMessages;
		TestDbSet<eHubError> testErrors;
		Func<DateTime> testNowProvider;

		string savedDatabaseTimeout;
		MessagesController messagesController;

		[TestInitialize]
		public void MessagesController_TestInitialize()
		{
			savedDatabaseTimeout = ConfigurationManager.AppSettings["DatabaseCommandTimeout"];
			ConfigurationManager.AppSettings["DatabaseCommandTimeout"] = "90";
			ConfigurationManager.AppSettings["BodySearchTimePeriod"] = "48";

			var mockqueryString = MockRepository.GenerateMock<NameValueCollection>();
			var mockRequest = MockRepository.GenerateMock<HttpRequestBase>();
			mockRequest.Stub(x => x.QueryString).Return(mockqueryString);
			mockqueryString.Stub(x => x.Count).Return(0);

			var mockResponse = MockRepository.GenerateMock<HttpResponseBase>();
			var mockHttpSessionStateBase = MockRepository.GenerateMock<HttpSessionStateBase>();
			var mockHttpContext = MockRepository.GenerateMock<HttpContextBase>();
			mockHttpContext.Stub(x => x.Request).Return(mockRequest);
			mockHttpContext.Stub(x => x.Response).Return(mockResponse);
			mockHttpContext.Stub(x => x.Session).Return(mockHttpSessionStateBase);

			stubTransaction = MockRepository.GenerateMock<IDbTransaction>();
			stubConnection = MockRepository.GenerateMock<IDbConnection>();
			testClients = new TestDbSet<eHubClient>();
			testInboxMessageDisplays = new TestDbSet<eHubInboxMessageDisplay>();
			testOutboxMessageDisplays = new TestDbSet<eHubOutboxMessageDisplay>();
			testInboxMessageArchives = new TestDbSet<eHubInboxMessageArchive>();
			testOutboxMessageArchives = new TestDbSet<eHubOutboxMessageArchive>();
			testArchiveSecondaryMessages = new TestDbSet<eHubArchiveMessage>();
			testErrors = new TestDbSet<eHubError>();

			stubeHubTransactionsContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			stubeHubTransactionsContext.Stub(x => x.BeginTransaction(Arg.Is(IsolationLevel.ReadUncommitted))).Return(stubTransaction);
			stubeHubTransactionsContext.Stub(x => x.Connection).Return(stubConnection);
			stubeHubTransactionsContext.Stub(x => x.CommandTimeout).PropertyBehavior();
			stubeHubTransactionsContext.Stub(x => x.eHubClients).Return(testClients);
			stubeHubTransactionsContext.Stub(x => x.eHubInboxMessageDisplays).Return(testInboxMessageDisplays);
			stubeHubTransactionsContext.Stub(x => x.eHubOutboxMessageDisplays).Return(testOutboxMessageDisplays);
			stubeHubTransactionsContext.Stub(x => x.eHubInboxMessageArchives).Return(testInboxMessageArchives);
			stubeHubTransactionsContext.Stub(x => x.eHubOutboxMessageArchives).Return(testOutboxMessageArchives);
			stubeHubTransactionsContext.Stub(x => x.eHubErrors).Return(testErrors);
			stubeHubTransactionsContext.Stub(x => x.Connection).Return(stubConnection);

			stubeHubArchiveOnlineSecondaryContext = MockRepository.GenerateMock<eHubArchiveOnlineContext>();
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.BeginTransaction(Arg.Is(IsolationLevel.ReadUncommitted))).Return(stubTransaction);
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.Connection).Return(stubConnection);
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.CommandTimeout).PropertyBehavior();
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.eHubArchiveMessages).Return(testArchiveSecondaryMessages);
			stubeHubArchiveOnlineSecondaryContext.Stub(x => x.Connection).Return(stubConnection);

			var inboxSpaceUsed = new[] { new SpaceUsedProcedureHelper { name = "eHubInboxMessageArchive", rows = "1000" } };
			var outboxSpaceUsed = new[] { new SpaceUsedProcedureHelper { name = "eHubOutboxMessageArchive", rows = "5000" } };
			stubeHubTransactionsContext.Stub(x => x.SqlQuery<SpaceUsedProcedureHelper>("sp_spaceused 'eHubInboxMessageArchive'")).WhenCalled(_ => Thread.Sleep(millisecondsTimeout: 10)).Return(inboxSpaceUsed);
			stubeHubTransactionsContext.Stub(x => x.SqlQuery<SpaceUsedProcedureHelper>("sp_spaceused 'eHubOutboxMessageArchive'")).WhenCalled(_ => Thread.Sleep(millisecondsTimeout: 50)).Return(outboxSpaceUsed);

			testNowProvider = () => new DateTime(2015, 2, 3, 4, 15, 0, DateTimeKind.Utc);
			messagesController = MockRepository.GeneratePartialMock<MessagesController>(stubeHubTransactionsContext, stubeHubArchiveOnlineSecondaryContext, testNowProvider);
			messagesController.ControllerContext = new ControllerContext(mockHttpContext, new RouteData(), messagesController);
		}

		[TestCleanup]
		public void MessagesController_TestCleanup()
		{
			ConfigurationManager.AppSettings["DatabaseCommandTimeout"] = savedDatabaseTimeout;
		}

		[TestMethod]
		public void MessageController_Query()
		{
			MessageController_QueryTest(messagesController, new Query(), "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { AnyRole = "AAAA", Role = "Any" }, "{\"Role\":\"Any\",\"AnyRole\":\"AAAA\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { SenderOutbox = "BBBB" }, "{\"Role\":\"Specific\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"SenderOutbox\":\"BBBB\",\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { To = "20150203" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinDates\",\"ToDate\":\"2015-02-03T00:00:00\",\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinDates", To = "20150203091500" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinDates\",\"ToDate\":\"2015-02-03T00:00:00\",\"ToTime\":\"09:15:00\",\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinDates", From = "20150203" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinDates\",\"FromDate\":\"2015-02-03T00:00:00\",\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinDates", From = "20150203091500" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinDates\",\"FromDate\":\"2015-02-03T00:00:00\",\"FromTime\":\"09:15:00\",\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = 1 }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null, IsIncludingArchiveStaging = true }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
		}

		[TestMethod]
		public void MessageController_Query_ArchiveStaging()
		{
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");

			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null, IsIncludingArchiveStaging = false }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":false,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");

			messagesController.Stub(x => x.IsBelowThresholdArchiveStaging()).Return(true).Repeat.Once();
			MessageController_QueryTest(messagesController, new Query(), "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");

			messagesController.Stub(x => x.IsBelowThresholdArchiveStaging()).Return(false).Repeat.Once();
			MessageController_QueryTest(messagesController, new Query(), "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":false,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
		}

		[TestMethod]
		public void MessageController_Query_MessageBox()
		{
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null, IsIncludingMessageBox = false }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":false,\"SearchType\":\"Advanced\"}");
		}

		[TestMethod]
		public void MessageController_Query_Archive()
		{
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null, IsIncludingArchive = false }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":false,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
		}

		[TestMethod]
		public void MessageController_Period_Query()
		{
			MessageController_QueryTest(messagesController, new Query(), "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = 1, FromPeriodType = "Hour" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = 1, FromPeriodType = "Day" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Day\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null, FromPeriodType = "Week" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Week\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null, FromPeriodType = "Month", IsIncludingArchiveStaging = true }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Month\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");

			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = 10, FromPeriodType = "Hour", ToPeriodValue = 1, ToPeriodType = "Hour" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":10,\"ToPeriodValue\":1,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = 10, FromPeriodType = "Hour", ToPeriodValue = null, ToPeriodType = "Hour" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":10,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null, FromPeriodType = "Month", ToPeriodType = "Month", IsIncludingArchiveStaging = true }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Month\",\"ToPeriodType\":\"Month\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = 10, FromPeriodType = "Minute", ToPeriodValue = 1, ToPeriodType = "Minute" }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Minute\",\"ToPeriodType\":\"Minute\",\"FromPeriodValue\":10,\"ToPeriodValue\":1,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
		}

		[TestMethod]
		public void MessageController_TimeOut()
		{
			SetupTestData();
			messagesController.checkArchiveStagingTimeout = TimeSpan.FromMilliseconds(10);

			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":false,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");

			messagesController.checkArchiveStagingTimeout = TimeSpan.FromMilliseconds(1000);
			MessageController_QueryTest(messagesController, new Query { DateFilterType = "WithinPeriod", FromPeriodValue = null }, "{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}");
		}

		[TestMethod]
		public void DataCorrectionTest()
		{
			var controller = messagesController;

			var query = new Query
			{
				DateFilterType = "WithinPeriod",
				FromPeriodType = "Day",
				FromPeriodValue = 123
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinPeriod",
				FromPeriodType = "Day",
				FromPeriodValue = 0
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates",
				FromPeriodType = "Day",
				FromPeriodValue = 0
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates",
				FromPeriodType = "Month",
				FromPeriodValue = 0
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates",
				FromPeriodType = "test",
				FromPeriodValue = 0
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates",
				From = "20150203000000",
				To = "20150203091500"
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates",
				From = "20150203000000",
				To = "20150203091500",
				FromPeriodValue = 123
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates",
				From = "123",
				To = "abc"
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates"
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				From = "123",
				To = "abc"
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			query = new Query
			{
				DateFilterType = "WithinDates",
				To = "abc"
			};

			controller.ViewData.ModelState.Clear();
			controller.QueryPost(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);

			controller.ViewData.ModelState.Clear();
			controller.Query(query);
			Assert.IsTrue(controller.ViewData.ModelState.Count == 0);
		}

		static void MessageController_QueryTest(MessagesController controller, Query query, string expected)
		{
			if (string.IsNullOrEmpty(query.DateFilterType))
			{
				controller.Query(query);
				Assert.IsTrue(!string.IsNullOrEmpty(query.DateFilterType));
			}

			var result = controller.Query(query) as ViewResult;
			Assert.IsNotNull(result);
			var resultModel = result.Model as Query;
			var actualResult = JsonConvert.SerializeObject(resultModel, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			Assert.AreEqual(expected, actualResult);
		}

		[TestMethod]
		public void MessageController_ChangeSearchTypeRedirect()
		{
			// Arrange
			const string basic = "Basic";

			// Act
			var resultRedirect = messagesController.ChangeSearchType(basic);

			// Assert
			Assert.IsNotNull(resultRedirect);
			Assert.IsInstanceOfType(resultRedirect, typeof(RedirectToRouteResult));
			Assert.AreEqual(basic, messagesController.TempData["searchType"]);
			var redirectToRoute = resultRedirect as RedirectToRouteResult;
			Assert.IsNotNull(redirectToRoute);
			Assert.IsNotNull(redirectToRoute.RouteValues);
			Assert.IsTrue(redirectToRoute.RouteValues.Count > 0);
			Assert.AreEqual("Query", redirectToRoute.RouteValues["action"]);

			// Arrange
			const string advanced = "Advanced";

			// Act
			resultRedirect = messagesController.ChangeSearchType(advanced);

			// Assert
			Assert.IsNotNull(resultRedirect);
			Assert.IsInstanceOfType(resultRedirect, typeof(RedirectToRouteResult));
			Assert.AreEqual(advanced, messagesController.TempData["searchType"]);
			redirectToRoute = resultRedirect as RedirectToRouteResult;
			Assert.IsNotNull(redirectToRoute);
			Assert.IsNotNull(redirectToRoute.RouteValues);
			Assert.IsTrue(redirectToRoute.RouteValues.Count > 0);
			Assert.AreEqual("Query", redirectToRoute.RouteValues["action"]);
		}

		[TestMethod]
		public void MessageController_QueryAfterChangeSearchTypeRedirect()
		{
			// Arrange
			const string basic = "Basic";
			string expected =
				"{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Basic\"}";
			var query = new Query();
			messagesController.TempData["searchType"] = basic;

			// Act
			var resultRedirect = messagesController.Query(query) as ViewResult;

			// Assert
			Assert.IsNotNull(resultRedirect);
			Assert.IsNull(messagesController.TempData["searchType"]);
			var resultModel = resultRedirect.Model as Query;
			var actualResult = JsonConvert.SerializeObject(resultModel, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			Assert.AreEqual(expected, actualResult);

			// Arrange
			const string advanced = "Advanced";
			expected =
				"{\"Role\":\"Any\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Advanced\"}";
			query = new Query();
			messagesController.TempData["searchType"] = advanced;

			// Act
			resultRedirect = messagesController.Query(query) as ViewResult;

			// Assert
			Assert.IsNotNull(resultRedirect);
			Assert.IsNull(messagesController.TempData["searchType"]);
			resultModel = resultRedirect.Model as Query;
			actualResult = JsonConvert.SerializeObject(resultModel, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			Assert.AreEqual(expected, actualResult);
		}

		[TestMethod]
		public void MessageController_QueryPost()
		{
			var query = new Query { Role = "Any", AnyRole = "AAAA", Key = Guid.Empty, ToDate = new DateTime(2010, 10, 10), ToTime = new TimeSpan(0, 8, 8, 8, 8), FromDate = new DateTime(2011, 11, 11), FromTime = new TimeSpan(0, 9, 9, 9, 9) };
			var resultRedirect = messagesController.QueryPost(query) as RedirectToRouteResult;
			Assert.IsNotNull(resultRedirect);
			Assert.AreEqual("", resultRedirect.RouteName);
			var jsonResult = JsonConvert.SerializeObject(resultRedirect.RouteValues, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			Assert.AreEqual(
				"{\"Role\":\"Any\",\"AnyRole\":\"AAAA\",\"DateFilterType\":null,\"FromPeriodType\":null,\"ToPeriodType\":null,\"FromPeriodValue\":null,\"ToPeriodValue\":null,\"SenderInbox\":null,\"RecipientInbox\":null,\"SenderOutbox\":null,\"RecipientOutbox\":null,\"To\":\"20101010080808\",\"ToDate\":null,\"ToTime\":null,\"From\":\"20111111090909\",\"FromDate\":null,\"FromTime\":null,\"TimeZoneIdIANA\":null,\"DateRangeType\":null,\"Received\":null,\"Status\":null,\"ApplicationFull\":null,\"ApplicationCode\":null,\"Refresh\":null,\"KeyType\":null,\"Key\":\"00000000-0000-0000-0000-000000000000\",\"IsIncludingArchiveStaging\":null,\"IsIncludingArchive\":null,\"IsIncludingMessageBox\":null,\"SearchType\":null,\"action\":\"Query\"}",
				jsonResult);
		}

		[TestMethod]
		public void MessageController_QueryPostRedirectWithoutDefaultValues()
		{
			var query = new Query();
			MessageController_QueryPostTest(messagesController, query, "{\"Role\":null,\"AnyRole\":null,\"DateFilterType\":null,\"FromPeriodType\":null,\"ToPeriodType\":null,\"FromPeriodValue\":null,\"ToPeriodValue\":null,\"SenderInbox\":null,\"RecipientInbox\":null,\"SenderOutbox\":null,\"RecipientOutbox\":null,\"To\":null,\"ToDate\":null,\"ToTime\":null,\"From\":null,\"FromDate\":null,\"FromTime\":null,\"TimeZoneIdIANA\":null,\"DateRangeType\":null,\"Received\":null,\"Status\":null,\"ApplicationFull\":null,\"ApplicationCode\":null,\"Refresh\":null,\"KeyType\":null,\"Key\":null,\"IsIncludingArchiveStaging\":null,\"IsIncludingArchive\":null,\"IsIncludingMessageBox\":null,\"SearchType\":null,\"action\":\"Query\"}");

			query = new Query() { IsIncludingArchive = null, IsIncludingMessageBox = null, IsIncludingArchiveStaging = true };
			MessageController_QueryPostTest(messagesController, query, "{\"Role\":null,\"AnyRole\":null,\"DateFilterType\":null,\"FromPeriodType\":null,\"ToPeriodType\":null,\"FromPeriodValue\":null,\"ToPeriodValue\":null,\"SenderInbox\":null,\"RecipientInbox\":null,\"SenderOutbox\":null,\"RecipientOutbox\":null,\"To\":null,\"ToDate\":null,\"ToTime\":null,\"From\":null,\"FromDate\":null,\"FromTime\":null,\"TimeZoneIdIANA\":null,\"DateRangeType\":null,\"Received\":null,\"Status\":null,\"ApplicationFull\":null,\"ApplicationCode\":null,\"Refresh\":null,\"KeyType\":null,\"Key\":null,\"IsIncludingArchiveStaging\":null,\"IsIncludingArchive\":false,\"IsIncludingMessageBox\":false,\"SearchType\":null,\"action\":\"Query\"}");

			query = new Query() { IsIncludingArchive = null, IsIncludingMessageBox = true, IsIncludingArchiveStaging = null };
			MessageController_QueryPostTest(messagesController, query, "{\"Role\":null,\"AnyRole\":null,\"DateFilterType\":null,\"FromPeriodType\":null,\"ToPeriodType\":null,\"FromPeriodValue\":null,\"ToPeriodValue\":null,\"SenderInbox\":null,\"RecipientInbox\":null,\"SenderOutbox\":null,\"RecipientOutbox\":null,\"To\":null,\"ToDate\":null,\"ToTime\":null,\"From\":null,\"FromDate\":null,\"FromTime\":null,\"TimeZoneIdIANA\":null,\"DateRangeType\":null,\"Received\":null,\"Status\":null,\"ApplicationFull\":null,\"ApplicationCode\":null,\"Refresh\":null,\"KeyType\":null,\"Key\":null,\"IsIncludingArchiveStaging\":false,\"IsIncludingArchive\":false,\"IsIncludingMessageBox\":null,\"SearchType\":null,\"action\":\"Query\"}");

			query = new Query() { DateFilterType = "WithinPeriod", FromPeriodType = "Hour", ToPeriodType = "Hour", ToPeriodValue = 0, FromPeriodValue = 1 };
			MessageController_QueryPostTest(messagesController, query, "{\"Role\":null,\"AnyRole\":null,\"DateFilterType\":null,\"FromPeriodType\":null,\"ToPeriodType\":null,\"FromPeriodValue\":null,\"ToPeriodValue\":null,\"SenderInbox\":null,\"RecipientInbox\":null,\"SenderOutbox\":null,\"RecipientOutbox\":null,\"To\":null,\"ToDate\":null,\"ToTime\":null,\"From\":null,\"FromDate\":null,\"FromTime\":null,\"TimeZoneIdIANA\":null,\"DateRangeType\":null,\"Received\":null,\"Status\":null,\"ApplicationFull\":null,\"ApplicationCode\":null,\"Refresh\":null,\"KeyType\":null,\"Key\":null,\"IsIncludingArchiveStaging\":null,\"IsIncludingArchive\":null,\"IsIncludingMessageBox\":null,\"SearchType\":null,\"action\":\"Query\"}");
		}

		[TestMethod]
		public void MessageController_ShowQueryOnLandingPage()
		{
			SetupTestData();

			var query = new Query();
			var result = messagesController.Query(query) as ViewResult;
			Assert.AreEqual(result.ViewBag.ShowQuery, true);
		}

		[TestMethod]
		public void MessageController_NotShowQueryOnLandingPageIfThereIsQuery()
		{
			var mockqueryString = MockRepository.GenerateMock<NameValueCollection>();
			var mockRequest = MockRepository.GenerateMock<HttpRequestBase>();
			mockRequest.Stub(x => x.QueryString).Return(mockqueryString);
			mockqueryString.Stub(x => x.Count).Return(1);

			var mockResponse = MockRepository.GenerateMock<HttpResponseBase>();
			var mockHttpSessionStateBase = MockRepository.GenerateMock<HttpSessionStateBase>();
			var mockHttpContext = MockRepository.GenerateMock<HttpContextBase>();
			mockHttpContext.Stub(x => x.Request).Return(mockRequest);
			mockHttpContext.Stub(x => x.Response).Return(mockResponse);
			mockHttpContext.Stub(x => x.Session).Return(mockHttpSessionStateBase);

			messagesController.ControllerContext = new ControllerContext(mockHttpContext, new RouteData(), messagesController);

			SetupTestData();

			var query = new Query();
			var result = messagesController.Query(query) as ViewResult;
			Assert.IsNull(result.ViewBag.ShowQuery);
		}

		[TestMethod]
		public void MessageController_ListShowLoadMoreButtonForArchive()
        {
			SetupTestData();

			var query = new Query() { IsIncludingMessageBox = false, IsIncludingArchive = true, IsIncludingArchiveStaging = false };
			var result = messagesController.List(query, null) as PartialViewResult;

			Assert.IsTrue(result.ViewBag.ShowLoadMore, "Should Show Load More Button");
		}


		[TestMethod]
		public void MessageController_ListNotShowLoadMoreButtonForArchive()
		{
			SetupTestDataForSinglePage();

			var query = new Query() { IsIncludingMessageBox = false, IsIncludingArchive = true, IsIncludingArchiveStaging = false };
			var result = messagesController.List(query, null) as PartialViewResult;

			Assert.IsFalse(result.ViewBag.ShowLoadMore, "Should NOT Show Load More Button");
		}

		static void MessageController_QueryPostTest(MessagesController controller, Query query, string expected)
		{
			var resultRedirect = controller.QueryPost(query) as RedirectToRouteResult;
			Assert.IsNotNull(resultRedirect);
			Assert.AreEqual("", resultRedirect.RouteName);
			var actualResult = JsonConvert.SerializeObject(resultRedirect.RouteValues, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			Assert.AreEqual(expected, actualResult);
		}

		[TestMethod]
		public void MessagesController_List_Empty()
		{
			var result = messagesController.List(new Query(), null) as PartialViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual("_List", result.ViewName);
			Assert.AreEqual(new DateTime(2015, 2, 2, 4, 15, 0), result.ViewBag.SearchLimit);
			Assert.AreEqual(false, result.ViewBag.ShowLoadMore);
			var resultModel = result.Model as List<InboxMessage>;
			Assert.IsNotNull(resultModel);
			Assert.AreEqual(0, resultModel.Count);
		}

		[TestMethod]
		public void MessagesController_List_ArchiveStaging_Filtered()
		{
			SetupTestData();

			var query = new Query();
			var expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 4, 0, 5),
				ShowLoadMore = true,
				Count = 50
			};
			var searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page1");

			query = new Query();
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 38, 10),
				ShowLoadMore = true,
				Count = 50
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query() { IsIncludingArchiveStaging = false };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 25, 15),
				ShowLoadMore = true,
				Count = 50
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_NoArchiveStaging_Page2", ((DateTime)searchLimit).ToString("o"));
		}

		[TestMethod]
		public void MessageController_List_MessageBox_Filtered()
		{
			SetupTestData();

			var query = new Query();
			var expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 4, 0, 5),
				ShowLoadMore = true,
				Count = 50
			};
			var searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page1");

			query = new Query();
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 38, 10),
				ShowLoadMore = true,
				Count = 50
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query() { IsIncludingMessageBox = false };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 27, 15),
				ShowLoadMore = true,
				Count = 50
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_NoMessageBox_Page2", ((DateTime)searchLimit).ToString("o"));
		}

		[TestMethod]
		public void MessageController_List_Archive_Filtered()
		{
			SetupTestData();

			var query = new Query();
			var expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 4, 0, 5),
				ShowLoadMore = true,
				Count = 50
			};
			var searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page1");

			query = new Query();
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 38, 10),
				ShowLoadMore = true,
				Count = 50
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query() { IsIncludingArchive = false };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 35, 05),
				ShowLoadMore = true,
				Count = 50
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_NoArchive_Page2", ((DateTime)searchLimit).ToString("o"));
		}

		[TestMethod]
		public void MessagesController_List_Filtered()
		{
			SetupTestData();

			/* No Filter */
			var query = new Query();
			var expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 4, 0, 5),
				ShowLoadMore = true,
				Count = 50
			};
			var searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page1");

			query = new Query();
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 38, 10),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query();
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 17, 15),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page3", ((DateTime)searchLimit).ToString("o"));

			query = new Query();
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 2, 45, 15),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_NoFilter_Page4", ((DateTime)searchLimit).ToString("o"));

			query = new Query();
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 2, 45, 15),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Any Role */
			query = new Query { AnyRole = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 57, 5),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_AnyRole_Page1");

			query = new Query { AnyRole = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 33, 10),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_AnyRole_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query { AnyRole = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 5, 10),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_AnyRole_Page3", ((DateTime)searchLimit).ToString("o"));

			query = new Query { AnyRole = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 3, 05, 10),
				ShowLoadMore = false,
				Count = 20
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_AnyRole_Page4", ((DateTime)searchLimit).ToString("o"));

			query = new Query { AnyRole = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 3, 05, 10),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Sender Inbox */
			query = new Query { SenderInbox = "3333" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 40
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_SenderInbox_Page1");

			query = new Query { SenderInbox = "3333" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Recipient Inbox */
			query = new Query { RecipientInbox = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 21, 15),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_RecipientInbox_Page1");

			query = new Query { RecipientInbox = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 3, 21, 15),
				ShowLoadMore = false,
				Count = 20
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_RecipientInbox_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query { RecipientInbox = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 3, 21, 15),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Sender Outbox */
			query = new Query { SenderOutbox = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 30
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_SenderOutbox_Page1");

			query = new Query { SenderOutbox = "2222" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Recipient Outbox */
			query = new Query { RecipientOutbox = "3333" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 8, 10),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_RecipientOutbox_Page1");

			query = new Query { RecipientOutbox = "3333" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 3, 8, 10),
				ShowLoadMore = false,
				Count = 10
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_RecipientOutbox_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query { RecipientOutbox = "3333" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 3, 8, 10),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Application Code */
			query = new Query { ApplicationCode = "BBB" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 7, 15),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_ApplicationCode_Page1");

			query = new Query { ApplicationCode = "BBB" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 3, 7, 15),
				ShowLoadMore = false,
				Count = 10
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_ApplicationCode_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query { ApplicationCode = "BBB" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 3, 7, 15),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Received */
			query = new Query { Status = "Received" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 10
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusReceived_Page1");

			query = new Query { Status = "Received" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Processing */
			query = new Query { Status = "Processing" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 10
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusProcessing_Page1");

			query = new Query { Status = "Processing" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Not Processed */
			query = new Query { Status = "Not Processed" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 20
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusNotProcessed_Page1");

			query = new Query { Status = "Not Processed" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Processed */
			query = new Query { Status = "Processed" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 10
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusProcessed_Page1");

			query = new Query { Status = "Processed" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Sending */
			query = new Query { Status = "Sending" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 20
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusSending_Page1");

			query = new Query { Status = "Sending" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Not Delivered */
			query = new Query { Status = "Not Delivered" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 43, 5),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusNotDelivered_Page1");

			query = new Query { Status = "Not Delivered" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 3, 43, 5),
				ShowLoadMore = false,
				Count = 10
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusNotDelivered_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query { Status = "Not Delivered" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 3, 43, 5),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Delivered */
			query = new Query { Status = "Delivered" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 0, 10),
				ShowLoadMore = true,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusDelivered_Page1");


			query = new Query { Status = "Delivered" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 3, 0, 10),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* Status Error */
			query = new Query { Status = "Error" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 4, 15, 0),
				ShowLoadMore = false,
				Count = 40
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_StatusError_Page1");

			query = new Query { Status = "Error" };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 4, 15, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* To */
			query = new Query { ToDate = new DateTime(2015, 2, 3), ToTime = new TimeSpan(14, 10, 0) };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 2, 3, 10, 0),
				ShowLoadMore = false,
				Count = 35
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_Before_Page1");

			query = new Query { ToDate = new DateTime(2015, 2, 3), ToTime = new TimeSpan(4, 10, 0) };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 1, 3, 10, 0),
				ShowLoadMore = false,
				Count = 0
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, string.Empty, ((DateTime)searchLimit).ToString("o"));

			/* From */
			query = new Query { FromDate = new DateTime(2015, 2, 3), FromTime = new TimeSpan(14, 10, 0) };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 4, 0, 5),
				ShowLoadMore = false,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_After_Page1");

			query = new Query { FromDate = new DateTime(2015, 2, 3), FromTime = new TimeSpan(14, 10, 0) };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 38, 10),
				ShowLoadMore = false,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_After_Page2", ((DateTime)searchLimit).ToString("o"));

			query = new Query { FromDate = new DateTime(2015, 2, 3), FromTime = new TimeSpan(14, 10, 0) };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 3, 17, 15),
				ShowLoadMore = false,
				Count = 50
			};
			searchLimit = MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_After_Page3", ((DateTime)searchLimit).ToString("o"));

			query = new Query { FromDate = new DateTime(2015, 2, 3), FromTime = new TimeSpan(14, 10, 0) };
			var actualResult = messagesController.List(query, ((DateTime)searchLimit).ToString("o")) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);
			var resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(15, resultModel.Count);
			var jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_After_Page4.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			/* From To */
			query = new Query { FromDate = new DateTime(2015, 2, 2), FromTime = new TimeSpan(4, 10, 0), ToDate = new DateTime(2015, 3, 2), ToTime = new TimeSpan(4, 10, 0) };
			expectedResult = new
			{
				SearchLimit = new DateTime(2015, 2, 3, 4, 0, 5),
				ShowLoadMore = false,
				Count = 50
			};
			MessagesController_List_FilteredTest(messagesController, query, expectedResult, "MessageController_List_From_To_Page1");

			/* Period - 1 Hour (Default) */
			query = new Query { FromPeriodValue = 1, FromPeriodType = "Day", DateFilterType = "WithinPeriod" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(50, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Period_Hour_Page1.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			/* Period - From To*/
			query = new Query { FromPeriodValue = 5, FromPeriodType = "Hour", ToPeriodValue = 1, ToPeriodType = "Hour", DateFilterType = "WithinPeriod" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(45, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Period_FromTo_4Hour_Page.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			/* EI_PK */
			query = new Query { KeyType = "EI_PK", Key = new Guid(2, 1, 5, new byte[8]) };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Key_EI_PK.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			/* OI_PK */
			query = new Query { KeyType = "OI_PK", Key = new Guid(1, 2, 5, new byte[8]) };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Key_OI_PK.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			/* AM_PK */
			query = new Query { KeyType = "AM_PK", Key = new Guid(3, 3, 5, new byte[8]) };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Key_AM_PK.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			/* MsgID */
			query = new Query { KeyType = "MsgID", Key = new Guid(1, 16, 5, new byte[8]) };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Key_MsgID_1.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);

			query = new Query { KeyType = "MsgID", Key = new Guid(2, 17, 4, new byte[8]) };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Key_MsgID_2.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);

			query = new Query { KeyType = "MsgID", Key = new Guid(3, 17, 5, new byte[8]) };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);
			resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, resultModel.Count);
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_List_Key_MsgID_3.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
		}

		[TestMethod]
		public void TestSydneyDST()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
			var utcTime1 = new DateTime(2021, 4, 3, 15, 0, 0, DateTimeKind.Utc);
			var utcTime2 = new DateTime(2021, 4, 3, 15, 59, 59, DateTimeKind.Utc);
			var utcTime3 = new DateTime(2021, 4, 3, 16, 0, 0, DateTimeKind.Utc);
			var utcTime4 = new DateTime(2021, 4, 3, 16, 59, 59, DateTimeKind.Utc);
			var utcTime5 = new DateTime(2021, 4, 3, 17, 0, 0, DateTimeKind.Utc);

			Assert.AreEqual(new TimeSpan(11, 0, 0), timeZone.GetUtcOffset(utcTime1));
			Assert.AreEqual(new TimeSpan(11, 0, 0), timeZone.GetUtcOffset(utcTime2));
			Assert.AreEqual(new TimeSpan(10, 0, 0), timeZone.GetUtcOffset(utcTime3));
			Assert.AreEqual(new TimeSpan(10, 0, 0), timeZone.GetUtcOffset(utcTime4));
			Assert.AreEqual(new TimeSpan(10, 0, 0), timeZone.GetUtcOffset(utcTime5));

			Assert.AreEqual(new DateTime(2021, 4, 4, 2, 0, 0), TimeZoneInfo.ConvertTimeFromUtc(utcTime1, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 4, 2, 59, 59), TimeZoneInfo.ConvertTimeFromUtc(utcTime2, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 4, 2, 0, 0), TimeZoneInfo.ConvertTimeFromUtc(utcTime3, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 4, 2, 59, 59), TimeZoneInfo.ConvertTimeFromUtc(utcTime4, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 4, 3, 0, 0), TimeZoneInfo.ConvertTimeFromUtc(utcTime5, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 4, 3, 0, 0), TimeZoneInfo.ConvertTimeFromUtc(utcTime5, timeZone));

			var localTime1 = new DateTime(2021, 4, 4, 1, 59, 59);
			var localTime2 = new DateTime(2021, 4, 4, 2, 0, 0);
			var localTime3 = new DateTime(2021, 4, 4, 2, 59, 59);
			var localTime4 = new DateTime(2021, 4, 4, 3, 0, 0);
			Assert.AreEqual(new DateTime(2021, 4, 3, 14, 59, 59), TimeZoneInfo.ConvertTimeToUtc(localTime1, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 3, 16, 0, 0), TimeZoneInfo.ConvertTimeToUtc(localTime2, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 3, 16, 59, 59), TimeZoneInfo.ConvertTimeToUtc(localTime3, timeZone));
			Assert.AreEqual(new DateTime(2021, 4, 3, 17, 0, 0), TimeZoneInfo.ConvertTimeToUtc(localTime4, timeZone));
		}

		[TestMethod]
		public void MessagesController_List_TimeZoneIdIANA()
		{
			SetupDataForTimeZoneTest();

			var query = new Query { ToDate = new DateTime(2021, 4, 4), ToTime = new TimeSpan(2, 10, 0), DateRangeType = "Local" };
			Assert.AreEqual(query.TimeZoneIdIANA, "Australia/Sydney", "Should have default value");

			var actualResult = messagesController.List(query, null) as PartialViewResult;
			var actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(5, actualResultModel.Count);

			query = new Query { ToDate = new DateTime(2021, 4, 4), ToTime = new TimeSpan(2, 10, 0), DateRangeType = "Local", TimeZoneIdIANA = "Asia/Taipei" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(8, actualResultModel.Count);
		}

		[TestMethod]
		public void MessagesController_List_DateRangeType_Local_AmbiguousTimeWillBeConvertedToUtcUsingStandardTime()
		{
			SetupDataForTimeZoneTest();

			// To
			var query = new Query { ToDate = new DateTime(2021, 4, 4), ToTime = new TimeSpan(1, 10, 0), DateRangeType = "Local" };
			var actualResult = messagesController.List(query, null) as PartialViewResult;
			var actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, actualResultModel.Count);

			query = new Query { ToDate = new DateTime(2021, 4, 4), ToTime = new TimeSpan(2, 10, 0), DateRangeType = "Local" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(5, actualResultModel.Count);

			query = new Query { ToDate = new DateTime(2021, 4, 4), ToTime = new TimeSpan(3, 10, 0), DateRangeType = "Local" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(7, actualResultModel.Count);

			// From
			query = new Query { FromDate = new DateTime(2021, 4, 4), FromTime = new TimeSpan(1, 10, 0), DateRangeType = "Local" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(7, actualResultModel.Count);

			query = new Query { FromDate = new DateTime(2021, 4, 4), FromTime = new TimeSpan(2, 10, 0), DateRangeType = "Local" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(3, actualResultModel.Count);

			query = new Query { FromDate = new DateTime(2021, 4, 4), FromTime = new TimeSpan(3, 10, 0), DateRangeType = "Local" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, actualResultModel.Count);
		}

		[TestMethod]
		public void MessagesController_List_DateRangeType_Utc()
		{
			SetupDataForTimeZoneTest();

			// To
			var query = new Query { ToDate = new DateTime(2021, 4, 3), ToTime = new TimeSpan(14, 10, 0), DateRangeType = "UTC" };
			var actualResult = messagesController.List(query, null) as PartialViewResult;
			var actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, actualResultModel.Count);

			query = new Query { ToDate = new DateTime(2021, 4, 3), ToTime = new TimeSpan(15, 10, 0), DateRangeType = "UTC" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(3, actualResultModel.Count);

			query = new Query { ToDate = new DateTime(2021, 4, 3), ToTime = new TimeSpan(16, 10, 0), DateRangeType = "UTC" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(5, actualResultModel.Count);

			// From
			query = new Query { FromDate = new DateTime(2021, 4, 3), FromTime = new TimeSpan(14, 10, 0), DateRangeType = "UTC" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(7, actualResultModel.Count);

			query = new Query { FromDate = new DateTime(2021, 4, 3), FromTime = new TimeSpan(15, 10, 0), DateRangeType = "UTC" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(5, actualResultModel.Count);

			query = new Query { FromDate = new DateTime(2021, 4, 3), FromTime = new TimeSpan(16, 10, 0), DateRangeType = "UTC" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(3, actualResultModel.Count);
		}

		[TestMethod]
		public void MessagesController_List_DateRangeType_NullAndInvalidValues_ShouldTreatAsLocal()
		{
			SetupDataForTimeZoneTest();

			var query = new Query { ToDate = new DateTime(2021, 4, 4), ToTime = new TimeSpan(2, 10, 0) };
			var actualResult = messagesController.List(query, null) as PartialViewResult;
			var actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(5, actualResultModel.Count);
			Assert.IsNull(query.DateRangeType);

			query = new Query { ToDate = new DateTime(2021, 4, 4), ToTime = new TimeSpan(2, 10, 0), DateRangeType = "Invalid Type" };
			actualResult = messagesController.List(query, null) as PartialViewResult;
			actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(5, actualResultModel.Count);
			Assert.AreEqual("Invalid Type", query.DateRangeType);
		}

		void SetupDataForTimeZoneTest()
		{
			var cc1 = testClients.Add(new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "111111111" });
			var cc2 = testClients.Add(new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "222222222" });

			var insertUtc = new DateTime(2021, 4, 3, 13, 59, 59);
			for (var i = 0; i < 8; i++)
			{
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = Guid.NewGuid(),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = insertUtc,
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = Guid.NewGuid().ToString(),
					EI_Status = 0,
					EI_MessageType = "TestMessageType"
				});
				insertUtc = insertUtc.AddMinutes(30);
			}
		}

		[TestMethod]
		public void MessageController_List_MultipleArchivedOutbox()
		{
			var cc1 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "111111111" });
			var cc2 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "222222222" });
			var cc3 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "333333333" });
			var cc4 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "444444444" });

			testArchiveSecondaryMessages.Add(new eHubArchiveMessage
			{
				AM_PK = GetPk(4, 3, 1),
				AM_ReceivedFromSenderUTC = new DateTime(2015, 1, 2),
				eHubClient_SenderInbox = cc1,
				eHubClient_RecipientInbox = cc2,
				eHubClient_SenderOutbox = cc3,
				eHubClient_RecipientOutbox = cc4,
				AM_ApplicationCode = "ZZZ",
				AM_InboxMessageTrackingID = GetPk(4, 16, 1),
				AM_OutboxMessageTrackingID = GetPk(4, 17, 1),
				AM_Status = 3,
				AM_EI_PK = GetPk(4, 1, 1),
				AM_SenderMessageRaw = ".",
			});

			testArchiveSecondaryMessages.Add(new eHubArchiveMessage
			{
				AM_PK = GetPk(4, 3, 2),
				AM_ReceivedFromSenderUTC = new DateTime(2015, 1, 2),
				eHubClient_SenderInbox = cc1,
				eHubClient_RecipientInbox = cc2,
				eHubClient_SenderOutbox = cc3,
				eHubClient_RecipientOutbox = cc4,
				AM_ApplicationCode = "ZZZ",
				AM_InboxMessageTrackingID = GetPk(4, 16, 2),
				AM_OutboxMessageTrackingID = GetPk(4, 17, 2),
				AM_Status = 3,
				AM_EI_PK = GetPk(4, 1, 1),
				AM_SenderMessageRaw = ".",
			});

			var query = new Query { KeyType = "AM_PK", Key = GetPk(4, 3, 1), IsIncludingArchive = true };
			var actualResult = messagesController.List(query, null) as PartialViewResult;
			Assert.IsNull(actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(false, actualResult.ViewBag.ShowLoadMore);

			var resultModel = actualResult.Model as List<InboxMessage>;
			Assert.AreEqual(1, resultModel.Count);
			Assert.AreEqual(2, resultModel[0].OutboxMessages.Count());
		}

		static object MessagesController_List_FilteredTest(MessagesController controller, Query query, dynamic expectedResult, string manifestResourceStream, string searchLimit = null, bool writeResultToFile = false)
		{
			var actualResult = controller.List(query, searchLimit) as PartialViewResult;

			Assert.IsNotNull(actualResult);
			Assert.AreEqual(expectedResult.SearchLimit, actualResult.ViewBag.SearchLimit);
			Assert.AreEqual(expectedResult.ShowLoadMore, actualResult.ViewBag.ShowLoadMore);

			var actualResultModel = actualResult.Model as List<InboxMessage>;
			Assert.IsNotNull(actualResultModel);
			Assert.AreEqual(expectedResult.Count, actualResultModel.Count);

			var jsonResult = JsonConvert.SerializeObject(actualResultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			if (writeResultToFile)
			{
				File.WriteAllText("D:\\Test\\" + manifestResourceStream + ".txt", jsonResult);
			}
			if (string.IsNullOrEmpty(manifestResourceStream))
			{
				Assert.AreEqual("[]", jsonResult);
			}
			else
			{
				using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles." + manifestResourceStream + ".txt"))
				using (var expectedJsonReader = new StreamReader(expectedJsonStream))
				{
					Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
				}
			}
			return actualResult.ViewBag.SearchLimit;
		}

		[TestMethod]
		public void MessageController_Details()
		{
			SetupTestData();

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EI_Content FROM eHubInboxMessage WHERE EI_PK = @pk"), Arg.Is(new Guid("00000001-0001-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(100);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk"), Arg.Is(new Guid("00000001-0001-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(50);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_XmlContent FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000001-0002-0000-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(250);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000001-0002-0000-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(500);

			var result = messagesController.Details(GetPk(1, 1, 4), null, null) as ViewResult;

			var resultModel = result.Model as InboxMessage;
			var jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_Live.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EI_Content FROM eHubInboxMessageArchive WHERE EI_PK = @pk"), Arg.Is(new Guid("00000002-0001-0005-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(100);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk"), Arg.Is(new Guid("00000002-0001-0005-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(50);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_XmlContent FROM eHubOutboxMessageArchive WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(250);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_Content FROM eHubOutboxMessageArchive WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(500);

			result = messagesController.Details(GetPk(2, 1, 5), null, null) as ViewResult;

			resultModel = result.Model as InboxMessage;
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_Moved.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			MessageController_Details_Archive(stubeHubArchiveOnlineSecondaryContext);
		}

		void MessageController_Details_Archive(eHubArchiveOnlineContext archiveContext)
		{
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_SenderMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0000-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(100);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_SenderMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0000-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(50);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_RecipientMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0000-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(250);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_RecipientMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0000-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(500);

			var result = messagesController.Details(GetPk(3, 1, 0), null, null) as ViewResult;

			var resultModel = result.Model as InboxMessage;
			var jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_Archived_EI.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			result = messagesController.Details(GetPk(3, 1, 0), null, new DateTime(2015, 2, 3, 2, 45, 15).ToBinary()) as ViewResult;

			resultModel = result.Model as InboxMessage;
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_Archived_EI.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_SenderMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(100);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_SenderMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(50);

			result = messagesController.Details(null, GetPk(3, 3, 1), null) as ViewResult;

			resultModel = result.Model as InboxMessage;
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_Archived_Error.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}

			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_SenderMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(100);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_SenderMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(50);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_RecipientMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(250);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_RecipientMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(500);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_RecipientMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0005-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(250);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(archiveContext), Arg.Is("SELECT AM_RecipientMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0005-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(500);

			result = messagesController.Details(null, GetPk(3, 3, 5), null) as ViewResult;

			resultModel = result.Model as InboxMessage;
			jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_Archived_AM.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}
		}

		[TestMethod]
		public void MessageController_Details_MultipleOutboxError()
		{
			var cc1 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "111111111" });
			var inboxPK = GetPk(2, 1, 7);
			var outboxPK = GetPk(2, 2, 7);

			var error = testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
			{
				EI_PK = inboxPK,
				eHubClient_Sender = cc1,
				eHubClient_Recipient = cc1,
				EI_InsertUTC = new DateTime(2015, 2, 3, 3, 20, 5),
				EI_ApplicationCode = "BBB",
				EI_MessageTrackingID = GetPk(2, 16, 7).ToString(),
				EI_Status = 255,
				eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
												new eHubOutboxMessageDisplay {
														OI_PK = outboxPK,
														eHubClient_Sender = cc1,
														eHubClient_Recipient = cc1,
														OI_InsertUTC = DateTime.MaxValue,
														OI_MessageTrackingID = GetPk(2, 17, 7).ToString(),
														OI_Status = 255
												}
										}).ToList()
			});
			testErrors.Add(new eHubError
			{
				EE_EI_Inbox = GetPk(2, 1, 6),
				EE_OI_Outbox = error.eHubOutboxMessageDisplays[0].OI_PK,
				EE_Description = "ERROR DESCRIPTION"
			});
			testErrors.Add(new eHubError
			{
				EE_EI_Inbox = error.EI_PK,
				EE_OI_Outbox = error.eHubOutboxMessageDisplays[0].OI_PK,
				EE_Description = "ERROR DESCRIPTION"
			});

			var inbox = testInboxMessageDisplays.FirstOrDefault(i => i.EI_PK == inboxPK);
			var outbox = testOutboxMessageDisplays.FirstOrDefault(o => o.OI_PK == outboxPK);
			outbox.eHubInboxMessageDisplay = inbox;
			outbox.OI_EI_InboxPK = inbox.EI_PK;

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EI_Content FROM eHubInboxMessage WHERE EI_PK = @pk"), Arg.Is(new Guid("00000002-0001-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(100);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk"), Arg.Is(new Guid("00000002-0001-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(50);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_XmlContent FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(250);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(500);

			var result = messagesController.Details(inboxPK, null, null) as ViewResult;

			var resultModel = result.Model as InboxMessage;
			var jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_MultipleOutboxError.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}
		}

		[TestMethod]
		public void MessageController_Details_MultipleOutboxWithOnlyOneError()
		{
			var cc1 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "111111111" });
			var inboxPK = GetPk(2, 1, 7);
			var outboxPK1 = GetPk(2, 2, 7);
			var outboxPK2 = GetPk(2, 3, 7);

			var error = testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
			{
				EI_PK = inboxPK,
				eHubClient_Sender = cc1,
				eHubClient_Recipient = cc1,
				EI_InsertUTC = new DateTime(2015, 2, 3, 3, 20, 5),
				EI_ApplicationCode = "BBB",
				EI_MessageTrackingID = GetPk(2, 16, 7).ToString(),
				EI_Status = 255,
				eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
												new eHubOutboxMessageDisplay {
														OI_PK = outboxPK1,
														eHubClient_Sender = cc1,
														eHubClient_Recipient = cc1,
														OI_InsertUTC = DateTime.MaxValue,
														OI_MessageTrackingID = GetPk(2, 17, 7).ToString(),
														OI_Status = 255
												},
												new eHubOutboxMessageDisplay {
													OI_PK = outboxPK2,
													eHubClient_Sender = cc1,
													eHubClient_Recipient = cc1,
													OI_InsertUTC = DateTime.MaxValue,
													OI_MessageTrackingID = GetPk(2, 17, 7).ToString(),
													OI_Status = 3
												}
										}).ToList()
			});
			testErrors.Add(new eHubError
			{
				EE_EI_Inbox = GetPk(2, 1, 6),
				EE_OI_Outbox = error.eHubOutboxMessageDisplays[0].OI_PK,
				EE_Description = "ERROR DESCRIPTION"
			});
			testErrors.Add(new eHubError
			{
				EE_EI_Inbox = error.EI_PK,
				EE_OI_Outbox = error.eHubOutboxMessageDisplays[0].OI_PK,
				EE_Description = "ERROR DESCRIPTION"
			});

			var inbox = testInboxMessageDisplays.FirstOrDefault(i => i.EI_PK == inboxPK);
			var outbox1 = testOutboxMessageDisplays.FirstOrDefault(o => o.OI_PK == outboxPK1);
			outbox1.eHubInboxMessageDisplay = inbox;
			outbox1.OI_EI_InboxPK = inbox.EI_PK;

			var outbox2 = testOutboxMessageDisplays.FirstOrDefault(o => o.OI_PK == outboxPK2);
			outbox2.eHubInboxMessageDisplay = inbox;
			outbox2.OI_EI_InboxPK = inbox.EI_PK;

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EI_Content FROM eHubInboxMessage WHERE EI_PK = @pk"), Arg.Is(new Guid("00000002-0001-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(100);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk"), Arg.Is(new Guid("00000002-0001-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(50);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_XmlContent FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(250);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(500);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_XmlContent FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0003-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.Null)).Return(750);
			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0003-0007-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.Null)).Return(800);

			var result = messagesController.Details(inboxPK, null, null) as ViewResult;

			var resultModel = result.Model as InboxMessage;
			var jsonResult = JsonConvert.SerializeObject(resultModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			using (var expectedJsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("eServices.eHubAdmin.Tests.TestFiles.MessageController_Details_MultipleOutboxWithOnlyOneError.txt"))
			using (var expectedJsonReader = new StreamReader(expectedJsonStream))
			{
				Assert.AreEqual(expectedJsonReader.ReadToEnd(), jsonResult);
			}
		}

		[TestMethod]
		public void MessagesController_Download()
		{
			SetupTestData();

			var readBlob = new Func<ContextBase, string, Guid, IDbTransaction, bool, Stream, long?>((context, commandText, pk, tran, decodeAndDecompress, outputStream) =>
			{
				outputStream.Write(Encoding.Default.GetBytes("DOWNLOADSTREAM 1"), 0, 14);
				outputStream.Position = 0;
				return 14;
			});

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EI_Content FROM eHubInboxMessage WHERE EI_PK = @pk"), Arg.Is(new Guid("00000001-0001-0004-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.NotNull)).Do(readBlob);

			var result = messagesController.Download("InboxRaw", GetPk(1, 1, 4), null, null);

			Assert.AreEqual("text/plain; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EI_Content FROM eHubInboxMessageArchive WHERE EI_PK = @pk"), Arg.Is(new Guid("00000002-0001-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("InboxRaw", GetPk(2, 1, 1), null, null);

			Assert.AreEqual("text/plain; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubArchiveOnlineSecondaryContext), Arg.Is("SELECT AM_SenderMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("InboxRaw", null, null, GetPk(3, 3, 1));

			Assert.AreEqual("text/plain; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk"), Arg.Is(new Guid("00000002-0001-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("InboxXml", GetPk(2, 1, 1), null, null);

			Assert.AreEqual("text/xml; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubArchiveOnlineSecondaryContext), Arg.Is("SELECT AM_SenderMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("InboxXml", null, null, GetPk(3, 3, 1));

			Assert.AreEqual("text/xml; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000001-0002-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("OutboxRaw", null, GetPk(1, 2, 1), null);

			Assert.AreEqual("text/plain; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_Content FROM eHubOutboxMessageArchive WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0002-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("OutboxRaw", null, GetPk(2, 2, 2), null);

			Assert.AreEqual("text/plain; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubArchiveOnlineSecondaryContext), Arg.Is("SELECT AM_RecipientMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(true), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("OutboxRaw", null, null, GetPk(3, 3, 1));

			Assert.AreEqual("text/plain; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_XmlContent FROM eHubOutboxMessage WHERE OI_PK = @pk"), Arg.Is(new Guid("00000001-0002-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("OutboxXml", null, GetPk(1, 2, 1), null);

			Assert.AreEqual("text/xml; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubTransactionsContext), Arg.Is("SELECT OI_XmlContent FROM eHubOutboxMessageArchive WHERE OI_PK = @pk"), Arg.Is(new Guid("00000002-0002-0002-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("OutboxXml", null, GetPk(2, 2, 2), null);

			Assert.AreEqual("text/xml; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());

			messagesController.Stub(x => x.ReadBlob(Arg.Is(stubeHubArchiveOnlineSecondaryContext), Arg.Is("SELECT AM_RecipientMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk"), Arg.Is(new Guid("00000003-0003-0001-0000-000000000000")), Arg.Is(stubTransaction), Arg.Is(false), Arg<Stream>.Is.NotNull)).Do(readBlob);

			result = messagesController.Download("OutboxXml", null, null, GetPk(3, 3, 1));

			Assert.AreEqual("text/xml; charset=utf-8", result.ContentType);
			Assert.AreEqual("DOWNLOADSTREAM", new StreamReader(result.FileStream).ReadToEnd());
		}

		[TestMethod]
		public void MessagesController_CreatePartialResult()
		{
			const string shortMessageContent = "<Configuration Name=\"GBCustomsCDS\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\"><Group Type=\"System\" Reference=\"HYEMIK\"><Group Type=\"CDS\" Status=\"VAL\"><Annotations><Item Name=\"TokenType\">Access</Item><Item Name=\"Message\">Access token refreshed OK</Item><Item Name=\"Issued\">2019-06-07T06:12:08Z</Item><Item Name=\"Expires\">2019-06-07T10:12:08Z</Item></Annotations><Item Name=\"MailBoxID\">GB025115100001</Item><Credential Name=\"Current\"><UserName>T01</UserName></Credential></Group></Group></Configuration>";
			const string longMessageContent = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Body><UniversalEvent xmlns =\"http://www.cargowise.com/Schemas/Universal/2012/11\"><Event><DataContext><DataTargetCollection><DataTarget><Type>ForwardingConsol</Type></DataTarget><DataTarget><Type>CustomsDeclaration</Type></DataTarget></DataTargetCollection></DataContext><EventTime>2019-06-07T14:23:00</EventTime><EventType>STU</EventType><EventParameters><Department>Carrier</Department><Location>SYD</Location><Type>Consignee or Agent Notified of Arrival</Type></EventParameters><ContextCollection><Context><Type>MAWBNumber</Type><Value>738-43333990</Value></Context><Context><Type>MAWBOriginIATAAirportCode</Type><Value>CDG</Value></Context><Context><Type>MAWBDestinationIATAAirportCode</Type><Value>SYD</Value></Context><Context><Type>OriginIATAAirportCode</Type><Value>MEL</Value></Context><Context><Type>DestinationIATAAirportCode</Type><Value>SYD</Value></Context><Context><Type>MAWBNumberOfPieces</Type><Value>92</Value></Context><Context><Type>FlightNumber</Type><Value>VN7382</Value></Context><Context><Type>NumberOfPieces</Type><Value>92</Value></Context><Context><Type>WeightOfGoods</Type><Value>854.5KG</Value></Context></ContextCollection></Event></UniversalEvent></Body></UniversalInterchange>";
			const string longPartialMessage = "<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"><Body><UniversalEvent xmlns =\"http://www.cargowise.com/Schemas/Universal/2012/11\"><Event><DataContext><DataTargetCollection><DataTarget><Type>ForwardingConsol</Type></DataTarget><DataTarget><Type>CustomsDeclaration</Type></DataTarget></DataTargetCollection></DataContext><EventTime>2019-06-07T14:23:00</EventTime><EventType>STU</EventType><EventParameters><Department>Carrier</Department><Location>SYD</Location><Type>Consignee or Agent Notified of Arrival</Type></EventParameters><ContextCollection><Context><Type>MAWBNumber</Type><Value>738-43333990</Value></Context><Context><Type>MAWBOriginIATAAirportCode</Type><Value>CDG</Value></Context><Context><Type>MAWBDestinationIATAAirportCode</Type><Value>SYD</Value></Context><Context><Type>OriginIATAAirportCode</Type><Value>MEL</Value></Context><Context><Type>DestinationIATAAirportCode</Type><Value>SYD</Value></Context><Context><Type>MAWBNumberOfPieces</Type><Value>92</Value></Context><Cont";

			var shortResult = messagesController.CreatePartialResult(new MemoryStream(Encoding.ASCII.GetBytes(shortMessageContent)));
			var longResult = messagesController.CreatePartialResult(new MemoryStream(Encoding.ASCII.GetBytes(longMessageContent)));

			using (StreamReader reader = new StreamReader(shortResult))
			{
				Assert.AreEqual(shortMessageContent, reader.ReadToEnd(), "Short Message returned incorrect partial message");
			}

			using (StreamReader reader = new StreamReader(longResult))
			{
				Assert.AreEqual(longPartialMessage, reader.ReadToEnd(), "Long Message returned incorrect partial message");
			}
		}

		[TestMethod]
		public void MessagesController_ReadBlob()
		{
			var stubCommand = MockRepository.GenerateMock<IDbCommand>();
			var stubReader = MockRepository.GenerateMock<IDataReader>();
			var stubParameters = MockRepository.GenerateMock<IDataParameterCollection>();
			stubConnection.Stub(x => x.CreateCommand()).Return(stubCommand);
			stubConnection.Stub(x => x.State).Return(ConnectionState.Closed);
			stubCommand.Stub(x => x.ExecuteReader()).Return(stubReader);
			stubCommand.Stub(x => x.Parameters).Return(stubParameters);
			stubReader.Stub(x => x.Read()).Return(true);
			stubReader.Stub(x => x.IsDBNull(0)).Return(false);

			string blobData = null;
			stubReader.Stub(x => x.GetChars(Arg<int>.Is.Anything, Arg<long>.Is.Anything, Arg<char[]>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything))
				.Do(new Func<int, long, char[], int, int, long>((i, fieldOffset, buffer, bufferoffset, length) =>
				{
					var actualLen = Math.Min(length, blobData.Length - fieldOffset);
					Array.Copy(blobData.ToArray(), fieldOffset, buffer, bufferoffset, actualLen);
					return actualLen;
				}));

			blobData = "H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Ip48//LJ/wN4SOqGBAAAAA==";
			var result = messagesController.ReadBlob(stubeHubTransactionsContext, "COMMAND", Guid.Empty, stubTransaction, true);
			Assert.AreEqual(4, result);

			Stream outputStream = new MemoryStream();
			result = messagesController.ReadBlob(stubeHubTransactionsContext, "COMMAND", Guid.Empty, stubTransaction, true, outputStream);
			Assert.AreEqual(4, result);
			Assert.AreEqual("BLOB", new StreamReader(outputStream).ReadToEnd());

			blobData = "<BLOB/>";
			result = messagesController.ReadBlob(stubeHubTransactionsContext, "COMMAND", Guid.Empty, stubTransaction, false);
			Assert.AreEqual(7, result);

			outputStream = new MemoryStream();
			result = messagesController.ReadBlob(stubeHubTransactionsContext, "COMMAND", Guid.Empty, stubTransaction, false, outputStream);
			Assert.AreEqual(7, result);
			Assert.AreEqual("<BLOB/>", new StreamReader(outputStream).ReadToEnd());
		}

		[TestMethod]
		public void MessageController_Query_BasicSearch()
		{
			MessageController_QueryTest(messagesController, new Query { AnyRole = "AAAA", ApplicationCode = "HUB", Status = "Delivered", DateFilterType = "WithinPeriod", FromPeriodType = "Hour", FromPeriodValue = 1, SearchType = "Basic" }, "{\"Role\":\"Any\",\"AnyRole\":\"AAAA\",\"DateFilterType\":\"WithinPeriod\",\"FromPeriodType\":\"Hour\",\"ToPeriodType\":\"Hour\",\"FromPeriodValue\":1,\"ToPeriodValue\":0,\"TimeZoneIdIANA\":\"Australia/Sydney\",\"Status\":\"Delivered\",\"ApplicationCode\":\"HUB\",\"IsIncludingArchiveStaging\":true,\"IsIncludingArchive\":true,\"IsIncludingMessageBox\":true,\"SearchType\":\"Basic\"}");
		}

		void SetupTestData()
		{
			var cc1 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "111111111" });
			var cc2 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "222222222" });
			var cc3 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "333333333" });

			var start = new DateTime(2015, 2, 3, 3, 30, 5);
			var timeInc = 0;
			var inboxInc = 0;
			var outboxInc = 0;
			var inboxMsgId = 0;
			var outboxMsgId = 0;
			for (var i = 0; i < 10; i++)
			{
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 0,
					EI_MessageType = "TestMessageType"
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc3,
					eHubClient_Recipient = cc1,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 1
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc3,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc2,
					eHubClient_Recipient = cc1,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 255
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 0
						}
					}).ToList()
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 1
						}
					}).ToList()
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						},
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 1
						}}).ToList()
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay {
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc2,
							eHubClient_Recipient = cc1,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						}
					}).ToList()
				});
			}
			foreach (var i in testInboxMessageDisplays)
				foreach (var o in i.eHubOutboxMessageDisplays)
				{
					o.eHubInboxMessageDisplay = i;
					o.OI_EI_InboxPK = i.EI_PK;
				}

			start = new DateTime(2015, 2, 3, 3, 0, 10);
			timeInc = inboxInc = outboxInc = inboxMsgId = outboxMsgId = 0;
			for (var i = 0; i < 10; i++)
			{
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc3,
					eHubClient_Recipient = cc1,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 255
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						}
					}).ToList()
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						} }).ToList()
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						},
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						}
					}).ToList()
				});
				var error = testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 255,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc2,
							eHubClient_Recipient = cc1,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 255
						}
					}).ToList()
				});
				testErrors.Add(new eHubError
				{
					EE_EI_Inbox = error.EI_PK,
					EE_OI_Outbox = error.eHubOutboxMessageArchives[0].OI_PK,
					EE_Description = "ERROR DESCRIPTION"
				});
			}
			foreach (var i in testInboxMessageArchives)
				foreach (var o in i.eHubOutboxMessageArchives)
				{
					o.eHubInboxMessageArchive = i;
					o.OI_EI_InboxPK = i.EI_PK;
				}

			start = new DateTime(2015, 2, 3, 2, 45, 15);
			timeInc = inboxInc = outboxInc = inboxMsgId = outboxMsgId = 0;

			for (var i = 0; i < 10; i++)
			{
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc2,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "BBB",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = "."
				});
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc3,
					eHubClient_RecipientInbox = cc1,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 255,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					AM_ErrorMessage = "ERROR MESSAGE"
				});
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc2,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					eHubClient_SenderOutbox = cc1,
					eHubClient_RecipientOutbox = cc3
				});
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc3,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					eHubClient_SenderOutbox = cc1,
					eHubClient_RecipientOutbox = cc2
				});

				var batchPk = GetPk(3, 1, inboxInc++);
				var batchId = GetPk(3, 16, inboxMsgId++);
				var batchTime = start.AddMinutes(timeInc++);
				{
					testArchiveSecondaryMessages.Add(new eHubArchiveMessage
					{
						AM_PK = GetPk(3, 3, outboxInc++),
						eHubClient_SenderInbox = cc1,
						eHubClient_RecipientInbox = cc3,
						AM_ReceivedFromSenderUTC = batchTime,
						AM_ApplicationCode = "BBB",
						AM_InboxMessageTrackingID = batchId,
						AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
						AM_Status = 3,
						AM_EI_PK = batchPk,
						AM_SenderMessageRaw = ".",
						eHubClient_SenderOutbox = cc1,
						eHubClient_RecipientOutbox = cc2
					});
					testArchiveSecondaryMessages.Add(new eHubArchiveMessage
					{
						AM_PK = GetPk(3, 3, outboxInc++),
						eHubClient_SenderInbox = cc1,
						eHubClient_RecipientInbox = cc3,
						AM_ReceivedFromSenderUTC = batchTime,
						AM_ApplicationCode = "BBB",
						AM_InboxMessageTrackingID = batchId,
						AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
						AM_Status = 3,
						AM_EI_PK = batchPk,
						AM_SenderMessageRaw = null,
						eHubClient_SenderOutbox = cc1,
						eHubClient_RecipientOutbox = cc3
					});
				}

				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc3,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					eHubClient_SenderOutbox = cc2,
					eHubClient_RecipientOutbox = cc1
				});
			}
		}

		void SetupTestDataForSinglePage()
		{
			var cc1 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "111111111" });
			var cc2 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "222222222" });
			var cc3 = testClients.Add(new eHubClient { CC_PK = Guid.Empty, CC_ID = "333333333" });

			var start = new DateTime(2015, 2, 3, 3, 30, 5);
			var timeInc = 0;
			var inboxInc = 0;
			var outboxInc = 0;
			var inboxMsgId = 0;
			var outboxMsgId = 0;
			for (var i = 0; i < 5; i++)
			{
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 0,
					EI_MessageType = "TestMessageType"
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc3,
					eHubClient_Recipient = cc1,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 1
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc3,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc2,
					eHubClient_Recipient = cc1,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 255
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 0
						}
					}).ToList()
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 1
						}
					}).ToList()
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 2,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						},
						new eHubOutboxMessageDisplay
						{
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 1
						}}).ToList()
				});
				testInboxMessageDisplays.Add(new eHubInboxMessageDisplay
				{
					EI_PK = GetPk(1, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(1, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageDisplays = testOutboxMessageDisplays.AddRange(new[] {
						new eHubOutboxMessageDisplay {
							OI_PK = GetPk(1, 2, outboxInc++),
							eHubClient_Sender = cc2,
							eHubClient_Recipient = cc1,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(1, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						}
					}).ToList()
				});
			}
			foreach (var i in testInboxMessageDisplays)
				foreach (var o in i.eHubOutboxMessageDisplays)
				{
					o.eHubInboxMessageDisplay = i;
					o.OI_EI_InboxPK = i.EI_PK;
				}

			start = new DateTime(2015, 2, 3, 3, 0, 10);
			timeInc = inboxInc = outboxInc = inboxMsgId = outboxMsgId = 0;
			for (var i = 0; i < 5; i++)
			{
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc3,
					eHubClient_Recipient = cc1,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 255
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc2,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						}
					}).ToList()
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						} }).ToList()
				});
				testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "BBB",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 3,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc2,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						},
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc1,
							eHubClient_Recipient = cc3,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 3
						}
					}).ToList()
				});
				var error = testInboxMessageArchives.Add(new eHubInboxMessageArchive
				{
					EI_PK = GetPk(2, 1, inboxInc++),
					eHubClient_Sender = cc1,
					eHubClient_Recipient = cc3,
					EI_InsertUTC = start.AddMinutes(timeInc++),
					EI_ApplicationCode = "AAA",
					EI_MessageTrackingID = GetPk(2, 16, inboxMsgId++).ToString(),
					EI_Status = 255,
					eHubOutboxMessageArchives = testOutboxMessageArchives.AddRange(new[] {
						new eHubOutboxMessageArchive
						{
							OI_PK = GetPk(2, 2, outboxInc++),
							eHubClient_Sender = cc2,
							eHubClient_Recipient = cc1,
							OI_InsertUTC = DateTime.MaxValue,
							OI_MessageTrackingID = GetPk(2, 17, outboxMsgId++).ToString(),
							OI_Status = 255
						}
					}).ToList()
				});
				testErrors.Add(new eHubError
				{
					EE_EI_Inbox = error.EI_PK,
					EE_OI_Outbox = error.eHubOutboxMessageArchives[0].OI_PK,
					EE_Description = "ERROR DESCRIPTION"
				});
			}
			foreach (var i in testInboxMessageArchives)
				foreach (var o in i.eHubOutboxMessageArchives)
				{
					o.eHubInboxMessageArchive = i;
					o.OI_EI_InboxPK = i.EI_PK;
				}

			start = new DateTime(2015, 2, 3, 2, 45, 15);
			timeInc = inboxInc = outboxInc = inboxMsgId = outboxMsgId = 0;

			for (var i = 0; i < 5; i++)
			{
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc2,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "BBB",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = "."
				});
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc3,
					eHubClient_RecipientInbox = cc1,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 255,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					AM_ErrorMessage = "ERROR MESSAGE"
				});
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc2,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					eHubClient_SenderOutbox = cc1,
					eHubClient_RecipientOutbox = cc3
				});
				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc3,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					eHubClient_SenderOutbox = cc1,
					eHubClient_RecipientOutbox = cc2
				});

				var batchPk = GetPk(3, 1, inboxInc++);
				var batchId = GetPk(3, 16, inboxMsgId++);
				var batchTime = start.AddMinutes(timeInc++);
				{
					testArchiveSecondaryMessages.Add(new eHubArchiveMessage
					{
						AM_PK = GetPk(3, 3, outboxInc++),
						eHubClient_SenderInbox = cc1,
						eHubClient_RecipientInbox = cc3,
						AM_ReceivedFromSenderUTC = batchTime,
						AM_ApplicationCode = "BBB",
						AM_InboxMessageTrackingID = batchId,
						AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
						AM_Status = 3,
						AM_EI_PK = batchPk,
						AM_SenderMessageRaw = ".",
						eHubClient_SenderOutbox = cc1,
						eHubClient_RecipientOutbox = cc2
					});
					testArchiveSecondaryMessages.Add(new eHubArchiveMessage
					{
						AM_PK = GetPk(3, 3, outboxInc++),
						eHubClient_SenderInbox = cc1,
						eHubClient_RecipientInbox = cc3,
						AM_ReceivedFromSenderUTC = batchTime,
						AM_ApplicationCode = "BBB",
						AM_InboxMessageTrackingID = batchId,
						AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
						AM_Status = 3,
						AM_EI_PK = batchPk,
						AM_SenderMessageRaw = null,
						eHubClient_SenderOutbox = cc1,
						eHubClient_RecipientOutbox = cc3
					});
				}

				testArchiveSecondaryMessages.Add(new eHubArchiveMessage
				{
					AM_PK = GetPk(3, 3, outboxInc++),
					eHubClient_SenderInbox = cc1,
					eHubClient_RecipientInbox = cc3,
					AM_ReceivedFromSenderUTC = start.AddMinutes(timeInc++),
					AM_ApplicationCode = "AAA",
					AM_InboxMessageTrackingID = GetPk(3, 16, inboxMsgId++),
					AM_OutboxMessageTrackingID = GetPk(3, 17, outboxMsgId++),
					AM_Status = 3,
					AM_EI_PK = GetPk(3, 1, inboxInc++),
					AM_SenderMessageRaw = ".",
					eHubClient_SenderOutbox = cc2,
					eHubClient_RecipientOutbox = cc1
				});
			}
		}

		static Guid GetPk(int a, int b, int c)
		{
			return new Guid(a, (short)b, (short)c, new byte[8]);
		}
	}
}
