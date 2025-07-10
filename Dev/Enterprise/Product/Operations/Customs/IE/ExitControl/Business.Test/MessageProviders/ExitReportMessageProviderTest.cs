using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class ExitReportMessageProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusExitReport missing", () => new ExitReportMessageProviderForTest(null));

				var exitReport = Factory.New<CusExitReport>();
				AssertExceptionThrown<ArgumentException>("CusExitHeader missing", () => new ExitReportMessageProviderForTest(exitReport));

				var exitHeader = Factory.New<CusExitHeader>();
				exitReport.CER_CXH_Header = exitHeader.PK;
				var consignment = exitHeader.CusExitConsignments.AddNew();
				exitReport.CER_CXC_Consignment = consignment.PK;
				AssertNoExceptionThrown("No missing", () => new ExitReportMessageProviderForTest(exitReport));
			});
		}

		class ExitReportMessageProviderForTest : ExitReportMessageProvider
		{
			public ExitReportMessageProviderForTest(CusExitReport exitReport)
				: base(exitReport)
			{
			}
		}
	}
}
