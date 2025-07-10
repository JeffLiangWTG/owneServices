using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	public class XtMessageEventsCollection : NonPersistentBusinessObjectCollection<XtMessageEvent>
	{
		public XtMessageEventsCollection(EDIInterchange interchange)
			: base(interchange.Factory)
		{
			this.xtMsgId = (ulong)interchange.EI_XTInternalMsgID;
			this.interchange = interchange;
		}

		readonly ulong xtMsgId;
		readonly EDIInterchange interchange;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("xT message log cannot be created by users in grid.");
		}

		protected override bool AllowNewCore => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log")]
		public void LoadCollection(IXtMessageEventsReaderClient client, bool shouldRefresh = true)
		{
			try
			{
				RemoveAll();
				if (client != null)
				{
					var msgIds = interchange.EI_ShowRelatedxTEventsLog ? GetRelatedInterchangesByEI_SessionGUID() : new HashSet<ulong> { xtMsgId };
					var eventKey = $"XtMessageEventsCollection_{string.Join("_", msgIds)}_{interchange.EI_ShowRelatedxTEventsLog}";
					if (shouldRefresh)
					{
						Factory.ClearCachedValue<IReadOnlyList<IXtMessageEventData>>(eventKey);
					}

					var result = Factory.GetCachedValue(eventKey, () => client.GetMsgEvents(msgIds, interchange.EI_ShowRelatedxTEventsLog));
					if (result.Count >= 1)
					{
						LoadxTEvent(result);
					}
					else
					{
						AddSpecificLog("No events found on xT server for the Interchange.", xtMsgId);
					}
				}
				else
				{
					AddSpecificLog("Xt Client is null.", xtMsgId);
				}
			}
			catch
			{
				AddSpecificLog("Connection Error.", xtMsgId);
			}
			finally
			{
				Sort(nameof(XtMessageEvent.LogTime));
				for (int i = 0; i < Count; i++)
				{
					this[i].Sequence = i + 1;
				}
			}
		}

		void LoadxTEvent(IReadOnlyList<IXtMessageEventData> xTEventList)
		{
			foreach (var xTEventData in xTEventList)
			{
				var interchangeForXtEvent = GetInterchangeByXtMsgID(xTEventData.XtMsgId);
				if (interchange.EI_ShowAllxTEventsLog || KeyXtEventTypes.Contains(xTEventData.LogEvent))
				{
					var xTEvent = new XtMessageEvent(interchangeForXtEvent, xTEventData);
					Add(xTEvent);
				}
			}
		}

		EDIInterchange GetInterchangeByXtMsgID(ulong xTMessageId)
		{
			EDIInterchange result = null;
			if (xTMessageId.Equals(xtMsgId))
			{
				result = interchange;
			}
			else
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_XTInternalMsgID, ZLong.ParseSafe(xTMessageId.ToString(), 0));
				result = Factory.GetCachedValue($"EDIInterchange_{xTMessageId}", () => Factory.LoadTop1<EDIInterchange>(query));
			}

			return result;
		}

		HashSet<ulong> GetRelatedInterchangesByEI_SessionGUID()
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, interchange.EI_SessionGUID);
			return Factory.GetCachedValue($"{xtMsgId}_RelatedInterchanges", () => Factory.Load<EDIInterchange>(query).Select(x => (ulong)x.EI_XTInternalMsgID).ToHashSet());
		}

		HashSet<int> KeyXtEventTypes => new ()
		{
			0,
			xTEventTypeCode.Codes.ReceivedFromApplication,
			xTEventTypeCode.Codes.MsgReceivedFromContract,
			xTEventTypeCode.Codes.MsgAcceptedByContract,
			xTEventTypeCode.Codes.SentByHttpClient,
			xTEventTypeCode.Codes.SentWithSMTP,
			xTEventTypeCode.Codes.ReceivedByHttpServer,
			xTEventTypeCode.Codes.ReceivedBySMTP,
			xTEventTypeCode.Codes.ProcessingError,
			xTEventTypeCode.Codes.NodeSendingError,
			xTEventTypeCode.Codes.HttpClientSendingError,
			xTEventTypeCode.Codes.SentToApplication
		};

		public void AddSpecificLog(string text, ulong id)
		{
			var dic = new Dictionary<string, string>
			{
				{ (NoResString)"logtext", text }
			};
			Add(new XtMessageEvent(null, new XtMessageEventData(1, id, Newtonsoft.Json.JsonConvert.SerializeObject(dic))));
		}
	}
}
