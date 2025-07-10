using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE044UnloadingRemarkProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044UnloadingRemarkProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsArrivalMovementHeader missing", () => new IE044UnloadingRemarkProvider(null));
			});
		}

		protected override IE044UnloadingRemarkProvider GetProvider() => new IE044UnloadingRemarkProvider(nctsHeader);

		public void TestConform()
		{
			movementHeader.BM_NoChangesToReport = true;
			var provider = GetProvider();
			Assert("Conform", provider.Conform);
		}

		public void TestUnloadingCompletion()
		{
			movementHeader.BM_UnloadingCompleted = true;
			var provider = GetProvider();
			Assert("Unloading Complete", provider.UnloadingCompletion);
		}

		public void TestUnloadingDate()
		{
			movementHeader.BM_UnloadingDate = ZDateTime.BrettsBirthday.ToOffset();

			var provider = GetProvider();
			AssertEquals("Unloading Date", ZDateTime.BrettsBirthday.ToDateTime(), provider.UnloadingDate);
		}

		public void TestHasStateOfSeals()
		{
			var provider = GetProvider();
			AssertEquals("No ArrivalHeaderContainers", false, provider.HasStateOfSeals);

			nctsHeader.ArrivalHeaderContainers.AddNew();
			var container = nctsHeader.ArrivalHeaderContainers.AddNew();
			AssertEquals("No Seals", false, provider.HasStateOfSeals);

			container.Seals.AddNew().BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals("Any container has Seals", true, provider.HasStateOfSeals);
		}

		public void TestStateOfSeals()
		{
			movementHeader.BM_StateOfSealsBoolean = true;

			var provider = GetProvider();
			AssertEquals("State of Seals", true, provider.StateOfSeals);
		}

		public void TestUnloadingRemark()
		{
			movementHeader.BM_UnloadingRemarks = "Unloading remarks";

			var provider = GetProvider();
			AssertEquals("Unloading remarks", "Unloading remarks", provider.UnloadingRemark);
		}

		public void TestMRN()
		{
			// Implement in future
			Assert(true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = "UAP";
		}

		NctsHeader nctsHeader;
		NctsArrivalMovementHeader movementHeader;
	}
}
