using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Builder.GenerateDbUpgraderResources.Testing
{
	sealed class SetupBaseMockRegenTest : TestCase
	{
		public void TestRegenarateFiles_Full_ForceMinor()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				string sourcePath = Path.Combine(tempDir, SetupForTest.SourceFileName);
				string versionPath = Path.Combine(tempDir, SetupForTest.VersionFileName);
				string resxPath = Path.Combine(tempDir, SetupForTest.ResxFileName);
				string mainSchemaFilePath = Path.Combine(tempDir, SetupForTest.MainDbSchemaFileName);
				string docManagerSchemaFilePath = Path.Combine(tempDir, SetupForTest.DocManagerDbSchemaFileName);

				CreateTestSourceFile(sourcePath);
				CreateTestVersionFile(versionPath);
				CreateTestSchemaFiles(mainSchemaFilePath, docManagerSchemaFilePath);

				// Run 1st regen
				SetupForTest testSetup = new SetupForTest(tempDir.DirectoryName);
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.ForceMinor);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "1st REGEN - Main");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "1st REGEN - DocManager");

				// Asserts Version File Contents - (1, 1) ; (1, 1)
				string expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(1, 1);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(1, 1);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "1st REGEN");

				// Run 2nd regen - NO CHANGES
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.ForceMinor);

				// Asserts Script File Contents - NO CHANGES
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "2nd REGEN - Main - NO CHANGES EXPECTED");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "2nd REGEN - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (1, 1) ; (1, 1) - NO CHANGES
				AssertFileContents(versionPath, expectedVersionContents, "2nd REGEN - NO CHANGES");

				// Run 3rd regen - NO DOCMANAGER CHANGES
				AppendToFile(sourcePath, "CREATE TABLE Tab2 ();");
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.ForceMinor);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase + System.Environment.NewLine + ExpectedMainSchemaContentsExtension, "3rd REGEN - Main");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "3rd REGEN - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (1, 2) ; (1, 1)
				expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(1, 2);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(1, 1);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "3rd REGEN - NO DOCMANAGER CHANGES");
			}
		}

		public void TestRegenarateFiles_Full_Standard()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				string sourecePath = Path.Combine(tempDir, SetupForTest.SourceFileName);
				string versionPath = Path.Combine(tempDir, SetupForTest.VersionFileName);
				string resxPath = Path.Combine(tempDir, SetupForTest.ResxFileName);
				string mainSchemaFilePath = Path.Combine(tempDir, SetupForTest.MainDbSchemaFileName);
				string docManagerSchemaFilePath = Path.Combine(tempDir, SetupForTest.DocManagerDbSchemaFileName);

				CreateTestSourceFile(sourecePath);
				CreateTestVersionFile(versionPath);
				CreateTestSchemaFiles(mainSchemaFilePath, docManagerSchemaFilePath);

				// Run 1st regen
				SetupForTest testSetup = new SetupForTest(tempDir.DirectoryName);
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.Standard);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "1st REGEN - Main");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "1st REGEN - DocManager");

				// Asserts Version File Contents - (2, 0) ; (2, 0)
				string expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(2, 0);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(2, 0);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "1st REGEN");

				// Run 2nd regen - NO CHANGES
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.Standard);

				// Asserts Script File Contents - NO CHANGES
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "2nd REGEN - Main - NO CHANGES EXPECTED");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "2nd REGEN - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (2, 0) ; (2, 0) - NO CHANGES
				AssertFileContents(versionPath, expectedVersionContents, "2nd REGEN - NO CHANGES");

				// Run 3rd regen - NO DOCMANAGER CHANGES
				AppendToFile(sourecePath, "CREATE TABLE Tab2 ();");
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.Standard);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase + System.Environment.NewLine + ExpectedMainSchemaContentsExtension, "3rd REGEN - Main");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "3rd REGEN - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (3, 0) ; (2, 0)
				expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(3, 0);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(2, 0);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "3rd REGEN - NO DOCMANAGER CHANGES");
			}
		}

		public void TestRegenarateFiles_BumpOnly_ForceMinor()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				string sourecePath = Path.Combine(tempDir, SetupForTest.SourceFileName);
				string versionPath = Path.Combine(tempDir, SetupForTest.VersionFileName);
				string resxPath = Path.Combine(tempDir, SetupForTest.ResxFileName);
				string mainSchemaFilePath = Path.Combine(tempDir, SetupForTest.MainDbSchemaFileName);
				string docManagerSchemaFilePath = Path.Combine(tempDir, SetupForTest.DocManagerDbSchemaFileName);

				CreateTestSourceFile(sourecePath);
				CreateTestVersionFile(versionPath);
				CreateTestSchemaFiles(mainSchemaFilePath, docManagerSchemaFilePath);

				// Run 1st regen - FULL
				SetupForTest testSetup = new SetupForTest(tempDir.DirectoryName);
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.ForceMinor);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "1st REGEN FULL - Main");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "1st REGEN FULL - DocManager");

				// Asserts Version File Contents - (1, 1) ; (1, 1)
				string expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(1, 1);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(1, 1);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "1st REGEN - FULL");

				// Run 2nd regen - BUMP
				testSetup.RegenarateFiles(RegenActionEnum.BumpOnly, RegenVersionChangeEnum.ForceMinor);

				// Asserts Script File Contents - BUMP
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "2nd REGEN BUMP ONLY - Main - NO CHANGES EXPECTED");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "2nd REGEN BUMP ONLY - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (1, 2) ; (1, 1) - BUMP
				expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(1, 2);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(1, 1);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "2nd REGEN - BUMP");

				// Run 3rd regen - BUMP
				AppendToFile(sourecePath, "CREATE TABLE Tab2 ();");
				testSetup.RegenarateFiles(RegenActionEnum.BumpOnly, RegenVersionChangeEnum.ForceMinor);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "3rd REGEN BUMP ONLY - Main - NO CHANGES EXPECTED");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "3rd REGEN BUMP ONLY - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (1, 3) ; (1, 1)
				expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(1, 3);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(1, 1);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "3rd REGEN - BUMP");
			}
		}

		public void TestRegenarateFiles_BumpOnly_Standard()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				string sourecePath = Path.Combine(tempDir, SetupForTest.SourceFileName);
				string versionPath = Path.Combine(tempDir, SetupForTest.VersionFileName);
				string resxPath = Path.Combine(tempDir, SetupForTest.ResxFileName);
				string mainSchemaFilePath = Path.Combine(tempDir, SetupForTest.MainDbSchemaFileName);
				string docManagerSchemaFilePath = Path.Combine(tempDir, SetupForTest.DocManagerDbSchemaFileName);

				CreateTestSourceFile(sourecePath);
				CreateTestVersionFile(versionPath);
				CreateTestSchemaFiles(mainSchemaFilePath, docManagerSchemaFilePath);

				// Run 1st regen - FULL
				SetupForTest testSetup = new SetupForTest(tempDir.DirectoryName);
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.Standard);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "1st REGEN FULL - Main");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "1st REGEN FULL - DocManager");

				// Asserts Version File Contents - (2, 0) ; (2, 0)
				string expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(2, 0);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(2, 0);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "1st REGEN - FULL");

				// Run 2nd regen - BUMP
				testSetup.RegenarateFiles(RegenActionEnum.BumpOnly, RegenVersionChangeEnum.Standard);

				// Asserts Script File Contents - BUMP
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "2nd REGEN BUMP ONLY - Main - NO CHANGES EXPECTED");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "2nd REGEN BUMP ONLY - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (3, 0) ; (2, 0) - BUMP
				expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(3, 0);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(2, 0);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "2nd REGEN - BUMP");

				// Run 3rd regen - BUMP
				AppendToFile(sourecePath, "CREATE TABLE Tab2 ();");
				testSetup.RegenarateFiles(RegenActionEnum.BumpOnly, RegenVersionChangeEnum.Standard);

				// Asserts Script File Contents
				AssertFileContents(mainSchemaFilePath, ExpectedMainSchemaContentsBase, "3rd REGEN BUMP ONLY - Main - NO CHANGES EXPECTED");
				AssertFileContents(docManagerSchemaFilePath, ExpectedDocManagerSchemaContents, "3rd REGEN BUMP ONLY - DocManager - NO CHANGES EXPECTED");

				// Asserts Version File Contents - (4, 0) ; (2, 0)
				expectedVersionContents =
					"public static readonly VersionLabel Application = new VersionLabel(4, 0);\r\n"
					+ "public static readonly VersionLabel DocManager = new VersionLabel(2, 0);\r\n";
				AssertFileContents(versionPath, expectedVersionContents, "3rd REGEN - BUMP");
			}
		}

		public void TestRegenarateFilesWithMultilineStatements()
		{
			using (var tempDir = new TempDirectory())
			{
				var sourcePath = Path.Combine(tempDir, SetupForTest.SourceFileName);
				var versionPath = Path.Combine(tempDir, SetupForTest.VersionFileName);
				var mainSchemaFilePath = Path.Combine(tempDir, SetupForTest.MainDbSchemaFileName);
				var docManagerSchemaFilePath = Path.Combine(tempDir, SetupForTest.DocManagerDbSchemaFileName);

				using (var writer = File.CreateText(sourcePath))
				{
					writer.WriteLine("CREATE TABLE Table1 (");
					writer.WriteLine("	Col1 VARCHAR(MAX),");
					writer.WriteLine("	Col2 NVARCHAR(MAX) NULL");
					writer.WriteLine(");");
					writer.WriteLine();
					writer.WriteLine("CREATE TABLE [StorageDocs] (");
					writer.WriteLine("	Col1 VARBINARY(MAX)");
					writer.WriteLine(");");
				}

				CreateTestVersionFile(versionPath);
				CreateTestSchemaFiles(mainSchemaFilePath, docManagerSchemaFilePath);

				var testSetup = new SetupForTest(tempDir.DirectoryName);
				testSetup.RegenarateFiles(RegenActionEnum.Full, RegenVersionChangeEnum.Standard);

				AssertFileContents(mainSchemaFilePath, File.ReadAllText(sourcePath), "Main");
				AssertFileContents(docManagerSchemaFilePath, "CREATE TABLE [StorageDocs] (\r\n\tCol1 VARBINARY(MAX)\r\n);", "DocManager");
			}
		}

		#region Implementation

		void CreateTestSourceFile(string sourecePath)
		{
			using (StreamWriter writer = File.CreateText(sourecePath))
			{
				writer.WriteLine("CREATE TABLE Tab1 (Col1 VARCHAR(MAX), Col2 NVARCHAR(MAX) NULL);");
				writer.WriteLine("CREATE TABLE [StorageDocs] (Col1 VARBINARY(MAX));");
				writer.WriteLine("CREATE NONCLUSTERED INDEX Tab1Index;");
				writer.WriteLine("ALTER TABLE dbo.StorageDocs ADD CONSTRAINT [StorageDocsFk] FOREIGN;");
			}
		}

		void CreateTestSchemaFiles(string mainSchemaFilePath, string docManagerSchemaFilePath)
		{
			using (StreamWriter writer = File.CreateText(mainSchemaFilePath))
			{
				writer.WriteLine("");
			}

			using (StreamWriter writer = File.CreateText(docManagerSchemaFilePath))
			{
				writer.WriteLine("");
			}
		}

		void CreateTestVersionFile(string versionPath)
		{
			using (StreamWriter writer = File.CreateText(versionPath))
			{
				writer.WriteLine("public static readonly VersionLabel Application = new VersionLabel(1, 0);");
				writer.WriteLine("public static readonly VersionLabel DocManager = new VersionLabel(0, 0);");
			}
		}

		void AppendToFile(string filePath, string textToAppend)
		{
			string fileText = File.ReadAllText(filePath);
			fileText += textToAppend;

			using (StreamWriter writer = File.CreateText(filePath))
			{
				writer.Write(fileText);
			}
		}

		void AssertFileContents(string filePath, string expectedContents, string assertMessagePrefix)
		{
			string actualContents = File.ReadAllText(filePath);
			AssertMultilineASCIIEquals(
				assertMessagePrefix + "\r\n" + filePath + " contents:",
				expectedContents, actualContents);
		}

		#region Expected Resx Contents

		const string ExpectedMainSchemaContentsBase =
			"CREATE TABLE Tab1 (Col1 VARCHAR(MAX), Col2 NVARCHAR(MAX) NULL);\r\n\r\n" +
			"CREATE TABLE [StorageDocs] (Col1 VARBINARY(MAX));\r\n\r\n" +
			"CREATE NONCLUSTERED INDEX Tab1Index;\r\n\r\n" +
			"ALTER TABLE dbo.StorageDocs ADD CONSTRAINT [StorageDocsFk] FOREIGN;\r\n";

		const string ExpectedMainSchemaContentsExtension =
			"CREATE TABLE Tab2 ();\r\n\r\n";

		const string ExpectedDocManagerSchemaContents =
			"CREATE TABLE [StorageDocs] (Col1 VARBINARY(MAX));\r\n\r\n" +
			"ALTER TABLE dbo.StorageDocs ADD CONSTRAINT [StorageDocsFk] FOREIGN;\r\n\r\n";

		#endregion

		#endregion
	}
}
