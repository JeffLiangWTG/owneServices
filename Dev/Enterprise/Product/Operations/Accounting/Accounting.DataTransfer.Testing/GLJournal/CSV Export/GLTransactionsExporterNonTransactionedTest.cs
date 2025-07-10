using System.IO;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[UseSnapshotProtection]
	public class GLTransactionsExporterNonTransactionedTest : TestCase
	{
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestBatchNumberNotUsedWhenNothingToExport()
		{
			BusinessObjectFactory factoryForTestData = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(factoryForTestData);
			AccPeriodManagement priorPeriod = null;
			AccPeriodManagement firstPeriod = null;
			AccGLAggregate aggregation = null;
			AccGLHeader unusedGLAccountWithAggregatedBalance = null;

			priorPeriod = PeriodTestHelper.SetupSinglePeriod(200602, new ZDateTime(2006, 2, 1), new ZDateTime(2006, 3, 1).AddMinutes(-1));
			firstPeriod = PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 4, 1).AddMinutes(-1));
			unusedGLAccountWithAggregatedBalance = testObjectCreator.CreateAccGLHeader("9999.50.50", "TS", "Some P&L with balance", "P&L", "DR");

			aggregation = factoryForTestData.New<AccGLAggregate>();
			aggregation.AA_AG = unusedGLAccountWithAggregatedBalance.PK;
			aggregation.AA_Amount = 2000m;
			aggregation.AA_Period = priorPeriod.AM_Period;
			aggregation.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregation.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregation.AA_GE = GlbDepartment.CurrentDepartment.PK;
			factoryForTestData.Save();

			BusinessObjectFactory factoryForExport = new BusinessObjectFactory();
			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1000.csv");
			TestCaseHelper.ClearTable(GenExportBatchSequenceSchema.Constants.TableName);

			long expectedBatchNumber = Env.NumberFountains.GenExportBatchSequenceBatchNo.GetTodaysPeriodFountain().PeekPreliminary(factoryForExport);

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(factoryForExport);
			bizObj.Exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = true;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				factoryForExport.Save();
			}
			catch (ZCannotSaveException e)
			{
				AssertEquals("Should be an export failed exception as there's nothing to export", "Export failed.", e.Message);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
			AssertEquals("Batch number should be same as last time because nothing exported", expectedBatchNumber, Env.NumberFountains.GenExportBatchSequenceBatchNo.GetTodaysPeriodFountain().PeekPreliminary(factoryForExport));
		}

		AccountingPeriodTestHelper PeriodTestHelper
		{
			get { return periodTestHelper ?? (periodTestHelper = new AccountingPeriodTestHelper()); }
		}
		AccountingPeriodTestHelper periodTestHelper;
	}
}
