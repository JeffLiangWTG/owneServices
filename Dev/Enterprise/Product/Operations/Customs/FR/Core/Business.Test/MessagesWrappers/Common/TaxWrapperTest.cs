using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class TaxWrapperTest : TestCaseWithFactory
	{
		public void TestTaxCodeIsNationalCode()
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.NationalFeeTypeCode = "A445";
			fee.CF_ChargeType = "B00";
			var taxWrapper = new TaxWrapper(fee);
			AssertEquals("TaxCode should proxy the NationalFeeTypeCode property", "A445", taxWrapper.TaxCode);
		}

		public void TestTaxTypeIsInferredFromNationalCode()
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_MethodOfCalculation = ZString.Empty;
			fee.NationalFeeTypeCode = "A445";
			var taxWrapper = new TaxWrapper(fee);
			AssertEquals("TaxType should be inferred from the NationalFeeTypeCode property", "0", taxWrapper.TaxType);

			fee.NationalFeeTypeCode = "A325";
			AssertEquals("TaxType should be inferred from the NationalFeeTypeCode property", "1", taxWrapper.TaxType);

			fee.NationalFeeTypeCode = "K835";
			AssertEquals("TaxType should be inferred from the NationalFeeTypeCode property", "2", taxWrapper.TaxType);
		}
	}
}
