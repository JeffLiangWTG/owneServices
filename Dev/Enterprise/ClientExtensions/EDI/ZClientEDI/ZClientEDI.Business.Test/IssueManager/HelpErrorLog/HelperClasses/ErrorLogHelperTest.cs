using System.IO;
using System.Text;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	class ErrorLogHelperTest : TestCase
	{
		public void TestReadAllText()
		{
			var testCases = new (string InputFileContent, int BufSize)[]
			{
				("ABC", 1),     // len % bufSize == 0
				("ABCDE", 3),   // len % bufSize !=0
				("AB", 3),      // len < bufSize
				("ABC", 3),     // len == bufSize

				// utf8, '我' means me
				("我", 1),      // len % bufSize == 0
				("A我E", 3),    // len % bufSize !=0
				("我", 5),      // len < bufSize
				("我", 3),      // len == bufSize
			};

			foreach (var testCase in testCases)
			{
				Test(testCase.InputFileContent, testCase.BufSize, true);
				Test(testCase.InputFileContent, testCase.BufSize, false);
			}

			void Test(string inputFileContent, int bufSize, bool byteOrderMark)
			{
				// Arrange
				var tempFile = WriteToTempFile(inputFileContent, byteOrderMark);

				// Act
				var actualResult = ErrorLogHelper.ReadAllText(tempFile, bufSize);

				// Assert
				AssertEquals(inputFileContent, actualResult.ToString());
			}
		}

		public void TestTrimEnd()
		{
			var testCases = new (string InputFileContent, string ExpectedResult)[]
			{
				(string.Empty, string.Empty),
				(" ", string.Empty),
				("\n", string.Empty),
				("\n\r", string.Empty),
				("A ", "A"),
				("ABC", "ABC"),
				(" ABC", " ABC"),
				(" ABC\t", " ABC"),
			};

			foreach (var testCase in testCases)
			{
				Test(testCase.InputFileContent, testCase.ExpectedResult);
			}

			void Test(string inputFileContent, string expectedResult)
			{
				// Arrange
				var sb = new StringBuilder(inputFileContent);

				// Act
				ErrorLogHelper.TrimEnd(sb);

				// Assert
				AssertEquals(expectedResult, sb.ToString());
			}
		}

		public void TestGetNormalizedContent()
		{
			var testCases = new (string InputFileContent, string ExpectedResult)[]
			{
				("<EDI_Exception_Report><ExceptionDetails></ExceptionDetails></EDI_Exception_Report>", "<EDI_Exception_Report><ExceptionDetails></ExceptionDetails></EDI_Exception_Report>"),
				("<EDI_Exception_Report><ExceptionDetails></ExceptionDetails>", "<EDI_Exception_Report><ExceptionDetails></ExceptionDetails></EDI_Exception_Report>"),
				("<EDI_Exception_Report>123<ExceptionDetails>456</ExceptionDetails>789 ", "<EDI_Exception_Report>123<ExceptionDetails>456</ExceptionDetails></EDI_Exception_Report>"),
				("<EDI_Exception_Report>123<ExceptionDetails>456789", "<EDI_Exception_Report>123<ExceptionDetails>456789</ExceptionDetails></EDI_Exception_Report>"),

				("'&#x0;&#x0;' & '\0\0'", "'NULLENTITYNULLENTITY' & 'ASCIINULLASCIINULL'"),

				(@"<element>test\0test</eletment>", @"<element>test\0test</eletment>"),
				(@"<eletment>test\0</eletment>", "<eletment>testNULLESCAPE</eletment>"),
			};

			foreach (var testCase in testCases)
			{
				Test(testCase.InputFileContent, testCase.ExpectedResult);
				Test(testCase.InputFileContent + "\n ", testCase.ExpectedResult);
			}

			void Test(string inputFileContent, string expectedResult)
			{
				// Arrange
				var tempFile = WriteToTempFile(inputFileContent, false);

				// Act
				var actualResult = ErrorLogHelper.GetNormalizedContent(tempFile);

				// Assert
				AssertEquals(expectedResult, actualResult);
			}
		}

		public void TestGetNormalizedContentDoesNotReturnNull()
		{
			// Arrange
			var tempFile = WriteToTempFile(string.Empty, false);

			// Act
			var actualResult = ErrorLogHelper.GetNormalizedContent(tempFile);

			// Assert
			AssertEquals(string.Empty, actualResult);
		}

		string WriteToTempFile(string fileContent, bool byteOrderMark)
		{
			var tempFile = Temp.GetTempFileName(tempDir);
			WriteToFile(tempFile, fileContent, byteOrderMark);
			return tempFile;
		}

		static void WriteToFile(string filePath, string fileContent, bool byteOrderMark)
		{
			if (!byteOrderMark)
			{
				File.WriteAllText(filePath, fileContent);
				return;
			}

			WriteToFilePrependingByteOrderMark();

			void WriteToFilePrependingByteOrderMark()
			{
				var fileData = Encoding.UTF8.GetBytes(fileContent);
				var preamble = Encoding.UTF8.GetPreamble();
				using (var fs = File.OpenWrite(filePath))
				{
					fs.Write(preamble, 0, preamble.Length);
					fs.Write(fileData, 0, fileData.Length);
					fs.SetLength(fs.Position);
				}
			}
		}

		TempDirectory tempDir;
		protected override void SetUp()
		{
			base.SetUp();
			tempDir = new TempDirectory();
		}

		protected override void TearDown()
		{
			tempDir.Dispose();
			base.TearDown();
		}
	}
}
