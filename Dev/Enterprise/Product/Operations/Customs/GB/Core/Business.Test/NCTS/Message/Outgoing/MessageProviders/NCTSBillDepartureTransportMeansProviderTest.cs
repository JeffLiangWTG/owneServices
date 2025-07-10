using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class NCTSBillDepartureTransportMeansProviderTest : DataProviderTestCase<NCTSBillDepartureTransportMeansProvider>
	{
		public void TestSequenceNumber()
		{
			departureCusTransportMeans.TPM_SequenceNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestTypeOfIdentification_TransportMode1()
		{
			AssertEquals(10, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_TransportMode2()
		{
			transportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureCusTransportMeans.TPM_SequenceNumber = 1;
			AssertEquals(20, Provider.TypeOfIdentification);

			departureCusTransportMeans.TPM_SequenceNumber = 2;
			AssertEquals(21, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_TransportMode3()
		{
			transportMode = ModeOfTransportList.Codes._3_RoadTransport;
			departureCusTransportMeans.TPM_SequenceNumber = 1;
			AssertEquals(30, Provider.TypeOfIdentification);

			departureCusTransportMeans.TPM_SequenceNumber = 2;
			AssertEquals(31, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_TransportMode4()
		{
			transportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals(40, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_TransportMode8()
		{
			transportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals(81, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_TransportMode9()
		{
			transportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			departureCusTransportMeans.TPM_TypeOfIdentification = "20";
			AssertEquals(20, Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			departureCusTransportMeans.TPM_IdentificationNumber = "ABC123";
			AssertEquals("ABC123", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			departureCusTransportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Belgium;
			AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.Nationality);
		}

		protected override void SetUp()
		{
			base.SetUp();

			departureCusTransportMeans = Factory.New<DepartureCusTransportMeans>();
			transportMode = ModeOfTransportList.Codes._1_SeaTransport;
		}

		DepartureCusTransportMeans departureCusTransportMeans;
		string transportMode;

		protected override NCTSBillDepartureTransportMeansProvider GetProvider() => new NCTSBillDepartureTransportMeansProvider(departureCusTransportMeans, transportMode);
	}
}
