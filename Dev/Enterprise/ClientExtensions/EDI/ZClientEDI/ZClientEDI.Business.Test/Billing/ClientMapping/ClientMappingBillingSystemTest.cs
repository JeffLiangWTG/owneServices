using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class ClientMappingBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new ClientMappingBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ClientMapping, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2014, 11, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new ClientMappingBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2014, 11, 30));

			var bill = billingSystem.LoadSystemBills(context).First() as ClientMappingBill;
			AssertEquals(BillingConstants.BillingSystem.ClientMapping, bill.SystemCode);
		}

		public void TestCreateSystemBill_CombinedByDatabase()
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

			var chargeableUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2015, 2, 1), licence1, 600);
			var chargeableUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2015, 3, 1), licence1, 500);
			var chargeableUsage3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 2", new ZDateTime(2015, 3, 1), licence1, 1200);
			var chargeableUsage4 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2015, 3, 1), licence2, 300);

			Factory.Save();

			var billingSystem = new ClientMappingBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 3, 31));
			var bill = billingSystem.LoadSystemBills(context).First(x => x.OrganisationPK == parentOrg.PK) as ClientMappingBill;

			var usages = bill.SystemUsages.OrderBy(u => u.PeriodStart).ThenBy(u => u.SubCode).ToList();

			AssertEquals(3, usages.Count);

			var usage1 = usages[0] as ClientMappingUsage;
			AssertEquals(org1.PK, usage1.OrganisationPK);
			AssertEquals(new ZDateTime(2015, 2, 1), usage1.PeriodStart);
			AssertEquals(600, usage1.TransactionCount);
			AssertEquals(200, usage1.UnitCount);
			Assert(usage1.ChargeableUsagePKs.Contains(chargeableUsage1.PK));

			var usage2 = usages[1] as ClientMappingUsage;
			AssertEquals(org1.PK, usage2.OrganisationPK);
			AssertEquals(new ZDateTime(2015, 3, 1), usage2.PeriodStart);
			AssertEquals(800, usage2.TransactionCount);
			AssertEquals(400, usage2.UnitCount);
			Assert(usage2.ChargeableUsagePKs.Contains(chargeableUsage2.PK));
			Assert(usage2.ChargeableUsagePKs.Contains(chargeableUsage4.PK));

			var usage3 = usages[2] as ClientMappingUsage;
			AssertEquals(org1.PK, usage3.OrganisationPK);
			AssertEquals(new ZDateTime(2015, 3, 1), usage3.PeriodStart);
			AssertEquals(1200, usage3.TransactionCount);
			AssertEquals(200, usage3.UnitCount);
			Assert(usage3.ChargeableUsagePKs.Contains(chargeableUsage3.PK));
		}

		public void TestCreateSystemBill_CombinedByInterfaceGroup()
		{
			var parentOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			parentOrg.OH_Code = "DDDDDDSYD";

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = parentOrg.PK;

			var database = enterprise.Databases.AddNew();
			database.LD_ServerCode = "PRD";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;

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
			invoiceDelivery1.L9_OH_InvoiceTo = org1.PK;

			var priceList = company1.PriceHeaders.AddNew();
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

			var chargeableUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2015, 3, 1), licence1, 500);
			var chargeableUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 2", new ZDateTime(2015, 3, 1), licence1, 200);

			var onlyOneInterfaceInGroupUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2015, 4, 1), licence1, 800);

			Factory.Save();

			var billingSystem = new ClientMappingBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 4, 30));
			var bill = billingSystem.LoadSystemBills(context).First() as ClientMappingBill;

			var usages = bill.SystemUsages.Cast<ClientMappingUsage>().ToList();
			AssertEquals(2, usages.Count);

			var usage1 = usages.First(x => x.PeriodStart == new ZDateTime(2015, 3, 1));
			AssertEquals(org1.PK, usage1.OrganisationPK);
			AssertEquals(new ZDateTime(2015, 3, 1), usage1.PeriodStart);
			AssertEquals(700, usage1.TransactionCount);
			AssertEquals(300, usage1.UnitCount);
			Assert(usage1.ChargeableUsagePKs.Contains(chargeableUsage1.PK));
			Assert(usage1.ChargeableUsagePKs.Contains(chargeableUsage2.PK));

			var usage2 = usages.First(x => x.PeriodStart == new ZDateTime(2015, 4, 1));
			AssertEquals("usage is under group code even if only one interface in the group is used", "G01", usage2.SubCode);
			AssertEquals(800, usage2.TransactionCount);
			AssertEquals(400, usage2.UnitCount);
			Assert(usage2.ChargeableUsagePKs.Contains(onlyOneInterfaceInGroupUsage.PK));
		}

		public void TestCreateSystemBill_CombinedByDatabaseAndInterfaceGroup()
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

			var priceList = company1.PriceHeaders.AddNew();
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var parentPriceItem = priceList.Items.AddNew();
			parentPriceItem.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			parentPriceItem.L7_Description = "Interface Group 1";
			parentPriceItem.L7_Ref4 = "G01";
			parentPriceItem.L7_Price = 0.10m;
			parentPriceItem.L7_UnitBreak = 1000;

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

			var chargeableUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2015, 3, 1), licence1, 300);
			var chargeableUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 2", new ZDateTime(2015, 3, 1), licence1, 500);
			var chargeableUsage3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "Interface 1", new ZDateTime(2015, 3, 1), licence2, 400);

			Factory.Save();

			var billingSystem = new ClientMappingBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 3, 31));
			var bill = billingSystem.LoadSystemBills(context).First(x => x.OrganisationPK == parentOrg.PK) as ClientMappingBill;

			var usages = bill.SystemUsages.OrderBy(u => u.PeriodStart).ThenBy(u => u.SubCode).ToList();

			AssertEquals(1, usages.Count);
			var combinedUsage = usages[0] as ClientMappingUsage;
			AssertEquals("G01", combinedUsage.SubCode);
			AssertEquals(org1.PK, combinedUsage.OrganisationPK);
			AssertEquals(new ZDateTime(2015, 3, 1), combinedUsage.PeriodStart);
			AssertEquals(1200, combinedUsage.TransactionCount);
			AssertEquals(200, combinedUsage.UnitCount);
			Assert(combinedUsage.ChargeableUsagePKs.Contains(chargeableUsage1.PK));
		}

		public void TestLoadOdplRawUsage()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "DDD");
			var licCompany = licHeader.Company;
			var org = licCompany.Header;

			var priceList1 = licCompany.PriceHeaders.AddNew();
			priceList1.L6_RX_NKCurrency = "AUD";
			priceList1.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceList1.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var priceItem1 = priceList1.Items.AddNew();
			priceItem1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem1.L7_Description = "PSQ 2001 - Interface 1";
			priceItem1.L7_Ref4 = "Interface 1";
			priceItem1.L7_Price = 0.10m;

			var priceItem2 = priceList1.Items.AddNew();
			priceItem2.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem2.L7_Description = "PSQ 2300 - Interface 3";
			priceItem2.L7_Ref4 = "Interface 3";
			priceItem2.L7_Price = 0.10m;

			var priceItem3 = priceList1.Items.AddNew();
			priceItem3.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem3.L7_Description = "PSQ 2400 - Interface 4";
			priceItem3.L7_Ref4 = "Interface 4";
			priceItem3.L7_Price = 0.10m;

			var priceListOld = licCompany.PriceHeaders.AddNew();
			priceListOld.L6_RX_NKCurrency = "AUD";
			priceListOld.L6_ValidFrom = new ZDateTime(2014, 10, 1);
			priceListOld.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var priceItemOld = priceListOld.Items.AddNew();
			priceItemOld.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItemOld.L7_Description = "PSQ 2001 - Interface 1 (Old)";
			priceItemOld.L7_Ref4 = "Interface 1";
			priceItemOld.L7_Price = 0.15m;

			Factory.Save();

			var licenceCode = licCompany.LicEnterprise.LE_EnterpriseCode + licCompany.LC_CompanyCode + licHeader.Database.LD_ServerCode;
			var clientNumber = licHeader.Database.DatabaseId + "." + licCompany.LC_CompanyCode;
			var clientCompany = ClientCompany.FindOrCreate(Factory, licCompany.LC_CompanyCode, licHeader.Database.PK, org.PK, "", "");

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 1, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 1", "OrderLine", "File-001", "B00003001", null, "HUB", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 2, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 1", "OrderLine", "File-002", "S00003002", null, "HUB", 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 3, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 1", "OrderLine", "File-003", "E00003003", null, "HUB", 10));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 1, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 2", "Per Message", "File-AAA-001", "D00002001", null, "HUB", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 2, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 2", "Per Message", "File-AAA-002", "D00002001", null, "HUB", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "SCO", new ZDateTime(2015, 3, 3, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 3", "4500023919", "AUTOPART INTERNATIONAL INC", "00000000000001028221", null, "HUB", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "SCS", new ZDateTime(2015, 3, 4, 10, 0, 0), licenceCode, null, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 4", "S16I00030928", null, "00000000000001043927", null, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ClientMappingBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 3, 1), org.PK, ZGuid.Empty, licCompany.PK, licHeader.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;

			AssertEquals(4, rawUsage.SummarySections.Count);

			var section1 = rawUsage.SummarySections[0];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nPSQ 2001 - Interface 1", section1.Header.TopLevelDescription);

				AssertEquals(3, section1.Lines.Count);

				AssertEquals(licenceCode, section1.Lines[0].Column1);
				AssertEquals(licenceCode, section1.Lines[1].Column1);
				AssertEquals(licenceCode, section1.Lines[2].Column1);

				AssertEquals("OrderLine", section1.Lines[0].Column2);
				AssertEquals("OrderLine", section1.Lines[1].Column2);
				AssertEquals("OrderLine", section1.Lines[2].Column2);

				AssertEquals("1", section1.Lines[0].Column3);
				AssertEquals("5", section1.Lines[1].Column3);
				AssertEquals("10", section1.Lines[2].Column3);

				AssertEquals("File-001", section1.Lines[0].Column4);
				AssertEquals("File-002", section1.Lines[1].Column4);
				AssertEquals("File-003", section1.Lines[2].Column4);
			});

			var section2 = rawUsage.SummarySections[1];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nInterface 2", section2.Header.TopLevelDescription);

				AssertEquals(2, section2.Lines.Count);

				AssertEquals(licenceCode, section2.Lines[0].Column1);
				AssertEquals(licenceCode, section2.Lines[1].Column1);

				AssertEquals("Per Message", section2.Lines[0].Column2);
				AssertEquals("Per Message", section2.Lines[1].Column2);

				AssertEquals("1", section2.Lines[0].Column3);
				AssertEquals("1", section2.Lines[1].Column3);

				AssertEquals("File-AAA-001", section2.Lines[0].Column4);
				AssertEquals("File-AAA-002", section2.Lines[1].Column4);
			});

			var section3 = rawUsage.SummarySections[2];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nPSQ 2300 - Interface 3", section3.Header.TopLevelDescription);

				AssertEquals(1, section3.Lines.Count);

				AssertEquals(licenceCode, section3.Lines[0].Column1);
				AssertEquals("4500023919", section3.Lines[0].Column2);
				AssertEquals("AUTOPART INTERNATIONAL INC", section3.Lines[0].Column3);
			});

			var section4 = rawUsage.SummarySections[3];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nPSQ 2400 - Interface 4", section4.Header.TopLevelDescription);

				AssertEquals(1, section4.Lines.Count);

				AssertEquals(licenceCode, section4.Lines[0].Column1);
				AssertEquals("S16I00030928", section4.Lines[0].Column2);
			});

			string expectedCsvResult =
