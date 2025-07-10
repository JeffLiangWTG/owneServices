using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class EmailProcessorTest : MergedPrintGroupProcessorTestCase
	{
		const int Megabyte = 1024 * 1024;

		public void TestProcess()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			string fileFullpath1 = CreateSampleFile("test1.txt", "Sample text 1");
			string fileFullpath2 = CreateSampleFile("test2.txt", "Sample text 2");

			try
			{
				var deliveryGroup = Factory.New<StmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;
				deliveryGroup.SB_EmailSubjectLine = "blah";

				ZGuid parentID = ZGuid.NewZGuid();

				var collection = new StmPrintJobMergedCollection(Factory);
				var samplePrintJob1 = GetSampleJob(fileFullpath1, deliveryGroup.PK);
				samplePrintJob1.SP_ParentGuid = parentID;
				samplePrintJob1.SP_ParentTableName = ZArchitecture.Schema.JobShipmentSchema.Constants.TableName;
				var samplePrintJob1CopyRecipient = samplePrintJob1.CarbonCopyRecipients.AddNew();
				samplePrintJob1CopyRecipient.SPR_RecipientType = Core.Constants.CopyRecipientType.CarbonCopyRecipient;
				samplePrintJob1CopyRecipient.SPR_EmailAddress = "copy@edi.com.au";
				samplePrintJob1.CarbonCopyRecipients.Add(samplePrintJob1CopyRecipient);
				collection.Add(samplePrintJob1);
				var samplePrintJob2 = GetSampleJob(fileFullpath2, deliveryGroup.PK);
				var samplePrintJob2CopyRecipient = samplePrintJob2.CarbonCopyRecipients.AddNew();
				samplePrintJob2CopyRecipient.SPR_RecipientType = Core.Constants.CopyRecipientType.CarbonCopyRecipient;
				samplePrintJob2CopyRecipient.SPR_EmailAddress = "copy@edi.com.au";
				samplePrintJob2.CarbonCopyRecipients.Add(samplePrintJob2CopyRecipient);
				collection.Add(samplePrintJob2);

				var processor = new EmailProcessorForTesting(collection);
				processor.Process();

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.FromAddress", "test@edi.com.au", email.FromAddress);
				Assert("Email.CCRecipients", email.CCRecipients.ToStringCollection().Contains("copy@edi.com.au"));
				AssertEquals("PrintJob.SP_EmailSignature", true, email.Body.Contains("Company Name"));
				AssertEquals("blah", email.Subject);
				AssertEquals(2, email.Attachments.Count);

				AssertEquals(parentID, email.BusinessEntityID);
				AssertEquals(ZArchitecture.Schema.JobShipmentSchema.Constants.Prefix, email.BusinessEntityTableCode);

				var sortedAttachments = email.Attachments.Cast<AttachmentDef>().OrderBy(a => a.DisplayName).ToList();
				AssertEquals("test1.txt", sortedAttachments[0].DisplayName);
				AssertEquals("Sample text 1", System.Text.Encoding.UTF8.GetString(sortedAttachments[0].Data));
				AssertEquals("test2.txt", sortedAttachments[1].DisplayName);
				AssertEquals("Sample text 2", System.Text.Encoding.UTF8.GetString(sortedAttachments[1].Data));
			}
			finally
			{
				DeleteIfExists(fileFullpath1);
				DeleteIfExists(fileFullpath2);
			}
		}

		[TestDate(2015, 01, 09, 10, 11, 12, 123)]
		public void TestProcess_WithZippedDocPack()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			string fileFullpath1 = CreateSampleFile("test1.txt", "Sample text 1");
			string fileFullpath2 = CreateSampleFile("test2.txt", "Sample text 2");

			try
			{
				var deliveryGroup = Factory.New<StmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;
				deliveryGroup.SB_IsZippedDocPack = true;
				deliveryGroup.SB_EmailSubjectLine = "blah";

				var collection = new StmPrintJobMergedCollection(Factory);
				collection.Add(GetSampleJob(fileFullpath1, deliveryGroup.PK));
				collection.Add(GetSampleJob(fileFullpath2, deliveryGroup.PK));

				var processor = new EmailProcessorForTesting(collection);
				processor.Process();

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.FromAddress", "test@edi.com.au", email.FromAddress);
				AssertEquals("PrintJob.SP_EmailSignature", true, email.Body.Contains("Company Name"));
				AssertEquals("blah", email.Subject);
				AssertEquals(1, email.Attachments.Count);

				AssertEquals("DocPack_20150109-101112-123.zip", email.Attachments[0].DisplayName);

				var zipExtractor = new ZipExtractor();
				string[] zippedFileNames = zipExtractor.GetFileNames(new MemoryStream(email.Attachments[0].Data));
				AssertContainsExactElementsInAnyOrder(new string[] { "test1.txt", "test2.txt" }, zippedFileNames);

				using (var stream = new MemoryStream())
				{
					zipExtractor.ExtractZipStream(new MemoryStream(email.Attachments[0].Data), stream, "test1.txt");
					AssertEquals("Sample text 1", Encoding.UTF8.GetString(stream.ToArray()));

					stream.SetLength(0);
					zipExtractor.ExtractZipStream(new MemoryStream(email.Attachments[0].Data), stream, "test2.txt");
					AssertEquals("Sample text 2", Encoding.UTF8.GetString(stream.ToArray()));
				}
			}
			finally
			{
				DeleteIfExists(fileFullpath1);
				DeleteIfExists(fileFullpath2);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2024, 05, 09, 10, 11, 12, 123)]
		public void TestProcess_WithZippedDocPack_IsNotEmpty_WhenSingleAttachment()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			deliveryGroup.SB_IsZippedDocPack = true;
			deliveryGroup.SB_EmailSubjectLine = "Test Subject";

			var job1 = GetSampleJob(
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName,
				deliveryGroup.PK);
			job1.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var job2 = GetSampleJob(
				PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFMultiPageFile,
				deliveryGroup.PK);
			job2.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var collection = new StmPrintJobMergedCollection(Factory) { job1, job2 };
			var processor = new EmailProcessorForTesting(collection);
			processor.Process();

			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, email.Attachments.Count);
			AssertEquals("DocPack_20240509-101112-123.zip", email.Attachments[0].DisplayName);
			AssertEquals("Test Subject", email.Subject);
			AssertEquals("Since there was no invalid PDF, there should be no log for it", 2, loggedMessages.Count);
			
			var zipExtractor = new ZipExtractor();
			var zippedFileNames = zipExtractor.GetFileNames(new MemoryStream(email.Attachments[0].Data));
			AssertContainsExactElementsInAnyOrder(new[] { "Subject Line - Merged.PDF" }, zippedFileNames);

			using (var stream = new MemoryStream())
			{
				zipExtractor.ExtractZipStream(new MemoryStream(email.Attachments[0].Data), stream, "Subject Line - Merged.PDF");
				using (var expectedPdfData = File.OpenRead(PrintProcessingConstants.TestMergedPDFFileFullPath))
				{
					AssertMultilineASCIIEquals("Output PDF should be the same.",
						ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData),
						ImageToPDFConverterTest.GetStringForPDFComparison(stream));
				}
			}
		}

		public void TestProcessWithAllowEmailsToBeSentFromUsersAddressFalse()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			string fileName = Path.Combine(Temp.TempPath, "test.txt");
			using (Stream file = File.OpenWrite(fileName))
			{
				byte[] info = new UTF8Encoding(true).GetBytes("Sample text");
				file.Write(info, 0, info.Length);
				file.Close();
			}
			try
			{
				StmDeliveryGroup deliveryGroup = Factory.New<StmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;
				deliveryGroup.SB_EmailSubjectLine = "blah";

				Env.OutgoingMailManager.EmailsCreated.Clear();
				SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				StmPrintJobMergedCollection collection = new StmPrintJobMergedCollection(Factory);
				StmPrintJob printJob = collection.AddNew();
				printJob.StoredAttachmentSizeKB = 80;
				printJob.SP_JobType = "EML";
				printJob.SP_EmailFromAddress = "test@edi.com.au";
				printJob.SP_Destination = "unit.test@cargowise.com";
				printJob.SP_EmailSignature = "Company Name";
				printJob.SP_EmailSubjectLine = "Subject Line";
				printJob.StoredAttachmentFilename = fileName;

				printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
				EmailProcessor processor = new EmailProcessorForTesting(collection);
				processor.Process();

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertNotEquals("Email.FromAddress", "test@edi.com.au", email.FromAddress);
				AssertEquals("PrintJob.SP_EmailSignature", true, email.Body.Contains("Company Name"));
				AssertEquals("blah", email.Subject);
			}
			finally
			{
				DeleteIfExists(fileName);
			}
		}

		public void TestUseMatchedSecondarySenderAddressIfNotAllowedEmailsToBeSentFromUsersAddress()
		{
			var secondarySMTPServerCollection = new SecondarySMTPServerCollection();
			var server = secondarySMTPServerCollection.AddNew();
			server.SMTPServer = "mail.server.com";
			server.SMTPPort = 587;
			server.SMTPSecureConnection = ZArchitecture.Core.SecureConnectionTypes.SSL;
			server.SMTPUsername = "user@server.com";
			server.SMTPPassword = "password";
			server.AllowEmailsToBeSentFromUsersAddress = false;
			server.SMTPSenderAddress = "sender@server.com";
			server.SupportedDomains = "edi.com.au, edi.com.cn";

			using (PhysicalServerDataRegistry.Instance.SecondarySMTPServers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secondarySMTPServerCollection))
			{
				StmDeliveryGroup deliveryGroup = Factory.New<StmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;
				deliveryGroup.SB_EmailSubjectLine = "blah";

				Env.OutgoingMailManager.EmailsCreated.Clear();
				SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				StmPrintJobMergedCollection collection = new StmPrintJobMergedCollection(Factory);
				StmPrintJob printJob = collection.AddNew();
				printJob.StoredAttachmentSizeKB = 80;
				printJob.SP_JobType = "EML";
				printJob.SP_EmailFromAddress = "test@edi.com.au";
				printJob.SP_Destination = "unit.test@cargowise.com";
				printJob.SP_EmailSignature = "Company Name";
				printJob.SP_EmailSubjectLine = "Subject Line";

				printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
				EmailProcessor processor = new EmailProcessorForTesting(collection);
				processor.Process();

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.FromAddress", "sender@server.com", email.FromAddress);
			}
		}

		public void TestAttachmentSizeLimit()
		{
			SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			StmPrintJobMergedCollection collection = new StmPrintJobMergedCollection(Factory);
			for (int i = 0; i < 15; i++)
			{
				StmPrintJob printJob = collection.AddNew();
				printJob.StoredAttachmentSizeKB = 80;
			}

			EmailProcessor processor = new EmailProcessorForTesting(collection);
			StmPrintJobGroupCollection groupedCollection = processor.SplitGroupIntoMultipleEmails();
			AssertEquals("Each print job is approx 80K, so 2 print groups should be made", 2, groupedCollection.Count);
			Assert("Each merged print group should be under 1M", groupedCollection[0].SizeInKB <= 1024);
			Assert("Each merged print group should be under 1M", groupedCollection[1].SizeInKB <= 1024);

			foreach (StmPrintJob job in collection)
			{
				Assert("Job should be in one of the groups", groupedCollection[0].Contains(job) ^ groupedCollection[1].Contains(job));
			}
		}

		public void TestAttachmentSizeLimit_DoesNotOverflow()
		{
			//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
			{
				var collection = new StmPrintJobMergedCollection(Factory);
				var printJob1 = collection.AddNew();
				printJob1.StoredAttachmentSizeKB = 80;
				var printJob2 = collection.AddNew();
				printJob2.StoredAttachmentSizeKB = 80;

				var processor = new EmailProcessorForTesting(collection);
				var groupedCollection = processor.SplitGroupIntoMultipleEmails();
				AssertEquals("Each print job is approx 80K, so 1 print group should be made", 1, groupedCollection.Count);
			}
		}

		public void TestPrintJobOrder()
		{
			int numAttachments = 4;
			try
			{
				var groupPK = Factory.New<StmDeliveryGroup>().PK;
				StmPrintJobMergedCollection collection = new StmPrintJobMergedCollection(Factory);
				for (int i = 0; i < numAttachments; i++)
				{
					int seq = numAttachments - 1 - i;
					StmPrintJob printJob = collection.AddNew();
					printJob.SP_Sequence = seq;
					printJob.SP_EmailSubjectLine = "Subject " + seq;
					string fileName = Path.Combine(Temp.TempPath, "File " + seq);
					File.WriteAllText(fileName, seq.ToString(), Encoding.ASCII);
					printJob.StoredAttachmentFilename = fileName;
					printJob.SP_JobType = "EML";
					printJob.SP_EmailFromAddress = "test@edi.com.au";
					printJob.SP_Destination = "unit.test@cargowise.com";
					printJob.SP_SB_DeliveryGroup = groupPK;
				}

				Env.OutgoingMailManager.EmailsCreated.Clear();
				new EmailProcessorForTesting(collection).Process();
				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Subject 0", email.Subject);
				AssertEquals("Number of attachments", numAttachments, email.Attachments.Count);
				for (int i = 0; i < numAttachments; i++)
				{
					AssertEquals("File " + i, email.Attachments[i].DisplayName);
					AssertEquals(Encoding.ASCII.GetBytes(i.ToString()), email.Attachments[i].Data);
				}
			}
			finally
			{
				for (int i = 0; i < numAttachments; i++)
				{
					string fileName = Path.Combine(Temp.TempPath, "File " + i);
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
				}
			}
		}

		public void TestProcessJob_EmailWithHtmlContent()
		{
			var groupPK = Factory.New<StmDeliveryGroup>().PK;
			var collection = new StmPrintJobMergedCollection(Factory);
			var printJob = collection.AddNew();
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_JobType = "EML";
			printJob.SP_EmailFromAddress = "test@edi.com.au";
			printJob.SP_Destination = "unit.test@cargowise.com";
			printJob.SP_SB_DeliveryGroup = groupPK;
			printJob.SP_EmailSignature = "TestSignature";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			new EmailProcessorForTesting(collection).Process();
			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(EmailContentTypes.HTML, email.ContentType);
			var body = "<body style='font-family:Calibri,sans-serif; font-size:16px'><br /><br />TestSignature</body>";
			AssertEquals(body, email.Body);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailHTMLAttachment()
		{
			var beforeConversion = Path.Combine(UnitTestingConstants.TestFilesDir, "test.xls");
			var group = Factory.New<StmDeliveryGroup>();
			var groupPK = group.PK;
			group.SB_IsProcessed = true;
			var testParentGuid = ZGuid.NewZGuid();
			var collection = new StmPrintJobMergedCollection(Factory);

			var printJob = collection.AddNew();
			printJob.SP_Sequence = 0;
			printJob.SP_SB_DeliveryGroup = groupPK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(beforeConversion);
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				new EmailProcessorForTesting(collection).Process();

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email subject", email.Subject);
				var body = email.Body.Trim();
				Assert(body.StartsWith("<!DOCTYPE HTML"));
				Assert(body.EndsWith(@"</html>"));
				AssertContains("<html>", body);
				AssertContains("<meta name=\"Generator\" content=\"FlexCel", body);
				AssertEquals("Number of attachments", 8, email.Attachments.Count);
				for (int i = 0; i < 8; i++)
				{
					var fileName = email.Attachments[i].DisplayName;
					AssertContains("v:imagedata src=\"cid:" + fileName + "\" ", body);
					AssertContains("<img src='cid:" + fileName + "' ", body);
				}
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailHTMLAttachment_NoImages()
		{
			var beforeConversion = Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, "test.xls");
			var group = Factory.New<StmDeliveryGroup>();
			var groupPK = group.PK;
			group.SB_IsProcessed = true;
			var testParentGuid = ZGuid.NewZGuid();
			StmPrintJobMergedCollection collection = new StmPrintJobMergedCollection(Factory);

			var printJob = collection.AddNew();
			printJob.SP_Sequence = 0;
			printJob.SP_SB_DeliveryGroup = groupPK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(beforeConversion);
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				new EmailProcessorForTesting(collection).Process();

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email subject", email.Subject);
				var body = email.Body.Trim();
				Assert(body.StartsWith("<!DOCTYPE HTML"));
				Assert(body.EndsWith(@"</html>"));
				AssertContains("<html>", body);
				AssertContains("<meta name=\"Generator\" content=\"FlexCel", body);
				AssertEquals("Number of attachments", 0, email.Attachments.Count);
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		public void TestEmailHTMLAttachment_NotFlexCel()
		{
			byte[] htmlBytes = Encoding.UTF8.GetBytes("<html><body>Even though this mentions FlexCel, it's still not a FlexCel XLS->HTML conversion.</body></html>");
			var group = Factory.New<StmDeliveryGroup>();
			var groupPK = group.PK;
			group.SB_IsProcessed = true;
			var testParentGuid = ZGuid.NewZGuid();
			StmPrintJobMergedCollection collection = new StmPrintJobMergedCollection(Factory);

			var printJob = collection.AddNew();
			printJob.SP_Sequence = 0;
			printJob.SP_SB_DeliveryGroup = groupPK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_Destination = "test@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = htmlBytes;
			printJob.SP_EmailAttachments = "test.html";
			printJob.SP_EmailSubjectLine = "email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				new EmailProcessorForTesting(collection).Process();

				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email subject", email.Subject);
				var body = email.Body.Trim();
				Assert(!body.StartsWith("<!DOCTYPE HTML"));
				Assert(!body.EndsWith(@"</html>"));
				AssertNotContains("<html>", body);
				AssertNotContains("<meta name=\"Generator\" content=\"FlexCel", body);
				AssertNotContains("Please see the attached documents.", body);
				AssertEquals("Number of attachments", 1, email.Attachments.Count);
				AssertEquals("test.html", email.Attachments[0].DisplayName);
				AssertEquals(htmlBytes, email.Attachments[0].Data);
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailAttachmentWithPdfcFormat()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			deliveryGroup.SB_EmailSubjectLine = "blah";

			var job1 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName, deliveryGroup.PK);
			job1.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var job2 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFMultiPageFile, deliveryGroup.PK);
			job2.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var collection = new StmPrintJobMergedCollection(Factory) { job1, job2 };
			var processor = new EmailProcessorForTesting(collection);
			processor.Process();

			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals(1, email.Attachments.Count);
			AssertEquals("Subject Line - Merged.PDF", email.Attachments[0].DisplayName);
			AssertEquals("Since there was no invalid PDF, there should be no extra log for it", 2, loggedMessages.Count);
			using (var expectedPdfData = File.OpenRead(PrintProcessingConstants.TestMergedPDFFileFullPath))
			{
				AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expectedPdfData), ImageToPDFConverterTest.GetStringForPDFComparison(email.Attachments[0].Data));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailAttachmentWithPdfcFormatInvalidFiles()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			deliveryGroup.SB_EmailSubjectLine = "blah";

			var job1 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF, deliveryGroup.PK);
			job1.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var job2 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF, deliveryGroup.PK);
			job2.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var collection = new StmPrintJobMergedCollection(Factory) { job1, job2 };
			var processor = new EmailProcessorForTesting(collection);
			processor.Process();

			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Original files were not attached", 2, email.Attachments.Count);
			AssertEquals(true, loggedMessages.Contains(@"The PDF file(s) mentioned below are invalid and therefore have been separately attached.
CORRUPTED_File.PDF
CORRUPTED_File.PDF"));
			AssertContains(@"Some files could not be merged and have been attached separately.", email.Body);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailAttachmentWithPdfcFormat_InvalidPDF()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			deliveryGroup.SB_EmailSubjectLine = "blah";

			var job1 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName, deliveryGroup.PK);
			job1.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var job2 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFMultiPageFile, deliveryGroup.PK);
			job2.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var job3 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestCorruptedPDF, deliveryGroup.PK);
			job3.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var collection = new StmPrintJobMergedCollection(Factory) { job1, job2, job3 };
			var processor = new EmailProcessorForTesting(collection);
			processor.Process();

			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Subject Line - Merged.PDF", email.Attachments[0].DisplayName);
			AssertEquals("The email shoudl contained two separeted attachement", 2, email.Attachments.Count);
			AssertEquals(true, loggedMessages.Contains(@"The PDF file(s) mentioned below are invalid and therefore have been separately attached.
CORRUPTED_File.PDF"));
			AssertContains(@"Some files could not be merged and have been attached separately.", email.Body);
		}

		public override void TestCulture()
		{
			Assert("Crashing because deliveryGroup not set and TestProcess() already takes over that test", true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmailWithNoRecipients()
		{
			var collection = new StmPrintJobMergedCollection(Factory);
			var group = Factory.New<StmDeliveryGroup>();
			group.SB_IsProcessed = true;

			var printJob = collection.AddNew();
			printJob.SP_Sequence = 0;
			printJob.SP_SB_DeliveryGroup = group.PK;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestFilesDir, "test.xls"));
			printJob.SP_EmailAttachments = "test.xls";
			printJob.SP_EmailSubjectLine = "Email subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = ZGuid.NewZGuid();
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SQ = ZGuid.Empty;

			Factory.Save();

			try
			{
				printJob.SaveAttachmentToFilesystem(0);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				new EmailProcessorForTesting(collection).Process();

				AssertMultilineASCIIEquals("The error should have been reported.", $@"An Email was created without any recipients.
PrintJob Pk: {printJob.PK}
Email Subject: {printJob.SP_EmailSubjectLine}
User Name: {printJob.SP_UserFullName}
Document Name: {printJob.SP_DocumentName}
Parent Table: {printJob.SP_ParentTableName}
Parent PK: {printJob.SP_ParentGuid}
Merged print jobs:
Job Pk: {printJob.PK}, Type: {printJob.SP_JobType}
Recipients: No recipient.", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
			finally
			{
				printJob.DeleteStoredAttachment();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAllocateToEDocs()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var emailAttachmentSizeLimitInMB = 1;
			var fileFullpath1 = CreateSampleFileWithLength("test1.txt", 1 * Megabyte);
			var fileFullpath2 = CreateSampleFileWithLength("test2.txt", 3 * Megabyte);
			var fileFullpath3 = CreateSampleFileWithLength("test3.txt", (int)(0.1 * Megabyte));
			var fileFullpath4 = CreateSampleFileWithLength("test4.txt", 4 * Megabyte);
			var glowPortalsUri = "https://localhost/Glow/";

			try
			{
				using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUri))
				{
					AssertNoExceptionThrown(() =>
					{
						var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
						deliveryGroup.SB_IsProcessed = true;
						deliveryGroup.SB_EmailSubjectLine = "Test Report Run Subject";

						var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
						var collection = new StmPrintJobMergedCollection(Factory);
						var printJob1 = GetSampleJob(fileFullpath1, deliveryGroup.PK);
						var printJob2 = GetSampleJob(fileFullpath2, deliveryGroup.PK);
						var printJob3 = GetSampleJob(fileFullpath3, deliveryGroup.PK);
						var printJob4 = GetSampleJob(fileFullpath4, deliveryGroup.PK);
						AddReportRunInfoForPrintJob(printJob1, stmReportRun);
						AddReportRunInfoForPrintJob(printJob2, stmReportRun);
						AddReportRunInfoForPrintJob(printJob3, stmReportRun);
						AddReportRunInfoForPrintJob(printJob4, stmReportRun);
						printJob1.SP_EmailAttachments = "test.xls";
						printJob1.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Xlsx;
						printJob1.SP_EmailSubjectLine = $"test({AttachmentTypeList.Codes.Xlsx})";
						printJob2.SP_EmailAttachments = "test.xls";
						printJob2.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
						printJob2.SP_EmailSubjectLine = $"test({AttachmentTypeList.Codes.Pdf})";
						printJob3.SP_EmailAttachments = "test.xls";
						printJob3.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Csv;
						printJob3.SP_EmailSubjectLine = $"test({AttachmentTypeList.Codes.Csv})";
						printJob4.SP_EmailAttachments = "test.xls";
						printJob4.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Tif;
						printJob4.SP_EmailSubjectLine = $"test({AttachmentTypeList.Codes.Tif})";
						collection.Add(printJob1);
						collection.Add(printJob2);
						collection.Add(printJob3);
						collection.Add(printJob4);
						Factory.Save();

						var processor = new EmailProcessorForTesting(collection);
						processor.Process();

						var expectedLoggedMessages1 = $@"Allocating document ""Test Report Run"" of type ""SREP"" with subject ""test(PDF)"" to eDocs for (RTS, {stmReportRun.PK})
Finished allocating document ""Test Report Run"" of type ""SREP""";
						var expectedLoggedMessages2 = $@"Allocating document ""Test Report Run"" of type ""SREP"" with subject ""test(TIF)"" to eDocs for (RTS, {stmReportRun.PK})
Finished allocating document ""Test Report Run"" of type ""SREP""";
						var expectedMessages = new[] { expectedLoggedMessages1, expectedLoggedMessages2 };

						var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
						var storageMains = documentFactory.Load<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, stmReportRun.PK));
						var newPrintJob1 = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.PK, printJob1.PK));
						var newPrintJob2 = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.PK, printJob2.PK));
						var newPrintJob3 = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.PK, printJob3.PK));
						var newPrintJob4 = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.PK, printJob4.PK));

						Assert("PrintJob1's SP_EDocsProcessed should true", newPrintJob1.SP_EDocsProcessed);
						Assert("PrintJob2's SP_EDocsProcessed should true", newPrintJob2.SP_EDocsProcessed);
						Assert("PrintJob3's SP_EDocsProcessed should true", newPrintJob3.SP_EDocsProcessed);
						Assert("PrintJob4's SP_EDocsProcessed should true", newPrintJob4.SP_EDocsProcessed);

						AssertEquals("Logged messages", true, expectedMessages.All(string.Join(System.Environment.NewLine, loggedMessages).Contains));
						AssertEquals("Splited to 4 emails because all attachment size is over the limit", 4, Env.OutgoingMailManager.EmailsCreated.Count);
						var email1 = Env.OutgoingMailManager.EmailsCreated[0];
						var email2 = Env.OutgoingMailManager.EmailsCreated[1];
						var email3 = Env.OutgoingMailManager.EmailsCreated[2];
						var email4 = Env.OutgoingMailManager.EmailsCreated[3];
						var emailBodys = new string[] { email1.Body, email2.Body, email3.Body, email4.Body };
						AssertEquals("Email1 attachments should be 1", 1, email1.Attachments.Count);
						AssertEquals("Email2 attachments should be empty because download link is created", 0, email2.Attachments.Count);
						AssertEquals("Email3 attachments should be 1", 1, email3.Attachments.Count);
						AssertEquals("Email4 attachments should be empty because download link is created", 0, email4.Attachments.Count);
						AssertEquals("Only one StorageMain should be created", 1, storageMains.Length);
						AssertEquals("Related eDocs of the report run should be 2", 2, storageMains[0].AllEDocs.Count);
						var eDocs1 = storageMains[0].AllEDocs[0];
						var eDocs2 = storageMains[0].AllEDocs[1];
						AssertEquals("eDocs1 should be published so it can be accessed by Staff and Contact", true, eDocs1.IsPublished);
						AssertEquals("eDocs2 should be published so it can be accessed by Staff and Contact", true, eDocs2.IsPublished);
						AssertCollectionContains("One of the email has", $"<body style='font-family:Calibri,sans-serif; font-size:16px'><br />The report exceeded the maximum attachment size allowed and could not be emailed. Please follow the applicable link to download the report: <br /><br />If you are an internal staff member, download the report via this link and authenticate with your {Core.Constants.ProductName} login credentials: <div><a href='{glowPortalsUri}goto/EDS?StmReportRunId={stmReportRun.PK}&EDocId={eDocs1.UniqueKey}'>test.pdf</a></div><br />If you are a web portal user, to download the report, click this link and authenticate with your web portal login credentials: <div><a href='{glowPortalsUri}goto/EDC?StmReportRunId={stmReportRun.PK}&EDocId={eDocs1.UniqueKey}'>test.pdf</a></div><br /><br /><br />Company Name</body>", emailBodys);
						AssertCollectionContains("One of the email has", $"<body style='font-family:Calibri,sans-serif; font-size:16px'><br />The report exceeded the maximum attachment size allowed and could not be emailed. Please follow the applicable link to download the report: <br /><br />If you are an internal staff member, download the report via this link and authenticate with your {Core.Constants.ProductName} login credentials: <div><a href='{glowPortalsUri}goto/EDS?StmReportRunId={stmReportRun.PK}&EDocId={eDocs2.UniqueKey}'>test.tif</a></div><br />If you are a web portal user, to download the report, click this link and authenticate with your web portal login credentials: <div><a href='{glowPortalsUri}goto/EDC?StmReportRunId={stmReportRun.PK}&EDocId={eDocs2.UniqueKey}'>test.tif</a></div><br /><br /><br />Company Name</body>", emailBodys);
						AssertCollectionContains("One of the email has", "<body style='font-family:Calibri,sans-serif; font-size:16px'><br />Please see the attached documents.<br /><br /><br />Company Name</body>", emailBodys);
					});
				}
			}
			finally
			{
				DeleteIfExists(fileFullpath1);
				DeleteIfExists(fileFullpath2);
				DeleteIfExists(fileFullpath3);
				DeleteIfExists(fileFullpath4);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAllocateToEDocs_ThrowException_WhenGlowIsNotConfigured()
		{
			var emailAttachmentSizeLimitInMB = 1;
			var fileFullpath = CreateSampleFileWithLength("test1.txt", 2 * Megabyte);

			try
			{
				using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				{
					AssertExceptionThrown<HostedServiceException>($"{GlowRegistry.Instance.GlowPortalsUri.Location()} must be configured before emailing the download link.", () =>
					{
						using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
						{
							var deliveryGroup = Factory.New<StmDeliveryGroup>();
							deliveryGroup.SB_IsProcessed = true;
							deliveryGroup.SB_EmailSubjectLine = "Test Report Run Subject";

							var collection = new StmPrintJobMergedCollection(Factory);
							var printjob = GetSampleJob(fileFullpath, deliveryGroup.PK);
							AddReportRunInfoForPrintJob(printjob);
							collection.Add(printjob);

							var processor = new EmailProcessorForTesting(collection);
							processor.Process();
						}
					});
				}
			}
			finally
			{
				DeleteIfExists(fileFullpath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAllocateToEDocs_ThrowException_WhenDocIsNotAllocated()
		{
			var emailAttachmentSizeLimitInMB = 1;
			var fileFullpath = CreateSampleFileWithLength("test1.txt", 2 * Megabyte);
			var glowPortalsUri = "https://localhost/Glow/";

			try
			{
				using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				{
					AssertExceptionThrown<HostedServiceException>(@"Document ""Test Report Run"" of type ""ZZZ"" was not allocated to eDocs", () =>
					{
						using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUri))
						{
							var docType = Factory.New<RefDocType>();
							docType.RT_LogSystemCreatedDocsToEDocs = false;
							docType.RT_DocType = "ZZZ";
							docType.RT_Desc = "This is a test doctype";
							docType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;

							var deliveryGroup = Factory.New<StmDeliveryGroup>();
							deliveryGroup.SB_IsProcessed = true;
							deliveryGroup.SB_EmailSubjectLine = "Test Report Run Subject";

							var collection = new StmPrintJobMergedCollection(Factory);
							var printJob = GetSampleJob(fileFullpath, deliveryGroup.PK);
							AddReportRunInfoForPrintJob(printJob);
							printJob.SP_DocumentType = "ZZZ";
							collection.Add(printJob);

							var processor = new EmailProcessorForTesting(collection);
							processor.Process();
						}
					});
				}
			}
			finally
			{
				DeleteIfExists(fileFullpath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAllocateToEDocs_ShouldHandleTrailingSlashInGlowPortalsUri()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;

			var emailAttachmentSizeLimitInMB = 1;
			var fileFullPath1 = CreateSampleFileWithLength("test1.txt", 3 * Megabyte);
			var fileFullPath2 = CreateSampleFileWithLength("test2.txt", 4 * Megabyte);
			
			try
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				var glowPortalsUriWithTrailingSlash = "https://localhost/Glow/";
				using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUriWithTrailingSlash))
				{
					AssertNoExceptionThrown(() =>
					{
						var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
						deliveryGroup.SB_IsProcessed = true;
						deliveryGroup.SB_EmailSubjectLine = "Test Report Run Subject";

						var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
						var collection = new StmPrintJobMergedCollection(Factory);
						var printJob = GetSampleJob(fileFullPath1, deliveryGroup.PK);
						AddReportRunInfoForPrintJob(printJob, stmReportRun);
						printJob.SP_EmailAttachments = "test.xls";
						printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
						printJob.SP_EmailSubjectLine = $"test({AttachmentTypeList.Codes.Pdf})";
						collection.Add(printJob);
						Factory.Save();

						var processor = new EmailProcessorForTesting(collection);
						processor.Process();

						var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
						var storageMains = documentFactory.Load<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, stmReportRun.PK));
						var newPrintJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.PK, printJob.PK));

						AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
						var email = Env.OutgoingMailManager.EmailsCreated[0];
						AssertEquals("Email attachments should be empty because download link is created", 0, email.Attachments.Count);
						AssertEquals("There should only be one related eDocs of the report run", 1, storageMains[0].AllEDocs.Count);
						var eDocs = storageMains[0].AllEDocs[0];
						AssertEquals($"<body style='font-family:Calibri,sans-serif; font-size:16px'><br />The report exceeded the maximum attachment size allowed and could not be emailed. Please follow the applicable link to download the report: <br /><br />If you are an internal staff member, download the report via this link and authenticate with your {Core.Constants.ProductName} login credentials: <div><a href='https://localhost/Glow/goto/EDS?StmReportRunId={stmReportRun.PK}&EDocId={eDocs.UniqueKey}'>test.pdf</a></div><br />If you are a web portal user, to download the report, click this link and authenticate with your web portal login credentials: <div><a href='https://localhost/Glow/goto/EDC?StmReportRunId={stmReportRun.PK}&EDocId={eDocs.UniqueKey}'>test.pdf</a></div><br /><br /><br />Company Name</body>", email.Body);
					});
				}

				Env.OutgoingMailManager.EmailsCreated.Clear();
				var glowPortalsUriWithoutTrailingSlash = "https://localhost/Glow";
				using (SystemDataRegistry.Instance.AllocateReportOverEmailAttachmentLimitToEDocs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailAttachmentSizeLimitInMB))
				using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsUriWithoutTrailingSlash))
				{
					AssertNoExceptionThrown(() =>
					{
						var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
						deliveryGroup.SB_IsProcessed = true;
						deliveryGroup.SB_EmailSubjectLine = "Test Report Run Subject";

						var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
						var collection = new StmPrintJobMergedCollection(Factory);
						var printJob = GetSampleJob(fileFullPath2, deliveryGroup.PK);
						AddReportRunInfoForPrintJob(printJob, stmReportRun);
						printJob.SP_EmailAttachments = "test.xls";
						printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
						printJob.SP_EmailSubjectLine = $"test({AttachmentTypeList.Codes.Pdf})";
						collection.Add(printJob);
						Factory.Save();

						var processor = new EmailProcessorForTesting(collection);
						processor.Process();

						var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
						var storageMains = documentFactory.Load<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, stmReportRun.PK));
						var newPrintJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.PK, printJob.PK));

						AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
						var email = Env.OutgoingMailManager.EmailsCreated[0];
						AssertEquals("Email attachments should be empty because download link is created", 0, email.Attachments.Count);
						AssertEquals("There should only be one related eDocs of the report run", 1, storageMains[0].AllEDocs.Count);
						var eDocs = storageMains[0].AllEDocs[0];
						AssertEquals($"<body style='font-family:Calibri,sans-serif; font-size:16px'><br />The report exceeded the maximum attachment size allowed and could not be emailed. Please follow the applicable link to download the report: <br /><br />If you are an internal staff member, download the report via this link and authenticate with your {Core.Constants.ProductName} login credentials: <div><a href='https://localhost/Glow/goto/EDS?StmReportRunId={stmReportRun.PK}&EDocId={eDocs.UniqueKey}'>test.pdf</a></div><br />If you are a web portal user, to download the report, click this link and authenticate with your web portal login credentials: <div><a href='https://localhost/Glow/goto/EDC?StmReportRunId={stmReportRun.PK}&EDocId={eDocs.UniqueKey}'>test.pdf</a></div><br /><br /><br />Company Name</body>", email.Body);
					});
				}
			}
			finally
			{
				DeleteIfExists(fileFullPath1);
				DeleteIfExists(fileFullPath2);
			}
		}

		public void TestLogProcessing()
		{
			var groupPK = Factory.New<StmDeliveryGroup>().PK;
			var collection = new StmPrintJobMergedCollection(Factory);
			var printJob = collection.AddNew();
			printJob.SP_EmailSubjectLine = "Email Subject";
			printJob.SP_JobType = "EML";
			printJob.SP_EmailFromAddress = "test@edi.com.au";
			printJob.SP_Destination = "unit.test@cargowise.com";
			printJob.SP_SB_DeliveryGroup = groupPK;
			printJob.SP_DocumentName = "Document Name";
			printJob.SP_DocumentType = "NOT";
			printJob.SP_RelatedBusinessContext = "SHP";
			printJob.SP_Copies = 10;

			Env.OutgoingMailManager.EmailsCreated.Clear();
			new EmailProcessorForTesting(collection).Process();
			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var expectedMessage = @"Processing document ""Document Name"" with subject ""Email Subject"".
Document type [NOT].
Related Business Context: [SHP].
Email Attachments: [default.XLS].
Number of Copies: [10].
Email To: [unit.test@cargowise.com].
Fax Destination:[].";
			AssertEquals(expectedMessage, loggedMessages[0]);
		}

		#region Implementation

		string CreateSampleFile(string filename, string content)
		{
			string filefullpath = Path.Combine(Temp.TempPath, filename);
			using (Stream file = File.OpenWrite(filefullpath))
			{
				byte[] info = new UTF8Encoding(true).GetBytes(content);
				file.Write(info, 0, info.Length);
				file.Close();
			}
			return filefullpath;
		}

		string CreateSampleFileWithLength(string filename, int length)
		{
			var filefullpath = Path.Combine(Temp.TempPath, filename);
			using (var file = File.OpenWrite(filefullpath))
			{
				var bytes = new byte[length];
				for (var index = 0; index < length; index++)
				{
					bytes[index] = (byte)(index % 255);
				}
				file.Write(bytes, 0, bytes.Length);
				file.Close();
			}
			return filefullpath;
		}

		StmPrintJob GetSampleJob(string storedAttachmentFilename, ZGuid deliveryGroupPK)
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.StoredAttachmentSizeKB = 80;
			printJob.SP_JobType = "EML";
			printJob.SP_EmailFromAddress = "test@edi.com.au";
			printJob.SP_Destination = "unit.test@cargowise.com";
			printJob.SP_EmailSignature = "Company Name";
			printJob.SP_EmailSubjectLine = "Subject Line";
			printJob.StoredAttachmentFilename = storedAttachmentFilename;
			printJob.SP_SB_DeliveryGroup = deliveryGroupPK;
			return printJob;
		}

		void AddReportRunInfoForPrintJob(StmPrintJob printJob, StmReportRun stmReportRun = null)
		{
			printJob.SP_DocumentName = "Test Report Run";
			printJob.SP_ParentGuid = stmReportRun?.PK ?? ZGuid.NewZGuid();
			printJob.SP_ParentTableName = StmReportRunSchema.Constants.TableName;
			printJob.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.ReportStatistic;
			printJob.SP_CustomProperties = File.ReadAllBytes(Path.Combine(UnitTestingConstants.TestFilesDir, "test.xls"));
			printJob.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
			printJob.SP_DocumentType = Core.Constants.RefDocTypes.ScheduledReport;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPDFCombinedWithInvalidName()
		{
			EnvProxy.Instance.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var deliveryGroup = Factory.New<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			deliveryGroup.SB_EmailSubjectLine = "blah";

			var job1 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName, deliveryGroup.PK);
			job1.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;
			job1.SP_EmailSubjectLine = "job/1";

			var job2 = GetSampleJob(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFMultiPageFile, deliveryGroup.PK);
			job2.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdfc;

			var collection = new StmPrintJobMergedCollection(Factory) { job1, job2 };
			var processor = new EmailProcessorForTesting(collection);
			processor.Process();

			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email1 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Number of attachments", 1, email1.Attachments.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals(1, email.Attachments.Count);
			AssertEquals("job 1 - Merged.PDF", email.Attachments[0].DisplayName);
		}

		public void TestSinglePrintJobWithNoSubjecUsesDefaultSubject()
		{
			try
			{
				var groupPK = Factory.New<StmDeliveryGroup>().PK;
				StmPrintJobMergedCollection collection = new StmPrintJobMergedCollection(Factory);
				StmPrintJob printJob = collection.AddNew();
				printJob.SP_Sequence = 0;
				printJob.SP_EmailSubjectLine = "";
				printJob.SP_JobType = "EML";
				printJob.SP_EmailFromAddress = "test@edi.com.au";
				printJob.SP_Destination = "unit.test@cargowise.com";
				printJob.SP_SB_DeliveryGroup = groupPK;
				string fileName = Path.Combine(Temp.TempPath, "File 1");
				File.WriteAllText(fileName, "1", Encoding.ASCII);
				printJob.StoredAttachmentFilename = fileName;

				Env.OutgoingMailManager.EmailsCreated.Clear();
				new EmailProcessorForTesting(collection).Process();
				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("No Subject", email.Subject);
				AssertEquals("Number of attachments", 1, email.Attachments.Count);
			}
			finally
			{
				string fileName = Path.Combine(Temp.TempPath, "File 1");
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}
			}
		}

		class EmailProcessorForTesting : EmailProcessor
		{
			public EmailProcessorForTesting(StmPrintJobMergedCollection mergedPrintGroup)
				: base(mergedPrintGroup, new PrintJobManager.ProgressDelegate((eventType, statusMessage) => { loggedMessages.Add(statusMessage); }))
			{
			}
		}

		static readonly List<String> loggedMessages = new List<String>();

		protected override void TearDown()
		{
			loggedMessages.Clear();
			base.TearDown();
		}

		internal override MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection printGroup)
		{
			return new EmailProcessor(printGroup, new PrintJobManager.ProgressDelegate((eventType, statusMessage) => { loggedMessages.Add(statusMessage); }));
		}

		#endregion
	}
}
