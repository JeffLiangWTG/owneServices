using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PrintProcessing
{
	public abstract class MergedPrintGroupProcessorTestCase : TestCaseWithFactory
	{
		public virtual void TestCulture()
		{
			int cultureChangeCounter = 0;

			StmPrintQueue printQueue = TestAssistant.CreateTestPrintQueue(Factory);
			StmPrintJobMergedCollection printGroup = new StmPrintJobMergedCollection(Factory);

			// Pre-cache lazy accessed data (product registration key) to not interfere with logic below
			_ = DataRegistry.Instance.RawRegistry.SystemEnterpriseCode.Value;
			_ = DataRegistry.Instance.PhysicalServerID;

			using (TempFile tempFile = TempFile.New())
			{
				StmPrintJob printJob = printGroup.AddNew();
				printJob.SP_JobType = "PRN";
				printJob.SP_IsLocalCulture = true;
				printJob.SP_SQ = printQueue.PK;
				printJob.StoredAttachmentFilename = tempFile.Filename;

				StmPrintJob printJob2 = printGroup.AddNew();
				printJob2.SP_JobType = "PRN";
				printJob2.SP_IsLocalCulture = false;
				printJob2.SP_SQ = printQueue.PK;
				printJob2.StoredAttachmentFilename = tempFile.Filename;

				StmPrintJob printJob3 = printGroup.AddNew();
				printJob3.SP_JobType = "PRN";
				printJob3.SP_IsLocalCulture = true;
				printJob3.SP_SQ = printQueue.PK;
				printJob3.StoredAttachmentFilename = tempFile.Filename;

				MergedPrintGroupProcessor processor = GetProcessor(printGroup);
				Culture.CultureChanged += Culture_CultureChanged;
				try
				{
					AssertEquals(0, cultureChangeCounter);

					processor.Process();

					AssertEquals("2 jobs are localized, so 2 events per job", 4, cultureChangeCounter);
				}
				finally
				{
					Culture.CultureChanged -= Culture_CultureChanged;
				}
			}

			void Culture_CultureChanged(object sender, Culture.CultureChangedEventArgs e)
			{
				cultureChangeCounter++;
			}
		}

		internal abstract MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection printGroup);
	}
}
