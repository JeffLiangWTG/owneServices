using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ClientMappingBill))]
	public class ClientMappingBillTest : TransactionalSystemBillTestCase<ClientMappingBill>
	{
		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList();
			var user = new UsingParty(org);
			var usageList = new List<ClientMappingUsage>();
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 2, 1)) { TransactionCount = 300, SubCode = "Interface 1", Reference3 = "Order Line" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 3, 1)) { TransactionCount = 500, SubCode = "Interface 1", Reference3 = "Order Line" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 3, 1)) { TransactionCount = 1500, SubCode = "Interface 2", Reference3 = "Per Message" });

			var bill = new ClientMappingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);

			AssertEquals(1, sections.Length);
			var section1 = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("eHub Interfaces - Transactional Fee Usage", section1.Header.MainDescription);
				AssertEquals("PSQ 2001 - Interface 1 Feb 2015", section1.Lines[0].MainDescription);
				AssertEquals("PSQ 2001 - Interface 1 Mar 2015", section1.Lines[1].MainDescription);

				AssertEquals("PSQ 2002 - Interface 2 Mar 2015", section1.Lines[2].MainDescription);

				AssertEquals("30.00", section1.Lines[0].Amount);
				AssertEquals("50.00", section1.Lines[1].Amount);
				AssertEquals("100.00", section1.Lines[2].Amount);
			});
		}

		public void TestGetGeneralSummarySections_CombinedByDatabase()
		{
			var parentOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			parentOrg.OH_Code = "DDDDDDSYD";

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = parentOrg.PK;

			var database = enterprise.Databases.AddNew();
			database.LD_ServerCode = "PRD";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;

			var parentCompany = enterprise.Companies.AddNew();
			parentCompany.LC_CompanyCode = "DDD";
			parentCompany.LC_OH = parentOrg.PK;
			parentCompany.LC_LE = enterprise.PK;

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "DDDAAASYD";

			var company1 = enterprise.Companies.AddNew();
			company1.LC_CompanyCode = "AAA";
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise.PK;

			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LD = database.PK;
			licence1.LA_LC = company1.PK;

			var invoiceDelivery1 = company1.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery1.L9_OH_InvoiceTo = parentOrg.PK;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "DDDBBBMEL";

			var company2 = enterprise.Companies.AddNew();
			company2.LC_CompanyCode = "BBB";
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise.PK;

			var licence2 = Factory.New<LicenceHeader>();
			licence2.LA_LD = database.PK;
			licence2.LA_LC = company2.PK;

			var invoiceDelivery2 = company2.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = parentOrg.PK;

			var priceList = parentCompany.PriceHeaders.AddNew();
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var priceItem1 = priceList.Items.AddNew();
			priceItem1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem1.L7_Description = "PSQ 2001 - Interface 1";
			priceItem1.L7_Ref4 = "Interface 1";
			priceItem1.L7_Price = 0.10m;
			priceItem1.L7_UnitBreak = 400;

			var priceItem2 = priceList.Items.AddNew();
			priceItem2.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem2.L7_Description = "PSQ 2046 - Interface 2";
			priceItem2.L7_Ref4 = "Interface 2";
			priceItem2.L7_Price = 0.20m;
			priceItem2.L7_UnitBreak = 1000;

			Factory.Save();

			var user1a = new UsingParty(org1, "PRD");
			var user1b = new UsingParty(org1, "PR2");
			var user2 = new UsingParty(org2, "PRD");
			var usageList = new List<ClientMappingUsage>();

			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user1a, new ZDateTime(2015, 1, 1)) { TransactionCount = 200, SubCode = "Interface 1", Reference3 = "Order Line" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user1b, new ZDateTime(2015, 1, 1)) { TransactionCount = 30, SubCode = "Interface 1", Reference3 = "Order Line" });

			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user2, new ZDateTime(2015, 2, 1)) { TransactionCount = 1000, SubCode = "Interface 1", Reference3 = "Order Line" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user2, new ZDateTime(2015, 3, 1)) { TransactionCount = 2000, SubCode = "Interface 2", Reference3 = "Per Message" });

			var combinedUsage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user1a, new ZDateTime(2015, 3, 1)) { TransactionCount = 800, SubCode = "Interface 1", Reference3 = "Order Line" };
			usageList.Add(combinedUsage);

			var bill = new ClientMappingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org1.PK);
			AssertEquals(1, sections.Length);
			var section1 = sections[0];
			var lines = section1.Lines.Cast<SummaryLine>().Where(x => !x.MainDescription.IsEmpty).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("eHub Interfaces - Transactional Fee Usage", section1.Header.MainDescription);

				AssertSummarySectionLine(lines[0], "PSQ 2001 - Interface 1 (DDD-AAA-PR2) Jan 2015", "400", "30", "0", "0.00");
				AssertSummarySectionLine(lines[1], "PSQ 2001 - Interface 1 (DDD-AAA-PRD) Jan 2015", "400", "200", "0", "0.00");
				AssertSummarySectionLine(lines[2], "PSQ 2001 - Interface 1 (DDD-AAA-PRD) Mar 2015", "400", "800", "400", "40.00");
			});
		}

		public void TestGetGeneralSummarySections_CombinedByInterfaceGroup()
		{
			var parentOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			parentOrg.OH_Code = "DDDDDDSYD";

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = parentOrg.PK;

			var database = enterprise.Databases.AddNew();
			database.LD_ServerCode = "PRD";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;

			var parentCompany = enterprise.Companies.AddNew();
			parentCompany.LC_CompanyCode = "DDD";
			parentCompany.LC_OH = parentOrg.PK;
			parentCompany.LC_LE = enterprise.PK;

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "DDDAAASYD";

			var company1 = enterprise.Companies.AddNew();
			company1.LC_CompanyCode = "AAA";
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise.PK;

			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LD = database.PK;
			licence1.LA_LC = company1.PK;

			var invoiceDelivery1 = company1.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery1.L9_OH_InvoiceTo = parentOrg.PK;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "DDDBBBMEL";

			var company2 = enterprise.Companies.AddNew();
			company2.LC_CompanyCode = "BBB";
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise.PK;

			var licence2 = Factory.New<LicenceHeader>();
			licence2.LA_LD = database.PK;
			licence2.LA_LC = company2.PK;

			var invoiceDelivery2 = company2.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = parentOrg.PK;

			var priceList = parentCompany.PriceHeaders.AddNew();
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var parentPriceItem = priceList.Items.AddNew();
			parentPriceItem.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			parentPriceItem.L7_Description = "Interface Group 1";
			parentPriceItem.L7_Ref4 = "G01";
			parentPriceItem.L7_Price = 0.10m;
			parentPriceItem.L7_UnitBreak = 400;

			var priceItem1 = priceList.Items.AddNew();
			priceItem1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem1.L7_Description = "PSQ 2001 - Interface 1";
			priceItem1.L7_Ref4 = "Interface 1";
			priceItem1.L7_ParentCode = "G01";

			var priceItem2 = priceList.Items.AddNew();
			priceItem2.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem2.L7_Description = "PSQ 2046 - Interface 2";
			priceItem2.L7_Ref4 = "Interface 2";
			priceItem2.L7_ParentCode = "G01";

			Factory.Save();

			var user1 = new UsingParty(org1, "PRD");
			var usageList = new List<ClientMappingUsage>();

			var combinedUsage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user1, new ZDateTime(2015, 3, 1)) { TransactionCount = 800, SubCode = "G01", Reference3 = "Order Line" };
			usageList.Add(combinedUsage);

			var bill = new ClientMappingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org1.PK);
			AssertEquals(1, sections.Length);
			var section1 = sections[0];
			var lines = section1.Lines.Cast<SummaryLine>().Where(x => !x.MainDescription.IsEmpty).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("eHub Interfaces - Transactional Fee Usage", section1.Header.MainDescription);
				AssertSummarySectionLine(lines[0], "Interface Group 1", "400", "800", "400", "40.00");
			});
		}

		void AssertSummarySectionLine(SummaryLine line, string expectedMainDescription, string expectedIncludedUsage, string expectedTotalUsage, string expectedExcessUsage, string expectedAmount)
		{
			AssertEquals("Main Desciption", expectedMainDescription, line.MainDescription);
			AssertEquals("Included Usages", expectedIncludedUsage, line.PurchasedCount);
			AssertEquals("Total Usages", expectedTotalUsage, line.UnitCount);
			AssertEquals("Excess Usages", expectedExcessUsage, line.TotalUnitCount);
			AssertEquals("Amount", expectedAmount, line.Amount);
		}

		public void TestGetDiscountSummarySections()
		{
			var org = SetupOrgWithPriceList();
			var user = new UsingParty(org, "PRD");

			var usageList = new List<ClientMappingUsage>();
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 2, 1)) { TransactionCount = 300, SubCode = "Interface 1", Reference3 = "Order Line" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 3, 1)) { TransactionCount = 500, SubCode = "Interface 1", Reference3 = "Order Line" });

			var bill = new ClientMappingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetDiscountSummarySections();
			AssertEquals(2, sections[0].Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("eHub Interfaces - Transactional Fee Amount Calculations Applied", sections[0].Header.MainDescription);
				AssertEquals("Feb 2015 Interface 1 - Volume Discount: -3.00 (-10% * 30.00)", sections[0].Lines[0].MainDescription);
				AssertEquals("Mar 2015 Interface 1 - Volume Discount: -5.00 (-10% * 50.00)", sections[0].Lines[1].MainDescription);
			});
		}

		public void TestGetSurchargeSummarySections()
		{
			var org = SetupOrgWithPriceList();
			var user = new UsingParty(org, "PRD");

			var usageList = new List<ClientMappingUsage>();
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 2, 1)) { TransactionCount = 300, SubCode = "Interface 1", Reference3 = "Order Line" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 3, 1)) { TransactionCount = 500, SubCode = "Interface 1", Reference3 = "Order Line" });

			var bill = new ClientMappingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetSurchargeSummarySections();
			AssertEquals(2, sections[0].Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("eHub Interfaces - Transactional Fee Amount Calculations Applied", sections[0].Header.MainDescription);
				AssertEquals("Feb 2015 Interface 1 - Testing Surcharge: 3.00 (10% * 30.00)", sections[0].Lines[0].MainDescription);
				AssertEquals("Mar 2015 Interface 1 - Testing Surcharge: 5.00 (10% * 50.00)", sections[0].Lines[1].MainDescription);
			});
		}

		public void TestCreateInvoiceLines()
		{
			var org = SetupOrgWithPriceList();
			var user = new UsingParty(org, "PRD");

			var usageList = new List<ClientMappingUsage>();
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 3, 1)) { TransactionCount = 500, SubCode = "Interface 1", Reference3 = "OrderLine" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 3, 1)) { TransactionCount = 1500, SubCode = "Interface 2", Reference3 = "Per Message" });

			var bill = new ClientMappingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());
			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(4, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals(
@"eHub Interfaces - Transactional Fee
PSQ 2001 - Interface 1
500 Order Line x AUD 0.1000", lines[0].Description);
				AssertEquals(50m, lines[0].Amount);
				AssertEquals(false, lines[0].RequireCommentLineAfter);

				AssertEquals(
@"eHub Interfaces - Transactional Fee Discount
PSQ 2001 - Interface 1", lines[1].Description);
				AssertEquals(-5m, lines[1].Amount);
				AssertEquals(false, lines[1].RequireCommentLineAfter);

				AssertEquals(
@"eHub Interfaces - Transactional Fee Surcharge
PSQ 2001 - Interface 1", lines[2].Description);
				AssertEquals(5m, lines[2].Amount);
				AssertEquals(true, lines[2].RequireCommentLineAfter);

				AssertEquals(
@"eHub Interfaces - Transactional Fee
PSQ 2002 - Interface 2
Included 1000 Message
Actual 1500 Message
Excess fee 500 Message x AUD 0.2000", lines[3].Description);
				AssertEquals(100m, lines[3].Amount);
				AssertEquals(true, lines[3].RequireCommentLineAfter);
			});
		}

		public void TestCreateInvoiceLines_MultipleTaxGroup()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();

			var org1 = SetupOrgWithPriceList();
			var user1 = new UsingParty(org1, "PRD");
			var invoiceDelivery1 = org1.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery1.L9_AT_TaxId = taxRate1.PK;

			var org2 = BillingTestHelper.CreateDependentOrganisation(org1, "BBB");
			var user2 = new UsingParty(org2);
			var invoiceDelivery2 = org2.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = org1.PK;
			invoiceDelivery2.L9_AT_TaxId = taxRate2.PK;

			var usageList = new List<ClientMappingUsage>();
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user1, new ZDateTime(2015, 3, 1)) { TransactionCount = 500, SubCode = "Interface 1", Reference3 = "OrderLine" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user1, new ZDateTime(2015, 3, 1)) { TransactionCount = 1500, SubCode = "Interface 2", Reference3 = "Per Message" });
			usageList.Add(new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user2, new ZDateTime(2015, 3, 1)) { TransactionCount = 1200, SubCode = "Interface 2", Reference3 = "Per Message" });

			var bill = new ClientMappingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());
			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(5, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals(
@"eHub Interfaces - Transactional Fee
PSQ 2001 - Interface 1
500 Order Line x AUD 0.1000", lines[0].Description);
				AssertEquals(50m, lines[0].Amount);
				AssertEquals(false, lines[0].RequireCommentLineAfter);

				AssertEquals(
@"eHub Interfaces - Transactional Fee Discount
PSQ 2001 - Interface 1", lines[1].Description);
				AssertEquals(-5m, lines[1].Amount);
				AssertEquals(false, lines[1].RequireCommentLineAfter);

				AssertEquals(
@"eHub Interfaces - Transactional Fee Surcharge
PSQ 2001 - Interface 1", lines[2].Description);
				AssertEquals(5m, lines[2].Amount);
				AssertEquals(true, lines[2].RequireCommentLineAfter);

				AssertEquals(
@"eHub Interfaces - Transactional Fee
PSQ 2002 - Interface 2
Included 1000 Message
Actual 1500 Message
Excess fee 500 Message x AUD 0.2000", lines[3].Description);
				AssertEquals(100m, lines[3].Amount);
				AssertEquals(true, lines[3].RequireCommentLineAfter);

				AssertEquals(
@"eHub Interfaces - Transactional Fee
PSQ 2002 - Interface 2
Included 1000 Message
Actual 1200 Message
Excess fee 200 Message x AUD 0.2000", lines[4].Description);
				AssertEquals(40m, lines[4].Amount);
				AssertEquals(true, lines[4].RequireCommentLineAfter);
			});
		}

		EDIOrgHeader SetupOrgWithPriceList()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "DDD");
			var licCompany = licHeader.Company;
			var org = licCompany.Header;
			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			var priceList = licCompany.PriceHeaders.AddNew();
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var priceItem1 = priceList.Items.AddNew();
			priceItem1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem1.L7_Description = "PSQ 2001 - Interface 1";
			priceItem1.L7_Ref4 = "Interface 1";
			priceItem1.L7_Price = 0.10m;

			var priceItem2 = priceList.Items.AddNew();
			priceItem2.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem2.L7_Description = "PSQ 2002 - Interface 2";
			priceItem2.L7_Ref4 = "Interface 2";
			priceItem2.L7_Price = 0.20m;
			priceItem2.L7_UnitBreak = 1000;

			var discount1 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = BillingConstants.BillingSystem.ClientMapping;
			discount1.L5_SubCode = "Interface 1";
			discount1.L5_Type = BillingConstants.DiscountType.Volume;
			discount1.L5_BreakAmount = 250;
			discount1.L5_Discount = 10;
			discount1.L5_StartDate = new ZDateTime(2015, 1, 1);

			var surcharge1 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			surcharge1.L5_SystemCode = BillingConstants.BillingSystem.ClientMapping;
			surcharge1.L5_SubCode = "Interface 1";
			surcharge1.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge1.L5_Discount = -10;
			surcharge1.L5_Description = "Testing";

			Factory.Save();
			return org;
		}

		#region Overrides

		protected override SystemUsage[] CreateValidSystemUsages()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			return new SystemUsage[]
			{
				new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2010, 10, 01)) { SubCode = "Interface 1" },
				new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2010, 11, 01)) { SubCode = "Interface 1" }
			};
		}

		protected override ClientMappingBill GetNewSystemBill()
		{
			return new ClientMappingBill(Factory);
		}

		#endregion
	}
}
