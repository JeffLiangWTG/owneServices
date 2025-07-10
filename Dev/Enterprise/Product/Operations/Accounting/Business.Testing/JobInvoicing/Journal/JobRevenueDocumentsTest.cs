using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournalDocumentSupporter))]
	public class JobRevenueDocumentsTest : DocumentSupporterTest
	{
		public void TestGetDocBusinessObject()
		{
			var wrappers = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.JobRevenueJournal, null);
			AssertNotNull("DocWrapper should be created.", wrappers[0]);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("DataContext.JobRevenueJournal is supported.", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.JobRevenueJournal)));
			AssertEquals("DataContext.GenericFreightJob should be supported to use DocBuilder.", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		JobRevenueJournalDocumentSupporter DocumentSupporter
		{
			get { return (JobRevenueJournalDocumentSupporter)Journal.DocumentSupporter; }
		}

		JobRevenueJournal Journal
		{
			get { return journal ?? (journal = (JobRevenueJournal)GetDocumentSupportableBusinessObject()); }
		}
		JobRevenueJournal journal;

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<JobRevenueJournal>();
		}
	}
}
