using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypesOfContainer()
		{
			AssertNotNull(lookups.TypesOfContainer);
			Assert("TypesOfContainer should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRContainerTypes>(), lookups.TypesOfContainer));
		}

		public void TestContainerSizes()
		{
			AssertNotNull(lookups.ContainerSizes);
			Assert("ContainerSizes should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRContainerSizes>(), lookups.ContainerSizes));
		}

		public void TestCargoTypes()
		{
			AssertNotNull(lookups.CargoTypes);
			Assert("CargoTypes should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<CMRImportCargoTypes>(), lookups.CargoTypes));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			lookups = container.Lookups;
		}

		CusSCAContainerLookups lookups;

		#endregion
	}
}
