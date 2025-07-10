using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class EmailTest : OnlineDeliveryBaseTest
	{
		public void TestAddress()
		{
			var contact = GetContact();
			var email = new Email(contact);
			AssertEquals(contact.Email, contact.DeliveryAddress);
			AssertEquals(contact.Email, email.Address);

			var ePrinterAddress = "email@printer.com";
			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrinterAddress);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			email = new Email(contact);
			AssertEquals(ePrinterAddress, contact.EPrintEmail);
			AssertEquals(ePrinterAddress, contact.DeliveryAddress);
			AssertEquals(ePrinterAddress, email.Address);
		}

		public void TestSetAddtionalPropertiesWithEmailCopyRecipients()
		{
			// Arrange
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			deliveryInfo.FileFormat = "ABC";
			deliveryInfo.SetFileContents(SimpleTestXls, "xls");
			var deliveryContact = GetContact();
			deliveryContact.EmailCarbonCopyRecipientsAsString = "test1@test.com";
			deliveryContact.EmailBlindCarbonCopyRecipientsAsString = "test2@test.com";
			var email = new Email(deliveryContact);
			email.AddFile(deliveryInfo);
			// Act
			email.Deliver();
			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			// Assert
			AssertEquals("StmPrintJob Count", 1, printJobs.Count);
			AssertEquals("Email Carbon Copy Recipients Count", 1, printJobs[0].CarbonCopyRecipients.Count);
			AssertEquals("Email Blind Carbon Copy Recipients Count", 1, printJobs[0].BlindCarbonCopyRecipients.Count);
		}

		public void TestEPrintSendEmailToEPrintAddressOnly()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ePrint@deliery.com");

			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			deliveryInfo.FileFormat = "ABC";
			deliveryInfo.SetFileContents(SimpleTestXls, "xls");

			var deliveryContact = GetContact();
			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			deliveryContact.Email = "contact@delivery.com";

			var email = new Email(deliveryContact);
			email.AddFile(deliveryInfo);
			email.Deliver();

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			AssertEquals("StmPrintJob Count", 1, printJobs.Count);
			AssertEquals("Should only send to ePrint address", 1, printJobs[0].EmailToRecipients.Count);
			AssertEquals("ePrint@deliery.com", printJobs[0].EmailToRecipients.Value);
		}

		public void TestDeliverDocumentByEPrintWhenEmptyRecipients()
		{
			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			deliveryInfo.SetFileContents(SimpleTestXls, "xls");
			var deliveryContact = GetContact();
			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			deliveryContact.Email = "contact@delivery.com";

			var email = new Email(deliveryContact);
			email.AddFile(deliveryInfo);
			var message = "A ePrint Email Address must be specified when using a delivery method of 'EPR'.Please go to the Registry > Documents > ePrint Email Address set configs.";
			email.Deliver();
			Assert(UnitTestUserNotification.Instance.LastMessage.Contains(message));
		}

		public void TestSetAdditionalPropertiesWithTxtFile()
		{
			var txtInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
			var txtTestFile = new StreamWriter(new MemoryStream());
			txtTestFile.Write("This is text file");
			txtInfo.SetFileContents(txtTestFile.BaseStream, "txt");

			var contact = GetContact();
			contact.AttachmentType = AttachmentTypeList.Codes.Txt_Comm;
			var instructions = new DeliveryInstructions();
			var email = new Email(contact) { Instructions = instructions };
			email.AddFile(txtInfo);

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			email.Deliver();

			var printJobsAfter = new StmPrintJobCollection(Factory);
			printJobsAfter.Load();

			// remove all the print jobs from the after collection so we're left only with the new print job
			foreach (StmPrintJob printJob in printJobs)
			{
				printJobsAfter.Remove(printJob);
			}

			AssertEquals("We should have one print job after email.deliver()", 1, printJobsAfter.Count);
			AssertEquals("Blob type should be TXT", "TXT", printJobsAfter[0].BlobType);
			AssertEquals("Email delivery type should be TXT", "TXT", printJobsAfter[0].SP_EmailAttachmentFormat);
		}

		public void TestSetAdditionalPropertiesWithTIFfile()
		{
			var tIFInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			tIFInfo.SetFileContents(TIFPage, "tif");

			var contact = GetContact();
			var instructions = new DeliveryInstructions();
			instructions.TIFAttachmentsOnly = true;
			var email = new Email(contact) { Instructions = instructions };
			email.AddFile(tIFInfo);

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			email.Deliver();

			var printJobsAfter = new StmPrintJobCollection(Factory);
			printJobsAfter.Load();

			// remove all the print jobs from the after collection so we're left only with the new print job
			foreach (StmPrintJob printJob in printJobs)
			{
				printJobsAfter.Remove(printJob);
			}

			AssertEquals("We should have one print job after email.deliver()", 1, printJobsAfter.Count);
			AssertEquals("Blob type should be TIF", "TIF", printJobsAfter[0].BlobType);
			AssertEquals("Email delivery type should be TIF even though delivery format is XLS, because TIF files can't be converted to XLS", "TIF", printJobsAfter[0].SP_EmailAttachmentFormat);
		}

		public void TestDeliverNoEmailBody()
		{
			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.SetFileContents(NewStyleTemplateXls, "xls");
			info2.SetFileContents(NewStyleTemplateXls, "xls");

			var instructions = new DeliveryInstructions();
			var contact = GetContact();
			instructions.Recipients.Add(contact);
			instructions.IncludeCoverNote = true;
			instructions.CoverNote = "";

			var email = new MockEmail(contact) { Instructions = instructions };

			email.AddFile(info1);
			email.AddFile(info2);

			var printJobCountBeforeDelivery = StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount();
			email.Deliver();
			AssertEquals("Email batch jobs should have been added", printJobCountBeforeDelivery + 1, StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount());

			var notes = email.PrintJob.Notes;
			AssertEquals("Note should exist - Notes.HasNotes", true, notes.HasNotes);
			AssertEquals("Cover note text", instructions.CoverNote, email.PrintJob.EmailFaxCoverNote);
		}

		public void TestDeliverNoCoverNote()
		{
			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.SetFileContents(NewStyleTemplateXls, "xls");
			info2.SetFileContents(NewStyleTemplateXls, "xls");

			var instructions = new DeliveryInstructions();
			var contact = GetContact();
			instructions.Recipients.Add(contact);
			instructions.IncludeCoverNote = false;

			var email = new MockEmail(contact) { Instructions = instructions };

			email.AddFile(info1);
			email.AddFile(info2);

			var printJobCountBeforeDelivery = StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount();
			email.Deliver();
			AssertEquals("Email batch jobs should have been added", printJobCountBeforeDelivery + 1, StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount());

			var notes = email.PrintJob.Notes;
			AssertEquals("No cover note should exist - Notes.HasNotes", false, notes.HasNotes);
		}

		public void TestDeliverWithEmailBody()
		{
			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.SetFileContents(NewStyleTemplateXls, "xls");
			info2.SetFileContents(NewStyleTemplateXls, "xls");

			var instructions = new DeliveryInstructions();
			var contact = GetContact();
			instructions.Recipients.Add(contact);
			instructions.IncludeCoverNote = true;
			instructions.CoverNote = "This is a test email body";

			var email = new MockEmail(contact) { Instructions = instructions };
			email.AddFile(info1);
			email.AddFile(info2);

			var printJobCountBeforeDelivery = StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount();
			email.Deliver();
			AssertEquals("Email batch jobs should have been added", printJobCountBeforeDelivery + 1, StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount());

			var notes = email.PrintJob.Notes;
			AssertEquals("Note should exist - Notes.HasNotes", true, notes.HasNotes);
			AssertEquals("Cover note text", instructions.CoverNote, email.PrintJob.EmailFaxCoverNote);
		}

		public void TestPrintJobOnDelete_ShouldDeleteCoverNote()
		{
			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.SetFileContents(NewStyleTemplateXls, "xls");
			info2.SetFileContents(NewStyleTemplateXls, "xls");

			var instructions = new DeliveryInstructions();
			var contact = GetContact();
			instructions.Recipients.Add(contact);
			instructions.IncludeCoverNote = true;
			instructions.CoverNote = "Test Cover Note";

			var email = new MockEmail(contact) { Instructions = instructions };
			email.AddFile(info1);
			email.AddFile(info2);

			var printJobCountBeforeDelivery = StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount();
			email.Deliver();
			AssertEquals("Email batch jobs should have been added", printJobCountBeforeDelivery + 1, StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount());

			email.PrintJob.Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var printJob_Reloaded = newFactory.Load<StmPrintJob>(email.PrintJob.PK);

			AssertEquals("Should has notes.", true, printJob_Reloaded.Notes.HasNotes);
			AssertEquals("Cover Note text.", "Test Cover Note", printJob_Reloaded.EmailFaxCoverNote);

			var notePK = (printJob_Reloaded.Notes.GetAllNotes().FirstOrDefault() as StmNote).PK;

			printJob_Reloaded.Delete();
			newFactory.Save();

			AssertEquals("Cover Note should have been deleted. No records should be loaded.", null, newFactory.Load<StmNote>(notePK));
		}

		public void TestNoneOrphanedCoverNotesWithCVRTypeShouldAlsoBeLoadedThenDeleted()
		{
			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.SetFileContents(NewStyleTemplateXls, "xls");
			info2.SetFileContents(NewStyleTemplateXls, "xls");

			var instructions = new DeliveryInstructions();
			var contact = GetContact();
			instructions.Recipients.Add(contact);
			instructions.IncludeCoverNote = true;
			instructions.CoverNote = "Test Cover Note";

			var email = new MockEmail(contact) { Instructions = instructions };
			email.AddFile(info1);
			email.AddFile(info2);

			var printJobCountBeforeDelivery = StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount();
			email.Deliver();
			AssertEquals("Email batch jobs should have been added", printJobCountBeforeDelivery + 1, StmPrintJobTestUtils.GetStmPrintJobQueuedBatchCount());

			var note = email.PrintJob.Notes.GetAllNotes().First() as StmNote;
			note.ST_NoteType = "CVR";
			var notePK = note.PK;
			email.PrintJob.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var printJob_Reloaded = newFactory.Load<StmPrintJob>(email.PrintJob.PK);

			AssertEquals("CVR StmNote should be loaded", "Test Cover Note", printJob_Reloaded.EmailFaxCoverNote);

			printJob_Reloaded.Delete();
			newFactory.Save();

			AssertEquals("CVR StmNote should have been deleted.", null, newFactory.Load<StmNote>(notePK));
		}

		public void TestEmailWithNoRecipientsThrowsException()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var contact = new DocDeliveryContact(Factory);
			contact.Name = "Test";
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			deliveryInfo.FileFormat = "ABC";
			deliveryInfo.EmailSubjectLine = "Test Email";

			var email = new Email(contact);
			email.AddFile(deliveryInfo);
			email.Deliver();

			AssertMultilineASCIIEquals("The error should have been reported.", @"An Email was created without any recipients.
Email Subject:Test Email
Parent Table:
Parent PK:00000000-0000-0000-0000-000000000000
Delivery Method:Email
Contact Name:Test
OrgHeader:
IsSystemDefaultContact:False
Document:
Path:
DocumentGroup:
UpdateEmailAndFax:True
Contact PK:
Contact Email:
OrgAddress PK:
OrgAddress Email:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestConsolidateReports()
		{
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.SendIndividually = false;

			var email = new MockEmail(contact);
			AssertEquals("ConsolidateReports", true, email.ConsolidateReports_Exposed);

			contact.SendIndividually = true;
			AssertEquals("ConsolidateReports", false, email.ConsolidateReports_Exposed);
		}

		protected override DeliveryMethod GetNewDeliveryMethod(DocDeliveryContact contact) => new Email(contact);

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		Stream NewStyleTemplateXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls");

		Stream SimpleTestXls => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest.xls");

		Stream TIFPage => resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TIFPage.tif");

		#region Test Objects

		internal class MockEmail : Email
		{
			public MockEmail(DocDeliveryContact contact)
				: base(contact)
			{
			}

			public StmPrintJob PrintJob
			{
				get { return fPrintJob; }
			}
			StmPrintJob fPrintJob;

			protected override void SetAdditionalProperties(StmPrintJob printJob, DeliveryInfo deliveryInfo)
			{
				base.SetAdditionalProperties(printJob, deliveryInfo);
				fPrintJob = printJob; // lame.
			}

			public bool ConsolidateReports_Exposed => base.ConsolidateReports;
		}

		internal static class StmPrintJobTestUtils
		{
			public static int GetStmPrintJobQueuedBatchCount()
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ZQuery sQLFilter = new ZQuery(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.LessThan, ZDateTime.UtcNow.AddMinutes(2));
				BusinessObject[] stmPrintJobs = factory.Load(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), sQLFilter);

				return stmPrintJobs.Length;
			}
		}

		#endregion

	}
}
