using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class EmailDocumentDeliveryJobTest : TestCaseWithFactory
	{
		public void TestEmailDocumentDeliveryJob()
		{
			var documentCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Delay Alert"));
			documentCommand.SU_DefaultAttachmentType = OrgConstants.AttachmentType.XLS;
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			Factory.Save();

			var emailDocumentDeliveryJob = new EmailDocumentDeliveryJob((IDocumentSupportable)shipment, documentCommand.PK, false, "justin@email.com", "dave@YourMother.com");

			var printJobs = LoadPrintJobs();
			AssertEquals("Precondition: no documents delivered", 0, printJobs.Length);

			emailDocumentDeliveryJob.Deliver(new NotificationBuffer());

			printJobs = LoadPrintJobs();
			AssertEquals("2 documents delivered", 2, printJobs.Length);

			AssertPrintJob(printJobs[0], "dave@YourMother.com");
			AssertPrintJob(printJobs[1], "justin@email.com");

			void AssertPrintJob(StmPrintJob printJob, string expectedRecipient)
			{
				AssertEquals("Parent", shipment.PK, printJob.SP_ParentGuid);
				AssertEquals("Email", expectedRecipient, printJob.EmailToRecipients[0].SPR_EmailAddress.ToString());
				AssertEquals("Attachment Type", "XLS", printJob.SP_EmailAttachmentFormat);
				AssertContains("Subject", "Delay Alert", printJob.SP_EmailSubjectLine);
			}

			StmPrintJob[] LoadPrintJobs()
			{
				return Factory.Load<StmPrintJob>(new ZQuery()).OrderBy(pj => pj.EmailToRecipients.Single().SPR_EmailAddress).ToArray();
			}
		}
	}
}
