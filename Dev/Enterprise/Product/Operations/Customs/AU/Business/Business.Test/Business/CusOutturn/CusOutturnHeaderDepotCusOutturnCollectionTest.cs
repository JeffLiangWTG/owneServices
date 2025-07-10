using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusOutturnHeaderDepotCusOutturnCollection))]
	sealed class CusOutturnHeaderDepotCusOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypeOfElements()
		{
			AssertEquals(typeof(DepotCusOutturn), GetCollectionToTest().TypeOfElements);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusOutturnHeaderDepotCusOutturnCollection(CusOutturnHeader.New(Factory));
	}
}
