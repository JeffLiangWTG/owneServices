using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Billing.Business;

namespace Enterprise.BufferManagement.Business
{
	public class PAVEFeatureUsageCollector : IPAVEUsageCollector
	{
		public void ReportTaskStatusChange(IDictionary<string, string> changeEvents)
		{
			var eventData = changeEvents.Select(entry => (name: entry.Key, value: (object)entry.Value)).ToArray();
			UsageCollector.Report("STC", eventData);
		}

		public void ReportVisualBoardOpen(IDictionary<string, string> changeEvents)
		{
			var eventData = changeEvents.Select(entry => (name: entry.Key, value: (object)entry.Value)).ToArray();
			UsageCollector.Report("VBO", eventData);
		}

		public void ReportDeferralWithESDOrADDRemoved(IDictionary<string, string> changeEvents)
		{
			var eventData = changeEvents.Select(entry => (name: entry.Key, value: (object)entry.Value)).ToArray();
			UsageCollector.Report("DEA", eventData);
		}
	}
}
