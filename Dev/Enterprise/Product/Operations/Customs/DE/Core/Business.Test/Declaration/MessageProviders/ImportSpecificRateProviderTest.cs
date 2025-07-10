namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportSpecificRateProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportSpecificRateProvider>
	{
		public void TestType()
		{
			AssertEquals("C", GetProviderCore(ImportChargeCodeList.Codes.SRC, decimal.Zero).Type);
		}

		public void TestType_ChargeType_Empty()
		{
			AssertNull(GetProviderCore(string.Empty, decimal.Zero).Type);
		}

		public void TestType_ChargeType_Null()
		{
			AssertNull(GetProviderCore(null, decimal.Zero).Type);
		}

		public void TestValue()
		{
			AssertEquals(45.10m, GetProviderCore(string.Empty, 45.10m).Value);
		}

		protected override ImportSpecificRateProvider GetProvider() => GetProviderCore(string.Empty, decimal.Zero);

		ImportSpecificRateProvider GetProviderCore(string chargeType, decimal value) => new ImportSpecificRateProvider(chargeType, value);
	}
}
