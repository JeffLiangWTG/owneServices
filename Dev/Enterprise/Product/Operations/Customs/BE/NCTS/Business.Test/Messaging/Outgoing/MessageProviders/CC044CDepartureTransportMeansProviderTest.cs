using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CC044CDepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<CC044CDepartureTransportMeansProvider>
	{
		public void TestSequenceNumber()
		{
			data.TPM_TransportState = "XXX";
			data.TPM_SequenceNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestTypeOfIdentification()
		{
			data.TPM_TransportState = "XXX";
			data.TPM_TypeOfIdentification = "3";
			AssertNull(Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			data.TPM_TransportState = "XXX";
			data.TPM_IdentificationNumber = "DepTranMeansID";
			AssertEquals(string.Empty, Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			data.TPM_TransportState = "XXX";
			data.TPM_RN_NKTransportNationality = "BE";
			AssertEquals(string.Empty, Provider.Nationality);
		}

		public void TestSequenceNumber_NEW()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			data.TPM_SequenceNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestTypeOfIdentification_NEW()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			data.TPM_TypeOfIdentification = "3";
			AssertEquals(3, Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber_NEW()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			data.TPM_IdentificationNumber = "DepTranMeansID";
			AssertEquals("DepTranMeansID", Provider.IdentificationNumber);
		}

		public void TestNationality_NEW()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			data.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Belgium;
			AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.Nationality);
		}

		public void TestSequenceNumber_MIS()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			data.TPM_SequenceNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestTypeOfIdentification_MIS()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			data.TPM_TypeOfIdentification = "3";
			AssertNull(Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber_MIS()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			data.TPM_IdentificationNumber = "DepTranMeansID";
			AssertEquals(string.Empty, Provider.IdentificationNumber);
		}

		public void TestNationality_MIS()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
			data.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Belgium;
			AssertEquals(string.Empty, Provider.Nationality);
		}

		public void TestInvalidTypeOfIdentification()
		{
			data.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
			data.TPM_TypeOfIdentification = "t";
			AssertNull(provider.TypeOfIdentification);
		}

		protected override void SetUp()
		{
			base.SetUp();
			data = Factory.New<CusTransportMeans>();
			provider = new CC044CDepartureTransportMeansProvider(data);
		}
		CusTransportMeans data;

		protected override CC044CDepartureTransportMeansProvider GetProvider() => provider;

		CC044CDepartureTransportMeansProvider provider;
	}
}
