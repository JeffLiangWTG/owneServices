using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBFilteredCollection))]
	sealed class CusHAWBFilteredCollectionTest : Customs.Business.Testing.CusHAWBFilteredCollectionTest<CusHAWBFilteredCollection>
	{
		protected override Customs.Business.CusMAWB GetNewMAWB() => Factory.New<CusMAWB>();

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusHAWB>();
	}
}
