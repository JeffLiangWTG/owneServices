using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSHeaderProviderForTest))]
	sealed class NCTSHeaderProviderBaseOnlyTest : NCTSHeaderProviderAbstractTest<NCTSHeaderProviderForTest>
	{
		public void TestConstructor() => AssertExceptionThrown<ArgumentException>(() => new NCTSHeaderProviderForTest(null));

		protected override NCTSHeaderProviderForTest GetHeaderProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			return new NCTSHeaderProviderForTest(nctsHeader);
		}
	}

	class NCTSHeaderProviderForTest : NCTSHeaderProvider
	{
		public NCTSHeaderProviderForTest(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}
	}
}
