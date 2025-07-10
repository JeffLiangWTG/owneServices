using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(ReadOnlyCusMAWBCollection))]
	public class ReadOnlyCusMAWBCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAddNew()
		{
			Assert(true);
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ReadOnlyCusMAWBCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(CusMAWB));
		}
	}
}
