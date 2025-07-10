using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotCollectionForHouseBillTest : SeaCargoTestCase
	{
		public void TestAddNewAddsToOceanBillPivots()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			var pivotCollection = new CusSCAPivotCollectionForHouseBill(houseBill);
			var pivot = pivotCollection.AddNew();
			AssertEquals("OceanBill.Pivots should contain the new pivot", true, oceanBill.Pivots.Contains(pivot));
		}

		public void TestAllowNew()
		{
			var houseBill = Factory.New<CusSCAHouse>();
			var pivots = (IBindingList)houseBill.Pivot;

			AssertEquals(false, houseBill.Pivot.ReadOnly);
			AssertEquals(true, pivots.AllowNew);

			houseBill.Pivot.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, houseBill.Pivot.ReadOnly);
			AssertEquals(false, pivots.AllowNew);

			houseBill.Pivot.SetReadOnlyIncludingChildren(false);
			AssertEquals(false, houseBill.Pivot.ReadOnly);
			AssertEquals(true, pivots.AllowNew);
		}

		public void TestHasAnElementSelfAssessed()
		{
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			AssertEquals("No Self-assessed packings", false, houseBill.Pivot.HasAnElementSelfAssessed);

			CusSCAPivot pivot = houseBill.Pivot.AddNew();
			AssertEquals("No Self-assessed packings", false, houseBill.Pivot.HasAnElementSelfAssessed);

			pivot.CV_IsSAC = true;
			AssertEquals("Self-assessed packings", true, houseBill.Pivot.HasAnElementSelfAssessed);
		}

		public void TestIndexer()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			CusSCAPivotCollectionForHouseBill pivotCollection = new CusSCAPivotCollectionForHouseBill(houseBill);
			BusinessObject pivotRecord = pivotCollection.AddNew();
			AssertEquals(pivotRecord, pivotCollection[0]);
		}

		public void TestTypedAddNew()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			CusSCAPivotCollectionForHouseBill oceanBillCollection = new CusSCAPivotCollectionForHouseBill(houseBill);
			BusinessObject pivot = oceanBillCollection.AddNew();
			AssertEquals(pivot.GetType(), typeof(CusSCAPivot));
		}

		public void TestFromContainer()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			CusSCAPivot pivot = houseBill.Pivot.AddNew();
			AssertNull("Pivot", houseBill.Pivot.FromContainer(container));
			pivot.CV_CN = container.PK;
			AssertNotNull("Pivot", houseBill.Pivot.FromContainer(container));
		}

		public void TestContainerPivotCollectionGetsNewPivots()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "111";
			AssertNotNull(container1.Pivots);// hit the pivot collection early
			CusSCAContainer container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "222";
			CusSCAPivot pivot1 = house.Pivot.AddNew();
			CusSCAPivot pivot2 = house.Pivot.AddNew();
			pivot1.CV_AssociatedContainer = "111";
			pivot2.CV_AssociatedContainer = "222";
			AssertEquals("Container1.Pivot.Count", 1, container1.Pivots.Count);
			AssertEquals("Container2.Pivot.Count", 1, container2.Pivots.Count);
		}

		public void TestContainerPivotCollectionRemovesPivotsWhenAssociatedContainerSet()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			CusSCAContainer container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "111";
			CusSCAPivot pivot1 = house.Pivot.AddNew();
			pivot1.CV_AssociatedContainer = "111";
			AssertEquals("Container1.Pivot.Count", 1, container1.Pivots.Count);
			pivot1.CV_AssociatedContainer = ZString.Empty;
			AssertEquals("Container1.Pivot.Count", 0, container1.Pivots.Count);
		}
	}
}
