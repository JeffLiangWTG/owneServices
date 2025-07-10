using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSAdjustmentFileConverterTest : TestCaseWithFactory
	{
		[TestDate(2015, 11, 05)]
		public void TestConverter()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				var header = CASSAdjustmentFileTestHelper.GetCASSAdjustmentHeader(Factory);
				var converter = new CASSAdjustmentFileConverter(notificationBuffer, Factory);
				converter.ExportFlatFile(header, new CASSAdjustmentFileFormat(), writer);

				testStream.Position = 0;
				var stream = new StreamReader(testStream);
				var lines = stream.ReadToEnd();
				var expectedLines = @"AA10203040506200816                                                                                                                                                                                                                                       
AW1601020304050600144154      000002500025000000003000000000180880000000000000000000000000000000000000000000000000000000002000000000001000000000132090+0011900K10                    0000 I want to test                                                                                                                                                                                                         
DO1721020304050667828073808640000004500025000000000000000000021618000000000000000000000000000000000000000000000000000000000200000000000300000000000400+0002150K11                    0000 I want to test again                                                                                                                                                                                                   
TT0000002                                                                                                                                                                                                                                                 
";
				AssertMultilineASCIIEquals("File Lines", expectedLines, lines);
			}
		}

		public void TestConverter_ISOMinorUnitRatio()
		{
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				var header = CASSAdjustmentFileTestHelper.GetCASSAdjustmentHeader(Factory, currency: "TWD");
				var converter = new CASSAdjustmentFileConverter(notificationBuffer, Factory);
				converter.ExportFlatFile(header, new CASSAdjustmentFileFormat(), writer);

				testStream.Position = 0;
				var stream = new StreamReader(testStream);
				var lines = stream.ReadToEnd();
				var expectedLines = @"AA10203040506200816                                                                                                                                                                                                                                       
AW1601020304050600144154      000002500025000000003000000000180880000000000000000000000000000000000000000000000000000000002000000000001000000000132090+0011900K10                    0000 I want to test                                                                                                                                                                                                         
DO1721020304050667828073808640000004500025000000000000000000021618000000000000000000000000000000000000000000000000000000000200000000000300000000000400+0002150K11                    0000 I want to test again                                                                                                                                                                                                   
TT0000002                                                                                                                                                                                                                                                 
";
				AssertMultilineASCIIEquals("File Lines", expectedLines, lines);
			}
		}

		[TestDate(2018, 01, 01)]
		public void TestCCANumberIsExportedToAdjustmentFileMaintainingFixedLengthBehaviour()
		{
			var cCADCM1 = new { Number = "C12345", ExpectedInAdjFile = "C12345" };
			var cCADCM2 = new { Number = "C1234", ExpectedInAdjFile = "C1234 " };
			var cCADCM3 = new { Number = "C234  ", ExpectedInAdjFile = "C234  " };
			var cCADCM4 = new { Number = "  C234", ExpectedInAdjFile = "  C234" };
			var cCADCM5 = new { Number = "", ExpectedInAdjFile = "      " };

			foreach (var cCADCM in new[] { cCADCM1, cCADCM2, cCADCM3, cCADCM4, cCADCM5 })
			{
				var testStream = new MemoryStream();
				using (StreamWriter writer = new StreamWriter(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();

					var billing = CASSAdjustmentFileTestHelper.GetFullyPopulatedCASS(Factory);
					billing.CostHeader.ExportLines[0].CCADCMNumber = cCADCM.Number;

					var header = CASSAdjustmentFileTestHelper.GetCASSAdjustmentHeader(Factory, billing);

					var converter = new CASSAdjustmentFileConverter(notificationBuffer, Factory);
					converter.ExportFlatFile(header, new CASSAdjustmentFileFormat(), writer);

					testStream.Position = 0;
					var stream = new StreamReader(testStream);
					var lines = stream.ReadToEnd();
					var expectedLines = $@"AA10203040506200816                                                                                                                                                                                                                                       
AW1601020304050600144154{cCADCM.ExpectedInAdjFile}000002500025000000003000000000180880000000000000000000000000000000000000000000000000000000002000000000001000000000132090+0011900K10                    0000 I want to test                                                                                                                                                                                                         
DO1721020304050667828073808640000004500025000000000000000000021618000000000000000000000000000000000000000000000000000000000200000000000300000000000400+0002150K11                    0000 I want to test again                                                                                                                                                                                                   
TT0000002                                                                                                                                                                                                                                                 
";
					AssertMultilineASCIIEquals("File Lines", expectedLines, lines);
				}
			}
		}
	}
}
