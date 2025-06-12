using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubAdmin.ViewModels.Messages;
using eServices.eHubAdmin.ViewModels.Monitors;
using Common.Logging;
using Newtonsoft.Json;
using TimeZoneConverter;

namespace eServices.eHubAdmin.Controllers
{
	[Authorize]
	public class MonitorsController : Controller
	{
		private readonly eHubTransactionsContext writeableContext;
		private readonly eHubTransactionsContext readOnlyContext;
		private readonly ILog logger;

		public MonitorsController() : this(new eHubTransactionsContext("eHubTransactionsWritableContext"), new eHubTransactionsContext(), LogManager.GetLogger("Monitors")) { }

		internal MonitorsController(eHubTransactionsContext writeableContext, eHubTransactionsContext readOnlyContext, ILog logger)
		{
			this.writeableContext = writeableContext;
			this.readOnlyContext = readOnlyContext;
			this.logger = logger;
		}

		public ActionResult Index()
		{
			var eHubMonitors = readOnlyContext.eHubMonitors.OrderBy(m => m.MO_ID);
			return View(eHubMonitors.ToList());
		}

		public ActionResult Details(string id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var monitor = readOnlyContext.eHubMonitors.Include(m => m.eHubClients).FirstOrDefault(m => m.MO_ID == id);
			if (monitor == null)
				return HttpNotFound();

			var model = new MonitorDetails { eHubMonitor = monitor, DateRangeType = "UTC" };

			try
			{

				var details = readOnlyContext.SqlQuery<MonitorCountDetail>("GetMonitorCountDetails @p0", id);
				if (details.Any() && details.First().SubItem != null)
				{
					model.MonitorCountDetails = details;
				}
				else
				{
					model.MonitorCounts = readOnlyContext.SqlQuery<MonitorCounts>("GetMonitorCounts @p0,1", id).FirstOrDefault();
				}

				return View(model);

			}
			catch (Exception e)
			{
				return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet); 
			}
		}

		public ActionResult Edit(string id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var monitor = readOnlyContext.eHubMonitors.FirstOrDefault(m => m.MO_ID == id);
			if (monitor == null)
				return HttpNotFound();

			var monitorEdit = new MonitorEdit
			{
				MO_ID = monitor.MO_ID,
				ErrorDelayMins = monitor.MO_ErrorDelayMins,
				Enabled = monitor.MO_Enabled,
				Notes = monitor.MO_Notes,
				MaintenanceStartDayOfWeek = monitor.MO_MaintenanceStartDayOfWeek,
				MaintenanceEndDayOfWeek = monitor.MO_MaintenanceEndDayOfWeek,
				MaintenanceStartTime = monitor.MO_MaintenanceStartTime,
				MaintenanceEndTime = monitor.MO_MaintenanceEndTime,
				DateRangeType = "UTC"
			};

			ModelState.Clear();
			return View(monitorEdit);
		}

		[HttpPost, ActionName("Edit")]
		public ActionResult EditPost(MonitorEdit model)
		{
			if (ModelState.IsValid)
			{
				var monitor = writeableContext.eHubMonitors.FirstOrDefault(m => m.MO_ID == model.MO_ID);
				if (monitor == null)
					return HttpNotFound();

				var before = logger.IsInfoEnabled ? FormatMonitorForLog(monitor) : string.Empty;

				monitor.MO_ErrorDelayMins = model.ErrorDelayMins;
				monitor.MO_Enabled = model.Enabled;
				monitor.MO_Notes = model.Notes;
				monitor.MO_MaintenanceStartDayOfWeek = model.MaintenanceStartDayOfWeek == 0 ? null : model.MaintenanceStartDayOfWeek;
				monitor.MO_MaintenanceEndDayOfWeek = model.MaintenanceEndDayOfWeek == 0 ? null : model.MaintenanceEndDayOfWeek;
				

				monitor.MO_MaintenanceStartTime = model.MaintenanceStartTime;
				monitor.MO_MaintenanceEndTime = model.MaintenanceEndTime;

				if (model.DateRangeType == "Local")
				{
					var startDayAndTime = ConvertDayOfWeekAndTime(false,model.MaintenanceStartDayOfWeek, model.MaintenanceStartTime);
					monitor.MO_MaintenanceStartDayOfWeek = startDayAndTime.Item1;
					monitor.MO_MaintenanceStartTime = startDayAndTime.Item2;

					var endDayAndTime = ConvertDayOfWeekAndTime(false, model.MaintenanceEndDayOfWeek, model.MaintenanceEndTime);
					monitor.MO_MaintenanceEndDayOfWeek = endDayAndTime.Item1;
					monitor.MO_MaintenanceEndTime = endDayAndTime.Item2;
				}

				writeableContext.SaveChanges();

				if (logger.IsInfoEnabled)
					logger.InfoFormat("{0}: User=\"{1}\"\r\n  Before={2}\r\n  After ={3}", nameof(EditPost), HttpContext.User.Identity.Name, before, FormatMonitorForLog(monitor));

				return RedirectToAction(nameof(Details), new { id = model.MO_ID });
			}
			ModelState.Clear();
			return View();
		}

