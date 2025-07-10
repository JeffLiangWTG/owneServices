using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	public class NonPersistentCheckInAllChildPiecesOrchestratorTests : TestCaseWithFactory
	{
		[TestDate(1986, 03, 12, 04, 27, 00)]
		public void TestNonPersistentCheckInAllChildPiecesOrchestrator()
		{
			var orchestrator = new NonPersistentCheckInAllChildPiecesOrchestrator(mawb);

			var dateExpected = new ZDateTime(1986, 03, 12, 04, 27, 00);
			var dateExpectedPlusOne = new ZDateTime(1986, 03, 13, 04, 27, 00);
			AssertEquals("PK", orchestrator.CheckInAllChildPiecesData.PackagesUnits);
			AssertEquals(dateExpected, orchestrator.CheckInAllChildPiecesData.ReceivedDate);
			AssertEquals("", orchestrator.CheckInAllChildPiecesData.MarksAndNumbers);
			AssertEquals(ZGuid.Empty, orchestrator.CheckInAllChildPiecesData.ShedStorageLocationId);
			AssertEquals(false, orchestrator.CheckInAllChildPiecesData.IsBeingReleasedNow);
			AssertEquals("", orchestrator.CheckInAllChildPiecesData.GoodsDescription);
			AssertEquals("", orchestrator.CheckInAllChildPiecesData.ContainerNumber);
			AssertEquals("", orchestrator.CheckInAllChildPiecesData.ContainerSeal);
			AssertEquals(false, orchestrator.CheckInAllChildPiecesData.IsDamaged);

			var guid = ZGuid.NewZGuid();
			orchestrator.CheckInAllChildPiecesData.PackagesUnits = "BG";
			orchestrator.CheckInAllChildPiecesData.ReceivedDate = ZDateTime.Now.AddDays(1);
			orchestrator.CheckInAllChildPiecesData.MarksAndNumbers = "Marks&Numbers";
			orchestrator.CheckInAllChildPiecesData.ShedStorageLocationId = guid;
			orchestrator.CheckInAllChildPiecesData.IsBeingReleasedNow = true;
			orchestrator.CheckInAllChildPiecesData.GoodsDescription = "Good";
			orchestrator.CheckInAllChildPiecesData.ContainerNumber = "ContainerNumber";
			orchestrator.CheckInAllChildPiecesData.ContainerSeal = "ContainerSeal";
			orchestrator.CheckInAllChildPiecesData.IsDamaged = true;

			AssertEquals("BG", orchestrator.CheckInAllChildPiecesData.PackagesUnits);
			AssertEquals(dateExpectedPlusOne, orchestrator.CheckInAllChildPiecesData.ReceivedDate);
			AssertEquals("Marks&Numbers", orchestrator.CheckInAllChildPiecesData.MarksAndNumbers);
			AssertEquals(guid, orchestrator.CheckInAllChildPiecesData.ShedStorageLocationId);
			AssertEquals(true, orchestrator.CheckInAllChildPiecesData.IsBeingReleasedNow);
			AssertEquals("Good", orchestrator.CheckInAllChildPiecesData.GoodsDescription);
			AssertEquals("ContainerNumber", orchestrator.CheckInAllChildPiecesData.ContainerNumber);
			AssertEquals("ContainerSeal", orchestrator.CheckInAllChildPiecesData.ContainerSeal);
			AssertEquals(true, orchestrator.CheckInAllChildPiecesData.IsDamaged);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();
			mawb.ChildBills.AddNew();
		}

		CusMAWB mawb;
	}
}
