namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class AdditionalFiscalReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalFiscalReferenceProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Sequence", "1", Provider.SequenceNumber);
		}

		public void TestRole()
		{
			AssertEquals("Role", "R1", Provider.Role);
		}

		public void TestVatIdentificationNumber()
		{
			AssertEquals("VatIdentificationNumber", "VIN", Provider.VatIdentificationNumber);
		}

		protected override AdditionalFiscalReferenceProvider GetProvider() => new AdditionalFiscalReferenceProvider(1, "R1", "VIN");
	}
}
