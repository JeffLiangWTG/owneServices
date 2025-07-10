using System;
using CargoWise.Types;
using Enterprise.Customs.DE.ExitControl.Business.Testing;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class ActiveBorderTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<ActiveBorderTransportMeansProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ActiveBorderTransportMeansProvider(null));
		}

		public void TestDepartureDateAndTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CER_DateTime is invalid", default(DateTime), GetProvider().DepartureDateAndTime);

				report.CER_DateTime = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);
				AssertEquals("CER_DateTime is valid", new DateTime(2022, 02, 15, 15, 43, 27), GetProvider().DepartureDateAndTime);
			});
		}

		public void TestLocation()
		{
			report.CER_Location = "Sydney";
			AssertEquals("Sydney", GetProvider().Location);
		}

		protected override ActiveBorderTransportMeansProvider GetProvider() => new ActiveBorderTransportMeansProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
			var header = CusExitHeaderTest.GetNewBusinessObject(Factory);
			report = header.CusExitReports.AddNew();
		}
		CusExitReport report;
	}
}
