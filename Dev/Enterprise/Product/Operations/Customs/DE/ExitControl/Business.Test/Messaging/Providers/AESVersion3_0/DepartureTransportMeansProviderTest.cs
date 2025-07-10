using System;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class DepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new DepartureTransportMeansProvider(null));
		}

		public void TestTypeOfIdentification()
		{
			report.CER_TransportType = CusExitReportTransportTypeList.Codes._10;
			AssertEquals(CusExitReportTransportTypeList.Codes._10, GetProvider().TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			report.CER_TransportID = "XYZ123";
			AssertEquals("XYZ123", GetProvider().IdentificationNumber);
		}

		public void TestNationality()
		{
			report.CER_RN_NKTransportNationality = Core.Constants.CountryCodes.Italy;
			AssertEquals(Core.Constants.CountryCodes.Italy, GetProvider().Nationality);
		}

		protected override DepartureTransportMeansProvider GetProvider() => new DepartureTransportMeansProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
			report = Factory.New<CusExitReport>();
		}
		CusExitReport report;
	}
}
