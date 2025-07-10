using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManOBLDetailLookupsTest : Customs.Business.Testing.CusSeaManOBLDetailLookupsTest
	{
		public override void TestPackageTypes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Expected overridden type for List. Check to see that we're getting the right class from Detail.Lookups.", typeof(CMRPackageTypes), detail.Lookups.PackageTypes.GetType());
		}

		public override void TestGrossWeightCodes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Expected overridden type for List. Check to see that we're getting the right class from Detail.Lookups.", typeof(CMRGrossWeightCodes), detail.Lookups.GrossWeightCodes.GetType());
		}

		public override void TestQuantityUnits()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Expected overridden type for List. Check to see that we're getting the right class from Detail.Lookups.", typeof(CMRQuantityUnits), detail.Lookups.QuantityUnits.GetType());
		}

		public override void TestCargoTypes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Expected overridden type for List. Check to see that we're getting the right class from Detail.Lookups.", typeof(CMRImportCargoTypes), detail.Lookups.CargoTypes.GetType());
		}

		public override void TestContainerSizes()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Expected overridden type for List. Check to see that we're getting the right class from Detail.Lookups.", typeof(CMRContainerSizes), detail.Lookups.ContainerSizes.GetType());
		}

		public override void TestTypesOfContainers()
		{
			CusSeaManOBLDetail detail = Factory.New<CusSeaManOBLDetail>();
			AssertEquals("Expected overridden type for List. Check to see that we're getting the right class from Detail.Lookups.", typeof(CMRContainerTypes), detail.Lookups.TypesOfContainers.GetType());
		}
	}
}
