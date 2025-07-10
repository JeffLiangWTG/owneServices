using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Enterprise.ZArchitecture.Core;
using Grpc.Core;
using Grpc.Core.Utils;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Business
{
	public class MsgClientWithDeadline : BasicMsgClient, IXtMessageEventsReaderClient
	{
		public MsgClientWithDeadline(Msg.MsgClient client, CancellationToken token, TimeSpan messageTimeout) : base(client, messageTimeout, token)
		{
		}

		public HashSet<ulong> GetRelatedMessageIdIncludeSelf(HashSet<ulong> xtMsgId, HashSet<ulong> knownRelatedMsgIds = null, bool isRecursive = false, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			if (knownRelatedMsgIds == null)
			{
				knownRelatedMsgIds = new HashSet<ulong>();
				xtMsgId.ForEach(id => knownRelatedMsgIds.Add(id));
			}

			try
			{
				var getMsgAttributesListMessage = new GetMsgAttributesListMessage();
				getMsgAttributesListMessage.Ids.AddRange(xtMsgId.Select(id => new MsgIdUri { Msgid = id }));
				var msgAttributesListReply =
					client.GetMsgAttributesList(getMsgAttributesListMessage,
						deadline: deadline ?? Utils.GetDeadline(messageTimeout),
						cancellationToken: cancellationToken ?? token).ResponseStream.ToListAsync().Result;
				var regex = new Regex(localRefRegexPattern);
				var newIds = new HashSet<ulong>();
				foreach (var reply in msgAttributesListReply.Where(m => m.Errorcode == 0))
				{
					var refIds = reply.Msgattr.Where(x => regex.IsMatch(x.Key))
						.Select(a => a.Value)
						.SelectMany(v => v.Split(','))
						.Select(id => ulong.Parse(id.Trim()))
						.Where(id => !knownRelatedMsgIds.Contains(id));

					foreach (var id in refIds)
					{
						knownRelatedMsgIds.Add(id);
						newIds.Add(id);
					}
				}

				if (isRecursive && newIds.Count > 0)
				{
					GetRelatedMessageIdIncludeSelf(newIds, knownRelatedMsgIds, isRecursive: true, deadline, cancellationToken);
				}
			}
			catch
			{
			}

			return knownRelatedMsgIds;
		}

		static readonly string localRefRegexPattern = (NoResString)"ref[created|creator|tocontract|internalsender|external|reply]";

		public IReadOnlyList<IXtMessageEventData> GetMsgEvents(HashSet<ulong> xtMsgIds, bool isRecursive = true, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			var result = new List<IXtMessageEventData>();
			var processedMsgIds = new HashSet<ulong>();

			if (isRecursive)
			{
				xtMsgIds = GetRelatedMessageIdIncludeSelf(xtMsgIds, null, isRecursive: true, deadline, cancellationToken);
			}

			foreach (var xtMsgId in xtMsgIds)
			{
				if (!processedMsgIds.Contains(xtMsgId))
				{
					var eventList = GetMsgEvents(xtMsgId, deadline, cancellationToken);
					if (eventList != null && eventList.Any())
					{
						result.AddRange(eventList);
					}
					processedMsgIds.Add(xtMsgId);
				}
			}

			return result;
		}

		internal IReadOnlyList<IXtMessageEventData> GetMsgEvents(ulong xtMsgId, DateTime? deadline = null, CancellationToken? cancellationToken = null)
		{
			try
			{
				var reply = client.GetMsgEvents(new GetMsgEventsMessage { Id = new MsgIdUri { Msgid = xtMsgId } }, deadline: deadline ?? Utils.GetDeadline(messageTimeout), cancellationToken: cancellationToken ?? token);

				if (reply.Errorcode == 0)
				{
					var index = 1;
					return reply.Events
						.OrderBy(e => e.Eventattrs["time"]).Select(e => new XtMessageEventData(index++, xtMsgId, Newtonsoft.Json.JsonConvert.SerializeObject(e.Eventattrs))).ToList();
				}

				return new List<IXtMessageEventData>
				{
					new XtMessageEventData(1, xtMsgId, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> { { LogTextKey, XtEvents.GetXtEventsReaderErrorDescription(reply.Errorcode) } }))
				};
			}
			catch (RpcException rpcEx)
			{
				return new List<IXtMessageEventData>
				{
					new XtMessageEventData(1, xtMsgId, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> { { LogTextKey, $"{rpcEx.StatusCode}:{rpcEx.Status.Detail}" } }))
				};
			}
			catch (Exception ex)
			{
				return new List<IXtMessageEventData>
				{
					new XtMessageEventData(1, xtMsgId, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string> { { LogTextKey, ex.Message } }))
				};
			}
		}
	}
}
