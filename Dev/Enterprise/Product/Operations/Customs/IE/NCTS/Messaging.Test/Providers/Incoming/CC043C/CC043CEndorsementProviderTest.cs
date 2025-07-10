using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CEndorsementProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("EndorsementType missing", () => new CC043CEndorsementProvider(null));
			});
		}

		public void TestAuthority()
		{
			AssertEquals("Test Auth", provider.Authority);
		}

		public void TestDate()
		{
			AssertEquals(new ZDate(2023, 08, 22), provider.Date);
		}

		public void TestCountry()
		{
			AssertEquals("XI", provider.Country);
		}

		public void TestPlace()
		{
			AssertEquals("XIBEL", provider.Place);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CEndorsementProvider(new EndorsementType03
			{
				Authority = "Test Auth",
				Country = "XI",
				Place = "XIBEL",
				Date = new DateTime(2023, 08, 22)
			});
		}
		CC043CEndorsementProvider provider;
	}
}
