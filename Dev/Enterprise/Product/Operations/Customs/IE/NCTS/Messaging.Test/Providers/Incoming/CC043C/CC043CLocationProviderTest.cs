using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CLocationProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("LocationType missing", () => new CC043CLocationProvider(null));
			});
		}

		public void TestQualifierOfIdentification()
		{
			AssertEquals("O", provider.QualifierOfIdentification);
		}

		public void TestCountry()
		{
			AssertEquals("XI", provider.Country);
		}

		public void TestUNLocode()
		{
			AssertEquals("XIBEL", provider.UNLocode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CLocationProvider(new LocationType02
			{
				QualifierOfIdentification = "O",
				Country = "XI",
				UnLocode = "XIBEL",
			});
		}
		CC043CLocationProvider provider;
	}
}
