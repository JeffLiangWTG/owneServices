using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.Testing.ComplianceReport.JPKV7M
{
	public class JPKV7MFileWriterTest : JPKTestBase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2023, 10, 2, 12, 34, 56)]
		public void TestWriteOutputFile_NoTransactions()
		{
			WriteAndAssertJPK("2023-10-02T123456_JPK_VAT.xml",
				@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\JPKV7M\TestFiles\JPKV7M_Header.xml");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2023, 10, 2, 12, 35, 57)]
		public void TestWriteOutputFile_ARTransactions()
		{
			SetupTaxRates();
			SetupDebtorsAndCreditors();
			SetupARTransactions();

			WriteAndAssertJPK("2023-10-02T123557_JPK_VAT.xml",
				@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\JPKV7M\TestFiles\JPKV7M_ARTransactions.xml");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2023, 10, 2, 12, 36, 58)]
		public void TestWriteOutputFile_APTransactions()
		{
			SetupTaxRates();
			SetupDebtorsAndCreditors();
			SetupAPTransactions();

			WriteAndAssertJPK("2023-10-02T123658_JPK_VAT.xml",
				@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\JPKV7M\TestFiles\JPKV7M_APTransactions.xml");
		}

		void WriteAndAssertJPK(string expectedOutputFileName, string expectedXmlFileName)
		{
			var logger = new LoggerForTest();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Poland))
			{
				var fileWriter = new JPKV7MFileWriter(Report, logger);

				using (var tempDirectory = new TempDirectory())
				{
					// Act
					var filePath = fileWriter.WriteOutputFile(tempDirectory);

					// Assert
					AssertContains("Filename", expectedOutputFileName, filePath);

					var filePathOfExpectedXML = BaseSourcePath + expectedXmlFileName;
					var expected = File.ReadAllText(filePathOfExpectedXML);
					using (var fileStream = new FileStream(filePath, FileMode.Open))
					{
						var actual = fileStream.WriteToString().Trim();
						this.AssertXMLEqualsByDiff(expected, actual);
					}
				}
			}

			// Assert
			var actualLogs = logger.ToString();
			AssertContains("Generated JPK_V7M file:", actualLogs);
			AssertContains(expectedOutputFileName, actualLogs);
		}

		[TestDate(2023, 10, 2, 12, 36, 58)]
		public void TestWriterThrowsExceptionOnWrongReportConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.FirstOrDefault() as ComplianceReportConfiguration;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;

			SetupTaxRates();
			SetupDebtorsAndCreditors();
			SetupARTransactions();

			var logger = new LoggerForTest();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Poland))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig))
			{
				var config = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).Cast<ComplianceReportConfiguration>();
				var fileWriter = new JPKV7MFileWriter(Report, logger);

				using (var tempDirectory = new TempDirectory())
				{
					AssertExceptionThrown(typeof(NotSupportedException), @"Unable to generate JPK output file.
Please, make sure the report's queue and line records were generated with valid configuration for JPK report.", () => fileWriter.WriteOutputFile(tempDirectory));
				}
				var actualLogs = logger.ToString();
				AssertEquals(@"Loading Compliance Report Data...
Building JPKV7M XML...
Starting generation main part of JPKV7M XML...
Generating main part JPKV7M XML...", actualLogs);
			}
		}

		[TestDate(2023, 10, 2, 12, 34, 56)]
		public void TestWriteOutputFile_Header_NIP()
		{
			AssertEquals("9876 543 210", Report.Company.GC_BusinessRegNo);

			var doc = WriteAndGetXmlDocument("2023-10-02T123456_JPK_VAT.xml");
			var nip = doc.GetElementsByTagName("NIP");
			AssertEquals(1, nip.Count);
			AssertEquals("GC_BusinessRegNo without spaces", "9876543210", nip[0].InnerText);

			Report.Company.GC_BusinessRegNo = ZString.Empty;
			Report.Factory.Save();

			doc = WriteAndGetXmlDocument("2023-10-02T123456_JPK_VAT.xml");
			nip = doc.GetElementsByTagName("NIP");
			AssertEquals(1, nip.Count);
			AssertEquals("Fallback for no GC_BusinessRegNo and PTU", "0000000000", nip[0].InnerText);

			Creator.CreateCustomsCodes(Report.Company.OrgProxy, CountryCodes.Poland, "PTU", "147 154 678");
			Report.Factory.Save();

			doc = WriteAndGetXmlDocument("2023-10-02T123456_JPK_VAT.xml");
			nip = doc.GetElementsByTagName("NIP");
			AssertEquals(1, nip.Count);
			AssertEquals("Fallback to PTU without spaces", "147154678", nip[0].InnerText);
		}

		[TestDate(2023, 10, 2, 12, 39, 59)]
		public void TestWriteOutputFile_Totals()
		{
			SetupTaxRates();
			SetupDebtorsAndCreditors();
			SetupARTransactions();
			SetupAPTransactions();

			var doc = WriteAndGetXmlDocument("2023-10-02T123959_JPK_VAT.xml");

			var totals = new Dictionary<string, decimal>
			{
				{ "K_10", decimal.Zero },
				{ "K_11", decimal.Zero },
				{ "K_12", decimal.Zero },
				{ "K_13", decimal.Zero },
				{ "K_15", decimal.Zero },
				{ "K_16", decimal.Zero },
				{ "K_17", decimal.Zero },
				{ "K_18", decimal.Zero },
				{ "K_19", decimal.Zero },
				{ "K_20", decimal.Zero },
				{ "K_23", decimal.Zero },
				{ "K_24", decimal.Zero },
				{ "K_27", decimal.Zero },
				{ "K_28", decimal.Zero },
				{ "K_29", decimal.Zero },
				{ "K_30", decimal.Zero },
				{ "K_42", decimal.Zero },
				{ "K_43", decimal.Zero },
				{ "P_10", decimal.Zero },
				{ "P_11", decimal.Zero },
				{ "P_12", decimal.Zero },
				{ "P_13", decimal.Zero },
				{ "P_15", decimal.Zero },
				{ "P_16", decimal.Zero },
				{ "P_17", decimal.Zero },
				{ "P_18", decimal.Zero },
				{ "P_19", decimal.Zero },
				{ "P_20", decimal.Zero },
				{ "P_23", decimal.Zero },
				{ "P_24", decimal.Zero },
				{ "P_27", decimal.Zero },
				{ "P_28", decimal.Zero },
				{ "P_29", decimal.Zero },
				{ "P_30", decimal.Zero },
				{ "P_37", decimal.Zero },
				{ "P_38", decimal.Zero },
				{ "P_39", decimal.Zero },
				{ "P_42", decimal.Zero },
				{ "P_43", decimal.Zero },
				{ "P_48", decimal.Zero },
				{ "P_51", decimal.Zero },
				{ "P_53", decimal.Zero },
				{ "P_54", decimal.Zero },
				{ "PodatekNalezny", decimal.Zero },
				{ "PodatekNaliczony", decimal.Zero },
			};

			foreach (var key in totals.Keys.ToArray())
			{
				totals[key] = doc.GetElementsByTagName(key).Cast<XmlNode>().Select(x => decimal.Parse(x.InnerText)).Sum();
			}

			CombineAssertions(() =>
			{
				AssertEquals("P_37 from other P_", totals["P_10"] + totals["P_11"] + totals["P_13"] + totals["P_15"] + totals["P_17"] + totals["P_19"] + totals["P_23"] + totals["P_27"] + totals["P_29"], totals["P_37"]);
				AssertEquals("P_37", 1505m, totals["P_37"]);

				AssertEquals("P_38 from other P_", totals["P_16"] + totals["P_18"] + totals["P_20"] + totals["P_24"] + totals["P_28"] + totals["P_30"], totals["P_38"]);
				AssertEquals("P_38", 172m, totals["P_38"]);

				AssertEquals("P_48 == P_43", totals["P_43"], totals["P_48"]);
				AssertEquals("P_48", 114m, totals["P_48"]);

				AssertEquals("PodatekNaliczony == Total K_43", totals["K_43"], totals["PodatekNaliczony"]);
				AssertEquals("PodatekNaliczony", 113.58m, totals["PodatekNaliczony"]);

				AssertEquals("PodatekNalezny from K_", totals["K_16"] + totals["K_18"] + totals["K_20"] + totals["K_24"] + totals["K_28"] + totals["K_30"], totals["PodatekNalezny"]);
				AssertEquals("PodatekNalezny", 173.35m, totals["PodatekNalezny"]);

				foreach (var key in totals.Keys.Where(x => x.StartsWith("K_")).ToArray())
				{
					var pKey = "P_" + key.Split('_')[1];
					AssertEquals(pKey + " == Rounded " + key, decimal.Round(totals[key], System.MidpointRounding.AwayFromZero), totals[pKey]);
				}
			});
		}

		XmlDocument WriteAndGetXmlDocument(string expectedOutputFileName)
		{
			var result = new XmlDocument();

			var logger = new LoggerForTest();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Poland))
			{
				var fileWriter = new JPKV7MFileWriter(Report, logger);

				using (var tempDirectory = new TempDirectory())
				{
					// Act
					var filePath = fileWriter.WriteOutputFile(tempDirectory);

					// Assert
					AssertContains("Filename", expectedOutputFileName, filePath);

					result.Load(filePath);
				}
			}

			// Assert
			var actualLogs = logger.ToString();
			AssertContains("Generated JPK_V7M file:", actualLogs);
			AssertContains(expectedOutputFileName, actualLogs);

			return result;
		}
	}
}
