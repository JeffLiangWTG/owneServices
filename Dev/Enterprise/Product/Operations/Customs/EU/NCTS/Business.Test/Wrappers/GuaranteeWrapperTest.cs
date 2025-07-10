using System;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class GuaranteeWrapperTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ControlResultWrapper(null));
		}

		public void TestGuaranteeType()
		{
			guarantee.PW_BondType = "BondType";
			AssertEquals("BondType", wrapper.GuaranteeType);
		}

		public void TestGuaranteeReferenceNumber()
		{
			guarantee.PW_BondNumber = "GuaranteeReferenceNumber";
			AssertEquals("GuaranteeReferenceNumber", wrapper.GuaranteeReferenceNumber);
		}

		public void TestOtherGuaranteeReference()
		{
			guarantee.PW_BondNumber2 = "pW_BondNumber2";
			AssertEquals("pW_BondNumber2", wrapper.OtherGuaranteeReference);
		}

		public void TestAccessCode()
		{
			guarantee.PW_Password = "1234";
			AssertEquals("1234", wrapper.AccessCode);
		}

		public void TestTaxAndDutyLiabiltyAmount()
		{
			guarantee.PW_BondAmount = 1.0m;
			AssertEquals(1.0m, wrapper.TaxAndDutyLiabiltyAmount);
		}

		public void TestNotValidForEC()
		{
			AssertEquals("0", wrapper.NotValidForEC);
		}

		public void TestNotValidForOtherContractingParties()
		{
			AssertEquals(1, wrapper.NotValidForOtherContractingParties.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			guarantee = Factory.New<NctsGuarantee>();
			wrapper = new GuaranteeWrapper(guarantee);
		}
		NctsGuarantee guarantee;
		GuaranteeWrapper wrapper;

		protected override GuaranteeWrapper GetProvider() => wrapper;
	}
}
