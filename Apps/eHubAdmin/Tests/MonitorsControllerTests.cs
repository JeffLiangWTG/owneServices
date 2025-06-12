using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubAdmin.Controllers;
using eServices.eHubAdmin.ViewModels.Messages;
using eServices.eHubAdmin.ViewModels.Monitors;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using Rhino.Mocks;

namespace eServices.eHubAdmin.Tests
{
	public class MonitorsControllerTests
	{
		private HttpRequestBase mockRequest;
		private HttpResponseBase mockResponse;
		private HttpSessionStateBase mockHttpSessionStateBase;
		private IIdentity mockIdentity;
		private IPrincipal mockPrincipal;
		private HttpContextBase mockHttpContext;
		private eHubTransactionsContext mockwriteableContext;
		private eHubTransactionsContext mockreadonlyContext;
		private TestDbSet<eHubMonitor> testMonitors;
		private TestDbSet<eHubInboxMessage> testInboxMessages;
		private ILog logger;
		private MonitorsController monitorsController;

		[SetUp]
		public void Setup()
		{
			mockRequest = MockRepository.GenerateMock<HttpRequestBase>();
			mockResponse = MockRepository.GenerateMock<HttpResponseBase>();
			mockHttpSessionStateBase = MockRepository.GenerateMock<HttpSessionStateBase>();
			mockIdentity = MockRepository.GenerateMock<IIdentity>();
			mockPrincipal = MockRepository.GenerateMock<IPrincipal>();
			mockHttpContext = MockRepository.GenerateMock<HttpContextBase>();
			mockHttpContext.Stub(x => x.Request).Return(mockRequest);
			mockHttpContext.Stub(x => x.Response).Return(mockResponse);
			mockHttpContext.Stub(x => x.Session).Return(mockHttpSessionStateBase);
			mockHttpContext.Stub(x => x.User).Return(mockPrincipal);
			mockPrincipal.Stub(x => x.Identity).Return(mockIdentity);
			logger = new ConsoleOutLogger("Monitors", LogLevel.All, true, false, false, null);
			mockwriteableContext = MockRepository.GenerateMock<eHubTransactionsContext>("eHubTransactionsWritableContext");
			mockreadonlyContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			testMonitors = new TestDbSet<eHubMonitor>();
			testInboxMessages = new TestDbSet<eHubInboxMessage>();
			mockwriteableContext.Stub(x => x.eHubMonitors).Return(testMonitors);
			mockwriteableContext.Stub(x => x.eHubInboxMessages).Return(testInboxMessages);
			mockreadonlyContext.Stub(x => x.eHubMonitors).Return(testMonitors);
			mockreadonlyContext.Stub(x => x.eHubInboxMessages).Return(testInboxMessages);

			monitorsController = new MonitorsController(mockwriteableContext, mockreadonlyContext, logger);
			monitorsController.ControllerContext = new ControllerContext(mockHttpContext, new RouteData(), monitorsController);
		}

		[Test]
		public void MonitorsController_Index()
		{
			testMonitors.AddRange(new[]
			{
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true },
				new eHubMonitor { MO_PK = new Guid("11111111-1111-1111-1111-111111111111"), MO_ID = "MON_BBB", MO_Description = "Test Monitor BBB", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = false },
			});

			var result = monitorsController.Index() as ViewResult;
			var resultData = (List<eHubMonitor>)result.Model;
			Assert.That(resultData.Count, Is.EqualTo(2));
			CollectionAssert.AreEqual(new[] { "MON_AAA", "MON_BBB" }, resultData.Select(m => m.MO_ID));
		}

		[Test]
		public void MonitorsController_Details()
		{
			var client = new eHubClient { CC_ID = "SVC" };
			testMonitors.Add(
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true, eHubClients = new List<eHubClient>() { client } }
			);
			mockreadonlyContext.Stub(x => x.SqlQuery<MonitorCountDetail>("GetMonitorCountDetails @p0", "MON_AAA")).Return(new[] { new MonitorCountDetail { SubItem = null, Age = -1, Queue = -1 } });
			mockreadonlyContext.Stub(x => x.SqlQuery<MonitorCounts>("GetMonitorCounts @p0,1", "MON_AAA")).Return(new[] { new MonitorCounts { Age = 5, Queue = 10 } });

