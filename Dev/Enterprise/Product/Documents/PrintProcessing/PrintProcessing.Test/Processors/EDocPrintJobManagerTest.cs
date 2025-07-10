using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class EDocPrintJobManagerTest : TestCaseWithFactory
	{
		public void TestGetProcessor()
		{
			var printJobManager = new EDocPrintJobManager();
			var printJobMergedCollection = new StmPrintJobMergedCollection(Factory);
			AssertEquals(typeof(EDocProcessor), printJobManager.GetProcessor(printJobMergedCollection, null).GetType());
		}

		public void TestBumpUpRetryAttemptWhenSaveException()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_JobType = nameof(PrintType.DDS);
			printJob.SP_EDocsProcessed = false;
			Factory.Save();

			var testZSaveException = true;
			BusinessObjectFactory.SavingEventHandler action = f =>
			{
				if (testZSaveException)
				{
					var row = ((IBusinessObjectInternals)Factory.Load<StmPrintJob>(new ZQuery()).First(x => x.PK == printJob.PK)).Row;
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(""), row, ((IDbConnected)f).Connection), f);
				}

				testZSaveException = false;
			};

			var manager = new EDocPrintJobManager();
			manager.ProcessPrintJobs(new[] { printJob });

			printJob.Reload();

			AssertEquals(false, printJob.IsDeleted);
			AssertEquals(1, (int)printJob.SP_RetryAttempts);
			AssertEquals(false, printJob.SP_EDocsProcessed);
		}

		public void TestPrintJobIgnoreConcurrencyCheckForJobTypeAndRetryAttempts()
		{
			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_RelatedBusinessContext = "DUM";
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = false;
			var reloadedPrintJob = anotherFactory.Load<StmPrintJob>(printJob.PK);
			reloadedPrintJob.SP_RetryAttempts = 1;
			reloadedPrintJob.SP_JobType = "DDS";
			anotherFactory.Save();

			AssertEquals("PRN", printJob.SP_JobType);
			AssertEquals(0, (int)printJob.SP_RetryAttempts);

			var manager = new EDocPrintJobManager();
			manager.ProcessPrintJobs(new[] { printJob });

			printJob.Reload();
			AssertEquals("DDS", printJob.SP_JobType);
			AssertEquals(1, (int)printJob.SP_RetryAttempts);
			AssertEquals(true, printJob.SP_EDocsProcessed);
		}

		public void TestProcessPrintJobs()
		{
			var log = "";
			var printJobManager = new EDocPrintManagerForTest();
			printJobManager.OnProgress += (eventType, message) => log += message + "\r\n";

			var printJob1 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob1.SP_RelatedBusinessContext = "SHP";
			printJob1.SP_JobType = nameof(PrintType.DDS);
			printJob1.SP_EDocsProcessed = true;

			var printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_RelatedBusinessContext = "SHP";
			printJob2.SP_JobType = nameof(PrintType.EML);
			printJob2.SP_EDocsProcessed = false;

			var printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_RelatedBusinessContext = "SHP";
			printJob3.SP_JobType = nameof(PrintType.DDS);
			printJob3.SP_EDocsProcessed = false;

			var printJob4 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob4.SP_RelatedBusinessContext = "";

			printJobManager.ProcessPrintJobs(new[] { printJob1, printJob2, printJob3, printJob4 });

			var expectedMessage = @"Processing 1 of 4 queued DDS jobs
Processing 2 of 4 queued EML jobs
Starting EDocProcessorForTest to process ""EML"" print jobs
Processing 3 of 4 queued DDS jobs
Starting EDocProcessorForTest to process ""DDS"" print jobs
Processing 4 of 4 queued PRN jobs
";
			AssertEquals("Should process these 4 jobs", expectedMessage, log);
			AssertEquals("printJob1 should be deleted", true, printJob1.IsDeleted);
			AssertEquals("printJob2 should be marked as processed", true, printJob2.SP_EDocsProcessed);
			AssertEquals("printJob3 should be deleted", true, printJob3.IsDeleted);
			AssertEquals("printJob4 should be marked as processed", true, printJob4.SP_EDocsProcessed);
		}

		public void TestPrintJobIgnoreEMLWithStmReportRun()
		{
			var log = "";
			var printJobManager = new EDocPrintManagerForTest();
			printJobManager.OnProgress += (eventType, message) => log += message + "\r\n";

			var printJob1 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob1.SP_JobType = nameof(PrintType.EML);
			printJob1.SP_RelatedBusinessContext = "RTS";
			printJob1.SP_ParentTableName = StmReportRunSchema.Constants.TableName;
			printJob1.SP_EDocsProcessed = false;

			var printJob2 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob2.SP_JobType = nameof(PrintType.DDS);
			printJob2.SP_RelatedBusinessContext = "RTS";
			printJob2.SP_ParentTableName = StmReportRunSchema.Constants.TableName;
			printJob2.SP_EDocsProcessed = false;

			var printJob3 = Factory.NewWithValidTestData<StmPrintJob>();
			printJob3.SP_JobType = nameof(PrintType.EML);
			printJob3.SP_RelatedBusinessContext = "SHP";
			printJob3.SP_EDocsProcessed = false;

			printJobManager.ProcessPrintJobs(new[] { printJob1, printJob2, printJob3 });

			var anotherFactory = new BusinessObjectFactory();
			var reloadedPrintJob = anotherFactory.Load<StmPrintJob>(printJob1.PK);
			AssertEquals("printjob1 should be marked as processed", true, reloadedPrintJob.SP_EDocsProcessed);
			var expectedMessage = @"Processing 1 of 2 queued DDS jobs
Starting EDocProcessorForTest to process ""DDS"" print jobs
Processing 2 of 2 queued EML jobs
Starting EDocProcessorForTest to process ""EML"" print jobs
";
			AssertEquals("Should process printjob2 & printjob3", expectedMessage, log);
		}
	}

	class EDocPrintManagerForTest : EDocPrintJobManager
	{
		internal override MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection mergedPrintGroup, ProgressDelegate logProgress)
		{
			return new EDocProcessorForTest(mergedPrintGroup, logProgress);
		}
	}

	class EDocProcessorForTest : EDocProcessor
	{
		public EDocProcessorForTest(StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager.ProgressDelegate logProgress) : base(mergedPrintGroup, logProgress)
		{
		}

		protected override void ProcessPrintJobs(StmPrintJobMergedCollection mergedPrintJobs) { }
	}
}
