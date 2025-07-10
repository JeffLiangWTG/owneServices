using System.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ExceptionReportTest : TestCase
	{
		public void TestHtmlFileName()
		{
			AssertNotNull(Report.HtmlFileName);
			AssertEquals(Report.HtmlFileName, new ExceptionReport(null).HtmlFileName);
		}

		public void TestTextFileName()
		{
			string file1 = Report.TextFileName;
			try
			{
				string file2 = new ExceptionReport(null).TextFileName;
				try
				{
					AssertNotNull(file1);
					Assert(file1 != file2);
				}
				finally
				{
					File.Delete(file2);
				}
			}
			finally
			{
				File.Delete(file1);
			}
		}

		public void TestCreateHtmlFile()
		{
			string fileName = Report.HtmlFileName;
			File.Delete(fileName);
			Assert(!File.Exists(fileName));

			Report.CreateHtmlFile();

			try
			{
				string fileContents = GetFileContents(fileName);
				Assert(fileContents.EndsWith(Report.Xml));
				Assert(fileContents != Report.Xml);
				Assert(fileContents.Contains("exception-report-test ひらがな"));
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		public void TestCreateTextFile()
		{
			string fileName = Report.TextFileName;
			File.Delete(fileName);
			Assert(!File.Exists(fileName));

			Report.CreateTextFile();
			try
			{
				AssertEquals(Report.Xml, GetFileContents(fileName));
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		ExceptionReport Report;

		protected override void SetUp()
		{
			Report = new ExceptionReport("<exception-report-test ひらがな/>");
		}

		string GetFileContents(string filePath)
		{
			string result = "";

			if (File.Exists(filePath))
			{
				using (StreamReader reader = File.OpenText(filePath))
				{
					result = reader.ReadToEnd();
				}
			}

			return result;
		}
	}
}
