using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PackingGroupLineNumberComparerTest : TestCaseWithFactory
	{
		public void TestComapre()
		{
			PackingGroup pack1 = Factory.New<PackingGroup>();
			pack1.CR_HouseContainerNumber = 1;

			PackingGroup pack2 = Factory.New<PackingGroup>();
			pack2.CR_HouseContainerNumber = 2;

			PackingGroupLineNumberComparer comparer = new PackingGroupLineNumberComparer();
			AssertEquals("Negative", true, comparer.Compare(pack1, pack2) < 0);

			pack2.CR_HouseContainerNumber = 1;
			AssertEquals("Zero", true, comparer.Compare(pack1, pack2) == 0);

			pack1.CR_HouseContainerNumber = 2;
			AssertEquals("Positive", true, comparer.Compare(pack1, pack2) > 0);
		}
	}
}
