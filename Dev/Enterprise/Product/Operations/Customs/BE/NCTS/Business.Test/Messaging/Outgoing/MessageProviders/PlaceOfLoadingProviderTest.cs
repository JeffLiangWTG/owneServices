using System;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class PlaceOfLoadingProviderTest : Customs.Business.Testing.DataProviderTestCase<PlaceOfLoadingProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PlaceOfLoadingProvider(null));
		}

		public void TestUnLocode() => CombineAssertions(() =>
		{
			movementHeader.BM_PortOfPresentationCode = "BE";
			AssertEquals("less than or 2 characters", string.Empty, Provider.UnLocode);
			movementHeader.BM_PortOfPresentationCode = "BE10";
			AssertEquals("more than 2 characters", "BE10", Provider.UnLocode);
		});

		public void TestCountry() => CombineAssertions(() =>
		{
			movementHeader.BM_PortOfPresentationCode = "BE";
			AssertEquals("less than or 2 characters", "BE", Provider.Country);
			movementHeader.BM_PortOfPresentationCode = "BE10";
			AssertEquals("more than 2 characters", string.Empty, Provider.Country);
		});

		public void TestLocation() => CombineAssertions(() =>
		{
			movementHeader.BM_PortOfPresentationCode = "BE";
			movementHeader.BM_PlaceOfLoading = "10";
			AssertEquals("less than or 2 characters", "10", Provider.Location);
			movementHeader.BM_PortOfPresentationCode = "BE10";
			AssertEquals("more than 2 characters", string.Empty, Provider.Location);
		});

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;

			provider = new PlaceOfLoadingProvider(movementHeader);
		}
		NctsDepartureMovementHeader movementHeader;
		PlaceOfLoadingProvider provider;

		protected override PlaceOfLoadingProvider GetProvider() => provider;
	}
}
