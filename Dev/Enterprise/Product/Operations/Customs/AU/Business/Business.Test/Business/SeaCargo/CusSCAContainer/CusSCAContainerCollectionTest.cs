using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAContainerCollectionTest : TestCaseWithFactory
	{
		public void TestIndexer()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var containerCollection = new CusSCAContainerCollection(oceanBill, Factory);
			var container = Factory.New<CusSCAContainer>();
			containerCollection.Add(container);
			AssertEquals(container, containerCollection[0]);
		}

		public void TestTypedAddNew()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var containerCollection = new CusSCAContainerCollection(oceanBill, Factory);
			BusinessObject container = containerCollection.AddNew();
			AssertEquals(typeof(CusSCAContainer), container.GetType());
		}

		public void TestOnlyContainersOnConsolAreFound()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var containerNotOnOceanBill = Factory.New<CusSCAContainer>();
			containerNotOnOceanBill.CN_ContainerNumber = TestContainerNumber;
			AssertNull("Container Not on Consol and should not be found", oceanBill.Containers.Find(TestContainerNumber));

			var containerOnOceanBill = Factory.New<CusSCAContainer>();
			containerOnOceanBill.CN_ContainerNumber = TestContainerNumber;
			oceanBill.Containers.Add(containerOnOceanBill);
			var foundContainer = oceanBill.Containers.Find(TestContainerNumber);
			AssertEquals("Container on OceanBill Should be one added to OceanBill", containerOnOceanBill, foundContainer);
		}

		#region Implementation

		const string TestContainerNumber = "GFYU9829302";

		#endregion
	}
}
