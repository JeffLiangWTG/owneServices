using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CTOCusHAWBAndPartShipCollection))]
	sealed class CTOCusHAWBAndPartShipCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddingToChildrenBillSyncs()
		{
			AssertEquals("Collection.Count", 0, Collection.Count);
			CTOMAWB.ChildBills.AddNew();
			AssertEquals("Collection.Count", 1, Collection.Count);
		}

		public void TestRemovingFromChildrenBillSyncs()
		{
			CTOMAWB.ChildBills.AddNew();
			AssertEquals("Collection.Count", 1, Collection.Count);
			CTOMAWB.ChildBills.Remove(CTOMAWB.ChildBills[0]);
			AssertEquals("Collection.Count", 0, Collection.Count);
		}

		public void TestAddingToCollectionAddsToChildrenBills()
		{
			AssertEquals(0, CTOMAWB.ChildBills.Count);
			Collection.Add(Factory.New(typeof(CTOCusHAWB)));
			AssertEquals(1, CTOMAWB.ChildBills.Count);
		}

		public void TestAddingNewToCollectionAddsToChildrenBills()
		{
			AssertEquals(0, CTOMAWB.ChildBills.Count);
			Collection.AddNew();
			AssertEquals(1, CTOMAWB.ChildBills.Count);
		}

		public void TestRemovingFromCollectionRemovesFromChildrenBills()
		{
			Collection.AddNew();
			AssertEquals(1, CTOMAWB.ChildBills.Count);
			Collection.RemoveAndDeleteAll();
			AssertEquals(0, CTOMAWB.ChildBills.Count);
		}

		public void TestPartShipsAreAPartOfTheCollection()
		{
			CTOMAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 11);
			CTOMAWB.CM_FlightNo = "QF321";
			CTOMAWB.CM_RL_NKDischargePort = "AUSYD";

			var secondMAWB = Factory.New<CTOCusMAWB>();
			secondMAWB.CM_FlightNo = "QF123";
			secondMAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 11);
			CusPartShip partShip1 = secondMAWB.ChildBills.AddNew().PartShips.AddNew();
			CusPartShip partShip2 = secondMAWB.ChildBills.AddNew().PartShips.AddNew();
			CusPartShip partShip3 = secondMAWB.ChildBills.AddNew().PartShips.AddNew();
			CusPartShip partShip4 = secondMAWB.ChildBills.AddNew().PartShips.AddNew();
			partShip1.CG_FlightNo = "QF321";
			partShip1.CG_ArrivalDate = new ZDateTime(2005, 7, 11);
			partShip1.CG_RL_NKDischargePort = "AUSYD";
			partShip2.CG_FlightNo = "QF322";
			partShip2.CG_ArrivalDate = new ZDateTime(2005, 7, 11);
			partShip2.CG_RL_NKDischargePort = "AUSYD";
			partShip3.CG_FlightNo = "QF322";
			partShip3.CG_ArrivalDate = new ZDateTime(2005, 7, 12);
			partShip3.CG_RL_NKDischargePort = "AUSYD";
			partShip4.CG_FlightNo = "QF322";
			partShip4.CG_ArrivalDate = new ZDateTime(2005, 7, 12);
			partShip4.CG_RL_NKDischargePort = "AUMEL";

			Collection.Load();
			AssertEquals(1, Collection.Count);
			AssertEquals("Element[0]", partShip1, Collection[0]);

			CTOMAWB.CM_FlightNo = "QF322";
			AssertEquals(1, Collection.Count);
			AssertEquals("Element[0]", partShip2, Collection[0]);

			CTOMAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 12);
			AssertEquals(1, Collection.Count);
			AssertEquals("Element[0]", partShip3, Collection[0]);

			CTOMAWB.CM_RL_NKDischargePort = "AUMEL";
			AssertEquals(1, Collection.Count);
			AssertEquals("Element[0]", partShip4, Collection[0]);
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestWeCantDeleteCusPartsFromCollection()
		{
			CTOMAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 11);
			CTOMAWB.CM_FlightNo = "QF321";
			CTOMAWB.CM_RL_NKDischargePort = "AUSYD";

			var secondMAWB = Factory.New<CTOCusMAWB>();
			secondMAWB.CM_FlightNo = "QF123";
			secondMAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 11);
			CusPartShip partShip1 = secondMAWB.ChildBills.AddNew().PartShips.AddNew();
			partShip1.CG_FlightNo = "QF321";
			partShip1.CG_ArrivalDate = new ZDateTime(2005, 7, 11);
			partShip1.CG_RL_NKDischargePort = "AUSYD";

			Collection.Load();
			AssertEquals(1, Collection.Count);
			try
			{
				Collection.RemoveAndDelete((BusinessObject)Collection[0]);
			}
			catch (CannotDeleteException e)
			{
				AssertEquals(1, Collection.Count);
				AssertEquals("Exception Message", "You can't delete this line because it comes from a Part Shipment from another CTO form.  To remove the item remove it from the CTO form it was created on.", e.Message);
				throw;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CTOCusHAWB>();

		protected override BusinessObjectCollection GetCollectionToTest() => Collection;

		new CTOCusHAWBAndPartShipCollection Collection => CTOMAWB.AllChildBills;

		CTOCusMAWB ctoMAWB;
		CTOCusMAWB CTOMAWB => ctoMAWB ?? (ctoMAWB = Factory.New<CTOCusMAWB>());
	}
}
