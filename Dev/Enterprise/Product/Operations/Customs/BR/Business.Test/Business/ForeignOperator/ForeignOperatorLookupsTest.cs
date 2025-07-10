using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ForeignOperatorLookups))]
	public class ForeignOperatorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			var goodsCatalogProduction = Factory.New<ForeignOperator>();
			var lookups = goodsCatalogProduction.Lookups;
			AssertType<RefCountryCollection>(lookups.Countries);
		}

		public void TestCustomsStatusList()
		{
			var goodsCatalogProduction = Factory.New<ForeignOperator>();
			var lookups = goodsCatalogProduction.Lookups;
			AssertSame(Factory.GetCachedValue<CustomsPostedStatusList>(), lookups.CustomsStatusList);
		}

		public void TestManufacturers()
		{
			var owner1 = Factory.New<OrgHeader>();
			var foreignOperator = Factory.New<OrgHeader>();

			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			cusGoodsCatalog.CGC_OH_Owner = owner1.PK;
			var cusBRForeignOperator = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_OH_Owner = owner1.PK;
			cusBRForeignOperator.BFR_OH_ForeignOperator = foreignOperator.PK;
			var cusBRForeignOperator2 = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_OH_Owner = owner1.PK;

			var productionInfo1 = cusGoodsCatalog.ForeignOperators.AddNew();

			AssertEquals(1, productionInfo1.Lookups.Manufacturers.Count);

			var filterOwner = productionInfo1.Lookups.Manufacturers.FilterBusinessObjectDefaults["Owner" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertNotNull("filter: Owner added", filterOwner);
			Assert("filter: readonly", !filterOwner.IsRemovable);
		}
	}
}
