using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDisplayPropertiesForGrid()
		{
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
			var location = Factory.NewWithValidTestData<CusGoodsLocation>();
			location.CGL_Type = "1";
			location.CGL_AdditionalIdentifier = "L0029A";
			location.CGL_LocationUse = CusGoodsLocationUseList.Codes.INS;

			AssertEquals("1 - CUSTOMS", location.InspectionType);
			AssertEquals("L0029A - Sevington", location.InspectionLocation);
		}

		public void TestLookups()
		{
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
			var location = Factory.NewWithValidTestData<CusGoodsLocation>();
			AssertGreaterThan(location.Lookups.TypeList.Count, 0);
			AssertGreaterThan(location.Lookups.InspectionLocationList.Count, 0);
		}

		public void TestValidationType()
		{
			var location = Factory.NewWithValidTestData<CusGoodsLocation>();
			AssertType<EU.Business.CusGoodsLocationValidation>(location.Validation);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return manifestHeader.InspectionLocations.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(null);
	}
}
