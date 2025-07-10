using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	sealed class SAFTXMLWriter1_30Test : SAFTXMLWriterBaseTest
	{
		protected override ISAFTXMLWriter GetWriter(AccComplianceReport report, AccComplianceReport[] reports, ReportModeAndCreditorSelector reportModeAndCreditorSelector)
		{
			if (report == null)
			{
				return new SAFTXMLWriter1_30(reports, reportModeAndCreditorSelector, null);
			}
			else
			{
				return new SAFTXMLWriter1_30(report, reportModeAndCreditorSelector, null);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 3, 8)]
		public override void TestWriteSingleReportXmlToStream()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(ZDateTime.Today.AddYears(-1));

				var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxRate = taxRateCollection.AddNew();
				validTaxRate.AT_Code = "TGST";
				validTaxRate.AT_Type = "RAT";
				validTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate.SetRateNumerator_ForTestOnly(10);
				validTaxRate.AT_ExtraTaxRateType = "QST";
				validTaxRate.SetExtraRate_ForTestOnly(4, 2);

				var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxMessage = taxMessageCollection.AddNew();
				validTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxMessage.A9_TaxGroupCode = "N1";

				Factory.Save();

				var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection
				{
					{ "N1", (NoResString)"Description N1", true, "N1.0" },
				};
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

				Factory.Save();

				var config = creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, validTaxRate, validTaxMessage));
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.NorwayCodeTypes.MVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report.ACR_DateFrom = ZDate.Today.AddDays(1);
				report.ACR_DateTo = ZDate.Today.AddDays(10);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				Factory.Save();

				var selector = new ReportModeAndCreditorSelector(Factory);
				selector.GenerateCreditorInvoices = true;
				selector.CreditorPK = Creator.AALSHI.PK;
				var writer = GetWriter(report, null, selector);
				var mockIComplianceReportGUIActionProvider = new Mock<IComplianceReportGUIActionProvider>();
				mockIComplianceReportGUIActionProvider.Setup(x => x.IsCountrySupportGenerateSAFT).Returns(true);
				mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, 100000));

				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceReportGUIActionProvider>>().Setup(x => x.Get()).Returns(mockIComplianceReportGUIActionProvider.Object);

				var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				var filePath = GetFullPath("SAFT-NO MothlyReport.xml");
				try
				{
					using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
					using (var dialog = GetXmlFileSaveDialog(filePath))
					{
						var result = writer.WriteSingleReportXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);
						var expectedMessage = @"The SAFT XML file was generated successfully.
";
						AssertExportedFile(result.FileNames.FirstOrDefault(), expectedMessage, result.Messages.ToString(), false);

						var expectXmlString = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><AuditFile xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:StandardAuditFile-Taxation-Financial:NO\"><Header><AuditFileVersion>1.30</AuditFileVersion><AuditFileCountry>NO</AuditFileCountry><AuditFileDateCreated>2019-03-08</AuditFileDateCreated><SoftwareCompanyName>Wisetech Global Limited</SoftwareCompanyName><SoftwareID>CargoWise</SoftwareID><SoftwareVersion>{ReleaseInfo.Instance.VersionNumber}</SoftwareVersion><Company><RegistrationNumber /><Name>Eagle Datamation International</Name><Address><StreetName>184 Bourke Road</StreetName><City>Alexandria</City><PostalCode>2015</PostalCode><Country>NO</Country><AddressType>PostalAddress</AddressType></Address><Contact><ContactPerson><FirstName>NotUsed</FirstName><LastName>CargoWise Support</LastName></ContactPerson><Telephone></Telephone></Contact><TaxRegistration><TaxRegistrationNumber>NA</TaxRegistrationNumber><TaxAuthority>Skatteetaten</TaxAuthority></TaxRegistration><BankAccount><BankAccountNumber></BankAccountNumber></BankAccount></Company><DefaultCurrencyCode>NOK</DefaultCurrencyCode><SelectionCriteria><SelectionStartDate>2019-03-09</SelectionStartDate><SelectionEndDate>2019-03-18</SelectionEndDate></SelectionCriteria><TaxAccountingBasis>A</TaxAccountingBasis></Header><MasterFiles><GeneralLedgerAccounts /><TaxTable><TaxTableEntry><TaxType>MVA</TaxType><Description>Merverdiavgift</Description><TaxCodeDetails><TaxCode>N1</TaxCode><Description>Description N1</Description><TaxPercentage>10</TaxPercentage><Country>NO</Country><StandardTaxCode>N1.0</StandardTaxCode><BaseRate>100</BaseRate></TaxCodeDetails></TaxTableEntry></TaxTable></MasterFiles><GeneralLedgerEntries><NumberOfEntries>0</NumberOfEntries><TotalDebit>0.00</TotalDebit><TotalCredit>0.00</TotalCredit><Journal><JournalID>EDI</JournalID><Description>Eagle Datamation International</Description><Type>A</Type></Journal></GeneralLedgerEntries></AuditFile>";
						var actualXmlString = GetStringFromFile(filePath);
						AssertEquals(expectXmlString, actualXmlString);

						mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, 1));

						result = writer.WriteSingleReportXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);

						expectedMessage = @"The SAFT XML file was generated successfully.
