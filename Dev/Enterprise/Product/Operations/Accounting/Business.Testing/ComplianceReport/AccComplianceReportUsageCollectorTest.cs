using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport
{
	public class AccComplianceReportUsageCollectorTest : TestCaseWithFactory
	{
		[TestDate(2023, 08, 11, 16, 27, 15)]
		public void TestReportProperties()
		{
			SetupDayBookReportConfiguration();
			SetupACRForDefaultCountry();
			SetupConfigForDefaultCountry();
			SetupTestData();
			var complianceReport = SetupDayBookComplianceReport();

			var stopWatchMock = new Mock<IStopwatch>();
			stopWatchMock.Setup(x => x.ElapsedMilliseconds).Returns(1234);
			using (ObjectFactory.Substitute(stopWatchMock.Object))
			{
				var usageCollector = GetRunAndDisposeUsageCollector(complianceReport);

				var acrReport = usageCollector.ReturnReportProperties();
				AssertEquals("ReportProperties - wrong number of ACR entries", acrExpectedAll.Count, acrReport?.Count());
				var actualAsString = JsonConvert.SerializeObject(acrReport, Formatting.Indented);
				var expectedAsString = JsonConvert.SerializeObject(acrExpectedAll, Formatting.Indented);
				AssertMultilineASCIIEquals("ReportProperties ACR - wrong names and/or values.", expectedAsString, actualAsString);

				var configReport = (List<(string name, object value)>)acrReport.FirstOrDefault(x => x.name == "ReportConfiguration").value;
				var eDocsReport = (List<(string name, object value)>)acrReport.FirstOrDefault(x => x.name == "eDocs").value;
				AssertVersionNumberIsCorrect(acrReport.Count(), configReport.Count, eDocsReport.Count, (ZString)acrReport.First(x => x.name == "ACRUsageCollectorVersion").value);
			}

			void AssertVersionNumberIsCorrect(int acrCount, int configCount, int eDocsCount, ZString version)
			{
				CombineAssertions("New properties detected without version change. When adding new properties to the usage collector, you must increment the version number as well.",
					() =>
					{
						AssertEquals("Wrong ACR usage collector version", "1.0", version);
						AssertEquals("Wrong number of ACR properties for this version", 20, acrCount);
						AssertEquals("Wrong number of Configuration properties for this version", 17, configCount);
						AssertEquals("Wrong number of eDocs properties for this version", 2, eDocsCount);
					});
			}
		}

		[TestDate(2023, 08, 11, 06, 27, 15)]
		public void TestReportPropertiesInEDIMessageTable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				SetupFECReportConfiguration();
				SetupACRForFrance();
				SetupConfigForFrance();
				SetupTestData();
				var complianceReport = SetupFECComplianceReport();

				var stopWatchMock = new Mock<IStopwatch>();
				stopWatchMock.Setup(x => x.ElapsedMilliseconds).Returns(1234);
				using (ObjectFactory.Substitute(stopWatchMock.Object))
				{
					var usageCollector = GetRunAndDisposeUsageCollector(complianceReport);

					// test usage data written to DATABASE
					var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
					AssertEquals("Wrong number of rows in EDIMessage table.", 1, ediMessages.Length);

					var jObjects = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail));
					var jObject = jObjects.First();

					var acrValue = jObject.Properties().FirstOrDefault(kp => kp.Name.Equals("ACR", StringComparison.InvariantCulture))?.Value;
					AssertNotNull("ACR is missing", acrValue);

					if (acrValue is JObject acrJObject)
					{
						var acrValueString = (ZString)JsonConvert.SerializeObject(acrValue, Formatting.Indented);
						var acrExpectedAsString = GetEmbeddedResourceAsZString("AccComplianceReportUsageCollectorReportProperties.txt");
						acrExpectedAsString = acrExpectedAsString.TrimEnd(new[] { '\r', '\n' });
						AssertMultilineASCIIEquals("Wrong value in EDIMessage table.", acrExpectedAsString, acrValueString);
					}
				}
			}
		}

		[TestDate(2023, 08, 11, 06, 27, 15)]
		public void TestReportPropertiesInEDIMessageTableWithEDocs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				SetupFECReportConfiguration();
				SetupACRForFrance();
				SetupConfigForFrance();
				SetupTestData();
				var complianceReport = SetupFECComplianceReport();
				using (complianceReport.Factory.AddDisposableService())
				{
					var tempDir = complianceReport.Factory.SubscribeForDispose(new TempDirectory());
					var testData = "Test data for eDocs should be at least 1 byte.";
					var fileName = "Test-file-size.txt";
					var tempFullFilename = Path.Combine(tempDir, fileName);
					File.WriteAllText(tempFullFilename, testData);
					complianceReport.AttachFileToEdoc(tempFullFilename, "Test file");

					fileName = "Test-file-size-copy.txt";
					tempFullFilename = Path.Combine(tempDir, fileName);
					File.WriteAllText(tempFullFilename, testData);
					complianceReport.AttachFileToEdoc(tempFullFilename, "Test file copy");
				}

				var stopWatchMock = new Mock<IStopwatch>();
				stopWatchMock.Setup(x => x.ElapsedMilliseconds).Returns(1234);
				using (ObjectFactory.Substitute(stopWatchMock.Object))
				{
					var usageCollector = GetRunAndDisposeUsageCollector(complianceReport);

					// test usage data written to DATABASE
					var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
					AssertEquals("Wrong number of rows in EDIMessage table.", 1, ediMessages.Length);

					var jObjects = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail));
					var jObject = jObjects.First();

					var acrValue = jObject.Properties().FirstOrDefault(kp => kp.Name.Equals("ACR", StringComparison.InvariantCulture))?.Value;
					AssertNotNull("ACR is missing", acrValue);

					if (acrValue is JObject acrJObject)
					{
						var acrValueString = (ZString)JsonConvert.SerializeObject(acrValue, Formatting.Indented);
						// "FEC Tax Audit"
						var acrExpectedAsString = GetEmbeddedResourceAsZString("AccComplianceReportUsageCollectorReportPropertiesWithEDocs.txt");
						acrExpectedAsString = acrExpectedAsString.TrimEnd(new[] { '\r', '\n' });
						AssertMultilineASCIIEquals("Wrong value in EDIMessage table.", acrExpectedAsString, acrValueString);
					}
				}
			}
		}

		AccComplianceReport SetupFECComplianceReport()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.FEC;
			complianceReport.ACR_DateFrom = new ZDate("2023-11-01");
			complianceReport.ACR_DateTo = new ZDate("2023-11-30");
			complianceReport.ACR_Description = "FEC 11/2023";
			return complianceReport;
		}

		AccComplianceReport SetupDayBookComplianceReport()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = "ABC";
			complianceReport.ACR_DateFrom = new ZDate("2023-11-01");
			complianceReport.ACR_DateTo = new ZDate("2023-11-30");
			complianceReport.ACR_Description = "ABC 11/2023";
			return complianceReport;
		}

		IAccComplianceReportUsageCollector GetRunAndDisposeUsageCollector(AccComplianceReport complianceReport)
		{
			var usageCollector = ObjectFactory.Get<IAccComplianceReportUsageCollectorFactory>().GetAccComplianceReportUsageCollector(complianceReport);
			var mockedUser = new Mock<IUser>(MockBehavior.Strict);
			mockedUser.Setup(m => m.Initials).Returns("TST");
			mockedUser.Setup(m => m.LoginName).Returns("TST");
			mockedUser.Setup(m => m.FullName).Returns("Test User");

			usageCollector.AddLogonUser(mockedUser.Object);
			usageCollector.AddChangedStatus(AccComplianceReport.Status.ReportCreated, AccComplianceReport.Status.ReportDataQueued);
			usageCollector.AddAction(AccComplianceReportUsageCollectorAction.Queueing);
			usageCollector.AddContext(AccComplianceReportUsageCollectorContext.CRQServiceTask);
			usageCollector.AddSessionId(Guid.Parse("ada64b32-6335-4db3-bd32-f455d9234ea6"));

			usageCollector.Dispose();
			Factory.Save();
			return usageCollector;
		}

		void SetupTestData()
		{
			configExpectedAll = new List<(string name, object value)>(configExpected)
			{
				("CountryRegistrationCode", (ZString)""),
				("ReportLineGrouping", (ZString)""),
				("ReportLineOrdering", (ZString)""),
				("GoodsServiceType", (ZString)""),
				("AmountsRoundingType", (ZString)""),
				("AmountsRoundingTruncating", (ZInt)0),
				("AmountThresholdLevel", (ZString)""),
				("ExTaxAmountThreshold", (ZDecimal)0M),
				("TaxAmountThreshold", (ZDecimal)0M),
				("RecipientPK", new ZGuid("00000000-0000-0000-0000-000000000000")),
				("IncludeQueuedForPreviousPeriod", new ZBool("N"))
			};
			acrExpectedAll.Add(("ReportConfiguration", configExpectedAll));

			var acrAddtional = new List<(string name, object value)>()
			{
				("LogonUserName", "TST"),
				("FullUserName", "Test User"),
				("OldReportStatus", "ADD"),
				("NewReportStatus", "QUE"),
				("Action", "Queueing"),
				("Context", "CRQServiceTask"),
				("SessionId", "ada64b32-6335-4db3-bd32-f455d9234ea6")
			};
			acrExpected.AddRange(acrAddtional);
			acrExpected.Add(("DurationSeconds", 1.234M));
			var executionDate = "2023-08-11 16:27:15";
			acrExpected.Add(("ExecutionDate", executionDate));
			acrExpectedAll.AddRange(acrAddtional);
			acrExpectedAll.Add(("DurationSeconds", 1.234M));
			acrExpectedAll.Add(("ExecutionDate", executionDate));

			// "eDocs": 
			eDocsExpected = new List<(string name, object value)>()
			{
				("NoOfFiles", 0),
				("TotalFileSizeInByte", 0)
			};
			acrExpectedAll.Add(("eDocs", eDocsExpected));
			acrExpected.Add(("eDocs", eDocsExpected));
		}

		void SetupACRForDefaultCountry()
		{
			// "ACR"
			acrExpected = new List<(string name, object value)>()
				{
					("ACRUsageCollectorVersion", (ZString)"1.0"),
					("ReportType", (ZString)"ABC"),
					("CRCompanyPK", (ZGuid)Env.CurrentCompany.PK),
					("CRCompanyCode", (ZString)Env.CurrentCompany.Code),
					("CountryCode", (ZString)"AU"),
					("ReportCreationDate","1998-01-02"),
					("ReportDescription", (ZString)"ABC 11/2023"),
					("DateFrom", "2023-11-01"),
					("DateTo", "2023-11-30")
				};
			acrExpectedAll = new List<(string name, object value)>(acrExpected);
		}

		void SetupConfigForDefaultCountry()
		{
			// "ReportConfiguration"
			configExpected = new List<(string name, object value)>()
				{
					("Country", (ZString)"AU"),
					("ReportCode", (ZString)"ABC"),
					("Title", (ZString)"ABC Test Report"),
					("TaxRegistrationType", (ZString)"ABN"),
					("Periodicity", (ZString)ReportPeriodicityCodes.DateRange),
					("TablePrefix", (ZString)ReportBaseTablePrefixListCodes.AllTransactions),
				};
			acrExpected.Add(("ReportConfiguration", configExpected));
		}

		void SetupACRForFrance()
		{
			// "ACR"
			acrExpected = new List<(string name, object value)>()
				{
					("ACRUsageCollectorVersion", (ZString)"1.0"),
					("ReportType", (ZString)"FEC"),
					("CRCompanyPK", (ZGuid)Env.CurrentCompany.PK),
					("CRCompanyCode", (ZString)Env.CurrentCompany.Code),
					("CountryCode", (ZString)"FR"),
					("ReportCreationDate","1998-01-02"),
					("ReportDescription", (ZString)"FEC 11/2023"),
					("DateFrom", "2023-11-01"),
					("DateTo", "2023-11-30")
				};
			acrExpectedAll = new List<(string name, object value)>(acrExpected);
		}

		void SetupConfigForFrance()
		{
			// "ReportConfiguration"
			configExpected = new List<(string name, object value)>()
				{
					("Country", (ZString)"FR"),
					("ReportCode", (ZString)"FEC"),
					("Title", (ZString)"Fichier des Écritures Comptables"),
					("TaxRegistrationType", (ZString)"TVA"),
					("Periodicity", (ZString)ReportPeriodicityCodes.RangeAccountingPeriod),
					("TablePrefix", (ZString)ReportBaseTablePrefixListCodes.AllTransactions)
				};
			acrExpected.Add(("ReportConfiguration", configExpected));
		}

		void SetupFECReportConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "FEC";
			reportConfig.ReportTitle = "Fichier des Écritures Comptables";
			reportConfig.Country = "FR";
			reportConfig.TaxRegistrationType = "TVA";
			reportConfig.ReportPeriodicity = ReportPeriodicityCodes.RangeAccountingPeriod;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
		}

		void SetupDayBookReportConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "ABC";
			reportConfig.ReportTitle = "ABC Test Report";
			reportConfig.Country = "AU";
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
		}

		ZString GetEmbeddedResourceAsZString(string filename)
		{
			var filecontent = ZString.Empty;
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.Business.Testing.ComplianceReport.TestFiles." + filename))
			{
				using (var sr = new StreamReader(stream))
				{
					filecontent = sr.ReadToEnd();
				}
			}
			return filecontent;
		}

		public List<(string name, object value)> acrExpected;
		public List<(string name, object value)> acrExpectedAll;
		public List<(string name, object value)> configExpected;
		public List<(string name, object value)> configExpectedAll;
		public List<(string name, object value)> eDocsExpected;
	}
}
