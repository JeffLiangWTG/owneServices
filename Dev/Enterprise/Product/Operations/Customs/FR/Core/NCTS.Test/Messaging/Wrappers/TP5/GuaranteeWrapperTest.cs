namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class GuaranteeWrapperTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeWrapper>
	{
		public void TestGuaranteeType()
		{
			AssertEquals("GuaranteeType should be mapped to PW_BondType.", "ABC", Provider.GuaranteeType);
		}

		public void TestOtherGuaranteeReference()
		{
			AssertEquals("GuaranteeReference should be mapped to PW_BondNumber2.", "REF1", Provider.OtherGuaranteeReference);
		}

		public void TestGuaranteeReference()
		{
			AssertEquals("ContactPerson should be mapped to Provider Guarantee.", "GRN1, 1234, 999.99, USD", $"{Provider.GuaranteeReference.Grn}, {Provider.GuaranteeReference.AccessCode}, {Provider.GuaranteeReference.AmountToBeCovered}, {Provider.GuaranteeReference.Currency}");
		}

		protected override GuaranteeWrapper GetProvider()
		{
			var guarantee = Factory.New<EU.NCTS.Business.NctsGuarantee>();
			guarantee.PW_BondType = "ABC";
			guarantee.PW_BondNumber2 = "REF1";
			guarantee.PW_BondNumber = "GRN1";
			guarantee.PW_Password = "1234";
			guarantee.PW_BondAmount = 999.99m;
			guarantee.PW_RX_NKCurrency = "USD";
			return GuaranteeWrapper.New(guarantee);
		}
	}
}
