using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAET_BaseValue()
		{
			var asycudaTax = CreateData();
			asycudaTax.AET_BaseValue = -1000;
			AssertHasErrorContaining(asycudaTax.AET_BaseValueInfo, asycudaTax.Validation.NegativeAmountNotAllowed);

			asycudaTax.AET_BaseValue = 1000;
			AssertNoErrorContaining(asycudaTax.AET_BaseValueInfo, asycudaTax.Validation.NegativeAmountNotAllowed);
		}

		public void TestCheckAET_ChargeAmount()
		{
			var asycudaTax = CreateData();
			asycudaTax.AET_ChargeAmount = -500;
			AssertHasErrorContaining(asycudaTax.AET_ChargeAmountInfo, asycudaTax.Validation.NegativeAmountNotAllowed);

			asycudaTax.AET_ChargeAmount = 500;
			AssertNoErrorContaining(asycudaTax.AET_ChargeAmountInfo, asycudaTax.Validation.NegativeAmountNotAllowed);
		}

		public void TestCheckAET_Rate()
		{
			var asycudaTax = CreateData();
			asycudaTax.AET_Rate = -250;
			AssertHasErrorContaining(asycudaTax.AET_RateInfo, asycudaTax.Validation.NegativeAmountNotAllowed);

			asycudaTax.AET_Rate = 250;
			AssertNoErrorContaining(asycudaTax.AET_RateInfo, asycudaTax.Validation.NegativeAmountNotAllowed);
		}

		public void TestCheckAET_RateOverrideReasonCode()
		{
			var asycudaTax = CreateData();
			asycudaTax.AET_RateOverrideReasonCode = "ABC";
			AssertHasErrorContaining(asycudaTax.AET_RateOverrideReasonCodeInfo, asycudaTax.Validation.CodeNotInList);

			asycudaTax.AET_RateOverrideReasonCode = "ADD";
			AssertNoErrorContaining(asycudaTax.AET_RateOverrideReasonCodeInfo, asycudaTax.Validation.CodeNotInList);

			asycudaTax.AET_RateOverrideReasonCode = "";
			AssertNoErrorContaining(asycudaTax.AET_RateOverrideReasonCodeInfo, asycudaTax.Validation.CodeNotInList);
		}

		public void TestCheckAET_MethodOfCalculation()
		{
			var asycudaTax = CreateData();
			asycudaTax.AET_MethodOfCalculation = "";
			AssertHasErrors(asycudaTax.AET_MethodOfCalculationInfo);

			asycudaTax.AET_MethodOfCalculation = "TEST";
			AssertNoErrors(asycudaTax.AET_MethodOfCalculationInfo);
		}

		AsycudaTax CreateData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBillWithIAsycudaTaxTypeSupporter>();
			header.Bills.Add(bill);
			var tax = Factory.New<AsycudaTax>();
			tax.AET_ABL = bill.PK;
			return tax;
		}
	}
}