			var result = monitorsController.Details("MON_AAA") as ViewResult;
			var resultData = (MonitorDetails)result.Model;
			Assert.That(resultData.eHubMonitor.MO_ID, Is.EqualTo("MON_AAA"));
			Assert.That(resultData.MonitorCounts.Age, Is.EqualTo(5));
			Assert.That(resultData.MonitorCounts.Queue, Is.EqualTo(10));

		}

		[Test]
		public void MonitorsController_Details_ThrowsException()
		{
			var client = new eHubClient { CC_ID = "SVC" };
			testMonitors.Add(
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true, eHubClients = new List<eHubClient>() { client } }
			);
			mockreadonlyContext.Stub(x => x.SqlQuery<MonitorCountDetail>("GetMonitorCountDetails @p0", "MON_AAA")).Return(new[] { new MonitorCountDetail { SubItem = null, Age = -1, Queue = -1 } });
			mockreadonlyContext.Stub(x => x.SqlQuery<MonitorCounts>("GetMonitorCounts @p0,1", "MON_AAA")).Throw(new Exception("EdiProd cannot be accessed. Reason: Upgrading, please try again later."));

			var result = (JsonResult)monitorsController.Details("MON_AAA");

			Assert.That(result.Data.ToString(), Is.EqualTo("{ success = False, message = EdiProd cannot be accessed. Reason: Upgrading, please try again later. }"));
		}

		[Test]
		public void MonitorsController_Details_CW1_DOWNLOAD()
		{
			var client = new eHubClient { CC_ID = "SVC" };
			testMonitors.Add(
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "CW1_DOWNLOAD", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true, eHubClients = new List<eHubClient>() { client } }
			);
			mockreadonlyContext.Stub(x => x.SqlQuery<MonitorCountDetail>("GetMonitorCountDetails @p0", "CW1_DOWNLOAD")).Return(new[] { new MonitorCountDetail { SubItem = "SVC", Age = 5, Queue = 10 } });

			var result = monitorsController.Details("CW1_DOWNLOAD") as ViewResult;
			var resultData = (MonitorDetails)result.Model;

			Assert.That(resultData.eHubMonitor.MO_ID, Is.EqualTo("CW1_DOWNLOAD"));
			Assert.That(resultData.MonitorCountDetails, Is.Not.Null);
			Assert.That(resultData.MonitorCountDetails.Count, Is.EqualTo(1));
			Assert.That(resultData.MonitorCountDetails.First().SubItem, Is.EqualTo("SVC"));
			Assert.That(resultData.MonitorCountDetails.First().Age, Is.EqualTo(5));
			Assert.That(resultData.MonitorCountDetails.First().Queue, Is.EqualTo(10));
		}

		[Test]
		public void MonitorsController_Edit()
		{
			testMonitors.AddRange(new[]
			{
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_ErrorDelayMins = 15, MO_Enabled = true, MO_Notes = null,MO_MaintenanceStartTime= TimeSpan.MinValue,MO_MaintenanceStartDayOfWeek = 1,
					MO_MaintenanceEndTime= TimeSpan.MaxValue, MO_MaintenanceEndDayOfWeek = 3, eHubClients = new List<eHubClient>() },
			});

			var result = monitorsController.Edit("MON_AAA");
			Assert.That(result, Is.TypeOf<ViewResult>());

			var resultModel = (MonitorEdit)((ViewResult)result).Model;
			Assert.That(resultModel.MO_ID, Is.EqualTo("MON_AAA"));
			Assert.That(resultModel.ErrorDelayMins, Is.EqualTo(15));
			Assert.That(resultModel.Enabled, Is.True);
			Assert.That(resultModel.Notes, Is.Null);
			Assert.That(resultModel.MaintenanceStartTime, Is.EqualTo(TimeSpan.MinValue));
			Assert.That(resultModel.MaintenanceEndTime, Is.EqualTo(TimeSpan.MaxValue));
			Assert.That(resultModel.MaintenanceStartDayOfWeek, Is.EqualTo(1));
			Assert.That(resultModel.MaintenanceEndDayOfWeek, Is.EqualTo(3));
		}

		[Test]
		public void MonitorsController_EditPost()
		{
			testMonitors.AddRange(new[]
			{
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_ErrorDelayMins = 15, MO_Enabled = true, MO_Notes = null, MO_MaintenanceStartTime= TimeSpan.MinValue,MO_MaintenanceStartDayOfWeek = 1,
					MO_MaintenanceEndTime= TimeSpan.MaxValue, MO_MaintenanceEndDayOfWeek = 3, eHubClients = new List<eHubClient>() },
			});
			mockIdentity.Stub(x => x.Name).Return("DOMAIN\\TestUser");

			var request = new MonitorEdit { MO_ID = "MON_AAA", ErrorDelayMins = 30, Enabled = false, Notes = "Add Note\r\n😀" , MaintenanceStartDayOfWeek = 1,MaintenanceEndDayOfWeek = 3, MaintenanceStartTime = TimeSpan.MinValue, MaintenanceEndTime = TimeSpan.MaxValue};

			var result = monitorsController.EditPost(request);

			Assert.That(result, Is.TypeOf<RedirectToRouteResult>().And.Property("RouteValues").EquivalentTo(new Dictionary<string, object> { { "id", "MON_AAA" }, { "action", "Details" } }));

			var monitorData = mockwriteableContext.eHubMonitors.First(m => m.MO_ID == "MON_AAA");
			Assert.That(monitorData.MO_ErrorDelayMins, Is.EqualTo(30));
			Assert.That(monitorData.MO_Enabled, Is.False);
			Assert.That(monitorData.MO_Notes, Is.EqualTo("Add Note\r\n😀"));
			Assert.That(monitorData.MO_MaintenanceStartTime, Is.EqualTo(TimeSpan.MinValue));
			Assert.That(monitorData.MO_MaintenanceEndTime, Is.EqualTo(TimeSpan.MaxValue));
			Assert.That(monitorData.MO_MaintenanceStartDayOfWeek, Is.EqualTo(1));
			Assert.That(monitorData.MO_MaintenanceEndDayOfWeek, Is.EqualTo(3));
		}

		[Test]
		public void MonitorsController_MonitorCounts()
		{
			var client = new eHubClient { CC_ID = "SVC" };
			testMonitors.Add(
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true, eHubClients = new List<eHubClient>() { client } }
			);
			mockreadonlyContext.Stub(x => x.SqlQuery<MonitorCounts>("GetMonitorCounts @p0", "MON_AAA")).Return(new[] { new MonitorCounts { Age = 5, Queue = 10 } });

			var result = monitorsController.MonitorCounts("MON_AAA");
			var resultData = (MonitorCounts)result.Data;
			Assert.That(resultData.Age, Is.EqualTo(5));
			Assert.That(resultData.Queue, Is.EqualTo(10));
		}

		[Test]
		public void MonitorsController_List_Empty()
		{
			var client = new eHubClient { CC_ID = "SVC" };
			testMonitors.Add(
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true, eHubClients = new List<eHubClient>() { client } }
			);
			mockreadonlyContext.Stub(x => x.SqlQuery<Guid>("GetMonitorEI_PKs @p0", "MON_AAA")).Return(new List<Guid>());

			var result = monitorsController.List("MON_AAA", 0, "America/New_York") as PartialViewResult;
			Assert.AreEqual(result.ViewBag.TimeZone.ToString(), "(UTC-05:00) Eastern Time (US & Canada)");
			CollectionAssert.IsEmpty((List<InboxMessage>)result.Model);
		}

		[Test]
		public void MonitorsController_List()
		{
			var client = new eHubClient { CC_ID = "SVC" };
			testMonitors.Add(
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true, eHubClients = new List<eHubClient>() { client } }
			);
			mockreadonlyContext.Stub(x => x.SqlQuery<Guid>("GetMonitorEI_PKs @p0", "MON_AAA")).Return( new List<Guid>() { new Guid("11111111-0000-0000-0000-000000000000"), new Guid("22222222-0000-0000-0000-000000000000") });

			var client1 = new eHubClient { CC_ID = "TSTAAA111" };
			var client2 = new eHubClient { CC_ID = "TSTAAA222" };
			var provider1 = new eHubClient { CC_ID = "PROVIDER1" };

			testInboxMessages.AddRange(new[]
			{
				new eHubInboxMessage { EI_PK = new Guid("00000000-0000-0000-0000-000000000000"), EI_MessageTrackingID = "00000000-0000-0000-0000-000000000000", eHubClient_Sender = client1, eHubClient_Recipient = provider1 },
				new eHubInboxMessage { EI_PK = new Guid("11111111-0000-0000-0000-000000000000"), EI_MessageTrackingID = "11111110-0000-0000-0000-000000000000", eHubClient_Sender = provider1, eHubClient_Recipient = client1 },
				new eHubInboxMessage { EI_PK = new Guid("22222222-0000-0000-0000-000000000000"), EI_MessageTrackingID = "22222220-0000-0000-0000-000000000000", eHubClient_Sender = client2, eHubClient_Recipient = provider1, EI_InsertUTC = DateTime.UtcNow.AddMinutes(-30) },
				new eHubInboxMessage { EI_PK = new Guid("33333333-0000-0000-0000-000000000000"), EI_MessageTrackingID = "33333330-0000-0000-0000-000000000000", eHubClient_Sender = client2, eHubClient_Recipient = provider1, EI_InsertUTC = DateTime.UtcNow.AddMinutes(-20) },
			});

			var result = monitorsController.List("MON_AAA", 0) as PartialViewResult;
			Assert.AreEqual(result.ViewBag.TimeZone.ToString(), "(UTC+10:00) Canberra, Melbourne, Sydney");
			CollectionAssert.AreEqual(new[] { "22222222-0000-0000-0000-000000000000", "11111111-0000-0000-0000-000000000000" }, ((List<InboxMessage>)result.Model).Select(i => i.PK.ToString()));
		}

		[Test]
		public void MonitorsController_List_Page1()
		{
			var client = new eHubClient { CC_ID = "SVC" };
			testMonitors.Add(
				new eHubMonitor { MO_PK = new Guid("00000000-0000-0000-0000-000000000000"), MO_ID = "MON_AAA", MO_Description = "Test Monitor AAA", MO_ErrorDelayMins = 15, MO_Type = "InboxRecipient", MO_Enabled = true, eHubClients = new List<eHubClient>() { client } }
			);
			mockreadonlyContext.Stub(x => x.SqlQuery<Guid>("GetMonitorEI_PKs @p0", "MON_AAA")).Return(new List<Guid>() {
				new Guid("00000000-0000-0000-0000-000000000000"), new Guid("33333333-0000-0000-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000000"),
				new Guid("00000006-0000-0000-0000-000000000000"), new Guid("00000007-0000-0000-0000-000000000000"), new Guid("00000008-0000-0000-0000-000000000000"), new Guid("00000009-0000-0000-0000-000000000000"), new Guid("00000010-0000-0000-0000-000000000000"),
				new Guid("00000011-0000-0000-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000000"),
				new Guid("00000016-0000-0000-0000-000000000000"), new Guid("00000007-0000-0000-0000-000000000000"), new Guid("00000008-0000-0000-0000-000000000000"), new Guid("00000009-0000-0000-0000-000000000000"), new Guid("00000010-0000-0000-0000-000000000000"),
				new Guid("00000021-0000-0000-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000000"),
				new Guid("00000026-0000-0000-0000-000000000000"), new Guid("00000007-0000-0000-0000-000000000000"), new Guid("00000008-0000-0000-0000-000000000000"), new Guid("00000009-0000-0000-0000-000000000000"), new Guid("00000010-0000-0000-0000-000000000000"),
				new Guid("00000031-0000-0000-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000000"),
				new Guid("00000036-0000-0000-0000-000000000000"), new Guid("00000007-0000-0000-0000-000000000000"), new Guid("00000008-0000-0000-0000-000000000000"), new Guid("00000009-0000-0000-0000-000000000000"), new Guid("00000010-0000-0000-0000-000000000000"),
				new Guid("00000041-0000-0000-0000-000000000000"), new Guid("00000002-0000-0000-0000-000000000000"), new Guid("00000003-0000-0000-0000-000000000000"), new Guid("00000004-0000-0000-0000-000000000000"), new Guid("00000005-0000-0000-0000-000000000000"),
				new Guid("00000046-0000-0000-0000-000000000000"), new Guid("00000007-0000-0000-0000-000000000000"), new Guid("00000008-0000-0000-0000-000000000000"), new Guid("00000009-0000-0000-0000-000000000000"), new Guid("00000010-0000-0000-0000-000000000000"),
				new Guid("11111111-0000-0000-0000-000000000000"), new Guid("22222222-0000-0000-0000-000000000000") });

			var client1 = new eHubClient { CC_ID = "TSTAAA111" };
			var client2 = new eHubClient { CC_ID = "TSTAAA222" };
			var provider1 = new eHubClient { CC_ID = "PROVIDER1" };

			testInboxMessages.AddRange(new[]
			{
				new eHubInboxMessage { EI_PK = new Guid("00000000-0000-0000-0000-000000000000"), EI_MessageTrackingID = "00000000-0000-0000-0000-000000000000", eHubClient_Sender = client1, eHubClient_Recipient = provider1 },
				new eHubInboxMessage { EI_PK = new Guid("11111111-0000-0000-0000-000000000000"), EI_MessageTrackingID = "11111110-0000-0000-0000-000000000000", eHubClient_Sender = provider1, eHubClient_Recipient = client1 },
				new eHubInboxMessage { EI_PK = new Guid("22222222-0000-0000-0000-000000000000"), EI_MessageTrackingID = "22222220-0000-0000-0000-000000000000", eHubClient_Sender = client2, eHubClient_Recipient = provider1, EI_InsertUTC = DateTime.UtcNow.AddMinutes(-30) },
				new eHubInboxMessage { EI_PK = new Guid("33333333-0000-0000-0000-000000000000"), EI_MessageTrackingID = "33333330-0000-0000-0000-000000000000", eHubClient_Sender = client2, eHubClient_Recipient = provider1, EI_InsertUTC = DateTime.UtcNow.AddMinutes(-20) },
			});

			var result = monitorsController.List("MON_AAA", 1) as PartialViewResult;
			Assert.AreEqual(result.ViewBag.TimeZone.ToString(), "(UTC+10:00) Canberra, Melbourne, Sydney");
			CollectionAssert.AreEqual(new[] { "22222222-0000-0000-0000-000000000000", "11111111-0000-0000-0000-000000000000" }, ((List<InboxMessage>)result.Model).Select(i => i.PK.ToString()));
		}

		[Test]
		public void ConvertDayAndTimes_WithLocalDateRangeAndValidTimes()
		{
			var model = new MonitorEdit();
			var dateRangeType = "Local";
			var startTime = TimeSpan.FromHours(9);
			var endTime = TimeSpan.FromHours(17);
			var startDay = 1;
			var endDay = 5;

			var result = monitorsController.ConvertDayAndTimesEdit(model, dateRangeType, startTime, endTime, startDay, endDay) as JsonResult;

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Data, Is.EqualTo(model));

			var timezoneInfo = TimeZoneInfo.Local;
			if (timezoneInfo.IsDaylightSavingTime(DateTime.Today))
			{
				Assert.That(model.MaintenanceStartTime, Is.EqualTo(TimeSpan.FromHours(20)));
				Assert.That(model.MaintenanceEndTime, Is.EqualTo(TimeSpan.FromHours(4)));
			}
			else
			{
				Assert.That(model.MaintenanceStartTime, Is.EqualTo(TimeSpan.FromHours(19)));
				Assert.That(model.MaintenanceEndTime, Is.EqualTo(TimeSpan.FromHours(3)));
			}
			Assert.That(model.MaintenanceStartDayOfWeek, Is.EqualTo(1));
			Assert.That(model.MaintenanceEndDayOfWeek, Is.EqualTo(6));
		}

		[Test]
		public void ConvertDayAndTimes_WithUtcDateRangeAndValidTimes()
		{
			var model = new MonitorEdit();
			var dateRangeType = "UTC";
			var startTime = TimeSpan.FromHours(9);
			var endTime = TimeSpan.FromHours(17);
			var startDay = 1;
			var endDay = 5;

			var result = monitorsController.ConvertDayAndTimesEdit(model, dateRangeType, startTime, endTime, startDay, endDay) as JsonResult;

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Data, Is.EqualTo(model));
			
			var timezoneInfo = TimeZoneInfo.Local;
			if (timezoneInfo.IsDaylightSavingTime(DateTime.Today))
			{
				Assert.That(model.MaintenanceStartTime, Is.EqualTo(TimeSpan.FromHours(22)));
				Assert.That(model.MaintenanceEndTime, Is.EqualTo(TimeSpan.FromHours(6)));
			}
			else
			{
				Assert.That(model.MaintenanceStartTime, Is.EqualTo(TimeSpan.FromHours(23)));
				Assert.That(model.MaintenanceEndTime, Is.EqualTo(TimeSpan.FromHours(7)));
			}

			Assert.That(model.MaintenanceStartDayOfWeek, Is.EqualTo(7));
			Assert.That(model.MaintenanceEndDayOfWeek, Is.EqualTo(5));
		}
	}
}
