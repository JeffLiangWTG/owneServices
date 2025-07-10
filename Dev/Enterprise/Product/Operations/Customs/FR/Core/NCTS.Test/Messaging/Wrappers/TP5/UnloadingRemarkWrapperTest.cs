using System;
using Enterprise.Customs.FR.Business.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class UnloadingRemarkWrapperTest : Customs.Business.Testing.DataProviderTestCase<UnloadingRemarkWrapper>
	{
		public void TestConform()
		{
			AssertEquals("Conform should equal to BM_NoChangesToReport.", true, Provider.Conform);
		}

		public void TestUnloadingCompletion()
		{
			AssertEquals("UnloadingCompletion should equal to BM_UnloadingCompleted.", true, Provider.UnloadingCompletion);
		}

		public void TestUnloadingRemark()
		{
			AssertEquals("UnloadingRemark should equal to BM_UnloadingRemarks.", "Unloading Remarks", Provider.UnloadingRemark);
		}

		[TestDate(2024, 09, 03)]
		public void TestUnloadingDate()
		{
			AssertEquals("UnloadingDate should equal to BM_UnloadingDate.ToDateTime().", new DateTime(2024, 09, 03), Provider.UnloadingDate);
		}

		public void TestStateOfSeals()
		{
			AssertEquals("StateOfSeals should equal to null if no seals provided.", null, Provider.StateOfSeals);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_StateOfSealsBoolean = true;
			var arrivalContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
			var seal = arrivalContainer.Seals.AddNew();
			var providerWithSeal = UnloadingRemarkWrapper.New(movementHeader);
			AssertEquals("StateOfSeals should equal to true if any seals provided.", true, providerWithSeal.StateOfSeals);
		}

		protected override UnloadingRemarkWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;

			movementHeader.BM_NoChangesToReport = true;
			movementHeader.BM_UnloadingCompleted = true;
			movementHeader.BM_StateOfSealsBoolean = true;
			movementHeader.BM_UnloadingRemarks = "Unloading Remarks";

			return UnloadingRemarkWrapper.New(movementHeader);
		}
	}
}
