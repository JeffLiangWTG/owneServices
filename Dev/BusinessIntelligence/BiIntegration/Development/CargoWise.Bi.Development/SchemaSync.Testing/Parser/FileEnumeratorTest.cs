using System;
using System.IO;
using System.Linq;
using CargoWise.Bi.Development.SchemaSync.Parser;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing.Parser;

public class FileEnumeratorTest : TestCase
{
	public void TestCannotResetEnumerator()
	{
		var fileEnumerator = new FileEnumerator(testDirectory, "*.sql");

		AssertExceptionThrown<NotSupportedException>(fileEnumerator.Reset);
	}

	public void TestOnlyMatchesFilesWithPattern()
	{
		var sqlFile = Path.Combine(testDirectory, "test.sql");
		var csFile = Path.Combine(testDirectory, "test.cs");

		File.WriteAllText(sqlFile, "SELECT 1");
		File.WriteAllText(csFile, "using System;");

		var fileEnumerator = new FileEnumerator(testDirectory, "*.sql");

		AssertEquals("FileEnumerator should only match files with the specified pattern.", 1, fileEnumerator.Count());
	}

	public void TestMatchesFilesInSubdirectories()
	{
		var sqlFile = Path.Combine(testDirectory, "test.sql");
		var csFile = Path.Combine(testDirectory, "test.cs");
		var subdirectory = Path.Combine(testDirectory, "subdirectory");
		var subdirectorySqlFile = Path.Combine(subdirectory, "test2.sql");

		File.WriteAllText(sqlFile, "SELECT 1");
		File.WriteAllText(csFile, "using System;");
		Directory.CreateDirectory(subdirectory);
		File.WriteAllText(subdirectorySqlFile, "SELECT 2");

		var fileEnumerator = new FileEnumerator(testDirectory, "*test*");

		AssertEquals("FileEnumerator should match files in subdirectories.", 3, fileEnumerator.Count());
	}

	public void TestFileContentIsCorrect()
	{
		var sqlFile = Path.Combine(testDirectory, "test.sql");

		File.WriteAllText(sqlFile, "SELECT 1");

		var fileEnumerator = new FileEnumerator(testDirectory, "test.sql");

		fileEnumerator.MoveNext();

		CombineAssertions(() =>
		{
			AssertEquals("SELECT 1", fileEnumerator.Current.ReadToEnd());
			fileEnumerator.MoveNext();
		});
	}

	string testDirectory;

	protected override void SetUp()
	{
		testDirectory = Path.Combine(Path.GetTempPath(), $"SchemaSyncTest{Path.GetRandomFileName()}");

		Directory.CreateDirectory(testDirectory);
	}

	protected override void TearDown()
	{
		Directory.Delete(testDirectory, true);
	}
}
