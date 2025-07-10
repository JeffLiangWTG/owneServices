using System.Diagnostics;
using System.Globalization;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PrintProcessing
{
	internal abstract class MergedPrintGroupProcessor
	{
		public MergedPrintGroupProcessor(StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager jobManager = null)
		{
			this.MergedPrintGroup = mergedPrintGroup;
			JobManager = jobManager;
		}

		protected StmPrintJobMergedCollection MergedPrintGroup { get; set; }

		public PrintJobManager JobManager { get; protected set; }

		public PrintJobManager.ProgressDelegate LogProgress { get; protected set; }

		#region Process

		public void Process()
		{
			this.MergedPrintGroup.Sort(StmPrintJob.Schema.SP_Sequence);

			int currentGroup = PrintJobGroups.Count == 1 ? 0 : 1;
			int totalGroupCount = PrintJobGroups.Count == 1 ? 0 : PrintJobGroups.Count;

			foreach (StmPrintJobMergedCollection mergedJobs in PrintJobGroups)
			{
				PreProcess(mergedJobs, currentGroup, totalGroupCount);
				ProcessPrintJobs(mergedJobs);
				PostProcess(mergedJobs, currentGroup, totalGroupCount);

				if (PrintJobGroups.Count > 1)
				{
					currentGroup++;
				}
			}
		}

		protected virtual void ProcessPrintJobs(StmPrintJobMergedCollection mergedPrintJobs)
		{
			foreach (StmPrintJob printJob in mergedPrintJobs)
			{
				ProcessIndividualItem(printJob);
			}
		}

		protected void ProcessIndividualItem(StmPrintJob printJob)
		{
			CultureInfo originalCulture = Culture.Current;
			try
			{
				if (printJob.SP_IsLocalCulture && printJob.Branch != null && printJob.Branch.Company != null && printJob.Branch.Company.Country != null)
				{
					CultureInfo culture = Culture.GetCulture(printJob.Branch.Company.GC_RN_NKCountryCode);
					if (culture != null)
					{
						Culture.Set(culture);
					}
				}
				if (JobManager != null)
				{
					JobManager.CurrentProcessingJob = printJob;
				}
				ProcessIndividualItemCore(printJob);
			}
			finally
			{
				Culture.Set(originalCulture);
			}
		}

		protected abstract void ProcessIndividualItemCore(StmPrintJob printJob);

		protected virtual void PreProcess(StmPrintJobMergedCollection printJobs, int groupNumber, int totalGroupCount)
		{
		}

		protected virtual void PostProcess(StmPrintJobMergedCollection printJobs, int groupNumber, int totalGroupCount)
		{
		}

		protected virtual StmPrintJobGroupCollection PrintJobGroups
		{
			get
			{
				StmPrintJobGroupCollection result = new StmPrintJobGroupCollection();
				result.Add(MergedPrintGroup);
				return result;
			}
		}

		protected void LogProcessing(StmPrintJob printJob)
		{
			var logMessage = Res.GetString("DE486B64-CD3B-4AD3-ACFF-E8A509207E55",
				"Processing document \"{0}\" with subject \"{1}\".\r\nDocument type [{2}].\r\nRelated Business Context: [{3}].\r\nEmail Attachments: [{4}].\r\nNumber of Copies: [{5}].\r\nEmail To: [{6}].\r\nFax Destination:[{7}].",
				printJob.SP_DocumentName, printJob.SP_EmailSubjectLine, printJob.SP_DocumentType, printJob.SP_RelatedBusinessContext, printJob.SP_EmailAttachments, printJob.SP_Copies, printJob.EmailToRecipients.Value, printJob.SP_FaxDestination);

			LogProgress?.Invoke(TraceEventType.Information, logMessage);
		}
		#endregion
	}
}
