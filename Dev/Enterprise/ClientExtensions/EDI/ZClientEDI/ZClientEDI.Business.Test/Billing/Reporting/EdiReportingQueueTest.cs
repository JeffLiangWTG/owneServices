using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiReportingQueue))]
	public class EdiReportingQueueTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGenerateReport()
		{
			Env.Registry.SMTPDefaultDoNotReplyEmailAddress = "DoNotReply@cw1.com";
			var contact = Factory.LoadTop1<OrgContact>(new ZQuery());
			contact.OC_Email = "contact@cw1.com";

			var queue = Factory.New<EdiReportingQueue>();
			queue.ERQ_ReportName = "Report123";
			queue.ERQ_OC = contact.PK;

			Factory.Save();

			using (var tempDir = new TempDirectory())
			{
				AssertEquals("NEW", queue.ERQ_Status);

				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				queue.GenerateReport();
				Factory.Save();

				AssertEquals("PCD", queue.ERQ_Status);

				AssertEquals(Path.Combine(tempDir.DirectoryName, queue.PK.ToString() + ".zip"), queue.ERQ_ReportFileFullName);
				AssertEquals(true, File.Exists(queue.ERQ_ReportFileFullName));

				var mailItems = new StandardMailItemCollection(Factory);
				mailItems.Load();

				AssertEquals("One email should be sent.", 1, mailItems.Count);
				AssertEquals("Email.MI_Status", MailManager.MailStatus.Queued, mailItems[0].MI_Status);
				AssertEquals("Email.MI_Subject", "CargoWise My Account - Report Download Notification", mailItems[0].MI_Subject);
				AssertEquals("Email.MI_Body", $"The Report is ready for download.<br/>Download Link:<a href='https://myaccount-portal.cargowise.com/myaccount/Download.aspx?report={queue.PK.ToString()}'>{queue.ERQ_ReportName}</a>", mailItems[0].MI_Body);

				AssertEquals("Email should have 1 recipients.", 1, mailItems[0].MailRecipients.Count);
				AssertEquals("First Recipient Address", "contact@cw1.com", mailItems[0].MailRecipients[0].EmailAddress);

				AssertEquals("PleaseDoNotReply <DoNotReply@cw1.com>", mailItems[0].MI_From);

				var fullFileName = queue.ERQ_ReportFileFullName;
				queue.Delete();
				Factory.Save();
				AssertEquals(false, File.Exists(fullFileName));
			}
		}
	}
}
