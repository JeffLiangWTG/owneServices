using System;
using System.IO;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class StreamExtensionsTest : NUnit.Framework.TestCase
	{
		[UseSnapshotProtection]
		public void TestCopyToFileViaFlexCel()
		{
			using (var tempFile = TempFile.New())
			{
				AssertEquals("File should be empty", 0, new FileInfo(tempFile.Filename).Length);

				using (var stream = new ControllableReadStream(0, new byte[] { 1, 2, 3, 56 }))
				{
					AssertExceptionThrown<ExcelInterfaceException>(() => stream.CopyToFileViaFlexCel(tempFile.Filename));
				}

				var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
					@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Whatever]
{A}-[#EndOfReport]");

				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, excelTemplate))
				{
					using (var stream = new MemoryStream())
					{
						report.Save(stream);
						stream.CopyToFileViaFlexCel(tempFile.Filename);

						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(tempFile.Filename);
							AssertEquals("{B}-[Whatever]", excelInterface.WorkSheets[0].ToString());

							excelInterface.LoadExcelFile(stream);
							AssertEquals("{B}-[Whatever]", excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		public void TestCopyToFileFromUnreadableStream()
		{
			using (TempFile tempFile = TempFile.New())
			{
				using (ControllableReadStream stream = new ControllableReadStream(0, new byte[] { 1, 2, 3, 56 }))
				{
					AssertExceptionThrown(typeof(InvalidOperationException), "Could not copy data from stream.", delegate
					{
						stream.CopyToFile(tempFile.Filename);
					});
				}
			}
		}

		public void TestCopyToFile1ByteAtATime()
		{
			AssertCopyToFile(1);
		}

		public void TestCopyToFile2BytesAtATime()
		{
			AssertCopyToFile(2);
		}

		public void TestCopyToFile3BytesAtATime()
		{
			AssertCopyToFile(3);
		}

		public void TestCopyToFile4BytesAtATime()
		{
			AssertCopyToFile(4);
		}

		public void TestCopyToFile5BytesAtATime()
		{
			AssertCopyToFile(5);
		}

		public void TestCopyToByteArray1ByteAtATime()
		{
			AssertCopyToByteArray(1);
		}

		public void TestCopyToByteArray2BytesAtATime()
		{
			AssertCopyToByteArray(2);
		}

		public void TestCopyToByteArray3BytesAtATime()
		{
			AssertCopyToByteArray(3);
		}

		public void TestCopyToByteArray4BytesAtATime()
		{
			AssertCopyToByteArray(4);
		}

		public void TestCopyToByteArray5BytesAtATime()
		{
			AssertCopyToByteArray(5);
		}

		public void TestCopyToByteArray0BytesAtATimeReturnsNull()
		{
			using (ControllableReadStream stream = new ControllableReadStream(0, new byte[] { 1, 2, 3, 56 }))
			{
				byte[] byteArray = stream.CopyToByteArray();
				AssertNull("stream.CopyToByteArray()", byteArray);
			}
		}

		#region Implementation

		static void AssertCopyToByteArray(int byteReadCount)
		{
			using (ControllableReadStream stream = new ControllableReadStream(byteReadCount, new byte[] { 1, 2, 3, 56 }))
			{
				byte[] byteArray = stream.CopyToByteArray();
				AssertNotNull("stream.CopyToByteArray()", byteArray);
				AssertByteArrayContents("1, 2, 3, 56", byteArray);
			}
		}

		static void AssertByteArrayContents(string expectedContents, byte[] actualArray)
		{
			ZStringBuilder actualContents = new ZStringBuilder();
			foreach (int value in actualArray)
			{
				actualContents.Append(value.ToString());
			}
			AssertEquals("Content of returned ByteArray", expectedContents, actualContents.ToStringWithDelimiterBetweenAppends(", "));
		}

		static void AssertCopyToFile(int byteReadCount)
		{
			using (TempFile tempFile = TempFile.New())
			{
				using (ControllableReadStream stream = new ControllableReadStream(byteReadCount, new byte[] { 1, 2, 3, 56 }))
				{
					stream.CopyToFile(tempFile.Filename);
				}
				byte[] fileContents = File.ReadAllBytes(tempFile.Filename);
				AssertByteArrayContents("1, 2, 3, 56", fileContents);
			}
		}

		#endregion
	}
}
