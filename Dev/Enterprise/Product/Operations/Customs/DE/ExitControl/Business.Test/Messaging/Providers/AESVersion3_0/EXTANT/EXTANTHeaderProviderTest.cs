using System;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class EXTANTHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<EXTANTHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTANTHeaderProvider(null));
		}

		protected override EXTANTHeaderProvider GetProvider() => new EXTANTHeaderProvider(report);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
		}
		CusExitReport report;
	}
}
