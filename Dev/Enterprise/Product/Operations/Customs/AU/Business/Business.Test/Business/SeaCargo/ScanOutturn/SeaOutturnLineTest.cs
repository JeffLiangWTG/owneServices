using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaOutturnLine))]
	sealed class SeaOutturnLineTest : OutturnLineTest
	{
		protected override IScanHouseBillProvider GetNewHouseBill()
		{
			return Factory.New<CusSCAHouse>();
		}
	}
}
