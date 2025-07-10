using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.DataInterface.Testing
{
	public class ChinaStandardExporterTest : TestCaseWithFactory
	{
		public void TestToFromDates()
		{
			ChinaStandardWrapper bizo = new ChinaStandardWrapper(Factory);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			bizo.Period = 200603;

			ChinaStandardExporterForTest exporter = new ChinaStandardExporterForTest(bizo, new NotificationBuffer());
			AssertEquals("From date", new ZDateTime(2006, 03, 1, 0, 0, 0), exporter.FromDateExposed);
			AssertEquals("To date", new ZDateTime(2006, 04, 1, 00, 00, 00), exporter.ToDateExposed);
		}

		protected AccountingPeriodTestHelper PeriodTestHelper
		{
			get { return periodTestHelper ?? (periodTestHelper = new AccountingPeriodTestHelper()); }
		}
		AccountingPeriodTestHelper periodTestHelper;

		class ChinaStandardExporterForTest : ChinaStandardExporter
		{
			public ChinaStandardExporterForTest(ChinaStandardWrapper bizObj, NotificationBuffer notification) : base(bizObj, notification) { }

			public ZDateTime FromDateExposed
			{
				get { return FromDate; }
			}

			public ZDateTime ToDateExposed
			{
				get { return ToDate; }
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected string BaseTestFilePath
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\"; }
		}
	}
}
