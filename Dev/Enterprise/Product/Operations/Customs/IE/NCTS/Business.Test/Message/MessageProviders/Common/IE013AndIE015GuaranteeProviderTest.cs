using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013AndIE015GuaranteeProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015GuaranteeProvider>
	{
		public void TestGuaranteeType()
		{
			AssertEquals("Guarantee Type", "3", Provider.GuaranteeType);
		}

		public void TestOtherGuaranteeReference()
		{
			AssertEquals("Other Reference", "TEST", Provider.OtherGuaranteeReference);
			guarantee.PW_BondType = "1";
			var provider = GetProvider();
			AssertEquals("Other Reference should be null when Guarantee Type != 3 or 8", null, provider.OtherGuaranteeReference);
		}

		public void TestGuaranteeReferences()
		{
			var provider = GetProvider();
			AssertEquals("References", 1, provider.GuaranteeReferences.Count);
			guarantee.PW_BondType = "B";
			provider = GetProvider();
			AssertNull("References should be null when Guarantee Type not in (0,1,2,3,4,5,9)", provider.GuaranteeReferences);
		}

		protected override IE013AndIE015GuaranteeProvider GetProvider() => new IE013AndIE015GuaranteeProvider(guarantee);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			guarantee = Factory.New<NctsGuarantee>();
			guarantee.PW_BondType = "3";
			guarantee.PW_BondNumber2 = "TEST";
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