@"""Interface"",""Client ID"",""Element"",""Units"",""Filename"",""Message Time (UTC)""
""PSQ 2001 - Interface 1"",""DDDDDDDDD"",""OrderLine"",""1"",""File-001"",""01-Mar-15 10:00""
""PSQ 2001 - Interface 1"",""DDDDDDDDD"",""OrderLine"",""5"",""File-002"",""02-Mar-15 10:00""
""PSQ 2001 - Interface 1"",""DDDDDDDDD"",""OrderLine"",""10"",""File-003"",""03-Mar-15 10:00""
""Interface 2"",""DDDDDDDDD"",""Per Message"",""1"",""File-AAA-001"",""01-Mar-15 10:00""
""Interface 2"",""DDDDDDDDD"",""Per Message"",""1"",""File-AAA-002"",""02-Mar-15 10:00""
""Interface"",""Client ID"",""Order No."",""Buyer"",""Message Time (UTC)""
""PSQ 2300 - Interface 3"",""DDDDDDDDD"",""4500023919"",""AUTOPART INTERNATIONAL INC"",""03-Mar-15 10:00""
""Interface"",""Client ID"",""Shipment ID"",""Message Time (UTC)""
""PSQ 2400 - Interface 4"",""DDDDDDDDD"",""S16I00030928"",""04-Mar-15 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Mar-15 10:00"",""DDDDDDDDD"",""B10"",""S10"",""Interface 1 OrderLine File-001"",""CMP"",""PSQ 2001 - Interface 1"",""1""
""02-Mar-15 10:00"",""DDDDDDDDD"",""B11"",""S11"",""Interface 1 OrderLine File-002"",""CMP"",""PSQ 2001 - Interface 1"",""5""
""03-Mar-15 10:00"",""DDDDDDDDD"",""B12"",""S12"",""Interface 1 OrderLine File-003"",""CMP"",""PSQ 2001 - Interface 1"",""10""
""01-Mar-15 10:00"",""DDDDDDDDD"",""B13"",""S13"",""Interface 2 Per Message File-AAA-001"",""CMP"",""Interface 2"",""1""
""02-Mar-15 10:00"",""DDDDDDDDD"",""B14"",""S14"",""Interface 2 Per Message File-AAA-002"",""CMP"",""Interface 2"",""1""
""03-Mar-15 10:00"",""DDDDDDDDD"",""B15"",""S15"",""Interface 3 4500023919 AUTOPART INTERNATIONAL INC"",""SCO"",""PSQ 2300 - Interface 3"",""1""
""04-Mar-15 10:00"",""DDDDDDDDD"",""B16"",""S16"",""Interface 4 S16I00030928"",""SCS"",""PSQ 2400 - Interface 4"",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "DDD");
			var licCompany = licHeader.Company;
			var org = licCompany.Header;

			var priceList1 = licCompany.PriceHeaders.AddNew();
			priceList1.L6_RX_NKCurrency = "AUD";
			priceList1.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceList1.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var priceItem1 = priceList1.Items.AddNew();
			priceItem1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem1.L7_Description = "PSQ 2001 - Interface 1";
			priceItem1.L7_Ref4 = "Interface 1";
			priceItem1.L7_Price = 0.10m;

			var priceItem2 = priceList1.Items.AddNew();
			priceItem2.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem2.L7_Description = "PSQ 2300 - Interface 3";
			priceItem2.L7_Ref4 = "Interface 3";
			priceItem2.L7_Price = 0.10m;

			var priceItem3 = priceList1.Items.AddNew();
			priceItem3.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItem3.L7_Description = "PSQ 2400 - Interface 4";
			priceItem3.L7_Ref4 = "Interface 4";
			priceItem3.L7_Price = 0.10m;

			var priceListOld = licCompany.PriceHeaders.AddNew();
			priceListOld.L6_RX_NKCurrency = "AUD";
			priceListOld.L6_ValidFrom = new ZDateTime(2014, 10, 1);
			priceListOld.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var priceItemOld = priceListOld.Items.AddNew();
			priceItemOld.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceItemOld.L7_Description = "PSQ 2001 - Interface 1 (Old)";
			priceItemOld.L7_Ref4 = "Interface 1";
			priceItemOld.L7_Price = 0.15m;

			Factory.Save();

			var licenceCode = licCompany.LicEnterprise.LE_EnterpriseCode + licCompany.LC_CompanyCode + licHeader.Database.LD_ServerCode;
			var clientNumber = licHeader.Database.DatabaseId + "." + licCompany.LC_CompanyCode;
			var clientCompany = ClientCompany.FindOrCreate(Factory, licCompany.LC_CompanyCode, licHeader.Database.PK, org.PK, "", "");

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 1, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 1", "OrderLine", "File-001", "B00003001", null, "HUB", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 2, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 1", "OrderLine", "File-002", "S00003002", null, "HUB", 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 3, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 1", "OrderLine", "File-003", "E00003003", null, "HUB", 10));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 1, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 2", "Per Message", "File-AAA-001", "D00002001", null, "HUB", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "CMP", new ZDateTime(2015, 3, 2, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 2", "Per Message", "File-AAA-002", "D00002001", null, "HUB", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "SCO", new ZDateTime(2015, 3, 3, 10, 0, 0), licenceCode, clientNumber, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 3", "4500023919", "AUTOPART INTERNATIONAL INC", "00000000000001028221", null, "HUB", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CMP", "SCS", new ZDateTime(2015, 3, 4, 10, 0, 0), licenceCode, null, licHeader.Database.DatabaseId, clientCompany.PK, "Interface 4", "S16I00030928", null, "00000000000001043927", null, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ClientMappingBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 3, 1), licHeader.Database.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context) as MultiSectionStlRawUsage;

			AssertEquals(4, rawUsage.SummarySections.Count);

			var section1 = rawUsage.SummarySections[0];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nPSQ 2001 - Interface 1", section1.Header.TopLevelDescription);

				AssertEquals(3, section1.Lines.Count);

				AssertEquals(licCompany.LC_CompanyCode, section1.Lines[0].Column1);
				AssertEquals(licCompany.LC_CompanyCode, section1.Lines[1].Column1);
				AssertEquals(licCompany.LC_CompanyCode, section1.Lines[2].Column1);

				AssertEquals("OrderLine", section1.Lines[0].Column2);
				AssertEquals("OrderLine", section1.Lines[1].Column2);
				AssertEquals("OrderLine", section1.Lines[2].Column2);

				AssertEquals("1", section1.Lines[0].Column3);
				AssertEquals("5", section1.Lines[1].Column3);
				AssertEquals("10", section1.Lines[2].Column3);

				AssertEquals("File-001", section1.Lines[0].Column4);
				AssertEquals("File-002", section1.Lines[1].Column4);
				AssertEquals("File-003", section1.Lines[2].Column4);
			});

			var section2 = rawUsage.SummarySections[1];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nInterface 2", section2.Header.TopLevelDescription);

				AssertEquals(2, section2.Lines.Count);

				AssertEquals(licCompany.LC_CompanyCode, section2.Lines[0].Column1);
				AssertEquals(licCompany.LC_CompanyCode, section2.Lines[1].Column1);

				AssertEquals("Per Message", section2.Lines[0].Column2);
				AssertEquals("Per Message", section2.Lines[1].Column2);

				AssertEquals("1", section2.Lines[0].Column3);
				AssertEquals("1", section2.Lines[1].Column3);

				AssertEquals("File-AAA-001", section2.Lines[0].Column4);
				AssertEquals("File-AAA-002", section2.Lines[1].Column4);
			});

			var section3 = rawUsage.SummarySections[2];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nPSQ 2300 - Interface 3", section3.Header.TopLevelDescription);

				AssertEquals(1, section3.Lines.Count);

				AssertEquals(licCompany.LC_CompanyCode, section3.Lines[0].Column1);
				AssertEquals("4500023919", section3.Lines[0].Column2);
				AssertEquals("AUTOPART INTERNATIONAL INC", section3.Lines[0].Column3);
			});

			var section4 = rawUsage.SummarySections[3];
			CombineAssertions(() =>
			{
				AssertEquals("\r\nPSQ 2400 - Interface 4", section4.Header.TopLevelDescription);

				AssertEquals(1, section4.Lines.Count);

				AssertEquals(licCompany.LC_CompanyCode, section4.Lines[0].Column1);
				AssertEquals("S16I00030928", section4.Lines[0].Column2);
			});

			string expectedCsvResult =
@"""Interface"",""Company Code"",""Element"",""Units"",""Filename"",""Message Time (UTC)""
""PSQ 2001 - Interface 1"",""DDD"",""OrderLine"",""1"",""File-001"",""01-Mar-15 10:00""
""PSQ 2001 - Interface 1"",""DDD"",""OrderLine"",""5"",""File-002"",""02-Mar-15 10:00""
""PSQ 2001 - Interface 1"",""DDD"",""OrderLine"",""10"",""File-003"",""03-Mar-15 10:00""
""Interface 2"",""DDD"",""Per Message"",""1"",""File-AAA-001"",""01-Mar-15 10:00""
""Interface 2"",""DDD"",""Per Message"",""1"",""File-AAA-002"",""02-Mar-15 10:00""
""Interface"",""Company Code"",""Order No."",""Buyer"",""Message Time (UTC)""
""PSQ 2300 - Interface 3"",""DDD"",""4500023919"",""AUTOPART INTERNATIONAL INC"",""03-Mar-15 10:00""
""Interface"",""Company Code"",""Shipment ID"",""Message Time (UTC)""
""PSQ 2400 - Interface 4"",""DDD"",""S16I00030928"",""04-Mar-15 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Mar-15 10:00"",""DDD"",""B10"",""S10"",""Interface 1 OrderLine File-001"",""CMP"",""PSQ 2001 - Interface 1"",""1""
""02-Mar-15 10:00"",""DDD"",""B11"",""S11"",""Interface 1 OrderLine File-002"",""CMP"",""PSQ 2001 - Interface 1"",""5""
""03-Mar-15 10:00"",""DDD"",""B12"",""S12"",""Interface 1 OrderLine File-003"",""CMP"",""PSQ 2001 - Interface 1"",""10""
""01-Mar-15 10:00"",""DDD"",""B13"",""S13"",""Interface 2 Per Message File-AAA-001"",""CMP"",""Interface 2"",""1""
""02-Mar-15 10:00"",""DDD"",""B14"",""S14"",""Interface 2 Per Message File-AAA-002"",""CMP"",""Interface 2"",""1""
""03-Mar-15 10:00"",""DDD"",""B15"",""S15"",""Interface 3 4500023919 AUTOPART INTERNATIONAL INC"",""SCO"",""PSQ 2300 - Interface 3"",""1""
""04-Mar-15 10:00"",""DDD"",""B16"",""S16"",""Interface 4 S16I00030928"",""SCS"",""PSQ 2400 - Interface 4"",""1""
", writer.ToString());
		}
	}
}