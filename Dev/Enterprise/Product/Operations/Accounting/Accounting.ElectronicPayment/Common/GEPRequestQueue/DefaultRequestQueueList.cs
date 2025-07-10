using System.Collections.Generic;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal static class DefaultRequestQueueList
	{
		internal static IEnumerable<IGEPRequestQueue> Get()
		{
			yield return new QuoteRequestQueue();
			yield return new DealRequestQueue();
			yield return new BeneficiaryRequestQueue();
		}
	}
}
