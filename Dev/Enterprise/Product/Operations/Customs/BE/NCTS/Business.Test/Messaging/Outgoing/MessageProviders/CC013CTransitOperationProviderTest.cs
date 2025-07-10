using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC013CTransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC013CTransitOperationProvider>
	{
		public void TestLRN()
		{
			CombineAssertions(() =>
			{
				header.ArrivalMrnFromUser = "MRNOfTheNCTS";
				header.MovementHeader.BM_PaperlessInbondNum = "LRNOfTheNCT";
				AssertNull("When MRN is filled, LRN must be null", Provider.LRN);

				header.ArrivalMrnFromUser = ZString.Empty;
				AssertEquals("When MRN is empty, LRN must be filled", "LRNOfTheNCT", Provider.LRN);
			});
		}

		public void TestMRN()
		{
			header.ArrivalMrnFromUser = "MRNOfTheNCTS";
			AssertEquals("MRN", "MRNOfTheNCTS", Provider.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			provider = new CC013CTransitOperationProvider(header);
		}

		CC013CTransitOperationProvider provider;
		NctsHeader header;

		protected override CC013CTransitOperationProvider GetProvider() => provider;
	}
}
