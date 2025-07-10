using System;
using System.Collections.Generic;
using System.Threading;

namespace Enterprise.Messaging.Integration
{
	public interface IXtMessageEventsReaderClient
	{
		(string stateCode, string stateName) GetMsgState(ulong xtMsgId, DateTime? deadline = null, CancellationToken? cancellationToken = null);
		IReadOnlyList<IXtMessageEventData> GetMsgEvents(HashSet<ulong> xtMsgIds, bool isRecursive, DateTime? deadline = null, CancellationToken? cancellationToken = null);
	}
}
