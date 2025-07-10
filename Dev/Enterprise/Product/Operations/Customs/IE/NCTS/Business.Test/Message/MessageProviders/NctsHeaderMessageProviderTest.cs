using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class NctsHeaderMessageProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new NctsHeaderMessageProviderForTest(null));
			});
		}

		class NctsHeaderMessageProviderForTest : NctsDepartureHeaderMessageProvider
		{
			public NctsHeaderMessageProviderForTest(NctsHeader nctsHeader)
				: base(nctsHeader)
			{ }
		}
	}
}
