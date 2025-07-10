using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class EXTPREHeaderProviderTest : Customs.Business.Testing.DataProviderTestCase<EXTPREHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXTPREHeaderProvider(null));
		}

		[TestDate(2022, 1, 21, 16, 47, 21)]
		[TestUtcOffset(1, 0, 0)]
		public void TestArrivalNotificationDateAndTime()
		{
			AssertEquals(new DateTime(2022, 1, 21, 16, 47, 21), Provider.ArrivalNotificationDateAndTime);
		}

		protected override EXTPREHeaderProvider GetProvider() => new EXTPREHeaderProvider(report);

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
