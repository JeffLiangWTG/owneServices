using System;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DataProviderFactoryTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetReportDataProvider()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(TemplateFileName, TestFilesSubFolder.ReportTestFiles);
			using (var rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				Assert(rpt.DataProvider is ReportDataProvider);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("Voyage").RowCount);
				AssertEquals(3, rpt.DataProvider.GetDataRowSource("ReceivedBooking").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("WeightUnit").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("VolumeUnit").RowCount);
				AssertEquals(2, rpt.DataProvider.GetDataRowSource("NonReceivedBooking").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("ReceivedBookingWeight").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("ReceivedBookingVolume").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("ReceivedBookingPackage").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("NonReceivedBookingWeight").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("NonReceivedBookingVolume").RowCount);
				AssertEquals(1, rpt.DataProvider.GetDataRowSource("NonReceivedBookingPackage").RowCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetBusinessObjectDataProvider()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(TemplateFileName, TestFilesSubFolder.ReportTestFiles);
			using (var rpt = new Report(pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", null, DocumentDirection.ANY, false))
			{
				rpt.ResetDataProvider();
				Assert(rpt.DataProvider is BusinessObjectDataProvider);
				AssertEquals(3, rpt.DataProvider.GetDataRowSource("Children").RowCount);
			}
		}

		const string TemplateFileName = "BookingSummaryDoc.xls";
		DocumentPack pack;
		IDisposable temporarilyUseMainConnection;

		protected override void SetUp()
		{
			base.SetUp();
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();
			pack = new DocumentPack();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		protected override void TearDown()
		{
			base.TearDown();
			temporarilyUseMainConnection?.Dispose();
		}
	}
}
