using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013AndIE015GuaranteeReferenceTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015GuaranteeReference>
	{
		public void TestGrn()
		{
			AssertEquals("GRN", "ABC123", Provider.Grn);
		}

		public void TestAccessCode()
		{
			AssertEquals("Access Code", "pass", Provider.AccessCode);
		}

		public void TestAmountToBeCovered()
		{
			AssertEquals("Amount to be covered", 555.23M, Provider.AmountToBeCovered);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency (should be hardceded to EUR", "EUR", Provider.Currency);
		}

		protected override IE013AndIE015GuaranteeReference GetProvider() => new IE013AndIE015GuaranteeReference(guarantee);
		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			guarantee = Factory.New<NctsGuarantee>();
			guarantee.PW_BondNumber = "ABC123";
			guarantee.PW_Password = "pass";
			guarantee.PW_BondAmount = 555.23M;
			guarantee.PW_RX_NKCurrency = "GBP";
			nctsHeader.MovementHeader.Guarantees.Add(guarantee);
		}

		NctsHeader nctsHeader;
		NctsGuarantee guarantee;
	}
}
