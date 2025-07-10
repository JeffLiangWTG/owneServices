using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAPivotCollection))]
	sealed class CusSCAPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<CusSCAPivotCollection>
	{
		public void TestByHouseBill()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill1 = oceanBill.HouseBills.AddNew();
			var houseBill2 = oceanBill.HouseBills.AddNew();
			var pivot1 = houseBill1.Pivot.AddNew();
			var pivot2 = houseBill2.Pivot.AddNew();

			AssertEquals(pivot1, oceanBill.Pivots.ByHouseBill[houseBill1.PK].First());
			AssertEquals(pivot2, oceanBill.Pivots.ByHouseBill[houseBill2.PK].First());

			var pivot3 = houseBill2.Pivot.AddNew();
			AssertEquals(2, oceanBill.Pivots.ByHouseBill[houseBill2.PK].Count());

			pivot3.CV_CA = houseBill1.PK;
			AssertEquals(2, oceanBill.Pivots.ByHouseBill[houseBill1.PK].Count());
			AssertEquals(1, oceanBill.Pivots.ByHouseBill[houseBill2.PK].Count());
		}

		public void TestByContainer()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container1 = oceanBill.Containers.AddNew();
			var container2 = oceanBill.Containers.AddNew();
			var pivot1 = container1.Pivots.AddNew();
			var pivot2 = container2.Pivots.AddNew();

			AssertEquals(pivot1, oceanBill.Pivots.ByContainer[container1.PK].First());
			AssertEquals(pivot2, oceanBill.Pivots.ByContainer[container2.PK].First());

			var pivot3 = container2.Pivots.AddNew();
			AssertEquals(2, oceanBill.Pivots.ByContainer[container2.PK].Count());

			pivot3.CV_CN = container1.PK;
			AssertEquals(2, oceanBill.Pivots.ByContainer[container1.PK].Count());
			AssertEquals(1, oceanBill.Pivots.ByContainer[container2.PK].Count());
		}

		public void TestSavingAnEmptyPivot()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var emptyPivot = oceanBill.Pivots.AddNew();
			var container1 = oceanBill.Containers.AddNew();
			var containerPivot = container1.Pivots.AddNew();

			AssertEquals(2, oceanBill.Pivots.Count);
			Factory.Save();

			AssertEquals("Pivot will self-delete on Save unless it has a container attached.", 1, oceanBill.Pivots.Count);
			AssertEquals("ByContainer method functions correctly", 1, oceanBill.Pivots.ByContainer[container1.PK].Count());
		}

		public void TestLoad()
		{
			ZGuid oceanBillPK = CreateOceanBillInDataBaseWith3Pivots();
			var oceanBill = Factory.Load<CusSCAOceanBill>(oceanBillPK);
			AssertEquals("Pivot Collection Count", 3, oceanBill.Pivots.Count);
		}

		[ExpectNoExceptions]
		public void TestLoadWhenHavingManyContainersAndHouseBills()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusSCAOceanBill oceanBill = factory.New<CusSCAOceanBill>();
			for (short i = 1; i < 325; i++)
			{
				CusSCAHouse housebill = oceanBill.HouseBills.AddNew();
				for (short j = 1; j < 2; j++)
				{
					CusSCAContainer container = oceanBill.Containers.AddNew();

					CusSCAPivot pivot = container.Pivots.AddNew();
					housebill.Pivot.Add(pivot);
				}
			}

			factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusSCAOceanBill sameOceanBill = newFactory.Load<CusSCAOceanBill>(oceanBill.PK);
			object pivots = sameOceanBill.Pivots;
		}

		ZGuid CreateOceanBillInDataBaseWith3Pivots()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusSCAOceanBill result = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container1 = result.Containers.AddNew();
			CusSCAContainer container2 = result.Containers.AddNew();

			CusSCAHouse house1 = result.HouseBills.AddNew();
			CusSCAHouse house2 = result.HouseBills.AddNew();

			CusSCAPivot pivot1 = container1.Pivots.AddNew();
			house1.Pivot.Add(pivot1);
			CusSCAPivot pivot2 = container1.Pivots.AddNew();
			house2.Pivot.Add(pivot2);
			CusSCAPivot pivot3 = container2.Pivots.AddNew();
			house1.Pivot.Add(pivot3);
			factory.Save();
			return result.PK;
		}

		#region RemoveAndDelete

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsTrue()
		{
			var pivot = Pivots.AddNew();
			CusUnderbond underbond = pivot.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;

			AssertEquals("Pivot should be deletable", true, pivot.CanDelete);
			AssertEquals("One item must exist", 1, Pivots.Count);

			Pivots.Delete(pivot);
			AssertEquals("No item must remain", 0, Pivots.Count);
		}

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsFalse()
		{
			var pivot = Pivots.AddNew();
			CusUnderbond underbond = pivot.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;

			AssertEquals("Pivot should not be deletable", false, pivot.CanDelete);
			AssertEquals("One item must exist", 1, Pivots.Count);

			Pivots.Delete(pivot);
			AssertEquals("No item must remain", 1, Pivots.Count);
		}

		#endregion

		#region Implementation

		CusSCAPivotCollection Pivots
		{
			get
			{
				if (fPivots == null)
				{
					fPivots = GetCollectionToTest();
				}
				return fPivots;
			}
		}
		CusSCAPivotCollection fPivots;

		protected override CusSCAPivotCollection GetCollectionToTest()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			return new CusSCAPivotCollection(oceanBill);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.OceanBill.HouseBills.AddNew().Pivot.AddNew();
		}

		#endregion
	}
}
