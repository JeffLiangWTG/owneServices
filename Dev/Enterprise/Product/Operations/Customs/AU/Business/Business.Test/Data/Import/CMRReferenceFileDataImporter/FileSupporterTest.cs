using System.IO;
using CargoWise.IO;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FileSupporterTest : TestCase
	{
		public void TestUnzipFilesAndDeleteTempDirectory()
		{
			try
			{
				var file = new EmbeddedResourceRetriever().GetBytes("Enterprise.Customs.AU.Declaration.Business.Testing.Data.Import.CMRReferenceFileDataImporter.TestFiles.MainFiles.tar.gz");
				supporter.UnzipFiles(file);
				var unzippedFiles = supporter.GetFilesInDirectory(".txt", supporter.UnzipFilePath);
				AssertEquals("There should be three files", 5, unzippedFiles.Length);
				Assert("First file should be text", unzippedFiles[0].EndsWith(".txt"));
				Assert("Second file should be text", unzippedFiles[1].EndsWith(".txt"));
				Assert("Third file should be text", unzippedFiles[2].EndsWith(".txt"));
			}
			finally
			{
				supporter.DeleteTempDirectory();
				AssertEquals("Directory no longer exists", false, Directory.Exists(supporter.UnzipFilePath));
			}
		}

		public void TestUnzipFilePath()
		{
			Assert("Unzip File Path should start with " + Env.TempPath, supporter.UnzipFilePath.StartsWith(Env.TempPath));
			Assert("Unzip File Path ends with CMRReferenceFiles", supporter.UnzipFilePath.EndsWith("CMRReferenceFiles"));

			FileSupporter newSupporter = new FileSupporter();

			Assert("Unzip File Path should start with " + Env.TempPath, newSupporter.UnzipFilePath.StartsWith(Env.TempPath));
			Assert("Unzip File Path ends with CMRReferenceFiles", newSupporter.UnzipFilePath.EndsWith("CMRReferenceFiles"));
			Assert("Should be different", newSupporter.UnzipFilePath != supporter.UnzipFilePath);
		}

		protected override void SetUp()
		{
			base.SetUp();
			supporter = new FileSupporter();
		}

		FileSupporter supporter;
	}
}
