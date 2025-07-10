using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(GlobalContainerTrackingBill))]
	public class GlobalContainerTrackingBillTest : TransactionalSystemBillTestCase<GlobalContainerTrackingBill>
	{
		#region Summary Section

		public void TestGetGeneralSummarySections()
		{
			SetupPriceList();

			var org1 = SetupOrg("DDDAAASYD");

			var usageList = new List<GlobalContainerTrackingUsage>();
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org1), new ZDateTime(2015, 7, 31)) { TransactionDescription = "5001 - 25000 unique container tracked", TransactionCount = 5010, TransactionPrice = 0.65m, CompanyUsageCount = 5010, DatabaseUsageCount = 5010 });

			var bill = new GlobalContainerTrackingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org1.PK);
			AssertEquals(1, sections.Length);
			AssertEquals("Container Automation Usage", sections[0].Header.MainDescription);
			AssertSummarySectionLine(sections[0].Lines[0], "Container Automation", "", "", "");
			AssertSummarySectionLine(sections[0].Lines[1], "    5001 - 25000 unique container tracked", "5010", "0.65", "3,256.50");
		}

		public void TestGetGeneralSummarySections_SharedDatabase()
		{
			SetupPriceList();

			var org1 = SetupOrg("DDDAAASYD");
			var org2 = SetupOrg("DDDBBBSYD");

			var usageList = new List<GlobalContainerTrackingUsage>();
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org1), new ZDateTime(2015, 7, 31)) { TransactionDescription = "1001 - 2000 unique container tracked", TransactionCount = 800, TransactionPrice = 0.85m, CompanyUsageCount = 800, DatabaseUsageCount = 2000 });
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org2), new ZDateTime(2015, 7, 31)) { TransactionDescription = "1001 - 2000 unique container tracked", TransactionCount = 1200, TransactionPrice = 0.85m, CompanyUsageCount = 1200, DatabaseUsageCount = 2000 });

			var bill = new GlobalContainerTrackingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org1.PK);
			AssertEquals(1, sections.Length);
			AssertEquals("Usage Charge Share", sections[0].Header.AdditionalDescription);
			AssertEquals("Container Automation Usage", sections[0].Header.MainDescription);
			AssertSummarySectionLine(sections[0].Lines[0], "Container Automation", "", "", "", "");
			AssertSummarySectionLine(sections[0].Lines[1], "    1001 - 2000 unique container tracked", "800", "0.85", "680.00", "800 / 2000");

			sections = bill.GetGeneralSummarySections(org2.PK);
			AssertEquals(1, sections.Length);
			AssertEquals("Usage Charge Share", sections[0].Header.AdditionalDescription);
			AssertEquals("Container Automation Usage", sections[0].Header.MainDescription);
			AssertSummarySectionLine(sections[0].Lines[0], "Container Automation", "", "", "", "");
			AssertSummarySectionLine(sections[0].Lines[1], "    1001 - 2000 unique container tracked", "1200", "0.85", "1,020.00", "1200 / 2000");
		}

		void AssertSummarySectionLine(SummaryLine line, string mainDescription, string unitCount, string unitPrice, string amount, string additionalDescription = "")
		{
			CombineAssertions(() =>
				{
					AssertEquals("Main desciption", mainDescription, line.MainDescription);
					AssertEquals("Unit count", unitCount, line.UnitCount);
					AssertEquals("Unit price", unitPrice, line.UnitPrice);
					AssertEquals("Amount", amount, line.Amount);
					AssertEquals("Additional description", additionalDescription, line.AdditionalDescription);
				});
		}

		#endregion

		#region Invoice

		public void TestCreateInvoiceLines()
		{
			SetupPriceList();

			var org = SetupOrg("DDDAAASYD");
			var usageList = new List<GlobalContainerTrackingUsage>();
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org), new ZDateTime(2015, 7, 31)) { TransactionDescription = "Container Automation (5001 - 25000 unique container tracked)", TransactionCount = 5010, TransactionPrice = 0.65m, CompanyUsageCount = 5010, DatabaseUsageCount = 5010 });

			var bill = new GlobalContainerTrackingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			CombineAssertions(() =>
			{
				AssertEquals(1, lines.Count);
				AssertEquals("Container Automation Usage", lines[0].Description);
				AssertEquals(3256.50m, lines[0].Amount);
			});
		}

		public void TestCreateInvoiceLines_SharedDatabase()
		{
			SetupPriceList();

			var payingOrg = SetupOrg("DDDYYYSYD");

			var org1 = SetupOrg("DDDAAASYD");
			org1.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;

			var org2 = SetupOrg("DDDBBBSYD");
			org2.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;

			var usageList = new List<GlobalContainerTrackingUsage>();
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org1), new ZDateTime(2015, 7, 31)) { TransactionDescription = "Container Automation (1001 - 2000 unique container tracked)", TransactionCount = 800, TransactionPrice = 0.85m, CompanyUsageCount = 800, DatabaseUsageCount = 2000 });
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org2), new ZDateTime(2015, 7, 31)) { TransactionDescription = "Container Automation (1001 - 2000 unique container tracked)", TransactionCount = 1200, TransactionPrice = 0.85m, CompanyUsageCount = 1200, DatabaseUsageCount = 2000 });

			var bill = new GlobalContainerTrackingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			CombineAssertions(() =>
			{
				AssertEquals(1, lines.Count);
				AssertEquals("Container Automation Usage", lines[0].Description);
				AssertEquals(1700m, lines[0].Amount);
			});
		}

		public void TestCreateInvoiceLines_SharedDatabaseDifferentTaxGroup()
		{
			SetupPriceList();

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();

			var payingOrg = SetupOrg("DDDYYYSYD");

			var org1 = SetupOrg("DDDAAASYD");
			org1.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;
			org1.LicCompany.InvoiceDeliveries[0].L9_AT_TaxId = taxRate1.PK;

			var org2 = SetupOrg("DDDBBBSYD");
			org2.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;
			org1.LicCompany.InvoiceDeliveries[0].L9_AT_TaxId = taxRate2.PK;

			var usageList = new List<GlobalContainerTrackingUsage>();
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org1), new ZDateTime(2015, 7, 31)) { TransactionDescription = "Container Automation (1001 - 2000 unique container tracked)", TransactionCount = 800, TransactionPrice = 0.85m, CompanyUsageCount = 800, DatabaseUsageCount = 2000 });
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org2), new ZDateTime(2015, 7, 31)) { TransactionDescription = "Container Automation (1001 - 2000 unique container tracked)", TransactionCount = 1200, TransactionPrice = 0.85m, CompanyUsageCount = 1200, DatabaseUsageCount = 2000 });

			var bill = new GlobalContainerTrackingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			CombineAssertions(() =>
			{
				AssertEquals(2, lines.Count);
				AssertEquals("Container Automation Usage", lines[0].Description);
				AssertEquals(680m, lines[0].Amount);
				AssertEquals("Container Automation Usage", lines[1].Description);
				AssertEquals(1020m, lines[1].Amount);
			});
		}

		public void TestCreateInvoiceLines_DifferentDatabase()
		{
			SetupPriceList();

			var payingOrg = SetupOrg("DDDYYYSYD");

			var org1 = SetupOrg("DDDAAASYD");
			org1.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;

			var org2 = SetupOrg("DDDBBBMEL");
			org2.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;

			var usageList = new List<GlobalContainerTrackingUsage>();
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org1, "SYD"), new ZDateTime(2015, 7, 31)) { TransactionDescription = "Container Automation (251 - 1000 unique container tracked)", TransactionCount = 800, TransactionPrice = 0.90m, CompanyUsageCount = 800, DatabaseUsageCount = 800 });
			usageList.Add(new GlobalContainerTrackingUsage(Factory, new UsingParty(org2, "MEL"), new ZDateTime(2015, 7, 31)) { TransactionDescription = "Container Automation (1001 - 2000 unique container tracked)", TransactionCount = 1200, TransactionPrice = 0.85m, CompanyUsageCount = 1200, DatabaseUsageCount = 1200 });

			var bill = new GlobalContainerTrackingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			CombineAssertions(() =>
			{
				AssertEquals(1, lines.Count);
				AssertEquals("Container Automation Usage", lines[0].Description);
				AssertEquals(1740m, lines[0].Amount);
			});
		}

		#endregion

		EDIOrgHeader SetupOrg(string orgCode)
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;

			var enterpriseCode = orgCode.Substring(0, 3);
			var companyCode = orgCode.Substring(3, 3);
			var databaseCode = orgCode.Substring(6, 3);

			var licEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			if (licEnterprise == null)
			{
				licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
				licEnterprise.LE_EnterpriseCode = enterpriseCode;
				licEnterprise.LE_OH = org.PK;
			}

			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_OH = org.PK;
			licCompany.LC_CompanyCode = companyCode;

			var licDatabaseQuery = new ZQuery(LicenceDatabaseSchema.LD_ServerCode, databaseCode);
			licDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LE, licEnterprise.PK);
			var licDatabase = Factory.LoadTop1<LicenceDatabase>(licDatabaseQuery);
			if (licDatabase == null)
			{
				licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
				licDatabase.LD_ServerCode = databaseCode;
				licDatabase.LD_LE = licEnterprise.PK;
			}

			var clientCompany = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany.LCC_Code = companyCode;
			clientCompany.LCC_LD = licDatabase.PK;
			clientCompany.LCC_OH = org.PK;

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		void SetupPriceList()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var ctrPriceList = stdLicCompany.PriceHeaders.AddNew();
			ctrPriceList.L6_PricelistVersion = "V1";
			ctrPriceList.L6_SystemCode = BillingConstants.BillingSystem.GlobalContainerTracking;
			ctrPriceList.L6_RX_NKCurrency = "USD";
			ctrPriceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.95m);
			priceItem1.L7_Description = "   1-250";
			priceItem1.L7_UnitBreak = 0;
			priceItem1.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem2 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.90m);
			priceItem2.L7_Description = "   251 - 1000";
			priceItem2.L7_UnitBreak = 250;
			priceItem2.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem3 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.85m);
			priceItem3.L7_Description = "   1001 - 2000";
			priceItem3.L7_UnitBreak = 1000;
			priceItem3.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem4 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.80m);
			priceItem4.L7_Description = "   2001 - 3000";
			priceItem4.L7_UnitBreak = 2000;
			priceItem4.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem5 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.75m);
			priceItem5.L7_Description = "   3001 - 4000";
			priceItem5.L7_UnitBreak = 3000;
			priceItem5.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem6 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.70m);
			priceItem6.L7_Description = "   4001 - 5000";
			priceItem6.L7_UnitBreak = 4000;
			priceItem6.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem7 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.65m);
			priceItem7.L7_Description = "   5001 - 25000";
			priceItem7.L7_UnitBreak = 5000;
			priceItem7.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem8 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.60m);
			priceItem8.L7_Description = "   25001 - 50000";
			priceItem8.L7_UnitBreak = 25000;
			priceItem8.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem9 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.55m);
			priceItem9.L7_Description = "   50001 - 100000";
			priceItem9.L7_UnitBreak = 50000;
			priceItem9.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem10 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.50m);
			priceItem10.L7_Description = "   More than 100000";
			priceItem10.L7_UnitBreak = 100000;
			priceItem10.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);
		}

		protected override GlobalContainerTrackingBill GetNewSystemBill()
		{
			return new GlobalContainerTrackingBill(Factory);
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
