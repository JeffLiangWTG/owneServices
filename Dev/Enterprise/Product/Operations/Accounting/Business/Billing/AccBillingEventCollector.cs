using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Billing
{
	public class AccBillingEventCollector
	{
		public static AccBillingEventCollector GetInstance(BusinessObjectFactory factory) => factory.GetCachedValue("AccountingBillingHeader", () => new AccBillingEventCollector(), CacheStalenessPolicy.StaleOnFactorySave);

		AccBillingEventCollector()
		{
			Events = new Dictionary<AccBillingEventInfoKey, HashSet<string>>();
		}

		public void AddEvent(string billingCode, ZGuid billedBizOPK, ZString billedBizOTableCode, string eventCode)
		{
			var key = new AccBillingEventInfoKey(billingCode, billedBizOPK, billedBizOTableCode);
			if (Events.ContainsKey(key))
			{
				Events[key].Add(eventCode);
			}
			else
			{
				Events.Add(key, new HashSet<string>() { eventCode });
			}
		}

		public IEnumerable<(ZGuid BilledBizOPK, ZString BilledBizOTableCode, string EventCode)> GetEvents(string billingCode)
		{
			var events = new List<(ZGuid BilledBizOPK, ZString BilledBizOTableCode, string EventCode)>();
			foreach (KeyValuePair<AccBillingEventInfoKey, HashSet<string>> eventInfo in Events)
			{
				if (eventInfo.Key.BillingCode == billingCode)
				{
					events.Add((eventInfo.Key.BilledBizOPK, eventInfo.Key.BilledBizOTableCode, GetEventName(eventInfo)));
				}
			}
			return events;

			string GetEventName(KeyValuePair<AccBillingEventInfoKey, HashSet<string>> valuePair)
			{
				return valuePair.Value.Count == 1 ? valuePair.Value.First() : AccBillingEvents.Codes.GenericPostEvent;
			}
		}

		public void ClearEvents(string billingCode, ZGuid billedBizOPK, ZString billedBizOTableCode)
		{
			var key = new AccBillingEventInfoKey(billingCode, billedBizOPK, billedBizOTableCode);
			if (Events.ContainsKey(key))
			{
				Events.Remove(key);
			}
		}

		Dictionary<AccBillingEventInfoKey, HashSet<string>> Events { get; }

		public class AccBillingEventInfoKey
		{
			public AccBillingEventInfoKey(string billingCode, ZGuid billedBizOPK, ZString billedBizOTableCode)
			{
				BilledBizOPK = billedBizOPK;
				BillingCode = billingCode;
				BilledBizOTableCode = billedBizOTableCode;
			}

			public ZGuid BilledBizOPK { get; }
			public ZString BilledBizOTableCode { get; }
			public string BillingCode { get; }

			public override bool Equals(object obj)
			{
				var key = (AccBillingEventInfoKey)obj;
				return key.BilledBizOPK == this.BilledBizOPK && key.BillingCode == this.BillingCode && key.BilledBizOTableCode == this.BilledBizOTableCode;
			}

			public override int GetHashCode()
			{
				return BilledBizOPK.GetHashCode() ^ BilledBizOTableCode.GetHashCode() ^ BillingCode.GetHashCode();
			}
		}
	}
}
