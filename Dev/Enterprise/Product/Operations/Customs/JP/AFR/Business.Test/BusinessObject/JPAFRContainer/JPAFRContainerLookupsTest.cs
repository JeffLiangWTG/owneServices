using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsConventionContainerApplicationList()
		{
			var container = Factory.New<JPAFRContainer>();
			AssertEquals(Factory.GetCachedValue<CustomsConventionContainerApplicationList>(), container.Lookups.CustomsConventionContainerApplicationList);
		}

		public void TestContainerOwnershipCodeList()
		{
			var container = Factory.New<JPAFRContainer>();
			AssertEquals(Factory.GetCachedValue<ContainerOwnershipCodeList>(), container.Lookups.ContainerOwnershipCodeList);
		}

		public void TestVanningTypeCodeList()
		{
			var container = Factory.New<JPAFRContainer>();
			AssertEquals(Factory.GetCachedValue<VanningTypeCodeList>(), container.Lookups.VanningTypeCodeList);
		}

		public void TestServiceTypeOnDeliveryCodeList()
		{
			var container = Factory.New<JPAFRContainer>();
			AssertEquals(Factory.GetCachedValue<ServiceTypeOnDeliveryCodeList>(), container.Lookups.ServiceTypeOnDeliveryCodeList);
		}
	}
}
