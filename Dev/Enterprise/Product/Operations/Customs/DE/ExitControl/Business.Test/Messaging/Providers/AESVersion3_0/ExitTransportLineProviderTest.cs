using System;
using Enterprise.Customs.DE.ExitControl.Business.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class ExitTransportLineProviderTest : Customs.Business.Testing.DataProviderTestCase<ExitTransportLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ExitTransportLineProvider(null));
		}

		public void TestSequenceNumber()
		{
			AssertEquals("2", GetProvider().SequenceNumber);
		}

		protected override ExitTransportLineProvider GetProvider() => new ExitTransportLineProvider(reportItem);

		protected override void SetUp()
		{
			base.SetUp();
			var (report, _) = CusExitReportTest.GetNewBusinessObject(Factory);
			reportItem = report.CusExitReportItems.AddNew();
			var consignmentItem = Factory.New<CusExitConsignmentItem>();
			consignmentItem.CCI_LineNumber = 2;
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
		}
		CusExitReportItem reportItem;
	}
}
