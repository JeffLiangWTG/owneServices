using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eServices.eHubDataModel.eHubArchiveOnline;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubAdmin.ViewModels.Messages;
using TimeZoneConverter;
using Query = eServices.eHubAdmin.ViewModels.Air.Query;

namespace eServices.eHubAdmin.Controllers
{
	public class AirController : Controller
	{
		readonly eHubTransactionsContext transactionsContext;
		readonly eHubArchiveOnlineContext archiveContext;

		public AirController() : this(new eHubTransactionsContext(), new eHubArchiveOnlineContext("eHubArchiveOnlineSecondaryContext")) { }

		public AirController(eHubTransactionsContext transactionsContext, eHubArchiveOnlineContext archiveContext)
		{
			this.transactionsContext = transactionsContext;
			this.archiveContext = archiveContext;
		}

		public ActionResult Query(Query query)
		{
			ViewBag.ST_ExpiryDays = transactionsContext.eHubSubscriptionTypes.Where(st => st.ST_ID == "AIRAWB").Select(st => st.ST_ExpiryDays).FirstOrDefault();
			return View(query);
		}

		public ActionResult List(Query query, string searchLimit)
		{
			ViewBag.TimeZone = TZConvert.GetTimeZoneInfo(query.TimeZoneIdIANA);
			ViewBag.ShowLoadMore = false;
			ViewBag.SearchLimit = null;

			var msgsList = new List<InboxMessage>();

			if (!string.IsNullOrEmpty(query.Waybill))
			{
				msgsList = WaybillQuery(query).SelectMany(x => x.ToList()).OrderByDescending(x => x.Received).ToList();
			}

			return PartialView("../Messages/_List", msgsList);
		}

		IEnumerable<IQueryable<InboxMessage>> WaybillQuery(Query query)
		{
			var trackingIds = transactionsContext.eHubSubscriptionValues.Where(sv => sv.eHubSubscriptionType.ST_ID == "AIRAWB" && sv.SV_Value.StartsWith(query.Waybill)).OrderByDescending(sv => sv.SV_SubscribedUTC).Select(sv => sv.SV_Reference).ToList();
			Func<string, IQueryable<InboxMessage>> transactionQryFunc = (id) => transactionsContext.eHubInboxMessages.Where(i => i.EI_MessageTrackingID == id).Select(i => new InboxMessage
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
			});

			Func<string, IQueryable<InboxMessage>> archiveQryFunc = (id) => archiveContext.eHubArchiveMessages.Where(a => a.AM_InboxMessageTrackingID == new Guid(id))
					.GroupJoin(archiveContext.eHubArchiveMessages,
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

			var queries = trackingIds.Select(id => transactionQryFunc(id).Count() != 0 ? transactionQryFunc(id) : archiveQryFunc(id));

			return queries;
		}
	}
}