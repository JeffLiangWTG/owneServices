using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.Billing.BorderWise;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IGenericUsageSetFactory
	{
		IEnumerable<IGenericUsageSet> GetUsageSets(BillingRunContext context, IReadOnlyDictionary<Guid, IBilledDatabase> mainDatabaseMap);
	}

	public class GenericUsageSetFactory : IGenericUsageSetFactory
	{
		public IEnumerable<IGenericUsageSet> GetUsageSets(BillingRunContext context, IReadOnlyDictionary<Guid, IBilledDatabase> mainDatabaseMap)
		{
			return new IGenericUsageSet[]  { new BorderWiseUsageSet(context, mainDatabaseMap) }
				.Concat(
					EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.OfType<UsageBillingPriceList>()
					.Select(x => new GenericUsageSet(x.ProductCode, x.RawUsageCategory, x.PriceListCode, context, mainDatabaseMap))
				 );
		}
	}
}