		[HttpPost]
		public JsonResult MonitorCounts(string id)
		{
			if (id == null)
				return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

			try
			{
				var monitorCounts = readOnlyContext.SqlQuery<MonitorCounts>("GetMonitorCounts @p0", id).FirstOrDefault();
				return Json(monitorCounts);
			}
			catch (Exception e)
			{
				return Json(e);
			}
		}

		[HttpPost]
		public ActionResult List(string id, int pageIndex, string TimeZoneIdIANA = "Australia/Sydney")
		{
			ViewBag.TimeZone = TZConvert.GetTimeZoneInfo(TimeZoneIdIANA);
			ViewBag.ShowLoadMore = false;
			ViewBag.SearchLimit = null;

			if (id == null)
				return Json(new HttpStatusCodeResult(HttpStatusCode.BadRequest));

			try
			{
				var monitorEI_PKs = readOnlyContext.SqlQuery<Guid>("GetMonitorEI_PKs @p0", id).ToList();
				var msgsList = new List<InboxMessage>();
				int totalCount = monitorEI_PKs.Count;

				if (totalCount > 0)
				{
					int startIndex = pageIndex * Constants.MaxMessagesInPage;
					monitorEI_PKs = monitorEI_PKs.GetRange(startIndex, Math.Min(Constants.MaxMessagesInPage, totalCount - startIndex));
					msgsList = MonitorQuery(monitorEI_PKs).ToList();

					var showLoadMore = totalCount > (startIndex + Constants.MaxMessagesInPage);
					var searchLimit = showLoadMore ? msgsList.Last().Received : null;
					ViewBag.ShowLoadMore = showLoadMore;
					ViewBag.SearchLimit = searchLimit;
					if (searchLimit != null) ViewBag.LoadMoreOnClick = "LoadList()";
				}

				return PartialView("../Messages/_List", msgsList);
			}
			catch (Exception e)
			{
				return Json(new { success = false, message = e.Message });
			}
		}

		private string FormatMonitorForLog(eHubMonitor monitor)
		{
			var details = new
			{
				monitor.MO_PK,
				monitor.MO_ID,
				monitor.MO_Description,
				monitor.MO_ErrorDelayMins,
				monitor.MO_Enabled,
				monitor.MO_Notes,
				monitor.MO_Type,
				eHubClients = monitor.eHubClients.Any() ? monitor.eHubClients.Select(c => c.CC_ID) : null,
			};

			var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
			return JsonConvert.SerializeObject(details, jsonSettings);
		}

		[HttpPost]
		public ActionResult ConvertDayAndTimesDetails(string dateRangeType, TimeSpan? startTime, TimeSpan? endTime, string startDay, string endDay)
		{
			var model = new MonitorDetails();
			model.eHubMonitor = new eHubMonitor();

			var startDayKey = Constants.DayOfWeek.FirstOrDefault(x => x.Value == startDay).Key;
			var endDayKey = Constants.DayOfWeek.FirstOrDefault(x => x.Value == endDay).Key;

			Convert(model, dateRangeType, startTime, endTime, startDayKey, endDayKey);
			return Json(model);
		}

