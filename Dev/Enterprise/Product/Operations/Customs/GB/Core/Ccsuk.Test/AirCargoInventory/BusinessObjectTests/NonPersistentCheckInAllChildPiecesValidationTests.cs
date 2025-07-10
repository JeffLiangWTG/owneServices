using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class NonPersistentCheckInAllChildPiecesValidationTests : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();
			hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			nonPersistentCheckInAllChildPieces = new NonPersistentCheckInAllChildPieces(mawb);
		}

		public void TestShedStorageLocationId()
		{
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);
			mawb.CargoTerminalOperator = "CAX";
			nonPersistentCheckInAllChildPieces.ShedStorageLocationId = locationCax.PK;
			AssertNoErrorContaining(nonPersistentCheckInAllChildPieces.ShedStorageLocationIdInfo, "valid");
			nonPersistentCheckInAllChildPieces.ShedStorageLocationId = locationBac1.PK;
			AssertHasErrorContaining(nonPersistentCheckInAllChildPieces.ShedStorageLocationIdInfo, "valid");
			nonPersistentCheckInAllChildPieces.ShedStorageLocationId = ZGuid.NewZGuid();
			AssertHasErrorContaining(nonPersistentCheckInAllChildPieces.ShedStorageLocationIdInfo, "valid");
		}

		public void TestMarksAndNumbers()
		{
			nonPersistentCheckInAllChildPieces.Validation.ValidateMarksAndNumbers();
			AssertHasErrorContaining("Splits but no marks - error", nonPersistentCheckInAllChildPieces.MarksAndNumbersInfo, "Splits exist, marks & numbers are required");
			nonPersistentCheckInAllChildPieces.MarksAndNumbers = "mark";
			nonPersistentCheckInAllChildPieces.Validation.ValidateMarksAndNumbers();
			AssertNoErrorContaining("Splits and marks - no error", nonPersistentCheckInAllChildPieces.MarksAndNumbersInfo, "Splits exist, marks & numbers are required");
			nonPersistentCheckInAllChildPieces.MarksAndNumbers = "";
			nonPersistentCheckInAllChildPieces.Validation.ValidateMarksAndNumbers();
			AssertHasErrorContaining("Reset: Splits asn no marks - error", nonPersistentCheckInAllChildPieces.MarksAndNumbersInfo, "Splits exist, marks & numbers are required");
			hawb.Splits.RemoveAndDeleteAll();
			nonPersistentCheckInAllChildPieces.Validation.ValidateMarksAndNumbers();
			AssertNoErrorContaining("No splits - no error", nonPersistentCheckInAllChildPieces.MarksAndNumbersInfo, "Splits exist, marks & numbers are required");
		}

		NonPersistentCheckInAllChildPieces nonPersistentCheckInAllChildPieces;
		CusMAWB mawb;
		CusHAWB hawb;
	}
}
