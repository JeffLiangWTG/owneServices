using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SystemBill))]
	internal class SystemBillTest : SystemBillTestCase<SystemBill>
	{
		public void TestDefaults()
		{
			SystemBill systemBill = new SystemBill(Factory);
			AssertEquals(ZGuid.Empty, systemBill.OrganisationPK);
			AssertEquals(ZDateTime.Empty, systemBill.PeriodStart);
			AssertEquals(ZString.Empty, systemBill.SystemCode);

			AssertNull(systemBill.Organisation);

			AssertEquals(0, systemBill.SystemUsages.Count);

			AssertEquals(0m, systemBill.Amount);
			AssertEquals(0m, systemBill.DiscountAmount);

			AssertEquals(ZString.Empty, systemBill.OrganisationCode);
			AssertEquals(ZString.Empty, systemBill.SystemDescription);
			AssertEquals(ZString.Empty, systemBill.Information);

			AssertEquals(0, systemBill.GetDiscountSummarySections().Length);
		}

		public void TestPopulateFromSystemUsages()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			DummyUsage systemUsage = new DummyUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01));
			systemUsage.Amount_Exposed = 10m;

			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			AssertEquals("Precondition", true, systemBill.TextNotifications.IsEmpty);

			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage });
			AssertEquals(true, systemBill.MethodWasCalled("CalculateGroupAmounts"));

			AssertEquals(organisation.PK, systemBill.OrganisationPK);
			AssertEquals(new ZDateTime(2010, 10, 01), systemBill.PeriodStart);
			AssertEquals("DUM", systemBill.SystemCode);

			AssertEquals(organisation, systemBill.Organisation);

			AssertEquals(1, systemBill.SystemUsages.Count);
			AssertEquals(systemUsage, systemBill.SystemUsages[0]);

			AssertEquals(10m, systemBill.Amount);
			AssertEquals(1m, systemBill.DiscountAmount);
			AssertEquals(9m, systemBill.TotalAmount);

			EDIOrgHeader anotherOrganisation = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			systemUsage = new DummyUsage(Factory, new UsingParty(anotherOrganisation), EdiDateTest.MonthToday);
			systemUsage.Amount_Exposed = -20m;

			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage });
			AssertEquals("OrganisationPK has been changed", anotherOrganisation.PK, systemBill.OrganisationPK);
			AssertEquals("Organisation has been changed", anotherOrganisation, systemBill.Organisation);
			AssertEquals(-20m, systemBill.Amount);
			AssertEquals(1m, systemBill.DiscountAmount);
			AssertEquals(-21m, systemBill.TotalAmount);
		}

		public void TestPopulateFromSystemUsages_MultipleUsages()
		{
			EDIOrgHeader organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			DummyUsage systemUsage1 = new DummyUsage(Factory, new UsingParty(organisation1), new ZDateTime(2010, 09, 01), 1m);

			EDIOrgHeader organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			DummyUsage systemUsage2 = new DummyUsage(Factory, new UsingParty(organisation2), new ZDateTime(2010, 11, 01), 2m);

			EDIOrgHeader organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC");
			DummyUsage systemUsage3 = new DummyUsage(Factory, new UsingParty(organisation3), new ZDateTime(2010, 10, 01), 4m);

			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2, systemUsage3 });

			AssertEquals("Most recent period start", new ZDateTime(2010, 11, 01), systemBill.PeriodStart);
			AssertEquals("All system usages has been added", 3, systemBill.SystemUsages.Count);
			AssertEquals("Group amount is the sum of all amounts", 7m, systemBill.Amount);
			AssertEquals(1m, systemBill.DiscountAmount);
			AssertEquals(6m, systemBill.TotalAmount);
		}

		public void TestHasUsagesForEarlierPeriods()
		{
			EDIOrgHeader organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			DummyUsage systemUsage1 = new DummyUsage(Factory, new UsingParty(organisation1), new ZDateTime(2010, 11, 01), 1m);

			EDIOrgHeader organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			DummyUsage systemUsage2 = new DummyUsage(Factory, new UsingParty(organisation2), new ZDateTime(2010, 11, 01), 2m);

			SystemBill systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2 });
			AssertEquals(false, systemBill.HasUsagesForEarlierPeriods);

			systemUsage2 = new DummyUsage(Factory, new UsingParty(organisation2), new ZDateTime(2010, 12, 01), 2m);
			systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2 });
			AssertEquals(true, systemBill.HasUsagesForEarlierPeriods);
		}

		public void TestCalculateGroupAmounts()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);
			DummyUsage systemUsage1 = new DummyUsage(Factory, user, new ZDateTime(2010, 10, 01), 8m);
			DummyUsage systemUsage2 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 16m);
			DummyUsage systemUsage3 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 32m);

			SystemBill systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2, systemUsage3 });

			AssertEquals("Amount", 8m + 16m + 32m, systemBill.Amount);
			AssertEquals("Discount", 0m, systemBill.DiscountAmount);
		}

		public void TestGetGeneralSummarySections()
		{
			var systemBill = new SystemBill(Factory);
			AssertEquals(0, systemBill.GetGeneralSummarySections(ZGuid.NewZGuid()).Length);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var organisation = lic.Company.Header;
			var licDb2 = BillingTestHelper.CreateAnotherDatabase(lic, "SC2");
			var user = new UsingParty(lic, lic.Database.ClientCompanies[0]);
			DummyUsage systemUsage1 = new DummyUsage(Factory, user, new ZDateTime(2010, 10, 01), 8m);
			DummyUsage systemUsage2 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 16m);

			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2 });
			var sections = systemBill.GetGeneralSummarySections(organisation.PK);
			AssertEquals("a section for each usage", 2, sections.Length);

			DummyUsage systemUsage3 = new DummyUsage(Factory, new UsingParty(licDb2, licDb2.Database.ClientCompanies[0]), new ZDateTime(2010, 10, 01), 32m, "DUM");
			systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2, systemUsage3 });
			sections = systemBill.GetGeneralSummarySections(organisation.PK);
			AssertEquals("a section for each usage", 3, sections.Length);
			AssertEquals("Dummy (AAA-SYD-AAA) Oct 2010", sections[0].Header.MainDescription);
			AssertEquals("Dummy (AAA-SYD-AAA) Nov 2010", sections[1].Header.MainDescription);
			AssertEquals("Dummy (AAA-SYD-SC2) Oct 2010", sections[2].Header.MainDescription);
			AssertEquals("SYD Co (AAA-SYD-AAA)", sections[0].Header.ClientCompanyDescription);
			AssertEquals("SYD Co (AAA-SYD-AAA)", sections[1].Header.ClientCompanyDescription);
			AssertEquals("SYD Co (AAA-SYD-SC2)", sections[2].Header.ClientCompanyDescription);
		}

		public void TestHasLicenceUnits()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");

			var systemUsage1 = new DummyUsageWithLicenceUnits(Factory, new UsingParty(org), new ZDateTime(2010, 10, 01), 8m, "DU1", 1m);
			var systemUsage2 = new DummyUsage(Factory, new UsingParty(org), new ZDateTime(2010, 10, 01), 8m, "DU2");

			var systemBill1 = new SystemBill(Factory);
			systemBill1.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1 });
			AssertEquals("systemBill1", true, systemBill1.HasLicenceUnits);

			var systemBill2 = new SystemBill(Factory);
			systemBill2.PopulateFromSystemUsages(new SystemUsage[] { systemUsage2 });
			AssertEquals("systemBill2", false, systemBill2.HasLicenceUnits);

			var systemBill3 = new SystemBill(Factory);
			systemBill3.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2 });
			AssertEquals("systemBill3", true, systemBill3.HasLicenceUnits);
		}

		public void TestGetGroupSummarySections()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "PRD");
			var licDb2 = BillingTestHelper.CreateAnotherDatabase(lic1, "SC2");
			var user1 = new UsingParty(lic1);
			var organisation1 = lic1.Company.Header;
			ClientLicencePriceHeader priceHeader1 = organisation1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_ValidFrom = new ZDate(2000, 1, 1);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var organisation2 = lic2.Company.Header;
			ClientLicencePriceHeader priceHeader2 = organisation2.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_ValidFrom = new ZDate(2000, 1, 1);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			var systemUsage11 = new DummyUsageWithLicenceUnits(Factory, user1, new ZDateTime(2010, 10, 01), 8.10m, 12);
			var systemUsage21 = new DummyUsageWithLicenceUnits(Factory, new UsingParty(lic2), new ZDateTime(2010, 10, 01), 32.30m, 9);

			SystemBill systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage11, systemUsage21 });

			SummarySection[] summarySections = systemBill.GetGroupSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);

			SummarySection summarySection = summarySections[0];
			AssertEquals(" Group Summary", summarySection.Header.MainDescription);
			AssertEquals("40.40", summarySection.Header.TotalAmount);
			AssertEquals("Total", summarySection.Header.TotalDescription);

			AssertEquals("2 lines", 2, summarySection.Lines.Count);
			AssertSummaryLine(summarySections[0].Lines[0], systemUsage11.Organisation.OH_Code + " (" + systemUsage11.LicenceNineCode + ")", "8.10", "12");
			AssertSummaryLine(summarySections[0].Lines[1], systemUsage21.Organisation.OH_Code + " (" + systemUsage21.LicenceNineCode + ")", "32.30", "9");
			var systemUsage12 = new DummyUsageWithLicenceUnits(Factory, user1, new ZDateTime(2010, 11, 01), 16.20m, 7);
			var systemUsage13 = new DummyUsageWithLicenceUnits(Factory, user1, new ZDateTime(2010, 11, 01), 0m, 10);

			systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage11, systemUsage12, systemUsage13, systemUsage21 });

			summarySections = systemBill.GetGroupSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);

			summarySection = summarySections[0];
			AssertEquals(" Group Summary", summarySection.Header.MainDescription);
			AssertEquals("56.60", summarySection.Header.TotalAmount);
			AssertEquals("28", summarySection.Header.TotalLicenceUnits);
			AssertEquals("Total", summarySection.Header.TotalDescription);
			AssertEquals("Total Price (AUD)", summarySection.Header.AmountDescription);

			AssertEquals("3 lines: not including usage with zero amount and showing months, as we have usages from different months", 3, summarySection.Lines.Count);
			AssertSummaryLine(summarySections[0].Lines[0], systemUsage11.Organisation.OH_Code + " (" + systemUsage11.LicenceNineCode + ") " + systemUsage11.PeriodStartAsText, "8.10", "12");
			AssertSummaryLine(summarySections[0].Lines[1], systemUsage12.Organisation.OH_Code + " (" + systemUsage12.LicenceNineCode + ") " + systemUsage12.PeriodStartAsText, "16.20", "7");
			AssertSummaryLine(summarySections[0].Lines[2], systemUsage21.Organisation.OH_Code + " (" + systemUsage21.LicenceNineCode + ") " + systemUsage21.PeriodStartAsText, "32.30", "9");

			systemUsage11.CurrencyCode_Exposed = "NZD";
			systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage11 });
			AssertEquals("Total", "Total", systemBill.GetGroupSummarySections().First().Header.TotalDescription);
			AssertEquals("Currency code in summary", "Total Price (NZD)", systemBill.GetGroupSummarySections().First().Header.AmountDescription);

			// different servers
			var systemUsage14 = new DummyUsageWithLicenceUnits(Factory, new UsingParty(licDb2), new ZDateTime(2010, 10, 01), 64.40m, "DUM", 64);
			systemUsage11.CurrencyCode_Exposed = "AUD";
			systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage11, systemUsage14 });
			summarySections = systemBill.GetGroupSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);
			AssertSummaryLine(summarySections[0].Lines[0], systemUsage11.Organisation.OH_Code + " (" + systemUsage11.LicenceNineCode + ")", "8.10", "12");
			AssertSummaryLine(summarySections[0].Lines[1], systemUsage12.Organisation.OH_Code + " (" + systemUsage14.LicenceNineCode + ")", "64.40", "64");

			// should show LicenceNineCode even for one group summary line
			var systemUsage15 = new DummyUsageWithLicenceUnits(Factory, new UsingParty(licDb2), new ZDateTime(2010, 10, 01), 64.40m, "DUM", 64);
			systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage15 });
			summarySections = systemBill.GetGroupSummarySections();
			AssertEquals(1, summarySections.Length);
			AssertEquals(1, summarySections[0].Lines.Count);
			AssertSummaryLine(summarySections[0].Lines[0], systemUsage15.Organisation.OH_Code + " (" + systemUsage15.LicenceNineCode + ")", "64.40", "64");
		}

		public void TestGetGroupSummarySections_OrderBy()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ZZZ", "SYD", "ZZZ");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "MMM", "SYD", "MMM");
			var user1 = new UsingParty(lic1);
			var user2 = new UsingParty(lic2);
			var user3 = new UsingParty(lic3);
			var organisation1 = lic1.Company.Header;
			var organisation2 = lic2.Company.Header;
			var organisation3 = lic3.Company.Header;
			Factory.Save();

			DummyUsage systemUsage11 = new DummyUsage(Factory, user1, new ZDateTime(2010, 10, 1), 1m);
			DummyUsage systemUsage12 = new DummyUsage(Factory, user1, new ZDateTime(2010, 11, 1), 1m);
			DummyUsage systemUsage21 = new DummyUsage(Factory, user2, new ZDateTime(2010, 10, 1), 1m);
			DummyUsage systemUsage22 = new DummyUsage(Factory, user2, new ZDateTime(2010, 11, 1), 1m);
			DummyUsage systemUsage31 = new DummyUsage(Factory, user3, new ZDateTime(2010, 10, 1), 1m);
			DummyUsage systemUsage32 = new DummyUsage(Factory, user3, new ZDateTime(2010, 11, 1), 1m);

			SystemBill systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage11, systemUsage12, systemUsage21, systemUsage22, systemUsage31, systemUsage32 });

			SummarySection[] summarySections = systemBill.GetGroupSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);

			SummarySection summarySection = summarySections[0];

			AssertEquals("6 lines", 6, summarySection.Lines.Count);
			AssertEquals(organisation2.OH_Code + " (AAA-SYD-AAA) Oct 2010", summarySections[0].Lines[0].MainDescription);
			AssertEquals(organisation2.OH_Code + " (AAA-SYD-AAA) Nov 2010", summarySections[0].Lines[1].MainDescription);
			AssertEquals(organisation3.OH_Code + " (MMM-SYD-MMM) Oct 2010", summarySections[0].Lines[2].MainDescription);
			AssertEquals(organisation3.OH_Code + " (MMM-SYD-MMM) Nov 2010", summarySections[0].Lines[3].MainDescription);
			AssertEquals(organisation1.OH_Code + " (ZZZ-SYD-ZZZ) Oct 2010", summarySections[0].Lines[4].MainDescription);
			AssertEquals(organisation1.OH_Code + " (ZZZ-SYD-ZZZ) Nov 2010", summarySections[0].Lines[5].MainDescription);
		}

		void AssertSummaryLine(SummaryLine summaryLine, ZString description, ZString amount, string licUsage = "")
		{
			AssertEquals(description, summaryLine.MainDescription);
			AssertEquals(amount, summaryLine.Amount);
			if (!string.IsNullOrEmpty(licUsage))
			{
				AssertEquals(licUsage, summaryLine.TotalLicenceUnits);
			}
		}

		public void TestGetDiscountSummarySections()
		{
			SystemBill systemBill = new SystemBill(Factory);
			AssertEquals(0, systemBill.GetDiscountSummarySections().Length);
		}

		public void TestCreateInvoiceLines()
		{
			// No currency conversion

			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK, "AUD");

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			DummyUsage systemUsage = new DummyUsage(Factory, new UsingParty(organisation), EdiDateTest.MonthToday, 100m);
			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage });

			AssertEquals("Amount", 100m, systemBill.Amount);
			AssertEquals("Discount", 1m, systemBill.DiscountAmount);

			List<SystemBill.BillLine> lines = new List<SystemBill.BillLine>();
			systemBill.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals("Amount and discount lines", 2, lines.Count);
			AssertAmountLine(lines[0], 100m, systemBill.GetAmountChargeCodeName(systemUsage), " Usage");
			AssertAmountLine(lines[1], -1m, systemBill.GetDiscountChargeCodeName(systemUsage), " Discount");
		}

		public void TestCreateInvoiceLines_UsageAmountsZeroAndTotalAmountNonZero()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK, "AUD");

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			DummyUsage systemUsage = new DummyUsage(Factory, new UsingParty(organisation), EdiDateTest.MonthToday, 0m);
			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage });
			systemBill.SetAmountForTesting(100);

			AssertEquals("Amount", 100m, systemBill.Amount);
			AssertEquals("Discount", 1m, systemBill.DiscountAmount);

			List<SystemBill.BillLine> lines = new List<SystemBill.BillLine>();
			systemBill.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals("Amount and discount lines", 2, lines.Count);
			AssertAmountLine(lines[0], 100m, systemBill.GetAmountChargeCodeName(systemUsage), " Usage");
			AssertAmountLine(lines[1], -1m, systemBill.GetDiscountChargeCodeName(systemUsage), " Discount");
		}

		public void TestCreateInvoiceLines_IsProcessingFeeExempt()
		{
			List<SystemBill.BillLine> lines = new List<SystemBill.BillLine>();
			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);

			// RSH and FAX are configured exempt in the registry
			lines.Add(new SystemBill.BillLine(70, new SystemBill.TaxGroup(null, null), "AUD", "CHARGECODE", "Desc FEE", 1, "ODM", Env.CurrentDepartment.PK));
			lines.Add(new SystemBill.BillLine(70, new SystemBill.TaxGroup(null, null), "AUD", "CHARGECODE", "Desc RSH", 1, "RSH", Env.CurrentDepartment.PK));
			lines.Add(new SystemBill.BillLine(70, new SystemBill.TaxGroup(null, null), "AUD", "CHARGECODE", "Desc FAX", 1, "FAX", Env.CurrentDepartment.PK));

			systemBill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals("ODM IsProcessingFeeExempt", false, lines[0].IsProcessingFeeExempt);
			AssertEquals("RSH IsProcessingFeeExempt", true, lines[1].IsProcessingFeeExempt);
			AssertEquals("FAX IsProcessingFeeExempt", true, lines[2].IsProcessingFeeExempt);
		}

		void AssertAmountLine(SystemBill.BillLine invoiceLine, ZDecimal amount, ZString amountChargeCodeName, ZString description)
		{
			AssertEquals(amount, invoiceLine.Amount);
			AssertEquals(amountChargeCodeName, invoiceLine.ChargeCodeName);
			AssertEquals(description, invoiceLine.Description);
		}

		public void TestOnInvoiceFactorySaving()
		{
			SystemBill systemBill = new SystemBill(Factory);
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{
				systemBill.OnInvoiceFactorySaving(null);
			});

			ClientChargeableUsage chargeableUsage1 = Factory.NewWithValidTestData<ClientChargeableUsage>();
			ClientChargeableUsage chargeableUsage2 = Factory.NewWithValidTestData<ClientChargeableUsage>();
			ClientChargeableUsage chargeableUsage3 = Factory.NewWithValidTestData<ClientChargeableUsage>();
			ClientChargeableUsage chargeableUsage4 = Factory.NewWithValidTestData<ClientChargeableUsage>();
			chargeableUsage1.U1_UnitCount = 1;
			chargeableUsage2.U1_UnitCount = 2;
			chargeableUsage3.U1_UnitCount = 3;
			chargeableUsage4.U1_UnitCount = 4;
			chargeableUsage4.U1_LC = ZGuid.Empty;
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			Factory.Save();

			var user = new UsingParty(organisation);
			DummyUsage systemUsage1 = new DummyUsage(Factory, user, new ZDateTime(2010, 10, 01));
			systemUsage1.ChargeableUsagePKs.Add(chargeableUsage1.PK);

			DummyUsage systemUsage2 = new DummyUsage(Factory, user, new ZDateTime(2010, 10, 01));
			systemUsage2.ChargeableUsagePKs.Add(chargeableUsage2.PK);
			systemUsage2.ChargeableUsagePKs.Add(chargeableUsage3.PK);

			systemBill = new SystemBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2 });

			AssertEquals("Precondition", true, chargeableUsage1.U1_AH_Invoice.IsEmpty);
			AssertEquals("Precondition", true, chargeableUsage2.U1_AH_Invoice.IsEmpty);
			AssertEquals("Precondition", true, chargeableUsage3.U1_AH_Invoice.IsEmpty);
			AssertEquals("Precondition", true, chargeableUsage4.U1_AH_Invoice.IsEmpty);
			AssertEquals("Precondition", 0m, chargeableUsage1.U1_InvoicedUnitCount);
			AssertEquals("Precondition", 0m, chargeableUsage2.U1_InvoicedUnitCount);
			AssertEquals("Precondition", 0m, chargeableUsage3.U1_InvoicedUnitCount);
			AssertEquals("Precondition", 0m, chargeableUsage4.U1_InvoicedUnitCount);

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();

			systemBill.OnInvoiceFactorySaving(invoice);
			AssertEquals("Factory not saved - no changes", true, chargeableUsage1.U1_AH_Invoice.IsEmpty);
			AssertEquals("Factory not saved - no changes", true, chargeableUsage2.U1_AH_Invoice.IsEmpty);
			AssertEquals("Factory not saved - no changes", true, chargeableUsage3.U1_AH_Invoice.IsEmpty);
			AssertEquals("Factory not saved - no changes", true, chargeableUsage4.U1_AH_Invoice.IsEmpty);

			invoice.Factory.Save();
			AssertEquals("U1_AH_Invoice was set", invoice.PK, chargeableUsage1.U1_AH_Invoice);
			AssertEquals("U1_AH_Invoice was set", invoice.PK, chargeableUsage2.U1_AH_Invoice);
			AssertEquals("U1_AH_Invoice was set", invoice.PK, chargeableUsage3.U1_AH_Invoice);
			AssertEquals("U1_AH_Invoice not set - this chargeable usage does not belong to any of system usages", ZGuid.Empty, chargeableUsage4.U1_AH_Invoice);

			AssertEquals("U1_InvoicedUnitCount was set", chargeableUsage1.U1_UnitCount, chargeableUsage1.U1_InvoicedUnitCount);
			AssertEquals("U1_InvoicedUnitCount was set", chargeableUsage2.U1_UnitCount, chargeableUsage2.U1_InvoicedUnitCount);
			AssertEquals("U1_InvoicedUnitCount was set", chargeableUsage3.U1_UnitCount, chargeableUsage3.U1_InvoicedUnitCount);
			AssertEquals("U1_InvoicedUnitCount not set - this chargeable usage does not belong to any of system usages", 0m, chargeableUsage4.U1_InvoicedUnitCount);

			AssertEquals("U1_LC set", user.LicenceCompanyPK, chargeableUsage1.U1_LC);
			AssertEquals("U1_LC set", user.LicenceCompanyPK, chargeableUsage2.U1_LC);
			AssertEquals("U1_LC set", user.LicenceCompanyPK, chargeableUsage3.U1_LC);
			AssertEquals("U1_LC not set", ZGuid.Empty, chargeableUsage4.U1_LC);
		}

		public void TestValidateAll()
		{
			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			AssertEquals("Precondition", true, systemBill.TextNotifications.IsEmpty);

			systemBill.ValidateAll(systemBill);
			AssertEquals(true, systemBill.MethodWasCalled("ValidateUnitPrice"));
			AssertEquals(true, systemBill.MethodWasCalled("ValidateAllCore"));
		}

		public void TestValidateAll_ValidateCurrencyCode()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK, "AUD");

			DummyUsage systemUsage1 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 8m);
			DummyUsage systemUsage2 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 16m);
			DummyUsage systemUsage3 = new DummyUsage(Factory, user, new ZDateTime(2010, 11, 01), 32m);

			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2, systemUsage3 });

			systemBill.ValidateAll(systemBill);
			AssertNoRowErrors(systemBill);

			systemUsage2.CurrencyCode_Exposed = "XXX";
			systemBill.ClearAllNotifications();
			systemBill.ValidateAll(systemBill);
			AssertHasRowErrorContaining(systemBill, ": Different price currencies exist for the billing group.");

			systemBill.CanHaveDifferentCurrenciesInTheGroup_Exposed = true;
			systemBill.ClearAllNotifications();
			systemBill.ValidateAll(systemBill);
			AssertNoRowErrors(systemBill);

			systemBill.CanHaveDifferentCurrenciesInTheGroup_Exposed = false;
			systemUsage2.CurrencyCode_Exposed = "AUD";
			systemBill.ClearAllNotifications();
			systemBill.ValidateAll(systemBill);
			AssertNoRowErrors(systemBill);

			systemUsage3.CurrencyCode_Exposed = "";
			systemBill.ClearAllNotifications();
			systemBill.ValidateAll(systemBill);
			AssertHasRowErrorContaining(systemBill, ": No currency specified for " + organisation.OH_Code);
		}

		public void TestValidateAll_MultipleStandardDiscounts()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK);

			EDIOrgHeader childOrganisation = BillingTestHelper.CreateDependentOrganisation(organisation, "DD1");

			ClientLicencePriceHeader childPriceHeader = childOrganisation.LicCompany.PriceHeaders.AddNew();
			childPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			childPriceHeader.L6_UseStandardDiscount = false;
			childPriceHeader.L6_RX_NKCurrency = "AUD";
			childPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			childPriceHeader.L6_DiscountCode = "V1";
			childPriceHeader.L6_PricelistVersion = "19.01";

			ClientLicencePriceHeader parentPriceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			parentPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			parentPriceHeader.L6_UseStandardDiscount = false;
			parentPriceHeader.L6_RX_NKCurrency = "AUD";
			parentPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			parentPriceHeader.L6_DiscountCode = "V2";
			parentPriceHeader.L6_PricelistVersion = "19.03";

			DummyUsage systemUsage1 = new DummyUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 11, 1), 8m);
			DummyUsage systemUsage2 = new DummyUsage(Factory, new UsingParty(childOrganisation), new ZDateTime(2010, 11, 1), 16m);

			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1, systemUsage2 });
			systemBill.ValidateAll(systemBill);
			AssertHasRowErrorContaining(systemBill, ZString.Format(": Multiple standard discounts - DDDSYD has price version 19.03 and discount V2 vs DD1SYD has price version 19.01 and discount V1", childOrganisation.OH_Code));

			childPriceHeader.L6_DiscountCode = "V2";
			systemBill.ClearAllNotifications();
			systemBill.ValidateAll(systemBill);
			AssertNoRowErrors(systemBill);

			childPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			childPriceHeader.L6_DiscountCode = "V16.10";
			systemBill.ClearAllNotifications();
			systemBill.ValidateAll(systemBill);
			AssertNoRowErrors(systemBill);
		}

		public void TestValidateAll_StandardAndOrganizationDiscountTypes()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var stdDiscounts = stdCompany.SelfBilling.BillingDiscounts;
			var stdDiscount1 = AddDiscount(stdDiscounts, "V1", BillingConstants.DiscountType.Commitment, 10);
			var stdDiscount2 = AddDiscount(stdDiscounts, "V2", BillingConstants.DiscountType.IncrementalVolume, 20);
			var stdDiscount3 = AddDiscount(stdDiscounts, "V3", BillingConstants.DiscountType.MinimumFee, 30);
			var stdDiscount4 = AddDiscount(stdDiscounts, "V4", BillingConstants.DiscountType.ModuleSpecific, 40);
			var stdDiscount5 = AddDiscount(stdDiscounts, "V5", BillingConstants.DiscountType.Prepayment, 50);
			var stdDiscount6 = AddDiscount(stdDiscounts, "V6", BillingConstants.DiscountType.Special, 60);
			var stdDiscount7 = AddDiscount(stdDiscounts, "V7", BillingConstants.DiscountType.Volume, 70);
			stdCompany.Factory.Save();

			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK);
			var orgDiscounts = organisation.LicCompany.SelfBilling.BillingDiscounts;
			AddDiscount(orgDiscounts, "", BillingConstants.DiscountType.Volume, 30);

			ClientLicencePriceHeader parentPriceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			parentPriceHeader.L6_UseStandardDiscount = false;
			parentPriceHeader.L6_RX_NKCurrency = "AUD";
			parentPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			parentPriceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;
			parentPriceHeader.L6_DiscountCode = "V7";

			SystemBillForTesting systemBill = new SystemBillForTesting(Factory);
			DummyUsage systemUsage1 = new DummyUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01));
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { systemUsage1 });

			systemBill.ValidateAll(systemBill);
			AssertHasRowErrorContaining(systemBill, ": standard and organization discounts both contain Volume discounts");

			parentPriceHeader.L6_DiscountCode = BillingConstants.DiscountType.Special;
			systemBill.ClearAllNotifications();
			systemBill.ValidateAll(systemBill);
			AssertNoRowErrors(systemBill);
		}

		public void TestCreateRevenueBreakdown()
		{
			var currentBranchInTestFactory = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, currentBranchInTestFactory);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			BillingTestHelper.SetInvoicingTo(lic2, lic1);
			var priceHeader = BillingTestHelper.CreatePriceList(lic1);
			var isfPrice = priceHeader.Items.AddNew();
			isfPrice.L7_Code = "ISF";
			isfPrice.L7_Price = 1.5m;
			isfPrice.L7_FeeType = BillingConstants.FeeType.Transactional;

			Factory.Save();

			var isfChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(currentBranchInTestFactory, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var isfDiscountChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(currentBranchInTestFactory, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);

			var usingParty1 = new UsingParty(lic1.ClientCompany);
			var usingParty2 = new UsingParty(lic2.ClientCompany);
			var periodStart = BillingTestHelper.MonthToday;
			var isfUsage1 = new PriceItemUsage(Factory, usingParty1, periodStart, "ISF", true);
			var isfUsage2 = new PriceItemUsage(Factory, usingParty2, periodStart, "ISF", true);
			isfUsage1.TransactionCount = 50;
			isfUsage2.TransactionCount = 30;
			var systemBill = new SystemBillForTesting(Factory, "ISF");
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { isfUsage1, isfUsage2 });
			systemBill.SetDiscountAmountForTesting(24m);

			// precondition
			AssertEquals((50 + 30) * 1.5m, systemBill.Amount);
			AssertEquals((50 + 30) * 1.5m - 24m, systemBill.TotalAmount);

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = lic1.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranchPK;
			invoice.AH_GC = Env.CurrentCompanyPK;
			invoice.AH_ExchangeRate = 1m;

			systemBill.CreateRevenueBreakdown(invoice, -10m);
			var allBilledUsage = Factory.Load<EdiBilledUsage>(new ZQuery());
			AssertEquals(2, allBilledUsage.Length);
			{
				var billed = allBilledUsage.Single(x => x.BU9_LCC == lic1.ClientCompany.PK);

				CombineAssertions(() =>
				{
					AssertEquals("BU9_AC_AmountChargeCode", isfChargeCodePk, billed.BU9_AC_AmountChargeCode);
					AssertEquals("BU9_AC_DiscountChargeCode", isfDiscountChargeCodePk, billed.BU9_AC_DiscountChargeCode);
					AssertEquals("BU9_AH_Invoice", invoice.PK, billed.BU9_AH_Invoice);
					AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billed.BU9_BillingModel);
					AssertEquals("BU9_LC", lic1.LA_LC, billed.BU9_LC);
					AssertEquals("BU9_LCC", lic1.ClientCompany.PK, billed.BU9_LCC);
					AssertEquals("BU9_LD", lic1.LA_LD, billed.BU9_LD);

					AssertEquals("BU9_LocalAmountPreDiscount", 50 * 1.5m, billed.BU9_LocalAmountPreDiscount);
					AssertEquals("BU9_LocalAmountPostDiscount", 50 * 1.5m * 0.72m, billed.BU9_LocalAmountPostDiscount);
					AssertEquals("BU9_LocalProcessingAmount", 50 * 1.5m * 0.8m * -0.1m, billed.BU9_LocalProcessingAmount);

					AssertEquals("BU9_PeriodStart", periodStart, billed.BU9_PeriodStart);
					AssertEquals("BU9_PriceCurrency", "AUD", billed.BU9_PriceCurrency);

					AssertEquals("BU9_TransactionAmountPreDiscount", 50 * 1.5m, billed.BU9_TransactionAmountPreDiscount);
					AssertEquals("BU9_TransactionAmountPostDiscount", 50 * 1.5m * 0.72m, billed.BU9_TransactionAmountPostDiscount);
					AssertEquals("BU9_TransactionProcessingAmount", 50 * 1.5m * 0.8m * -0.1m, billed.BU9_TransactionProcessingAmount);

					AssertEquals("BU9_UnitCount", 50m, billed.BU9_UnitCount);
					AssertEquals("BU9_UsageCode", "ISF", billed.BU9_UsageCode);
					AssertEquals("BU9_UsageSubCode", "", billed.BU9_UsageSubCode);
					AssertEquals("BU9_PriceCode", "ISF", billed.BU9_PriceCode);
					AssertEquals("BU9_L7", isfPrice.PK, billed.BU9_L7);
				});
			}

			{
				var billed = allBilledUsage.Single(x => x.BU9_LCC == lic2.ClientCompany.PK);
				CombineAssertions(() =>
				{
					AssertEquals("BU9_AC_AmountChargeCode", isfChargeCodePk, billed.BU9_AC_AmountChargeCode);
					AssertEquals("BU9_AC_DiscountChargeCode", isfDiscountChargeCodePk, billed.BU9_AC_DiscountChargeCode);
					AssertEquals("BU9_AH_Invoice", invoice.PK, billed.BU9_AH_Invoice);
					AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billed.BU9_BillingModel);
					AssertEquals("BU9_LC", lic2.LA_LC, billed.BU9_LC);
					AssertEquals("BU9_LCC", lic2.ClientCompany.PK, billed.BU9_LCC);
					AssertEquals("BU9_LD", lic2.LA_LD, billed.BU9_LD);

					AssertEquals("BU9_LocalAmountPreDiscount", 30 * 1.5m, billed.BU9_LocalAmountPreDiscount);
					AssertEquals("BU9_LocalAmountPostDiscount", 30 * 1.5m * 0.72m, billed.BU9_LocalAmountPostDiscount);
					AssertEquals("BU9_LocalProcessingAmount", 30 * 1.5m * 0.8m * -0.1m, billed.BU9_LocalProcessingAmount);

					AssertEquals("BU9_PeriodStart", periodStart, billed.BU9_PeriodStart);
					AssertEquals("BU9_PriceCurrency", "AUD", billed.BU9_PriceCurrency);

					AssertEquals("BU9_TransactionAmountPreDiscount", 30 * 1.5m, billed.BU9_TransactionAmountPreDiscount);
					AssertEquals("BU9_TransactionAmountPostDiscount", 30 * 1.5m * 0.72m, billed.BU9_TransactionAmountPostDiscount);
					AssertEquals("BU9_TransactionProcessingAmount", 30 * 1.5m * 0.8m * -0.1m, billed.BU9_TransactionProcessingAmount);

					AssertEquals("BU9_UnitCount", 30m, billed.BU9_UnitCount);
					AssertEquals("BU9_UsageCode", "ISF", billed.BU9_UsageCode);
					AssertEquals("BU9_UsageSubCode", "", billed.BU9_UsageSubCode);
					AssertEquals("BU9_PriceCode", "ISF", billed.BU9_PriceCode);
					AssertEquals("BU9_L7", isfPrice.PK, billed.BU9_L7);
				});
			}
		}

		public void TestCreateRevenueBreakdown_HostingUsage()
		{
			var currentBranchInTestFactory = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, currentBranchInTestFactory);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			BillingTestHelper.SetInvoicingTo(lic2, lic1);
			var priceHeader = BillingTestHelper.CreatePriceList(lic1);
			var price = priceHeader.Items.AddNew();
			price.L7_Code = "#HD";
			price.L7_Price = 9.5m;
			price.L7_FeeType = BillingConstants.FeeType.Per10GBPerMonthMin1GB;

			Factory.Save();

			var chargeCodePk = BillingInvoicingHelper.GetChargeCodePK(currentBranchInTestFactory, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(currentBranchInTestFactory, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var usingParty1 = new UsingParty(lic1.ClientCompany);
			var periodStart = BillingTestHelper.MonthToday;

			var usage1 = new Hosting.HostingUsage(Factory, usingParty1, periodStart, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 2000);
			var systemBill = new SystemBillForTesting(Factory, BillingConstants.BillingSystem.HostingStorage);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });
			systemBill.SetDiscountAmountForTesting(24m);

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = lic1.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranchPK;
			invoice.AH_GC = Env.CurrentCompanyPK;
			invoice.AH_ExchangeRate = 1m;

			systemBill.CreateRevenueBreakdown(invoice, -10m);
			var billedUsages = Factory.Load<EdiBilledUsage>(new ZQuery());
			AssertEquals(1, billedUsages.Length);
			var billedUsage = billedUsages.First();

			CombineAssertions(() =>
			{
				AssertEquals("BU9_AC_AmountChargeCode", chargeCodePk, billedUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCodePk, billedUsage.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_AH_Invoice", invoice.PK, billedUsage.BU9_AH_Invoice);
				AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billedUsage.BU9_BillingModel);
				AssertEquals("BU9_LC", lic1.LA_LC, billedUsage.BU9_LC);
				AssertEquals("BU9_LCC", lic1.ClientCompany.PK, billedUsage.BU9_LCC);
				AssertEquals("BU9_LD", lic1.LA_LD, billedUsage.BU9_LD);

				AssertEquals("BU9_LocalAmountPreDiscount", 9.5m, billedUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", -13.05m, billedUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_LocalProcessingAmount", 1.45m, billedUsage.BU9_LocalProcessingAmount);

				AssertEquals("BU9_PeriodStart", periodStart, billedUsage.BU9_PeriodStart);
				AssertEquals("BU9_PriceCurrency", "AUD", billedUsage.BU9_PriceCurrency);

				AssertEquals("BU9_TransactionAmountPreDiscount", 9.5m, billedUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", -13.05m, billedUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_TransactionProcessingAmount", 1.45m, billedUsage.BU9_TransactionProcessingAmount);

				AssertEquals("BU9_UnitCount", 1m, billedUsage.BU9_UnitCount);
				AssertEquals("BU9_UsageCode", "HOS", billedUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "#HD", billedUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#HD", billedUsage.BU9_PriceCode);
				AssertEquals("BU9_L7", price.PK, billedUsage.BU9_L7);
			});
		}

		public void TestCreateRevenueBreakdown_UsageCodeForBilledUsage()
		{
			var currentBranchInTestFactory = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, currentBranchInTestFactory);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var priceHeader = BillingTestHelper.CreatePriceList(lic1);
			var price = priceHeader.Items.AddNew();
			price.L7_Code = "AMS";
			price.L7_Price = 9.5m;
			price.L7_FeeType = BillingConstants.FeeType.Transactional;

			Factory.Save();

			var chargeCodePk = BillingInvoicingHelper.GetChargeCodePK(currentBranchInTestFactory, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(currentBranchInTestFactory, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var usingParty1 = new UsingParty(lic1.ClientCompany);
			var periodStart = BillingTestHelper.MonthToday;

			var usage1 = new PriceItemUsage(Factory, usingParty1, periodStart, "AMS", true);
			usage1.SubCode = "AMS";
			usage1.TransactionCount = 1;
			var systemBill = new SystemBillForTesting(Factory, "AMS");
			systemBill.UsageCodeForBilledUsage = "CPT";
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = lic1.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranchPK;
			invoice.AH_GC = Env.CurrentCompanyPK;
			invoice.AH_ExchangeRate = 1m;

			systemBill.CreateRevenueBreakdown(invoice, 0);
			var billedUsages = Factory.Load<EdiBilledUsage>(new ZQuery());
			AssertEquals(1, billedUsages.Length);
			var billedUsage = billedUsages.First();

			CombineAssertions(() =>
			{
				AssertEquals("BU9_AC_AmountChargeCode", chargeCodePk, billedUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCodePk, billedUsage.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_AH_Invoice", invoice.PK, billedUsage.BU9_AH_Invoice);
				AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billedUsage.BU9_BillingModel);
				AssertEquals("BU9_LC", lic1.LA_LC, billedUsage.BU9_LC);
				AssertEquals("BU9_LCC", lic1.ClientCompany.PK, billedUsage.BU9_LCC);
				AssertEquals("BU9_LD", lic1.LA_LD, billedUsage.BU9_LD);

				AssertEquals("BU9_LocalAmountPreDiscount", 9.5m, billedUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_PeriodStart", periodStart, billedUsage.BU9_PeriodStart);
				AssertEquals("BU9_PriceCurrency", "AUD", billedUsage.BU9_PriceCurrency);
				AssertEquals("BU9_TransactionAmountPreDiscount", 9.5m, billedUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_UnitCount", 1m, billedUsage.BU9_UnitCount);
				AssertEquals("BU9_UsageCode", "CPT", billedUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AMS", billedUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "AMS", billedUsage.BU9_PriceCode);
				AssertEquals("BU9_L7", price.PK, billedUsage.BU9_L7);
			});
		}

		public void TestValidateAll_DiscountOver100Percent()
		{
			var systemBill = new SystemBillForTesting(Factory);
			systemBill.SetAmountForTesting(100m);
			systemBill.SetDiscountAmountForTesting(200m);
			systemBill.ValidateAll(systemBill);
			AssertHasRowError(systemBill, ": Discount over 100%");
		}

		#region Overrides

		protected override SystemBill GetNewSystemBill()
		{
			return new SystemBill(Factory);
		}

		#endregion

		#region Implementation

		static ClientLicenceBillingDiscount AddDiscount(ClientLicenceBillingDiscountCollection discounts, ZString code, ZString discountType, decimal discountPercentage, decimal breakAmount = 0m)
		{
			var discount = discounts.AddNew();
			discount.L5_SystemCode = "DUM";
			discount.L5_DiscountCode = code;
			discount.L5_Type = discountType;
			discount.L5_Discount = discountPercentage;
			discount.L5_BreakAmount = breakAmount;
			return discount;
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

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SystemBill(Factory);
		}

		internal class SystemBillForTesting : SystemBill
		{
			public SystemBillForTesting(BusinessObjectFactory factory)
				: base(factory)
			{
				TextNotifications = new ZStringBuilder();
			}

			public SystemBillForTesting(BusinessObjectFactory factory, string systemCode)
				: base(factory)
			{
				TextNotifications = new ZStringBuilder();
				SystemCode = systemCode;
			}

			public readonly ZStringBuilder TextNotifications;

			public void SetAmountForTesting(ZDecimal amount)
			{
				Amount = amount;
			}

			public void SetDiscountAmountForTesting(ZDecimal discountAmount)
			{
				DiscountAmount = discountAmount;
			}

			public bool MethodWasCalled(string methodName)
			{
				return TextNotifications.ToStringWithNewLineBetweenAppends().Contains(methodName);
			}

			protected override void CalculateGroupAmounts()
			{
				base.CalculateGroupAmounts();
				TextNotifications.Append("CalculateGroupAmounts");

				DiscountAmount = 1m;
			}

			protected override bool CanHaveDifferentCurrenciesInTheGroup
			{
				get { return CanHaveDifferentCurrenciesInTheGroup_Exposed; }
			}
			public bool CanHaveDifferentCurrenciesInTheGroup_Exposed;

			protected override void ValidateUnitPrice(BusinessObject notificationOwner)
			{
				TextNotifications.Append("ValidateUnitPrice");
				base.ValidateUnitPrice(notificationOwner);
			}

			protected override void ValidateAllCore(BusinessObject notificationOwner)
			{
				TextNotifications.Append("ValidateAllCore");
				base.ValidateUnitPrice(notificationOwner);
			}
		}

		#endregion
	}

	[TestsSubclassesOf(typeof(SystemBill))]
	public abstract class SystemBillTestCase<T> : NonPersistentBusinessObjectTestCase
		where T : SystemBill
	{
		protected abstract T GetNewSystemBill();
	}
}
