using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	public class AsycudaTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAET_ChargeType()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			asycudaTax.AET_ChargeType = "018";
			asycudaTax.Validation.ValidateAET_ChargeType();
			AssertHasMessageErrorContaining(asycudaTax.AET_ChargeTypeInfo, ListValidation.InvalidCodeMessageError);

			asycudaTax.AET_ChargeType = ChargeCodeList.Codes._008;
			asycudaTax.Validation.ValidateAET_ChargeType();
			AssertNoMessageErrors(asycudaTax.AET_ChargeTypeInfo);

			var asycudaTax2 = asycudaTax.Bill.AsycudaTaxes.AddNew();
			asycudaTax2.AET_ChargeType = ChargeCodeList.Codes._008;
			asycudaTax2.Validation.ValidateAET_ChargeType();
			AssertHasMessageErrorContaining(asycudaTax2.AET_ChargeTypeInfo, "The Charge Code cannot be repeated.");

			asycudaTax2.AET_ChargeType = ChargeCodeList.Codes._001;
			asycudaTax2.Validation.ValidateAET_ChargeType();
			AssertNoMessageErrors(asycudaTax2.AET_ChargeTypeInfo);
		}

		public void TestCheckAET_MethodOfPayment()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			asycudaTax.AET_MethodOfPayment = "ABC";
			asycudaTax.Validation.ValidateAET_MethodOfPayment();
			AssertHasMessageErrorContaining(asycudaTax.AET_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);

			asycudaTax.AET_MethodOfPayment = Core.Constants.PaymentType.Prepaid;
			asycudaTax.Validation.ValidateAET_MethodOfPayment();
			AssertNoMessageErrors(asycudaTax.AET_MethodOfPaymentInfo);
		}

		public void TestCheckAET_RX_NKCurrency()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			asycudaTax.AET_RX_NKCurrency = "ABC";
			asycudaTax.Validation.ValidateAET_RX_NKCurrency();
			AssertHasMessageErrorContaining(asycudaTax.AET_RX_NKCurrencyInfo, ListValidation.InvalidCodeMessageError);

			asycudaTax.AET_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			asycudaTax.Validation.ValidateAET_RX_NKCurrency();
			AssertNoMessageErrors(asycudaTax.AET_RX_NKCurrencyInfo);
		}

		public void TestCheckAET_MethodOfCalculation()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();
			asycudaTax.Validation.ValidateAET_MethodOfCalculation();
			AssertEquals("Should not call base.CheckAET_MethodOfCalculation", false, asycudaTax.AET_MethodOfCalculationInfo.HasNotifications());
		}

		AsycudaTax SetupTestEnvironmentForAsycudaTax()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.AsycudaTaxes.AddNew();
		}
	}
}
