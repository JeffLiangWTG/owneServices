using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class FtpTest : OnlineDeliveryBaseTest
	{
		public void TestDeliverPDF()
		{
			AssertDeliver("PDF", "PDF");
		}

		public void TestDeliverCSV()
		{
			AssertDeliver("CSV", "CSV");
		}

		public void TestDeliverCS2()
		{
			AssertDeliver("CS2", "CSV");
		}

		void AssertDeliver(string attachmentType, string attachmentFormat)
		{
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Ftp;
			contact.FileLocation = "localhost";
			contact.UserName = "User 1";
			contact.Password = "password";
			contact.AttachmentType = attachmentType;
			ZGuid parentGuid = new ZGuid(new Guid());
			var ftp = new Ftp(contact, parentGuid);

			using (var stream = new MemoryStream())
			{
				stream.Write(new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0, 10);

				var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
				info.SetFileContents(stream, attachmentFormat);
				info.AttachedFilename = "123456789_123456789_123456789_123456789_XXX"; // Last part 'XXX' will be cut due to max length
				ftp.AddFile(info);

				var printJobQuery = new ZQuery(StmPrintJobSchema.SP_JobType, nameof(PrintType.FTP));
				AssertNull(Factory.LoadTop1<StmPrintJob>(printJobQuery));

				ftp.Deliver();

				var printJob = Factory.LoadTop1<StmPrintJob>(printJobQuery);
				AssertNotNull(printJob);
				AssertEquals(nameof(PrintType.FTP), printJob.SP_JobType);
				AssertEquals("123456789_123456789_123456789_123456789_", printJob.SP_DocumentName);
				AssertEquals(attachmentFormat, printJob.SP_EmailAttachmentFormat);
				AssertEquals("localhost", printJob.SP_FaxDestination);
				AssertEquals("User 1\0password", printJob.SP_EmailFromAddress);
				AssertEquals(parentGuid, printJob.SP_ParentGuid);
				AssertEquals("StmScheduleTask", printJob.SP_ParentTableName);
				AssertEquals(Ftp.ScheduledReportFtp, printJob.SP_RelatedBusinessContext);
			}
		}

		#region Implementation

		protected override DeliveryMethod GetNewDeliveryMethod(DocDeliveryContact contact)
		{
			return new Ftp(contact, new ZGuid(new Guid()));
		}

		#endregion
	}
}
