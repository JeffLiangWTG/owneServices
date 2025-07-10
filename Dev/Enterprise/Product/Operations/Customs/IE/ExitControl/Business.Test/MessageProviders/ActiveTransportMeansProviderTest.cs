using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class ActiveTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<ActiveTransportMeansProvider, CargoWise.Customs.IE.MessageContracts.Interfaces.ITransportMeans>
	{
		public void TestTypeOfIdentification()
		{
			AssertEquals("Identification Type", TransportTypeList.Codes.Air, IProvider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("Identification Number", "TEST123456", IProvider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals("Nationality", Core.Constants.CountryCodes.Ireland, IProvider.Nationality);
		}

		protected override ActiveTransportMeansProvider GetProvider() => new ActiveTransportMeansProvider(CreateData());

		CusExitReport CreateData()
		{
			var exitReport = Factory.New<CusExitReport>();
			exitReport.CER_RN_NKTransportNationality = Core.Constants.CountryCodes.Ireland;
			exitReport.CER_TransportID = "TEST123456";
			exitReport.CER_TransportType = TransportTypeList.Codes.Air;
			return exitReport;
		}
	}
}
