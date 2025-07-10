using System.IO;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	sealed class CASSHOTFileDataImporterTest : FlatFileDataImporterTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData()
		{
			using (StreamReader reader = new StreamReader(PathToTestFile))
			{
				CASSBilling cassBilling = new CASSBilling(Factory);
				CASSHOTFileDataImporter importer = new CASSHOTFileDataImporter(cassBilling);

				bool result = importer.ImportData(reader, PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				var adapter = new CASSHOTFileDataAdapter(new NotificationBuffer());
				adapter.Fill(cassBilling);

				int countOfAWMLines = 0;
				int countOfCCADCMLines = 0;
				foreach (CASSBillingLine line in cassBilling.Lines)
				{
					if (line.CASSCostAdjustedValue != 0)
					{
						countOfCCADCMLines++;
					}
					else
					{
						countOfAWMLines++;
					}
				}

				AssertEquals(1965, cassBilling.Lines.Count);
				AssertEquals(countOfAWMLines + countOfCCADCMLines, cassBilling.Lines.Count);
				AssertEquals(1941, countOfAWMLines);
				AssertEquals(24, countOfCCADCMLines);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithECRRecord()
		{
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", false, CASSBilling.IsRejectedClaimLinesExpected);
			using (StreamReader reader = new StreamReader(PathToTestFileWithECRRecords))
			{
				CASSBilling cassBilling = new CASSBilling(Factory);
				CASSHOTFileDataImporter importer = new CASSHOTFileDataImporter(cassBilling);

				bool result = importer.ImportData(reader, PathToTestFileWithECRRecords, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				var adapter = new CASSHOTFileDataAdapter(new NotificationBuffer());
				adapter.Fill(cassBilling);

				int countOfNonECRLines = 0;
				int countOfECRLines = 0;
				foreach (CASSBillingLine line in cassBilling.Lines)
				{
					if (line.IsRejectedClaimLine)
					{
						countOfECRLines++;
					}
					else
					{
						countOfNonECRLines++;
					}
				}

				AssertEquals(2, cassBilling.Lines.Count);
				AssertEquals(countOfNonECRLines + countOfECRLines, cassBilling.Lines.Count);
				AssertEquals(2, countOfNonECRLines);
				AssertEquals(0, countOfECRLines);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", true, CASSBilling.IsRejectedClaimLinesExpected);
			using (StreamReader reader = new StreamReader(PathToTestFileWithECRRecords))
			{
				CASSBilling cassBilling = new CASSBilling(Factory);
				CASSHOTFileDataImporter importer = new CASSHOTFileDataImporter(cassBilling);

				bool result = importer.ImportData(reader, PathToTestFileWithECRRecords, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				var adapter = new CASSHOTFileDataAdapter(new NotificationBuffer());
				adapter.Fill(cassBilling);

				int countOfNonECRLines = 0;
				int countOfECRLines = 0;
				foreach (CASSBillingLine line in cassBilling.Lines)
				{
					if (line.IsRejectedClaimLine)
					{
						countOfECRLines++;
					}
					else
					{
						countOfNonECRLines++;
					}
				}

				AssertEquals(4, cassBilling.Lines.Count);
				AssertEquals(countOfNonECRLines + countOfECRLines, cassBilling.Lines.Count);
				AssertEquals(2, countOfNonECRLines);
				AssertEquals(2, countOfECRLines);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_ImportRecords()
		{
			using (StreamReader reader = new StreamReader(PathToTestFileWithImportRecords))
			{
				CASSBilling cassBilling = new CASSBilling(Factory);
				CASSHOTFileDataImporter importer = new CASSHOTFileDataImporter(cassBilling);

				bool result = importer.ImportData(reader, PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				var adapter = new CASSHOTFileDataAdapter(new NotificationBuffer());
				adapter.Fill(cassBilling);

				int countOfIBILines = 0;
				int countOfIBOIBRLines = 0;
				foreach (CASSBillingLine line in cassBilling.Lines)
				{
					if (line.CASSCostAdjustedValue != 0)
					{
						countOfIBOIBRLines++;
					}
					else
					{
						countOfIBILines++;
					}
				}

				AssertEquals(61, cassBilling.Lines.Count);
				AssertEquals(countOfIBILines + countOfIBOIBRLines, cassBilling.Lines.Count);
				AssertEquals(61, countOfIBILines);
				AssertEquals(0, countOfIBOIBRLines);
			}
		}

		#region FlatFileDataImporterTestCase

		protected override string PathToTestFile
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\CASSBilling\Testing\" + "23470060003_HOTFILESAMPLE.HOT"; }
		}

		string PathToTestFileWithECRRecords
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\CASSBilling\Testing\" + "CNS_CLT_1004 - SMALL.fil"; }
		}

		string PathToTestFileWithImportRecords
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\CASSBilling\Testing\" + "20100801_20100815_Import.hot"; }
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			return new CASSHOTFileDataImporter(new CASSBilling(Factory));
		}

		#endregion
	}
}
