using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PrintProcessing.Mailer.Email.Test
{
	sealed class EmailBodyTest : TestCaseWithFactory
	{
		public void TestBodyWithOutCoverNoteWhenAttachmentTypeIsHTMF()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_EmailSignature = "singnature text";
			printJob.SP_EmailAttachmentFormat = "HTMF";
			var emailAttachmentList = new AttachmentDefCollection();

			var note = Factory.New<StmNote>();
			note.ST_NoteType = "PRV";
			note.ST_ParentID = printJob.PK;
			note.ST_Table = StmPrintJob.Schema.TableName;
			note.ST_NoteText = "cover note text";

			DocumentsDataRegistry.Instance.AddEmailFaxCoverNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var expected = System.Environment.NewLine + System.Environment.NewLine + printJob.SP_EmailSignature;
			AssertEquals("Email body", expected, EmailBody.Body(printJob, emailAttachmentList));

			printJob.SP_EmailAttachmentFormat = "HTML";
			expected = note.ST_NoteText + System.Environment.NewLine + System.Environment.NewLine + printJob.SP_EmailSignature;
			AssertEquals("Email body", expected, EmailBody.Body(printJob, emailAttachmentList));
		}

		public void TestBodyWithCoverNote()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_EmailSignature = "singnature text";
			var emailAttachmentList = new AttachmentDefCollection();

			var note = Factory.New<StmNote>();
			note.ST_NoteType = "PRV";
			note.ST_ParentID = printJob.PK;
			note.ST_Table = StmPrintJob.Schema.TableName;
			note.ST_NoteText = "cover note text";

			var expected = note.ST_NoteText + System.Environment.NewLine + System.Environment.NewLine + printJob.SP_EmailSignature;
			AssertEquals("Email body", expected, EmailBody.Body(printJob, emailAttachmentList));
		}

		public void TestBodyWithOutCoverNoteWhenRegistyIsSetToFalse()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_EmailSignature = "singnature text";
			var emailAttachmentList = new AttachmentDefCollection();

			var note = Factory.New<StmNote>();
			note.ST_NoteType = "PRV";
			note.ST_ParentID = printJob.PK;
			note.ST_Table = StmPrintJob.Schema.TableName;
			note.ST_NoteText = "cover note text";

			DocumentsDataRegistry.Instance.AddEmailFaxCoverNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var expected = System.Environment.NewLine + System.Environment.NewLine + printJob.SP_EmailSignature;
			AssertEquals("Email body should not contain cover note text", expected, EmailBody.Body(printJob, emailAttachmentList));

			DocumentsDataRegistry.Instance.AddEmailSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Email body should not contain cover note text and signature", string.Empty, EmailBody.Body(printJob, emailAttachmentList));
		}

		public void TestBodyWithNoAttachments()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_EmailSignature = "signature text";
			var emailAttachmentList = new AttachmentDefCollection();

			var expected = System.Environment.NewLine + System.Environment.NewLine + printJob.SP_EmailSignature;
			AssertEquals("Email body", expected, EmailBody.Body(printJob, emailAttachmentList));
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBodyWithAttachments()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_EmailAttachmentFormat = "HTML";
			printJob.SP_EmailSignature = "signature";
			var emailAttachmentList = new AttachmentDefCollection();

			emailAttachmentList.Add(new AttachmentDef(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestHtmlFileName));
			var footer = System.Environment.NewLine + System.Environment.NewLine + printJob.SP_EmailSignature;
			var expected = string.Empty;
			AssertEquals(expected + footer, EmailBody.Body(printJob, emailAttachmentList));

			emailAttachmentList.Add(new AttachmentDef(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPngFileName));
			AssertEquals(expected + footer, EmailBody.Body(printJob, emailAttachmentList));

			emailAttachmentList.Add(new AttachmentDef(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestXLSFileName));
			printJob.SP_EmailAttachmentFormat = "XLS";
			expected = System.Environment.NewLine + "Please see the attached documents." + System.Environment.NewLine;
			AssertEquals(expected + footer, EmailBody.Body(printJob, emailAttachmentList));

			emailAttachmentList.Add(new AttachmentDef(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName));
			printJob.SP_EmailAttachmentFormat = "PDF";

			expected = System.Environment.NewLine + "Please see the attached documents." + System.Environment.NewLine;

			AssertEquals(expected + footer, EmailBody.Body(printJob, emailAttachmentList));
		}
	}
}
