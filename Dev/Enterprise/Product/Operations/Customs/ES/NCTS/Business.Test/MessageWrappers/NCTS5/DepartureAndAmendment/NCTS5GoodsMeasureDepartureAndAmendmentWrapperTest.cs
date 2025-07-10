using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5GoodsMeasureDepartureAndAmendmentWrapperTest : WrapperHelperTest<NCTS5GoodsMeasureDepartureAndAmendmentWrapper>
	{
		public void TestSupplementaryUnits()
		{
			goodsItem.BY_CustomsSecondUnitQty = Core.Constants.Weight.Kilograms;
			goodsItem.BY_CustomsSecondQuantity = 1.1235678m;
			AssertEquals("Expected filled SupplementaryUnits", 1.123568m, wrapper.SupplementaryUnits);
		}

		public void TestSupplementaryUnitsSpecified()
		{
			goodsItem.BY_CustomsSecondUnitQty = Core.Constants.Weight.Kilograms;
			goodsItem.BY_CustomsSecondQuantity = 1.1235m;
			AssertEquals("Expected true SupplementaryUnitsSpecified when SupplementaryUnits is not 0", true, wrapper.SupplementaryUnitsSpecified);

			goodsItem.BY_CustomsSecondQuantity = 0m;
			AssertEquals("Expected false SupplementaryUnitsSpecified when SupplementaryUnits is 0", false, wrapper.SupplementaryUnitsSpecified);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			goodsItem = nctsBill.GoodsItems.AddNew();
			wrapper = new NCTS5GoodsMeasureDepartureAndAmendmentWrapper(goodsItem);
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NCTS5GoodsMeasureDepartureAndAmendmentWrapper wrapper;

		protected override NCTS5GoodsMeasureDepartureAndAmendmentWrapper GetProvider() => wrapper;
	}
}
