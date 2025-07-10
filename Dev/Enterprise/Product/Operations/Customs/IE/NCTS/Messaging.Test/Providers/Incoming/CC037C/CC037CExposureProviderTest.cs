using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037CExposureProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ExposureType missing", () => new CC037CExposureProvider(null));
			});
		}

		public void TestExposure()
		{
			AssertEquals("Exposure", 7M, provider.Exposure);
		}

		public void TestExposureCounter()
		{
			AssertEquals("ExposureCounter", "1", provider.ExposureCounter);
		}

		public void TestBalance()
		{
			AssertEquals("ExposureBalance", 4M, provider.Balance);
		}

		public void TestCurrency()
		{
			AssertEquals("ExposureCurrency", "YGV", provider.Currency);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CExposureProvider(new ExposureType
			{
				Exposure = 7,
				ExposureCounter = "1",
				Balance = 4,
				Currency = "YGV"
			});
		}
		CC037CExposureProvider provider;
	}
}
