using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class ExportInstructionsTest : TestCaseWithFactory
	{
		public void TestOutputFile()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertEquals("Empty by default", ZString.Empty, instructions.OutputFile);

			using (TempFile file = TempFile.New())
			{
				instructions.SetOutputFile(file.Filename);
				AssertEquals("Set to temp file's filename", file.Filename, instructions.OutputFile);
			}
		}

		public void TestMethodOfExport()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertEquals("File export by default", ExportType.File, instructions.MethodOfExport);

			instructions.MethodOfExport = ExportType.Email;
			AssertEquals("Should now be email", ExportType.Email, instructions.MethodOfExport);

			instructions.MethodOfExport = ExportType.File;
			AssertEquals("Back to file", ExportType.File, instructions.MethodOfExport);
		}

		public void TestBasePath()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertEquals("temp path by default", Temp.TempPath, instructions.BasePath);

			instructions.BasePath = "BasePath";
			AssertEquals("Should now be BasePath", "BasePath", instructions.BasePath);
		}

		public void TestSpecifiedFilePathWithExtension()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertEquals("Empty by default - no SpecifiedFilename", ZString.Empty, instructions.SpecifiedFilePathWithExtension);

			instructions.BasePath = @"BasePath\Enterprise\";
			AssertEquals("Still empty even with a base path - there's no SpecifiedFilename", ZString.Empty, instructions.SpecifiedFilePathWithExtension);

			instructions.SpecifiedFilename = "ABC";
			AssertEquals("Now that a filename and base path exist, should return something", @"BasePath\Enterprise\ABC.txt", instructions.SpecifiedFilePathWithExtension);

			instructions.BasePath = Temp.TempPath;
			AssertEquals("Changing BasePath to TempPath updates the filename", Temp.TempPath + "ABC.txt", instructions.SpecifiedFilePathWithExtension);
		}

		public void TestUpperAndLowerCaseFileExtension()
		{
			ExportInstructions instructions = new ExportInstructions();
			instructions.BasePath = @"BasePath\ThisIsJustAString\";
			instructions.SpecifiedFilename = "ABC";
			instructions.UseUpperCaseFileExtension = true;
			AssertEquals("File extension should be in UpperCase", @"BasePath\ThisIsJustAString\ABC.TXT", instructions.SpecifiedFilePathWithExtension);

			instructions.UseUpperCaseFileExtension = false;
			AssertEquals("File extension should be in UpperCase", @"BasePath\ThisIsJustAString\ABC.txt", instructions.SpecifiedFilePathWithExtension);
		}

		public void TestSpecifiedFilename()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertEquals("Specified filename empty by default", ZString.Empty, instructions.SpecifiedFilename);
			instructions.SpecifiedFilename = "ABC123";
			AssertEquals("Specified filename not empty any more", "ABC123", instructions.SpecifiedFilename);
		}

		public void TestFileExtension()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertEquals("extension type text by default", FileExtensionType.Txt, instructions.FileExtension);

			instructions.FileExtension = FileExtensionType.Csv;
			AssertEquals("Extension type csv after setting", FileExtensionType.Csv, instructions.FileExtension);
		}

		public void TestEmailProperties()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertNotNull("EmailProperties can't be null", instructions.EmailProperties);

			EmailExportInstructions emailInstructions = new EmailExportInstructions();
			emailInstructions.UserEnteredRecipients = "blah";
			instructions.EmailProperties = emailInstructions;
			AssertEquals("Email properties", "blah", instructions.EmailProperties.UserEnteredRecipients);
		}

		public void TestFtpProperties()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertNotNull("FtpProperties can't be null", instructions.FtpProperties);

			FtpExportInstructions ftpInstructions = new FtpExportInstructions();
			ftpInstructions.Username = "blah";
			instructions.FtpProperties = ftpInstructions;
			AssertEquals("Ftp properties", "blah", instructions.FtpProperties.Username);
		}
	}
}
