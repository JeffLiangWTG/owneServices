using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk.Testing
{
	class CcsukWrapper_TestsForRraAndTfm : TestCaseWithFactory
	{
		public void TestAllProperties()
		{
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			mawb.CargoTerminalOperator = "CAX";
			mawb.CargoTerminalOperatorAirport = "LHR";
			var line1 = mawb.OutTurns.AddNew();
			var line2 = mawb.OutTurns.AddNew();
			var line3 = mawb.OutTurns.AddNew();
			line1.C5_PackagesOutturned = 11;
			line2.C5_PackagesOutturned = 22;
			line3.C5_PackagesOutturned = 33;
			line1.WarehouseLocationID = locationBac1.PK;
			line2.WarehouseLocationID = locationBac2.PK;
			line3.WarehouseLocationID = locationCax.PK;
			line1.IsBeingReleasedNow = true;
			line2.IsBeingReleasedNow = false;
			line3.IsBeingReleasedNow = true;
			var wrapper = CcsukWrapper.New(mawb, Factory);
			AssertEquals("RELEASE NOTE", wrapper.RELEASEORREMOVALTITLE);
			mawb.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
			wrapper = CcsukWrapper.New(mawb, Factory);
			AssertEquals("REMOVAL AUTHORITY", wrapper.RELEASEORREMOVALTITLE);
			AssertEquals("", wrapper.RELEASEONLYTONEWSHED);
			AssertEquals("Unticked lines are excluded from the print", 2, wrapper.OutTurnLines.Count);
			AssertEquals(11, wrapper.OutTurnLines[0].PackagesOutturned);
			AssertEquals(33, wrapper.OutTurnLines[1].PackagesOutturned);
			AssertEquals(CusOutTurnTest.rowName1, wrapper.OutTurnLines[0].WarehouseLocation);
			AssertEquals(CusOutTurnTest.rowName3, wrapper.OutTurnLines[1].WarehouseLocation);
			mawb.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterShedRemoval, ZDateTime.BrettsBirthday);
			wrapper = CcsukWrapper.New(mawb, Factory);
			AssertEquals("Goods must only be released to the new shed operator", wrapper.RELEASEONLYTONEWSHED);
			AssertContains(mawb.PK.ToString(), wrapper.SCANNINGBARCODERAW);
		}

		public void TestCreateForVariousAwbs()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			var wrapper = CcsukWrapper.New(mawb, Factory);
			AssertEquals("111-22222222", wrapper.MAWBHAWBSPLIT);
			var house = mawb.ChildBills.AddNew();
			house.CS_HAWB = "33333333";
			wrapper = CcsukWrapper.New(house, Factory);
			AssertEquals("111-22222222-33333333", wrapper.MAWBHAWBSPLIT);
			var splitHouse = house.Splits.AddNew();
			splitHouse.SplitReference = "69";
			wrapper = CcsukWrapper.New(splitHouse, Factory);
			AssertEquals("111-22222222-33333333/69", wrapper.MAWBHAWBSPLIT);
		}

		public void TestOutTurnsForVariousAwbs()
		{
			var basic = Factory.New<CusMAWB>();
			var deliveryLine1 = basic.OutTurns.AddNew();
			var deliveryLine2 = basic.OutTurns.AddNew();
			deliveryLine1.IsBeingReleasedNow = true;
			deliveryLine2.IsBeingReleasedNow = true;
			var wrapper = CcsukWrapper.New(basic, Factory);
			AssertEquals(2, wrapper.OutTurnLines.Count);

			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			var deliveryLineH1 = house.OutTurns.AddNew();
			var deliveryLineH2 = house.OutTurns.AddNew();
			var deliveryLineH3 = house.OutTurns.AddNew();
			deliveryLineH1.IsBeingReleasedNow = true;
			deliveryLineH2.IsBeingReleasedNow = true;
			deliveryLineH3.IsBeingReleasedNow = true;
			wrapper = CcsukWrapper.New(house, Factory);
			AssertEquals(3, wrapper.OutTurnLines.Count);

			var splitHouse = house.Splits.AddNew();
			splitHouse.SplitReference = "01";
			deliveryLineH1.SplitReferenceToWhichThisPertains = splitHouse.SplitReference;
			var deliveryLineSH2 = house.OutTurns.AddNew();
			deliveryLineSH2.IsBeingReleasedNow = true;
			deliveryLineSH2.SplitReferenceToWhichThisPertains = splitHouse.SplitReference;
			wrapper = CcsukWrapper.New(splitHouse, Factory);
			AssertEquals(2, wrapper.OutTurnLines.Count);
		}
	}

	[TestedType(typeof(CcsukWrapperForDeliveryOrTransferLineCollection))]
	public class CcsukWrapperForDeliveryOrTransferLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CcsukWrapperForDeliveryOrTransferLineCollection>
	{
		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(CcsukWrapperForDeliveryOrTransferLineCollection);
		}

		protected override CcsukWrapperForDeliveryOrTransferLineCollection GetCollectionToTest()
		{
			return new CcsukWrapperForDeliveryOrTransferLineCollection(Basic, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return CcsukWrapperForDeliveryOrTransferLine.New(Basic.OutTurns.AddNew(), Factory);
		}

		CusMAWB basic;
		CusMAWB Basic
		{
			get { return basic ?? (basic = Factory.New<CusMAWB>()); }
		}
	}
}
