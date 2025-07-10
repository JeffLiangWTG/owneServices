using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	sealed class DatabaseUsageSetTest : TestCaseWithFactory
	{
		// Disables critical validation "Missing Reversing Transaction for this canceled transaction".
		[SuspendCriticalValidation]
		[TestDate(2015, 12, 1)]
		public void TestGetUsages()
		{
			var periodStart = new ZDate(TestDateAttribute.Date);
			var lastPeriod = periodStart.AddMonths(-1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_AgreedLiveDate = periodStart.AddYears(-1);
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, "#NP", "GFR");
			priceHeader.L6_TestDbPriceCode = "#NP";
			priceHeader.Items[1].L7_FeeType = BillingConstants.FeeType.Database;

			var priceLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink1.PHL_L6 = priceHeader.PK;
			priceLink1.PHL_ValidFrom = periodStart.AddYears(-1);
			priceLink1.PHL_RX_NKCurrency = "AUD";

			var priceLink2 = lic2.Database.PriceHeaderLinks.AddNew();
			priceLink2.PHL_L6 = priceHeader.PK;
			priceLink2.PHL_ValidFrom = periodStart.AddYears(-1);
			priceLink2.PHL_RX_NKCurrency = "AUD";

			// Save invoice for 2 months ago usage
			TestDateAttribute.Date = lastPeriod.ToDateTime();
			ARInvoice lastInvoice3 = Factory.NewWithValidTestData<ARInvoice>();
			lastInvoice3.AH_OH = org1.PK;
			var lastMonthBilled3 = Factory.New<EdiBilledUsage>();
			lastMonthBilled3.BU9_AH_Invoice = lastInvoice3.PK;
			lastMonthBilled3.BU9_LCC = lic1.ClientCompany.PK;
			lastMonthBilled3.BU9_LD = lic1.LA_LD;
			lastMonthBilled3.BU9_PeriodStart = lastPeriod.AddMonths(-1);
			Factory.Save();

			// Late usage for 2 months ago
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var tooOldUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", lastPeriod.AddMonths(-1), lic1.ClientCompany, 7);
			Factory.Save();

			// Saves for usage for last period
			TestDateAttribute.Date = periodStart.ToDateTime();
			var lastUsageExcludedSTL = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriod, lic1.ClientCompany, 5);
			var lastUsageExcludedODM = BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", lastPeriod, lic1.ClientCompany, 5);
			var lastUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", lastPeriod, lic2.ClientCompany, 11);
			var lastUsageManuallyProcessed1 = BillingTestHelper.CreateChargeableUsage(Factory, "DPS", "", lastPeriod, lic1.ClientCompany, 7);
			lastUsageManuallyProcessed1.U1_ManuallyProcessed = true;
			Factory.Save();

			// Saves for invoices for last period
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			ARInvoice lastInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			lastInvoice1.AH_OH = org1.PK;
			lastInvoice1.AH_SystemCreateTimeUtc = periodStart;
			ARInvoice lastInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			lastInvoice2.AH_OH = org2.PK;

			lastUsage2.U1_AH_Invoice = lastInvoice2.PK;

			var lastMonthBilled1 = Factory.New<EdiBilledUsage>();
			lastMonthBilled1.BU9_AH_Invoice = lastInvoice1.PK;
			lastMonthBilled1.BU9_LCC = lic1.ClientCompany.PK;
			lastMonthBilled1.BU9_LD = lic1.LA_LD;
			lastMonthBilled1.BU9_PeriodStart = lastPeriod;

			var lastMonthBilled2 = Factory.New<EdiBilledUsage>();
			lastMonthBilled2.BU9_AH_Invoice = lastInvoice2.PK;
			lastMonthBilled2.BU9_LCC = lic2.ClientCompany.PK;
			lastMonthBilled2.BU9_LD = lic2.LA_LD;
			lastMonthBilled2.BU9_PeriodStart = lastPeriod;

			Factory.Save();

			// Late usage
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			ARInvoice invoiceCancelled = Factory.NewWithValidTestData<ARInvoice>();
			invoiceCancelled.AH_OH = org1.PK;
			invoiceCancelled.AH_IsCancelled = true;
			var lastUsageLangPack = BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "GFR", lastPeriod, lic1.ClientCompany, 5);
			var lastUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", lastPeriod, lic1.ClientCompany, 7);
			lastUsage1.U1_AH_Invoice = invoiceCancelled.PK;
			Factory.Save();

			// Saves for usage for this period
			TestDateAttribute.Date = periodStart.ToDateTime();
			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", periodStart, lic1.ClientCompany, 5);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", periodStart, lic2.ClientCompany, 9);
			var usageManuallyProcessed1 = BillingTestHelper.CreateChargeableUsage(Factory, "DPS", "", periodStart, lic1.ClientCompany, 5);
			usageManuallyProcessed1.U1_ManuallyProcessed = true;
			Factory.Save();

			TestDateAttribute.Date = periodStart.AddHours(1).ToDateTime();
			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbInfoList = new List<BilledDatabase>();
			var usages = DatabaseUsageSet.GetUsages(context, false, null, 1, dbInfoList);
			AssertEquals(ClientChargeableUsageSchema.Constants.TableName, ((IBusinessObjectInternals)usages.First()).Row.Table.TableName);
			AssertEquals("invoiced usage from last month not included", false, usages.Any(x => x.PK == lastUsage2.PK));
			AssertEquals("cancelled invoice usage from last month IS included", true, usages.Any(x => x.PK == lastUsage1.PK));
			AssertEquals("usage1 this month included", true, usages.Any(x => x.PK == usage1.PK));
			AssertEquals("usage2 this month included", true, usages.Any(x => x.PK == usage2.PK));
			AssertEquals("ODM lang pack from last month included", true, usages.Any(x => x.PK == lastUsageLangPack.PK));
			AssertEquals("2 usage this month, 2 usage last month", 4, usages.Count);
			AssertEquals(2, dbInfoList.Count);
			dbInfoList.Single(x => x.PK == lic1.LA_LD && x.PriceHeaderPk == priceHeader.PK && x.IsMainDatabase && x.AgreedLiveDate == lic1.LA_AgreedLiveDate);
			dbInfoList.Single(x => x.PK == lic2.LA_LD && x.PriceHeaderPk == priceHeader.PK && x.IsMainDatabase && x.AgreedLiveDate == lic2.LA_AgreedLiveDate);

			priceHeader.Items[1].L7_FeeType = BillingConstants.FeeType.Transactional;
			Factory.Save();

			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1), ZGuid.Empty, lic1.Database.EnterpriseCode);
			dbInfoList = new List<BilledDatabase>();
			usages = DatabaseUsageSet.GetUsages(context, false, null, 2, dbInfoList);
			AssertEquals("cancelled invoice usage from last month IS included", true, usages.Any(x => x.PK == lastUsage1.PK));
			AssertEquals("usage1 this month included", true, usages.Any(x => x.PK == usage1.PK));
			AssertEquals("usage two months old is included", true, usages.Any(x => x.PK == tooOldUsage1.PK));
			AssertEquals("1 usage this month, 1 usage last month, 1 usage 2 months ago", 3, usages.Count);
			AssertEquals(1, dbInfoList.Count);
			dbInfoList.Single(x => x.PK == lic1.LA_LD && x.PriceHeaderPk == priceHeader.PK && x.IsMainDatabase);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 9, 1)]
		public void TestGetUsages_AccumulationAndPriceListChange()
		{
			var thisPeriod = new ZDateTime(TestDateAttribute.Date);
			var prevPeriod = thisPeriod.AddMonths(-1);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var keyUSR = new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR");
			var keyA = new UsageCodeKey(BillingConstants.BillingSystem.DeniedPartyScreening, "AAA");
			var keyB = new UsageCodeKey(BillingConstants.BillingSystem.AirlineMessaging, "BBB");
			var keyC = new UsageCodeKey(BillingConstants.BillingSystem.USCustoms, "CCC");
			var pricesOld = BillingTestHelper.CreateValidStlPriceListWithExchangeRates(stdLicCompany, keyUSR, keyA, keyB);
			var pricesNew = BillingTestHelper.CreateValidStlPriceListWithExchangeRates(stdLicCompany, keyUSR, keyA, keyB, keyC);
			pricesOld.L6_ValidFrom = prevPeriod;
			pricesNew.L6_ValidFrom = thisPeriod;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD");
			lic1.LA_AgreedLiveDate = thisPeriod.AddYears(-1);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreatePriceLink(lic1.Database, pricesOld, prevPeriod);
			BillingTestHelper.CreatePriceLink(lic1.Database, pricesNew, thisPeriod);

			ARInvoice lastInvoice = Factory.NewWithValidTestData<ARInvoice>();
			lastInvoice.AH_OH = lic1.Company.LC_OH;

			// prev usage
			var uu = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", prevPeriod, lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, keyA.Category, keyA.Code, prevPeriod, lic1.ClientCompany, 4);
			BillingTestHelper.CreateChargeableUsage(Factory, keyC.Category, keyC.Code, prevPeriod, lic1.ClientCompany, 3);

			Factory.Save();

			// Create previous month's invoice
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = prevPeriod.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().First();
			AssertNoErrors(bill1);
			var usageLines = bill1.MonthlyUsages.Single().UsageLines.ToList();
			AssertEquals("PRE: Usage billed prev month", "AAA x 4, USR x 5", string.Join(", ", usageLines.Select(x => x.SingleUsage.SubCode + " x " + x.TotalUnitCount).OrderBy(x => x)));
			bill1.CreateInvoices(ZDateTime.Empty);

			// "BBB" usage from previous month arrived after billing
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			BillingTestHelper.CreateChargeableUsage(Factory, "BBB", "BBB", prevPeriod, lic1.ClientCompany, 2);
			Factory.Save();

			// This month's usage
			TestDateAttribute.Date = thisPeriod.ToDateTime();

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", thisPeriod, lic1.ClientCompany, 9);
			BillingTestHelper.CreateChargeableUsage(Factory, keyA.Category, keyA.Code, thisPeriod, lic1.ClientCompany, 8);
			BillingTestHelper.CreateChargeableUsage(Factory, keyB.Category, keyB.Code, thisPeriod, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, keyC.Category, keyC.Code, thisPeriod, lic1.ClientCompany, 6);

			Factory.Save();

			// This month...
			var context = new BillingRunContext(new BusinessObjectFactory() { RefreshEnabled = false }, thisPeriod.AddMonths(1), thisPeriod.AddMonths(1).AddDays(-1));
			var dbInfoList = new List<BilledDatabase>();
			var usages = DatabaseUsageSet.GetUsages(context, true, null, 1, dbInfoList);
			var prevUsages = usages.Where(x => x.U1_PeriodStart == prevPeriod);
			var thisUsages = usages.Where(x => x.U1_PeriodStart == thisPeriod);
			CombineAssertions(() =>
			{
				AssertEquals("Only usage created after the invoice accumulates, so not CCC", "BBB x 2.0000", string.Join(", ", prevUsages.Select(x => (string)x.U1_SubCode + " x " + x.U1_UnitCount).OrderBy(x => x)));
				AssertEquals("Usage from this month", "AAA x 8.0000, BBB x 7.0000, CCC x 6.0000, USR x 9.0000", string.Join(", ", thisUsages.Select(x => (string)x.U1_SubCode + " x " + x.U1_UnitCount).OrderBy(x => x)));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 11, 1)]
		public void TestGetUsages_Accumulation_NotPerDatabase()
		{
			var thisPeriod = new ZDateTime(TestDateAttribute.Date);
			var prevPeriod = thisPeriod.AddMonths(-1);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var keyUSR = new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR");
			var keyA = new UsageCodeKey(BillingConstants.BillingSystem.DeniedPartyScreening, "AAA");
			var keyB = new UsageCodeKey(BillingConstants.BillingSystem.AirlineMessaging, "BBB");
			var prices = BillingTestHelper.CreateValidStlPriceListWithExchangeRates(stdLicCompany, keyUSR, keyA, keyB);
			prices.L6_ValidFrom = prevPeriod;
			prices.Items.FindByCode("AAA").L7_FeeType = "DAT";
			prices.Items.FindByCode("BBB").L7_FeeType = "TRA";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD");
			lic1.LA_AgreedLiveDate = thisPeriod.AddYears(-1);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreatePriceLink(lic1.Database, prices, prevPeriod);

			ARInvoice lastInvoice = Factory.NewWithValidTestData<ARInvoice>();
			lastInvoice.AH_OH = lic1.Company.LC_OH;

			// prev usage
			var usageU = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", prevPeriod, lic1.ClientCompany, 5);
			var usageA = BillingTestHelper.CreateChargeableUsage(Factory, keyA.Category, keyA.Code, prevPeriod, lic1.ClientCompany, 4);
			var usageB = BillingTestHelper.CreateChargeableUsage(Factory, keyB.Category, keyA.Code, prevPeriod, lic1.ClientCompany, 3);
			usageA.U1_Reference1 = "usageA - 001";
			usageB.U1_Reference1 = "usageB - 001";
			Factory.Save();

			// Create previous month's invoice
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = prevPeriod.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().First();
			AssertNoErrors(bill1);
			bill1.CreateInvoices(ZDateTime.Empty);

			// usages after invoice
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			usageA = BillingTestHelper.CreateChargeableUsage(Factory, keyA.Category, keyA.Code, prevPeriod, lic1.ClientCompany, 1);
			usageB = BillingTestHelper.CreateChargeableUsage(Factory, keyB.Category, keyB.Code, prevPeriod, lic1.ClientCompany, 2);
			usageA.U1_Reference1 = "usageA - 002";
			usageB.U1_Reference1 = "usageB - 002";
			Factory.Save();

			// This month's usage
			TestDateAttribute.Date = thisPeriod.ToDateTime();
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", thisPeriod, lic1.ClientCompany, 9);
			BillingTestHelper.CreateChargeableUsage(Factory, keyA.Category, keyA.Code, thisPeriod, lic1.ClientCompany, 8);
			BillingTestHelper.CreateChargeableUsage(Factory, keyB.Category, keyB.Code, thisPeriod, lic1.ClientCompany, 7);
			Factory.Save();

			// This month...
			var context = new BillingRunContext(new BusinessObjectFactory() { RefreshEnabled = false }, thisPeriod.AddMonths(1), thisPeriod.AddMonths(1).AddDays(-1));
			var dbInfoList = new List<BilledDatabase>();
			var usages = DatabaseUsageSet.GetUsages(context, true, null, 1, dbInfoList);

			var usagesAsString = string.Join("\r\n", usages.Select(x => $"{x.U1_PeriodStart.ToISO8601ShortDateString()} - {x.U1_Code} - {x.U1_SubCode} - {x.U1_UnitCount}").OrderBy(x => x));
			AssertEquals(@"2022-10-01 - AMG - BBB - 2.0000
2022-11-01 - AMG - BBB - 7.0000
2022-11-01 - DPS - AAA - 8.0000
2022-11-01 - STL - USR - 9.0000", usagesAsString);
		}

		// Disables critical validation "Missing Reversing Transaction for this canceled transaction".
		[SuspendCriticalValidation]
		public void TestGetDatabaseUsages()
		{
			var periodStart = new ZDate(2015, 12, 1);
			var lastPeriod = periodStart.AddMonths(-1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var org1 = lic1.Company.Header;

			ARInvoice invoiceCancelled = Factory.NewWithValidTestData<ARInvoice>();
			invoiceCancelled.AH_OH = org1.PK;
			invoiceCancelled.AH_IsCancelled = true;

			ARInvoice lastInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			lastInvoice1.AH_OH = org1.PK;

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", periodStart, lic1.ClientCompany, 5);
			var usageManuallyProcessed1 = BillingTestHelper.CreateChargeableUsage(Factory, "DPS", "", periodStart, lic1.ClientCompany, 5);
			var lastUsageExcludedSTL = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriod, lic1.ClientCompany, 5);
			var lastUsageExcludedODM = BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", lastPeriod, lic1.ClientCompany, 5);
			var lastUsageLangPack = BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "GFR", lastPeriod, lic1.ClientCompany, 5);
			var lastUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", lastPeriod, lic1.ClientCompany, 7);
			var lastUsageManuallyProcessed1 = BillingTestHelper.CreateChargeableUsage(Factory, "DPS", "", lastPeriod, lic1.ClientCompany, 7);
			var tooOldUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", lastPeriod.AddMonths(-1), lic1.ClientCompany, 7);
			lastUsage1.U1_AH_Invoice = invoiceCancelled.PK;
			usageManuallyProcessed1.U1_ManuallyProcessed = true;
			lastUsageManuallyProcessed1.U1_ManuallyProcessed = true;

			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", periodStart, lic2.ClientCompany, 9);

			var lastMonthBilled1 = Factory.New<EdiBilledUsage>();
			lastMonthBilled1.BU9_AH_Invoice = lastInvoice1.PK;
			lastMonthBilled1.BU9_LCC = lic1.ClientCompany.PK;
			lastMonthBilled1.BU9_LD = lic1.LA_LD;
			lastMonthBilled1.BU9_PeriodStart = lastPeriod;

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var usages = DatabaseUsageSet.GetDatabaseUsages(context.Factory, context.PeriodStart, lic1.LA_LD.ToGuid(), new string[] { "GFR" });
			AssertEquals("usage1 this month included", true, usages.Any(x => x.PK == usage1.PK));
		}

		public void TestHostedProductionUsage()
		{
			var periodStart = new ZDateTime(2020, 11, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_HostedLocation = "SYD";
			var priceHeader = BillingTestHelper.CreateStlPrices(lic1.Company, new UsageCodeKey(BillingConstants.BillingSystem.Service, BillingConstants.Hosting.WiseCloudProductionLicenceCode));
			var priceLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = periodStart.AddYears(-1);
			priceLink.PHL_RX_NKCurrency = "AUD";

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			var usage = dbUsage.Usages.Single(x => x.SubCode == BillingConstants.Hosting.WiseCloudProductionLicenceCode);
			AssertEquals(lic1.Database.PK.ToGuid(), usage.MainDatabasePk);

			// Set database to not hosted
			lic1.Database.LD_HostedLocation = Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			Factory.Save();
			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(0, dbUsageSet.DatabaseUsages.Count());
		}

		public void TestNonProductionUsage()
		{
			var periodStart = new ZDateTime(2015, 12, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var priceHeader = BillingTestHelper.CreateStlPrices(lic1.Company, new UsageCodeKey(BillingConstants.BillingSystem.Service, "#NP"));
			priceHeader.L6_TestDbPriceCode = "#NP";
			var priceLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = periodStart.AddYears(-1);
			priceLink.PHL_RX_NKCurrency = "AUD";

			var testLic = BillingTestHelper.CreateAnotherDatabase(lic1, "TST");
			testLic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			testLic.Database.LD_LD_ParentDatabase = lic1.Database.PK;

			BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "#HG", periodStart, testLic, 7);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			var testUsage = dbUsage.Usages.Single(x => x.SubCode == "#NP");
			AssertEquals(lic1.Database.PK.ToGuid(), testUsage.MainDatabasePk);
			AssertEquals(testLic.Database.PK.ToGuid(), testUsage.Database.PK);
		}

		public void TestNonProductionUsage_NotBillable()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var licProd = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD");
			licProd.LA_AgreedLiveDate = periodStart.AddMonths(-1);
			licProd.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var licTestNotBillable = BillingTestHelper.CreateAnotherDatabase(licProd, "TST");
			licTestNotBillable.LA_AgreedLiveDate = periodStart.AddMonths(-6);
			licTestNotBillable.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			licTestNotBillable.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			licTestNotBillable.Database.LD_LD_ParentDatabase = licProd.LA_LD;
			licTestNotBillable.Database.LD_Billable = DatabaseBillableFlagList.Codes.No;

			var licTestBillable = BillingTestHelper.CreateAnotherDatabase(licProd, "TS2");
			licTestBillable.LA_AgreedLiveDate = periodStart.AddMonths(-6);
			licTestBillable.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			licTestBillable.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			licTestBillable.Database.LD_LD_ParentDatabase = licProd.LA_LD;
			licTestBillable.Database.LD_Billable = DatabaseBillableFlagList.Codes.YesCustomer;

			var priceHeader = BillingTestHelper.CreateStlPrices(licProd.Company,
				new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR"),
				new UsageCodeKey(BillingConstants.BillingSystem.Service, "#NP"),
				new UsageCodeKey(BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.NonProductionStorageCode));
			priceHeader.L6_TestDbPriceCode = "#NP";

			BillingTestHelper.CreatePriceLink(licProd.Database, priceHeader, periodStart.AddYears(-1));

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, licProd, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, periodStart, licProd, 50000);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.NonProductionStorageCode, periodStart, licTestNotBillable, 10000);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.NonProductionStorageCode, periodStart, licTestBillable, 20000);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			var actual = string.Join("\r\n", dbUsage.Usages.Select(x => x.Database.LD_ServerCode + " - " + x.SubCode + " - " + x.UnitCount).OrderBy(x => x));
			AssertEquals("no test system usage when test system is not billable",
@"PRD - #HD - 50000.0000
PRD - USR - 10.0000
TS2 - #HN - 20000.0000", actual);
		}

		public void TestNonProductionUsage_TestSystemOnly()
		{
			var periodStart = new ZDateTime(2015, 12, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var priceHeader = BillingTestHelper.CreateStlPrices(lic1.Company, new UsageCodeKey(BillingConstants.BillingSystem.Service, "#NP"));
			priceHeader.L6_TestDbPriceCode = "#NP";
			var priceLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = periodStart.AddYears(-1);
			priceLink.PHL_RX_NKCurrency = "AUD";
			lic1.Database.LD_LicenceType = "TST";

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			var testUsage = dbUsage.Usages.Single(x => x.SubCode == "#NP");
			AssertEquals(lic1.Database.PK.ToGuid(), testUsage.MainDatabasePk);
			AssertEquals(lic1.Database.PK.ToGuid(), testUsage.Database.PK);
		}

		public void TestNonProductionUsage_ProductionSystemNotLive()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var licProdNotLive = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD");
			licProdNotLive.LA_AgreedLiveDate = periodStart.AddMonths(6);
			licProdNotLive.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var licTest = BillingTestHelper.CreateAnotherDatabase(licProdNotLive, "TST");
			licTest.LA_AgreedLiveDate = periodStart.AddMonths(-1);
			licTest.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			licTest.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			licTest.Database.LD_LD_ParentDatabase = licProdNotLive.LA_LD;

			// Another production system that is live
			var licProd2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "PRD");
			licProd2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			licProd2.LA_AgreedLiveDate = periodStart.AddMonths(-1);

			var priceHeader = BillingTestHelper.CreateStlPriceList(licProdNotLive.Company, "#NP", BillingConstants.Hosting.NonProductionStorageCode);
			priceHeader.L6_TestDbPriceCode = "#NP";

			BillingTestHelper.CreatePriceLink(licProdNotLive.Database, priceHeader, periodStart.AddYears(-1));
			BillingTestHelper.CreatePriceLink(licProd2.Database, priceHeader, periodStart.AddYears(-1));

			// only the test system has usage
			BillingTestHelper.CreateChargeableUsage(Factory, "HOS", BillingConstants.Hosting.NonProductionStorageCode, periodStart, licTest, 100);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, true, null, 0);
			AssertEquals(0, dbUsageSet.DatabaseUsages.Count());
		}

		public void TestPremiumServicePerCompanyBillingAndNoUsage()
		{
			var periodStart = new ZDateTime(2015, 12, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_IsBilledPerCompany = true;
			lic1.LA_RX_NKPriceCurrency = "AUD";
			var prices = BillingTestHelper.CreateStlPriceList(lic1.Company, "#HC");
			var service = BillingTestHelper.CreatePremiumService(lic1.Database, "#HC", periodStart, ZDateTime.Empty);
			var priceLink = BillingTestHelper.CreatePriceLink(lic1.Database, prices, periodStart, "AUD");

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 1);
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			AssertEquals("usage has correct LicHeader", lic1.PK, dbUsage.OwnerDelivery.Owner.LicHeader.PK);
		}

		public void TestPerCompanyBillingAndHostingUsage()
		{
			var periodStart = new ZDateTime(2015, 12, 1);

			// Owner of company specific usage
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_IsBilledPerCompany = true;
			lic1.LA_RX_NKPriceCurrency = "USD";
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "USD");

			// Owner of database level usage like hosting
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB", false);
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.Database.LD_IsBilledPerCompany = true;
			lic2.LA_RX_NKPriceCurrency = "NZD";
			lic2.Database.LD_OH_BillingParty = lic2.Company.LC_OH;
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranchPK, "USD");

			var prices = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR", BillingConstants.Hosting.DataAccessCode);
			var priceLink = BillingTestHelper.CreatePriceLink(lic1.Database, prices, periodStart, "USD");

			var databaseLevelChargeableUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingDataAccess, BillingConstants.Hosting.DataAccessCode, periodStart, lic1, 2000);
			databaseLevelChargeableUsage.U1_LCC = ZGuid.Empty;

			var clientCompanyChargeableUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, lic1.ClientCompany, 7);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals("count of databases with usage", 1, dbUsageSet.DatabaseUsages.Count());
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			var databaseLevelUsage = dbUsage.Usages.Single(x => x.Code == BillingConstants.BillingSystem.HostingDataAccess);
			var companyLevelUsage = dbUsage.Usages.Single(x => x.Code == BillingConstants.BillingSystem.STL);

			AssertEquals("NZD", databaseLevelUsage.OwnerDelivery.Owner.PerCompanyBillingPriceCurrency);
			AssertEquals("USD", companyLevelUsage.OwnerDelivery.Owner.PerCompanyBillingPriceCurrency);
		}

		public void TestPerCompanyBillingAndMissingInvoiceDelivery()
		{
			var periodStart = new ZDateTime(2015, 12, 1);

			// Owner of company specific usage
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_IsBilledPerCompany = true;
			lic1.LA_RX_NKPriceCurrency = "USD";
			var delivery = BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "USD");
			delivery.L9_SystemCode = "PUR";
			AssertEquals(false, delivery.IsAllSystemCode);

			var prices = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR", BillingConstants.Hosting.DataAccessCode);
			var priceLink = BillingTestHelper.CreatePriceLink(lic1.Database, prices, periodStart, "USD");
			var clientCompanyChargeableUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, lic1.ClientCompany, 7);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals("count of databases with usage", 1, dbUsageSet.DatabaseUsages.Count());
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			var companyLevelUsage = dbUsage.Usages.Single(x => x.Code == BillingConstants.BillingSystem.STL);

			AssertNull(companyLevelUsage.OwnerDelivery.Delivery);
		}

		[TestDate(2017, 6, 1)]
		public void TestAddValidationNotifications_StlMilestone()
		{
			var stdLicCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var stlPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			var stlPriceList = new PriceList(stlPriceHeader, null, null, null, null);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");

			// Note milestone error check was added for period 2016-11 and later.
			var period1TooOld = new ZDateTime(2016, 10, 1);
			var period2HasLastMilestone = new ZDateTime(2016, 11, 1);
			var period3MissingLastMilestone = new ZDateTime(2016, 12, 1);
			var period4MissingAllMilestone = new ZDateTime(2017, 1, 1);
			var period5TwoMilestones = new ZDateTime(2017, 2, 1);
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPriceHeader, period1TooOld);

			Factory.Save();

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1TooOld, lic1.ClientCompany, 5);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period2HasLastMilestone, lic1.ClientCompany, 5);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period3MissingLastMilestone, lic1.ClientCompany, 5);
			var usage4 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period4MissingAllMilestone, lic1.ClientCompany, 5);
			var usage5a = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period5TwoMilestones, lic1.ClientCompany, 5);
			var usage5b = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period5TwoMilestones, lic2.ClientCompany, 7);

			var period1MilestoneUsage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "STL", period1TooOld, lic1.ClientCompany, 2);
			var period2MilestoneUsage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "STL", period2HasLastMilestone, lic1.ClientCompany, 1);
			var period3MilestoneUsage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "STL", period3MissingLastMilestone, lic1.ClientCompany, 2);
			var period5MilestoneBad = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "STL", period5TwoMilestones, lic1.ClientCompany, 2);
			var period5MilestoneGood = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "STL", period5TwoMilestones, lic2.ClientCompany, 1);

			var owner1 = new UsageOwnerDelivery(new UsageOwner(lic1), null, lic1.Company);

			var dbUsage1TooOldForMilestones = new DatabaseUsage(owner1, new Usage[] { new Usage(usage1), new Usage(period1MilestoneUsage) },
				"", stlPriceList, null, period1TooOld, true, new Dictionary<string, PriceList>());
			var bizo = Factory.New<DummyBusinessObject>();
			dbUsage1TooOldForMilestones.AddValidationNotifications(bizo);
			AssertNoNotifications(bizo);

			var dbUsage2HasMilestone = new DatabaseUsage(owner1, new Usage[] { new Usage(usage2), new Usage(period2MilestoneUsage) },
				"", stlPriceList, null, period2HasLastMilestone, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage2HasMilestone.AddValidationNotifications(bizo);
			AssertNoNotifications(bizo);

			var dbUsage3MissingLastMilestone = new DatabaseUsage(owner1, new Usage[] { new Usage(usage3), new Usage(period3MilestoneUsage) },
				"", stlPriceList, null, period3MissingLastMilestone, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage3MissingLastMilestone.AddValidationNotifications(bizo);
			AssertHasRowError(bizo, "Database AAA is missing some STL data");

			var dbUsage4MissingAllMilestone = new DatabaseUsage(owner1, new Usage[] { new Usage(usage4) },
				"", stlPriceList, null, period4MissingAllMilestone, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage4MissingAllMilestone.AddValidationNotifications(bizo);
			AssertNoNotifications(bizo);

			var dbUsage5TwoMilestones = new DatabaseUsage(owner1, new Usage[] { new Usage(usage5a), new Usage(usage5b), new Usage(period5MilestoneBad), new Usage(period5MilestoneGood) },
				"", stlPriceList, null, period5TwoMilestones, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage5TwoMilestones.AddValidationNotifications(bizo);
			AssertNoNotifications(bizo);
		}

		public void TestAddValidationNotifications_StlPriceListMissing()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			Factory.Save();

			var periodStart = BillingTestHelper.MonthToday;

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			var owner1 = new UsageOwnerDelivery(new UsageOwner(lic1), null, lic1.Company);

			var dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usage1), new Usage(usage1) },
				"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			var bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertHasRowError(bizo, "Database AAA is missing an STL pricelist");
		}

		public void TestAddValidationNotifications_NonCW1PriceListMissingShouldNotShowErrorForBorderWise()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			lic.LA_LicenceAdvStdOth = "STL";
			lic.Database.LD_Product = ProductTypes.Codes.BorderWise;
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "EDF", "SYD");
			Factory.Save();

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			Factory.Save();

			var owner1 = new UsageOwnerDelivery(new UsageOwner(lic), null, lic.Company);
			var periodStart = BillingTestHelper.MonthToday;

			var usage0 = BillingTestHelper.CreateChargeableUsage(Factory, ProductTypes.Codes.BorderWise, "USR", periodStart, lic.ClientCompany, 5);
			AssertEquals("Precondition: Should not be global", false, EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.ContainsProductCode(ProductTypes.Codes.BorderWise));
			var dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usage0), },
				"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			var bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertNoRowError("Should not show error if stl price list missing and the product is BorderWise", bizo, $"Database SYD is missing the {ProductTypes.Codes.BorderWise} pricelist");

			lic.Database.LD_Product = "ABC";
			Factory.Save();

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ABC", "USR", periodStart, lic.ClientCompany, 5);
			AssertEquals("Precondition: Should not be global", false, EDIDataRegistry.Instance.UsageBillingSettings.Value.PriceLists.ContainsProductCode("ABC"));

			dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usage1), new Usage(usage1) },
				"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertHasRowError("Should show error if stl price list missing and the product is not global", bizo, "Database SYD is missing the ABC pricelist");
		}

		public void TestAddValidationNotifications_NonCW1PriceListMissing()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			lic.LA_LicenceAdvStdOth = "STL";
			lic.Database.LD_Product = "STL";
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "EDF", "SYD");
			Factory.Save();

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			Factory.Save();

			var owner1 = new UsageOwnerDelivery(new UsageOwner(lic), null, lic.Company);
			var periodStart = BillingTestHelper.MonthToday;

			var usage0 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic.ClientCompany, 5);
			AssertEquals("Precondition: Should not be global", false, PriceHeaderType.IsGlobal("STL"));
			var dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usage0), },
				"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			var bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertNoRowError("Should not show error if stl price list missing and the product is not global", bizo, "Database SYD is missing the ABC pricelist");

			lic.Database.LD_Product = "ABC";
			Factory.Save();

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ABC", "USR", periodStart, lic.ClientCompany, 5);
			AssertEquals("Precondition: Should not be global", false, PriceHeaderType.IsGlobal("ABC"));

			dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usage1), new Usage(usage1) },
				"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertHasRowError("Should show error if stl price list missing and the product is not global", bizo, "Database SYD is missing the ABC pricelist");

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("ABC", "ABC");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);
			AssertEquals("Precondition: Should be global", true, PriceHeaderType.IsGlobal("ABC"));
			dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usage1), new Usage(usage1) },
				"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertNoRowError("Should not show error if the product is global", bizo, "Database SYD is missing the ABC pricelist");
		}

		public void TestAddValidationNotifications_USR_RBU_Missing()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			Factory.Save();

			var periodStart = BillingTestHelper.MonthToday;
			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic1.ClientCompany, 1);
			var owner1 = new UsageOwnerDelivery(new UsageOwner(lic1), null, lic1.Company);

			var dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usage1) },
				"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			var bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertEquals(0, dbUsage.ActiveUserCount);
			AssertEquals(0, dbUsage.RoboticUserCount);
			AssertHasRowError(bizo, "Database AAA is missing STL Active User and Robotic User data");

			var rbu1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "RBU", periodStart, lic1.ClientCompany, 1);
			dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(rbu1) },
						"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertEquals(0, dbUsage.ActiveUserCount);
			AssertEquals(1, dbUsage.RoboticUserCount);
			AssertNoRowError(bizo, "Database AAA is missing STL Active User and Robotic User data");

			var usr1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 1);
			dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usr1) },
						"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertEquals(1, dbUsage.ActiveUserCount);
			AssertEquals(0, dbUsage.RoboticUserCount);
			AssertNoRowError(bizo, "Database AAA is missing STL Active User and Robotic User data");

			dbUsage = new DatabaseUsage(owner1, new Usage[] { new Usage(usr1), new Usage(rbu1) },
						"", null, null, periodStart, true, new Dictionary<string, PriceList>());
			bizo = Factory.New<DummyBusinessObject>();
			dbUsage.AddValidationNotifications(bizo);
			AssertEquals(1, dbUsage.ActiveUserCount);
			AssertEquals(1, dbUsage.RoboticUserCount);
			AssertNoRowError(bizo, "Database AAA is missing STL Active User and Robotic User data");
		}

		public void TestBorderWise()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany);
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "AUD";

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BOR");
			var lic2 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BOR");
			var licCw = BillingTestHelper.CreateAnotherDatabase(lic1, "CW1", true);
			licCw.LA_AgreedLiveDate = periodStart;
			licCw.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(licCw.Database, stlPrices, periodStart);

			var priceSetting1 = Factory.New<PriceLicenceSetting>();
			priceSetting1.LS9_LD = lic1.LA_LD;
			priceSetting1.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode);
			priceSetting1.LS9_Price = 77;
			priceSetting1.LS9_RX_NKPriceCurrency = "AUD";
			lic1.Database.LicenceSettings.Add(priceSetting1);
			var legacyLicences = Factory.New<BorderWisePurchasedLicenceSetting>();
			legacyLicences.LS9_LD = lic1.LA_LD;
			legacyLicences.LicenceCount = 2;
			legacyLicences.PriceCode = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			lic1.Database.LicenceSettings.Add(priceSetting1);

			var priceSetting2 = Factory.New<PriceLicenceSetting>();
			priceSetting2.LS9_LD = licCw.LA_LD;
			priceSetting2.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR");
			priceSetting2.LS9_Price = 44;
			priceSetting2.LS9_RX_NKPriceCurrency = "AUD";
			licCw.Database.LicenceSettings.Add(priceSetting2);

			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicing(lic2.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, lic1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUProPackPriceCode, periodStart, lic1.LA_LC, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCw.ClientCompany, 5);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, lic2.LA_LC, 11);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null, new GenericUsageSetFactory());
			AssertEquals(2, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == licCw.LA_LD);
			var db2Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic2.LA_LD);
			AssertNotNull(db1Usage.GetCustomPriceSetting(new UsageCodeKey("STL", "USR")));
			AssertNotNull(db1Usage.GetCustomPriceSetting(new UsageCodeKey(BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode)));
			AssertEquals(1, db1Usage.GetBorderWisePurchasedLicenceSettings().Count());
			AssertEquals(2, db1Usage.GetBorderWisePurchasedLicenceSettings().First().LicenceCount);
			AssertEquals(3, db1Usage.Usages.Count());
			AssertEquals("BOR usage included", 7m, db1Usage.Usages.Single(x => x.SubCode == BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode).UnitCount);
			AssertEquals("BOR usage included", 3m, db1Usage.Usages.Single(x => x.SubCode == BillingConstants.BorderWise.AUProPackPriceCode).UnitCount);
			AssertEquals("STL usage included", 5m, db1Usage.Usages.Single(x => x.SubCode == "USR").UnitCount);

			AssertEquals(1, db2Usage.Usages.Count());
			AssertEquals("BOR usage included", 11m, db2Usage.Usages.Single(x => x.SubCode == BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode).UnitCount);
		}

		public void TestGoldenTax()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var goldenTaxPrices = BillingTestHelper.CreateGoldenTaxPriceList(stdLicCompany);
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "AUD";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.Category.GoldenTax, "GTS", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			AssertEquals(2, db1Usage.Usages.Count());
			AssertEquals(7m, db1Usage.Usages.Single(x => x.SubCode == "GTS").UnitCount);
			AssertEquals(5m, db1Usage.Usages.Single(x => x.SubCode == "USR").UnitCount);
		}

		public void TestGoldenTax_UniversalAndStlPricing()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var goldenTaxPrices = BillingTestHelper.CreateGoldenTaxPriceList(stdLicCompany);
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "AUD";
			var goldenTaxOnStlPriceList = BillingTestHelper.AddPriceItem(stlPrices, new UsageCodeKey("ACC", "GTS"), "TRA", 0.16m);
			goldenTaxOnStlPriceList.L7_Description = "  New Per Additional Invoice";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.Category.GoldenTax, "GTS", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			AssertEquals(2, db1Usage.Usages.Count());
			AssertEquals(7m, db1Usage.Usages.Single(x => x.SubCode == "GTS").UnitCount);
			AssertEquals(5m, db1Usage.Usages.Single(x => x.SubCode == "USR").UnitCount);
		}

		public void TestGoldenTax_StlPricingOnly()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "AUD";
			var goldenTaxOnStlPriceList = BillingTestHelper.AddPriceItem(stlPrices, new UsageCodeKey("ACC", "GTS"), "TRA", 0.16m);
			goldenTaxOnStlPriceList.L7_Description = "  New Per Additional Invoice";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.Category.GoldenTax, "GTS", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			AssertEquals(2, db1Usage.Usages.Count());
			AssertEquals(7m, db1Usage.Usages.Single(x => x.SubCode == "GTS").UnitCount);
			AssertEquals(5m, db1Usage.Usages.Single(x => x.SubCode == "USR").UnitCount);
		}

		public void TestGlobalPriceListFromConfig()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("INV", "E-Invoicing");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "IT1";
			usageCodes1.PriceHeaderCode = "INV";
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "AUD";

			var globalPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "IT1");
			globalPrices1.L6_SystemCode = "INV";
			AssertNoErrors(globalPrices1.L6_SystemCodeInfo);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IT1", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			AssertEquals(2, db1Usage.Usages.Count());
			var usageGlobal = db1Usage.Usages.Single(x => x.SubCode == "IT1");
			AssertEquals(7m, usageGlobal.UnitCount);
			AssertEquals("ACC", usageGlobal.Code);

			var usageUSR = db1Usage.Usages.Single(x => x.SubCode == "USR");
			AssertEquals(5m, usageUSR.UnitCount);
			AssertEquals("STL", usageUSR.Code);
		}

		public void TestGlobalPriceList_Mapping()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("INV", "E-Invoicing");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "IT1";
			usageCodes1.PriceHeaderCode = "INV";
			var usageCodes2 = billingCodes.AddNew();
			usageCodes2.Category = "ACC";
			usageCodes2.PriceItemCode = "VN1";
			usageCodes2.PriceHeaderCode = "INV";
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "AUD";

			var globalPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "IT1");
			globalPrices1.L6_SystemCode = "INV";
			AssertNoErrors(globalPrices1.L6_SystemCodeInfo);
			BillingTestHelper.AddUsageMap(globalPrices1, "INV", "IT1", "VN1");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "IT1", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "VN1", periodStart, lic1.ClientCompany, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			var prices = db1Usage.GetPriceList("INV");
			var nodeIT1 = prices.GetPriceNodeFromUsageKey(new UsageCodeKey("ACC", "IT1"));
			var nodeVN1 = prices.GetPriceNodeFromUsageKey(new UsageCodeKey("ACC", "VN1"));
			AssertEquals("usage is mapped", nodeIT1, nodeVN1);

			AssertEquals(3, db1Usage.Usages.Count());
			var usageGlobal = db1Usage.Usages.Single(x => x.SubCode == "IT1");
			AssertEquals(7m, usageGlobal.UnitCount);
			AssertEquals("ACC", usageGlobal.Code);

			var usageUSR = db1Usage.Usages.Single(x => x.SubCode == "USR");
			AssertEquals(5m, usageUSR.UnitCount);
			AssertEquals("STL", usageUSR.Code);
		}

		public void TestFlightStats()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var flightStatsPrices = stdLicCompany.PriceHeaders.AddNew();
			flightStatsPrices.L6_SystemCode = BillingConstants.PriceHeaderType.FlightStats;
			flightStatsPrices.L6_UseStandardDiscount = false;
			flightStatsPrices.L6_DiscountCode = "STL1";
			flightStatsPrices.L6_PricelistVersion = "FMS v1";
			flightStatsPrices.L6_RX_NKCurrency = "AUD";
			flightStatsPrices.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var item1 = flightStatsPrices.Items.AddNew();
			item1.L7_Code = "FMS";
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_Description = "Air Waybill Automation";
			item1.L7_Price = 200m;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "AUD";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.FlightStats, "FMS", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			AssertEquals(2, db1Usage.Usages.Count());
			AssertEquals(7m, db1Usage.Usages.Single(x => x.SubCode == "FMS").UnitCount);
			AssertEquals(5m, db1Usage.Usages.Single(x => x.SubCode == "USR").UnitCount);
		}

		public void TestHasCustomHighVolumeFeatureSetting()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "C01");
			stlPrices.L6_RX_NKCurrency = "AUD";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C01", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);

			var hvfSetting = Factory.New<HighVolumeFeatureSetting>();
			hvfSetting.LS9_LD = lic1.LA_LD;
			hvfSetting.PriceKey = new UsageCodeKey("STL", "USR");
			lic1.Database.LicenceSettings.Add(hvfSetting);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			AssertEquals(true, db1Usage.HasCustomHighVolumeFeatureSetting(new UsageCodeKey("STL", "USR")));
			AssertEquals(false, db1Usage.HasCustomHighVolumeFeatureSetting(new UsageCodeKey("STL", "C01")));

			AssertEquals(2, db1Usage.Usages.Count());
			AssertEquals(7m, db1Usage.Usages.Single(x => x.SubCode == "C01").UnitCount);
			AssertEquals(5m, db1Usage.Usages.Single(x => x.SubCode == "USR").UnitCount);
		}

		public void TestBilledDatabaseIsEnterpriseFamilyDatabase()
		{
			var db1 = new BilledDatabase(ZGuid.Empty, "", 0, ZGuid.Empty, true, "", true, ZGuid.Empty, "", ZGuid.Empty, ZDateTime.Empty, true, "", ProductTypes.Codes.Enterprise, "", ZGuid.Empty);
			var db2 = new BilledDatabase(ZGuid.Empty, "", 0, ZGuid.Empty, true, "", true, ZGuid.Empty, "", ZGuid.Empty, ZDateTime.Empty, true, "", ProductTypes.Codes.CargoWiseOne, "", ZGuid.Empty);
			var db3 = new BilledDatabase(ZGuid.Empty, "", 0, ZGuid.Empty, true, "", true, ZGuid.Empty, "", ZGuid.Empty, ZDateTime.Empty, true, "", ProductTypes.Codes.ProductivityWise, "", ZGuid.Empty);
			var db4 = new BilledDatabase(ZGuid.Empty, "", 0, ZGuid.Empty, true, "", true, ZGuid.Empty, "", ZGuid.Empty, ZDateTime.Empty, true, "", ProductTypes.Codes.GLOW, "", ZGuid.Empty);
			var db5 = new BilledDatabase(ZGuid.Empty, "", 0, ZGuid.Empty, true, "", true, ZGuid.Empty, "", ZGuid.Empty, ZDateTime.Empty, true, "", ProductTypes.Codes.CargoWiseNext, "", ZGuid.Empty);

			AssertEquals(true, db1.IsEnterpriseFamilyDatabase);
			AssertEquals(true, db2.IsEnterpriseFamilyDatabase);
			AssertEquals(true, db3.IsEnterpriseFamilyDatabase);
			AssertEquals(false, db4.IsEnterpriseFamilyDatabase);
			AssertEquals(true, db5.IsEnterpriseFamilyDatabase);
		}

		public void TestGetDiscountSuspensionPolicyLicenceSetting()
		{
			var periodStart = new ZDateTime(2015, 12, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var priceHeader = BillingTestHelper.CreateStlPrices(lic1.Company, new UsageCodeKey(BillingConstants.BillingSystem.Service, "#NP"));
			priceHeader.L6_TestDbPriceCode = "#NP";
			var priceLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = periodStart.AddYears(-1);
			priceLink.PHL_RX_NKCurrency = "AUD";
			lic1.Database.LD_LicenceType = "TST";
			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var dbUsage = dbUsageSet.DatabaseUsages.First();
			AssertNull(dbUsage.GetDiscountSuspensionPolicyLicenceSetting());

			var setting = lic1.Database.LicenceSettings.AddNew();
			setting.LS9_Type = BillingConstants.LicenceSetting.DiscountSuspensionPolicy;
			setting.LS9_Name = "NVR";
			setting.LS9_ValidFrom = periodStart.AddMonths(1);
			Factory.Save();
			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			dbUsage = dbUsageSet.DatabaseUsages.First();
			AssertNull(dbUsage.GetDiscountSuspensionPolicyLicenceSetting());

			setting.LS9_ValidFrom = periodStart;
			Factory.Save();
			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			dbUsageSet = new DatabaseUsageSet(context, false, null, 0);
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			dbUsage = dbUsageSet.DatabaseUsages.First();
			AssertEquals("NVR", dbUsage.GetDiscountSuspensionPolicyLicenceSetting().PolicyCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 9, 1)]
		public void TestBillingSummaryCurrencySetting()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var thisPeriod = new ZDateTime(TestDateAttribute.Date);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var keyA = new UsageCodeKey("SAT", "P01");
			var priceHeader = BillingTestHelper.CreateValidStlPriceListWithExchangeRates(stdLicCompany, keyA);
			priceHeader.L6_SystemCode = "DEF";
			priceHeader.L6_ValidFrom = thisPeriod;
			var rate = priceHeader.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "USD";
			rate.PHE_Rate = 0.6m;
			rate.PHE_UpliftPercent = 0m;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");
			lic1.Database.LD_Product = "ABC";
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", thisPeriod, lic1.ClientCompany, 5);

			var currencySetting = Factory.New<BillingSummaryCurrencyLicenceSetting>();
			currencySetting.LS9_LD = lic1.LA_LD;
			currencySetting.LS9_RX_NKPriceCurrency = "USD";
			lic1.Database.LicenceSettings.Add(currencySetting);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, thisPeriod.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null, new GenericUsageSetFactory());
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);

			AssertEquals("USD", db1Usage.PriceCurrency);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNonCW1DeliveryInstruction()
		{
			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var abcDatabase = lic1.Database;
			abcDatabase.LD_Product = "ABC";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "CW2");
			var cw1Database = lic2.Database;

			var stlPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CUS");
			BillingTestHelper.CreatePriceLink(lic2.Database, stlPriceHeader, periodStart);

			var delivery1 = lic1.Company.InvoiceDeliveries.AddNew();
			delivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			delivery1.L9_RX_NKInvoiceCurrency = "AUD";
			delivery1.L9_OH_InvoiceTo = org1.PK;
			delivery1.L9_SystemCode = "ABC";
			delivery1.L9_ServerCode = abcDatabase.LD_ServerCode;

			var delivery2 = lic1.Company.InvoiceDeliveries.AddNew();
			delivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			delivery2.L9_RX_NKInvoiceCurrency = "USD";
			delivery2.L9_OH_InvoiceTo = org2.PK;
			delivery2.L9_SystemCode = "STL";
			delivery2.L9_ServerCode = cw1Database.LD_ServerCode;

			var delivery3 = lic1.Company.InvoiceDeliveries.AddNew();
			delivery3.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			delivery3.L9_RX_NKInvoiceCurrency = "GBP";
			delivery3.L9_OH_InvoiceTo = org2.PK;
			delivery3.L9_SystemCode = "ALL";
			delivery3.L9_ServerCode = "";

			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", periodStart, lic1.Database.ClientCompanies[0], 6);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2.Database.ClientCompanies[0], 1);
			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null, new GenericUsageSetFactory());
			AssertEquals(2, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == abcDatabase.PK);
			var db2Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == cw1Database.PK);

			AssertEquals(delivery1.PK, db1Usage.OwnerDelivery.Delivery.PK);
			AssertEquals(delivery2.PK, db2Usage.OwnerDelivery.Delivery.PK);

			AssertEquals(6m, db1Usage.Usages.Single(x => x.SubCode == "P01").UnitCount);
			AssertEquals(1m, db2Usage.Usages.Single(x => x.SubCode == "USR").UnitCount);
		}

		[TestDate(2024, 7, 1)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenericPriceListFallback()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var thisPeriod = new ZDateTime(TestDateAttribute.Date);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var keyA = new UsageCodeKey("SAT", "P01");
			var priceHeader = BillingTestHelper.CreateValidStlPriceListWithExchangeRates(stdLicCompany, keyA);
			priceHeader.L6_SystemCode = "DEF";
			priceHeader.L6_ValidFrom = thisPeriod;
			var rate = priceHeader.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "USD";
			rate.PHE_Rate = 0.6m;
			rate.PHE_UpliftPercent = 0m;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");
			lic1.Database.LD_Product = "ABC";
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", thisPeriod, lic1.ClientCompany, 5);
			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, thisPeriod.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var dbUsageSet = new DatabaseUsageSet(context, false, null, 0, null, new GenericUsageSetFactory());
			AssertEquals(1, dbUsageSet.DatabaseUsages.Count());
			var db1Usage = dbUsageSet.DatabaseUsages.Single(x => x.Database.PK == lic1.LA_LD);
			AssertNull(db1Usage.PriceHeaderLink);
			AssertEquals(priceHeader.PK, db1Usage.StlPriceList.Header.PK);
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}
	}
}
