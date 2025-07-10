using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(MonthlyUsageBilling))]
	sealed class MonthlyUsageBillingTest : UsageBillingTest
	{
		public void TestBillingSystems()
		{
			var billing = new MonthlyUsageBilling(new BusinessObjectFactory());
			foreach (var billingSystem in new BillingSystemList())
			{
				AssertEquals(1, billing.BillingSystems.Count(x => x.SystemCode == billingSystem.SystemCode));
			}
		}

		public void TestBillingSystemText()
		{
			var billing = new MonthlyUsageBilling(new BusinessObjectFactory());

			billing.BillingSystemsText = BillingConstants.BillingSystem.Fax;
			var enabledList = billing.BillingSystems.Where(s => s.IsEnabled).ToArray();
			AssertEquals(1, enabledList.Length);
			AssertEquals(BillingConstants.BillingSystem.Fax, enabledList[0].SystemCode);

			billing.BillingSystemsText = BillingConstants.BillingSystem.ODM + ',' + BillingConstants.BillingSystem.eBACCA;
			enabledList = billing.BillingSystems.Where(s => s.IsEnabled).ToArray();
			AssertEquals(2, enabledList.Length);
			Assert(enabledList.Any(s => s.SystemCode == BillingConstants.BillingSystem.eBACCA));
			Assert(enabledList.Any(s => s.SystemCode == BillingConstants.BillingSystem.ODM));

			billing.BillingSystemsText = "ALL";
			enabledList = billing.BillingSystems.Where(s => s.IsEnabled).ToArray();
			AssertEquals(billing.BillingSystems.Count, enabledList.Length);
		}

		public void TestGenerateReport()
		{
			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.BillingSystems.Clear();

			var dummyBilling1 = new DummyBillingSystem();
			var dummyBilling2 = new DummyBillingSystem();
			monthlyUsageBilling.BillingSystems.Add(dummyBilling1);
			monthlyUsageBilling.BillingSystems.Add(dummyBilling2);

			AssertEquals("Precondition", true, monthlyUsageBilling.OrganisationPK.IsEmpty);
			foreach (DummyBillingSystem dummyBilling in monthlyUsageBilling.BillingSystems)
			{
				AssertEquals("Precondition", null, dummyBilling.Context_Exposed);
				AssertEquals("Precondition", false, dummyBilling.MethodWasCalled("LoadChargeableUsages"));
			}

			monthlyUsageBilling.GenerateReport(null);
			foreach (DummyBillingSystem dummyBilling in monthlyUsageBilling.BillingSystems)
			{
				AssertEquals(monthlyUsageBilling.Factory, dummyBilling.Context_Exposed.Factory);
				AssertEquals(monthlyUsageBilling.DateTo, dummyBilling.Context_Exposed.DateToInclusive);
				AssertEquals(ZGuid.Empty, dummyBilling.Context_Exposed.OrganisationPK);

				AssertEquals("Method was called", true, dummyBilling.MethodWasCalled("LoadChargeableUsages"));
			}

			monthlyUsageBilling.OrganisationPK = ZGuid.NewZGuid();
			AssertEquals("Precondition", false, monthlyUsageBilling.OrganisationPK.IsEmpty);
			foreach (DummyBillingSystem dummyBilling in monthlyUsageBilling.BillingSystems)
			{
				dummyBilling.ClearTextNotifications();
				AssertEquals("Precondition", false, dummyBilling.MethodWasCalled("LoadChargeableUsages"));
			}

			monthlyUsageBilling.GenerateReport(null);
			foreach (DummyBillingSystem dummyBilling in monthlyUsageBilling.BillingSystems)
			{
				AssertEquals(monthlyUsageBilling.Factory, dummyBilling.Context_Exposed.Factory);
				AssertEquals(monthlyUsageBilling.DateTo, dummyBilling.Context_Exposed.DateToInclusive);
				AssertEquals(monthlyUsageBilling.OrganisationPK, dummyBilling.Context_Exposed.OrganisationPK);
			}
		}

		public void TestGenerateReport_MultipleDeliveries()
		{
			var parentOrg1 = BillingTestHelper.CreateOrganisation(Factory, "PAR");
			var org1 = BillingTestHelper.CreateDependentOrganisation(parentOrg1, "AA1");
			var org2 = BillingTestHelper.CreateDependentOrganisation(parentOrg1, "BBB");

			// For Org1, database 1 is paid by parent, 2 is paid by itself
			BillingTestHelper.SetInvoicing(parentOrg1, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicing(org1, Env.CurrentBranch.PK);
			var licHeader1a = org1.LicCompany.LicHeadersForAllDatabases[0];
			var licHeader1b = BillingTestHelper.CreateAnotherDatabase(licHeader1a, "AA2");
			var delivery1a = org1.LicCompany.InvoiceDeliveries[0];
			var delivery1b = org1.LicCompany.InvoiceDeliveries.AddNew();
			delivery1a.L9_ServerCode = "AA1";
			delivery1b.L9_ServerCode = "AA2";

			// For Org2, system 1 is paid by parent, system 2 is paid itself, system 3 is not billed
			BillingTestHelper.SetInvoicing(org2, Env.CurrentBranch.PK);
			var licHeader2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			var delivery2a = org2.LicCompany.InvoiceDeliveries[0];
			var delivery2b = org2.LicCompany.InvoiceDeliveries.AddNew();
			var delivery2c = org2.LicCompany.InvoiceDeliveries.AddNew();
			delivery2a.L9_SystemCode = "DUM";
			delivery2b.L9_SystemCode = "DM2";
			delivery2c.L9_SystemCode = "DM3";
			delivery2c.L9_IsBilled = false;

			var clientCompany1a = BillingTestHelper.FindOrCreateClientCompany(licHeader1a);
			var clientCompany1b = BillingTestHelper.FindOrCreateClientCompany(licHeader1b);
			var clientCompany2 = BillingTestHelper.FindOrCreateClientCompany(licHeader2);

			var periodStart = new ZDateTime(2010, 10, 01);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, clientCompany1a, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, clientCompany1b, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, clientCompany2, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "DM2", "", periodStart, clientCompany2, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "DM3", "", periodStart, clientCompany2, 13);

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.DateTo = periodStart.AddMonths(1).AddDays(-1);
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem("DUM"));
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem("DM2"));
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem("DM3"));
			AssertEquals("Precondition", 0, monthlyUsageBilling.OrganisationBills.Count);

			monthlyUsageBilling.GenerateReport(null);
			AssertEquals("OrganisationBills created", 4, monthlyUsageBilling.OrganisationBills.Count);

			var bills = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>().ToArray();

			var parentBill = bills.First(s => s.OrganisationPK == parentOrg1.PK);
			var org1Bill = bills.First(s => s.OrganisationPK == org1.PK);
			var org2Bill = bills.First(s => s.OrganisationPK == org2.PK && s.IsBilled);
			var org2NonBill = bills.First(s => s.OrganisationPK == org2.PK && !s.IsBilled);
			AssertEquals("parent pays for", 1, parentBill.SystemBills.Count);
			AssertEquals("org1 pays for", 1, org1Bill.SystemBills.Count);
			AssertEquals("org2 pays for", 1, org2Bill.SystemBills.Count);
			AssertEquals("org2 not billed", 1, org2NonBill.SystemBills.Count);

			var system1 = parentBill.SystemBills[0].SystemUsages.First(s => s.OrganisationPK == org1.PK);
			var system2 = parentBill.SystemBills[0].SystemUsages.First(s => s.OrganisationPK == org2.PK);
			AssertEquals("units", 3 + 7m, system1.Amount + system2.Amount);

			AssertEquals(org1.PK, org1Bill.SystemBills[0].SystemUsages[0].OrganisationPK);
			AssertEquals("units", 5m, org1Bill.SystemBills[0].SystemUsages[0].Amount);

			AssertEquals(org2.PK, org2Bill.SystemBills[0].SystemUsages[0].OrganisationPK);
			AssertEquals("units", 11m, org2Bill.SystemBills[0].SystemUsages[0].Amount);

			AssertEquals(org2.PK, org2NonBill.SystemBills[0].SystemUsages[0].OrganisationPK);
			AssertEquals("units", 13m, org2NonBill.SystemBills[0].SystemUsages[0].Amount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_FlightStats_TRA()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var flightStatsPriceList = stdLicCompany.PriceHeaders.AddNew();
			flightStatsPriceList.L6_PricelistVersion = "FlightStats V1";
			flightStatsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.FlightStats;
			flightStatsPriceList.L6_RX_NKCurrency = "AUD";
			flightStatsPriceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = flightStatsPriceList.Items.AddNew();
			item1.L7_Code = "FMS";
			item1.L7_Price = 1.00m;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = chargeCode;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_UnitBreak = 100;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");

			var priceHeader = licence.Company.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 800);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 600);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 300);
			usage1.U1_Reference1 = "Job#1";
			usage2.U1_Reference1 = "Job#2";
			usage3.U1_Reference1 = "Job#3";

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(Factory);
			monthlyUsageBilling.DateTo = new ZDateTime(2018, 1, 31);
			monthlyUsageBilling.OrganisationPK = licence.Company.Header.PK;

			monthlyUsageBilling.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;
			monthlyUsageBilling.GenerateReport(null);
			var bill = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>().Single();

			AssertEquals((800m + 600m + 300m - 100m), bill.Amount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_FlightStats_TRB()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var flightStatsPriceList = stdLicCompany.PriceHeaders.AddNew();
			flightStatsPriceList.L6_PricelistVersion = "FlightStats V1";
			flightStatsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.FlightStats;
			flightStatsPriceList.L6_RX_NKCurrency = "AUD";
			flightStatsPriceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			for (var idx = 1; idx < 20; idx++)
			{
				var item = flightStatsPriceList.Items.AddNew();
				item.L7_Code = "FMS";
				item.L7_Price = 1.00m * idx;
				item.L7_Order = (ZShort)idx;
				item.L7_ChargeCode = chargeCode;
				item.L7_UnitBreak = 100 * idx;
				item.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			}

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");

			var priceHeader = licence.Company.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 800);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 600);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 300);
			usage1.U1_Reference1 = "Job#1";
			usage2.U1_Reference1 = "Job#2";
			usage3.U1_Reference1 = "Job#3";

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(Factory);
			monthlyUsageBilling.DateTo = new ZDateTime(2018, 1, 31);
			monthlyUsageBilling.OrganisationPK = licence.Company.Header.PK;

			monthlyUsageBilling.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;
			monthlyUsageBilling.GenerateReport(null);
			var bill = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>().Single();

			AssertEquals((800m + 600m + 300m) * 16m, bill.Amount);
		}

		[TestDate(2010, 11, 11)]
		public void TestCreateOrganisationBills_Factories()
		{
			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(organisation1, Env.CurrentBranch.PK);

			var organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			BillingTestHelper.SetInvoicing(organisation2, Env.CurrentBranch.PK);

			var anotherBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			var organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC");
			BillingTestHelper.SetInvoicing(organisation3, anotherBranch1.PK);

			var anotherBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			var organisation4 = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			BillingTestHelper.SetInvoicing(organisation4, anotherBranch2.PK);

			var periodStart = new ZDateTime(2010, 10, 01);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation1.LicCompany.PK, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation2.LicCompany.PK, 22);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation3.LicCompany.PK, 33);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation4.LicCompany.PK, 44);

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem());
			AssertEquals("Precondition", 0, monthlyUsageBilling.OrganisationBills.Count);

			monthlyUsageBilling.GenerateReport(null);
			AssertEquals("OrganisationBills created", 4, monthlyUsageBilling.OrganisationBills.Count);

			// Each branch should have it's own factory due to currency conversion issues and OrganisationBill should be created with related branch factory
			var bills = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>();
			var bill1 = bills.First(x => x.OrganisationPK == organisation1.PK);
			var bill2 = bills.First(x => x.OrganisationPK == organisation2.PK);
			var bill3 = bills.First(x => x.OrganisationPK == organisation3.PK);
			var bill4 = bills.First(x => x.OrganisationPK == organisation4.PK);

			AssertEquals("Same factory", true, bill1.Factory == bill2.Factory);
			AssertEquals("Different factories", true, bill1.Factory != bill3.Factory);
			AssertEquals("Different factories", true, bill1.Factory != bill4.Factory);
			AssertEquals("Different factories", true, bill3.Factory != bill4.Factory);
		}

		[TestDate(2016, 7, 19)]
		public void TestCreateOrganisationBills_DateForExchangeRate()
		{
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var company1 = branch1.Company;
			var company2 = branch2.Company;
			Factory.Save();

			var currentMonth = new ZDateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			var lastMonth = currentMonth.AddMonths(-1);

			var company1CurrentPeriod = periodHelper.SetupSinglePeriod(201607, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company1.PK);
			var company1LastPeriod = periodHelper.SetupSinglePeriod(201606, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company1.PK);

			var company2CurrentPeriod = periodHelper.SetupSinglePeriod(201607, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company2.PK);
			var company2LastPeriod = periodHelper.SetupSinglePeriod(201606, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company2.PK);

			company1LastPeriod.AM_IsSubLedgerClosed = false;
			company2LastPeriod.AM_IsSubLedgerClosed = true;

			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(organisation1, branch1.PK);

			var organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			BillingTestHelper.SetInvoicing(organisation2, branch2.PK);

			var periodStart = new ZDateTime(2016, 6, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation1.LicCompany.PK, 11);
			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem());
			monthlyUsageBilling.DateTo = new ZDateTime(2016, 6, 30);
			monthlyUsageBilling.GenerateReport(null);

			var bills = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>();
			var bill1 = bills.First(x => x.OrganisationPK == organisation1.PK);
			AssertEquals(true, monthlyUsageBilling.IsBackPostAvailable);
			AssertEquals(new ZDateTime(2016, 6, 30), bill1.DateForExchangeRate);

			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation2.LicCompany.PK, 22);
			Factory.Save();
			monthlyUsageBilling.GenerateReport(null);

			bills = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>();
			bill1 = bills.First(x => x.OrganisationPK == organisation1.PK);
			var bill2 = bills.First(x => x.OrganisationPK == organisation2.PK);
			AssertEquals(false, monthlyUsageBilling.IsBackPostAvailable);
			AssertEquals(new ZDateTime(2016, 7, 19), bill1.DateForExchangeRate);
			AssertEquals(new ZDateTime(2016, 7, 19), bill1.DateForExchangeRate);
		}

		public void TestCreateOrganisationBills_NotBillable()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var licBillable = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1", false);
			var licNotBillable = BillingTestHelper.CreateAnotherDatabase(licBillable, "DB2", false);
			licNotBillable.Database.LD_Billable = Licencing.Business.DatabaseBillableFlagList.Codes.No;
			BillingTestHelper.SetInvoicing(licBillable, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, licBillable, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, licNotBillable, 13);
			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(Factory);
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem());
			monthlyUsageBilling.DateTo = periodStart.AddMonths(1).AddDays(-1);
			monthlyUsageBilling.GenerateReport(null);
			var orgBills = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>().ToList();
			AssertEquals(2, orgBills.Count);
			var bill1 = orgBills.Single(x => x.IsBillable);
			var bill2 = orgBills.Single(x => !x.IsBillable);
			AssertNoNotifications(bill1);
		}

		[TestDate(2010, 11, 11)]
		public void TestMinimumAmountToBill_IsTooSmallToBill()
		{
			EDIDataRegistry.Instance.MinimumAmountToBill.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15m);
			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			AssertEquals(15m, monthlyUsageBilling.MinimumAmountToBill);

			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(organisation1, Env.CurrentBranch.PK, "AUD");
			organisation1.CompanyData.OB_IsDebtor = true;

			var organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			BillingTestHelper.SetInvoicing(organisation2, Env.CurrentBranch.PK, "AUD");
			organisation2.CompanyData.OB_IsDebtor = true;

			var periodStart = new ZDateTime(2010, 10, 01);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation1.LicCompany.PK, 14);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, organisation2.LicCompany.PK, 15);

			Factory.Save();

			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem());
			monthlyUsageBilling.GenerateReport(null);
			AssertEquals("OrganisationBills created", 2, monthlyUsageBilling.OrganisationBills.Count);

			var bills = monthlyUsageBilling.OrganisationBills.Cast<OrganisationBill>();
			var bill1 = bills.First(x => x.OrganisationPK == organisation1.PK);
			var bill2 = bills.First(x => x.OrganisationPK == organisation2.PK);

			AssertEquals(true, bill1.IsTooSmallToBill);
			AssertEquals(false, bill2.IsTooSmallToBill);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoices_Partners()
		{
			BillingTestHelper.LoadClientSpecificDocuments();

			var partner1Branch = Factory.NewWithValidTestData<GlbBranch>();
			var partner2Branch = Factory.NewWithValidTestData<GlbBranch>();
			partner1Branch.Company.GC_RX_NKLocalCurrency = "ZAR";
			partner2Branch.Company.GC_RX_NKLocalCurrency = "USD";

			var partner1 = BillingTestHelper.CreateOrganisation(Factory, "PA1");
			partner1.LicCompany.SelfBilling.L4_IsPartner = true;
			partner1.LicCompany.SelfBilling.PartnerEmail = "jim@cargowise.com ; jo@cargowise.com";

			var partner2 = BillingTestHelper.CreateOrganisation(Factory, "PA2");
			partner2.LicCompany.SelfBilling.L4_IsPartner = true;
			partner2.LicCompany.SelfBilling.PartnerEmail = "bill@cargowise.com  betty@cargowise.com";

			var normalClient = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(normalClient, Env.CurrentBranch.PK, "AUD");
			normalClient.CompanyData.OB_IsDebtor = true;

			var partner1Client = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			BillingTestHelper.SetInvoicing(partner1Client, partner1Branch.PK, "ZAR");
			partner1Client.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = partner1.PK;

			var partner2Client = BillingTestHelper.CreateOrganisation(Factory, "CCC");
			BillingTestHelper.SetInvoicing(partner2Client, partner2Branch.PK, "USD");
			partner2Client.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = partner2.PK;

			var periodStart = new ZDateTime(2010, 10, 01);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, normalClient.LicCompany.PK, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, partner1Client.LicCompany.PK, 300);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, partner2Client.LicCompany.PK, 400);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, partner1Branch);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, partner2Branch);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem());
			monthlyUsageBilling.DateTo = periodStart.AddMonths(1).AddDays(-1);
			monthlyUsageBilling.GenerateReport(null);
			AssertEquals("OrganisationBills created", 3, monthlyUsageBilling.OrganisationBills.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			monthlyUsageBilling.CreateInvoices(monthlyUsageBilling.OrganisationBills.ToArray<OrganisationBill>(), null);
			AssertEquals("partner emails", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingMailManager.EmailsCreated.First(s => s.Subject.StartsWith(partner1.OH_Code));
			AssertEquals("subject", partner1.OH_Code + " client billing summaries Oct 2010", email1.Subject);
			AssertEquals("recipient", "jim@cargowise.com", email1.Recipients[0]);
			AssertEquals("recipient", "jo@cargowise.com", email1.Recipients[1]);
			AssertEquals("attachment", 1, email1.Attachments.Count);
			AssertEquals("attachment name", "Billing Summaries.zip", email1.Attachments[0].DisplayName);

			var email2 = Env.OutgoingMailManager.EmailsCreated.First(s => s.Subject.StartsWith(partner2.OH_Code));
			AssertEquals("subject", partner2.OH_Code + " client billing summaries Oct 2010", email2.Subject);
			AssertEquals("recipient", "bill@cargowise.com", email2.Recipients[0]);
			AssertEquals("recipient", "betty@cargowise.com", email2.Recipients[1]);
			AssertEquals("attachment", 1, email2.Attachments.Count);
			AssertEquals("attachment name", "Billing Summaries.zip", email2.Attachments[0].DisplayName);
		}

		[TestDate(2015, 6, 17)]
		public void TestCreateInvoices_BackPost()
		{
			var normalClient = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(normalClient, Env.CurrentBranch.PK, "AUD");
			normalClient.CompanyData.OB_IsDebtor = true;

			var periodStart = new ZDateTime(2015, 5, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, normalClient.LicCompany.PK, 200);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();

			var dateForExchangeRate = ZDateTime.Now;

			var monthlyUsageBilling = new MonthlyUsageBilling(Factory);
			var bill = new OrganisationBill(Factory, Env.CurrentBranch.PK, normalClient.PK, "AUD", dateForExchangeRate);
			var bill2 = new OrganisationBill(Factory, Env.CurrentBranch.PK, normalClient.PK, "AUD", dateForExchangeRate);
			var bill3 = new OrganisationBill(Factory, Env.CurrentBranch.PK, normalClient.PK, "AUD", dateForExchangeRate);
			monthlyUsageBilling.DateTo = periodStart.AddMonths(1).AddDays(-1);
			bill.DateTo = bill2.DateTo = bill3.DateTo = monthlyUsageBilling.DateTo;
			monthlyUsageBilling.OrganisationBills.Add(bill);
			monthlyUsageBilling.OrganisationBills.Add(bill2);
			monthlyUsageBilling.OrganisationBills.Add(bill3);
			monthlyUsageBilling.IsBackPostAllowed = true;
			monthlyUsageBilling.SetIsBackPostAvailableForTest(true);

			monthlyUsageBilling.CreateInvoices(new[] { bill }, null);
			var invoice = Factory.Load<ARInvoice>(bill.InvoicePkForThisMonth);
			AssertEquals("back posted", new ZDateTime(2015, 5, 31), invoice.AH_InvoiceDate);
			AssertEquals("back posted", new ZDateTime(2015, 5, 31), invoice.AH_PostDate);

			monthlyUsageBilling.IsBackPostAllowed = false;
			monthlyUsageBilling.CreateInvoices(new[] { bill2 }, null);
			invoice = Factory.Load<ARInvoice>(bill2.InvoicePkForThisMonth);
			AssertEquals("not back posted - not allowed", TestDateAttribute.Date, invoice.AH_InvoiceDate);
			AssertEquals("not back posted - not allowed", TestDateAttribute.Date, invoice.AH_PostDate);

			monthlyUsageBilling.IsBackPostAllowed = true;
			monthlyUsageBilling.SetIsBackPostAvailableForTest(false);
			monthlyUsageBilling.CreateInvoices(new[] { bill3 }, null);
			invoice = Factory.Load<ARInvoice>(bill3.InvoicePkForThisMonth);
			AssertEquals("not back posted - not available", TestDateAttribute.Date, invoice.AH_InvoiceDate);
			AssertEquals("not back posted - not available", TestDateAttribute.Date, invoice.AH_PostDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoices_UsesSingleInstanceOfUSSalesTaxCalculator()
		{
			BillingTestHelper.LoadClientSpecificDocuments();

			var branchZAR = Factory.NewWithValidTestData<GlbBranch>();
			var branchUSD = Factory.NewWithValidTestData<GlbBranch>();
			branchZAR.Company.GC_RX_NKLocalCurrency = "ZAR";
			branchUSD.Company.GC_RX_NKLocalCurrency = "USD";

			var clientAUD = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(clientAUD, Env.CurrentBranch.PK, "AUD");
			clientAUD.CompanyData.OB_IsDebtor = true;

			var clientZAR = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			BillingTestHelper.SetInvoicing(clientZAR, branchZAR.PK, "ZAR");
			clientZAR.CompanyData.OB_IsDebtor = true;

			var clientUSD = BillingTestHelper.CreateOrganisation(Factory, "CCC");
			BillingTestHelper.SetInvoicing(clientUSD, branchUSD.PK, "USD");
			clientUSD.CompanyData.OB_IsDebtor = true;

			var periodStart = new ZDateTime(2010, 10, 01);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, clientAUD.LicCompany.PK, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, clientZAR.LicCompany.PK, 300);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, clientUSD.LicCompany.PK, 400);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, branchZAR);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, branchUSD);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();

			var monthlyUsageBilling = new MonthlyUsageBilling(new BusinessObjectFactory());
			monthlyUsageBilling.BillingSystems.Clear();
			monthlyUsageBilling.BillingSystems.Add(new DummyBillingSystem());
			monthlyUsageBilling.DateTo = periodStart.AddMonths(1).AddDays(-1);
			monthlyUsageBilling.GenerateReport(null);
			AssertEquals("OrganisationBills created", 3, monthlyUsageBilling.OrganisationBills.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			mockCalculator.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculator.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			var calculationResult = new CalculationResult(5m);
			mockCalculator.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculator.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			var mockCalculatorFactory = new Mock<IUSSalesTaxCalculatorFactory>();
			mockCalculatorFactory.Setup(x => x.Get()).Returns(mockCalculator.Object);

			var factoryList = (System.Collections.ArrayList)CargoWise.Application.ObjectFactory.Get("IUSSalesTaxCalculator_ClientSpecific");
			var oldFactory = factoryList[0];
			factoryList.Clear();
			factoryList.Add(mockCalculatorFactory.Object);
			try
			{
				var beforeCount = Factory.GetDatabaseCount(typeof(AccTransactionHeader));
				AssertEquals("Precondition: no invoices", 0, beforeCount);

				monthlyUsageBilling.CreateInvoices(monthlyUsageBilling.OrganisationBills.ToArray<OrganisationBill>(), null);

				var count = Factory.CreateNewFactory().GetDatabaseCount(typeof(AccTransactionHeader));
				AssertEquals("Three invoices should be created", 3, count);

				mockCalculatorFactory.Verify(x => x.Get(), Times.Once(), "One USSalesTaxCalculator should be created, and re-used for all invoices across any branch");
				mockCalculator.Verify(x => x.SetSalesTaxLineItem(It.IsAny<InvoicingBase>(), It.IsAny<decimal>()), Times.Exactly(3), "Sales tax line items should be added for each invoice");
				mockCalculator.Verify(x => x.Dispose(), Times.Once(), "Dispose should be called once, after all invoices are processed");
			}
			finally
			{
				factoryList.Clear();
				factoryList.Add(oldFactory);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}
	}
}
