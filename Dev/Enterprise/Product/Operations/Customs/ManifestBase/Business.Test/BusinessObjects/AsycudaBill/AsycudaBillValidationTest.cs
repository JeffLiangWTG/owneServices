using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase.Testing
{
	internal class AsycudaBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckContainerPK()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pivot = Factory.New<AsycudaContainerBillOrPackageLink>();
			pivot.APC_ClusterKey = bill.ABL_ClusterKey;
			pivot.APC_ABL_Bill = bill.PK;
			bill.Validation.ValidateContainerPK();
			AssertHasNotifications("Container PK is null error", bill.ContainerPKInfo);
		}

		public void TestCheckABL_NetWeightUQ()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_NetWeight = 10m;
			bill.ABL_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoErrors(bill.ABL_NetWeightUQInfo);

			bill.ABL_NetWeightUQ = ZString.Empty;
			AssertHasErrorContaining(bill.ABL_NetWeightUQInfo, MandatoryValidation.MustBeEntered);

			bill.ABL_NetWeight = 0m;
			bill.Validation.ValidateABL_NetWeightUQ();
			AssertNoErrors(bill.ABL_NetWeightUQInfo);
		}

		public void TestCheckABL_RX_NKGoodsValueCurrency()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertNoErrorContaining("Currency has no not entered error", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);

			bill.ABL_GoodsValue = 100m;
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			AssertNoErrorContaining("Valid Currency no error", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining("Valid Currency no error", bill.ABL_RX_NKGoodsValueCurrencyInfo, ListValidation.InvalidCodeError);

			bill.ABL_RX_NKGoodsValueCurrency = "XXX";
			AssertNoErrorContaining("Invalid Currency no error", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(bill.ABL_RX_NKGoodsValueCurrencyInfo, ListValidation.InvalidCodeError);

			bill.ABL_RX_NKGoodsValueCurrency = "";
			AssertHasErrorContaining("Currency is required as value is entered", bill.ABL_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
