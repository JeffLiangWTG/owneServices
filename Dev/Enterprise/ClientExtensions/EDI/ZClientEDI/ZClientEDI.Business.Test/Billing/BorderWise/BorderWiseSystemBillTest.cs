using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.BorderWise.Test
{
	[TestedType(typeof(BorderWiseSystemBill))]
	public class BorderWiseSystemBillTest : SystemBillTestCase<BorderWiseSystemBill>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewSystemBill();
		}

		protected override BorderWiseSystemBill GetNewSystemBill()
		{
			return new BorderWiseSystemBill(Factory);
		}

		public void TestGetChargeCodeNames()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			var userPrice = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.UserPriceCode);
			var extraPrice = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.ExtraMachinePriceCode);
			userPrice.L7_ChargeCode = "USERCHARGE";
			extraPrice.L7_ChargeCode = "XCHARGE";
			userPrice.L7_DiscountChargeCode = "USERDISCO";
			extraPrice.L7_DiscountChargeCode = "XDISCO";
			var licBOR = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BOR");
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var bill = new BorderWiseSystemBill(Factory);
			var usage1 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR), periodStart,
				BillingConstants.BorderWise.UserPriceCode, 1);
			var usage2 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR), periodStart,
				BillingConstants.BorderWise.ExtraMachinePriceCode, 1);
			var usage3 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR), periodStart,
				"???", 1);

			AssertEquals("USERCHARGE", bill.GetAmountChargeCodeName(usage1));
			AssertEquals("XCHARGE", bill.GetAmountChargeCodeName(usage2));
			AssertEquals("", bill.GetAmountChargeCodeName(usage3));
			AssertEquals("USERDISCO", bill.GetDiscountChargeCodeName(usage1));
			AssertEquals("XDISCO", bill.GetDiscountChargeCodeName(usage2));
			AssertEquals("", bill.GetDiscountChargeCodeName(usage3));
		}

		public void TestPriceCurrency()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			BillingTestHelper.AddPriceItemRates(borderWisePrices, "USD", 2);
			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			var licBOR2 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW2", LicenceAdvStdOthList.Codes.OnDemand);
			BillingTestHelper.SetInvoicing(licBOR1, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicing(licBOR2, Env.CurrentBranchPK, "USD");
			var licCW1 = BillingTestHelper.CreateAnotherDatabase(licBOR1, "CW1");
			var licCW2 = BillingTestHelper.CreateAnotherDatabase(licBOR2, "CW2");
			var cwPrices1 = BillingTestHelper.CreatePriceList(licCW1, 5m);
			var cwPrices2 = BillingTestHelper.CreatePriceList(licCW2, 5m);
			cwPrices2.L6_RX_NKCurrency = "USD";
			cwPrices2.Items.FindByCode("COR").L7_Price = 2.5;
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 3);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode, periodStart, licBOR1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode, periodStart, licBOR2.LA_LC, 9);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, licCW1.ClientCompany, 101);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, licCW2.ClientCompany, 102);
			// Suppress any warning about missing active users
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, licCW1.ClientCompany, 103);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, licCW2.ClientCompany, 104);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billing = new MonthlyUsageBilling(Factory);
			var odplBilling = new OdplBillingSystem();
			var borderWiseBilling = new BorderWiseBillingSystem();
			billing.BillingSystems.Clear();
			billing.BillingSystems.Add(odplBilling);
			billing.BillingSystems.Add(borderWiseBilling);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(2, billing.OrganisationBills.Count);
			var orgBill1 = billing.OrganisationBills.Cast<OrganisationBill>().Single(x => x.OrganisationPK == licBOR1.Company.LC_OH);
			var orgBill2 = billing.OrganisationBills.Cast<OrganisationBill>().Single(x => x.OrganisationPK == licBOR2.Company.LC_OH);
			AssertNoNotifications(orgBill1);
			AssertNoNotifications(orgBill2);
			AssertEquals(7 * 200m + 101 * 3m, orgBill1.Amount);
			AssertEquals(9 * 400m + 102 * 2.5m, orgBill2.Amount);
			AssertEquals("Price is AUD", true, orgBill1.SystemUsages.Cast<SystemUsage>().All(x => x.CurrencyCode == "AUD"));
			AssertEquals("Price is USD", true, orgBill2.SystemUsages.Cast<SystemUsage>().All(x => x.CurrencyCode == "USD"));
		}

		public void TestDiscountCalculation_OnDemandDiscountOnly()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany, 10);
			borderWisePrices.L6_LicenceUnitRate = 5m;

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");
			var licCW = BillingTestHelper.CreateAnotherDatabase(lic1, "CW1");
			var cwPrices = BillingTestHelper.CreatePriceList(licCW, 5m);
			licCW.Database.LD_HostedLocation = "SYD";
			var wiseCloudDiscount = licCW.Company.SelfBilling.BillingDiscounts.AddNew();
			wiseCloudDiscount.L5_StartDate = periodStart;
			wiseCloudDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			wiseCloudDiscount.L5_Type = BillingConstants.DiscountType.WiseCloud;
			wiseCloudDiscount.L5_Discount = 10;

			var volDiscount1 = licCW.Company.SelfBilling.BillingDiscounts.AddNew();
			volDiscount1.L5_StartDate = periodStart;
			volDiscount1.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			volDiscount1.L5_Type = BillingConstants.DiscountType.Volume;
			volDiscount1.L5_BreakAmount = 5000 / 50;
			volDiscount1.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			volDiscount1.L5_Discount = 20m;

			// 7*200 = $1400 or 7000/50 licence units
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode, periodStart, lic1.LA_LC, 7);
			// 3*30 = $90 or 450/50 licence units
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.ExtraMachinePriceCode, periodStart, lic1.LA_LC, 3);
			// 100*3 = $300 or 1500/50 licence units
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, licCW.ClientCompany, 100);
			// Suppress any warning about missing active users
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, licCW.ClientCompany, 100);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode, periodStart.AddMonths(-1), lic1.LA_LC, 5);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billing = new MonthlyUsageBilling(Factory);
			var odplBilling = new OdplBillingSystem();
			var borderWiseBilling = new BorderWiseBillingSystem();
			billing.BillingSystems.Clear();
			billing.BillingSystems.Add(odplBilling);
			billing.BillingSystems.Add(borderWiseBilling);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.OrganisationBills.Count);
			var bill = billing.OrganisationBills[0];
			AssertNoNotifications(bill);
			AssertEquals(7 * 200m + 3 * 30m + 100 * 3m, bill.Amount);
			AssertEquals(bill.Amount * 0.3m, bill.DiscountAmount);
		}

		public void TestDiscountCalculation_BorderwiseAndOnDemand()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");
			var licCW = BillingTestHelper.CreateAnotherDatabase(lic1, "CW1");
			var cwPrices = BillingTestHelper.CreatePriceList(licCW, 5m);
			licCW.Database.LD_HostedLocation = "SYD";
			var wiseCloudDiscount = licCW.Company.SelfBilling.BillingDiscounts.AddNew();
			wiseCloudDiscount.L5_StartDate = periodStart;
			wiseCloudDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			wiseCloudDiscount.L5_Type = BillingConstants.DiscountType.WiseCloud;
			wiseCloudDiscount.L5_Discount = 10;

			var odmModuleDiscount = licCW.Company.SelfBilling.BillingDiscounts.AddNew();
			odmModuleDiscount.L5_StartDate = periodStart;
			odmModuleDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			odmModuleDiscount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			odmModuleDiscount.L5_Discount = 100;
			odmModuleDiscount.L5_ModuleCode = "WAR";

			var stdLegacyDiscount1 = stdLicCompany.SelfBilling.BillingDiscounts.AddNew();
			stdLegacyDiscount1.L5_StartDate = periodStart;
			stdLegacyDiscount1.L5_SystemCode = BillingConstants.BillingSystem.BorderWise;
			stdLegacyDiscount1.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			stdLegacyDiscount1.L5_Discount = 100m;
			stdLegacyDiscount1.L5_ModuleCode = BillingConstants.BorderWise.TradefoxOrDigeratiUserPriceCode;
			stdLegacyDiscount1.L5_DiscountCode = borderWisePrices.L6_DiscountCode;

			var borUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode, periodStart, lic1.LA_LC, 3);
			var borUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.ExtraMachinePriceCode, periodStart, lic1.LA_LC, 5);
			var borLegacyUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.TradefoxOrDigeratiUserPriceCode, periodStart, lic1.LA_LC, 7);

			var corUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, licCW.ClientCompany, 9);
			var warUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "WAR", periodStart, licCW.ClientCompany, 8);
			// Suppress any warning about missing active users
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, licCW.ClientCompany, 9);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billing = new MonthlyUsageBilling(Factory);
			var odplBilling = new OdplBillingSystem();
			var borderWiseBilling = new BorderWiseBillingSystem();
			billing.BillingSystems.Clear();
			billing.BillingSystems.Add(odplBilling);
			billing.BillingSystems.Add(borderWiseBilling);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.OrganisationBills.Count);
			var bill = billing.OrganisationBills[0];
			AssertNoNotifications(bill);
			var corePrice = cwPrices.Items.FindByCode("COR").L7_Price;
			var warPrice = cwPrices.Items.FindByCode("WAR").L7_Price;
			var corAmount = corUsage.U1_UnitCount * corePrice;
			var warAmount = warUsage.U1_UnitCount * warPrice;
			var borUserAmount = borUsage1.U1_UnitCount * borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.UserPriceCode).L7_Price
				+ borUsage2.U1_UnitCount * borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.ExtraMachinePriceCode).L7_Price;

			var borLegacyAmount = borLegacyUsage.U1_UnitCount * borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.TradefoxOrDigeratiUserPriceCode).L7_Price;
			var borSystemBill = (BorderWiseSystemBill)bill.SystemBills.Cast<SystemBill>().Single(x => x.SystemCode == BillingConstants.BillingSystem.BorderWise);
			var odmSystemBill = (OdplSystemBill)bill.SystemBills.Cast<SystemBill>().Single(x => x.SystemCode == BillingConstants.BillingSystem.ODM);
			var borDiscounts = borSystemBill.DiscountCalculation.Details.ToList();
			var odmDiscounts = odmSystemBill.DiscountCalculation.Details.ToList();

			AssertEquals(2, odmDiscounts.Count);
			var odmWiseCloudDiscountDetails = odmDiscounts.Single(x => x.DiscountType == BillingConstants.DiscountType.WiseCloud);
			var warDiscountDetails = odmDiscounts.Single(x => x.DiscountType == BillingConstants.DiscountType.ModuleSpecific);
			AssertEquals(corAmount * 0.1m, odmWiseCloudDiscountDetails.DiscountAmount);
			AssertEquals(warAmount, warDiscountDetails.DiscountAmount);

			AssertEquals(2, borDiscounts.Count);
			var legacyDiscountDetails = borDiscounts.Single(x => x.DiscountType == BillingConstants.DiscountType.ModuleSpecific);
			var borWiseCloudDiscountDetails = borDiscounts.Single(x => x.DiscountType == BillingConstants.DiscountType.WiseCloud);
			AssertEquals(borUserAmount * 0.1m, borWiseCloudDiscountDetails.DiscountAmount);
			AssertEquals(borLegacyAmount, legacyDiscountDetails.DiscountAmount);

			AssertEquals(borUserAmount + borLegacyAmount + corAmount + warAmount, bill.Amount);
			AssertEquals(borLegacyAmount + warAmount + (corAmount + borUserAmount) * 0.1m, bill.DiscountAmount);
		}

		public void TestCreateInvoiceLines()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			var userPrice = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.UserPriceCode);
			var userPrice2 = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.UserPriceCode2);
			var userPrice3 = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.UserPriceCode3);
			var extraPrice = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.ExtraMachinePriceCode);
			userPrice.L7_ChargeCode = "USERCHARGE";
			userPrice2.L7_ChargeCode = "USER2";
			userPrice3.L7_ChargeCode = "USER3";
			extraPrice.L7_ChargeCode = "XCHARGE";
			userPrice.L7_DiscountChargeCode = "USERDISCO";
			userPrice2.L7_DiscountChargeCode = "USERDISCO";
			userPrice3.L7_DiscountChargeCode = "USERDISCO";
			extraPrice.L7_DiscountChargeCode = "XDISCO";
			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BOR");
			var licBOR2 = BillingTestHelper.CreateAnotherLicence(licBOR1, "CO3", false);
			BillingTestHelper.SetInvoicing(licBOR1, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicing(licBOR2, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicingTo(licBOR2, licBOR1);
			var discount = licBOR1.Company.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.BorderWise;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 10m;
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var bill = new BorderWiseSystemBill(Factory);
			var usage1 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR1), periodStart,
				BillingConstants.BorderWise.UserPriceCode, 7);
			var usage1b = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR1), periodStart,
				BillingConstants.BorderWise.UserPriceCode2, 7);
			var usage1c = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR1), periodStart,
				BillingConstants.BorderWise.UserPriceCode3, 7);
			var usage2 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR1), periodStart,
				BillingConstants.BorderWise.ExtraMachinePriceCode, 3);
			var usage3 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR2), periodStart,
				BillingConstants.BorderWise.UserPriceCode, 11);
			var usage3b = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR2), periodStart,
				BillingConstants.BorderWise.UserPriceCode2, 11);
			var usage3c = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR2), periodStart,
				BillingConstants.BorderWise.UserPriceCode3, 11);
			var usage4 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR2), periodStart,
				BillingConstants.BorderWise.ExtraMachinePriceCode, 5);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage1b, usage1c, usage2, usage3, usage3b, usage3c, usage4 });
			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Today);
			AssertEquals(8, lines.Count);

			AssertEquals("USERCHARGE", lines[0].ChargeCodeName);
			AssertEquals(18 * 200m, lines[0].Amount);
			AssertEquals("", lines[0].Description);

			AssertEquals("USER2", lines[1].ChargeCodeName);
			AssertEquals(18 * 50m, lines[1].Amount);

			AssertEquals("USER3", lines[2].ChargeCodeName);
			AssertEquals(18 * 40m, lines[2].Amount);

			AssertEquals("XCHARGE", lines[3].ChargeCodeName);
			AssertEquals(8 * 30m, lines[3].Amount);

			AssertEquals("USERDISCO", lines[4].ChargeCodeName);
			AssertEquals(-(18 * 200m) * 0.1m, lines[4].Amount);

			AssertEquals("USERDISCO", lines[5].ChargeCodeName);
			AssertEquals(-(18 * 50m) * 0.1m, lines[5].Amount);

			AssertEquals("USERDISCO", lines[6].ChargeCodeName);
			AssertEquals(-(18 * 40m) * 0.1m, lines[6].Amount);

			AssertEquals("XDISCO", lines[7].ChargeCodeName);
			AssertEquals(-(8 * 30m) * 0.1m, lines[7].Amount);
		}

		public void TestGetGeneralSummarySections()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			var userPrice = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.UserPriceCode);
			userPrice.L7_ChargeCode = "USERCHARGE";
			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BOR");
			var licBOR2 = BillingTestHelper.CreateAnotherLicence(licBOR1, "CO3", false);
			BillingTestHelper.SetInvoicing(licBOR1, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicing(licBOR2, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicingTo(licBOR2, licBOR1);
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var usage1 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR1), periodStart,
				BillingConstants.BorderWise.UserPriceCode, 7);
			var usage2 = new UniversalPriceSystemUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.PriceHeaderType.BorderWise,
				new UsingParty(licBOR2), periodStart,
				BillingConstants.BorderWise.UserPriceCode, 11);
			AssertEquals(licBOR1.Company.LC_OH, usage1.InvoicedOrganisationPK);
			AssertEquals(licBOR1.Company.LC_OH, usage2.InvoicedOrganisationPK);
			{
				var bill = new BorderWiseSystemBill(Factory);
				bill.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage2 });
				var summary1 = bill.GetGeneralSummarySections(licBOR1.Company.LC_OH);
				var summary2 = bill.GetGeneralSummarySections(licBOR2.Company.LC_OH);
				AssertEquals(1, summary1.Length);
				AssertEquals(1, summary2.Length);
				AssertNotNull(summary1[0]);
				AssertNotNull(summary2[0]);
			}

			{
				var bill = new BorderWiseSystemBill(Factory);
				bill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });
				var summary1 = bill.GetGeneralSummarySections(licBOR1.Company.LC_OH);
				var summary2 = bill.GetGeneralSummarySections(licBOR2.Company.LC_OH);
				AssertEquals(1, summary1.Length);
				AssertEquals(0, summary2.Length);
				AssertNotNull(summary1[0]);
			}

			{
				var bill = new BorderWiseSystemBill(Factory);
				bill.PopulateFromSystemUsages(new SystemUsage[] { usage2 });
				var summary1 = bill.GetGeneralSummarySections(licBOR1.Company.LC_OH);
				var summary2 = bill.GetGeneralSummarySections(licBOR2.Company.LC_OH);
				AssertEquals(0, summary1.Length);
				AssertEquals(1, summary2.Length);
				AssertNotNull(summary2[0]);
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
