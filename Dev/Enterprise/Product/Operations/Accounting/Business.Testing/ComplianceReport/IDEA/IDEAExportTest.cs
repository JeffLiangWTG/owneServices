using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.IDEA;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.IDEA
{
	[TestedType(typeof(IDEATaxAuditExport))]
	public class IDEAExportTest : NonPersistentBusinessObjectTestCase
	{
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		public void TestTempFiles()
		{
			var serviceLogger = new DummyLogger();
			var fileName = "testfile.txt";
			var testData = "IDEA Test String";
			var tempFolder = string.Empty;
			using (var tempFiles = new IDEATempFiles(serviceLogger))
			{
				tempFiles.WriteFileToTempFolder(fileName, testData);
				tempFolder = tempFiles.FolderName;
				AssertNotNullOrEmpty(tempFolder);
				var fileData = File.ReadAllText(Path.Combine(tempFolder, fileName));
				AssertEquals("The data read from the temp file does not match the data written to it", testData, fileData);
			}
			var folderExists = Directory.Exists(tempFolder);
			AssertEquals("The temp folder should have been removed when it is no longer needed", false, folderExists);
		}

		public void TestZipFileSplitting()
		{
			var serviceLogger = new DummyLogger();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var zipCreator = new IDEAZipFile(complianceReport, serviceLogger);
			var eDocs = new IDEAeDocs(complianceReport);
			var fiftyBytesTestData = "12345678901234567890123456789012345678901234567890";

			using (complianceReport.Factory.AddDisposableService())
			{
				using (var tempFiles = new IDEATempFiles(serviceLogger))
				{
					for (var i = 0; i < 30; i++)
					{
						tempFiles.AddFile("testfile." + i.ToString("000"), fiftyBytesTestData, 60, true, zipCreator, eDocs);
					}

					zipCreator.CreateZipFileForEDocs(tempFiles.FolderName, eDocs);
					var docmanager = complianceReport.DocManagerInfo();
					var zipFilename = zipCreator.GetZipFileNameForComplianceReport();
					var filenameWithoutExtension = Path.GetFileNameWithoutExtension(zipFilename);
					var extension = Path.GetExtension(zipFilename);
					AssertEquals("Three files have been added to eDocs", 3,
						docmanager.AllEDocs.Count); // 30 files of 50 bytes (compressed to 5 bytes) result in 2 archives of 60 bytes with 12 files and a 3rd archive with the remaining 6 files
					AssertEquals("Name of first archive", $"{filenameWithoutExtension}-part001{extension}", docmanager.AllEDocs[0].FileName);
					AssertEquals("Name of second archive", $"{filenameWithoutExtension}-part002{extension}", docmanager.AllEDocs[1].FileName);
					AssertEquals("Name of third archive", $"{filenameWithoutExtension}-part003{extension}", docmanager.AllEDocs[2].FileName);
				}
			}
		}

		public void TestGetMaxArchiveNumber()
		{
			var serviceLogger = new DummyLogger();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var zipCreator = new IDEAZipFile(complianceReport, serviceLogger);
			var eDocs = new IDEAeDocs(complianceReport);
			var textTestData = "IDEA Test String";
			AssertEquals("No files attached", 0, eDocs.GetMaxArchiveNumber());

			using (complianceReport.Factory.AddDisposableService())
			{
				using (var tempFiles = new IDEATempFiles(serviceLogger))
				{
					var docManager = complianceReport.DocManagerInfo();
					var newFile = docManager.AddFileOrDocument(Encoding.UTF8.GetBytes(textTestData), "file1.txt", Core.Constants.RefDocTypes.MiscellaneousDocument, description: IDEATaxAuditExport.FileDescription);
					AssertEquals("A file but no ZIP file attached", 0, eDocs.GetMaxArchiveNumber());

					for (var i = 0; i < 3; i++)
					{
						tempFiles.AddFile("testfile." + i.ToString("000"), textTestData, 60, true, zipCreator, eDocs);
					}
					zipCreator.CreateZipFileForEDocs(tempFiles.FolderName, eDocs);
					AssertEquals("One ZIP file attached", 1, eDocs.GetMaxArchiveNumber());

					for (var i = 3; i < 5; i++)
					{
						tempFiles.AddFile("testfile." + i.ToString("000"), textTestData, 60, true, zipCreator, eDocs);
					}
					zipCreator.CreateZipFileForEDocs(tempFiles.FolderName, eDocs);
					AssertEquals("Two ZIP files attached", 2, eDocs.GetMaxArchiveNumber());
				}
			}
		}

		public void TestDataProviderOpeningBalance()
		{
			var serviceLogger = new DummyLogger();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
			var dataProvider = new IDEADataProvider(Factory, complianceReport, serviceLogger);
			Creator.CreateTestPeriods(new ZDateTime(2020, 1, 1));
			var accounts = new string[] { "TEST.10.00", "TEST.20.00", "RTER.00.00" };
			var account1 = CreateAccount(accounts[0], "OV", "Account 1", "P&L", "CR");  // P&L accounts don't have an opening balance ...
			var account2 = CreateAccount(accounts[1], "OV", "Account 2", "BSH", "CR");
			var retainedEarningsAccount = CreateAccount(accounts[2], "OV", "Retained Earnings", "BSH", "CR");  // ... because the balance is moved to retained earnings
			CreateAccountAggregate(account1, 500, 201901);
			CreateAccountAggregate(account1, 300, 201902);
			CreateAccountAggregate(account2, 500, 201901);
			CreateAccountAggregate(account2, 300, 202001);
			Factory.Save();

			var openingBalances = dataProvider.RetrieveOpeningBalanceForAccounts(accounts, accounts[2]);
			AssertEquals("Number of opening balances", 2, openingBalances.Count);
			AssertEquals("Opening balance for account #2", -500.0m, openingBalances[accounts[1]]);
			AssertEquals("Opening balance for Retained Earnings account", -800.0m, openingBalances[accounts[2]]);
		}

		public void TestDataProviderControlAccounts()
		{
			var controlAccountAndReportSubCodeMapping = new ControlAccountAndReportSubCodeMapping();

			AssertEquals("Number of special accounts", 8, controlAccountAndReportSubCodeMapping.ControlAccountsNumbers.Count);
			AssertEquals("AP Control Account", "8210.00.00", controlAccountAndReportSubCodeMapping.GetAccountNumber(AccountingConfigurationRegistry.Instance.APControlAccount.Value));
			AssertEquals("AR Control Account", "6210.00.00", controlAccountAndReportSubCodeMapping.GetAccountNumber(AccountingConfigurationRegistry.Instance.ARControlAccount.Value));
			AssertEquals("Accrued Cost Account", "8410.10.00", controlAccountAndReportSubCodeMapping.GetAccountNumber(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value));
			AssertEquals("Accrued Revenue Account", "6240.00.00", controlAccountAndReportSubCodeMapping.GetAccountNumber(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value));
			AssertEquals("CFX Account", "5300.00.00", controlAccountAndReportSubCodeMapping.GetAccountNumber(AccountingConfigurationRegistry.Instance.CFXAccount.Value));
			AssertEquals("GST Input Account", "6310.00.00", controlAccountAndReportSubCodeMapping.GetAccountNumber(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value));
			AssertEquals("GST Output Account", "8310.00.00", controlAccountAndReportSubCodeMapping.GetAccountNumber(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value));
			AssertEquals("Retained Earnings Account", "4900.00.00", controlAccountAndReportSubCodeMapping.GetAccountNumber((Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value));
		}

		public void TestControlAccountNumbersRelatedToSubCodes()
		{
			var controlAccountAndReportSubCodeMapping = new ControlAccountAndReportSubCodeMapping();

			foreach (var key in controlAccountAndReportSubCodeMapping.AccountPkToSubCodesMapping.Keys)
			{
				var expectedAccountNumber = "";

				if (key == AccountingConfigurationRegistry.Instance.APControlAccount.Value)
				{
					expectedAccountNumber = "8210.00.00";
				}
				else if (key == AccountingConfigurationRegistry.Instance.ARControlAccount.Value)
				{
					expectedAccountNumber = "6210.00.00";
				}
				else if (key == AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value)
				{
					expectedAccountNumber = "8410.10.00";
				}
				else if (key == AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value)
				{
					expectedAccountNumber = "6240.00.00";
				}
				else if (key == AccountingConfigurationRegistry.Instance.CFXAccount.Value)
				{
					expectedAccountNumber = "5300.00.00";
				}
				else if (key == AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value)
				{
					expectedAccountNumber = "6310.00.00";
				}
				else if (key == AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value)
				{
					expectedAccountNumber = "8310.00.00";
				}

				foreach (var subcode in controlAccountAndReportSubCodeMapping.AccountPkToSubCodesMapping[key])
				{
					AssertEquals(expectedAccountNumber, controlAccountAndReportSubCodeMapping.GetAccountNumber(subcode, ""));
				}
			}
		}

		public void TestDataProviderRetrieveAuditData()
		{
			var serviceLogger = new DummyLogger();

			var company = Creator.CreateNewCompany("DE1", "DE");
			var taxOrgProxy = Creator.CreateOrgHeader("", false, false, "DEHAM");
			var taxBranch = Creator.CreateBranch("B1", "TaxBranch", company, taxOrgProxy);
			company.GC_OH_OrgProxy = taxOrgProxy.PK;
			company.GC_BusinessRegNo = "123456789";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, taxBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var taxBranchAddress = Creator.CreateAddress(header: company.FirstActiveBranch.OrgProxy, addressType: OrgAddressType.Office, isMain: true, streetAddress1: "Proxy Address Line 1",
						streetAddress2: "Proxy Address Line 2", city: "Org ProxyCity", stateCode: "HH", countryCode: Core.Constants.CountryCodes.Germany, postCode: "", phone: "", email: "");

				var taxBranchAddress2 = Creator.CreateAddress(header: company.FirstActiveBranch.OrgProxy, addressType: OrgAddressType.Office, isMain: false, streetAddress1: "Proxy Address2 Line 1",
						streetAddress2: "Proxy Address2 Line 2", city: "Org ProxyCity2", stateCode: "HH", countryCode: Core.Constants.CountryCodes.Germany, postCode: "", phone: "", email: "");

				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
				var dataProvider = new IDEADataProvider(Factory, complianceReport, serviceLogger);
				var accounts = new string[]
				{
					"1000.10.XX", "1000.20.XX", "5300.00.00", "6210.00.00", "6240.00.00", "6310.00.00",
					"8210.00.00", "8310.00.00", "8410.10.00"
				};
				var account1 = CreateAccount(accounts[0], "OV", "Gemeinkosten", "P&L", "CR");
				var account2 = CreateAccount(accounts[1], "OV", "Gemeinkosten", "P&L", "DR");

				var organisations = new string[] { "CUSTOMER1", "CUSTOMER2", "CUSTOMER3", "CUSTOMER4" };
				var org1 = CreateOrganisation(organisations[0]);
				var org1Addr1 = org1.Addresses[0];
				SetAddressData(org1Addr1, "Org1 Delivery Address1", "Org1 Delivery Address Line 2", "Org11 Short Code", "Org1 D-City", "NSW", Core.Constants.CountryCodes.Australia);
				var org1Addr2 = Creator.CreateAddress(org1, OrgAddressType.Office, true);
				SetAddressData(org1Addr2, "Org1 Main Office Address1", "Org1 Office Address Line 2", "Org12 Short Code", "Org1 O-City", "NSW", Core.Constants.CountryCodes.Australia);
				var org1Addr3 = Creator.CreateAddress(org1, OrgAddressType.Office, false);
				SetAddressData(org1Addr3, "Org1 normal Office Address1", "Org1 Address Line 2", "Org13 Short Code", "Org1 NO-City", "NSW", Core.Constants.CountryCodes.Australia);
				var org1Addr4 = Creator.CreateAddress(org1, OrgAddressType.Receivables, true);
				SetAddressData(org1Addr4, "Org1 Main Receivables Address1", "Org1 Receivables Address Line 2", "Org14 Short Code", "Org1 O-City", "NSW", Core.Constants.CountryCodes.Australia);

				var org2 = CreateOrganisation(organisations[1]);
				var org2Addr1 = org2.Addresses[0];
				org2Addr1.CapabilitiesCollection[0].PZ_IsMainAddress = false;
				org2Addr1.CapabilitiesCollection[0].PZ_AddressType = OrgAddressType.Delivery.Code;
				SetAddressData(org2Addr1, "Org2 Delivery Address1", "Org2 Delivery Address Line 2", "Org21 Short Code", "Org2 D-City", "HH", Core.Constants.CountryCodes.Germany);
				var org2Addr2 = Creator.CreateAddress(org2, OrgAddressType.Office, false);
				SetAddressData(org2Addr2, "Org2 normal Office Address1", "Org2 Office Address Line 2", "Org22 Short Code", "Org2 O-City", "HH", Core.Constants.CountryCodes.Germany);
				var org2Addr3 = Creator.CreateAddress(org2, OrgAddressType.Pickup, false);
				SetAddressData(org2Addr3, "Org2 pickup Address1", "Org2 Pickup Address Line 2", "Org23 Short Code", "Org2 NO-City", "NDS", Core.Constants.CountryCodes.Germany);

				var org3 = CreateOrganisation(organisations[2]);
				var org3Addr1 = org3.Addresses[0];
				org3Addr1.CapabilitiesCollection[0].PZ_IsMainAddress = false;
				org3Addr1.CapabilitiesCollection[0].PZ_AddressType = OrgAddressType.Delivery.Code;
				SetAddressData(org3Addr1, "Org3 Delivery Address1", "Org3 Delivery Address Line 2", "Org31 Short Code", "Org3 D-City", "HH", Core.Constants.CountryCodes.Germany);
				var org3Addr2 = Creator.CreateAddress(org3, OrgAddressType.Miscellaneous, false);
				SetAddressData(org3Addr2, "Org3 Misc Address1", "Org3 Miscellaneous Address Line 2", "Org32 Short Code", "Org3 O-City", "HH", Core.Constants.CountryCodes.Germany);
				var org3Addr3 = Creator.CreateAddress(org3, OrgAddressType.Pickup, false);
				SetAddressData(org3Addr3, "Org3 pickup Address1", "Org3 Pickup Address Line 2", "Org33 Short Code", "Org3 NO-City", "NDS", Core.Constants.CountryCodes.Germany);

				var org4 = CreateOrganisation(organisations[3]);
				var org4Addr1 = org4.Addresses[0];

				CreateApTransaction(complianceReport, "INV001", 600, new ZDate(2020, 1, 1), org1, account1);
				CreateApTransaction(complianceReport, "INV002", -500, new ZDate(2020, 3, 1), org1, account2);
				CreateApTransaction(complianceReport, "INV003", 300, new ZDate(2020, 5, 1), org3, account1);

				Factory.Save();

				// delete the address of CUSTOMER4
				org4Addr1.Delete();

				var orgCusCode = Creator.CreateCustomsCodes(org1, codeCountry: "AU", codeType: "ABN", customsRegNo: "1234567AU");
				orgCusCode = Creator.CreateCustomsCodes(org1, codeCountry: "DE", codeType: "UST", customsRegNo: "1234567DE");
				orgCusCode = Creator.CreateCustomsCodes(org2, codeCountry: "DE", codeType: "UST", customsRegNo: "DE123456700");
				orgCusCode = Creator.CreateCustomsCodes(org2, codeCountry: "DE", codeType: "TAO", customsRegNo: "DE12345555555");

				Factory.Save();

				var accountRawData =
					dataProvider.RetrieveAccounts(new HashSet<string>(accounts));
				AssertEquals("Number of accounts", 9, accountRawData.Count);
				var retrievedAccounts =
					accountRawData.Select(k => (string)(ZString)k["Kontonr"]).OrderBy(v => v).ToArray();
				AssertArrayEqualsByElements("Account numbers", accounts, retrievedAccounts);

				var organisationsRawData =
					dataProvider.RetrieveOrganizations(new HashSet<string>(organisations));
				AssertEquals("Number of organisations", 4, organisationsRawData.Length);
				var retrievedOrganisations = organisationsRawData.Select(k => (string)(ZString)k["PKKtonr"])
					.OrderBy(v => v).ToArray();
				AssertArrayEqualsByElements("Organisation numbers", organisations, retrievedOrganisations);

				var org1Retr = organisationsRawData.FirstOrDefault(k => (string)(ZString)k["PKKtonr"] == "CUSTOMER1");
				AssertOrganisation(org1Addr2, org1Retr);
				AssertEquals("Wrong VAT ID", "1234567AU", org1Retr["UmsatzsteuerID"]);

				var org2Retr = organisationsRawData.FirstOrDefault(k => (string)(ZString)k["PKKtonr"] == "CUSTOMER2");
				AssertOrganisation(org2Addr2, org2Retr);
				AssertEquals("Wrong VAT ID", "DE123456700", org2Retr["UmsatzsteuerID"]);

				var org3Retr = organisationsRawData.FirstOrDefault(k => (string)(ZString)k["PKKtonr"] == "CUSTOMER3");
				var emptyAddr = Factory.New<OrgAddress>();
				AssertOrganisation(emptyAddr, org3Retr);
				AssertEquals("Wrong VAT ID", string.Empty, org3Retr["UmsatzsteuerID"]);

				var org4Retr = organisationsRawData.FirstOrDefault(k => (string)(ZString)k["PKKtonr"] == "CUSTOMER4");
				AssertOrganisation(emptyAddr, org3Retr);
				AssertEquals("Wrong VAT ID", string.Empty, org4Retr["UmsatzsteuerID"]);

				var companyRawData = dataProvider.RetrieveCompany();
				AssertEquals("Number of companies", 1, companyRawData.Count);
				var retrievedCompany = companyRawData.FirstOrDefault();
				AssertOrganisation(taxBranchAddress, retrievedCompany);
				AssertEquals("Wrong VAT ID", "123456789", retrievedCompany["UStID"]);
			}
		}

		public void TestDataProviderRetrieveCompanyWithOfficeAddress()
		{
			var serviceLogger = new DummyLogger();

			var company = Creator.CreateNewCompany("DE1", "DE");
			var taxOrgProxy = Creator.CreateOrgHeader("", false, false, "DEHAM");
			var taxBranch = Creator.CreateBranch("B1", "TaxBranch", company, taxOrgProxy);
			company.GC_OH_OrgProxy = taxOrgProxy.PK;
			company.GC_BusinessRegNo = "123456789";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, taxBranch.PK.ToGuid(),
					   Env.CurrentDepartmentPK))
			{
				// The address capability is not saved into OrgAddressCapability if "isMain: false". Therefore, I have to switch PZ_IsMainAddress.
				var taxBranchAddress = Creator.CreateAddress(header: company.FirstActiveBranch.OrgProxy, addressType: OrgAddressType.Miscellaneous, isMain: true, streetAddress1: "Proxy Address2 Line 1",
						streetAddress2: "Proxy Address2 Line 2", city: "Org ProxyCity2", stateCode: "HH", countryCode: Core.Constants.CountryCodes.Germany, postCode: "", phone: "", email: "");
				taxBranchAddress.CapabilitiesCollection[0].PZ_IsMainAddress = false;
				Factory.Save();

				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
				var dataProvider = new IDEADataProvider(Factory, complianceReport, serviceLogger);

				var companyRawData = dataProvider.RetrieveCompany();
				AssertEquals("Number of companies", 1, companyRawData.Count);
				var retrievedCompany = companyRawData.FirstOrDefault();
				AssertOrganisation(taxOrgProxy.Addresses[0], retrievedCompany);
				AssertEquals("Wrong VAT ID", "123456789", retrievedCompany["UStID"]);
			}
		}

		public void TestDataProviderRetrieveCompanyWithoutOfficeAddress()
		{
			var serviceLogger = new DummyLogger();

			var company = Creator.CreateNewCompany("DE1", "DE");
			var taxOrgProxy = Creator.CreateOrgHeader("", false, false, "DEHAM");
			var taxBranch = Creator.CreateBranch("B1", "TaxBranch", company, taxOrgProxy);
			company.GC_OH_OrgProxy = taxOrgProxy.PK;
			company.GC_BusinessRegNo = "123456789";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, taxBranch.PK.ToGuid(),
					   Env.CurrentDepartmentPK))
			{
				// The address capability is not saved into OrgAddressCapability if "isMain: false". Therefore, I have to switch PZ_IsMainAddress.
				var taxBranchAddress = taxOrgProxy.Addresses[0];
				taxBranchAddress.CapabilitiesCollection[0].PZ_AddressType = OrgAddressType.Miscellaneous.Code;
				taxBranchAddress.CapabilitiesCollection[0].PZ_IsMainAddress = false;
				Factory.Save();

				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
				var dataProvider = new IDEADataProvider(Factory, complianceReport, serviceLogger);

				var companyRawData = dataProvider.RetrieveCompany();
				AssertEquals("Number of companies", 1, companyRawData.Count);
				var retrievedCompany = companyRawData.FirstOrDefault();
				var emptyAddr = Factory.New<OrgAddress>();
				AssertOrganisation(emptyAddr, retrievedCompany);
				AssertEquals("Wrong VAT ID", "123456789", retrievedCompany["UStID"]);
			}
		}

		public void TestDataProviderRetrieveCompanyWithoutAddress()
		{
			var serviceLogger = new DummyLogger();

			var company = Creator.CreateNewCompany("DE1", "DE");
			var taxOrgProxy = Creator.CreateOrgHeader("", false, false, "DEHAM");
			var taxBranch = Creator.CreateBranch("B1", "TaxBranch", company, taxOrgProxy);
			company.GC_OH_OrgProxy = taxOrgProxy.PK;
			company.GC_BusinessRegNo = "123456789";

			// delete the address of the org.proxy
			var address = taxOrgProxy.Addresses[0];
			address.Delete();
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, taxBranch.PK.ToGuid(),
					   Env.CurrentDepartmentPK))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
				var dataProvider = new IDEADataProvider(Factory, complianceReport, serviceLogger);

				var companyRawData = dataProvider.RetrieveCompany();
				AssertEquals("Number of companies", 1, companyRawData.Count);
				var retrievedCompany = companyRawData.FirstOrDefault();
				var emptyAddr = Factory.New<OrgAddress>();
				AssertOrganisation(emptyAddr, retrievedCompany);
				AssertEquals("Wrong VAT ID", "123456789", retrievedCompany["UStID"]);
			}
		}

		public void TestDataProviderCompanyWithoutReachingFallbackValues()
		{
			TestDataProviderCompanyWithFallbackLevels(false);
		}

		public void TestDataProviderCompanyWithReachingFallbackValues()
		{
			TestDataProviderCompanyWithFallbackLevels(true);
		}

		void TestDataProviderCompanyWithFallbackLevels(bool reachFallbackData = true)
		{
			var serviceLogger = new DummyLogger();

			var company = Creator.CreateNewCompany("DE1", "DE");
			company.GC_Name = "Test Company";
			company.GC_BusinessRegNo = "123456789";
			company.GC_RX_NKLocalCurrency = "EUR";

			var taxOrgProxy = Creator.CreateOrgHeader("", false, false, "DEHAM");
			taxOrgProxy.OH_FullName = "Test Org";
			var taxBranch = Creator.CreateBranch("B1", "TaxBranch", company, taxOrgProxy);
			company.GC_OH_OrgProxy = taxOrgProxy.PK;
			company.GC_WebAddress = "www.companywebsite.com";
			company.GC_Phone = "0112655555";
			company.GC_Fax = "0112655556";
			company.GC_Email = "www.companyemail@xyz.com";

			if (!reachFallbackData)
			{
				var address1 = Creator.CreateAddress(header: taxOrgProxy, addressType: OrgAddressType.Office, isMain: true, streetAddress1: "Proxy Address1 Office Line 1",
					streetAddress2: "Proxy Address1 Office Line 2", city: "Org ProxyCity1", stateCode: "HH", countryCode: Core.Constants.CountryCodes.Germany, postCode: "12345", phone: "00991112222", email: "www.officeaddres1@wise.com");
				address1.OA_Fax = "00991112212";

				Creator.CreateCustomsCodes(taxOrgProxy, CountryCodes.Germany, "UST", "0147154321");
				Creator.CreateCustomsCodes(taxOrgProxy, CountryCodes.Germany, "STE", "19960117");

				var url = taxOrgProxy.OrgWebURLs.AddNew();
				url.PU_URL = "http://www.organizationwesite.com";
			}
			else
			{
				var taxBranchAddress = taxOrgProxy.Addresses[0];
				taxBranchAddress.OA_IsActive = false;
			}

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, taxBranch.PK.ToGuid(),
					   Env.CurrentDepartmentPK))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
				var dataProvider = new IDEADataProvider(Factory, complianceReport, serviceLogger);

				var companyRawData = dataProvider.RetrieveCompany();
				AssertEquals("Number of companies", 1, companyRawData.Count);
				var retrievedCompany = companyRawData.FirstOrDefault();

				if (!reachFallbackData)
				{
					AssertEquals("Company Name", "Test Org", retrievedCompany["Unternehmensname"]);
					AssertEquals("UStID", "0147154321", retrievedCompany["UStID"]);
					AssertEquals("SteuerlicheIdentifikationsnummer", "19960117", retrievedCompany["SteuerlicheIdentifikationsnummer"]);

					AssertEquals("Address", "Proxy Address1 Office Line 1 Proxy Address1 Office Line 2", retrievedCompany["Anschrift"]);
					AssertEquals("PLZ", "12345", retrievedCompany["Postleitzahl"]);
					AssertEquals("City", "Org ProxyCity1", retrievedCompany["Ort"]);
					AssertEquals("State", "HH", retrievedCompany["Bundesland"]);
					AssertEquals("Country", "DE", retrievedCompany["Land"]);
					AssertEquals("Phone Number", "00991112222", retrievedCompany["Telefon"]);
					AssertEquals("Fax", "00991112212", retrievedCompany["Fax"]);
					AssertEquals("E-mail", "www.officeaddres1@wise.com", retrievedCompany["Email"]);
					AssertEquals("Website", "http://www.organizationwesite.com", retrievedCompany["Internet"]);
					AssertEquals("Currency", "EUR", retrievedCompany["Fibuwaehrung"]);
				}
				else
				{
					AssertEquals("Company Name", "Test Org", retrievedCompany["Unternehmensname"]);
					AssertEquals("UStID", "123456789", retrievedCompany["UStID"]);
					AssertEquals("SteuerlicheIdentifikationsnummer", "", retrievedCompany["SteuerlicheIdentifikationsnummer"]);

					var emptyAddr = Factory.New<OrgAddress>();
					AssertEquals("Address", (emptyAddr.OA_Address1 + ' ' + emptyAddr.OA_Address2).TrimEnd().TrimStart(), retrievedCompany["Anschrift"]);
					AssertEquals("PLZ", "", retrievedCompany["Postleitzahl"]);
					AssertEquals("City", "", retrievedCompany["Ort"]);
					AssertEquals("State", "", retrievedCompany["Bundesland"]);
					AssertEquals("Country", "", retrievedCompany["Land"]);
					AssertEquals("Phone Number", "0112655555", retrievedCompany["Telefon"]);
					AssertEquals("Fax", "0112655556", retrievedCompany["Fax"]);
					AssertEquals("E-mail", "www.companyemail@xyz.com", retrievedCompany["Email"]);
					AssertEquals("Website", "www.companywebsite.com", retrievedCompany["Internet"]);
					AssertEquals("Currency", "EUR", retrievedCompany["Fibuwaehrung"]);
				}
			}
		}

		[TestDate()]
		public void TestWIPIdea_BelegnummerNotEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var firstDay = new ZDate(ZDate.Today.Year, 1, 1);
				var lastDay = new ZDate(ZDate.Today.Year, 12, 31);

				var t = new TestObjectCreator(Factory);
				var shipment = t.CreateShipment("SHP007");
				var job = t.CreateJob(shipment, createWithMutex: false);
				var charge = t.CreateCharge(job, t.CC1, "CC1", t.EUR, 1000, t.Creditor1, "SHP007", t.EUR, 1000, t.AALSHI);

				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_DateFrom = firstDay;
				complianceReport.ACR_DateTo = lastDay;
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
				Factory.Save();

				var jobTransactions = Factory.Load<TransactionLine>(new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK));

				AssertEquals("Transaction Line Count", 2, jobTransactions.Length);
				Creator.CreateComplianceReportQueueEntry(complianceReport, $"*JC*{jobTransactions[0].AL_LineType}**", null, jobTransactions[0]);
				Creator.CreateComplianceReportQueueEntry(complianceReport, $"*JC*{jobTransactions[1].AL_LineType}**", null, jobTransactions[1]);

				var data = RetrieveAccountsMovement(Env.CurrentCompanyPK, firstDay, lastDay);
				var isEmpty = data.Any(x => string.IsNullOrEmpty(x["Belegnummer"].ToString()));
				Assert("Belegnummer should not be empty", !isEmpty);

				var jobNumberFound = data.Where(x => x["Belegnummer"].ToString() == job.JH_JobNum).Any();
				Assert("Belegnummer should contain Jobnumber", jobNumberFound);
			}
		}

		DynamicBusinessObject[] RetrieveAccountsMovement(ZGuid company, ZDate startDate, ZDate endDate)
		{
			var controlAccountAndReportSubCodeMapping = new ControlAccountAndReportSubCodeMapping();
			var mappingTable = controlAccountAndReportSubCodeMapping.AccountPkToSubCodesTable;
			var command = Db.Connection.Command(IDEADataProvider.AccMovementsSQL);
			command.AddParameterBasedOnDbColumn("@Company", company.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			command.AddParameterBasedOnDbColumn("@StartDate", startDate.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);
			command.AddParameterBasedOnDbColumn("@EndDate", endDate.ToDateTime().Date, AccTransactionComplianceReportQueueSchema.ACQ_Date);
			command.AddTableValuedParameter("@SubCodeToGLAccountMapping", "dbo.TVP_CodeToGuidMapping", mappingTable);

			var dataTable = DataUtils.GetDataTableFromCommand(command);
			var rows = dataTable.Select("");
			var bOs = new DynamicBusinessObject[rows.Length];
			for (var i = 0; i < rows.Length; i++)
			{
				bOs[i] = new DynamicBusinessObject(new BusinessObjectFactory(), rows[i]);
			}

			return bOs;
		}

		void SetAddressData(OrgAddress orgAddress, string address1, string address2, string code, string city, string state, string countryCode)
		{
			orgAddress.Address1 = address1;
			orgAddress.OA_Address2 = address2;
			orgAddress.OA_Code = code;
			orgAddress.OA_City = city;
			orgAddress.OA_State = state;
			orgAddress.OA_RN_NKCountryCode = countryCode;
		}

		void AssertOrganisation(OrgAddress addr, DynamicBusinessObject retrieved)
		{
			AssertEquals("Wrong Address1+2", (addr.OA_Address1 + ' ' + addr.OA_Address2).TrimEnd().TrimStart(), retrieved["Anschrift"]);
			AssertEquals("Wrong City", addr.OA_City, retrieved["Ort"]);
			AssertEquals("Wrong State", addr.OA_State, retrieved["Bundesland"]);
		}

		[TestDate(2023, 12, 19, 13, 43, 0)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateReport_WithoutMappings() => TestGenerateReport_Core(@"No GL accounts have been mapped to German (account-numbers and -descriptions) using the GL Multi-Language Mapping.
This means that only the standard chart of accounts (account-numbers and -descriptions) will be exported in this IDEA export.", CreateMappings.CreateNoMappings);

		[TestDate(2023, 12, 19, 13, 43, 0)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateReport_WithPartialMappings() => TestGenerateReport_Core(@"Part of the GL accounts have been mapped to German (account-numbers and -descriptions) using the GL Multi-Language Mapping. 
This means that the standard chart of accounts (account-numbers and -descriptions) will be exported in this IDEA export along with the German mapping of those GL accounts that have been mapped.
It should be considered to map the un-mapped accounts which are:
TEST.10.00 - Gemeinkosten
TEST.20.00 - Gemeinkosten", CreateMappings.CreatePartialMapping);

		[TestDate(2023, 12, 19, 13, 43, 0)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateReport_WithFullMappings() => TestGenerateReport_Core("", CreateMappings.CreateFullMapping);

		void TestGenerateReport_Core(string notesText, CreateMappings createMappings)
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			Db.Connection.BeginTransaction();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew(IDEADataProvider.TaxRegistrationCode, "I4/554/45755", Constants.CountryCodes.Germany);
			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
			complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 5, 31);
			Creator.CreateTestPeriods(new ZDateTime(2020, 1, 1));
			var account1 = CreateAccount("TEST.10.00", "OV", "Gemeinkosten", "P&L", "CR");
			var account2 = CreateAccount("TEST.20.00", "OV", "Gemeinkosten", "P&L", "DR");
			var account3 = Creator.CreateGLHeader("TEST.30.00");
			var account4 = Creator.CreateGLHeader("TEST.40.00");
			var account5 = Creator.CreateGLHeader("TEST.50.00");
			var account6 = Creator.CreateGLHeader("TEST.60.00");

			if (createMappings == CreateMappings.CreateFullMapping)
			{
				Creator.CreateLocalAccountMappingForGLHeader(account1, Constants.Languages.German, Constants.CountryCodes.Germany);
				Creator.CreateLocalAccountMappingForGLHeader(account2, Constants.Languages.German, Constants.CountryCodes.Germany);
			}
			if (createMappings == CreateMappings.CreatePartialMapping || createMappings == CreateMappings.CreateFullMapping)
			{
				Creator.CreateLocalAccountMappingForGLHeader(Creator.CreateRetainedEarningsAccount(), Constants.Languages.German, Constants.CountryCodes.Germany);
				LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.CFXAccount.Value);
				LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
				LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);
				LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value);
				LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
				LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value);
				LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);

				Creator.CreateLocalAccountMappingForGLHeader(account3, Constants.Languages.German, Constants.CountryCodes.Germany);
				Creator.CreateLocalAccountMappingForGLHeader(account4, Constants.Languages.German, Constants.CountryCodes.Germany);
				Creator.CreateLocalAccountMappingForGLHeader(account5, Constants.Languages.German, Constants.CountryCodes.Germany);
				Creator.CreateLocalAccountMappingForGLHeader(account6, Constants.Languages.German, Constants.CountryCodes.Germany);
			}

			var org1 = CreateOrganisation("CUSTOMER1");
			var org2 = CreateOrganisation("CUSTOMER2");

			_ = Creator.CreateCustomsCodes(org2, Constants.CountryCodes.Germany, IDEADataProvider.TaxRegistrationCode, "I4/554/45754");

			CreateApTransaction(complianceReport, "INV001", 600, new ZDate(2020, 1, 1), org1, account1);
			CreateApTransaction(complianceReport, "INV002", -500, new ZDate(2020, 3, 1), org1, account2);
			CreateApTransaction(complianceReport, "INV003", 300, new ZDate(2020, 5, 1), org2, account1);

			var shipment = Creator.CreateShipment("SHP007");
			var job = Creator.CreateJob(shipment, createWithMutex: false);
			var charge = Creator.CreateCharge(job, Creator.CC1, "CC1", Creator.EUR, 1000, Creator.Creditor1, "SHP007", Creator.EUR, 1000, Creator.AALSHI);
			charge.WIPAccrualCreationDate = new ZDateTime(2020, 5, 2);

			var journal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 10));
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
			var journalTransactions = new TransactionLine[] {
				Creator.CreateGLJournalLine(journal, 350m, DebitCredit.DR, account3.PK),
				Creator.CreateGLJournalLine(journal, 350m, DebitCredit.CR, account4.PK) };

			Factory.Save();

			var jobTransactions = Factory.Load<TransactionLine>(new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK));

			AssertEquals("Transaction Line Count", 2, jobTransactions.Length);

			jobTransactions[0].AL_AG = (jobTransactions[0].AL_LineType == "WIP") ? account6.PK : account5.PK;
			jobTransactions[1].AL_AG = (jobTransactions[1].AL_LineType == "WIP") ? account6.PK : account5.PK;

			Creator.CreateComplianceReportQueueEntry(complianceReport, $"*JC*{jobTransactions[0].AL_LineType}**", null, jobTransactions[0]);
			Creator.CreateComplianceReportQueueEntry(complianceReport, $"*JC*{jobTransactions[1].AL_LineType}**", null, jobTransactions[1]);

			Creator.CreateComplianceReportQueueEntry(complianceReport, journalTransactions);

			var ajlJournal = Creator.CreateGLJournal<GLJournal>(transactionType: "AJL", invoiceDate: new ZDate(2020, 1, 3), postDate: new ZDate(2020, 1, 31), dueDate: new ZDate(2020, 3, 1));
			var ajlLine1 = Creator.CreateGLJournalLine(ajlJournal, 250M, DebitCredit.DR, account1.PK);
			var ajlLine2 = Creator.CreateGLJournalLine(ajlJournal, 250M, DebitCredit.CR, account2.PK);

			var rjlJournal = Creator.CreateGLJournal<GLJournal>(transactionType: "RJL", invoiceDate: new ZDate(2020, 1, 5), postDate: new ZDate(2020, 1, 31), dueDate: new ZDate(2020, 3, 1));
			var rjlLine1 = Creator.CreateGLJournalLine(rjlJournal, 250M, DebitCredit.DR, account1.PK);
			var rjlLine2 = Creator.CreateGLJournalLine(rjlJournal, 250M, DebitCredit.CR, account2.PK);

			// create an AR/AP contra
			var contra = CreateContra(new DateTime(2020, 04, 12));

			// create a TFR transfer
			var transfer = Creator.CreateTransfer<ARTransfer>(55.0M, new DateTime(2020, 04, 18), org1.PK, org2.PK);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202301", null, ajlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202301", null, ajlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202302", new ZDate(2020, 2, 29), ajlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202302", new ZDate(2020, 2, 29), ajlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202303", new ZDate(2020, 3, 1), ajlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202303", new ZDate(2020, 3, 1), ajlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**", null, rjlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**", null, rjlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**Rev-", new ZDate(2020, 3, 1), rjlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**Rev-", new ZDate(2020, 3, 1), rjlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.APRow }, (_) => new ZDate(2020, 04, 12), (_) => "*AP*CTR*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.ARRow }, (_) => new ZDate(2020, 04, 12), (_) => "*AR*CTR*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { transfer.TransferFrom }, (_) => new ZDate(2020, 04, 18), (_) => "*AR*CTR*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { transfer.TransferTo }, (_) => new ZDate(2020, 04, 18), (_) => "*AR*CTR*ARCtrl*");

			Db.Connection.CommitTransaction();
			var reportPK = complianceReport.PK;
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			IDEATaxAuditExport ideaExport;
			var serviceLogger = new DummyLogger();
			bool allDone;
			string statusMessage;
			IDEAPersistentData persistentData = null;
			var stepCounter = 0;
			do  // simulate CRQ service task calling the IDEA export multiple times but stop after 5 runs to prevent an infinite loop in case ExportData() does not return TRUE as expected
			{
				var factory = new BusinessObjectFactory();
				complianceReport = factory.Load<AccComplianceReport>(reportPK);

				using (complianceReport.Factory.AddDisposableService())
				{
					ideaExport = factory.New<IDEATaxAuditExport>();
					(allDone, statusMessage) = ideaExport.ExportData(complianceReport, serviceLogger);
					factory.Save();
					stepCounter++;
					if (!allDone)
					{
						AssertEquals($"Value of AccComplianceReport.NextProcessingStepFromDate after step {stepCounter}", new ZDate(2020, stepCounter + 1, 1), complianceReport.NextProcessingStepFromDate);
						persistentData = IDEAPersistentData.Load(complianceReport);
						AssertEquals("StmNote record exists while exporting", true, persistentData.AccountBalancePerPeriod.Count > 0);
					}
				}
			} while (!allDone && stepCounter < 5);

			// assert that the log doesn't contain a message for the missing account number for CTR and TFR
			var log = serviceLogger.ToString();
			AssertNotContains("No account entered for transaction number CTR00001000", log);
			AssertNotContains("No account entered for transaction number ARTFR00001000", log);

			AssertEquals("The last export step should have return value", true, allDone);
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			var accountBalancePerPeriod = persistentData.AccountBalancePerPeriod.OrderBy(x => x.Key);
			var accountLastPostDate = persistentData.AccountLastPostDate.OrderBy(x => x.Key);
			var allAccounts = persistentData.AllAccounts.ToList().OrderBy(x => x);
			var allOrganisations = persistentData.AllOrganisations.ToList().OrderBy(x => x);
			var allCSVFiles = persistentData.AllCSVFiles.ToList().OrderBy(x => x);

			var actualPersistentData = $@"# AccountBalancePerPeriod
{string.Join(System.Environment.NewLine, accountBalancePerPeriod)}

