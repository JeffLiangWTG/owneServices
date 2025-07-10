using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Fee.Test
{
	public class FeeBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			FeeBillingSystem feeBilling = new FeeBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.Fee, feeBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 10m);
			Factory.Save();

			FeeBillingSystem feeBilling = new FeeBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));
			AssertEquals(true, feeBilling.LoadSystemBills(context).First() is FeeBill);
		}

		public void TestLoadRawUsage()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			FeeBillingSystem feeBilling = new FeeBillingSystem();
			var licence = organisation.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 12, 1), organisation.PK, licence.ClientCompany.PK, ZGuid.Empty, licence.Database.PK);
			FeeRawUsage rawUsage = feeBilling.LoadOdplRawUsage(context) as FeeRawUsage;
			AssertNotNull("Raw usage", rawUsage);
			AssertEquals(Factory, rawUsage.Factory);
			AssertEquals(organisation.OH_Code, rawUsage.OrgCode);
			AssertEquals(new ZDateTime(2010, 12, 1), rawUsage.PeriodStart);

			string expectedCsvResult =
@"""Product Fees"",""Amount""
";

			var builder = new ZStringBuilder();
			feeBilling.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			feeBilling.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"", writer.ToString());
		}

		public void TestLoadUsages()
		{
			EDIOrgHeader organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 10m);

			EDIOrgHeader organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			EDIOrgHeader childOrganisation12 = BillingTestHelper.CreateDependentOrganisation(organisation2, "BB1");
			BillingTestHelper.CreateLicenceFee(childOrganisation12.LicCompany, "BBB", 10m);

			EDIOrgHeader organisationWithoutFees = BillingTestHelper.CreateOrganisation(Factory, "CCC");

			EDIOrgHeader organisationRemit = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			var fee = BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 10m);
			fee.L8_OH_RemitToOrg = organisationRemit.PK;

			Factory.Save();

			FeeBillingSystem feeBilling = new FeeBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			IEnumerable<FeeBill> bills = feeBilling.LoadSystemBills(context).Cast<FeeBill>();
			AssertEquals("Three bills loaded", 3, bills.Count());
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisation1.PK && x.Amount == 20m));
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisation2.PK && x.Amount == 10m));
			AssertEquals(false, bills.Any(x => x.OrganisationPK == organisationWithoutFees.PK));
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisationRemit.PK && x.Amount == -10m));

			foreach (FeeBill feeBill in bills)
			{
				foreach (SystemUsage systemUsage in feeBill.SystemUsages)
				{
					AssertEquals(true, systemUsage is FeeSystemUsage);
				}
			}

			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31), organisation1.PK);
			bills = feeBilling.LoadSystemBills(context).Cast<FeeBill>();
			AssertEquals("Bill for Context.Organisation loaded", 2, bills.Count());
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisation1.PK && x.Amount == 20m));
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisationRemit.PK && x.Amount == -10m));

			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31), organisation2.PK);
			bills = feeBilling.LoadSystemBills(context).Cast<FeeBill>();
			AssertEquals("Bill for Context.Organisation loaded", 1, bills.Count());
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisation2.PK && x.Amount == 10m));

			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31), organisationRemit.PK);
			bills = feeBilling.LoadSystemBills(context).Cast<FeeBill>();
			AssertEquals("Bill for Context.Organisation loaded", 2, bills.Count());
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisationRemit.PK && x.Amount == -10m));
			AssertEquals(true, bills.Any(x => x.OrganisationPK == organisation1.PK && x.Amount == 10m));
		}

		public void TestLoadUsages_RenewalMonthsAndCurrencies()
		{
			EDIOrgHeader organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var fee1 = BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 10m);
			var fee2NoStartDate = BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 20m);
			var fee3 = BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 30m);
			var fee4NotDue = BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 40m);
			var fee5 = BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 50m);

			fee2NoStartDate.L8_RenewalMonths = 2;
			fee3.L8_RenewalMonths = 3;
			fee4NotDue.L8_RenewalMonths = 4;

			fee3.L8_StartDate = new ZDateTime(2010, 7, 1);
			fee4NotDue.L8_StartDate = new ZDateTime(2010, 7, 1);
			fee5.L8_RX_NKCurrency = "USD";
			Factory.Save();

			FeeBillingSystem feeBilling = new FeeBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			FeeBill[] bills = feeBilling.LoadSystemBills(context).Cast<FeeBill>().OrderBy(s => s.CurrencyCode).ToArray();
			AssertEquals("one bill per currency", 2, bills.Length);
			AssertEquals("AUD", bills[0].CurrencyCode);
			AssertEquals("USD", bills[1].CurrencyCode);
			AssertEquals("SystemUsage count", 1, bills[0].SystemUsages.Count);
			AssertEquals("SystemUsage count", 1, bills[1].SystemUsages.Count);
			FeeSystemUsage usageAUD = (FeeSystemUsage)bills[0].SystemUsages[0];
			FeeSystemUsage usageUSD = (FeeSystemUsage)bills[1].SystemUsages[0];
			ClientLicenceFee[] feesAUD = usageAUD.Fees.ToArray();
			ClientLicenceFee[] feesUSD = usageUSD.Fees.ToArray();
			AssertEquals("fee1 + fee3", 2, feesAUD.Length);
			AssertEquals("fee5", 1, feesUSD.Length);
			AssertNotNull(feesAUD.First(s => s.PK == fee1.PK));
			AssertNotNull(feesAUD.First(s => s.PK == fee3.PK));
			AssertNotNull(feesUSD.First(s => s.PK == fee5.PK));
		}

		public void TestLoadUsages_AlreadyInvoiced()
		{
			EDIOrgHeader organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.CreateLicenceFee(organisation1.LicCompany, "AAA", 100m);

			Factory.Save();

			FeeBillingSystem feeBilling = new FeeBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			FeeBill[] bills = feeBilling.LoadSystemBills(context).Cast<FeeBill>().ToArray();
			AssertEquals("bills loaded", 1, bills.Length);
			FeeBill feeBill = bills[0];
			AssertEquals("systems usages", 1, feeBill.SystemUsages.Count);
			AssertEquals("no ChargeableUsagePKs", 0, feeBill.SystemUsages[0].ChargeableUsagePKs.Count);

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();
			feeBill.OnInvoiceFactorySaving(invoice);
			invoiceFactory.Save();

			feeBilling = new FeeBillingSystem();
			bills = feeBilling.LoadSystemBills(context).Cast<FeeBill>().ToArray();
			AssertEquals("bills loaded", 1, bills.Length);
			feeBill = bills[0];
			AssertEquals("systems usages", 1, feeBill.SystemUsages.Count);
			AssertEquals("existing ChargeableUsagePKs", 1, feeBill.SystemUsages[0].ChargeableUsagePKs.Count);
		}

		public void TestBuildFeesDueQuery()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			AssertBuildFeesDueQuery("AAA", ProductTypes.Codes.CargoWiseOne);
			AssertBuildFeesDueQuery("BBB", ProductTypes.Codes.CargoWiseNext);
			AssertBuildFeesDueQuery("CCC", ProductTypes.Codes.CargoWise);
		}

		void AssertBuildFeesDueQuery(string entCode, string dbProduct)
		{
			var lic = BillingTestHelper.CreateLicence(Factory, entCode);
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			lic.Database.LD_Product = dbProduct;

			var priceLink = lic.Database.PriceHeaderLinks.AddNew();
			var priceHeader = BillingTestHelper.CreateStlPriceList(lic.Company, "USR");
			priceLink.PHL_LD = lic.LA_LD;
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = ZDateTime.Today.AddMonths(-12);

			var feeNoDb = lic.Company.Fees.AddNew();
			feeNoDb.L8_Amount = 100;
			feeNoDb.L8_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			feeNoDb.L8_RX_NKCurrency = "AUD";
			feeNoDb.L8_SystemCode = BillingConstants.BillingSystem.ODM;
			feeNoDb.L8_Type = "ESV";

			var feeDb = lic.Company.Fees.AddNew();
			feeDb.L8_Amount = 99;
			feeDb.L8_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			feeDb.L8_RX_NKCurrency = "AUD";
			feeDb.L8_SystemCode = BillingConstants.BillingSystem.ODM;
			feeDb.L8_Type = "ESV";
			feeDb.L8_LD = lic.LA_LD;

			Factory.Save();

			var periodStart = BillingTestHelper.MonthToday;

			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var stlDatabaseActiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, true));
			var odplDatabaseActiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, false));

			lic.Database.LD_IsActive = false;
			Factory.Save();

			var stlDatabaseInactiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, true));
			var odplDatabaseInactiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, false));

			lic.Database.LD_Product = Licencing.Business.ProductTypes.Codes.ProductivityWise;
			Factory.Save();

			var stlDatabaseInactiveFeesPRW = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, true));
			var odplDatabaseInactiveFeesPRW = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, false));

			CombineAssertions(() =>
			{
				AssertCollectionContains("fee not linked to DB for company with active STL database is an STL only fee", feeNoDb, stlDatabaseActiveFees);
				AssertCollectionNotContains("fee not linked to DB for company with active STL database is NOT an ODPL fee", feeNoDb, odplDatabaseActiveFees);
				AssertCollectionContains("fee not linked to DB for company with no-active STL database is an STL only fee", feeNoDb, stlDatabaseInactiveFees);
				AssertCollectionContains("fee not linked to DB for company with no-active STL database is an STL only fee", feeNoDb, stlDatabaseInactiveFeesPRW);
				AssertCollectionNotContains("fee not linked to DB for company with no-active STL database is an STL fee", feeNoDb, odplDatabaseInactiveFees);
				AssertCollectionNotContains("fee not linked to DB for company with no-active STL database is an STL fee", feeNoDb, odplDatabaseInactiveFeesPRW);

				AssertCollectionContains("fee linked to DB for company with active STL database is an STL only fee", feeDb, stlDatabaseActiveFees);
				AssertCollectionNotContains("fee linked to DB for company with active STL database is NOT an ODPL fee", feeDb, odplDatabaseActiveFees);
				AssertCollectionContains("fee linked to inactive STL DB is an STL fee", feeDb, stlDatabaseInactiveFees);
				AssertCollectionContains("fee linked to inactive STL DB is an STL fee", feeDb, stlDatabaseInactiveFeesPRW);
				AssertCollectionNotContains("fee linked to inactive STL DB is not an ODPL fee", feeDb, odplDatabaseInactiveFees);
				AssertCollectionNotContains("fee linked to inactive STL DB is not an ODPL fee", feeDb, odplDatabaseInactiveFeesPRW);
			});
		}

		public void TestBuildFeesDueQuery_NoDatabase()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "AAA", "COM");

			var feeNoDb = licCompany.Fees.AddNew();
			feeNoDb.L8_Amount = 100;
			feeNoDb.L8_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			feeNoDb.L8_RX_NKCurrency = "AUD";
			feeNoDb.L8_SystemCode = BillingConstants.BillingSystem.ODM;
			feeNoDb.L8_Type = "ESV";

			Factory.Save();

			var periodStart = BillingTestHelper.MonthToday;

			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var stlDatabaseActiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, true));
			var odplDatabaseActiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, false));

			CombineAssertions(() =>
			{
				AssertCollectionContains("fee not linked to DB for company with no CW1 is an STL fee", feeNoDb, stlDatabaseActiveFees);
				AssertCollectionNotContains("fee not linked to DB for company with no CW1 is NOT an ODPL fee", feeNoDb, odplDatabaseActiveFees);
			});
		}

		public void TestBuildFeesDueQuery_FeeHasNoDatabase_CompanyHasBothSTLandODPL()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var licSTL = BillingTestHelper.CreateLicence(Factory, "AAA", "COM", "STL", createClientCompany: false);
			var licODM = BillingTestHelper.CreateAnotherDatabase(licSTL.Company, "ODM", createClientCompany: false);
			licSTL.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			licODM.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;

			var licCompany = licSTL.Company;

			var feeNoDb = licCompany.Fees.AddNew();
			feeNoDb.L8_Amount = 100;
			feeNoDb.L8_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			feeNoDb.L8_RX_NKCurrency = "AUD";
			feeNoDb.L8_SystemCode = BillingConstants.BillingSystem.ODM;
			feeNoDb.L8_Type = "ESV";

			Factory.Save();

			var periodStart = BillingTestHelper.MonthToday;

			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var stlActiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, true));
			var odplActiveFees = Factory.Load<ClientLicenceFee>(FeeBillingSystem.BuildFeesDueQuery(periodStart, context, false, false));

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("fee not linked to DB for company with both STL and ODPL CW1 is NOT an odpl fee", feeNoDb, stlActiveFees);
				AssertCollectionContains("fee not linked to DB for company with both STL and ODPL CW1 is an ODPL fee", feeNoDb, odplActiveFees);
			});
		}
	}
}
