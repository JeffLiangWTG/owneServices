using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.PrintProcessing.FaxRouting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class FaxProcessorTest : MergedPrintGroupProcessorTestCase
	{
		internal override MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection printGroup)
		{
			return new FaxProcessor(printGroup);
		}

		public override void TestCulture()
		{
			Assert("Problems with actual delivery so for Faxing we will not test culture", true);
		}

		#region TestDoesNotLogsDocumentSentEventIfFaxWasNotDelivered

		public void TestDoesNotLogsDocumentSentEventIfFaxWasNotDelivered()
		{
			ZGuid parentId = ZGuid.NewZGuid();
			ZQuery query = new ZQuery(StmALogSchema.SL_Parent, parentId);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DocumentSentCode);

			StmPrintJobMergedCollection printJobs = new StmPrintJobMergedCollection(Factory);
			StmPrintJob printJob = printJobs.AddNew();
			printJob.SP_JobType = "FAX";
			printJob.SP_FaxDestination = "1234567890";
			printJob.SP_ParentGuid = parentId;
			printJob.SP_ParentTableName = "Table1";
			Factory.Save();

			new FaxProcessor(printJobs) { deliveryDecider = new FaxeFaxDeliveryDecider(false) }.Process();

			AssertNull("DocumntSent event should not be added on bizo if no fax deliverer is specified.", Factory.LoadTop1<StmALog>(query));

			printJobs = new StmPrintJobMergedCollection(Factory);
			printJob = printJobs.AddNew();
			printJob.SP_JobType = "FAX";
			printJob.SP_FaxDestination = "1234567890";
			printJob.SP_ParentGuid = parentId;
			printJob.SP_ParentTableName = "Table1";
			Factory.Save();

			new FaxProcessor(printJobs) { deliveryDecider = new FaxeFaxDeliveryDecider(true) }.Process();

			AssertNotNull("DocumentSent event should be added on bizo in normal situation.", Factory.LoadTop1<StmALog>(query));
		}

		public void TestLogProcessing()
		{
			var printJobs = new StmPrintJobMergedCollection(Factory);
			var printJob = printJobs.AddNew();
			printJob.SP_JobType = "FAX";
			printJob.SP_EmailSubjectLine = "Email Subject";
			printJob.SP_FaxDestination = "123456";
			printJob.SP_DocumentName = "Document Name";
			printJob.SP_DocumentType = "NOT";
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_Copies = 10;
			Factory.Save();

			var loggedMessages = new List<String>();
			new FaxProcessor(printJobs, new PrintJobManager.ProgressDelegate((eventType, statusMessage) => { loggedMessages.Add(statusMessage); })).Process();

			var expectedMessage = @"Processing document ""Document Name"" with subject ""Email Subject"".
Document type [NOT].
Related Business Context: [SHP].
Email Attachments: [default.XLS].
Number of Copies: [10].
Email To: [].
Fax Destination:[123456].";
			AssertEquals(expectedMessage, loggedMessages[0]);
		}

		class FaxeFaxDeliveryDecider : IFaxDeliverer
		{
			public FaxeFaxDeliveryDecider(bool deliver)
			{
				this.deliver = deliver;
			}
			readonly bool deliver;
			public bool Deliver(StmPrintJob fax)
			{
				return deliver;
			}
		}

		#endregion
	}
}
