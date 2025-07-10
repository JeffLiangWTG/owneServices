using System;
using Enterprise.Customs.DE.ExitControl.Business.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class PackageActiveBorderTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<PackageActiveBorderTransportMeansProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new PackageActiveBorderTransportMeansProvider(null, ""));
		}

		public void TestActiveBorderTransportMeans() => CombineAssertions(() =>
		{
			informationType = A0132ReportInformationType.Codes.LP;
			AssertEquals("ActiveBorderTransportMeans when InformationType != 'FP'", "Sydney", GetProvider().ActiveBorderTransportMeans.Location);

			informationType = A0132ReportInformationType.Codes.FP;
			AssertNull("ActiveBorderTransportMeans when InformationType = 'FP'", GetProvider().ActiveBorderTransportMeans);
		});

		protected override PackageActiveBorderTransportMeansProvider GetProvider() => new PackageActiveBorderTransportMeansProvider(reportItem, informationType);

		protected override void SetUp()
		{
			base.SetUp();
			var (report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			report.CER_Location = "Sydney";
			reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CXP_Package = Factory.New<CusExitConsignmentPackage>().PK;
			informationType = "";
		}
		CusExitReportItem reportItem;
		string informationType;
	}
}
