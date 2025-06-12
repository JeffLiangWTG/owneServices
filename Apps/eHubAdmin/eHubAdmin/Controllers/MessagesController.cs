using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubArchiveOnline;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubAdmin.Helpers;
using eServices.eHubAdmin.ViewModels.Messages;
using TimeZoneConverter;

namespace eServices.eHubAdmin.Controllers
{
	public class MessagesController : Controller
	{
		readonly eHubTransactionsContext eHubTransactionsContext;
		readonly eHubArchiveOnlineContext eHubArchiveOnlineSecondaryContext;
		readonly Func<DateTime> utcNowProvider;
		internal TimeSpan checkArchiveStagingTimeout = TimeSpan.FromMilliseconds(1000);
		bool disposed = false;

		const int ThresholdArchiveStaging = 10000;
		const string FirstTimeCheckingThresholdArchiveStaging = "FirstTimeCheckingThresholdArchiveStaging";

		public MessagesController()
			: this(new eHubTransactionsContext(),
				new eHubArchiveOnlineContext("eHubArchiveOnlineSecondaryContext"),
				() => DateTime.UtcNow)
		{ }

		internal MessagesController(eHubTransactionsContext eHubTransactionsContext,
			eHubArchiveOnlineContext eHubArchiveOnlineSecondaryContext,
			Func<DateTime> utcNowProvider)
		{
			this.eHubTransactionsContext = eHubTransactionsContext;
			this.eHubArchiveOnlineSecondaryContext = eHubArchiveOnlineSecondaryContext;
			this.utcNowProvider = utcNowProvider;
		}

		public ActionResult Query(Query query, int? refresh = null)
		{
			if (TempData["searchType"] != null)
			{
				query.SearchType = TempData["searchType"].ToString();
				TempData.Remove("searchType");
			}
			query.ConvertStringsToDateTimes();
			query.Refresh = refresh;
			if (string.IsNullOrEmpty(query.DateFilterType) || (query.DateFilterType == "WithinDates" && query.FromDate == null && query.ToDate == null))
			{
				query.FixData();
				Session[FirstTimeCheckingThresholdArchiveStaging] = null;
			}
			query.Role = string.IsNullOrWhiteSpace(query.SenderInbox + query.RecipientInbox + query.SenderOutbox + query.RecipientOutbox) ? "Any" : "Specific";
			if (query.DateFilterType == "WithinPeriod")
			{
				if (query.FromPeriodValue == null)
				{
					query.FromPeriodValue = 1;
				}
				if (string.IsNullOrWhiteSpace(query.FromPeriodType))
				{
					query.FromPeriodType = "Hour";
				}
				if (query.ToPeriodValue == null)
				{
					query.ToPeriodValue = 0;
				}
				if (string.IsNullOrWhiteSpace(query.ToPeriodType))
				{
					query.ToPeriodType = "Hour";
				}
			}

			if (!ModelState.IsValid || HttpContext.Request.QueryString.Count == 0)
			{
				ViewBag.ShowQuery = true;
			}

			if (query.IsIncludingArchiveStaging.Value)
			{
				var isBelowThresholdArchiveStaging = IsBelowThresholdArchiveStaging();
				if (!isBelowThresholdArchiveStaging)
				{
					ViewBag.ArchiveStagingWarning = Session[FirstTimeCheckingThresholdArchiveStaging] == null ? "Searching in the archive staging tables has been disabled." : "Recommend to disable!";
				}
				if (Session[FirstTimeCheckingThresholdArchiveStaging] == null)
				{
					query.IsIncludingArchiveStaging = isBelowThresholdArchiveStaging;
				}
				Session[FirstTimeCheckingThresholdArchiveStaging] = true;
			}

			return View(query);
		}

		internal virtual bool IsBelowThresholdArchiveStaging()
		{
			SpaceUsedProcedureHelper inboxArchiveSpaceUsedProcedureHelper = null;
			SpaceUsedProcedureHelper outboxArchiveSpaceUsedProcedureHelper = null;

			var completed = ExecuteWithTimeLimit(checkArchiveStagingTimeout, () =>
			{
				inboxArchiveSpaceUsedProcedureHelper = eHubTransactionsContext.SqlQuery<SpaceUsedProcedureHelper>("sp_spaceused 'eHubInboxMessageArchive'").Single();
				outboxArchiveSpaceUsedProcedureHelper = eHubTransactionsContext.SqlQuery<SpaceUsedProcedureHelper>("sp_spaceused 'eHubOutboxMessageArchive'").Single();
			});
			if (completed && inboxArchiveSpaceUsedProcedureHelper != null && outboxArchiveSpaceUsedProcedureHelper != null)
			{
				return int.Parse(inboxArchiveSpaceUsedProcedureHelper.rows) < ThresholdArchiveStaging && int.Parse(outboxArchiveSpaceUsedProcedureHelper.rows) < ThresholdArchiveStaging;
			}

			return false;
		}

		static bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock)
		{
			try
			{
				var task = Task.Factory.StartNew(codeBlock);
				task.Wait(timeSpan);
				return task.IsCompleted;
			}
			catch (AggregateException ex)
			{
				throw ex.InnerException;
			}
		}

		public string ShowArchiveStagingWarning()
		{
			return !IsBelowThresholdArchiveStaging() ? "Recommend to disable!" : string.Empty;
		}

