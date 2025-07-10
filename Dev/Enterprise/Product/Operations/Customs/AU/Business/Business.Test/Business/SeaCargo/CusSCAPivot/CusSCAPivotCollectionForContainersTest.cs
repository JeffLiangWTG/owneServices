using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAPivotCollectionForContainers))]
	sealed class CusSCAPivotCollectionForContainersTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewAddsToOceanBillPivots()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var pivotCollection = new CusSCAPivotCollectionForContainers(container);
			var pivot = pivotCollection.AddNew();
			AssertEquals("OceanBill.Pivots should contain the new pivot", true, oceanBill.Pivots.Contains(pivot));
		}

		public void TestHousePivotCollectionGetsNewPivots()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			CusSCAHouse house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "111";
			AssertNotNull(house1.Pivot);// hit the pivot collection early
			CusSCAHouse house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "222";
			CusSCAPivot pivot1 = container.Pivots.AddNew();
			CusSCAPivot pivot2 = container.Pivots.AddNew();
			pivot1.CV_AssociatedHouse = "111";
			pivot2.CV_AssociatedHouse = "222";
			AssertEquals("House1.Pivot.Count", 1, house1.Pivot.Count);
			AssertEquals("House2.Pivot.Count", 1, house2.Pivot.Count);
		}

		public void TestHousePivotCollectionRemovesPivotsWhenAssociatedHouseSet()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			CusSCAHouse house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "111";
			CusSCAPivot pivot1 = container.Pivots.AddNew();
			pivot1.CV_AssociatedHouse = "111";
			AssertEquals("House1.Pivot.Count", 1, house1.Pivot.Count);
			pivot1.CV_AssociatedHouse = ZString.Empty;
			AssertEquals("House1.Pivot.Count", 0, house1.Pivot.Count);
		}

		public override void TestDelete()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			CusSCAHouse house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "111";
			CusSCAPivot pivot1 = container.Pivots.AddNew();
			pivot1.CV_AssociatedHouse = "111";
			AssertEquals("House1.Pivot.Count", 1, house1.Pivot.Count);
			house1.Pivot.RemoveAndDelete(pivot1);
			AssertEquals("Container.Pivots.Count", 0, container.Pivots.Count);
			AssertEquals("House1.Pivot.Count", 0, house1.Pivot.Count);
		}

		#region Implementation

		CusSCAOceanBill OceanBill
		{
			get
			{
				if (fOceanBill == null)
				{
					fOceanBill = Factory.New<CusSCAOceanBill>();
				}
				return fOceanBill;
			}
		}
		CusSCAOceanBill fOceanBill;

		CusSCAContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = OceanBill.Containers.AddNew();
				}
				return fContainer;
			}
		}
		CusSCAContainer fContainer;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSCAPivotCollectionForContainers(Container);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusSCAHouse house = OceanBill.HouseBills.AddNew();
			return house.Pivot.AddNew();
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		#endregion
	}
}
