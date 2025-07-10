using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	static class GatewayProfitRedistributionHelper
	{
		public static bool IsSendingOrReceivingAgentGTAOrGTT(this ForwardingConsol consol) => consol.IsSendingAgentGTAOrGTT() || consol.IsReceivingAgentGTAOrGTT();

		public static bool IsSendingAgentGTAOrGTT(this ForwardingConsol consol) => consol.IsGatewayAgentOrGatewayAgentWithTariff(consol.JK_SendingForwarderHandlingType);

		public static bool IsReceivingAgentGTAOrGTT(this ForwardingConsol consol) => consol.IsGatewayAgentOrGatewayAgentWithTariff(consol.JK_ReceivingForwarderHandlingType);

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			return source.Where(element => seenKeys.Add(keySelector(element)));
		}
	}
}
