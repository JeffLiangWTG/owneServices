using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class AcceptableFileValidatorTest : TestCaseWithFactory
	{
		public void TestDisguisedFile()
		{
			using (var disguisedExeFile = TempFile.NewWithExtension("txt"))
			{
				using (var writeStream = new FileInfo(disguisedExeFile.Filename).OpenWrite())
				{
					writeStream.WriteByte(0x4D);
					writeStream.WriteByte(0x5A);
				}
				var validator = new AcceptableFileValidator(true, disguisedExeFile.Filename);
				var validFiles = validator.GetValidFiles();
				AssertEquals(0, validFiles.Length);
				var message = validator.GetInvalidFilesMessage();
				var listFormatter = new FileListFormatter();
				var expected = "The following files were not added because they are potentially dangerous file types:"
					+ System.Environment.NewLine + Path.GetFileName(disguisedExeFile.Filename)
					+ " (Actually exe)";
				AssertContains(expected, message);
			}
		}

		public void TestGetValidFiles_FlagPDF()
		{
			using (TempFile emptyFile = TempFile.New())
			using (TempFile bannedExtensionFile = TempFile.NewWithExtension("exe"))
			{
				AcceptableFileValidator validator = new AcceptableFileValidator(true, TestTifPath, emptyFile.Filename, bannedExtensionFile.Filename, SamplePdfPath);

				string[] validFiles = validator.GetValidFiles();
				AssertEquals("One valid file returned if FlagPDf is true - others are empty, banned or PDF files", 1, validFiles.Length);
				AssertEquals(TestTifPath, validFiles[0]);
			}
		}

		public void TestGetValidFiles_DoNotFlagPDF()
		{
			using (TempFile emptyFile = TempFile.New())
			using (TempFile bannedExtensionFile = TempFile.NewWithExtension("exe"))
			{
				AcceptableFileValidator validator = new AcceptableFileValidator(false, TestTifPath, emptyFile.Filename, bannedExtensionFile.Filename, SamplePdfPath);
				string[] validFiles = validator.GetValidFiles();
				AssertEquals("Two valid files are returned if FlagPDF is false - others are empty and banned", 2, validFiles.Length);
				AssertEquals(TestTifPath, validFiles[0]);
				AssertEquals(SamplePdfPath, validFiles[1]);
			}
		}

		public void TestGetValidFiles_DontIncludeFilesThatDontExist()
		{
			using (TempFile fileThatDoesntExist = TempFile.New())
			{
				File.Delete(fileThatDoesntExist.Filename);
				AcceptableFileValidator validator = new AcceptableFileValidator(false, fileThatDoesntExist.Filename, TestTifPath);
				string[] validFiles = validator.GetValidFiles();
				AssertEquals("One file existed", 1, validFiles.Length);
				AssertEquals(TestTifPath, validFiles[0]);
			}
		}

		public void TestGetInvalidFilesMessage_FlagPDF()
		{
			using (TempFile emptyFile = TempFile.NewWithExtension("txt"))
			using (TempFile bannedExtensionFile = TempFile.NewWithExtension("exe"))
			{
				AcceptableFileValidator validator = new AcceptableFileValidator(true, TestTifPath, emptyFile.Filename, bannedExtensionFile.Filename, SamplePdfPath);
				FileListFormatter listFormatter = new FileListFormatter();

				string message = validator.GetInvalidFilesMessage();
				string expected = "The following files were not added because they are potentially dangerous file types:" + System.Environment.NewLine + listFormatter.FormatListToString(new string[] { Path.GetFileName(bannedExtensionFile.Filename) }, false);
				AssertContains(expected, message);

				expected = "The following files were not added because they are empty:" + System.Environment.NewLine + listFormatter.FormatListToString(new string[] { Path.GetFileName(emptyFile.Filename) }, false);
				AssertContains(expected, message);

				expected = "PDF Files are no longer supported for add. Please add the file to the eDocs tab of the appropriate form, or alternatively you can manually convert the file to TIF." + System.Environment.NewLine + listFormatter.FormatListToString(new string[] { Path.GetFileName(SamplePdfPath) }, false);
				AssertContains(expected, message);
			}
		}

		public void TestGetInvalidFilesMessage_TooLongNameFiles()
		{
			var exceed260Characters = "aaa";
			for (int i = 0; i < 8; i++)
			{
				exceed260Characters += exceed260Characters;
			}
			Assert("Precodition: file name must be larger than 260 characters.", exceed260Characters.Length > 260);
			AcceptableFileValidator validator = new AcceptableFileValidator(true, exceed260Characters);
			AssertContains("The following files were not added because they are too long(The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.):" + System.Environment.NewLine +
				new FileListFormatter().FormatListToString(new string[] { exceed260Characters }, false),
				validator.GetInvalidFilesMessage());
		}

		public void TestGetInvalidFilesMessage_UnsupportedFiles()
		{
			var unsupportedFilePath = "&#:";
			var validator = new AcceptableFileValidator(true, unsupportedFilePath);
			AssertContains("The following files were not added because they were not supported. Please check that they are present, valid and accessible:" + System.Environment.NewLine +
				new FileListFormatter().FormatListToString(new string[] { unsupportedFilePath }, true),
				validator.GetInvalidFilesMessage());
		}

		public void TestGetInvalidFilesMessage_DoNotFlagPDF()
		{
			using (TempFile emptyFile = TempFile.NewWithExtension("txt"))
			using (TempFile bannedExtensionFile = TempFile.NewWithExtension("exe"))
			{
				AcceptableFileValidator validator = new AcceptableFileValidator(false, TestTifPath, emptyFile.Filename, bannedExtensionFile.Filename, SamplePdfPath);
				FileListFormatter listFormatter = new FileListFormatter();

				string message = validator.GetInvalidFilesMessage();
				string expected = "The following files were not added because they are potentially dangerous file types:" + System.Environment.NewLine + listFormatter.FormatListToString(new string[] { Path.GetFileName(bannedExtensionFile.Filename) }, false);
				AssertContains(expected, message);

				expected = "The following files were not added because they are empty:" + System.Environment.NewLine + listFormatter.FormatListToString(new string[] { Path.GetFileName(emptyFile.Filename) }, false);
				AssertContains(expected, message);

				expected = "PDF Files are no longer supported for add. Please add the file to the eDocs tab of the appropriate form, or alternatively you can manually convert the file to TIF." + System.Environment.NewLine + listFormatter.FormatListToString(new string[] { Path.GetFileName(SamplePdfPath) }, false);
				Assert("Message should not have a warning about PDFs", !message.Contains(expected));
			}
		}

		public void TestGetInvalidFilesMessage_NoInvalidFiles()
		{
			AcceptableFileValidator validator = new AcceptableFileValidator(true, TestTifPath);
			Assert("No files have errors, message should be empty", validator.GetInvalidFilesMessage().IsEmpty);
		}

		[ExpectNoExceptions]
		public void TestDoesNotThrowIfNoFiles()
		{
			AcceptableFileValidator validator = new AcceptableFileValidator(true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string TestTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(testTifPath))
				{
					testTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");
				}
				return testTifPath;
			}
		}
		string testTifPath;

		string SamplePdfPath
		{
			get
			{
				if (string.IsNullOrEmpty(samplePdf))
				{
					samplePdf = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
				}
				return samplePdf;
			}
		}
		string samplePdf;
	}
}
