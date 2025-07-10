using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceCompany))]
	public class LicenceCompanyTest : SecurityBusinessObjectTestCase
	{
		public void TestCompanyId()
		{
			var licCompany = Factory.New<LicenceCompany>();
			AssertEquals("", licCompany.CompanyId);

			licCompany.LC_CompanyNumber = 1;
			AssertEquals(Base27Encoding.Encode(licCompany.LC_CompanyNumber), licCompany.CompanyId);
		}

		#region Deposits

		public void TestDeposits()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var licCompany = org.LicCompany;
			Factory.Save();

			AssertEquals(0m, licCompany.MonthlyUsageDepositBalance);
			AssertEquals("", licCompany.MonthlyUsageDepositCurrency);
			AssertEquals(true, licCompany.IsMonthlyUsageDepositValid);

			var adjust = licCompany.DepositAdjustments.AddNew();
			adjust.DEA_Amount = 123m;
			adjust.DEA_ChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			adjust.DEA_RX_NKCurrency = "AUD";

			var adjust2 = licCompany.DepositAdjustments.AddNew();
			adjust2.DEA_Amount = 456m;
			adjust2.DEA_ChargeCode = EDIDataRegistry.SecurityDepositChargeCode;
			adjust2.DEA_RX_NKCurrency = "USD";

			Factory.Save();

			AssertEquals(123m, licCompany.MonthlyUsageDepositBalance);
			AssertEquals("AUD", licCompany.MonthlyUsageDepositCurrency);
			AssertEquals(true, licCompany.IsMonthlyUsageDepositValid);
			AssertEquals(456m, licCompany.GetDepositBalanceAmount(EDIDataRegistry.SecurityDepositChargeCode));
			AssertEquals("USD", licCompany.GetDepositBalanceCurrency(EDIDataRegistry.SecurityDepositChargeCode));
		}

		#endregion

		#region Delete

		public void TestDeleteLicenceCompany()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			LicenceCompany licCompany = org.LicCompany;
			licCompany.SelfBilling.L4_Comment = "test";
			licCompany.Fees.AddNew();
			licCompany.InvoiceDeliveries.AddNew();
			licCompany.PriceHeaders.AddNew();
			licCompany.PriceHeaders[0].Items.AddNew();
			var fee1 = licCompany.Fees.AddNew();
			Factory.Save();

			AssertNotNull("Should have a licence company", org.LicCompany);
			Assert("SelfBilling should be saved", licCompany.SelfBilling.IsInDatabase);
			Assert("InvoiceDelivery should be saved", licCompany.InvoiceDeliveries[0].IsInDatabase);
			LicenceDatabase database = org.LicCompany.LicDatabases.AddNew();
			var fee2 = licCompany.Fees.AddNew();
			fee2.L8_LD = database.PK;

			Factory.Save();
			Assert("LicDatabase should be saved", database.IsInDatabase);

			org.LicCompany.Delete();
			Factory.Save();
			AssertEquals("LicCompany should NOT be linked to Company", 0, licCompany.LicDatabases.Count);
			AssertEquals("LicCompany SelfBilling deleted", true, licCompany.SelfBilling.IsDeleted);
			AssertEquals("LicCompany InvoiceDeliveries deleted", 0, licCompany.InvoiceDeliveries.Count);
			AssertEquals("LicCompany PrioceHeaders deleted", 0, licCompany.PriceHeaders.Count);
			AssertEquals("LicCompany only fee deleted", true, fee1.IsDeleted);
			AssertEquals("LicCompany fee with DB not deleted", false, fee2.IsDeleted);
			AssertEquals("fee company empty", ZGuid.Empty, fee2.L8_LC);
		}

		#endregion

		#region Logging

		public void TestBusinessObjectsWithRelatedEvents()
		{
			LicenceCompany company = Factory.New<LicenceCompany>();
			AssertEquals("Should be 0 business objects with related logs", 0, company.BusinessObjectsWithRelatedEvents.Length);

			var db = company.LicDatabases.AddNew();
			var licHeader = company.GetHeader(db);
			AssertCollectionContains("Business objects with related logs should contain", db, company.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("Business objects with related logs should contain", licHeader, company.BusinessObjectsWithRelatedEvents);
		}

		[TestDate(2010, 10, 1)]
		public void TestGetCustomLogReference()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceCompany company = testHeader.LicCompany;
			company.LC_CompanyCode = "///";
			company.LC_RX_NKCurrency = "AUD";
			TestDateAttribute.Date = new DateTime(2010, 10, 1, 0, 0, 1);
			Factory.Save();

			AssertEquals("Logs count", 1, company.Logs.GetAllLogs().Count);
			AssertEquals("Log should have reference", "Company ///", company.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);

			company.LC_CompanyCode = "{{{";
			TestDateAttribute.Date = new DateTime(2010, 10, 1, 0, 0, 2);
			Factory.Save();
			AssertEquals("Logs count", 2, company.Logs.GetAllLogs().Count);
			AssertEquals("Log should have reference", "Company {{{(///)", company.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			company.LC_RX_NKCurrency = "USD";
			TestDateAttribute.Date = new DateTime(2010, 10, 1, 0, 0, 3);
			Factory.Save();
			AssertEquals("Logs count", 3, company.Logs.GetAllLogs().Count);
			AssertEquals("Log should have reference", "Company {{{ - Currency: USD(AUD)", company.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			company.LC_RX_NKCurrency = "AUD";
			TestDateAttribute.Date = new DateTime(2010, 10, 1, 0, 0, 4);
			Factory.Save();
			AssertEquals("Logs count", 4, company.Logs.GetAllLogs().Count);
			AssertEquals("Log should have reference", "Company {{{ - Currency: AUD(USD)", company.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestLogChanges()
		{
			EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = true;
			EDIOrgHeader org = HeaderForTest;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LC_IsReciprocal = false;
			org.LicCompany.LC_IsGSTRegistered = true;
			org.LicCompany.LC_IsGSTCashBasis = false;
			org.LicCompany.LC_IsWHTRegistered = true;
			org.LicCompany.LC_IsWHTCashBasis = false;
			Factory.Save();

			org.LicCompany.LC_IsReciprocal = true;
			Factory.Save();
			AssertEquals(true, org.LicCompany.Logs.HasLogWith(StmALogSchema.SL_Reference, "Exchange Reciprocal Rate Status from No to Yes"));

			org.LicCompany.LC_IsGSTRegistered = false;
			org.LicCompany.LC_IsGSTCashBasis = true;
			Factory.Save();
			AssertEquals(true, org.LicCompany.Logs.HasLogWith(StmALogSchema.SL_Reference, "GST Registered Status from Yes to No"));
			AssertEquals(true, org.LicCompany.Logs.HasLogWith(StmALogSchema.SL_Reference, "GST Cash Basis Status from No to Yes"));

			org.LicCompany.LC_IsWHTRegistered = false;
			org.LicCompany.LC_IsWHTCashBasis = true;
			Factory.Save();
			AssertEquals(true, org.LicCompany.Logs.HasLogWith(StmALogSchema.SL_Reference, "WHT Registered Status from Yes to No"));
			AssertEquals(true, org.LicCompany.Logs.HasLogWith(StmALogSchema.SL_Reference, "WHT Cash Basis Status from No to Yes"));
		}

		#endregion

		#region Properties

		public void TestIsReciprocal()
		{
			EDIOrgHeader org = HeaderForTest;
			Factory.Save();
			AssertEquals("Test No Errors", 0, org.NotificationsIncludingChildren.GetErrors().Count());

			org.CreateAndLoadLicenceForOrg();
			Assert("IsReciprocal should be false", !org.LicCompany.LC_IsReciprocal);

			org.OH_RL_NKClosestPort = "THBKK";
			org.CreateAndLoadLicenceForOrg();
			Assert("IsReciprocal should be true", org.LicCompany.LC_IsReciprocal);
		}

		public void TestIsGSTCashBasis()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "CMBEL";
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("GST should be Cash Basis", testHeader.LicCompany.LC_IsGSTCashBasis);

			testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "CIABJ";
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("GST should be Cash Basis", testHeader.LicCompany.LC_IsGSTCashBasis);

			testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "MZCHO";
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("GST should not be Cash Basis", !testHeader.LicCompany.LC_IsGSTCashBasis);

			testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "SNCSK";
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("GST should not be Cash Basis", !testHeader.LicCompany.LC_IsGSTCashBasis);
		}

		public void TestIsGSTRegistered()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("GST Registered for AUBNE", testHeader.LicCompany.LC_IsGSTRegistered);

			testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "HKHKG";
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("GST should NOT be Registered", !testHeader.LicCompany.LC_IsGSTRegistered);
		}

		public void TestGSTRegisteredDifferentToDefault()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("GST Not Different", !testHeader.LicCompany.GSTRegisteredDifferentToDefault);

			testHeader.LicCompany.LC_IsGSTRegistered = !testHeader.LicCompany.LC_IsGSTRegistered;
			Assert("GST Is Different", testHeader.LicCompany.GSTRegisteredDifferentToDefault);

			testHeader.LicCompany.LC_IsGSTRegistered = !testHeader.LicCompany.LC_IsGSTRegistered;
			Assert("GST Is No longer Different", !testHeader.LicCompany.GSTRegisteredDifferentToDefault);
		}

		public void TestExchangeRateText()
		{
			EDIOrgHeader header = HeaderForTest;
			header.CreateAndLoadLicenceForOrg();
			header.LicCompany.LC_IsReciprocal = true;
			AssertEquals("1 Unit of Foreign is equal to X Unit(s) of Local", header.LicCompany.ExchangeRateText);

			header.LicCompany.LC_IsReciprocal = false;
			AssertEquals("1 Unit of Local is equal to X Unit(s) of Foreign", header.LicCompany.ExchangeRateText);
		}

		public void TestPricelistRegion()
		{
			ZArchitecture.Core.CodeDescriptionPairList list = new ZArchitecture.Core.CodeDescriptionPairList();
			list.AddPair("AU", "OC1");
			list.AddPair("US", "USC");
			EDIDataRegistry.Instance.PricelistCountryRegions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			EDIOrgHeader header = HeaderForTest;
			header.CreateAndLoadLicenceForOrg();

			header.LicCompany.LC_CompanyCountry = "AU";
			AssertEquals("OC1", header.LicCompany.PricelistRegion);

			header.LicCompany.LC_CompanyCountry = "US";
			AssertEquals("USC", header.LicCompany.PricelistRegion);

			header.LicCompany.LC_CompanyCountry = "SH";
			Assert(header.LicCompany.PricelistRegion.IsEmpty);

			header.LicCompany.LC_CompanyCountry = "";
			Assert(header.LicCompany.PricelistRegion.IsEmpty);
		}

		#endregion

		#region IsWHTRegistered

		public void TestIsWHTRegistered()
		{
			EDIOrgHeader org = HeaderForTest;
			org.CreateAndLoadLicenceForOrg();

			Assert("WHT NOT Registered", !org.LicCompany.LC_IsWHTRegistered);
			org.MiscServ.OM_ARWHTApplicable = true;
			Assert("WHT Registered", org.LicCompany.LC_IsWHTRegistered);
		}

		public void TestIsWHTRegisteredCopiesValuesToCompanyData()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("WHT Not Registered", !testHeader.CompanyData.OB_ARWHTApplicable);

			testHeader.LicCompany.LC_IsWHTRegistered = ZBool.True;
			Assert("WHT Registered", testHeader.CompanyData.OB_ARWHTApplicable);
		}

		#endregion

		#region Related Business Objects

		public void TestEnterpriseLicence()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Assert("Enterprise Licence Accessible", testHeader.LicCompany.LicEnterprise.Equals(testHeader.LicEnterprise));
		}

		public void TestLicDatabases()
		{
			LicenceCompany comp = Factory.New<LicenceCompany>();
			AssertEquals("Collection Is Empty", 0, comp.LicDatabases.Count);
			LicenceDatabase dB1 = comp.LicDatabases.AddNew();
			LicenceDatabase dB2 = comp.LicDatabases.AddNew();
			AssertEquals("Collection Contains 2 elements", 2, comp.LicDatabases.Count);
		}

		public void TestActiveOrAllLicDatabases()
		{
			LicenceCompany comp = Factory.New<LicenceCompany>();
			AssertEquals("Collection Is Empty", 0, comp.ActiveOrAllLicDatabases.Count);

			LicenceDatabase dB1 = comp.LicDatabases.AddNew();
			LicenceDatabase dB2 = comp.LicDatabases.AddNew();
			dB1.LD_IsActive = true;
			dB2.LD_IsActive = false;

			comp.IncludeInactiveDatabases = false;
			AssertEquals("Collection Contains 1 element", 1, comp.ActiveOrAllLicDatabases.Count);

			comp.IncludeInactiveDatabases = true;
			AssertEquals("Collection Contains 2 elements", 2, comp.ActiveOrAllLicDatabases.Count);
		}

		public void TestLicHeaders()
		{
			LicenceCompany comp = Factory.New<LicenceCompany>();
			AssertEquals("Collection Is Empty", 0, comp.LicHeadersForAllDatabases.Count);
			LicenceDatabase dB1 = comp.LicDatabases.AddNew();
			LicenceDatabase dB2 = comp.LicDatabases.AddNew();
			AssertEquals("Collection Contains 2 elements", 2, comp.LicHeadersForAllDatabases.Count);
		}

		#region Billing and Prices

		public void TestPriceHeaderForDate()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceCompany licCompany = testHeader.LicCompany;

			ClientLicencePriceHeader oldPriceList = licCompany.PriceHeaders.AddNew();
			oldPriceList.L6_ValidFrom = new ZDateTime(2007, 1, 1);

			ClientLicencePriceHeader janPriceList = licCompany.PriceHeaders.AddNew();
			janPriceList.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			janPriceList.L6_ValidTo = new ZDateTime(2010, 1, 31);

			ClientLicencePriceHeader feb2PriceList = licCompany.PriceHeaders.AddNew();
			feb2PriceList.L6_ValidFrom = new ZDateTime(2010, 2, 2);
			feb2PriceList.L6_ValidTo = new ZDateTime(2010, 2, 28);

			ClientLicencePriceHeader marToAprPriceList = licCompany.PriceHeaders.AddNew();
			marToAprPriceList.L6_ValidFrom = new ZDateTime(2010, 3, 1);
			marToAprPriceList.L6_ValidTo = new ZDateTime(2010, 4, 30);

			ClientLicencePriceHeader mayPriceList = licCompany.PriceHeaders.AddNew();
			mayPriceList.L6_ValidFrom = new ZDateTime(2010, 5, 1);

			AssertEquals(janPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 1, 1)).PK);
			AssertEquals(janPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 1, 15)).PK);
			AssertEquals(janPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 1, 31)).PK);

			AssertNull(licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 2, 1)));
			AssertEquals(feb2PriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 2, 2)).PK);
			AssertEquals(feb2PriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 2, 15)).PK);
			AssertEquals(feb2PriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 2, 28)).PK);

			AssertEquals(marToAprPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 3, 1)).PK);
			AssertEquals(marToAprPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 3, 15)).PK);
			AssertEquals(marToAprPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 4, 30)).PK);

			AssertEquals(mayPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 5, 1)).PK);
			AssertEquals(mayPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2011, 1, 1)).PK);

			mayPriceList.L6_ValidTo = new ZDateTime(2010, 6, 1);
			AssertEquals(mayPriceList.PK, licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 6, 1)).PK);
			AssertNull(licCompany.OnDemandPriceHeaderForDate(new DateTime(2010, 6, 2)));
		}

		public void TestPriceHeaders()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceCompany licCompany = testHeader.LicCompany;

			var priceList1 = licCompany.PriceHeaders.AddNew();
			var priceList2 = licCompany.PriceHeaders.AddNew();
			var priceList3 = licCompany.PriceHeaders.AddNew();
			priceList1.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceList2.L6_ValidFrom = new ZDateTime(2010, 3, 1);
			priceList3.L6_ValidFrom = new ZDateTime(2010, 2, 1);
			AssertEquals("yes security rights", false, licCompany.PriceHeaders.ReadOnly);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var licCompanyReloaded = factory2.Load<LicenceCompany>(licCompany.PK);
			AssertEquals("sorted by ValidFrom desc", new ZDateTime(2010, 3, 1), licCompany.PriceHeaders[0].L6_ValidFrom);
			AssertEquals("sorted by ValidFrom desc", new ZDateTime(2010, 2, 1), licCompany.PriceHeaders[1].L6_ValidFrom);
			AssertEquals("sorted by ValidFrom desc", new ZDateTime(2010, 1, 1), licCompany.PriceHeaders[2].L6_ValidFrom);

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.OH_Code = "TGBLOG";
			testHeader.OH_RL_NKClosestPort = "AUBNE";
			testHeader.CreateAndLoadLicenceForOrg();
			licCompany = testHeader.LicCompany;
			AssertEquals("no security rights", true, licCompany.PriceHeaders.ReadOnly);
		}

		public void TestCopyPriceList()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();

			// Populate standard pricelist
			LicenceHeader stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			LicenceCompany stdLicCompany = stdHeader.Company;
			ClientLicencePriceHeader stdPriceListODM = stdLicCompany.PriceHeaders.AddNew();
			stdPriceListODM.L6_PricelistVersion = "V19";
			stdPriceListODM.L6_IsStandard = true;
			stdPriceListODM.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			stdPriceListODM.L6_ValidFrom = new ZDateTime(2010, 11, 1);
			stdPriceListODM.L6_ValidTo = new ZDateTime(2010, 11, 30);
			ClientLicencePriceItem stdPriceItemODM1 = stdPriceListODM.Items.AddNew();
			stdPriceItemODM1.L7_Code = "SD1";
			ClientLicencePriceItem stdPriceItemODM2 = stdPriceListODM.Items.AddNew();
			stdPriceItemODM2.L7_Code = "SD2";
			ClientLicencePriceItem stdPriceItemODM3 = stdPriceListODM.Items.AddNew();
			stdPriceItemODM3.L7_Code = "SD3";
			ClientLicencePriceHeader stdPriceListOTL = stdLicCompany.PriceHeaders.AddNew();
			stdPriceListOTL.L6_PricelistVersion = "V103";
			stdPriceListOTL.L6_IsStandard = true;
			stdPriceListOTL.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			stdPriceListOTL.L6_ValidFrom = new ZDateTime(2010, 11, 1);
			stdPriceListOTL.L6_ValidTo = new ZDateTime(2010, 11, 30);
			ClientLicencePriceItem stdPriceItemOTL1 = stdPriceListOTL.Items.AddNew();
			stdPriceItemOTL1.L7_Code = "ST1";
			ClientLicencePriceItem stdPriceItemOTL2 = stdPriceListOTL.Items.AddNew();
			stdPriceItemOTL2.L7_Code = "ST2";
			ClientLicencePriceItem stdPriceItemOTL3 = stdPriceListOTL.Items.AddNew();
			stdPriceItemOTL3.L7_Code = "ST3";
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			// Populate local pricelist
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceCompany licCompany = testHeader.LicCompany;
			ClientLicencePriceHeader localPriceList1 = licCompany.PriceHeaders.AddNew();
			localPriceList1.L6_IsStandard = false;
			localPriceList1.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			localPriceList1.L6_ValidFrom = new ZDateTime(2010, 11, 1);
			localPriceList1.L6_ValidTo = new ZDateTime(2010, 11, 30);
			ClientLicencePriceItem localPrice1Item1 = localPriceList1.Items.AddNew();
			localPrice1Item1.L7_Code = "L11";
			ClientLicencePriceItem localPrice1Item2 = localPriceList1.Items.AddNew();
			localPrice1Item2.L7_Code = "L12";

			ClientLicencePriceHeader localPriceList2 = licCompany.PriceHeaders.AddNew();
			localPriceList2.L6_IsStandard = false;
			localPriceList2.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			localPriceList2.L6_ValidFrom = new ZDateTime(2010, 12, 1);
			localPriceList2.L6_ValidTo = new ZDateTime(2010, 12, 30);
			ClientLicencePriceItem localPrice2Item1 = localPriceList2.Items.AddNew();
			localPrice2Item1.L7_Code = "L21";
			ClientLicencePriceItem localPrice2Item2 = localPriceList2.Items.AddNew();
			localPrice2Item2.L7_Code = "L22";
			var localPrice2Item2Rate = localPrice2Item2.CurrencyRates.AddNew();

			// Validate Standard Pricelist
			ClientLicencePriceHeader testPriceList = licCompany.PriceHeaders.AddNew();
			testPriceList.L6_IsStandard = true;
			testPriceList.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			testPriceList.L6_ValidFrom = new ZDateTime(2010, 11, 1);
			testPriceList.L6_ValidTo = new ZDateTime(2010, 11, 30);
			testPriceList.L6_PricelistVersion = "abc"; // invalid price list version
			AssertNull(licCompany.CopyPriceList(testPriceList));
			testPriceList.L6_PricelistVersion = "V19";
			ClientLicencePriceHeader destLicPriceHeader = licCompany.CopyPriceList(testPriceList);
			AssertEquals(stdPriceListODM.L6_SystemCode, destLicPriceHeader.L6_SystemCode);
			AssertEquals(stdPriceListODM.L6_ValidFrom, destLicPriceHeader.L6_ValidFrom);
			AssertEquals(stdPriceListODM.L6_PricelistVersion, destLicPriceHeader.L6_PricelistVersion);
			AssertEquals(3, destLicPriceHeader.Items.Count);
			Assert(destLicPriceHeader.Items.Any(p => p.L7_Code == stdPriceItemODM1.L7_Code));
			Assert(destLicPriceHeader.Items.Any(p => p.L7_Code == stdPriceItemODM2.L7_Code));
			Assert(destLicPriceHeader.Items.Any(p => p.L7_Code == stdPriceItemODM3.L7_Code));

			// Validate Local Pricelist
			destLicPriceHeader = licCompany.CopyPriceList(localPriceList1);
			AssertNotEquals(localPriceList1.PK, destLicPriceHeader.PK);
			AssertEquals(localPriceList1.L6_SystemCode, destLicPriceHeader.L6_SystemCode);
			AssertEquals(localPriceList1.L6_ValidFrom, destLicPriceHeader.L6_ValidFrom);
			AssertEquals(localPriceList1.L6_PricelistVersion, destLicPriceHeader.L6_PricelistVersion);
			AssertEquals(2, destLicPriceHeader.Items.Count);
			Assert(destLicPriceHeader.Items.Any(p => p.L7_Code == localPrice1Item1.L7_Code));
			Assert(destLicPriceHeader.Items.Any(p => p.L7_Code == localPrice1Item2.L7_Code));

			destLicPriceHeader = licCompany.CopyPriceList(localPriceList2);
			AssertNotEquals(destLicPriceHeader.PK, localPriceList2.PK);
			AssertEquals(localPriceList2.L6_SystemCode, destLicPriceHeader.L6_SystemCode);
			AssertEquals(localPriceList2.L6_ValidFrom, destLicPriceHeader.L6_ValidFrom);
			AssertEquals(localPriceList2.L6_PricelistVersion, destLicPriceHeader.L6_PricelistVersion);
			AssertEquals(2, destLicPriceHeader.Items.Count);
			Assert(destLicPriceHeader.Items.Any(p => p.L7_Code == localPrice2Item1.L7_Code));
			Assert(destLicPriceHeader.Items.Any(p => p.L7_Code == localPrice2Item2.L7_Code));
			AssertEquals(1, destLicPriceHeader.Items.First(p => p.L7_Code == localPrice2Item2.L7_Code).CurrencyRates.Count);
		}
		#endregion

		#endregion

		#region Default Values

		public void TestSettingDefaults()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();

			AssertEquals("Check Currency is set", GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency, testHeader.LicCompany.LC_RX_NKCurrency);
			testHeader.CompanyData.SetAPTaxApplicable(true);
			testHeader.MiscServ.OM_ARWHTApplicable = true;
			Factory.Save();
			Assert("Should be GST Registered", testHeader.LicCompany.LC_IsGSTRegistered);
			Assert("Should NOT be reciprocal", !testHeader.LicCompany.LC_IsReciprocal);
		}

		#endregion

		#region Read Only Security

		public void Test3rdPartyCollectionIsReadonlyWhenNoSecurityAllowed()
		{
			bool oldLicence3rdPartySoftwareValue = EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed;
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			try
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = true;
				Assert(!testHeader.LicCompany.Licence3rdPartySoftware.ReadOnly);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = oldLicence3rdPartySoftwareValue;
			}
		}

		public void Test3rdPartyCollectionIsReadonlyWhenSecurityDenied()
		{
			bool oldLicence3rdPartySoftwareValue = EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed;
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			try
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = false;
				Assert(testHeader.LicCompany.Licence3rdPartySoftware.ReadOnly);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify3rdPartySoftware.IsAllowed = oldLicence3rdPartySoftwareValue;
			}
		}

		public void TestReadOnlySecurityOfOrgLicenceModifyCheckpoint()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;

			EDIOrgHeader org = HeaderForTest;
			org.CreateAndLoadLicenceForOrg();
			try
			{
				string[] propertyNamesToExcept = new string[]
					{
						LicenceCompanySchema.LC_IsWHTRegistered.Name,
						LicenceCompanySchema.LC_IsGSTRegistered.Name,
						LicenceCompanySchema.LC_IsGSTCashBasis.Name,
						LicenceCompanySchema.LC_IsWHTCashBasis.Name,
						LicenceCompanySchema.LC_IsReciprocal.Name
					};
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
				AssertPropertyInfosReadOnly(org.LicCompany, false, propertyNamesToExcept);

				propertyNamesToExcept = Array.Empty<string>();
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = false;
				AssertPropertyInfosReadOnly(org.LicCompany, true, propertyNamesToExcept);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue;
			}
		}

		public void TestReadOnlySecurityOfOrgLicenceModifyExchangeRatesAndTaxCheckpoint()
		{
			EDIOrgHeader org = HeaderForTest;
			org.CreateAndLoadLicenceForOrg();
			EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = true;
			AssertEquals("IsWHTRegistered should be editable", false, org.LicCompany.LC_IsWHTRegisteredInfo.ReadOnly);
			AssertEquals("IsGSTRegistered should be editable", false, org.LicCompany.LC_IsGSTRegisteredInfo.ReadOnly);
			AssertEquals("IsGSTCashBasis should be editable", false, org.LicCompany.LC_IsGSTCashBasisInfo.ReadOnly);
			AssertEquals("IsWHTCashBasis should be editable", false, org.LicCompany.LC_IsWHTCashBasisInfo.ReadOnly);
			AssertEquals("IsReciprocal should be editable", false, org.LicCompany.LC_IsReciprocalInfo.ReadOnly);

			EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = false;
			AssertEquals("IsWHTRegistered should be readonly", true, org.LicCompany.LC_IsWHTRegisteredInfo.ReadOnly);
			AssertEquals("IsGSTRegistered should be readonly", true, org.LicCompany.LC_IsGSTRegisteredInfo.ReadOnly);
			AssertEquals("IsGSTCashBasis should be readonly", true, org.LicCompany.LC_IsGSTCashBasisInfo.ReadOnly);
			AssertEquals("IsWHTCashBasis should be readonly", true, org.LicCompany.LC_IsWHTCashBasisInfo.ReadOnly);
			AssertEquals("IsReciprocal should be readonly", true, org.LicCompany.LC_IsReciprocalInfo.ReadOnly);
		}

		public void TestInactiveDatabaseCount()
		{
			LicenceCompany company = Factory.New<LicenceCompany>();
			AssertEquals("Collection Is Empty", 0, company.ActiveOrAllLicDatabases.Count);
			AssertEquals(0, company.InactiveDatabaseCount);

			LicenceDatabase db1 = company.LicDatabases.AddNew();
			LicenceDatabase db2 = company.LicDatabases.AddNew();
			db1.LD_IsActive = true;
			db2.LD_IsActive = false;
			AssertEquals(1, company.InactiveDatabaseCount);

			db2.LD_IsActive = true;
			AssertEquals(0, company.InactiveDatabaseCount);

			db1.LD_IsActive = false;
			db2.LD_IsActive = false;
			AssertEquals(2, company.InactiveDatabaseCount);

			db1.LD_IsActive = true;
			AssertEquals(1, company.InactiveDatabaseCount);
			var licHeader1 = company.GetHeader(db1);
			licHeader1.LA_IsActive = false;
			AssertEquals(2, company.InactiveDatabaseCount);
		}

		public void TestIsActive()
		{
			LicenceCompany company = Factory.New<LicenceCompany>();
			LicenceDatabase db1 = company.LicDatabases.AddNew();
			AssertEquals(true, company.IsActive(db1));
			db1.LD_IsActive = false;
			AssertEquals(false, company.IsActive(db1));
			db1.LD_IsActive = true;
			AssertEquals(true, company.IsActive(db1));
			var licHeader1 = company.GetHeader(db1);
			licHeader1.LA_IsActive = false;
			AssertEquals(false, company.IsActive(db1));
		}

		public void TestGetHeader()
		{
			LicenceCompany company = Factory.New<LicenceCompany>();
			LicenceDatabase db1 = company.LicDatabases.AddNew();
			LicenceDatabase db2 = company.LicDatabases.AddNew();
			AssertEquals(db1.PK, company.GetHeader(db1).LA_LD);
			AssertEquals(db2.PK, company.GetHeader(db2).LA_LD);
		}

		#endregion

		#region Quick Transactional Price List

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		[TestDate(2010, 11, 9)]
		public void TestQuickAddTransactionalPriceList()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			org.LicCompany.LC_CompanyCountry = "AU";
			org.LicCompany.LC_RX_NKCurrency = "AUD";
			LicenceDatabase database = org.LicCompany.LicDatabases.AddNew();
			database.LD_LicenceType = "PRD";
			Factory.Save();

			AssertEquals(0, org.LicCompany.PriceHeaders.Count);

			org.LicCompany.QuickAddTransactionalPriceList();
			AssertEquals(1, org.LicCompany.PriceHeaders.Count);
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders[0];
			AssertEquals("AU", priceHeader.L6_RN_NKCountry);
			AssertEquals("", priceHeader.L6_RX_NKCurrency);
			AssertEquals("Not setting the start date - should be done manually", ZDateTime.Empty, priceHeader.L6_ValidFrom);
			AssertEquals(7, priceHeader.Items.Count);
			AssertHasTransactionalPriceItem(priceHeader, BillingConstants.BillingSystem.ImporterSecurityFiling);
			AssertHasTransactionalPriceItem(priceHeader, BillingConstants.BillingSystem.eBACCA);
			AssertHasTransactionalPriceItem(priceHeader, BillingConstants.BillingSystem.DeniedPartyScreening);
			AssertHasTransactionalPriceItem(priceHeader, BillingConstants.BillingSystem.ExDocs);
			AssertHasTransactionalPriceItem(priceHeader, BillingConstants.BillingSystem.DistanceCalculatorGeneric);
			AssertHasTransactionalPriceItem(priceHeader, BillingConstants.BillingSystem.DistanceCalculatorPcMiler);
			AssertHasTransactionalPriceItem(priceHeader, BillingConstants.BillingSystem.S8Cargo);
			var exDocs = priceHeader.Items.FindByCode(BillingConstants.BillingSystem.ExDocs);
			AssertEquals("description from billing system for non-module code", BillingConstants.BillingSystemList.GetDescriptionFromCode(BillingConstants.BillingSystem.ExDocs), exDocs.L7_Description);

			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			Factory.Save();

			org.LicCompany.QuickAddTransactionalPriceList();
			AssertEquals("Should not create price header again if exist", 1, org.LicCompany.PriceHeaders.Count);
		}

		[TestDate(2010, 11, 9)]
		public void TestQuickAddTransactionalPriceList_NoValidPriceList()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			org.LicCompany.LC_CompanyCountry = "AU";
			org.LicCompany.LC_RX_NKCurrency = "AUD";
			LicenceDatabase database = org.LicCompany.LicDatabases.AddNew();
			database.LD_LicenceType = "PRD";
			ClientLicencePriceHeader obsoletedPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			obsoletedPriceHeader.L6_LicenceEdition = "COU";
			obsoletedPriceHeader.L6_RN_NKCountry = "AU";
			obsoletedPriceHeader.L6_RX_NKCurrency = "AUD";
			obsoletedPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			obsoletedPriceHeader.L6_ValidTo = new ZDateTime(2010, 9, 1);
			Factory.Save();

			AssertEquals(1, org.LicCompany.PriceHeaders.Count);

			org.LicCompany.QuickAddTransactionalPriceList();
			AssertEquals("Should create when no current valid price header", 2, org.LicCompany.PriceHeaders.Count);
		}

		void AssertHasTransactionalPriceItem(ClientLicencePriceHeader priceHeader, ZString code)
		{
			AssertEquals(true, priceHeader.Items.Any(x => x.L7_Code == code && x.L7_FeeType == BillingConstants.FeeType.Transactional));
		}

		#endregion

		public void TestCreatePriceList()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			LicenceCompany licCompany = org.LicCompany;
			var db = licCompany.LicDatabases.AddNew();
			var licHeader = licCompany.GetHeader(db);
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentCountry;

			var prices = licCompany.CreatePriceList(false);
			AssertEquals(1, licCompany.PriceHeaders.Count);
			AssertEquals(false, prices.L6_IsStandard);
			AssertEquals(licCompany.LC_CompanyCountry, prices.L6_RN_NKCountry);
			AssertEquals("", prices.L6_RX_NKCurrency);
			AssertEquals(BillingConstants.BillingSystem.Maintenance, prices.L6_SystemCode);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			prices = licCompany.CreatePriceList(true);
			AssertEquals(2, licCompany.PriceHeaders.Count);
			AssertEquals(true, prices.L6_IsStandard);
			AssertEquals(licCompany.LC_CompanyCountry, prices.L6_RN_NKCountry);
			AssertEquals("", prices.L6_RX_NKCurrency);
			AssertEquals(BillingConstants.BillingSystem.ODM, prices.L6_SystemCode);
		}

		public void TestSelfBilling()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();

			EDIOrgHeader header = Factory.NewWithValidTestData<EDIOrgHeader>();
			header.CompanyData.OB_GB_ControllingBranch = branch.PK;
			header.CompanyData.OB_RX_NKARDDefltCurrency = Constants.CurrencyCodes.Moldova;
			header.CompanyData.OB_IsDebtor = false;
			header.CreateAndLoadLicenceForOrg();
			ClientLicenceBilling billing = header.LicCompany.SelfBilling;

			AssertNotNull(billing);
		}

		public void TestReadonlySelfBilling()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();

			EDIOrgHeader header = Factory.NewWithValidTestData<EDIOrgHeader>();
			header.CompanyData.OB_GB_ControllingBranch = branch.PK;
			header.CompanyData.OB_RX_NKARDDefltCurrency = Constants.CurrencyCodes.Moldova;
			header.CompanyData.OB_IsDebtor = false;
			header.CreateAndLoadLicenceForOrg();
			ClientLicenceBilling billing = header.LicCompany.ReadonlySelfBilling;
			AssertNull(billing);

			billing = header.LicCompany.SelfBilling;
			billing = header.LicCompany.ReadonlySelfBilling;
			AssertNotNull(billing);

			billing.Delete();
			billing = header.LicCompany.ReadonlySelfBilling;
			AssertNull(billing);
		}

		public void TestLoad()
		{
			EDIOrgHeader organisation1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader organisation2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			organisation1.CreateAndLoadLicenceForOrg();
			organisation2.CreateAndLoadLicenceForOrg();

			organisation1.LicenceEnterpriseCode = "LE1";
			organisation2.LicenceEnterpriseCode = "LE2";

			organisation1.LicCompany.LC_CompanyCode = "LC1";
			organisation2.LicCompany.LC_CompanyCode = "LC2";

			LicenceDatabase database1 = organisation1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database2 = organisation2.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database1b = organisation1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database1c = organisation1.LicCompany.LicDatabases.AddNew();

			database1.LD_ServerCode = "LD1";
			database2.LD_ServerCode = "LD2";
			database1b.LD_ServerCode = "LDX";
			database1c.LD_ServerCode = "LDY";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals("Should have loaded.", organisation1.LicCompany.PK, LicenceCompany.Load(newFactory, "LE1", "LC1", "LD1").PK);
			AssertEquals("Should have loaded.", organisation2.LicCompany.PK, LicenceCompany.Load(newFactory, "LE2", "LC2", "LD2").PK);
			AssertNull("Should not have loaded anything.", LicenceCompany.Load(newFactory, "LE3", "LC1", "LD1"));
			AssertNull("Should not have loaded anything.", LicenceCompany.Load(newFactory, "LE1", "LC3", "LD1"));
			AssertNull("Should not have loaded anything.", LicenceCompany.Load(newFactory, "LE1", "LC1", "LD3"));
		}

		public void TestOnSaving_CompanyNumber()
		{
			var licCompany1 = BillingTestHelper.CreateLicence(Factory, "AAA").Company;
			Factory.Save();

			var licCompany2 = BillingTestHelper.CreateLicence(Factory, "BBB").Company;
			Factory.Save();

			AssertEquals(1000, licCompany1.LC_CompanyNumber);
			AssertEquals(1001, licCompany2.LC_CompanyNumber);
		}

		public void TestCopyPersistentValuesFrom()
		{
			var company1 = Factory.New<LicenceCompany>();
			var company2 = Factory.New<LicenceCompany>();
			company1.LC_CompanyNumber = 200;
			company2.LC_CompanyNumber = 300;
			company2.CopyPersistentValuesFrom(company1);
			AssertEquals(300, company2.LC_CompanyNumber);
		}

		public void TestFeeCollectionReadOnly()
		{
			var company = Factory.New<LicenceCompany>();
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			AssertEquals(false, company.Fees.ReadOnly);

			company = Factory.New<LicenceCompany>();
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			AssertEquals(true, company.Fees.ReadOnly);
		}

		public void TestDepositBalancesHasChanges()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();

			var deposit1 = org.LicCompany.DepositBalances.OfType<DepositBalance>().First();
			var adjustment1 = deposit1.Adjustments.AddNew();
			adjustment1.DEA_Amount = 12.34;
			adjustment1.DEA_RX_NKCurrency = "AUD";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var org2 = newFactory.Load<EDIOrgHeader>(org.PK);
			var deposit2 = org2.LicCompany.DepositBalances.OfType<DepositBalance>().First(x => x.ChargeCode == deposit1.ChargeCode);
			var adjustment2 = deposit2.Adjustments[0];
			AssertEquals(adjustment1.PK, adjustment2.PK);
			AssertEquals(12.34m, adjustment2.DEA_Amount);
			AssertEquals(false, org2.LicCompany.DepositBalances.HasChanges);
			AssertEquals(false, org2.LicCompany.HasChanges);
			AssertEquals(false, org2.HasChanges);

			var newAdjustment = deposit2.Adjustments.AddNew();
			AssertEquals(true, org2.LicCompany.DepositBalances.HasChanges);
			AssertEquals(true, org2.LicCompany.HasChanges);
			AssertEquals(true, org2.HasChanges);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			OriginalValueOfOrgLicenceModifyExchangeRatesAndTaxCheckpoint = EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed;
			EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = false;
		}

		bool OriginalValueOfOrgLicenceModifyExchangeRatesAndTaxCheckpoint;

		protected override void TearDown()
		{
			base.TearDown();
			EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = OriginalValueOfOrgLicenceModifyExchangeRatesAndTaxCheckpoint;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			LicenceCompany result = Factory.New<LicenceCompany>();
			result.LC_OH = Factory.New<OrgHeader>().PK;
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIOrgHeader testHeader = factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "Address";
			testHeader.CreateAndLoadLicenceForOrg();
			return testHeader.LicCompany;
		}

		public EDIOrgHeader HeaderForTest
		{
			get
			{
				EDIOrgHeader fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
				fHeaderForTest.OH_Code = "TGBLOG";
				fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";
				fHeaderForTest.MainAddress.OA_Phone = "+61426829924";

				return fHeaderForTest;
			}
		}

		#endregion
	}
}
