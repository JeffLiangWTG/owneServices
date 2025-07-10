using System;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class PlaceOfUnloadingProviderTest : Customs.Business.Testing.DataProviderTestCase<PlaceOfUnloadingProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PlaceOfUnloadingProvider(null));
		}

		public void TestUnLocode() => CombineAssertions(() =>
		{
			movementHeader.BM_ForeignDestPortKCode = "BE";
			AssertEquals("less than or 2 characters", string.Empty, Provider.UnLocode);
			movementHeader.BM_ForeignDestPortKCode = "BE10";
			AssertEquals("more than 2 characters", "BE10", Provider.UnLocode);
		});

		public void TestCountry() => CombineAssertions(() =>
		{
			movementHeader.BM_ForeignDestPortKCode = "BE";
			AssertEquals("less than or 2 characters", "BE", Provider.Country);
			movementHeader.BM_ForeignDestPortKCode = "BE10";
			AssertEquals("more than 2 characters", string.Empty, Provider.Country);
		});

		public void TestLocation() => CombineAssertions(() =>
		{
			movementHeader.BM_ForeignDestPortKCode = "BE";
			movementHeader.BM_PlaceOfUnloading = "10";
			AssertEquals("less than or 2 characters", "10", Provider.Location);
			movementHeader.BM_ForeignDestPortKCode = "BE10";
			AssertEquals("more than 2 characters", string.Empty, Provider.Location);
		});

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;

			provider = new PlaceOfUnloadingProvider(movementHeader);
		}
		NctsDepartureMovementHeader movementHeader;
		PlaceOfUnloadingProvider provider;

		protected override PlaceOfUnloadingProvider GetProvider() => provider;
	}
}
