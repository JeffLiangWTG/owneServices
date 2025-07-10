using System.IO;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class FileListFormatterTest : TestCaseWithDocumentFactory
	{
		public void TestFormatListToString()
		{
			string file1 = Path.Combine(Env.TempPath, @"test\document1.txt");
			string file2 = Path.Combine(Env.TempPath, @"test\document2.txt");

			string[] filenames = new string[] { file1, file2 };

			string formattedList = new FileListFormatter().FormatListToString(filenames, true);
			AssertEquals("List contents",
				string.Format("document1.txt{0}document2.txt{0}", System.Environment.NewLine),
				formattedList);
		}

		public void TestFormatListToStringWithFullPathNames()
		{
			string file1 = Path.Combine(Env.TempPath, @"test\document1.txt");
			string file2 = Path.Combine(Env.TempPath, @"test\document2.txt");

			string[] filenames = new string[] { file1, file2 };

			string formattedList = new FileListFormatter().FormatListToString(filenames, false);
			AssertEquals("List contents",
				string.Format(file1 + "{0}" + file2 + "{0}", System.Environment.NewLine),
				formattedList);
		}

		[ExpectNoExceptions]
		public void TestFormatListToStringWithFileNameContainsInvalidCharacter()
		{
			string file = Env.TempPath + @"test\t<>|e?st.txt";
			string[] fileNames = new string[] { file };

			string formattedList = new FileListFormatter().FormatListToString(fileNames, true);
			AssertEquals("List contents",
				string.Format("t___e?st.txt{0}", System.Environment.NewLine),
				formattedList);
		}
	}
}