# AccountLastPostDate
{string.Join(System.Environment.NewLine, accountLastPostDate)}

# AllAccounts
{string.Join(System.Environment.NewLine, allAccounts)}

# AllOrganisations
{string.Join(System.Environment.NewLine, allOrganisations)}

# AllCSVFiles
{string.Join(System.Environment.NewLine, allCSVFiles)}
";
			var expectedPersistentData = GetEmbeddedResourceAsZString("persistentdata.txt");
			AssertEquals("Persistent data", expectedPersistentData, actualPersistentData);

			AssertEquals("One eDoc added", 5, ideaExport.DocManagerInfo.AllEDocs.Count);
			var filename = ideaExport.DocManagerInfo.AllEDocs[0].FileName;
			AssertStartsWith("Name of created ZIP file starts with", "IDEA", filename);
			AssertEndsWith("Name of created ZIP file has suffix", ".zip", filename);

			var createdFiles = GetUnzippedContent(ideaExport.DocManagerInfo);
			AssertEquals("Eleven eDocs added", 11, createdFiles.Count);
			AssertFileContent(createdFiles, true, "kontobuchungen-20200101-20200131.csv", GetEmbeddedResourceAsZString("kontobuchungen01.csv"));
			AssertFileContent(createdFiles, true, "kontobuchungen-20200201-20200229.csv", GetEmbeddedResourceAsZString("kontobuchungen02.csv"));
			AssertFileContent(createdFiles, true, "kontobuchungen-20200301-20200331.csv", GetEmbeddedResourceAsZString("kontobuchungen03.csv"));
			AssertFileContent(createdFiles, true, "kontobuchungen-20200401-20200430.csv", GetEmbeddedResourceAsZString("kontobuchungen04.csv"));
			AssertFileContent(createdFiles, true, "kontobuchungen-20200501-20200531.csv", GetEmbeddedResourceAsZString("kontobuchungen05.csv"));
			var sachkontenstammFile = "";
			switch (createMappings)
			{
				case CreateMappings.CreateNoMappings:
					sachkontenstammFile = "sachkontenstamm_noMapping.csv";
					break;
				case CreateMappings.CreatePartialMapping:
					sachkontenstammFile = "sachkontenstamm_partialMapping.csv";
					break;
				case CreateMappings.CreateFullMapping:
					sachkontenstammFile = "sachkontenstamm_fullMapping.csv";
					break;
			}

			AssertFileContent(createdFiles, true, "sachkontenstamm.csv", GetEmbeddedResourceAsZString(sachkontenstammFile));
			AssertFileContent(createdFiles, true, "mandantendaten.csv", GetEmbeddedResourceAsZString("mandantendaten.csv"));
			AssertFileContent(createdFiles, true, "debitorenkreditorenstammdaten.csv", GetEmbeddedResourceAsZString("debitorenkreditorenstammdaten.csv"));
			AssertFileContent(createdFiles, true, "mvz.csv", GetEmbeddedResourceAsZString("mvz.csv"));
			AssertFileContent(createdFiles, true, "index.xml", GetEmbeddedResourceAsZString("expected-index.xml"));
			AssertFileContent(createdFiles, false, "gdpdu-01-08-2002.dtd", "");

			if (createMappings != CreateMappings.CreateFullMapping)
			{
				// in case of full mappings the status is not set to GEN in this test because it would be done in AccComplianceReport.GenerateIDEAFromQueue() which is not called.
				AssertEquals("The status message is wrong.", "Account mapping is incomplete. Please check the Notes tab for details.", statusMessage);
			}

			var notes = complianceReport.GetNotes().GetAllNotes();
			var expectedNoOfNotes = createMappings == CreateMappings.CreateFullMapping ? 0 : 1;
			AssertEquals("Number of notes attached to report", expectedNoOfNotes, notes.Count);
			if (notes.Count > 0)
			{
				var firstNote = notes.Cast<StmNote>().First();
				AssertEquals("Note text", notesText, firstNote.ST_NoteDataAsText);
			}
		}

		[TestDate(2023, 12, 19, 13, 43, 0)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateReportFails_WithMissingControlAccount()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			Db.Connection.BeginTransaction();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew(IDEADataProvider.TaxRegistrationCode, "I4/554/45755", Constants.CountryCodes.Germany);
			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
			complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 5, 31);
			Creator.CreateTestPeriods(new ZDateTime(2020, 1, 1));
			var account1 = CreateAccount("TEST.10.00", "OV", "Gemeinkosten", "P&L", "CR");
			var account2 = CreateAccount("TEST.20.00", "OV", "Gemeinkosten", "P&L", "DR");
			var account3 = Creator.CreateGLHeader("TEST.30.00");
			var account4 = Creator.CreateGLHeader("TEST.40.00");
			var account5 = Creator.CreateGLHeader("TEST.50.00");
			var account6 = Creator.CreateGLHeader("TEST.60.00");

			// Add control account mapping
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);

			// Set an empty registry value for control account
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var org1 = CreateOrganisation("CUSTOMER1");
			var org2 = CreateOrganisation("CUSTOMER2");

			Creator.CreateCustomsCodes(org2, Constants.CountryCodes.Germany, IDEADataProvider.TaxRegistrationCode, "I4/554/45754");

			// create an AR/AP contra
			var contra = CreateContra(new DateTime(2020, 04, 12));

			// create a TFR transfer
			var transfer = Creator.CreateTransfer<ARTransfer>(55.0M, new DateTime(2020, 04, 18), org1.PK, org2.PK);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.APRow }, (_) => new ZDate(2020, 04, 12), (_) => "*AP*CTR*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.ARRow }, (_) => new ZDate(2020, 04, 12), (_) => "*AR*CTR*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { transfer.TransferFrom }, (_) => new ZDate(2020, 04, 18), (_) => "*AR*CTR*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { transfer.TransferTo }, (_) => new ZDate(2020, 04, 18), (_) => "*AR*CTR*ARCtrl*");

			Db.Connection.CommitTransaction();
			var reportPK = complianceReport.PK;
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			var serviceLogger = new DummyLogger();
			var stepCounter = 0;

			// simulate CRQ service task calling the IDEA export multiple times but stop after 5 runs to prevent an infinite loop in case ExportData() does not return TRUE as expected
			AssertExceptionThrown(typeof(InvalidOperationException), () =>
			{
				do
				{
					var factory = new BusinessObjectFactory();
					complianceReport = factory.Load<AccComplianceReport>(reportPK);

					using (complianceReport.Factory.AddDisposableService())
					{
						var ideaExport = factory.New<IDEATaxAuditExport>();
						ideaExport.ExportData(complianceReport, serviceLogger);
						factory.Save();
						stepCounter++;
					}
				} while (stepCounter < 5);
			});
		}

		Contra CreateContra(ZDateTime postDate)
		{
			// create an AP contra
			var contra = Creator.CreateContra(100m, postDate, Creator.Debtor.PK, Creator.ABIGAS.PK);
			contra.APRow.AH_PostDate = postDate;
			contra.APRow.AH_InvoiceDate = postDate;
			contra.APRow.AH_Desc = "AP CONTRA";

			contra.ARRow.AH_PostDate = postDate;
			contra.ARRow.AH_InvoiceDate = postDate;
			contra.ARRow.AH_Desc = "RECEIVABLE AND PAYABLE CONTRA";

			return contra;
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 12, 19, 13, 43, 0)]
		public void TestEndOfRangeTransactionsInKontobuchungen()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			Db.Connection.BeginTransaction();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = new ZDate(2023, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2023, 12, 31);
			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;

			Creator.CreateTestPeriods(new ZDateTime(2023, 1, 1));
			var account1 = CreateAccount("TEST.10.00", "OV", "Gemeinkosten", "P&L", "CR");
			var account2 = CreateAccount("TEST.20.00", "OV", "Gemeinkosten", "P&L", "DR");
			var org1 = CreateOrganisation("CUSTOMER1");
			var org2 = CreateOrganisation("CUSTOMER2");
			CreateApTransaction(complianceReport, "INV001", 400, new ZDate(2023, 8, 1), org1, account1);
			CreateApTransaction(complianceReport, "INV002", -300, new ZDate(2023, 8, 10), org1, account2);
			CreateApTransaction(complianceReport, "INV003", 200, new ZDate(2023, 8, 31), org2, account1);
			Factory.Save();
			Db.Connection.CommitTransaction();
			var reportPK = complianceReport.PK;
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			IDEATaxAuditExport ideaExport;
			var serviceLogger = new DummyLogger();
			bool allDone;
			var stepCounter = 0;

			do  // simulate CRQ service task calling the IDEA export multiple times but stop after 12 runs to prevent an infinite loop in case ExportData() does not return TRUE as expected
			{
				var factory = new BusinessObjectFactory();
				complianceReport = factory.Load<AccComplianceReport>(reportPK);

				using (complianceReport.Factory.AddDisposableService())
				{
					ideaExport = factory.New<IDEATaxAuditExport>();
					(allDone, _) = ideaExport.ExportData(complianceReport, serviceLogger);
					factory.Save();
					stepCounter++;
					if (!allDone)
					{
						AssertEquals($"Value of AccComplianceReport.NextProcessingStepFromDate after step {stepCounter}", new ZDate(2023, stepCounter + 1, 1), complianceReport.NextProcessingStepFromDate);
						var persistentData = IDEAPersistentData.Load(complianceReport);
						AssertEquals("StmNote record exists while exporting", true, persistentData.AccountBalancePerPeriod.Any());
					}
				}
			} while (!allDone && stepCounter < 12);

			AssertEquals("The last export step should have return value", true, allDone);
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			AssertEquals("One eDoc added", 12, ideaExport.DocManagerInfo.AllEDocs.Count);
			var filename = ideaExport.DocManagerInfo.AllEDocs[0].FileName;
			AssertStartsWith("Name of created ZIP file", "IDEA", filename);
			AssertEndsWith("Name of created ZIP file", ".zip", filename);

			var createdFiles = GetUnzippedContent(ideaExport.DocManagerInfo);
			AssertEquals("Eighteen eDocs added", 18, createdFiles.Count);
			AssertFileContent(createdFiles, false, "kontobuchungen-20230101-20230131.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230201-20230228.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230301-20230331.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230401-20230430.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230501-20230531.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230601-20230630.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230701-20230731.csv", "");
			AssertFileContent(createdFiles, true, "kontobuchungen-20230801-20230831.csv", GetEmbeddedResourceAsZString("kontobuchungen23_08.csv"));
			AssertFileContent(createdFiles, false, "kontobuchungen-20230901-20230930.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20231001-20231031.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20231101-20231130.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20231201-20231231.csv", "");
		}

		void InternalTestApArTransactions(bool shareSequentialInvoiceReferenceNumbers, bool shareSequentialInvoiceTransactionNumbers, string embeddedCsvFilename)
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			Db.Connection.BeginTransaction();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();

			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
			complianceReport.ACR_DateFrom = new ZDate(2023, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2023, 12, 31);
			Creator.CreateTestPeriods(new ZDateTime(2023, 1, 1));
			var account1 = CreateAccount("TEST.10.00", "OV", "Gemeinkosten", "P&L", "CR");
			var account2 = CreateAccount("TEST.20.00", "OV", "Gemeinkosten", "P&L", "DR");

			var org1 = CreateOrganisation("CUSTOMER1");
			var org2 = CreateOrganisation("CUSTOMER2");

			CreateApTransaction(complianceReport, "INV001", 400, new ZDate(2023, 8, 1), org1, account1);
			CreateApTransaction(complianceReport, "INV002", -300, new ZDate(2023, 8, 10), org1, account2);
			CreateApTransaction(complianceReport, "INV003", 200, new ZDate(2023, 8, 31), org2, account1);

			CreateArTransaction(complianceReport, "INV004", 100, new ZDate(2023, 8, 2), org1, account1);
			CreateArTransaction(complianceReport, "INV005", 200, new ZDate(2023, 8, 5), org1, account2);
			CreateArTransaction(complianceReport, "INV006", 300, new ZDate(2023, 8, 15), org2, account1);

			Factory.Save();

			Db.Connection.CommitTransaction();

			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.SetValue(
				complianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty,
				new ShareSequentialReferenceNumbers() { Value = shareSequentialInvoiceReferenceNumbers });

			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.SetValue(
				complianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty,
				new ShareSequentialTransactionNumbers() { Value = shareSequentialInvoiceTransactionNumbers });

			IDEATaxAuditExport ideaExport;
			var serviceLogger = new DummyLogger();
			bool allDone;
			var stepCounter = 0;

			do  // simulate CRQ service task calling the IDEA export multiple times but stop after 12 runs to prevent an infinite loop in case ExportData() does not return TRUE as expected
			{
				var factory = new BusinessObjectFactory();
				complianceReport = factory.Load<AccComplianceReport>(complianceReport.PK);

				using (complianceReport.Factory.AddDisposableService())
				{
					ideaExport = factory.New<IDEATaxAuditExport>();
					(allDone, _) = ideaExport.ExportData(complianceReport, serviceLogger);
					factory.Save();
					stepCounter++;
				}
			} while (!allDone && stepCounter < 12);

			var createdFiles = GetUnzippedContent(ideaExport.DocManagerInfo);
			AssertFileContent(createdFiles, true, "kontobuchungen-20230801-20230831.csv", GetEmbeddedResourceAsZString(embeddedCsvFilename));
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 12, 19, 13, 43, 0)]
		public void TestApArTransactions_ShareSequentialInvoiceReferenceNumbers_False_ShareSequentialInvoiceTransactionNumbers_False()
		{
			InternalTestApArTransactions(shareSequentialInvoiceReferenceNumbers: false, shareSequentialInvoiceTransactionNumbers: false, "kontobuchungen23_08_ap_false_ar_false.csv");
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 12, 19, 13, 43, 0)]
		public void TestApArTransactions_ShareSequentialInvoiceReferenceNumbers_True_ShareSequentialInvoiceTransactionNumbers_True()
		{
			InternalTestApArTransactions(shareSequentialInvoiceReferenceNumbers: true, shareSequentialInvoiceTransactionNumbers: true, "kontobuchungen23_08_ap_true_ar_true.csv");
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 12, 19, 13, 43, 0)]
		public void TestApArTransactions_ShareSequentialInvoiceReferenceNumbers_True_ShareSequentialInvoiceTransactionNumbers_False()
		{
			InternalTestApArTransactions(shareSequentialInvoiceReferenceNumbers: true, shareSequentialInvoiceTransactionNumbers: false, "kontobuchungen23_08_ap_true_ar_false.csv");
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 12, 19, 13, 43, 0)]
		public void TestApArTransactions_ShareSequentialInvoiceReferenceNumbers_False_ShareSequentialInvoiceTransactionNumbers_True()
		{
			InternalTestApArTransactions(shareSequentialInvoiceReferenceNumbers: false, shareSequentialInvoiceTransactionNumbers: true, "kontobuchungen23_08_ap_false_ar_true.csv");
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 12, 19, 13, 43, 0)]
		public void TestBelegPositionNumberStartwithOne()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			Db.Connection.BeginTransaction();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = new ZDate(2023, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2023, 12, 31);
			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;

			Creator.CreateTestPeriods(new ZDateTime(2023, 1, 1));
			var receipt = CreateARReceipt(complianceReport, new ZDate(2023, 9, 1));
			Factory.Save();
			Db.Connection.CommitTransaction();
			var reportPK = complianceReport.PK;
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			IDEATaxAuditExport ideaExport;
			var serviceLogger = new DummyLogger();
			bool allDone;
			IDEAPersistentData persistentData = null;
			var stepCounter = 0;
			do // simulate CRQ service task calling the IDEA export multiple times but stop after 12 runs to prevent an infinite loop in case ExportData() does not return TRUE as expected
			{
				var factory = new BusinessObjectFactory();
				complianceReport = factory.Load<AccComplianceReport>(reportPK);
				using (factory.AddDisposableService())
				{
					ideaExport = factory.New<IDEATaxAuditExport>();
					(allDone, _) = ideaExport.ExportData(complianceReport, serviceLogger);
					factory.Save();
				}

				stepCounter++;

				if (!allDone)
				{
					AssertEquals($"Value of AccComplianceReport.NextProcessingStepFromDate after step {stepCounter}", new ZDate(2023, stepCounter + 1, 1),
						complianceReport.NextProcessingStepFromDate);
					persistentData = IDEAPersistentData.Load(complianceReport);
					AssertEquals("StmNote record exists while exporting", true, persistentData.AccountBalancePerPeriod.Any());
				}
			} while (!allDone && stepCounter < 12);

			AssertEquals("The last export step should have return value", true, allDone);
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			AssertEquals("One eDoc added", 12, ideaExport.DocManagerInfo.AllEDocs.Count);
			var filename = ideaExport.DocManagerInfo.AllEDocs[0].FileName;
			AssertStartsWith("Name of created ZIP file", "IDEA", filename);
			AssertEndsWith("Name of created ZIP file", ".zip", filename);

			var createdFiles = GetUnzippedContent(ideaExport.DocManagerInfo);
			AssertEquals("Eighteen eDocs added", 18, createdFiles.Count);
			AssertFileContent(createdFiles, false, "kontobuchungen-20230101-20230131.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230201-20230228.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230301-20230331.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230401-20230430.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230501-20230531.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230601-20230630.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230701-20230731.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20230801-20230831.csv", "");
			AssertFileContent(createdFiles, true, "kontobuchungen-20230901-20230930.csv", GetEmbeddedResourceAsZString("kontobuchungen_belegposition.csv"));
			AssertFileContent(createdFiles, false, "kontobuchungen-20231001-20231031.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20231101-20231130.csv", "");
			AssertFileContent(createdFiles, false, "kontobuchungen-20231201-20231231.csv", "");
		}

		[TestDate(2024, 03, 28, 13, 43, 0)]
		public void TestCancelationFlagInRetrieveAccountsMovement()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var firstDay = new ZDate(2024, 3, 1);
				var lastDay = new ZDate(2024, 3, 31);
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_DateFrom = firstDay;
				complianceReport.ACR_DateTo = lastDay;
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;

				Creator.CreateTestPeriods(new ZDateTime(2023, 1, 1));

				var testReceipt1 = Factory.NewWithValidTestData<ARReceipt>();
				testReceipt1.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
				testReceipt1.AH_ReceiptBatchNo = "00001580";
				Factory.Save();

				var testReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
				testReceipt2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
				testReceipt2.AH_ReceiptBatchNo = "00001581";
				Factory.Save();

				var reversing = new ReceiptReversing(testReceipt1);
				reversing.Reverse();
				Assert("Receipt 1 is Canceled", testReceipt1.AH_IsCancelled);
				Assert("Receipt 2 is not  Canceled", !testReceipt2.AH_IsCancelled);
				Factory.Save();

				Creator.CreateComplianceReportQueueEntry(complianceReport, testReceipt1);
				Creator.CreateComplianceReportQueueEntry(complianceReport, testReceipt2);

				var data = RetrieveAccountsMovement(Env.CurrentCompanyPK, firstDay, lastDay);
				var isEmpty = data.Any(x => string.IsNullOrEmpty(x["Storniert"].ToString()));
				Assert("Storniert Flag should not be empty", !isEmpty);

				var transaction1 = data.FirstOrDefault(x => x["Belegnummer"].ToString() == testReceipt1.AH_TransactionNum);
				AssertEquals("Storniert Flag should be Y", "Y", transaction1["Storniert"]);

				var transaction2 = data.FirstOrDefault(x => x["Belegnummer"].ToString() == testReceipt2.AH_TransactionNum);
				AssertEquals("Storniert Flag should be N", "N", transaction2["Storniert"]);
			}
		}

		public void TestFormatAmount()
		{
			AssertEquals("Integer value", "123.00", IDEATaxAuditExport.FormatAmount("123", 2));
			AssertEquals("Decimal value with fewer digits", "123.10", IDEATaxAuditExport.FormatAmount("123.1", 2));
			AssertEquals("Decimal value with too many digits", "123.12", IDEATaxAuditExport.FormatAmount("123.1234", 2));
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2023, 12, 19, 13, 43, 0)]
		public void TestRetrieveTransactionData_WhenInternalChunkSizeExceeded()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			Db.Connection.BeginTransaction();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;

			Creator.CreateTestPeriods(new ZDateTime(2020, 1, 1));
			var account1 = CreateAccount("TEST.10.00", "OV", "Gemeinkosten", "P&L", "CR");
			var org1 = CreateOrganisation("CUSTOMER1");
			for (var i = 0; i < 10; i++)
			{
				CreateApTransaction(complianceReport, $"INV{i + 1:D3}", i * 100 + 100, new ZDate(2020, 1, i + 1), org1, account1);
			}
			Factory.Save();
			Db.Connection.CommitTransaction();

			using (complianceReport.Factory.AddDisposableService())
			{
				var maxChunkSizeForTesting = 600;
				var ideaExport = Factory.New<IDEATaxAuditExport>();
				var serviceLogger = new DummyLogger();
				ideaExport.ExportData(complianceReport, serviceLogger, maxChunkSizeForTesting);

				var createdFiles = GetUnzippedContent(ideaExport.DocManagerInfo);
				AssertFileContent(createdFiles, true, "kontobuchungen-20200101-20200131.csv", GetEmbeddedResourceAsZString("kontobuchungen_chunktest.csv"));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestRollback()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			Db.Connection.BeginTransaction();
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;

			Creator.CreateTestPeriods(new ZDateTime(2020, 1, 1));
			var account1 = CreateAccount("TEST.10.00", "OV", "Gemeinkosten", "P&L", "CR");
			var account2 = CreateAccount("TEST.20.00", "OV", "Gemeinkosten", "P&L", "DR");
			var org1 = CreateOrganisation("CUSTOMER1");
			var org2 = CreateOrganisation("CUSTOMER2");
			CreateApTransaction(complianceReport, "INV001", 600, new ZDate(2020, 1, 1), org1, account1);
			CreateApTransaction(complianceReport, "INV002", -500, new ZDate(2020, 3, 1), org1, account2);
			CreateApTransaction(complianceReport, "INV003", 300, new ZDate(2020, 5, 1), org2, account1);
			Factory.Save();
			Db.Connection.CommitTransaction();
			var reportPK = complianceReport.PK;

			using (complianceReport.Factory.AddDisposableService())
			{
				// generate first 2 steps (months)
				var ideaExport = Factory.New<IDEATaxAuditExport>();
				var serviceLogger = new DummyLogger();
				ideaExport.ExportData(complianceReport, serviceLogger);
				Factory.Save();
				ideaExport.ExportData(complianceReport, serviceLogger);
				Factory.Save();
				AssertEquals("Value of AccComplianceReport.NextProcessingStepFromDate", new ZDate(2020, 3, 1), complianceReport.NextProcessingStepFromDate);
				AssertEquals("Two ZIP files created", 2, ideaExport.DocManagerInfo.AllEDocs.Count);
				var stmNoteDataBeforeRollback = GetStmNoteData(complianceReport);
				AssertEquals("StmNote record exists", true, stmNoteDataBeforeRollback.Length > 0);

				// fail next step with exception
				try
				{
					complianceReport.ACR_PageNumberFrom = 4;
					complianceReport.ACR_PageNumberTo = 3;
					ideaExport.ExportData(complianceReport, serviceLogger);
					Factory.Save();
				}
				catch (ZSaveException)
				{
					// there is a database constraint which causes the saving to fail because ACR_PageNumberFrom > ACR_PageNumberTo
				}

				var factory = new BusinessObjectFactory();
				complianceReport = factory.Load<AccComplianceReport>(reportPK);
				AssertEquals("Value of AccComplianceReport.NextProcessingStepFromDate after error", new ZDate(2020, 3, 1), complianceReport.NextProcessingStepFromDate);
				AssertEquals("Two ZIP files created after error", 2, complianceReport.DocManagerInfo().AllEDocs.Count);
				var stmNoteDataAfterRollback = GetStmNoteData(complianceReport);
				AssertEquals("StmNote data", stmNoteDataBeforeRollback, stmNoteDataAfterRollback);
			}
		}

		[TestDate(2023, 12, 19, 13, 43, 0)]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestCsvLineBreakFiltering()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany);
			Db.Connection.BeginTransaction();

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew(IDEADataProvider.TaxRegistrationCode, "I4/554/45755", CountryCodes.Germany);
			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
			complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 5, 31);
			Creator.CreateTestPeriods(new ZDateTime(2020, 1, 1));
			var account1 = CreateAccount("TEST.10.00", "OV", "Gemeinkosten\r\nCR", "P&L", "CR");
			var account2 = CreateAccount("TEST.20.00", "OV", "Gemeinkosten\nDR", "P&L", "DR");
			var account3 = Creator.CreateGLHeader("TEST.30.00");
			var account4 = Creator.CreateGLHeader("TEST.40.00");
			var account5 = Creator.CreateGLHeader("TEST.50.00");
			var account6 = Creator.CreateGLHeader("TEST.60.00");

			Creator.CreateLocalAccountMappingForGLHeader(account1, Languages.German, CountryCodes.Germany);
			Creator.CreateLocalAccountMappingForGLHeader(account2, Languages.German, CountryCodes.Germany);
			Creator.CreateLocalAccountMappingForGLHeader(account3, Languages.German, CountryCodes.Germany);
			Creator.CreateLocalAccountMappingForGLHeader(account4, Languages.German, CountryCodes.Germany);
			Creator.CreateLocalAccountMappingForGLHeader(account5, Languages.German, CountryCodes.Germany);
			Creator.CreateLocalAccountMappingForGLHeader(account6, Languages.German, CountryCodes.Germany);
			Creator.CreateLocalAccountMappingForGLHeader(Creator.CreateRetainedEarningsAccount(), Languages.German, CountryCodes.Germany);
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.CFXAccount.Value);
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value);
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value);
			LoadAccountAndCreateMappingForGLHeader(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);

			var org1 = CreateOrganisation("CUSTOMER1", "Customer\r\n1");
			var org2 = CreateOrganisation("CUSTOMER2", "Customer\n2");
			Creator.CreateCustomsCodes(org2, CountryCodes.Germany, IDEADataProvider.TaxRegistrationCode, "I4/554/45754");

			CreateApTransaction(complianceReport, "INV001", 600, new ZDate(2020, 1, 1), org1, account1, "Normal invoice line");
			CreateApTransaction(complianceReport, "INV002", -500, new ZDate(2020, 3, 1), org1, account2, "Invoice line\r\nwith line-breaks\rof different\ntypes\r\n");
			CreateApTransaction(complianceReport, "INV003", 300, new ZDate(2020, 5, 1), org2, account1, "Invoice line with line-break at the end\n");

			var shipment = Creator.CreateShipment("SHP007");
			var job = Creator.CreateJob(shipment, createWithMutex: false);

			var chargeDescriptions = new[]
			{
				"CC1\r\nLINE-BREAK", "CC1\nLINE-BREAK", "CC1\rLINE-BREAK", "CC1\n\rLINE-BREAK",		// Line-break in the middle
				"CC1 LINE-BREAK\r\n", "CC1 LINE-BREAK\n", "CC1 LINE-BREAK\r", "CC1 LINE-BREAK\n\r",	// Line-break at the end
				"\r\nCC1 LINE-BREAK", "\nCC1 LINE-BREAK", "\rCC1 LINE-BREAK", "\n\rCC1 LINE-BREAK",	// Line-break at the start
			};
			foreach (var description in chargeDescriptions)
			{
				var charge = Creator.CreateCharge(job, Creator.CC1, description, Creator.EUR, 1000, Creator.Creditor1, "SHP007", Creator.EUR, 1000, Creator.AALSHI);
				charge.WIPAccrualCreationDate = new ZDateTime(2020, 5, 2);
			}

			var journal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 10));
			journal.AH_TransactionCategory = TransactionCategory.Codes.Clearing;
			var journalTransactions = new TransactionLine[] {
				Creator.CreateGLJournalLine(journal, 350m, DebitCredit.DR, account3.PK),
				Creator.CreateGLJournalLine(journal, 350m, DebitCredit.CR, account4.PK) };

			Factory.Save();

			var jobTransactions = Factory.Load<TransactionLine>(new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK));
			AssertEquals("Transaction Line Count", 24, jobTransactions.Length);

			jobTransactions[0].AL_AG = (jobTransactions[0].AL_LineType == "WIP") ? account6.PK : account5.PK;
			jobTransactions[1].AL_AG = (jobTransactions[1].AL_LineType == "WIP") ? account6.PK : account5.PK;

			Creator.CreateComplianceReportQueueEntry(complianceReport, $"*JC*{jobTransactions[0].AL_LineType}**", null, jobTransactions[0]);
			Creator.CreateComplianceReportQueueEntry(complianceReport, $"*JC*{jobTransactions[1].AL_LineType}**", null, jobTransactions[1]);

			Creator.CreateComplianceReportQueueEntry(complianceReport, journalTransactions);

			var ajlJournal = Creator.CreateGLJournal<GLJournal>(transactionType: "AJL", invoiceDate: new ZDate(2020, 1, 3), postDate: new ZDate(2020, 1, 31), dueDate: new ZDate(2020, 3, 1));
			var ajlLine1 = Creator.CreateGLJournalLine(ajlJournal, 250M, DebitCredit.DR, account1.PK);
			var ajlLine2 = Creator.CreateGLJournalLine(ajlJournal, 250M, DebitCredit.CR, account2.PK);

			var rjlJournal = Creator.CreateGLJournal<GLJournal>(transactionType: "RJL", invoiceDate: new ZDate(2020, 1, 5), postDate: new ZDate(2020, 1, 31), dueDate: new ZDate(2020, 3, 1));
			var rjlLine1 = Creator.CreateGLJournalLine(rjlJournal, 250M, DebitCredit.DR, account1.PK);
			var rjlLine2 = Creator.CreateGLJournalLine(rjlJournal, 250M, DebitCredit.CR, account2.PK);

			// create an AR/AP contra
			var contra = CreateContra(new DateTime(2020, 04, 12));

			// create a TFR transfer
			var transfer = Creator.CreateTransfer<ARTransfer>(55.0M, new DateTime(2020, 04, 18), org1.PK, org2.PK);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202301", null, ajlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202301", null, ajlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202302", new ZDate(2020, 2, 29), ajlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202302", new ZDate(2020, 2, 29), ajlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202303", new ZDate(2020, 3, 1), ajlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*AJL**202303", new ZDate(2020, 3, 1), ajlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**", null, rjlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**", null, rjlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**Rev-", new ZDate(2020, 3, 1), rjlLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*GL*RJL**Rev-", new ZDate(2020, 3, 1), rjlLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.APRow }, (_) => new ZDate(2020, 04, 12), (_) => "*AP*CTR*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.ARRow }, (_) => new ZDate(2020, 04, 12), (_) => "*AR*CTR*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { transfer.TransferFrom }, (_) => new ZDate(2020, 04, 18), (_) => "*AR*CTR*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { transfer.TransferTo }, (_) => new ZDate(2020, 04, 18), (_) => "*AR*CTR*ARCtrl*");

			Db.Connection.CommitTransaction();
			var reportPK = complianceReport.PK;
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			IDEATaxAuditExport ideaExport;
			var serviceLogger = new DummyLogger();
			bool allDone;
			string statusMessage;
			IDEAPersistentData persistentData = null;
			var stepCounter = 0;
			do  // simulate CRQ service task calling the IDEA export multiple times but stop after 5 runs to prevent an infinite loop in case ExportData() does not return TRUE as expected
			{
				var factory = new BusinessObjectFactory();
				complianceReport = factory.Load<AccComplianceReport>(reportPK);

				using (complianceReport.Factory.AddDisposableService())
				{
					ideaExport = factory.New<IDEATaxAuditExport>();
					(allDone, statusMessage) = ideaExport.ExportData(complianceReport, serviceLogger);
					factory.Save();
					stepCounter++;
					if (!allDone)
					{
						AssertEquals($"Value of AccComplianceReport.NextProcessingStepFromDate after step {stepCounter}", new ZDate(2020, stepCounter + 1, 1), complianceReport.NextProcessingStepFromDate);
						persistentData = IDEAPersistentData.Load(complianceReport);
						AssertEquals("StmNote record exists while exporting", true, persistentData.AccountBalancePerPeriod.Count > 0);
					}
				}
			} while (!allDone && stepCounter < 5);

			// assert that the log doesn't contain a message for the missing account number for CTR and TFR
			var log = serviceLogger.ToString();
			AssertNotContains("No account entered for transaction number CTR00001000", log);
			AssertNotContains("No account entered for transaction number ARTFR00001000", log);

			AssertEquals("The last export step should have return value", true, allDone);
			AssertNull("StmNote", IDEAPersistentData.GetIDEANoteIfItExists(complianceReport));

			AssertEquals("One eDoc added", 5, ideaExport.DocManagerInfo.AllEDocs.Count);
			var filename = ideaExport.DocManagerInfo.AllEDocs[0].FileName;
			AssertStartsWith("Name of created ZIP file starts with", "IDEA", filename);
			AssertEndsWith("Name of created ZIP file has suffix", ".zip", filename);

			var createdFiles = GetUnzippedContent(ideaExport.DocManagerInfo);
			AssertEquals("Eleven eDocs added", 11, createdFiles.Count);

			List<(string FileName, string ResourceName)> expectedFiles =
			[
				("kontobuchungen-20200101-20200131.csv", "CsvLineBreakFilteringTest.kontobuchungen01.csv"),
				("kontobuchungen-20200301-20200331.csv", "CsvLineBreakFilteringTest.kontobuchungen03.csv"),
				("kontobuchungen-20200501-20200531.csv", "CsvLineBreakFilteringTest.kontobuchungen05.csv"),
				("sachkontenstamm.csv", "CsvLineBreakFilteringTest.sachkontenstamm.csv"),
				("debitorenkreditorenstammdaten.csv", "CsvLineBreakFilteringTest.debitorenkreditorenstammdaten.csv"),
			];
			foreach (var test in expectedFiles)
			{
				AssertFileContent(createdFiles, true, test.FileName, GetEmbeddedResourceAsZString(test.ResourceName));
			}

			var notes = complianceReport.GetNotes().GetAllNotes();
			AssertEquals("Number of notes attached to report", 0, notes.Count);
		}

		#region Implementation

		public enum CreateMappings
		{
			CreateNoMappings = 0,
			CreatePartialMapping = 1,
			CreateFullMapping = 2
		}

		protected override void SetUp()
		{
			CreateIDEAReportConfiguration();
			base.SetUp();
		}

		void LoadAccountAndCreateMappingForGLHeader(Guid accountPK)
		{
			var account = Factory.Load<AccGLHeader>(accountPK);
			Creator.CreateLocalAccountMappingForGLHeader(account, Constants.Languages.German, Constants.CountryCodes.Germany);
		}

		void CreateIDEAReportConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "IDE";
			reportConfig.ReportTitle = "Test Compliance Report";
			reportConfig.Country = "DE";
			reportConfig.TaxRegistrationType = "UST";
			reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBookWithPresentation;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			// This does not solve the problem of checking for DayBook reports. in GLAccountToLocalAccountMapping
			var reportTypeList = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			reportTypeList.Cast<CodeDescriptionBool>().Where(v => !v.Bool).ForEach(x => x.Bool = true);
			AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportTypeList);
		}

		AccGLHeader CreateAccount(ZString accountNum, ZString column, ZString description, ZString accountType, ZString debitCredit)
		{
			var account = Creator.CreateAccGLHeader(accountNum, column, description, accountType, debitCredit);
			return account;
		}

		AccGLAggregate CreateAccountAggregate(AccGLHeader account, decimal amount, int period)
		{
			var aggregate = Creator.CreateAccGLAggregate(amount, period, account.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			return aggregate;
		}

		OrgHeader CreateOrganisation(string code, string fullName = null)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			if (fullName != null)
			{
				org.OH_FullName = fullName;
			}
			return org;
		}

		AccTransactionHeader CreateApTransaction(AccComplianceReport complianceReport, ZString transactionNumber, ZDecimal amount, ZDate postDate, OrgHeader org, AccGLHeader account, string lineDescription = null)
		{
			var apInvoice = Creator.CreateAPInvoice<APInvoice>("AP0001", Creator.EUR, 1m, amount, amount / 10, 0m, amount, amount / 10, 0m, org);
			apInvoice.AH_PostDate = postDate;
			apInvoice.AH_InvoiceDate = postDate;
			apInvoice.AH_DueDate = postDate;
			apInvoice.AH_TransactionNum = transactionNumber;
			var line = apInvoice.Lines[0];
			line.AL_AG = account.PK;
			if (lineDescription != null)
			{
				line.AL_Desc = lineDescription;
			}
			Factory.Save();
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<AccTransactionHeader>() { apInvoice }, (_) => postDate, (_) => "*AP*INV*APCtrl*Total", company: null);
			AccTransactionLines[] transactionLines = { line };
			Creator.CreateComplianceReportQueueEntry(complianceReport, subCode: "*AP*INV*GSTIn*-", overrideDate: postDate, transactionLines);
			Creator.CreateComplianceReportQueueEntry(complianceReport, subCode: "*AP*INV**-", overrideDate: postDate, transactionLines);
			return apInvoice;
		}

		AccTransactionHeader CreateArTransaction(AccComplianceReport complianceReport, ZString transactionNumber, ZDecimal amount, ZDate postDate, OrgHeader org, AccGLHeader account)
		{
			var arInvoice = Creator.CreateARInvoice<ARInvoice>("AR0001", Creator.EUR, 1m, org);
			arInvoice.AH_PostDate = postDate;
			arInvoice.AH_InvoiceDate = postDate;
			arInvoice.AH_DueDate = postDate;
			arInvoice.AH_TransactionNum = transactionNumber;
			arInvoice.AH_AG = account.PK;

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<AccTransactionHeader>() { arInvoice }, (_) => postDate, (_) => "*AR*INV*ARCtrl*Total", company: null);
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<AccTransactionHeader>() { arInvoice }, (_) => postDate, (_) => "*AR*INV*GSTOut*-", company: null);
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<AccTransactionHeader>() { arInvoice }, (_) => postDate, (_) => "*AR*INV**-", company: null);
			return arInvoice;
		}

		ARReceipt CreateARReceipt(AccComplianceReport complianceReport, ZDate postDate)
		{
			var reciept = (ARReceipt)Creator.CreateReceiptOrPayment(ReceiptTypes.DirectCredit, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, Creator.EURBankAccount.PK);
			reciept.AH_PostDate = postDate;
			reciept.AH_InvoiceDate = postDate;
			reciept.AH_DueDate = postDate;
			Factory.Save();
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<AccTransactionHeader>() { reciept }, (_) => postDate, (_) => "*AR*REC*Bank*-", company: null);
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<AccTransactionHeader>() { reciept }, (_) => postDate, (_) => "*AR*REC*ARCtrl*", company: null);
			return reciept;
		}

		string GetStmNoteData(AccComplianceReport complianceReport)
		{
			var sql = string.Format(CultureInfo.InvariantCulture,
				@"SELECT {0} FROM {1} WHERE {2} = {3} AND {4} = '{5}'",
				StmNoteSchema.Constants.ST_NoteData,
				StmNoteSchema.Constants.TableName,
				StmNoteSchema.Constants.ST_ParentID,
				complianceReport.PK.ToSqlGuid(),
				StmNoteSchema.Constants.ST_Description,
				IDEAPersistentData.IncompleteIDEADataDescription);
			var command = Db.Connection.Command(sql);
			var noteData = (byte[])command.ExecuteScalar();
			return noteData == null ? "" : Encoding.UTF8.GetString(noteData);
		}

		ZString GetEmbeddedResourceAsZString(string filename)
		{
			ZString filecontent = ZString.Empty;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.Business.Testing.ComplianceReport.IDEA.Testfiles." + filename))
			{
				using (StreamReader sr = new StreamReader(stream))
				{
					filecontent = sr.ReadToEnd();
				}
			}
			return filecontent;
		}

		ZString SortTextLines(ZString textLines)
		{
			var lines = textLines.Trim(IDEATaxAuditExport.CSVLineTerminator.ToCharArray()).Split(IDEATaxAuditExport.CSVLineTerminator);
			lines = lines.OrderBy(v => v).ToArray();
			return string.Join(IDEATaxAuditExport.CSVLineTerminator, lines) + IDEATaxAuditExport.CSVLineTerminator;
		}

		Dictionary<string, string> GetUnzippedContent(DocManagerInfo eDocs)
		{
			var result = new Dictionary<string, string>();
			if (eDocs.AllEDocs.Count > 0)
			{
				using (var tempDir = new TempDirectory())
				{
					var zipFile = tempDir + ".zip";
					for (var i = 0; i < eDocs.AllEDocs.Count; i++)
					{
						if (eDocs.AllEDocs[i].FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
						{
							File.WriteAllBytes(zipFile, eDocs.AllEDocs[i].ImageData);
							ZArchitecture.Core.ZipCompression.Unzip(zipFile, tempDir);
							File.Delete(zipFile);
							var files = Directory.GetFiles(tempDir);
							foreach (var file in files)
							{
								var fileContent = File.ReadAllText(file);
								result.Add(file.Substring(file.LastIndexOf('\\') + 1), fileContent);
								File.Delete(file);
							}
						}
					}
				}
			}
			return result;
		}

		void AssertFileContent(Dictionary<string, string> files, bool checkContent, string expectedFilename, string expectedContent)
		{
			if (files.TryGetValue(expectedFilename, out var actualContent))
			{
				if (checkContent)
				{
					AssertEquals($"Content of file {expectedFilename}", SortTextLines(expectedContent), SortTextLines(actualContent));
				}
			}
			else
			{
				Fail($"File was not generated: {expectedFilename}");
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<IDEATaxAuditExport>();
		}

		#endregion
	}
}
