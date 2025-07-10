using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	class EdiTrustedMessagingConfigLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductTypeList()
		{
			var trustedServices = new CodeDescriptionBoolCollection
			{
				{ "DDD", (NoResString)"Demo Service", true },
				{ "TCA", (NoResString)"Inactive Demo Service", false }
			};
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, trustedServices);

			var lookups = new EdiTrustedMessagingConfigLookups(null);
			var products = lookups.ProductTypeList;
			AssertEquals(true, products.ContainsCode("DDD"));
			AssertEquals(false, products.ContainsCode("TCA"));
		}
	}
}
