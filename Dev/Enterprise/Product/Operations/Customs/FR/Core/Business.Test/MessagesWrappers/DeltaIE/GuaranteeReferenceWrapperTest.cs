using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GuaranteeReferenceWrapperTest : DataProviderTestCase<GuaranteeReferenceWrapper>
	{
		public void TestAccessCode()
		{
			AssertEquals("AccessCode should equal guarantee PW_Password.", "1234", Provider.AccessCode);
		}

		public void TestAmountToBeCovered()
		{
			AssertEquals("AmountToBeCovered should equal guarantee PW_BondAmount.", 123.456d, Provider.AmountToBeCovered);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier should never be written for France.", Provider.CcQualifier);
		}

		public void TestCurrencyCode()
		{
			AssertEquals("CurrencyCode should equal guarantee PW_RX_NKCurrency.", Core.Constants.CurrencyCodes.France, Provider.CurrencyCode);
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			AssertEquals("CustomsOfficeOfGuarantee should use CustomsOfficeOfGuaranteeWrapper.", "FR000001", Provider.CustomsOfficeOfGuarantee.ReferenceNumber);
		}

		public void TestGrn()
		{
			AssertEquals("Grn should equal guarantee PW_BondNumber.", "GRN0001", Provider.Grn);
		}

		public void TestOtherGuaranteeReference()
		{
			AssertEquals("OtherGuaranteeReference should equal guarantee PW_BondNumber2.", "OTH0001", Provider.OtherGuaranteeReference);
		}

		protected override GuaranteeReferenceWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = entryInstruction.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GRN0001";
			guarantee.PW_Password = "1234";
			guarantee.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.France;
			guarantee.PW_BondNumber2 = "OTH0001";
			guarantee.PW_BondFiledPort = "FR000001";
			guarantee.PW_BondAmount = 123.456m;
			return GuaranteeReferenceWrapper.New(guarantee);
		}
	}
}
