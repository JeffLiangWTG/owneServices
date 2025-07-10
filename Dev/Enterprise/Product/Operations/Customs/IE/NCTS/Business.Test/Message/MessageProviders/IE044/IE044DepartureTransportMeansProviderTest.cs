using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE044DepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044DepartureTransportMeansProvider>
	{
		public void TestTypeOfIdentification()
		{
			cusTransportMeans.TPM_TransportState = "DEC";
			cusTransportMeans.TPM_TypeOfIdentification = "3";
			AssertEquals(ZString.Empty, Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			cusTransportMeans.TPM_TransportState = "DEC";
			cusTransportMeans.TPM_IdentificationNumber = "DepTranMeansID";
			AssertEquals(string.Empty, Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			cusTransportMeans.TPM_TransportState = "DEC";
			cusTransportMeans.TPM_RN_NKTransportNationality = "IE";
			AssertEquals(string.Empty, Provider.Nationality);
		}

		public void TestTypeOfIdentification_NEW()
		{
			cusTransportMeans.TPM_TransportState = "NEW";
			cusTransportMeans.TPM_TypeOfIdentification = "3";
			AssertEquals("3", Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_MIS()
		{
			cusTransportMeans.TPM_TransportState = "MIS";
			cusTransportMeans.TPM_TypeOfIdentification = "2";
			AssertEquals("2", Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber_NEW()
		{
			cusTransportMeans.TPM_TransportState = "NEW";
			cusTransportMeans.TPM_IdentificationNumber = "DepTranMeansID";
			AssertEquals("DepTranMeansID", Provider.IdentificationNumber);
		}

		public void TestNationality_NEW()
		{
			cusTransportMeans.TPM_TransportState = "NEW";
			cusTransportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Ireland;
			AssertEquals(Core.Constants.CountryCodes.Ireland, Provider.Nationality);
		}

		protected override IE044DepartureTransportMeansProvider GetProvider() => new IE044DepartureTransportMeansProvider(cusTransportMeans);

		protected override void SetUp()
		{
			base.SetUp();
			cusTransportMeans = Factory.New<DepartureCusTransportMeans>();
		}
		DepartureCusTransportMeans cusTransportMeans;
	}
}
