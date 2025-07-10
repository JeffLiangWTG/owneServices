using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondPerson))]
	class CusInBondPersonBaseOnlyTest : CusInBondPersonAbstractTest<NctsHeader>
	{
		public void TestNctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			var cusInBondPerson = Customs.Business.CusInBondPerson.LoadOrCreate<CusInBondPerson>(header, CusInBondPerson.LocationContactType);
			AssertSame(header, cusInBondPerson.NctsHeader);
		}
	}
}
