using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsGuaranteeValueSetStrategyTest : BusinessObjectValidationTestCase
	{
		public void TestPW_BondAmountReCalculatedAfterFieldChanges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
				var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				goodsItem.BY_ZZF_NKTaxType = ZString.Empty;
				goodsItem.BY_MonetaryValue = 1000m;
				CombineAssertions(() =>
				{
					AssertEquals("1", 250m, guarantee.PW_BondAmount);

					guarantee.PW_BondAmount = 0;
					guarantee.PW_BondType = "0";

					AssertEquals("2", 250m, guarantee.PW_BondAmount);

					guarantee.PW_BondAmount = 0;
					guarantee.PW_SuretyCode = "HAL";

					AssertEquals("3", 250m, guarantee.PW_BondAmount);
				});
			}
		}
	}
}