As the XML exceeded the maximum file size accepted by the Tax Authority, the XML is compressed into a ZIP file.
The exported file exceed the maximum file size accepted by the Tax Authority, please raise an eRequest for assistance.
";
						AssertExportedFile(result.FileNames.FirstOrDefault(), expectedMessage, result.Messages.ToString(), true, expectXmlString);

						mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, 1000));

						result = writer.WriteSingleReportXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);
						expectedMessage = @"The SAFT XML file was generated successfully.
As the XML exceeded the maximum file size accepted by the Tax Authority, the XML is compressed into a ZIP file.
";
						AssertExportedFile(result.FileNames.FirstOrDefault(), expectedMessage, result.Messages.ToString(), true, expectXmlString);
					}
				}
				finally
				{
					var xmlFile = new FileInfo(GetFullPath("SAFT-NO MothlyReport.xml"));
					xmlFile?.Delete();

					var zipFile = new FileInfo(GetFullPath("SAFT-NO MothlyReport.zip"));
					zipFile?.Delete();
				}
			}

			void AssertExportedFile(string fileName, string expectedMessage, string actualErrorMessage, bool isAfterCompressed, string expectString = "")
			{
				AssertEquals(expectedMessage, actualErrorMessage);
				var fileLength = new FileInfo(fileName).Length;

				if (isAfterCompressed)
				{
					Assert("XML file is compressed to zip file.", fileLength < 1000);
					Assert(fileName.Contains(".zip"));

					using (var archive = ZipFile.Open(fileName, ZipArchiveMode.Update))
					{
						var xmlFileName = Path.GetFileNameWithoutExtension(fileName) + ".xml";
						var entry = archive.GetEntry(xmlFileName);
						AssertNotNull(entry);
						using (var zipStream = entry.Open())
						{
							AssertEquals(expectString, zipStream.WriteToString());
						}
					}
				}
				else
				{
					Assert("XML file is not compressed", fileLength > 1000);
				}
			}

			string GetStringFromFile(string filePath)
			{
				var result = string.Empty;
				using (var resultFileDialog = new ZOpenFileDialog())
				{
					resultFileDialog.FileName = filePath;

					using (var resultStream = resultFileDialog.OpenFile())
					using (var reader = new StreamReader(resultStream))
					{
						result = reader.ReadToEnd();
					}
				}

				return result;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 3, 8)]
		public override void TestWriteAnnualReportXmlToStream()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				var filePath = @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT1_30\SAFT-NO Annual.xml";
				TestWriteSAFTAnnualReportCore(new Dictionary<string, string>(), filePath, shouldAddTaxNumber: false);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2023, 2, 22)]
		public void TestWriteSAFTAnnualReport_WriteReportToSeparateXMLAndZip()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				Creator.ABIGAS.OH_RL_NKClosestPort = "PTLIS";
				Creator.ABIGAS.OH_Code = "ABIGAS";
				Creator.CC10.AC_LocalLanguageDescription = "Local " + Creator.CC10.AC_Desc;
				Factory.Save();

				var startOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
				Creator.CreateTestPeriods(startOfFinancialYear); // Need to set up Financial year to pass balance from report to report inside Financial Year
				var periodCalculator = new AccountingPeriodCalculator(Factory);

				var apSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
				var arSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseAccountPK);
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseAccountPK);

				Creator.GLHeader1.AG_AccountNum = "8888.10.00";
				Creator.GLHeader1.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
				Creator.GLHeader1.AG_Description = "Test GL Account1";

				Creator.GLHeader2.AG_AccountNum = "8888.20.00";
				Creator.GLHeader2.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
				Creator.GLHeader2.AG_Description = "Test GL Account2";

				var openingPeriod = periodCalculator.GetPeriodFromDate(startOfFinancialYear);
				var openingBalanceDR = Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				var openingBalanceCR = Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				Factory.Save();

				var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
				report1.ACR_DateFrom = new ZDate(ZDateTime.Now.Year, 2, 1);
				report1.ACR_DateTo = new ZDate(ZDateTime.Now.Year, 2, 28);

				report1.Company.GC_Phone = "012345678";
				report1.Company.GC_Email = "email@company.com";
				report1.Company.Postcode = "";

				var reportPeriod = periodCalculator.GetPeriodManagementFromDate(report1.ACR_DateFrom, report1.ACR_GC_Company);
				report1.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report1.AccountingPeriod = reportPeriod.AM_Period;
				Creator.CreateConfigurationForComplianceReport(report1, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
				Factory.Save();

				CreateTestData(report1, Creator.ABIGAS, Creator.CC10, Creator.GLHeader1, "Report1-ATCUD");

				Creator.CreateAccGLAggregate(250m, reportPeriod.AM_Period, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-210m, reportPeriod.AM_Period, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(510m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6210.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-60m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8310.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-600m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8888.10.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(150m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "ZUSDHeader").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
				report2.ACR_ReportType = report1.ACR_ReportType;
				report2.ACR_DateFrom = new ZDate(ZDateTime.Now.Year, 3, 1);
				report2.ACR_DateTo = new ZDate(ZDateTime.Now.Year, 3, 31);
				reportPeriod = periodCalculator.GetPeriodManagementFromDate(report2.ACR_DateFrom, report2.ACR_GC_Company);
				report2.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report2.AccountingPeriod = reportPeriod.AM_Period;
				Factory.Save();

				CreateTestData(report2, Creator.AALSHI, Creator.CC6, Creator.GLHeader2, "Report2-ATCUD");

				var selector = new ReportModeAndCreditorSelector(Factory);
				selector.GenerateCreditorInvoices = true;
				selector.CreditorPK = Creator.AALSHI.PK;
				var writer = GetWriter(null, new AccComplianceReport[] { report1, report2 }, selector);

				var filePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT1_30\SAFT-NO Annual_Split.xml";

				try
				{
					var mockIComplianceReportGUIActionProvider = new Mock<IComplianceReportGUIActionProvider>();
					mockIComplianceReportGUIActionProvider.SetupGet(x => x.IsCountrySupportGenerateSAFT).Returns(false);
					mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, 1));

					var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
					mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceReportGUIActionProvider>>().Setup(x => x.Get()).Returns(mockIComplianceReportGUIActionProvider.Object);

					var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
					mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

					using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
					using (var dialog = GetXmlFileSaveDialog(filePath))
					using (var stream = new MemoryStream())
					{
						var createdFileNames = writer.WriteAnnualReportXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName).FileNames;

						AssertEquals(2, createdFileNames.Count());
						AssertEquals(GetFullPath("SAFT-NO Annual_Split_1.zip"), createdFileNames.FirstOrDefault());
						AssertEquals(GetFullPath("SAFT-NO Annual_Split_2.zip"), createdFileNames.LastOrDefault());

						var expectXml1 = $@"<?xml version=""1.0"" encoding=""utf-8""?><AuditFile xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:StandardAuditFile-Taxation-Financial:NO""><Header><AuditFileVersion>1.30</AuditFileVersion><AuditFileCountry>NO</AuditFileCountry><AuditFileDateCreated>2023-02-22</AuditFileDateCreated><SoftwareCompanyName>Wisetech Global Limited</SoftwareCompanyName><SoftwareID>CargoWise</SoftwareID><SoftwareVersion>{ReleaseInfo.Instance.VersionNumber}</SoftwareVersion><Company><RegistrationNumber /><Name>Eagle Datamation International</Name><Address><StreetName>184 Bourke Road</StreetName><City>Alexandria</City><Country>NO</Country><AddressType>PostalAddress</AddressType></Address><Contact><ContactPerson><FirstName>NotUsed</FirstName><LastName>CargoWise Support</LastName></ContactPerson><Telephone></Telephone></Contact><TaxRegistration><TaxRegistrationNumber>NA</TaxRegistrationNumber><TaxAuthority>Skatteetaten</TaxAuthority></TaxRegistration><BankAccount><BankAccountNumber></BankAccountNumber></BankAccount></Company><DefaultCurrencyCode>NOK</DefaultCurrencyCode><SelectionCriteria><SelectionStartDate>2023-02-01</SelectionStartDate><SelectionEndDate>2023-03-31</SelectionEndDate></SelectionCriteria><TaxAccountingBasis>A</TaxAccountingBasis></Header><MasterFiles><GeneralLedgerAccounts><Account><GroupingCategory>RF-1167</GroupingCategory><AccountID>6210.00.00</AccountID><AccountDescription>TRADE DEBTORS CONTROL</AccountDescription><AccountType>GL</AccountType><AccountCreationDate></AccountCreationDate><OpeningDebitBalance>0.00</OpeningDebitBalance><ClosingDebitBalance>1020.00</ClosingDebitBalance></Account><Account><GroupingCategory>RF-1167</GroupingCategory><AccountID>6240.00.00</AccountID><AccountDescription>WIP CONTROL</AccountDescription><AccountType>GL</AccountType><AccountCreationDate></AccountCreationDate><OpeningDebitBalance>123.00</OpeningDebitBalance><ClosingDebitBalance>533.00</ClosingDebitBalance></Account><Account><GroupingCategory>RF-1167</GroupingCategory><AccountID>8310.00.00</AccountID><AccountDescription>OUTPUT TAX PAYABLE</AccountDescription><AccountType>GL</AccountType><AccountCreationDate></AccountCreationDate><OpeningDebitBalance>0.00</OpeningDebitBalance><ClosingCreditBalance>120.00</ClosingCreditBalance></Account><Account><GroupingCategory>RF-1167</GroupingCategory><AccountID>8410.10.00</AccountID><AccountDescription>ACCRUAL - JOB COSTING</AccountDescription><AccountType>GL</AccountType><AccountCreationDate></AccountCreationDate><OpeningCreditBalance>123.00</OpeningCreditBalance><ClosingCreditBalance>443.00</ClosingCreditBalance></Account><Account><GroupingCategory>RF-1167</GroupingCategory><AccountID>8888.10.00</AccountID><AccountDescription>Test GL Account1</AccountDescription><AccountType>GL</AccountType><AccountCreationDate>2023-02-22</AccountCreationDate><OpeningDebitBalance>0.00</OpeningDebitBalance><ClosingCreditBalance>1000.00</ClosingCreditBalance></Account><Account><GroupingCategory>RF-1167</GroupingCategory><AccountID>ZUSDHeader</AccountID><AccountDescription></AccountDescription><AccountType>GL</AccountType><AccountCreationDate>2023-02-22</AccountCreationDate><OpeningDebitBalance>0.00</OpeningDebitBalance><ClosingDebitBalance>300.00</ClosingDebitBalance></Account><Account><GroupingCategory>RF-1167</GroupingCategory><AccountID>8888.20.00</AccountID><AccountDescription>Test GL Account2</AccountDescription><AccountType>GL</AccountType><AccountCreationDate>2023-02-22</AccountCreationDate><OpeningDebitBalance>0.00</OpeningDebitBalance><ClosingCreditBalance>200.00</ClosingCreditBalance></Account></GeneralLedgerAccounts><Customers><Customer><RegistrationNumber></RegistrationNumber><Name>A.A.L. SHIPPING AGENCIES P/L</Name><Address><StreetName>PO BOX 10446</StreetName><AdditionalAddressDetail>ADELAIDE ST, BRISBANE  QLD</AdditionalAddressDetail><PostalCode>4000</PostalCode><Country>AU</Country><AddressType>PostalAddress</AddressType></Address><Contact><ContactPerson><FirstName>NotUsed</FirstName><LastName /></ContactPerson><Telephone /><Email /></Contact><TaxRegistration><TaxRegistrationNumber>NA</TaxRegistrationNumber><TaxAuthority>Skatteetaten</TaxAuthority></TaxRegistration><CustomerID>AALSHI</CustomerID><BalanceAccount><AccountID>6210.00.00</AccountID><OpeningDebitBalance>0</OpeningDebitBalance><ClosingDebitBalance>330.0000</ClosingDebitBalance></BalanceAccount></Customer><Customer><RegistrationNumber></RegistrationNumber><Name>ABI GAS &amp; TOOLS</Name><Address><StreetName>171 ABBOTSFORD ROAD</StreetName><AdditionalAddressDetail>MAYNE, QLD</AdditionalAddressDetail><PostalCode>4006</PostalCode><Country>PT</Country><AddressType>PostalAddress</AddressType></Address><Contact><ContactPerson><FirstName>NotUsed</FirstName><LastName /></ContactPerson><Telephone /><Email /></Contact><TaxRegistration><TaxRegistrationNumber>NA</TaxRegistrationNumber><TaxAuthority>Skatteetaten</TaxAuthority></TaxRegistration><CustomerID>ABIGAS</CustomerID><BalanceAccount><AccountID>6210.00.00</AccountID><OpeningDebitBalance>0</OpeningDebitBalance><ClosingDebitBalance>690.0000</ClosingDebitBalance></BalanceAccount></Customer></Customers><TaxTable><TaxTableEntry><TaxType>MVA</TaxType><Description>Merverdiavgift</Description></TaxTableEntry></TaxTable></MasterFiles></AuditFile>";

						AssertZipFile(GetFullPath("SAFT-NO Annual_Split_1.zip"), "SAFT-NO Annual_Split_1_3.xml", expectXml1);

						var expectedXml2 = $@"<?xml version=""1.0"" encoding=""utf-8""?><AuditFile xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:StandardAuditFile-Taxation-Financial:NO""><Header><AuditFileVersion>1.30</AuditFileVersion><AuditFileCountry>NO</AuditFileCountry><AuditFileDateCreated>2023-02-22</AuditFileDateCreated><SoftwareCompanyName>Wisetech Global Limited</SoftwareCompanyName><SoftwareID>CargoWise</SoftwareID><SoftwareVersion>{ReleaseInfo.Instance.VersionNumber}</SoftwareVersion><Company><RegistrationNumber /><Name>Eagle Datamation International</Name><Address><StreetName>184 Bourke Road</StreetName><City>Alexandria</City><Country>NO</Country><AddressType>PostalAddress</AddressType></Address><Contact><ContactPerson><FirstName>NotUsed</FirstName><LastName>CargoWise Support</LastName></ContactPerson><Telephone></Telephone></Contact><TaxRegistration><TaxRegistrationNumber>NA</TaxRegistrationNumber><TaxAuthority>Skatteetaten</TaxAuthority></TaxRegistration><BankAccount><BankAccountNumber></BankAccountNumber></BankAccount></Company><DefaultCurrencyCode>NOK</DefaultCurrencyCode><SelectionCriteria><SelectionStartDate>2023-02-01</SelectionStartDate><SelectionEndDate>2023-02-28</SelectionEndDate></SelectionCriteria><TaxAccountingBasis>A</TaxAccountingBasis></Header><GeneralLedgerEntries><NumberOfEntries>10</NumberOfEntries><TotalDebit>3140.00</TotalDebit><TotalCredit>3040.00</TotalCredit><Journal><JournalID>EDI</JournalID><Description>Eagle Datamation International</Description><Type>A</Type><Transaction><TransactionID>ACR20230222</TransactionID><Period>2</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-02-22</TransactionDate><Description>ACR Transaction</Description><SystemEntryDate>2023-02-22</SystemEntryDate><GLPostingDate>2023-02-22</GLPostingDate><Line><RecordID>JCACR20230222-1</RecordID><AccountID>8410.10.00</AccountID><Description>ACCRUAL - JOB COSTING</Description><CreditAmount><Amount>110.00</Amount></CreditAmount></Line></Transaction><Transaction><TransactionID>WIP20230222</TransactionID><Period>2</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-02-22</TransactionDate><Description>WIP Transaction</Description><SystemEntryDate>2023-02-22</SystemEntryDate><GLPostingDate>2023-02-22</GLPostingDate><Line><RecordID>JCWIP20230222-1</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>160.00</Amount></DebitAmount></Line></Transaction><Transaction><TransactionID>ARINV00001001-20230223</TransactionID><Period>2</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-02-23</TransactionDate><Description>Test Invoice AmendingReason1</Description><SystemEntryDate>2023-02-23</SystemEntryDate><GLPostingDate>2023-02-23</GLPostingDate><Line><RecordID>ARINV00001001-1</RecordID><AccountID>6210.00.00</AccountID><CustomerID>ABIGAS</CustomerID><Description>TRADE DEBTORS CONTROL</Description><DebitAmount><Amount>330.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001001-2</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001001-3</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>200.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001001-4</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>20.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001001-5</RecordID><AccountID>8888.10.00</AccountID><Description>Test GL Account1</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001001-6</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001001-7</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>100.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001001-8</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>10.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001001-9</RecordID><AccountID>8888.10.00</AccountID><Description>Test GL Account1</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line></Transaction><Transaction><TransactionID>ARREC00001000</TransactionID><Period>2</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-02-23</TransactionDate><Description>AR RECEIPT</Description><SystemEntryDate>2023-02-23</SystemEntryDate><GLPostingDate>2023-02-23</GLPostingDate><Line><RecordID>ARREC00001000-1</RecordID><AccountID>6210.00.00</AccountID><CustomerID>ABIGAS</CustomerID><Description>TRADE DEBTORS CONTROL</Description><CreditAmount><Amount>150.00</Amount></CreditAmount></Line><Line><RecordID>ARREC00001000-2</RecordID><AccountID>ZUSDHeader</AccountID><Description></Description><DebitAmount><Amount>150.00</Amount></DebitAmount></Line></Transaction><Transaction><TransactionID>ARINV00001002-20230222</TransactionID><Period>2</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-02-22</TransactionDate><Description>Test Invoice AmendingReason2</Description><SystemEntryDate>2023-02-22</SystemEntryDate><GLPostingDate>2023-02-22</GLPostingDate><Line><RecordID>ARINV00001002-1</RecordID><AccountID>6210.00.00</AccountID><CustomerID>ABIGAS</CustomerID><Description>TRADE DEBTORS CONTROL</Description><DebitAmount><Amount>330.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001002-2</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001002-3</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>200.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001002-4</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>20.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001002-5</RecordID><AccountID>8888.10.00</AccountID><Description>Test GL Account1</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001002-6</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001002-7</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>100.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001002-8</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>10.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001002-9</RecordID><AccountID>8888.10.00</AccountID><Description>Test GL Account1</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line></Transaction></Journal></GeneralLedgerEntries></AuditFile>";

						AssertZipFile(GetFullPath("SAFT-NO Annual_Split_2.zip"), "SAFT-NO Annual_Split_2_3.xml", expectedXml2);

						var expectedXml3 = $@"<?xml version=""1.0"" encoding=""utf-8""?><AuditFile xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:StandardAuditFile-Taxation-Financial:NO""><Header><AuditFileVersion>1.30</AuditFileVersion><AuditFileCountry>NO</AuditFileCountry><AuditFileDateCreated>2023-02-22</AuditFileDateCreated><SoftwareCompanyName>Wisetech Global Limited</SoftwareCompanyName><SoftwareID>CargoWise</SoftwareID><SoftwareVersion>{ReleaseInfo.Instance.VersionNumber}</SoftwareVersion><Company><RegistrationNumber /><Name>Eagle Datamation International</Name><Address><StreetName>184 Bourke Road</StreetName><City>Alexandria</City><Country>NO</Country><AddressType>PostalAddress</AddressType></Address><Contact><ContactPerson><FirstName>NotUsed</FirstName><LastName>CargoWise Support</LastName></ContactPerson><Telephone></Telephone></Contact><TaxRegistration><TaxRegistrationNumber>NA</TaxRegistrationNumber><TaxAuthority>Skatteetaten</TaxAuthority></TaxRegistration><BankAccount><BankAccountNumber></BankAccountNumber></BankAccount></Company><DefaultCurrencyCode>NOK</DefaultCurrencyCode><SelectionCriteria><SelectionStartDate>2023-03-01</SelectionStartDate><SelectionEndDate>2023-03-31</SelectionEndDate></SelectionCriteria><TaxAccountingBasis>A</TaxAccountingBasis></Header><GeneralLedgerEntries><NumberOfEntries>10</NumberOfEntries><TotalDebit>3140.00</TotalDebit><TotalCredit>3040.00</TotalCredit><Journal><JournalID>EDI</JournalID><Description>Eagle Datamation International</Description><Type>A</Type><Transaction><TransactionID>ACR20230322</TransactionID><Period>3</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-03-22</TransactionDate><Description>ACR Transaction</Description><SystemEntryDate>2023-03-22</SystemEntryDate><GLPostingDate>2023-03-22</GLPostingDate><Line><RecordID>JCACR20230322-1</RecordID><AccountID>8410.10.00</AccountID><Description>ACCRUAL - JOB COSTING</Description><CreditAmount><Amount>110.00</Amount></CreditAmount></Line></Transaction><Transaction><TransactionID>WIP20230322</TransactionID><Period>3</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-03-22</TransactionDate><Description>WIP Transaction</Description><SystemEntryDate>2023-03-22</SystemEntryDate><GLPostingDate>2023-03-22</GLPostingDate><Line><RecordID>JCWIP20230322-1</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>160.00</Amount></DebitAmount></Line></Transaction><Transaction><TransactionID>ARINV00001004-20230323</TransactionID><Period>3</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-03-23</TransactionDate><Description>Test Invoice AmendingReason1</Description><SystemEntryDate>2023-03-23</SystemEntryDate><GLPostingDate>2023-03-23</GLPostingDate><Line><RecordID>ARINV00001004-1</RecordID><AccountID>6210.00.00</AccountID><CustomerID>ABIGAS</CustomerID><Description>TRADE DEBTORS CONTROL</Description><DebitAmount><Amount>330.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001004-2</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001004-3</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>200.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001004-4</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>20.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001004-5</RecordID><AccountID>8888.10.00</AccountID><Description>Test GL Account1</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001004-6</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001004-7</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>100.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001004-8</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>10.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001004-9</RecordID><AccountID>8888.10.00</AccountID><Description>Test GL Account1</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line></Transaction><Transaction><TransactionID>ARREC00001001</TransactionID><Period>3</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-03-23</TransactionDate><Description>AR RECEIPT</Description><SystemEntryDate>2023-03-23</SystemEntryDate><GLPostingDate>2023-03-23</GLPostingDate><Line><RecordID>ARREC00001001-1</RecordID><AccountID>6210.00.00</AccountID><CustomerID>ABIGAS</CustomerID><Description>TRADE DEBTORS CONTROL</Description><CreditAmount><Amount>150.00</Amount></CreditAmount></Line><Line><RecordID>ARREC00001001-2</RecordID><AccountID>ZUSDHeader</AccountID><Description></Description><DebitAmount><Amount>150.00</Amount></DebitAmount></Line></Transaction><Transaction><TransactionID>ARINV00001005-20230222</TransactionID><Period>2</Period><PeriodYear>2023</PeriodYear><TransactionDate>2023-02-22</TransactionDate><Description>Test Invoice AmendingReason2</Description><SystemEntryDate>2023-02-22</SystemEntryDate><GLPostingDate>2023-02-22</GLPostingDate><Line><RecordID>ARINV00001005-1</RecordID><AccountID>6210.00.00</AccountID><CustomerID>AALSHI</CustomerID><Description>TRADE DEBTORS CONTROL</Description><DebitAmount><Amount>330.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001005-2</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001005-3</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>200.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001005-4</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>20.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001005-5</RecordID><AccountID>8888.20.00</AccountID><Description>Test GL Account2</Description><CreditAmount><Amount>200.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001005-6</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001005-7</RecordID><AccountID>6240.00.00</AccountID><Description>WIP CONTROL</Description><DebitAmount><Amount>100.00</Amount></DebitAmount></Line><Line><RecordID>ARINV00001005-8</RecordID><AccountID>8310.00.00</AccountID><Description>OUTPUT TAX PAYABLE</Description><CreditAmount><Amount>10.00</Amount></CreditAmount></Line><Line><RecordID>ARINV00001005-9</RecordID><AccountID>8888.10.00</AccountID><Description>Test GL Account1</Description><CreditAmount><Amount>100.00</Amount></CreditAmount></Line></Transaction></Journal></GeneralLedgerEntries></AuditFile>";

						AssertZipFile(GetFullPath("SAFT-NO Annual_Split_2.zip"), "SAFT-NO Annual_Split_3_3.xml", expectedXml3);
					}
				}
				finally
				{
					var xmlFile = new FileInfo(GetFullPath("SAFT-NO Annual_Split.xml"));
					xmlFile?.Delete();

					var zipFile = new FileInfo(GetFullPath("SAFT-NO Annual_Split_1.zip"));
					zipFile?.Delete();

					zipFile = new FileInfo(GetFullPath("SAFT-NO Annual_Split_2.zip"));
					zipFile?.Delete();
				}
			}

			void AssertZipFile(string zipFileName, string xmlFileName, string expectXml)
			{
				Assert(zipFileName.Contains(".zip"));

				using (var archive = ZipFile.Open(zipFileName, ZipArchiveMode.Update))
				{
					var entry = archive.GetEntry(xmlFileName);
					AssertNotNull(entry);
					using (var zipStream = entry.Open())
					{
						this.AssertXMLEqualsIgnoreChildOrder("Xml is same as expect.", expectXml, zipStream.WriteToString());
					}
				}
			}

			string GetFullPath(string fileName) => base.GetFullPath(fileName, "SAFT1_30");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var mockFeatureManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
			ObjectFactory.Substitute(mockFeatureManager.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
		}
	}
}
