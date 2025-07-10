using System.IO;
using CargoWise.IO;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FileTypeValidationTest : TestCaseWithDummy
	{
		public void TestIsDangerousFileStream()
		{
			//dll and exe aren't quick to distinguish, but we don't want randos to upload dlls either so it works out in practice
			//NOTE: Can't just grab an exe/dll from Bin because BaseSourcePath is banned ( https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/1494/Refactoring-BaseSourcePath )
			//and can't embed one because it's banned to add new exe/dlls to CW1 repo without a good enough reason (and this isn't good enough)
			//so we'll just make our own from bytes ( http://www.phreedom.org/research/tinype/ )
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			string extension;
			var result = Validator.IsDangerousFile(new MemoryStream(new byte[] { 0x4D, 0x5A, 0x00, 0x00, 0x50, 0x45, 0x00, 0x00, 0x4C, 0x01, 0x01, 0x00, 0x6A, 0x2A, 0x58,
					0xC3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x03, 0x01, 0x0B, 0x01, 0x08, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
					0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04,
					0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x68, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 }), out extension);
			AssertEquals(true, result);
			AssertEquals("exe", extension);
			result = Validator.IsDangerousFile(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.test.zip")), out extension);
			AssertEquals(false, result);
			AssertEquals("zip", extension);
			result = Validator.IsDangerousFile(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.Fax.ico")), out extension);
			AssertEquals(false, result);
			AssertEquals("ico", extension);
			result = Validator.IsDangerousFile(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.test.txt")), out extension);
			AssertEquals(false, result);
			AssertEquals(null, extension);
			result = Validator.IsDangerousFile(new MemoryStream(resourceRetriever.GetBytes("CargoWise.EntityFramework.test empty.txt")), out extension);
			AssertEquals(false, result);
			AssertEquals(null, extension);
		}

		public void TestIsFileType()
		{
			Assert("Should match", Validator.IsFileType(DOCFile, ".doc"));
			Assert("Should match", Validator.IsFileType(EXEFile, ".EXE"));
			Assert("Should match", Validator.IsFileType(TIFFFile, ".TiFf"));
			Assert("Should match", Validator.IsFileType(TIFFile, ".tIf"));

			Assert("Should NOT match", !Validator.IsFileType(DOCFile, ".exe"));
		}

		public void TestIsValidFileExtensionForFax()
		{
			Assert("Should match TIF", Validator.IsValidFileExtensionForFax(TIFFile));
			Assert("Should match TIFF", Validator.IsValidFileExtensionForFax(TIFFFile));
			Assert("Should NOT match DOC", !Validator.IsValidFileExtensionForFax(DOCFile));
			Assert("Should NOT match EXE", !Validator.IsValidFileExtensionForFax(EXEFile));
		}

		public void TestIsValidFileExtensionForEmail()
		{
			Assert("Should match TIF", Validator.IsValidFileExtensionForEmail(TIFFile));
			Assert("Should match TIFF", Validator.IsValidFileExtensionForEmail(TIFFFile));
			Assert("Should match DOC", Validator.IsValidFileExtensionForEmail(DOCFile));
			Assert("Should NOT match EXE", !Validator.IsValidFileExtensionForEmail(EXEFile));
		}

		public void TestIsDangerousFile()
		{
			Assert("Should match EXE", Validator.IsDangerousFile(EXEFile));
			Assert("Should not match Bin", !Validator.IsDangerousFile(BINFile));
			Assert("Should not match DOC", !Validator.IsDangerousFile(DOCFile));
			Assert("Should not match TIF", !Validator.IsDangerousFile(TIFFile));
			Assert("Should not match TIFF", !Validator.IsDangerousFile(TIFFFile));
			Assert("Should match invalid filename", Validator.IsDangerousFile("Test*"));
		}

		public void TestZipIsNonDangerous()
		{
			Assert(!Validator.IsDangerousFile("hello.zip"));
		}

		public void TestDangerousFileListIsCorrect()
		{
			foreach (string ext in FileTypeValidation.DangerousFileExtensions)
			{
				Assert("Must start with a dot", ext.StartsWith("."));
				AssertEquals("Must be in uppercase", ext.ToUpper(), ext);
			}
		}

		#region Setup

		FileTypeValidation Validator;

		const string TIFFile = @"Testfile.tif";
		const string TIFFFile = @"Testfile.tiff";
		const string EXEFile = @"Testfile.exe";
		const string DOCFile = @"Testfile.DOC";
		const string BINFile = @"Testfile.Bin";

		protected override void SetUp()
		{
			base.SetUp();
			Validator = new FileTypeValidation();
		}

		#endregion

	}
}