		[HttpPost, ActionName("Query")]
		public ActionResult QueryPost(Query query)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.ShowQuery = true;
				return View(query);
			}

			if (!query.IsIncludingArchive.HasValue)
			{
				query.IsIncludingArchive = false;
			}
			if (!query.IsIncludingArchiveStaging.HasValue)
			{
				query.IsIncludingArchiveStaging = false;
			}
			if (!query.IsIncludingMessageBox.HasValue)
			{
				query.IsIncludingMessageBox = false;
			}

			if (query.FromDate.HasValue && !query.FromTime.HasValue)
			{
				query.FromTime = new TimeSpan(0, 0, 0);
			}
			if (query.ToDate.HasValue && !query.ToTime.HasValue)
			{
				query.ToTime = new TimeSpan(23, 59, 59);
			}
			if (query.FromDate.HasValue && query.ToDate.HasValue)
			{
				var from = query.FromDate.Value.Add(query.FromTime.Value);
				var to = query.ToDate.Value.Add(query.ToTime.Value);
				if (DateTime.Compare(from, to) == 0 && TimeSpan.Compare(query.ToTime.Value, new TimeSpan(0, 0, 0)) == 0)
				{
					query.FromTime = new TimeSpan(0, 0, 0);
					query.ToTime = new TimeSpan(23, 59, 59);
				}
			}
			if (query.DateFilterType == "WithinDates" && query.FromDate == null && query.ToDate == null)
			{
				query.FixData();
			}
			query.ConvertDateTimesToStrings();
			RemoveDefaultValues(ref query);
			return RedirectToAction("Query", query);
		}

		private void RemoveDefaultValues(ref Query query)
		{
			var fixQueryValues = new Query();
			fixQueryValues.FixData();
			var type = typeof(Query);
			foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				object selfValue = type.GetProperty(pi.Name).GetValue(fixQueryValues, null);
				object toValue = type.GetProperty(pi.Name).GetValue(query, null);
				if (toValue != null && selfValue != null && selfValue.Equals(toValue))
				{
					type.GetProperty(pi.Name).SetValue(query, null);
				}
			}
		}

		[HttpPost]
		public ActionResult List(Query query, string searchLimit)
		{
			if (!ModelState.IsValid)
			{
				query.FixData();
			}

			var timeZone = TZConvert.GetTimeZoneInfo(query.TimeZoneIdIANA);
			ViewBag.TimeZone = timeZone;

			DateTime? fromDateQuery;
			DateTime? toDateQuery;
			if (query.DateRangeType == "UTC")
			{
				fromDateQuery = query.FromDate.HasValue ? DateTime.SpecifyKind(query.FromDate.Value.Add(query.FromTime.GetValueOrDefault()), DateTimeKind.Utc) : (DateTime?)null;
				toDateQuery = query.ToDate.HasValue ? DateTime.SpecifyKind(query.ToDate.Value.Add(query.ToTime.GetValueOrDefault()), DateTimeKind.Utc) : (DateTime?)null;
			}
			else
			{
				fromDateQuery = query.FromDate.HasValue ? TimeZoneInfo.ConvertTimeToUtc(query.FromDate.Value.Add(query.FromTime.GetValueOrDefault()), timeZone) : (DateTime?)null;
				toDateQuery = query.ToDate.HasValue ? TimeZoneInfo.ConvertTimeToUtc(query.ToDate.Value.Add(query.ToTime.GetValueOrDefault()), timeZone) : (DateTime?)null;
			}

			DateTime? searchLimitValue = null;
			if (!string.IsNullOrWhiteSpace(searchLimit))
			{
				if (DateTimeOffset.TryParseExact(searchLimit, "o", null, DateTimeStyles.AssumeUniversal, out DateTimeOffset searchLimitParsed))
				{
					searchLimitValue = searchLimitParsed.UtcDateTime;
				}
			}

			var now = utcNowProvider.Invoke();

			if (query.DateFilterType == "WithinPeriod")
			{
				fromDateQuery = query.GetPeriodDate(now, query.FromPeriodValue.Value, query.FromPeriodType);
				toDateQuery = query.ToPeriodValue == null ? now : query.GetPeriodDate(now, query.ToPeriodValue.Value, query.ToPeriodType);
			}

			query.ToDate = searchLimitValue ?? toDateQuery;
			query.FromDate = query.KeyType != "EI_PK" && query.KeyType != "OI_PK" && query.KeyType != "AM_PK" && query.KeyType != "MsgID" ? fromDateQuery ?? (query.ToDate ?? now).AddDays(-1) : (DateTime?)null;

			ViewBag.SearchLimit = searchLimit;
			using (eHubTransactionsContext.BeginTransaction(IsolationLevel.ReadUncommitted))
			using (eHubArchiveOnlineSecondaryContext.BeginTransaction(IsolationLevel.ReadUncommitted))
			{
				var msgsList = new List<InboxMessage>();
				try
				{
					int unGroupedListCount = 0;

					eHubTransactionsContext.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseCommandTimeout"]);
					eHubArchiveOnlineSecondaryContext.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseCommandTimeout"]);

					IQueryable<InboxMessage> msgsQuery;
					if (query.KeyType != "AM_PK")
					{
						if (query.IsIncludingMessageBox.Value)
						{
							msgsQuery = LiveMessagesQuery(query);
							msgsList = msgsList.Concat(msgsQuery).ToList();
							unGroupedListCount = msgsList.Count;
						}

						if (!(msgsList.Count > 0 && (query.KeyType == "EI_PK" || query.KeyType == "OI_PK")) && (string.IsNullOrWhiteSpace(query.Status) || query.Status == "Delivered" || query.Status == "Error") && query.IsIncludingArchiveStaging.Value)
						{
							if (msgsList.Count >= Constants.MaxMessagesInPage)
							{
								var dateTime = msgsList.Min(m => m.Received);
								if (dateTime != null)
								{
									query.FromDate = dateTime.Value;
								}
							}
							msgsQuery = MovedMessagesQuery(query);
							msgsList = msgsList.Concat(msgsQuery).OrderByDescending(m => m.Received).Take(Constants.MaxMessagesInPage).ToList();
							unGroupedListCount = msgsList.Count;
						}
					}

					if (query.KeyType != "EI_PK" && query.KeyType != "OI_PK" && (string.IsNullOrWhiteSpace(query.Status) || query.Status == "Error") && query.IsIncludingArchive.Value)
					{
						Guid key;
						if (query.KeyType == "AM_PK")
						{
							key = (Guid)query.Key;
							query.Received
								= eHubArchiveOnlineSecondaryContext.eHubArchiveMessages.Where(a => a.AM_PK == key).Select(a => a.AM_ReceivedFromSenderUTC).FirstOrDefault();
						}
						else if (query.KeyType == "MsgID")
						{
							key = (Guid)query.Key;
							var inboxDate
								= eHubArchiveOnlineSecondaryContext.eHubArchiveMessages.Where(a => a.AM_InboxMessageTrackingID == key).Select(a => a.AM_ReceivedFromSenderUTC).FirstOrDefault();
							var outboxDate
								= eHubArchiveOnlineSecondaryContext.eHubArchiveMessages.Where(a => a.AM_OutboxMessageTrackingID == key).Select(a => a.AM_ReceivedFromSenderUTC).FirstOrDefault();
							query.Received = new DateTime(Math.Max(inboxDate.GetValueOrDefault().Ticks, outboxDate.GetValueOrDefault().Ticks));
						}

						using (var queryOption = new QueryOptionCommandInterceptor(eHubArchiveOnlineSecondaryContext, "USE HINT('ENABLE_PARALLEL_PLAN_PREFERENCE')"))
						{
							var msgsUngrouped = ArchivedMessagesQuery(query, eHubArchiveOnlineSecondaryContext).ToList();
							unGroupedListCount += msgsUngrouped.Count;

							var msgQuery = msgsUngrouped.GroupBy(a => new { a.Received, a.AM_EI_PK }).Select(gr_a => gr_a.FirstOrDefault());
							msgsList = msgsList.Concat(msgQuery).OrderByDescending(m => m.Received).Take(Constants.MaxMessagesInPage).ToList();
						}
					}

					ViewBag.ShowLoadMore = query.FromDate.HasValue && unGroupedListCount >= Constants.MaxMessagesInPage && (!fromDateQuery.HasValue || msgsList.Last().Received <= fromDateQuery);
					var searchLimitDt = unGroupedListCount >= Constants.MaxMessagesInPage ? msgsList.Last().Received : !fromDateQuery.HasValue || query.FromDate < fromDateQuery ? query.FromDate : null;
					ViewBag.SearchLimit = searchLimitDt;
					if (searchLimitDt != null) ViewBag.LoadMoreOnClick = $"LoadList(false, '{ViewBag.SearchLimit.ToString("o")}')";
				}
				catch (EntityCommandExecutionException ex)
				{
					ViewBag.Exception = ex.InnerException.Message;
					if (ex.InnerException.Message.Contains("Timeout Expired"))
					{
						ViewBag.Exception += " Try to narrow down your query.";
					}
				}
				catch (Exception ex)
				{
					ViewBag.Exception = ex.Message;
				}
				return PartialView("_List", msgsList);
			}
		}

		IQueryable<InboxMessage> LiveMessagesQuery(Query query)
		{
			var inboxQuery = eHubTransactionsContext.eHubInboxMessageDisplays.Select(i => i);
			var outboxQuery = eHubTransactionsContext.eHubOutboxMessageDisplays.Select(o => o);
			var outboxRequired = false;
			var outboxIncluded = false;

			if (query.ToDate.HasValue)
			{
				inboxQuery = inboxQuery.Where(i => i.EI_InsertUTC < query.ToDate);
				outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_InsertUTC < query.ToDate);
			}

			if (query.FromDate.HasValue)
			{
				inboxQuery = inboxQuery.Where(i => i.EI_InsertUTC > query.FromDate);
				outboxQuery = outboxQuery.Where(o => o.OI_InsertUTC > query.FromDate && o.eHubInboxMessageDisplay.EI_InsertUTC > query.FromDate);
			}

			if (!string.IsNullOrWhiteSpace(query.AnyRole))
			{
				inboxQuery = inboxQuery.Where(i => i.eHubClient_Sender.CC_ID.StartsWith(query.AnyRole) || i.eHubClient_Recipient.CC_ID.StartsWith(query.AnyRole));
				outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.eHubClient_Sender.CC_ID.StartsWith(query.AnyRole)
													 || o.eHubInboxMessageDisplay.eHubClient_Recipient.CC_ID.StartsWith(query.AnyRole)
													 || o.eHubClient_Sender.CC_ID.StartsWith(query.AnyRole)
													 || o.eHubClient_Recipient.CC_ID.StartsWith(query.AnyRole));
				outboxIncluded = true;
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(query.SenderInbox))
				{
					inboxQuery = inboxQuery.Where(i => i.eHubClient_Sender.CC_ID.StartsWith(query.SenderInbox));
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.eHubClient_Sender.CC_ID.StartsWith(query.SenderInbox));
				}
				if (!string.IsNullOrWhiteSpace(query.RecipientInbox))
				{
					inboxQuery = inboxQuery.Where(i => i.eHubClient_Recipient.CC_ID.StartsWith(query.RecipientInbox));
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.eHubClient_Recipient.CC_ID.StartsWith(query.RecipientInbox));
				}
				if (!string.IsNullOrWhiteSpace(query.SenderOutbox))
				{
					outboxQuery = outboxQuery.Where(o => o.eHubClient_Sender.CC_ID.StartsWith(query.SenderOutbox));
					outboxRequired = true;
				}
				if (!string.IsNullOrWhiteSpace(query.RecipientOutbox))
				{
					outboxQuery = outboxQuery.Where(o => o.eHubClient_Recipient.CC_ID.StartsWith(query.RecipientOutbox));
					outboxRequired = true;
				}
			}

			switch (query.Status)
			{
				case "Received":
					inboxQuery = inboxQuery.Where(i => i.EI_Status == 0);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_Status == 0);
					break;
				case "Processing":
					inboxQuery = inboxQuery.Where(i => i.EI_Status == 1);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_Status == 1);
					break;
				case "Not Processed":
					inboxQuery = inboxQuery.Where(i => i.EI_Status == 0 || i.EI_Status == 1);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_Status == 0 ||
														 o.eHubInboxMessageDisplay.EI_Status == 1);
					break;
				case "Processed":
					outboxQuery = outboxQuery.Where(o => o.OI_Status == 0 && o.eHubInboxMessageDisplay.EI_Status == 2);
					outboxRequired = true;
					break;
				case "Sending":
					outboxQuery = outboxQuery.Where(o => o.OI_Status == 1 && o.eHubInboxMessageDisplay.EI_Status == 2);
					outboxRequired = true;
					break;
				case "Not Delivered":
					inboxQuery = inboxQuery.Where(i => i.EI_Status != 3 && i.EI_Status != 255);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_Status == 0 ||
														 o.eHubInboxMessageDisplay.EI_Status == 1 ||
														 o.eHubInboxMessageDisplay.EI_Status == 2 && (o.OI_Status == 0 || o.OI_Status == 1));
					break;
				case "Delivered":
					inboxQuery = inboxQuery.Where(i => i.EI_Status == 3);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_Status == 3);
					break;
				case "Error":
					inboxQuery = inboxQuery.Where(i => i.EI_Status == 255);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_Status == 255);
					break;
			}

			if (!string.IsNullOrWhiteSpace(query.ApplicationCode))
			{
				inboxQuery = inboxQuery.Where(i => i.EI_ApplicationCode == query.ApplicationCode);
				outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_ApplicationCode == query.ApplicationCode);
			}

			if (query.Key.HasValue)
			{
				switch (query.KeyType)
				{
					case "EI_PK":
						inboxQuery = inboxQuery.Where(i => i.EI_PK == query.Key);
						outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_PK == query.Key);
						break;
					case "OI_PK":
						outboxQuery = outboxQuery.Where(o => o.OI_PK == query.Key);
						outboxRequired = true;
						break;
					case "MsgID":
						inboxQuery = inboxQuery.Where(i => i.EI_MessageTrackingID.ToUpper() == query.Key.ToString());
						outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageDisplay.EI_MessageTrackingID.ToUpper() == query.Key.ToString() || o.OI_MessageTrackingID.ToUpper() == query.Key.ToString().ToUpper());
						outboxIncluded = true;
						break;
				}
			}

			if (outboxRequired)
			{
				inboxQuery = outboxQuery.Select(o => o.eHubInboxMessageDisplay).Distinct();
			}
			else if (outboxIncluded)
			{
				inboxQuery = inboxQuery.GroupJoin(outboxQuery, i => i.EI_PK, o => o.OI_EI_InboxPK, (i, io) => new { i, io }).Where(t => !t.io.Any()).Select(t => t.i);
				inboxQuery = inboxQuery.Union(outboxQuery.Select(o => o.eHubInboxMessageDisplay));
			}

			return inboxQuery.Select(i => new InboxMessage
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
				OutboxMessages = i.eHubOutboxMessageDisplays.Select(o => new OutboxMessage
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
			}).OrderByDescending(i => i.Received).Take(Constants.MaxMessagesInPage);
		}

		IQueryable<InboxMessage> MovedMessagesQuery(Query query)
		{
			var inboxQuery = eHubTransactionsContext.eHubInboxMessageArchives.Select(i => i);
			var outboxQuery = eHubTransactionsContext.eHubOutboxMessageArchives.Select(o => o);
			var outboxRequired = false;
			var outboxIncluded = false;

			if (query.ToDate.HasValue)
			{
				inboxQuery = inboxQuery.Where(i => i.EI_InsertUTC < query.ToDate);
				outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.EI_InsertUTC < query.ToDate);
			}

			if (query.FromDate.HasValue)
			{
				inboxQuery = inboxQuery.Where(i => i.EI_InsertUTC > query.FromDate);
				outboxQuery = outboxQuery.Where(o => o.OI_InsertUTC > query.FromDate && o.eHubInboxMessageArchive.EI_InsertUTC > query.FromDate);
			}

			if (!string.IsNullOrWhiteSpace(query.AnyRole))
			{
				inboxQuery = inboxQuery.Where(i => i.eHubClient_Sender.CC_ID.StartsWith(query.AnyRole) || i.eHubClient_Recipient.CC_ID.StartsWith(query.AnyRole));
				outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.eHubClient_Sender.CC_ID.StartsWith(query.AnyRole)
													 || o.eHubInboxMessageArchive.eHubClient_Recipient.CC_ID.StartsWith(query.AnyRole)
													 || o.eHubClient_Sender.CC_ID.StartsWith(query.AnyRole)
													 || o.eHubClient_Recipient.CC_ID.StartsWith(query.AnyRole));
				outboxIncluded = true;
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(query.SenderInbox))
				{
					inboxQuery = inboxQuery.Where(i => i.eHubClient_Sender.CC_ID.StartsWith(query.SenderInbox));
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.eHubClient_Sender.CC_ID.StartsWith(query.SenderInbox));
				}
				if (!string.IsNullOrWhiteSpace(query.RecipientInbox))
				{
					inboxQuery = inboxQuery.Where(i => i.eHubClient_Recipient.CC_ID.StartsWith(query.RecipientInbox));
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.eHubClient_Recipient.CC_ID.StartsWith(query.RecipientInbox));
				}
				if (!string.IsNullOrWhiteSpace(query.SenderOutbox))
				{
					outboxQuery = outboxQuery.Where(o => o.eHubClient_Sender.CC_ID.StartsWith(query.SenderOutbox));
					outboxRequired = true;
				}
				if (!string.IsNullOrWhiteSpace(query.RecipientOutbox))
				{
					outboxQuery = outboxQuery.Where(o => o.eHubClient_Recipient.CC_ID.StartsWith(query.RecipientOutbox));
					outboxRequired = true;
				}
			}

			switch (query.Status)
			{
				case "Delivered":
					inboxQuery = inboxQuery.Where(i => i.EI_Status == 3);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.EI_Status == 3);
					break;
				case "Error":
					inboxQuery = inboxQuery.Where(i => i.EI_Status == 255);
					outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.EI_Status == 255);
					break;
			}

			if (!string.IsNullOrWhiteSpace(query.ApplicationCode))
			{
				inboxQuery = inboxQuery.Where(i => i.EI_ApplicationCode == query.ApplicationCode);
				outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.EI_ApplicationCode == query.ApplicationCode);
			}

			if (query.Key.HasValue)
			{
				switch (query.KeyType)
				{
					case "EI_PK":
						inboxQuery = inboxQuery.Where(i => i.EI_PK == query.Key);
						outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.EI_PK == query.Key);
						break;
					case "OI_PK":
						outboxQuery = outboxQuery.Where(o => o.OI_PK == query.Key);
						outboxRequired = true;
						break;
					case "MsgID":
						inboxQuery = inboxQuery.Where(i => i.EI_MessageTrackingID.ToUpper() == query.Key.ToString().ToUpper());
						outboxQuery = outboxQuery.Where(o => o.eHubInboxMessageArchive.EI_MessageTrackingID.ToUpper() == query.Key.ToString() || o.OI_MessageTrackingID.ToUpper() == query.Key.ToString().ToUpper());
						outboxIncluded = true;
						break;
				}
			}

			if (outboxRequired)
			{
				inboxQuery = outboxQuery.Select(o => o.eHubInboxMessageArchive).Distinct();
			}
			else if (outboxIncluded)
			{
				inboxQuery = inboxQuery.GroupJoin(outboxQuery, i => i.EI_PK, o => o.OI_EI_InboxPK, (i, io) => new { i, io }).Where(t => !t.io.Any()).Select(t => t.i);
				inboxQuery = inboxQuery.Union(outboxQuery.Select(o => o.eHubInboxMessageArchive));
			}

			return inboxQuery.Select(i => new InboxMessage
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
				OutboxMessages = i.eHubOutboxMessageArchives.Select(o => new OutboxMessage
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
			}).OrderByDescending(i => i.Received).Take(Constants.MaxMessagesInPage);
		}

		IQueryable<InboxMessage> ArchivedMessagesQuery(Query query, eHubArchiveOnlineContext context)
		{
			var archiveQuery = context.eHubArchiveMessages
				.GroupJoin(context.eHubArchiveMessages,
					i => new { i.AM_ReceivedFromSenderUTC, i.AM_EI_PK },
					o => new { o.AM_ReceivedFromSenderUTC, o.AM_EI_PK },
					(i, io) => new InboxMessage
					{
						Received = i.AM_ReceivedFromSenderUTC,
						Sender = i.eHubClient_SenderInbox.CC_ID,
						SenderObject = i.eHubClient_SenderInbox,
						Recipient = i.eHubClient_RecipientInbox.CC_ID,
						RecipientObject = i.eHubClient_RecipientInbox,
						ApplicationCode = i.AM_ApplicationCode,
						Status = i.AM_Status,
						PK = null,
						AM_PK = i.AM_PK,
						AM_EI_PK = i.AM_EI_PK,
						MessageTrackingID = i.AM_InboxMessageTrackingID.HasValue ? i.AM_InboxMessageTrackingID.Value.ToString() : null,
						FileName = i.AM_InboxFileNameOverride,
						Subject = i.AM_EmailSubjectOverride,
						MessageType = i.eHubRecipientMessageType == null ? "None" : i.eHubRecipientMessageType.DT_Code ?? "None",
						OutboxMessages = io.Where(o => o.eHubClient_RecipientOutbox != null).Select(o => new OutboxMessage
						{
							Sender = o.eHubClient_SenderOutbox.CC_ID,
							SenderObject = o.eHubClient_SenderOutbox,
							Recipient = o.eHubClient_RecipientOutbox.CC_ID,
							RecipientObject = o.eHubClient_RecipientOutbox,
							Status = o.AM_Status,
							Processed = o.AM_ReadyForDeliveryUTC,
							Delivered = o.AM_SentToRecipientUTC,
							PK = null,
							AM_PK = o.AM_PK,
							MessageTrackingID = o.AM_OutboxMessageTrackingID.HasValue ? o.AM_OutboxMessageTrackingID.Value.ToString() : null,
							FileName = o.AM_OutboxFileNameOverride,
							Subject = o.AM_EmailSubjectOverride,
						})
					});

			if (query.Received.HasValue)
			{
				archiveQuery = archiveQuery.Where(a => a.Received == query.Received);
			}
			if (query.ToDate.HasValue)
			{
				archiveQuery = archiveQuery.Where(a => a.Received < query.ToDate);
			}
			if (query.FromDate.HasValue)
			{
				archiveQuery = archiveQuery.Where(a => a.Received > query.FromDate);
			}
			if (!string.IsNullOrWhiteSpace(query.AnyRole))
			{
				archiveQuery = archiveQuery.Where(a => a.Sender.StartsWith(query.AnyRole)
														|| a.Recipient.StartsWith(query.AnyRole)
														|| a.OutboxMessages.Any(o => o.Sender.StartsWith(query.AnyRole) || o.Recipient.StartsWith(query.AnyRole)));
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(query.SenderInbox))
				{
					archiveQuery = archiveQuery.Where(a => a.Sender.StartsWith(query.SenderInbox));
				}
				if (!string.IsNullOrWhiteSpace(query.RecipientInbox))
				{
					archiveQuery = archiveQuery.Where(a => a.Recipient.StartsWith(query.RecipientInbox));
				}
				if (!string.IsNullOrWhiteSpace(query.SenderOutbox))
				{
					archiveQuery = archiveQuery.Where(a => a.OutboxMessages.Any(o => o.Sender.StartsWith(query.SenderOutbox)));
				}
				if (!string.IsNullOrWhiteSpace(query.RecipientOutbox))
				{
					archiveQuery = archiveQuery.Where(a => a.OutboxMessages.Any(o => o.Recipient.StartsWith(query.RecipientOutbox)));
				}
			}

			switch (query.Status)
			{
				case "Error":
					archiveQuery = archiveQuery.Where(a => a.Status == 255);
					break;
			}

			if (!string.IsNullOrWhiteSpace(query.ApplicationCode))
			{
				archiveQuery = archiveQuery.Where(a => a.ApplicationCode == query.ApplicationCode);
			}

			if (query.Key.HasValue)
			{
				switch (query.KeyType)
				{
					case "AM_PK":
						archiveQuery = archiveQuery.Where(a => a.AM_PK == query.Key || a.OutboxMessages.Any(o => o.AM_PK == query.Key));
						break;
					case "MsgID":
						archiveQuery = archiveQuery.Where(a => a.MessageTrackingID == query.Key.ToString() || a.OutboxMessages.Any(o => o.MessageTrackingID == query.Key.ToString()));
						break;
					case "EI_PK":
						archiveQuery = archiveQuery.Where(a => a.AM_EI_PK == query.Key);
						break;
				}
			}

			return archiveQuery.OrderByDescending(a => a.Received).Take(Constants.MaxMessagesInPage);
		}

		public ActionResult Details(Guid? EI_PK, Guid? AM_PK, long? Ref)
		{
			InboxMessage result = null;
			var moved = false;

			using (var traneHub = eHubTransactionsContext.BeginTransaction(IsolationLevel.ReadUncommitted))
			using (var tranArchiveSecondary = eHubArchiveOnlineSecondaryContext.BeginTransaction(IsolationLevel.ReadUncommitted))
			{
				IDbTransaction tran;
				eHubTransactionsContext.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseCommandTimeout"]);
				eHubArchiveOnlineSecondaryContext.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseCommandTimeout"]);

				if (AM_PK.HasValue)
				{
					tran = tranArchiveSecondary;
					var received = eHubArchiveOnlineSecondaryContext.eHubArchiveMessages.Where(a => a.AM_PK == AM_PK).Select(a => a.AM_ReceivedFromSenderUTC).FirstOrDefault();

					if (received.HasValue)
					{
						result = ArchivedMessagesQuery(new Query { KeyType = "AM_PK", Key = AM_PK, Received = received }, eHubArchiveOnlineSecondaryContext).FirstOrDefault();
					}
				}
				else
				{
					var query = new Query { KeyType = "EI_PK", Key = EI_PK };
					if (Ref.HasValue)
					{
						query.Received = DateTime.FromBinary(Ref.Value);
					}
					tran = tranArchiveSecondary;
					result = ArchivedMessagesQuery(query, eHubArchiveOnlineSecondaryContext).FirstOrDefault();
					if (result == null)
					{
						tran = traneHub;
						result = LiveMessagesQuery(query).FirstOrDefault();
						{
							if (result == null)
							{
								result = MovedMessagesQuery(query).FirstOrDefault();
								if (result != null)
								{
									moved = true;
								}
							}
						}
					}
				}

				if (result != null)
				{
					result.Error = result.AM_PK.HasValue
						? eHubArchiveOnlineSecondaryContext.eHubArchiveMessages
							.Where(a => a.AM_PK == result.AM_PK).Select(a => a.AM_ErrorMessage).SingleOrDefault()
						: eHubTransactionsContext.eHubErrors.Where(e => e.EE_EI_Inbox == result.PK)
							.OrderByDescending(e => e.EE_DateTimeUTC).Select(e => e.EE_Description)
							.FirstOrDefault();


					if (result.OutboxMessages.Any(o => o.Status == 255))
					{
						result.OutboxMessages = result.OutboxMessages.ToList();
						foreach (var o in result.OutboxMessages.Where(o => o.Status == 255))
						{
							o.Error = o.AM_PK.HasValue
								? eHubArchiveOnlineSecondaryContext.eHubArchiveMessages
									.Where(a => a.AM_PK == o.AM_PK).Select(a => a.AM_ErrorMessage).SingleOrDefault()
								: eHubTransactionsContext.eHubErrors.Where(e => e.EE_OI_Outbox == o.PK)
									.Select(e => e.EE_Description).FirstOrDefault();
						}
					}

					UpdateMessageLengths(result, moved, tran);
				}
				return View(result);
			}
		}

		public ActionResult MessageBasicDetails(Guid? EI_PK, Guid? AM_PK, long? Ref)
		{
			InboxMessage result = null;
			var moved = false;

			using (var traneHub = eHubTransactionsContext.BeginTransaction(IsolationLevel.ReadUncommitted))
			using (var tranArchiveSecondary = eHubArchiveOnlineSecondaryContext.BeginTransaction(IsolationLevel.ReadUncommitted))
			{
				IDbTransaction tran;
				eHubTransactionsContext.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseCommandTimeout"]);
				eHubArchiveOnlineSecondaryContext.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseCommandTimeout"]);

				if (AM_PK.HasValue)
				{
					tran = tranArchiveSecondary;
					var received = eHubArchiveOnlineSecondaryContext.eHubArchiveMessages.Where(a => a.AM_PK == AM_PK).Select(a => a.AM_ReceivedFromSenderUTC).FirstOrDefault();

					if (received.HasValue)
					{
						result = ArchivedMessagesQuery(new Query { KeyType = "AM_PK", Key = AM_PK, Received = received }, eHubArchiveOnlineSecondaryContext).FirstOrDefault();
					}
				}
				else
				{
					var query = new Query { KeyType = "EI_PK", Key = EI_PK };
					tran = traneHub;
					result = LiveMessagesQuery(query).FirstOrDefault();
					{
						if (result == null)
						{
							result = MovedMessagesQuery(query).FirstOrDefault();
							if (result != null)
							{
								moved = true;
							}
							else
							{
								if (Ref.HasValue)
								{
									query.Received = DateTime.FromBinary(Ref.Value);
									tran = tranArchiveSecondary;
									result = ArchivedMessagesQuery(query, eHubArchiveOnlineSecondaryContext).FirstOrDefault();
								}
							}
						}
					}
				}

				if (result != null)
				{
					result.Error = result.AM_PK.HasValue
						? eHubArchiveOnlineSecondaryContext.eHubArchiveMessages
							.Where(a => a.AM_PK == result.AM_PK).Select(a => a.AM_ErrorMessage).SingleOrDefault()
						: eHubTransactionsContext.eHubErrors.Where(e => e.EE_EI_Inbox == result.PK)
							.OrderByDescending(e => e.EE_DateTimeUTC).Select(e => e.EE_Description)
							.FirstOrDefault();


					if (result.OutboxMessages.Any(o => o.Status == 255))
					{
						result.OutboxMessages = result.OutboxMessages.ToList();
						foreach (var o in result.OutboxMessages.Where(o => o.Status == 255))
						{
							o.Error = o.AM_PK.HasValue
								? eHubArchiveOnlineSecondaryContext.eHubArchiveMessages
									.Where(a => a.AM_PK == o.AM_PK).Select(a => a.AM_ErrorMessage).SingleOrDefault()
								: eHubTransactionsContext.eHubErrors.Where(e => e.EE_OI_Outbox == o.PK)
									.Select(e => e.EE_Description).FirstOrDefault();
						}
					}
					ViewBag.PKIDsHTML = result.GetPKIDsHTML();
					ViewBag.ApplicationCodeAndDescription = result.GetApplicationCodeAndDescription();
					ViewBag.InboxSender = result.GetMoreInfo(result.SenderObject);
					ViewBag.InboxRecipient = result.GetMoreInfo(result.RecipientObject);
					UpdateMessageLengths(result, moved, tran);
				}
				return PartialView("_MessageDetails", result);
			}
		}

		public ActionResult ChangeSearchType(string searchType)
		{
			TempData["searchType"] = searchType;

			return RedirectToAction("Query");
		}

		void UpdateMessageLengths(InboxMessage inboxMessage, bool moved, IDbTransaction tran)
		{
			if (inboxMessage.AM_PK.HasValue)
			{
				var archiveContext = eHubArchiveOnlineSecondaryContext;
				inboxMessage.ContentRawLength = ReadBlob(archiveContext, "SELECT AM_SenderMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk", inboxMessage.AM_PK.Value, tran, true);
				inboxMessage.ContentXmlLength = ReadBlob(archiveContext, "SELECT AM_SenderMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk", inboxMessage.AM_PK.Value, tran);
				foreach (var outboxMessage in inboxMessage.OutboxMessages)
				{
					outboxMessage.ContentXmlLength = ReadBlob(archiveContext, "SELECT AM_RecipientMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk", outboxMessage.AM_PK.Value, tran);
					outboxMessage.ContentRawLength = ReadBlob(archiveContext, "SELECT AM_RecipientMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk", outboxMessage.AM_PK.Value, tran, true);
				}
			}
			else if (moved)
			{
				inboxMessage.ContentRawLength = ReadBlob(eHubTransactionsContext, "SELECT EI_Content FROM eHubInboxMessageArchive WHERE EI_PK = @pk", inboxMessage.PK.Value, tran, true);
				inboxMessage.ContentXmlLength = ReadBlob(eHubTransactionsContext, "SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk", inboxMessage.PK.Value, tran);
				foreach (var outboxMessage in inboxMessage.OutboxMessages)
				{
					outboxMessage.ContentXmlLength = ReadBlob(eHubTransactionsContext, "SELECT OI_XmlContent FROM eHubOutboxMessageArchive WHERE OI_PK = @pk", outboxMessage.PK.Value, tran);
					outboxMessage.ContentRawLength = ReadBlob(eHubTransactionsContext, "SELECT OI_Content FROM eHubOutboxMessageArchive WHERE OI_PK = @pk", outboxMessage.PK.Value, tran, true);
				}
			}
			else
			{
				inboxMessage.ContentRawLength = ReadBlob(eHubTransactionsContext, "SELECT EI_Content FROM eHubInboxMessage WHERE EI_PK = @pk", inboxMessage.PK.Value, tran, true);
				inboxMessage.ContentXmlLength = ReadBlob(eHubTransactionsContext, "SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk", inboxMessage.PK.Value, tran);
				foreach (var outboxMessage in inboxMessage.OutboxMessages)
				{
					outboxMessage.ContentXmlLength = ReadBlob(eHubTransactionsContext, "SELECT OI_XmlContent FROM eHubOutboxMessage WHERE OI_PK = @pk", outboxMessage.PK.Value, tran);
					outboxMessage.ContentRawLength = ReadBlob(eHubTransactionsContext, "SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK = @pk", outboxMessage.PK.Value, tran, true);
				}
			}
		}

		public FileStreamResult Download(string id, Guid? EI_PK, Guid? OI_PK, Guid? AM_PK, bool isPartialResult = false)
		{
			var commandText = string.Empty;
			var pk = Guid.Empty;
			var raw = false;
			var outputStream = new MemoryStream();
			ContextBase context = null;

			var archiveContext = eHubArchiveOnlineSecondaryContext;

			switch (id)
			{
				case "InboxRaw":
					raw = true;
					if (AM_PK.HasValue)
					{
						context = archiveContext;
						commandText = "SELECT AM_SenderMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk";
						pk = AM_PK.Value;
					}
					else
					{
						context = eHubTransactionsContext;
						commandText = eHubTransactionsContext.eHubInboxMessageArchives.Any(i => i.EI_PK == EI_PK) ? "SELECT EI_Content FROM eHubInboxMessageArchive WHERE EI_PK = @pk" : "SELECT EI_Content FROM eHubInboxMessage WHERE EI_PK = @pk";
						pk = EI_PK.GetValueOrDefault();
					}
					break;
				case "InboxXml":
					if (AM_PK.HasValue)
					{
						context = archiveContext;
						commandText = "SELECT AM_SenderMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk";
						pk = AM_PK.Value;
					}
					else
					{
						context = eHubTransactionsContext;
						commandText = "SELECT EX_XmlContent FROM eHubInboxXmlContent WHERE EX_EI_Inbox = @pk";
						pk = EI_PK.GetValueOrDefault();
					}
					break;
				case "OutboxRaw":
					raw = true;
					if (AM_PK.HasValue)
					{
						context = archiveContext;
						commandText = "SELECT AM_RecipientMessageRaw FROM eHubArchiveMessage WHERE AM_PK = @pk";
						pk = AM_PK.Value;
					}
					else
					{
						context = eHubTransactionsContext;
						commandText = eHubTransactionsContext.eHubOutboxMessageArchives.Any(o => o.OI_PK == OI_PK) ? "SELECT OI_Content FROM eHubOutboxMessageArchive WHERE OI_PK = @pk" : "SELECT OI_Content FROM eHubOutboxMessage WHERE OI_PK = @pk";
						pk = OI_PK.GetValueOrDefault();
					}
					break;
				case "OutboxXml":
					if (AM_PK.HasValue)
					{
						context = archiveContext;
						commandText = "SELECT AM_RecipientMessageXML FROM eHubArchiveMessage WHERE AM_PK = @pk";
						pk = AM_PK.Value;
					}
					else
					{
						context = eHubTransactionsContext;
						commandText = eHubTransactionsContext.eHubOutboxMessageArchives.Any(o => o.OI_PK == OI_PK) ? "SELECT OI_XmlContent FROM eHubOutboxMessageArchive WHERE OI_PK = @pk" : "SELECT OI_XmlContent FROM eHubOutboxMessage WHERE OI_PK = @pk";
						pk = OI_PK.GetValueOrDefault();
					}
					break;
			}

			using (var tran = context is eHubTransactionsContext ? eHubTransactionsContext.BeginTransaction(IsolationLevel.ReadUncommitted)
																 : archiveContext.BeginTransaction(IsolationLevel.ReadUncommitted))
			{
				ReadBlob(context, commandText, pk, tran, raw, outputStream);
			}

			if (isPartialResult)
			{
				outputStream = CreatePartialResult(outputStream);
			}

			return File(outputStream, raw ? "text/plain; charset=utf-8" : "text/xml; charset=utf-8");
		}

		internal MemoryStream CreatePartialResult(MemoryStream stream)
		{
			byte[] byteArray;
			using (StreamReader reader = new StreamReader(stream))
			{
				string outputString = reader.ReadToEnd();
				const int cuttOff = 1024;
				outputString = outputString.Length > cuttOff ? outputString.Substring(0, cuttOff) : outputString;
				byteArray = Encoding.ASCII.GetBytes(outputString);
			}
			return new MemoryStream(byteArray);
		}

		internal virtual long? ReadBlob(ContextBase context, string commandText, Guid pk, IDbTransaction tran, bool decodeAndDecompress = false, Stream outputStream = null)
		{
			long? length = null;
			using (var cmd = context.Connection.CreateCommand())
			{
				context.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["DatabaseCommandTimeout"]);
				if (context.Connection.State == ConnectionState.Closed)
				{
					context.Connection.Open();
				}
				cmd.CommandText = commandText;
				cmd.Parameters.Add(new SqlParameter("@pk", pk));
				cmd.CommandType = CommandType.Text;
				cmd.Transaction = tran;
				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.Read() && !rdr.IsDBNull(0))
					{
						var buffer = new char[2000];
						length = 0;
						long index = 0;
						using (var nonOutputStream = new MemoryStream())
						{
							long count;
							if (decodeAndDecompress)
							{
								using (var decodedStream = new MemoryStream())
								using (var decoder = new CryptoStream(decodedStream, new FromBase64Transform(), CryptoStreamMode.Write))
								using (var writer = new BinaryWriter(decoder, Encoding.UTF8, true))
								using (var decompressor = new GZipStream(decodedStream, CompressionMode.Decompress, true))
								{
									while ((count = rdr.GetChars(0, index, buffer, 0, buffer.Length)) > 0)
									{
										writer.Write(buffer, 0, (int)count);
										decodedStream.Position = 0;
										decompressor.CopyTo(outputStream ?? nonOutputStream);
										if (outputStream == null)
										{
											length += nonOutputStream.Length;
											nonOutputStream.SetLength(0);
										}
										decodedStream.SetLength(0);
										index += count;
									}
								}
							}
							else
							{
								using (var writer = new BinaryWriter(outputStream ?? nonOutputStream, Encoding.UTF8, true))
								{
									while ((count = rdr.GetChars(0, index, buffer, 0, buffer.Length)) > 0)
									{
										writer.Write(buffer, 0, (int)count);
										if (outputStream == null)
										{
											length += nonOutputStream.Length;
											nonOutputStream.SetLength(0);
										}
										index += count;
									}
								}
							}
						}
						if (outputStream != null)
						{
							length = outputStream.Length;
							outputStream.Position = 0;
						}
					}
				}
			}
			return length;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}
			if (disposing)
			{
				eHubTransactionsContext?.Dispose();
				eHubArchiveOnlineSecondaryContext?.Dispose();
			}
			disposed = true;
			base.Dispose(disposing);
		}
	}
}
