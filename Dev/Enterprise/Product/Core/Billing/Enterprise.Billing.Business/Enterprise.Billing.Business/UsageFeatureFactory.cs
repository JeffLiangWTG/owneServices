using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Billing.Business
{
	public class UsageFeatureFactory
	{
		readonly Dictionary<string, RefStlFieldMapping> fieldMappings;

		public UsageFeatureFactory()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			fieldMappings = factory.Load<RefStlFieldMapping>(new ZQuery()).ToDictionary(f => f.SFM_FeatureCode.ToString());
		}

		public IUsageFeature Create(UsageFeatureProperties featureProperties)
		{
			if (fieldMappings.TryGetValue(featureProperties.Code, out var fieldMapping))
			{
				return new BilledFeature(featureProperties, fieldMapping);
			}

			return new UsageFeature(featureProperties);
		}
	}
}