			[HttpPost]
		public ActionResult ConvertDayAndTimesEdit(MonitorEdit model, string dateRangeType, TimeSpan? startTime, TimeSpan? endTime, int? startDay, int? endDay)
		{
			Convert(model, dateRangeType, startTime, endTime, startDay, endDay);
			return Json(model);
		}

		private static void Convert(dynamic model, string dateRangeType, TimeSpan? startTime, TimeSpan? endTime,
			int? startDay, int? endDay)
		{
			if (dateRangeType == "Local")
			{
				var startDayAndTime = ConvertDayOfWeekAndTime(true, startDay, startTime);
				model.MaintenanceStartDayOfWeek = startDayAndTime.Item1;
				model.MaintenanceStartTime = startDayAndTime.Item2;

				var endDayAndTime = ConvertDayOfWeekAndTime(true, endDay, endTime);
				model.MaintenanceEndDayOfWeek = endDayAndTime.Item1;
				model.MaintenanceEndTime = endDayAndTime.Item2;
			}
			else
			{
				var startDayAndTime = ConvertDayOfWeekAndTime(false, startDay, startTime);
				model.MaintenanceStartDayOfWeek = startDayAndTime.Item1;
				model.MaintenanceStartTime = startDayAndTime.Item2;

				var endDayAndTime = ConvertDayOfWeekAndTime(false, endDay, endTime);
				model.MaintenanceEndDayOfWeek = endDayAndTime.Item1;
				model.MaintenanceEndTime = endDayAndTime.Item2;
			}
		}

		public static (int?, TimeSpan?) ConvertDayOfWeekAndTime(bool isUtcToLocal, int? dayOfWeek, TimeSpan? time)
		{
			if (dayOfWeek == null || time == null)
			{
				return (null, null);
			}

			var offset = isUtcToLocal ? TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow) : -TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
			var convertedTime = time.Value + offset;
			int? convertedDayOfWeek = dayOfWeek;

			if (convertedTime < TimeSpan.Zero)
			{
				convertedDayOfWeek = dayOfWeek - 1;
				convertedTime += TimeSpan.FromDays(1);
			}
			else if (convertedTime >= TimeSpan.FromDays(1))
			{
				convertedDayOfWeek = dayOfWeek + 1;
				convertedTime -= TimeSpan.FromDays(1);
			}

			if (convertedDayOfWeek == 0)
			{
				convertedDayOfWeek = 7;
			}
			else if (convertedDayOfWeek == 8)
			{
				convertedDayOfWeek = 1;
			}

			return (convertedDayOfWeek, convertedTime);
		}

		IQueryable<InboxMessage> MonitorQuery(List<Guid> EI_PKs)
		{
			return readOnlyContext.eHubInboxMessages.Where(i => EI_PKs.Contains(i.EI_PK)).Select(i => new InboxMessage
			{
				Received = i.EI_InsertUTC,
				Sender = i.eHubClient_Sender.CC_ID,
				SenderObject = i.eHubClient_Sender,
				Recipient = i.eHubClient_Recipient.CC_ID,
				RecipientObject = i.eHubClient_Recipient,
				ApplicationCode = i.EI_ApplicationCode,
				Status = i.EI_Status,
				PK = i.EI_PK,
				AM_PK = null,
				MessageTrackingID = i.EI_MessageTrackingID,
				FileName = i.EI_FileNameOverride,
				Subject = i.EI_EmailSubjectOverride,
				MessageType = i.EI_MessageType,
				OutboxMessages = i.eHubOutboxMessages.Select(o => new OutboxMessage
				{
					Sender = o.eHubClient_Sender.CC_ID,
					SenderObject = o.eHubClient_Sender,
					Recipient = o.eHubClient_Recipient.CC_ID,
					RecipientObject = o.eHubClient_Recipient,
					Status = o.OI_Status,
					Processed = o.OI_InsertUTC,
					Delivered = o.OI_LastUpdateUTC,
					PK = o.OI_PK,
					AM_PK = null,
					MessageTrackingID = o.OI_MessageTrackingID,
					FileName = o.OI_OverrideFilename,
					Subject = o.OI_OverrideEmailSubject
				})
			}).OrderByDescending(i => i.Received);
		}

	}
}
