using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.ODPL.Test
{
	[TestedType(typeof(OdplSystemBill))]
	internal class OdplSystemBillTest : SystemBillTestCase<OdplSystemBill>
	{
		public void TestAmountChargeCodeName()
		{
			OdplSystemBill bill = new OdplSystemBill(Factory);
			AssertEquals(EDIDataRegistry.Instance.OdplUsageChargeCode.Value, bill.GetAmountChargeCodeName(null));
		}

		public void TestDiscountChargeCodeName()
		{
			OdplSystemBill bill = new OdplSystemBill(Factory);
			AssertEquals(EDIDataRegistry.Instance.OdplDiscountChargeCode.Value, bill.GetDiscountChargeCodeName(null));
		}

		public void TestDiscounts()
		{
			AssertEquals(true, OdplBill.DiscountCalculation.DiscountAmount > 0);

			OdplBill.ClearDiscounts();
			AssertEquals(true, OdplBill.DiscountCalculation.DiscountAmount > 0);

			Organisation.LicCompany.SelfBilling.BillingDiscounts.DeleteAll();
			OdplBill.ClearDiscounts();
			AssertEquals(true, OdplBill.DiscountCalculation.DiscountAmount == 0);
		}

		public void TestIOdplDiscountable()
		{
			var discountable = OdplBill as IOdplDiscountable;
			AssertNotNull(discountable);

			AssertEquals(OdplBill.SystemCode, discountable.SystemCode);
			AssertEquals(OdplBill.GroupProductionAmount, discountable.AmountToDiscount);

			string moduleName;
			ZDecimal amount = discountable.AmountToDiscountForModule("AAA", out moduleName);
			AssertEquals(140m, amount);
			AssertEquals("AAA Module", moduleName);

			LicHeader.Database.LD_HostedLocation = "@#$";
			AssertEquals(false, LicHeader.Database.IsHostedOnWiseCloud);
			AssertEquals(false, discountable.IsHostedOnWiseCloud);

			LicHeader.Database.LD_HostedLocation = "SYD";
			AssertEquals(true, LicHeader.Database.IsHostedOnWiseCloud);
			AssertEquals(true, discountable.IsHostedOnWiseCloud);
		}

		public void TestAmounts()
		{
			AssertEquals(140m, OdplBill.GroupProductionAmount);
			AssertEquals(200m, OdplBill.Amount);
			AssertEquals(20m, OdplBill.DiscountAmount);
			AssertEquals(10m, OdplBill.SurchargeAmount);
		}

		public void TestPurchasedLicenceUnits()
		{
			OdplUsage odplUsage1 = CreateUsage(LicHeader);
			OdplUsageTest.AddModuleUsage(odplUsage1, "AAA", 100, 15m, 3);
			odplUsage1.PurchasedLicenceUnits = 100;
			odplUsage1.LicenceUnitRate = 5m;

			OdplUsage odplUsage2 = CreateUsage(ChildLicHeader);
			OdplUsageTest.AddModuleUsage(odplUsage2, "AAA", 40, 15m, 3);

			var bill = new OdplSystemBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			AssertEquals(100, bill.PurchasedLicenceUnits);
			AssertEquals(5m, bill.LicenceUnitRate);

			AssertEquals((100 + 40) * 15m, bill.GroupProductionAmount);
			AssertEquals((100 + 40) * 15m - 500m, bill.Amount);
			AssertEquals((100 + 40) * 15m - 500m, ((IOdplDiscountable)bill).AmountToDiscount);
		}

		public void TestAmountAndDiscount_LicenceUnits()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "XXX", "SYD", "XXX");
			EDIOrgHeader organisation = licHeader.Company.Header;
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK, "AUD");
			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m, 6);

			ClientLicenceBillingDiscount discount = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Volume;
			discount.L5_BreakAmount = 90m;
			discount.L5_Discount = 30m;

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			OdplUsage odplUsageWithDiscount = CreateUsage(licHeader);
			OdplUsageTest.AddModuleUsage(odplUsageWithDiscount, BillingConstants.CoreModuleCode, 1, 80, 100);
			OdplSystemBill odplBillWithDiscount = new OdplSystemBill(Factory);
			odplBillWithDiscount.PopulateFromSystemUsages(new SystemUsage[] { odplUsageWithDiscount });
			AssertEquals("Amount", 80m, odplBillWithDiscount.Amount);
			AssertEquals("Discount - Licence-Usage reach break amount", 24m, odplBillWithDiscount.DiscountAmount);

			discount.L5_BreakAmount = 110;
			OdplUsage odplUsageWithNoDiscount1 = CreateUsage(licHeader);
			OdplUsageTest.AddModuleUsage(odplUsageWithNoDiscount1, BillingConstants.CoreModuleCode, 1, 80, 100);
			OdplSystemBill odplBillWithNoDiscount1 = new OdplSystemBill(Factory);
			odplBillWithNoDiscount1.PopulateFromSystemUsages(new SystemUsage[] { odplUsageWithNoDiscount1 });
			AssertEquals("Amount", 80m, odplBillWithNoDiscount1.Amount);
			AssertEquals("Licence Usage does not reach break-amount", 0m, odplBillWithNoDiscount1.DiscountAmount);

			discount.L5_BreakAmount = 90;
			OdplUsage odplUsageWithNoDiscount2 = CreateUsage(licHeader);
			OdplUsageTest.AddModuleUsage(odplUsageWithNoDiscount2, BillingConstants.CoreModuleCode, 1, 100, 80);
			OdplSystemBill odplBillWithNoDiscount2 = new OdplSystemBill(Factory);
			odplBillWithNoDiscount2.PopulateFromSystemUsages(new SystemUsage[] { odplUsageWithNoDiscount2 });
			AssertEquals("Amount", 100m, odplBillWithNoDiscount2.Amount);
			AssertEquals("Licence Usage does not reach break-amount; but Amount reach break-amount", 0m, odplBillWithNoDiscount2.DiscountAmount);
		}

		public void TestCreateInvoiceLines_Hybrid()
		{
			var parentLicence = BillingTestHelper.CreateLicence(Factory, "XXX");
			var parentOrg = parentLicence.Company.Header;
			BillingTestHelper.SetInvoicing(parentOrg, Env.CurrentBranch.PK, "AUD");

			var childLicenceWithSeats = BillingTestHelper.CreateAnotherLicence(parentLicence, "YYY");
			childLicenceWithSeats.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			childLicenceWithSeats.Company.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = parentOrg.PK;
			childLicenceWithSeats.GetCoreModule().LM_UserCount = 3;

			ClientLicencePriceHeader priceHeader = parentOrg.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);

			ClientLicenceBillingDiscount discount = parentOrg.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 10m;

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			OdplUsage usageParent = CreateUsage(parentLicence);
			OdplUsageTest.AddModuleUsage(usageParent, BillingConstants.CoreModuleCode, 50);
			usageParent.CalculateAmount();

			OdplUsage usageChild = CreateUsage(childLicenceWithSeats);
			OdplUsageTest.AddModuleUsage(usageChild, BillingConstants.CoreModuleCode, 7);
			usageChild.CalculateAmount();

			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { usageParent, usageChild });

			AssertEquals("Amount", 540m, odplBill.Amount);
			AssertEquals("Discount", 54m, odplBill.DiscountAmount);

			List<SystemBill.BillLine> lines = new List<SystemBill.BillLine>();
			odplBill.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals("lines", 4, lines.Count);
			AssertAmountLine(lines[0], 40m, EDIDataRegistry.Instance.OdplHybridUsageChargeCode.Value, "On Demand Hybrid Usage");
			AssertAmountLine(lines[1], 500m, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, "On Demand Usage");
			AssertAmountLine(lines[2], -4m, EDIDataRegistry.Instance.OdplHybridDiscountChargeCode.Value, "On Demand Hybrid Discount");
			AssertAmountLine(lines[3], -50m, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value, "On Demand Discount");

			discount.L5_Type = BillingConstants.DiscountType.Surcharge;
			discount.L5_Discount = -20m;
			Factory.Save();

			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { usageParent, usageChild });
			AssertEquals("Amount", 540m, odplBill.Amount);
			AssertEquals("Surcharge", 108m, odplBill.SurchargeAmount);

			lines = new List<SystemBill.BillLine>();
			odplBill.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals("lines", 4, lines.Count);
			AssertAmountLine(lines[0], 40m, EDIDataRegistry.Instance.OdplHybridUsageChargeCode.Value, "On Demand Hybrid Usage");
			AssertAmountLine(lines[1], 500m, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, "On Demand Usage");
			AssertAmountLine(lines[2], 8m, EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value, "On Demand Hybrid Surcharge");
			AssertAmountLine(lines[3], 100m, EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value, "On Demand Surcharge");
		}

		void AssertAmountLine(SystemBill.BillLine invoiceLine, ZDecimal amount, ZString amountChargeCodeName, ZString description)
		{
			AssertEquals(amount, invoiceLine.Amount);
			AssertEquals(amountChargeCodeName, invoiceLine.ChargeCodeName);
			AssertEquals(description, invoiceLine.Description);
		}

		public void TestOnInvoiceFactorySaving()
		{
			ZDecimal cappedDiscountInitialAmount = 200m;
			ClientLicenceBillingDiscount cappedDiscount = Organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			cappedDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			cappedDiscount.L5_Type = BillingConstants.DiscountType.Capped;
			cappedDiscount.L5_BreakAmount = cappedDiscountInitialAmount;
			cappedDiscount.L5_Discount = 10m;
			cappedDiscount.L5_Duration = 12;
			Factory.Save();

			OdplBill.ClearDiscounts();
			AssertNotNull("Re-calculating discounts using capped discount", OdplBill.DiscountCalculation);
			AssertEquals("Precondition", true, cappedDiscount.L5_BreakAmount == cappedDiscountInitialAmount);
			AssertEquals("Precondition", true, cappedDiscount.L5_StartDate.IsEmpty);

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();

			OdplBill.OnInvoiceFactorySaving(invoice);
			invoiceFactory.Save();

			AssertEquals("Capped discount was updated", false, cappedDiscount.L5_BreakAmount == cappedDiscountInitialAmount);
			AssertEquals("Date range was updated", false, cappedDiscount.L5_StartDate.IsEmpty);
		}

		public void TestGetGroupSummarySections()
		{
			SummarySection[] summarySections = OdplBill.GetGroupSummarySections();
			AssertEquals("sections", 1, summarySections.Length);

			SummarySection productionSection = summarySections[0];
			AssertEquals(OdplBill.SystemDescription + " Production Group Summary", productionSection.Header.MainDescription);
			AssertEquals(2, productionSection.Lines.Count);

			AssertEquals(ChildOrganisation.OH_Code + " (AAA-BBB-AAA)", productionSection.Lines[0].MainDescription);
			AssertEquals("40.00", productionSection.Lines[0].Amount);

			AssertEquals(Organisation.OH_Code + " (AAA-SYD-AAA)", productionSection.Lines[1].MainDescription);
			AssertEquals("100.00", productionSection.Lines[1].Amount);
		}

		public void TestGetGroupSummarySections_PurchasedLicenceUnits()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "DDD", "SYD", "DDD");
			var org = licHeader.Company.Header;
			var childLic = BillingTestHelper.CreateDependentLicence(licHeader, "EEE");
			var childOrg = childLic.Company.Header;
			var prices = BillingTestHelper.CreatePriceList(org.LicCompany);

			OdplUsage odplUsage1 = CreateUsage(licHeader);
			OdplUsageTest.AddModuleUsage(odplUsage1, "AAA", 50, 100, 20);

			OdplUsage odplUsage2 = CreateUsage(childLic);
			OdplUsageTest.AddModuleUsage(odplUsage2, "AAA", 60, 100, 20);

			odplUsage1.PurchasedLicenceUnits = 1111;
			odplUsage1.LicenceUnitRate = 5;

			var bill = new OdplSystemBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });

			SummarySection[] summarySections = bill.GetGroupSummarySections();
			AssertEquals("sections", 1, summarySections.Length);

			SummarySection productionSection = summarySections[0];
			AssertEquals("-5,555.00", productionSection.Header.TotalAmountAdjustment);
			productionSection.Header.TotalLicenceUnitsAdjustment = "-1111";

			// usage amount is 11000/2200 (currency/licence units)
			// after adjust is 5445/1089
			AssertEquals("5,445.00", productionSection.Header.TotalAmountAfterAdjustment);
			AssertEquals("1089", productionSection.Header.TotalLicenceUnitsAfterAdjustment);
		}

		public void TestGetDiscountSummarySections()
		{
			SummarySection[] summarySections = OdplBill.GetDiscountSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);
			AssertEquals("Single summary line in section", 1, summarySections[0].Lines.Count);

			SummaryLine summaryLine = summarySections[0].Lines[0];
			AssertEquals(OdplBill.DiscountCalculation.DiscountDescriptions.ToArray()[0], summaryLine.MainDescription);

			SummaryLine summaryHeader = summarySections[0].Header;
			AssertEquals("On Demand Amount Calculations Applied", summaryHeader.MainDescription);
		}

		public void TestValidateAll()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			LicenceHeader licHeader = organisation.LicCompany.LicHeadersForAllDatabases[0];

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var chargeableUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2010, 10, 1), licHeader, 10);

			OdplUsage odplUsage = CreateUsage(licHeader);
			odplUsage.ChargeableUsagePKs.Add(chargeableUsage1.PK);
			OdplUsageTest.AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 5, 100);
			OdplUsageTest.AddModuleUsage(odplUsage, BillingConstants.NonProductionDatabase.CoreModuleCode, 10, 10);

			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 11, 30));
			OdplSystemBill odplBill = new OdplSystemBill(context);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage });

			licHeader.LA_AgreedLiveDate = ZDateTime.Empty;
			odplBill.ValidateAll(odplBill);
			AssertHasRowWarning(odplBill, "Blank site-live usage from " + organisation.OH_Code + " (Server DDD) is included.");

			var chargeableUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2016, 11, 1), licHeader, 10);
			odplUsage = CreateUsage(licHeader, new ZDateTime(2016, 11, 1));
			odplUsage.ChargeableUsagePKs.Add(chargeableUsage2.PK);
			OdplUsageTest.AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 5, 100);
			odplBill = new OdplSystemBill(context);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage });
			odplBill.ValidateAll(odplBill);
			AssertHasRowWarning(odplBill, "Database DDD is missing STL Active User data");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2016, 11, 1), licHeader, 15);
			Factory.Save();
			odplBill = new OdplSystemBill(context);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage });
			odplBill.ValidateAll(odplBill);
			AssertNoRowWarningContaining(odplBill, "Database DDD is missing STL Active User data");
		}

		public void TestValidateAll_PriceHeader()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "SYD", "DDD");
			var organisation = lic.Company.Header;
			var licChild = BillingTestHelper.CreateDependentLicence(lic, "DD1");
			var childOrganisation = licChild.Company.Header;
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK);

			OdplUsage odplUsage1 = CreateUsage(lic);
			OdplUsage odplUsage2 = CreateUsage(licChild);
			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			odplBill.ValidateAll(odplBill);
			AssertHasRowErrorContaining(odplBill, ": No pricelist found for " + organisation.OH_Code);
			AssertHasRowErrorContaining(odplBill, ": No pricelist found for " + childOrganisation.OH_Code);

			ClientLicencePriceHeader childPriceHeader = childOrganisation.LicCompany.PriceHeaders.AddNew();
			childPriceHeader.L6_RX_NKCurrency = "AUD";
			childPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			childPriceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;
			odplUsage1 = CreateUsage(lic);
			odplUsage2 = CreateUsage(licChild);
			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			odplBill.ValidateAll(odplBill);
			AssertHasRowErrorContaining(odplBill, ": No pricelist found for " + organisation.OH_Code);
			AssertNoRowErrorContaining(odplBill, ": No pricelist found for " + childOrganisation.OH_Code);

			ClientLicencePriceHeader parentPriceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			parentPriceHeader.L6_RX_NKCurrency = "AUD";
			parentPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			parentPriceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;
			odplUsage1 = CreateUsage(lic);
			odplUsage2 = CreateUsage(licChild);
			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			odplBill.ValidateAll(odplBill);
			AssertNoRowErrors(odplBill);

			childPriceHeader.L6_LicenceUnitRate = 1.1234m;
			parentPriceHeader.L6_LicenceUnitRate = 5.6789m;
			odplUsage1 = CreateUsage(lic);
			odplUsage2 = CreateUsage(licChild);
			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			odplBill.ValidateAll(odplBill);
			AssertHasRowErrorContaining(odplBill, ZString.Format(": pricelists for {0} has multiple licence unit rate i.e. {1} and {2}", childOrganisation.OH_Code, 5.6789m, 1.1234m));
		}

		public void TestValidateAll_UsageWithoutPriceHeader()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "SYD", "DDD");
			var organisation = lic.Company.Header;
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK);
			ClientLicencePriceHeader childPriceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			childPriceHeader.L6_RX_NKCurrency = "AUD";
			childPriceHeader.L6_ValidFrom = new ZDateTime(2010, 5, 1);
			childPriceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;

			OdplUsage odplUsage1 = CreateUsage(lic);
			OdplUsage odplUsage2 = CreateUsage(lic, new ZDateTime(2010, 1, 1));
			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			odplBill.ValidateAll(odplBill);
			AssertHasRowErrorContaining(odplBill, ": No currency specified for " + organisation.OH_Code);
			AssertHasRowErrorContaining(odplBill, ": Different price currencies exist for the billing group");
			AssertHasRowErrorContaining(odplBill, ": No pricelist found for " + organisation.OH_Code);
		}

		public void TestValidateAll_DiscountOver100Percent()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "XXX");
			var org = lic.Company.Header;
			BillingTestHelper.SetInvoicing(org, Env.CurrentBranch.PK, "AUD");

			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);

			ClientLicenceBillingDiscount discount1 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount1.L5_Type = BillingConstants.DiscountType.Special;
			discount1.L5_Discount = 10m;

			ClientLicenceBillingDiscount discount2 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount2.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount2.L5_Type = BillingConstants.DiscountType.Special;
			discount2.L5_Discount = 100m;

			OdplUsage usage = CreateUsage(lic);
			OdplUsageTest.AddModuleUsage(usage, BillingConstants.CoreModuleCode, 50);
			usage.CalculateAmount();

			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { usage });
			odplBill.ValidateAll(odplBill);
			AssertHasRowErrorContaining(odplBill, ": Discount over 100%");
		}

		public void TestCoreOnDemandUsers()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "SYD", "DDD");
			var organisation = lic.Company.Header;
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK);

			OdplUsage odplUsage1 = CreateUsage(lic);
			OdplUsageTest.AddModuleUsage(odplUsage1, BillingConstants.CoreModuleCode, 7, 100);
			OdplUsageTest.AddModuleUsage(odplUsage1, "ACC", 99, 100);

			OdplUsage odplUsage2 = CreateUsage(lic);
			OdplUsageTest.AddModuleUsage(odplUsage2, "ACC", 55, 100);

			OdplUsage odplUsage3 = CreateUsage(lic);
			OdplUsageTest.AddModuleUsage(odplUsage3, BillingConstants.CoreModuleCode, 11, 100);
			OdplUsageTest.AddModuleUsage(odplUsage3, "FOR", 66, 100);

			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			AssertEquals(0, odplBill.CoreOnDemandUsers);

			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1 });
			AssertEquals(7, odplBill.CoreOnDemandUsers);

			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			AssertEquals(7, odplBill.CoreOnDemandUsers);

			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2, odplUsage3 });
			AssertEquals(18, odplBill.CoreOnDemandUsers);
		}

		public void TestMixedAmount()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "SYD", "DDD");
			var organisation = lic.Company.Header;
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK);

			OdplUsage odplUsage1 = CreateUsage(lic);
			var module1 = OdplUsageTest.AddModuleUsage(odplUsage1, BillingConstants.CoreModuleCode, 7, 100, 20);
			module1.PurchasedStaffCount = 3;
			var module2 = OdplUsageTest.AddModuleUsage(odplUsage1, "ACC", 99, 100, 20);

			OdplUsage odplUsage2 = CreateUsage(lic);
			var module3 = OdplUsageTest.AddModuleUsage(odplUsage2, "ACC", 55, 100, 20);

			OdplUsage odplUsage3 = CreateUsage(lic);
			var module4 = OdplUsageTest.AddModuleUsage(odplUsage3, BillingConstants.CoreModuleCode, 11, 100, 25);
			module4.PurchasedStaffCount = 99;
			var module5 = OdplUsageTest.AddModuleUsage(odplUsage3, "FOR", 66, 100, 25);

			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			AssertEquals(0m, odplBill.MixedAmountAsMoney);
			AssertEquals(0m, odplBill.MixedAmountAsLicenceUnits);

			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1 });
			AssertEquals(700m + 9900m, odplBill.MixedAmountAsMoney);
			AssertEquals(140m + 1980m, odplBill.MixedAmountAsLicenceUnits);

			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
			AssertEquals(700m + 9900m + 5500m, odplBill.MixedAmountAsMoney);
			AssertEquals(140m + 1980m + 1100m, odplBill.MixedAmountAsLicenceUnits);

			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2, odplUsage3 });
			AssertEquals(700m + 9900m + 5500m + 66 * 100m + 99 * 100m, odplBill.MixedAmountAsMoney);
			AssertEquals(140m + 1980m + 1100m + 66 * 25m + 99 * 25m, odplBill.MixedAmountAsLicenceUnits);
		}

		public void TestCreateRevenueBreakdown()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var hybridChargeCode = BillingTestHelper.CreateChargeCode(Factory, null, EDIDataRegistry.Instance.OdplHybridUsageChargeCode.Value);
			var hybridDiscountChargeCode = BillingTestHelper.CreateChargeCode(Factory, null, EDIDataRegistry.Instance.OdplHybridDiscountChargeCode.Value);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "DDD", "DDD");
			var lic2 = BillingTestHelper.CreateDependentLicence(lic1, "EEE");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);

			ClientLicenceBillingDiscount discount = lic1.Company.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 20m;

			var priceHeader = BillingTestHelper.CreatePriceList(lic1);
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			var org = lic1.Company.Header;

			var core = lic1.GetCoreModule();
			core.LM_LicenceType = LicenceTypes.Codes.ODM;
			core.LM_UserCount = 1;

			Factory.Save();
			var chargeCodePk = BillingInvoicingHelper.GetChargeCodePK(GlbBranch.CurrentBranch, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(GlbBranch.CurrentBranch, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);

			var periodStart = BillingTestHelper.MonthToday;

			OdplUsage odplUsage1 = CreateUsage(lic1, periodStart);
			var moduleUsage1 = OdplUsageTest.AddModuleUsage(odplUsage1, "COR", 50);
			var moduleUsage2 = OdplUsageTest.AddModuleUsage(odplUsage1, "WAR", 30);
			odplUsage1.CalculateAmount();

			OdplUsage odplUsage2 = CreateUsage(lic2, periodStart);
			var moduleUsage3 = OdplUsageTest.AddModuleUsage(odplUsage2, "COR", 20);
			odplUsage2.CalculateAmount();

			var bill = new OdplSystemBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = lic1.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranchPK;
			invoice.AH_GC = Env.CurrentCompanyPK;
			invoice.AH_ExchangeRate = 1m;

			bill.CreateRevenueBreakdown(invoice, -10);

			var allBilledUsage = Factory.Load<EdiBilledUsage>(new ZQuery());
			AssertEquals(3, allBilledUsage.Length);
			{
				var billed = allBilledUsage.Single(x => x.BU9_LCC == lic1.ClientCompany.PK && x.BU9_UsageSubCode == "COR");
				var units = 49m;
				var price = 3.0m;

				CombineAssertions(() =>
				{
					AssertEquals("BU9_AC_AmountChargeCode", hybridChargeCode.PK, billed.BU9_AC_AmountChargeCode);
					AssertEquals("BU9_AC_DiscountChargeCode", hybridDiscountChargeCode.PK, billed.BU9_AC_DiscountChargeCode);
					AssertEquals("BU9_AH_Invoice", invoice.PK, billed.BU9_AH_Invoice);
					AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billed.BU9_BillingModel);
					AssertEquals("BU9_LC", lic1.LA_LC, billed.BU9_LC);
					AssertEquals("BU9_LCC", lic1.ClientCompany.PK, billed.BU9_LCC);
					AssertEquals("BU9_LD", lic1.LA_LD, billed.BU9_LD);

					AssertEquals("BU9_LocalAmountPreDiscount", units * price, billed.BU9_LocalAmountPreDiscount);
					AssertEquals("BU9_LocalAmountPostDiscount", units * price * 0.72m, billed.BU9_LocalAmountPostDiscount);
					AssertEquals("BU9_LocalProcessingAmount", units * price * 0.8m * -0.1m, billed.BU9_LocalProcessingAmount);

					AssertEquals("BU9_PeriodStart", periodStart, billed.BU9_PeriodStart);
					AssertEquals("BU9_PriceCurrency", "AUD", billed.BU9_PriceCurrency);

					AssertEquals("BU9_TransactionAmountPreDiscount", units * price, billed.BU9_TransactionAmountPreDiscount);
					AssertEquals("BU9_TransactionAmountPostDiscount", units * price * 0.72m, billed.BU9_TransactionAmountPostDiscount);
					AssertEquals("BU9_TransactionProcessingAmount", units * price * 0.8m * -0.1m, billed.BU9_TransactionProcessingAmount);

					AssertEquals("BU9_UnitCount", units, billed.BU9_UnitCount);
					AssertEquals("BU9_UsageCode", "ODM", billed.BU9_UsageCode);
					AssertEquals("BU9_UsageSubCode", "COR", billed.BU9_UsageSubCode);
					AssertEquals("BU9_PriceCode", "COR", billed.BU9_PriceCode);
					AssertEquals("BU9_L7", priceHeader.Items.FindByCode("COR").PK, billed.BU9_L7);

					var billedDiscounts = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billed.PK));
					AssertEquals(1, billedDiscounts.Length);
					AssertEquals(20m, billedDiscounts[0].BD9_Percent);
					AssertEquals(ZGuid.Empty, billedDiscounts[0].BD9_PHD_Discount);
					AssertEquals(29.40m, billedDiscounts[0].BD9_TransactionAmount);
					AssertEquals("SPE", billedDiscounts[0].BD9_Type);
				});
			}

			{
				var billed = allBilledUsage.Single(x => x.BU9_LCC == lic1.ClientCompany.PK && x.BU9_UsageSubCode == "WAR");
				var units = 30m;
				var price = 7.0m;

				CombineAssertions(() =>
				{
					AssertEquals("BU9_AC_AmountChargeCode", hybridChargeCode.PK, billed.BU9_AC_AmountChargeCode);
					AssertEquals("BU9_AC_DiscountChargeCode", hybridDiscountChargeCode.PK, billed.BU9_AC_DiscountChargeCode);
					AssertEquals("BU9_AH_Invoice", invoice.PK, billed.BU9_AH_Invoice);
					AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billed.BU9_BillingModel);
					AssertEquals("BU9_LC", lic1.LA_LC, billed.BU9_LC);
					AssertEquals("BU9_LCC", lic1.ClientCompany.PK, billed.BU9_LCC);
					AssertEquals("BU9_LD", lic1.LA_LD, billed.BU9_LD);

					AssertEquals("BU9_LocalAmountPreDiscount", units * price, billed.BU9_LocalAmountPreDiscount);
					AssertEquals("BU9_LocalAmountPostDiscount", units * price * 0.72m, billed.BU9_LocalAmountPostDiscount);
					AssertEquals("BU9_LocalProcessingAmount", units * price * 0.8m * -0.1m, billed.BU9_LocalProcessingAmount);

					AssertEquals("BU9_PeriodStart", periodStart, billed.BU9_PeriodStart);
					AssertEquals("BU9_PriceCurrency", "AUD", billed.BU9_PriceCurrency);

					AssertEquals("BU9_TransactionAmountPreDiscount", units * price, billed.BU9_TransactionAmountPreDiscount);
					AssertEquals("BU9_TransactionAmountPostDiscount", units * price * 0.72m, billed.BU9_TransactionAmountPostDiscount);
					AssertEquals("BU9_TransactionProcessingAmount", units * price * 0.8m * -0.1m, billed.BU9_TransactionProcessingAmount);

					AssertEquals("BU9_UnitCount", units, billed.BU9_UnitCount);
					AssertEquals("BU9_UsageCode", "ODM", billed.BU9_UsageCode);
					AssertEquals("BU9_UsageSubCode", "WAR", billed.BU9_UsageSubCode);
					AssertEquals("BU9_PriceCode", "WAR", billed.BU9_PriceCode);
					AssertEquals("BU9_L7", priceHeader.Items.FindByCode("WAR").PK, billed.BU9_L7);

					var billedDiscounts = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billed.PK));
					AssertEquals(1, billedDiscounts.Length);
					AssertEquals(20m, billedDiscounts[0].BD9_Percent);
					AssertEquals(ZGuid.Empty, billedDiscounts[0].BD9_PHD_Discount);
					AssertEquals(42.00m, billedDiscounts[0].BD9_TransactionAmount);
					AssertEquals("SPE", billedDiscounts[0].BD9_Type);
				});
			}

			{
				var billed = allBilledUsage.Single(x => x.BU9_LCC == lic2.ClientCompany.PK && x.BU9_UsageSubCode == "COR");
				var units = 20m;
				var price = 3.0m;

				CombineAssertions(() =>
				{
					AssertEquals("BU9_AC_AmountChargeCode", chargeCodePk, billed.BU9_AC_AmountChargeCode);
					AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCodePk, billed.BU9_AC_DiscountChargeCode);
					AssertEquals("BU9_AH_Invoice", invoice.PK, billed.BU9_AH_Invoice);
					AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billed.BU9_BillingModel);
					AssertEquals("BU9_LC", lic2.LA_LC, billed.BU9_LC);
					AssertEquals("BU9_LCC", lic2.ClientCompany.PK, billed.BU9_LCC);
					AssertEquals("BU9_LD", lic2.LA_LD, billed.BU9_LD);

					AssertEquals("BU9_LocalAmountPreDiscount", units * price, billed.BU9_LocalAmountPreDiscount);
					AssertEquals("BU9_LocalAmountPostDiscount", units * price * 0.72m, billed.BU9_LocalAmountPostDiscount);
					AssertEquals("BU9_LocalProcessingAmount", units * price * 0.8m * -0.1m, billed.BU9_LocalProcessingAmount);

					AssertEquals("BU9_PeriodStart", periodStart, billed.BU9_PeriodStart);
					AssertEquals("BU9_PriceCurrency", "AUD", billed.BU9_PriceCurrency);

					AssertEquals("BU9_TransactionAmountPreDiscount", units * price, billed.BU9_TransactionAmountPreDiscount);
					AssertEquals("BU9_TransactionAmountPostDiscount", units * price * 0.72m, billed.BU9_TransactionAmountPostDiscount);
					AssertEquals("BU9_TransactionProcessingAmount", units * price * 0.8m * -0.1m, billed.BU9_TransactionProcessingAmount);

					AssertEquals("BU9_UnitCount", units, billed.BU9_UnitCount);
					AssertEquals("BU9_UsageCode", "ODM", billed.BU9_UsageCode);
					AssertEquals("BU9_UsageSubCode", "COR", billed.BU9_UsageSubCode);
					AssertEquals("BU9_PriceCode", "COR", billed.BU9_PriceCode);
					AssertEquals("BU9_L7", priceHeader.Items.FindByCode("COR").PK, billed.BU9_L7);

					var billedDiscounts = Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, billed.PK));
					AssertEquals(1, billedDiscounts.Length);
					AssertEquals(20m, billedDiscounts[0].BD9_Percent);
					AssertEquals(ZGuid.Empty, billedDiscounts[0].BD9_PHD_Discount);
					AssertEquals(12.00m, billedDiscounts[0].BD9_TransactionAmount);
					AssertEquals("SPE", billedDiscounts[0].BD9_Type);
				});
			}
		}

		public void TestCalculateMinimumFeeContribution()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var user1 = new UsingParty(licence1);

			var licence2 = BillingTestHelper.CreateLicence(Factory, "EEE", "BBB", "MEL");
			var user2 = new UsingParty(licence2);

			Factory.Save();

			var odplUsage1 = new OdplUsage(Factory, user1, new ZDateTime(2016, 5, 1));
			OdplUsageTest.AddModuleUsage(odplUsage1, "COR", 100, 1);
			odplUsage1.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;

			var odplUsage2 = new OdplUsage(Factory, user1, new ZDateTime(2016, 5, 1));
			OdplUsageTest.AddModuleUsage(odplUsage2, "FOR", 50, 1);
			odplUsage2.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;

			var odplUsage3 = new OdplUsage(Factory, user2, new ZDateTime(2016, 5, 1));
			OdplUsageTest.AddModuleUsage(odplUsage3, "COR", 20, 1);
			odplUsage3.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;

			var odplUsage4 = new OdplUsage(Factory, user1, new ZDateTime(2016, 6, 1));
			OdplUsageTest.AddModuleUsage(odplUsage4, "COR", 40, 1);
			odplUsage4.LicenceMode = MonthlyUsageBilling.LicenceModeConstants.Codes.Odpl;

			var systemBill = new OdplSystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2, odplUsage3, odplUsage4 });

			var minimumFeeContribution = systemBill.CalculateMinimumFeeContribution().ToArray();
			AssertEquals(3, minimumFeeContribution.Length);
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence2.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 6, 1)));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewSystemBill();
		}

		protected override OdplSystemBill GetNewSystemBill()
		{
			return new OdplSystemBill(Factory);
		}

		OdplSystemBill OdplBill;
		EDIOrgHeader Organisation;
		EDIOrgHeader ChildOrganisation;
		LicenceHeader LicHeader;
		LicenceHeader ChildLicHeader;

		OdplUsage CreateUsage(LicenceHeader licHeader)
		{
			return CreateUsage(licHeader, new ZDateTime(2010, 10, 1));
		}

		OdplUsage CreateUsage(LicenceHeader lic, ZDateTime periodStart)
		{
			return new OdplUsage(Factory, lic, periodStart, lic.ClientCompany);
		}

		protected override void SetUp()
		{
			base.SetUp();

			LicenceCompany.ClearStandardPricesCompanyCache();

			LicHeader = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			Organisation = LicHeader.Company.Header;

			ClientLicenceBillingDiscount discount = LicHeader.Company.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.L5_BreakAmount = 200m;
			discount.L5_Discount = 10m;

			ClientLicenceBillingDiscount surcharge = LicHeader.Company.SelfBilling.BillingDiscounts.AddNew();
			surcharge.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_Discount = -5m;

			ChildLicHeader = BillingTestHelper.CreateDependentLicence(LicHeader, "BBB");
			ChildOrganisation = ChildLicHeader.Company.Header;

			OdplUsage odplUsage1 = CreateUsage(LicHeader);
			OdplUsageTest.AddModuleUsage(odplUsage1, "AAA", 100, 1);

			OdplUsage odplUsage2 = CreateUsage(ChildLicHeader);
			OdplUsageTest.AddModuleUsage(odplUsage2, "AAA", 40, 1);

			OdplBill = new OdplSystemBill(Factory);
			OdplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage1, odplUsage2 });
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
