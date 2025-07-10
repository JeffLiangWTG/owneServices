namespace Enterprise.Customs.DE.Business.Testing
{
	class ReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<ReferenceProvider>
	{
		public void TestFullType()
		{
			AssertNullOrEmpty(Provider.FullType);
		}

		public void TestType()
		{
			alternativeEvidence.DocType = "A123A2S";
			AssertEquals("A123", Provider.Type);
		}

		public void TestQualifier_2Chars()
		{
			alternativeEvidence.DocType = "A123A2";
			AssertEquals("A2", Provider.Qualifier);
		}

		public void TestQualifier_3Chars()
		{
			alternativeEvidence.DocType = "A123A2S";
			AssertEquals("Qualifier is capped at 3 chars", "A2S", Provider.Qualifier);
		}

		public void TestReferenceNumber()
		{
			alternativeEvidence.Reference = "A123B";
			AssertEquals("A123B", Provider.ReferenceNumber);
		}

		public void TestDetail()
		{
			AssertNullOrEmpty(Provider.Detail);
		}

		public void TestComplement()
		{
			AssertNullOrEmpty(Provider.Complement);
		}

		public void TestCurrency()
		{
			AssertNullOrEmpty(Provider.Currency);
		}

		public void TestAmount()
		{
			AssertEquals(0.0m, Provider.Amount);
		}

		protected override ReferenceProvider GetProvider() => new ReferenceProvider(alternativeEvidence);

		protected override void SetUp()
		{
			base.SetUp();
			alternativeEvidence = new AlternativeEvidence(Factory);
		}
		AlternativeEvidence alternativeEvidence;
	}
}
