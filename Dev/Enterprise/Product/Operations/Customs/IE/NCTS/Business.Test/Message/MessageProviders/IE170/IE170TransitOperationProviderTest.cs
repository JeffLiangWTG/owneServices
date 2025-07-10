using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE170TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE170TransitOperationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new IE170TransitOperationProvider(null));
		}

		public void TestLRN()
		{
			movementHeader.BM_PaperlessInbondNum = "LRN00ROS1234";
			AssertEquals("LRN", "LRN00ROS1234", Provider.LRN);
		}

		public void TestLimitDate()
		{
			AssertEquals(DateTime.MinValue, Provider.LimitDate);
			movementHeader.BM_ExportDate = ZDateTime.BrettsBirthday;
			AssertEquals(new DateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Unspecified), Provider.LimitDate);
		}

		public void TestReducedDatasetIndicator()
		{
			movementHeader.BM_ReducedDatasetIndicator = ZBool.False;
			AssertEquals(false, Provider.ReducedDatasetIndicator);

			movementHeader.BM_ReducedDatasetIndicator = ZBool.True;
			AssertEquals(true, Provider.ReducedDatasetIndicator);
		}

		protected override IE170TransitOperationProvider GetProvider() => new IE170TransitOperationProvider(movementHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}
