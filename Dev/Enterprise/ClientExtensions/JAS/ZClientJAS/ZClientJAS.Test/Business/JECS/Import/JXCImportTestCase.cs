using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class JXCImportTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			SetupTestFileDirectory();
		}

		protected override void TearDown()
		{
			CleanUpTestFileDirectory();
			base.TearDown();
		}

		void SetupTestFileDirectory()
		{
			TestFileDir = Path.Combine(Env.TempPath, "JXCImportTest");
			Directory.CreateDirectory(TestFileDir);
		}

		void CleanUpTestFileDirectory()
		{
			TempDirectory.DeleteDirectory(TestFileDir);
		}

		protected string CreateTestFile(string parentDir, string content)
		{
			string fileName = Path.Combine(parentDir, ZGuid.NewZGuid().ToString());
			using (StreamWriter writer = File.CreateText(fileName))
			{
				writer.Write(content);
			}

			return fileName;
		}

		protected string CreateTestFile(string content)
		{
			return CreateTestFile(TestFileDir, content);
		}

		protected string TestFileDir;
	}
}
