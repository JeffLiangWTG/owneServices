using System;
using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class DbFileCheckerTest : TestCase
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestDbFileChecker_EmptyFilename()
		{
			new DbFileChecker("");
		}

		public void TestIsDbInFile()
		{
			string filename = Path.Combine(TempForTest.TempPath, "DBFilename.txt");
			DbFileChecker checker = new DbFileChecker(filename);
			Assert("File 'DBFilename.txt' should not exist", !checker.IsDbInFile("Servername", "DatabaseName"));
			using (StreamWriter writer = File.AppendText(filename))
			{
				writer.WriteLine("  .  , DataBASeName  ");
				writer.WriteLine(" Server , DataName");
			}

			Assert(!checker.IsDbInFile("Servername", "DatabaseName"));
			Assert(checker.IsDbInFile(" . ", " DATABASENAme "));
			Assert(checker.IsDbInFile(" SERVER  ", " DATAname "));
			DeleteIfExists(filename);
		}
	}
}
