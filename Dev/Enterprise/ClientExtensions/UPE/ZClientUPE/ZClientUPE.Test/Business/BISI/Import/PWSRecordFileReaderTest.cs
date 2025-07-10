using System;
using System.Collections;
using System.IO;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class PWSRecordFileReaderTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestReadInvalidFile()
		{
			using (var reader = new PWSRecordFileReader("FileDoNotExistMehMeh.txt", NotificationSubscriber))
			{
				AssertEquals("There should be 1 notification being reported", 1, NotificationSubscriber.Events.Length);
				Assert(((ErrorNotification)NotificationSubscriber.Events[0]).AdditionalInfo.StartsWith("Cannot read PWS File. "));
				foreach (var record in reader)
				{
					_ = record; // to keep the compiler happy
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsValid()
		{
			using (var reader = new PWSRecordFileReader("FileDoNotExistMehMeh.txt", NotificationSubscriber))
			{
				AssertEquals("Should not be valid", false, reader.IsValid);
			}

			using (var reader = new PWSRecordFileReader(CODFilePath, NotificationSubscriber))
			{
				AssertEquals("Should be valid", true, reader.IsValid);
			}
		}

		public void TestReadEmptyFile()
		{
			var testFilePath = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			try
			{
				using (File.Create(testFilePath))
				{
				}

				using (var reader = new PWSRecordFileReader(testFilePath, NotificationSubscriber))
				{
					foreach (var record in reader)
					{
						object notUsed = record; // to keep the compiler happy
						Fail("There should not be any records in the empty file");
					}
				}

				AssertEquals("There should be no errors or warnings", 0, NotificationSubscriber.Events.Length);
			}
			finally
			{
				File.Delete(testFilePath);
			}
		}

		public void TestStreamReaderIsClosedOnDispose()
		{
			string tempFileName = null;
			try
			{
				tempFileName = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());
				using (var writer = new StreamWriter(tempFileName))
				{
					writer.Write("MEH MEH");
				}

				PWSRecordFileReaderForTest pWSReader = null;
				try
				{
					pWSReader = new PWSRecordFileReaderForTest(tempFileName, NotificationSubscriber);
					AssertNotNull("Reader should be instantiated in the constructor", pWSReader.Reader);
					AssertEquals("Content is not as expected", "MEH MEH", pWSReader.Reader.ReadToEnd());
					Assert(pWSReader.Reader.BaseStream.CanRead);
				}
				finally
				{
					if (pWSReader != null)
					{
						pWSReader.Dispose();
					}
				}

				AssertNull("Should be closed on Dispose()", pWSReader.Reader.BaseStream);
			}
			finally
			{
				if (tempFileName != null)
				{
					File.Delete(tempFileName);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEnumerator()
		{
			using (var reader = new PWSRecordFileReader(CODFilePath, NotificationSubscriber))
			{
				var allRecords = new ArrayList();
				foreach (var record in reader)
				{
					allRecords.Add(record);
				}

				AssertEquals("There should be no errors or warnings", 0, NotificationSubscriber.Events.Length);
				AssertEquals("All records should be enumerated", 52, allRecords.Count);
				Assert("First record should be a header", allRecords[0] is PWSHeaderRecord);
				Assert("50th record should be TotalDataArea record", allRecords[49] is PWSTotalDataAreaRecord);
				Assert("51st record should be TotalDataArea record", allRecords[50] is PWSTotalDataAreaRecord);
				Assert("Last record should be the trailer", allRecords[51] is PWSTrailerRecord);
				var expectedRecord = PWSTotalDataAreaRecord.New(RecordContent1, NotificationSubscriber);
				AssertTotalDataAreaContent(expectedRecord, allRecords[49] as PWSTotalDataAreaRecord);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEnumerator_SomeBlankLines()
		{
			using (var reader = new PWSRecordFileReader(UPETestHelper.TestFiles.BISI.Import.Folder + "CODFileWithBlankLines.txt", NotificationSubscriber))
			{
				var allRecords = new ArrayList();
				foreach (var record in reader)
				{
					allRecords.Add(record);
				}

				AssertEquals("There should be no errors or warnings", 0, NotificationSubscriber.Events.Length);
				AssertEquals("All records should be enumerated", 52, allRecords.Count);
				Assert("First record should be a header", allRecords[0] is PWSHeaderRecord);
				Assert("50th record should be TotalDataArea record", allRecords[49] is PWSTotalDataAreaRecord);
				Assert("51st record should be TotalDataArea record", allRecords[50] is PWSTotalDataAreaRecord);
				Assert("Last record should be the trailer", allRecords[51] is PWSTrailerRecord);
				var expectedRecord = PWSTotalDataAreaRecord.New(RecordContent1, NotificationSubscriber);
				AssertTotalDataAreaContent(expectedRecord, allRecords[49] as PWSTotalDataAreaRecord);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEnumerator_OutOfSyncTotalDataArea()
		{
			using (var reader = new PWSRecordFileReader(UPETestHelper.TestFiles.BISI.Import.Folder + "CODFileWithOutOfSyncTotalDataArea.txt", NotificationSubscriber))
			{
				var allRecords = new ArrayList();
				foreach (var record in reader)
				{
					allRecords.Add(record);
				}

				AssertEquals("There should be warnings", 3, NotificationSubscriber.Events.Length);
				AssertEquals("Line 11 to 132. Invalid Total Data Area Structure", ((WarningNotification)NotificationSubscriber.Events[0]).AdditionalInfo);
				AssertEquals("Line 173. Detail Record less than 43 characters", ((WarningNotification)NotificationSubscriber.Events[1]).AdditionalInfo);
				AssertEquals("Line 173 to 198. Invalid Total Data Area Structure", ((WarningNotification)NotificationSubscriber.Events[2]).AdditionalInfo);
				AssertEquals("All records should be enumerated", 5, allRecords.Count);
				Assert("First record should be a header", allRecords[0] is PWSHeaderRecord);
				AssertNotNull("2nd to 4th record should be TotalDataArea record", allRecords.GetRange(1, 3).ToArray(typeof(PWSTotalDataAreaRecord)));
				Assert("Last record should be the trailer", allRecords[4] is PWSTrailerRecord);
				AssertEquals("WAA25665589", (allRecords[1] as PWSTotalDataAreaRecord).WayBillNumber);
				AssertEquals("1BB98E686640623521", (allRecords[2] as PWSTotalDataAreaRecord).WayBillNumber);
				AssertEquals("1CC98E686640623521", (allRecords[3] as PWSTotalDataAreaRecord).WayBillNumber);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEnumerator_UnknownRecords()
		{
			using (var reader = new PWSRecordFileReader(UPETestHelper.TestFiles.BISI.Import.Folder + "CODFileWithUnknownRecords.txt", NotificationSubscriber))
			{
				if (NotificationSubscriber.HasErrors)
				{
					Fail(NotificationSubscriber.AsString);
				}

				var allRecords = new ArrayList();
				foreach (var record in reader)
				{
					allRecords.Add(record);
				}

				AssertEquals("There should be 73 records", 73, allRecords.Count);
				AssertNotNull("Should be listed as unknown records", allRecords.ToArray(typeof(PWSUnknownRecord)));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEnumerator_WithTrimmedDataLines()
		{
			using (var reader = new PWSRecordFileReader(UPETestHelper.TestFiles.BISI.Import.Folder + "CODFileWithTrimmedDataLines.txt", NotificationSubscriber))
			{
				var allRecords = new ArrayList();
				foreach (var record in reader)
				{
					allRecords.Add(record);
				}

				AssertEquals("There should be no errors or warnings; " + NotificationSubscriber.AsString, 0, NotificationSubscriber.Events.Length);
				AssertEquals("All records should be enumerated", 52, allRecords.Count);
				Assert("First record should be a header", allRecords[0] is PWSHeaderRecord);
				Assert("50th record should be TotalDataArea record", allRecords[49] is PWSTotalDataAreaRecord);
				Assert("51st record should be TotalDataArea record", allRecords[50] is PWSTotalDataAreaRecord);
				Assert("Last record should be the trailer", allRecords[51] is PWSTrailerRecord);
				var expectedRecord = PWSTotalDataAreaRecord.New(RecordContent1, NotificationSubscriber);
				AssertTotalDataAreaContent(expectedRecord, allRecords[49] as PWSTotalDataAreaRecord);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReset()
		{
			using (var reader = new PWSRecordFileReader(CODFilePath, NotificationSubscriber))
			{
				if (NotificationSubscriber.HasErrors)
				{
					Fail(NotificationSubscriber.AsString);
				}

				var enumerator = reader.GetEnumerator();
				AssertNull("Current record should not be available yet", enumerator.Current);
				enumerator.MoveNext();
				AssertNotNull("First record should be header", enumerator.Current is PWSHeaderRecord);
				enumerator.MoveNext();
				AssertNotNull("Current record should be an unknown record", enumerator.Current is PWSUnknownRecord);
				enumerator.Reset();
				AssertNull("Current record should not be available yet", enumerator.Current);
				enumerator.MoveNext();
				AssertNotNull("After reset, should now return the header record again", enumerator.Current is PWSHeaderRecord);
			}
		}

		void AssertTotalDataAreaContent(PWSTotalDataAreaRecord expectedRecord, PWSTotalDataAreaRecord generatedRecord)
		{
			var recordDataField = typeof(PWSTotalDataAreaRecord).GetField("RecordData", BindingFlags.NonPublic | BindingFlags.Instance);
			var expected = (ZString)recordDataField.GetValue(expectedRecord);
			var generated = (ZString)recordDataField.GetValue(generatedRecord);
			AssertEquals("Content is not as expected", expected, generated);
		}

		NotificationBuffer notificationSubscriber;
		NotificationBuffer NotificationSubscriber
		{
			get
			{
				if (notificationSubscriber == null)
				{
					notificationSubscriber = new NotificationBuffer();
				}

				return notificationSubscriber;
			}
		}

		string CODFilePath => UPETestHelper.TestFiles.BISI.Import.Folder + "CODFile.txt";

		PWSDetailRecord[] RecordContent1
		{
			get
			{
				return new PWSDetailRecord[] { new PWSDetailRecord("020W6625665589                        0001BW6625665589W6625665589                        00000062139709MAY2002      A59935    8SG0157203CREATIVE PRODUCTS CORP.      02562", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0002B        6/F PEAKSON BLDG. 1505 PRENCETON   ST.,CORNER SHAW                    MANDALUYONG                 1552      PHPH       ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0003BILIPPINES                      AIRTROPOLIS EXPRESS (S) PTE LTD    9 AIRLINE RD UNIT #01-20 (WRHS)    & UNIT 04-17 (OFC)        ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0004BCARGO AGENT     SINGAPORE                   819827    65 5431377     SGSINGAPORE      10MAY2002                                ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0005B           WW EXPRESS     X                     11.5                                        KGSA                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0006B           1    NON-DOCUMENTCYBORG KURO CHAN BETACAM                190.54           SGDIM2E005828 1.808000000                 ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0007B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0008B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0009B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0010B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0011B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0012B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0013B      140.10CASH ONLY           004GST-WAIVED (LOW CIF/FTZ)           005406 7279 9263                      00610052002        ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0014B                          00719                                 009UPS02051019                        0107PCS                  ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0015B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0016B                                                    FUEL SURCHARGE                                    1.03                     ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0017B                1.03FREIGHT                                         137.00                            137.00THAILAND'S E       ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0018BXPORT                                 2.07                              2.07                                                   ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0019B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0020B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0021B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0022B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0023B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0024B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0025B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0026B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0027B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0028B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0029B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0030B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0031B                                                                                                                               ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0032B                                                                    10MAY2002                                                  ", NotificationSubscriber), new PWSDetailRecord("020W6625665589                        0033D                                                                                                                   SGD         ", NotificationSubscriber) };
			}
		}

		sealed class PWSRecordFileReaderForTest : PWSRecordFileReader
		{
			public PWSRecordFileReaderForTest(string fileName, INotifications notificationSubscriber) : base(fileName, notificationSubscriber)
			{
			}

			internal new StreamReader Reader => base.Reader;
		}
	}
}
