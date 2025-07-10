using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Licensing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.ODPL.Test
{
	[TestedType(typeof(OdplUsage))]
	internal class OdplUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			OdplUsage odplUsage = CreateOdplUsage();
			AssertEquals(BillingConstants.BillingSystem.ODM, odplUsage.SystemCode);
		}

		public void TestPriceHeader()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			ClientLicencePriceHeader parentPriceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			parentPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(parentPriceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);

			EDIOrgHeader childOrganisation = BillingTestHelper.CreateDependentOrganisation(organisation, "BBB");
			ClientLicencePriceHeader childPriceHeader = childOrganisation.LicCompany.PriceHeaders.AddNew();
			childPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(childPriceHeader, "XXX", BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(childPriceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);

			OdplUsage odplUsage = CreateUsage(childOrganisation, new ZDateTime(2010, 10, 01));
			AssertEquals("Using own priceheader 'cause it's not a quick transactional (has CORE price)", childPriceHeader, odplUsage.PriceHeader);

			childOrganisation.LicCompany.InvoiceDeliveries[0].L9_UseParentPrices = true;
			odplUsage = CreateUsage(childOrganisation, new ZDateTime(2010, 10, 01));
			AssertEquals("Using parent priceheader according to L9_UseParentPrices", parentPriceHeader, odplUsage.PriceHeader);

			childOrganisation.LicCompany.InvoiceDeliveries[0].L9_UseParentPrices = false;
			childPriceHeader.Items.DeleteAll();
			BillingTestHelper.AddPriceItem(childPriceHeader, "XXX", BillingConstants.FeeType.Module, "", 10m);

			odplUsage = CreateUsage(childOrganisation, new ZDateTime(2010, 10, 01));
			AssertEquals("Using parent's priceheader, because own one is a quick transactional", parentPriceHeader, odplUsage.PriceHeader);

			parentPriceHeader.Items.DeleteAll();
			odplUsage = CreateUsage(childOrganisation, new ZDateTime(2010, 10, 01));
			AssertEquals("Both own and parent priceheaders do not have core price => can't use them", null, odplUsage.PriceHeader);
		}

		public void TestAddModuleUsage()
		{
			OdplUsage odplUsage = CreateOdplUsage();
			AssertEquals("Precondition", 0, odplUsage.ModuleUsages.Count);

			AddModuleUsage(odplUsage, "AAA", 4);
			AddModuleUsage(odplUsage, "BBB", 8);
			AssertEquals(2, odplUsage.ModuleUsages.Count);

			AssertEquals("AAA", odplUsage.ModuleUsages[0].ModuleCode);
			AssertEquals(4, odplUsage.ModuleUsages[0].StaffCount);

			AssertEquals("BBB", odplUsage.ModuleUsages[1].ModuleCode);
			AssertEquals(8, odplUsage.ModuleUsages[1].StaffCount);

			AssertExceptionThrown<InvalidOperationException>(() => AddModuleUsage(odplUsage, "BBB", 16));
		}

		public void TestAmount()
		{
			OdplUsage odplUsage = CreateOdplUsage();
			AssertEquals("Precondition", 0m, odplUsage.Amount);

			AddModuleUsage(odplUsage, "AAA", 10, 1);
			AddModuleUsage(odplUsage, "BBB", 10, 2);
			AssertEquals(30m, odplUsage.Amount);

			var module = AddModuleUsage(odplUsage, "CCC", 10, 1);
			AssertEquals(40m, odplUsage.Amount);
			module.PurchasedStaffCount = 3;
			AssertEquals(37m, odplUsage.Amount);
		}

		public void TestHasLicenceUnits()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			var itemAAA = BillingTestHelper.AddPriceItem(priceHeader, "AAA", BillingConstants.FeeType.NamedUser, "", 2m, 1m);
			AssertEquals("Precondition", true, itemAAA.HasLicenceUnits);
			var itemBBB = BillingTestHelper.AddPriceItem(priceHeader, "BBB", BillingConstants.FeeType.NamedUser, "", 5m);
			AssertEquals("Precondition", false, itemBBB.HasLicenceUnits);

			var periodStart = ZDateTime.Today.Date;
			periodStart = periodStart.AddDays(periodStart.Day - 1);
			OdplUsage odplUsage1 = CreateUsage(organisation, periodStart);
			OdplUsage odplUsage2_NotShownOnSummary = CreateUsage(organisation, periodStart);
			OdplUsage odplUsage3 = CreateUsage(organisation, periodStart);
			OdplUsage odplUsage4_NotShownOnSummary = CreateUsage(organisation, periodStart);
			AddModuleUsage(odplUsage1, "AAA", 8, 2.5m);
			AddModuleUsage(odplUsage2_NotShownOnSummary, "AAA", 0, 10m);
			AddModuleUsage(odplUsage3, "AAA", 8, 2.5m);
			AddModuleUsage(odplUsage3, "BBB", 0, 10m);
			AddModuleUsage(odplUsage4_NotShownOnSummary, "BBB", 8, 10m);

			AssertEquals(true, odplUsage1.HasLicenceUnits);
			AssertEquals(false, odplUsage2_NotShownOnSummary.HasLicenceUnits);
			AssertEquals(true, odplUsage3.HasLicenceUnits);
			AssertEquals(false, odplUsage4_NotShownOnSummary.HasLicenceUnits);
		}

		public void TestMixedAmountAsMoney()
		{
			OdplUsage odplUsage = CreateOdplUsage();
			AssertEquals("Precondition", 0m, odplUsage.MixedAmountAsMoney);

			var module1 = AddModuleUsage(odplUsage, "AAA", 10, 1m);
			var module2 = AddModuleUsage(odplUsage, "BBB", 10, 2m);
			module2.PurchasedStaffCount = 2;
			AssertEquals(30m, odplUsage.MixedAmountAsMoney);

			var module3 = AddModuleUsage(odplUsage, "CCC", 10, 3m);
			module3.PurchasedStaffCount = 99;
			AssertEquals(30m + 99 * 3m, odplUsage.MixedAmountAsMoney);

			var module4 = AddModuleUsage(odplUsage, "DDD", 10, 4m);
			module4.PurchasedStaffCount = 3;
			AssertEquals(30m + 99 * 3m + 10 * 4m, odplUsage.MixedAmountAsMoney);
		}

		public static OdplModuleUsage AddModuleUsage(OdplUsage odplUsage, string moduleCode, ZInt staffCount, ZDecimal unitPrice, short licenceUnits)
		{
			OdplModuleUsage result = AddModuleUsage(odplUsage, moduleCode, staffCount, unitPrice);
			result.LicenceUnits = licenceUnits;
			return result;
		}

		public static OdplModuleUsage AddModuleUsage(OdplUsage odplUsage, string moduleCode, ZInt staffCount, ZDecimal unitPrice)
		{
			OdplModuleUsage result = AddModuleUsage(odplUsage, moduleCode, staffCount);
			result.ModuleName = moduleCode + " Module";
			result.UnitPrice = unitPrice;
			result.FeeType = BillingConstants.FeeType.NamedUser;
			return result;
		}

		public static OdplModuleUsage AddModuleUsage(OdplUsage odplUsage, ZString moduleCode, int staffCount)
		{
			ClientLicencePriceItem priceItem = null;
			ClientLicencePriceHeader priceHeader = odplUsage.PriceHeader;
			if (priceHeader != null)
			{
				priceItem = priceHeader.LocalOrStandardItems.FindByCode(moduleCode);
			}

			var result = odplUsage.AddModuleUsage(moduleCode, staffCount, priceItem);

			if (odplUsage.LicHeader != null)
			{
				LicenceModules licModule = odplUsage.LicHeader.Modules.FindByCode(moduleCode);
				if (licModule != null)
				{
					result.PurchasedStaffCount = licModule.LM_UserCount;
				}
			}

			return result;
		}

		public void TestLicenceUnitsAmount()
		{
			OdplUsage odplUsage = CreateOdplUsage();
			AssertEquals("Precondition", 0m, odplUsage.LicenceUnitsAmount);

			AddModuleUsage(odplUsage, "AAA", 10, 1, 5);
			AddModuleUsage(odplUsage, "BBB", 10, 2, 10);
			AssertEquals(30m, odplUsage.Amount);
			AssertEquals(150m, odplUsage.LicenceUnitsAmount);

			AddModuleUsage(odplUsage, "CCC", 10, 1, 6);
			AssertEquals(40m, odplUsage.Amount);
			AssertEquals(210m, odplUsage.LicenceUnitsAmount);
		}

		public void TestMixedAmountAsLicenceUnits()
		{
			OdplUsage odplUsage = CreateOdplUsage();
			AssertEquals("Precondition", 0m, odplUsage.MixedAmountAsLicenceUnits);

			var module1 = AddModuleUsage(odplUsage, "AAA", 10, 1, 5);
			module1.PurchasedStaffCount = 7;
			var module2 = AddModuleUsage(odplUsage, "BBB", 10, 2, 10);
			module2.PurchasedStaffCount = 9;
			AssertEquals(10 * 5m + 10 * 10m, odplUsage.MixedAmountAsLicenceUnits);
		}

		public void TestCurrencyCode()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			OdplUsage odplUsage = CreateUsage(organisation, new ZDateTime(2010, 10, 01));
			AssertEquals("No price header, no currency", "", odplUsage.CurrencyCode);

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			odplUsage = CreateUsage(organisation, new ZDateTime(2010, 10, 01));
			AssertEquals("AUD", odplUsage.CurrencyCode);

			priceHeader.L6_RX_NKCurrency = "MDL";
			odplUsage = CreateUsage(organisation, new ZDateTime(2010, 10, 01));
			AssertEquals("MDL", odplUsage.CurrencyCode);
		}

		public void TestCountryCode()
		{
			var today = ZDateTime.Today;
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "HST", true);
			var usage = new OdplUsage(Factory, licence, new ZDateTime(today.Year, today.Month, 1), licence.Database.ClientCompanies[0]);
			usage.ClientCo.LCC_RN_NKCountryCode = "AU";
			usage.LicCompany.LC_CompanyCountry = "US";
			AssertEquals("AU", usage.CountryCode);

			usage.ClientCo.LCC_RN_NKCountryCode = "";
			AssertEquals("US", usage.CountryCode);
		}

		public void TestAddModuleUsage_NonOnDemandModulesAreIgnored()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, "AAA", BillingConstants.FeeType.NamedUser, "", 2m);
			BillingTestHelper.AddPriceItem(priceHeader, "BBB", BillingConstants.FeeType.Included, "AAA", 5m);
			BillingTestHelper.AddPriceItem(priceHeader, "CCC", BillingConstants.FeeType.Transactional, "", 5m);

			OdplUsage odplUsage = CreateUsage(organisation, new ZDateTime(2010, 10, 01));
			AssertNotNull(AddModuleUsage(odplUsage, "AAA", 1));
			AssertNotNull(AddModuleUsage(odplUsage, "BBB", 1));
			AssertNull(AddModuleUsage(odplUsage, "CCC", 1));

			AssertEquals("Non on-demand modules are not present", 2, odplUsage.ModuleUsages.Count);
			AssertEquals(true, odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Any(x => x.ModuleCode == "AAA"));
			AssertEquals(true, odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Any(x => x.ModuleCode == "BBB"));
			AssertEquals(false, odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Any(x => x.ModuleCode == "CCC"));
		}

		public void TestCalculateAmount_IsHybrid()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			var clientCo = BillingTestHelper.FindOrCreateClientCompany(licHeader);
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			var org = licHeader.Company.Header;

			var core = licHeader.GetCoreModule();
			core.LM_LicenceType = LicenceTypes.Codes.PUR;
			core.LM_UserCount = 1;

			OdplUsage odplUsage = CreateUsage(clientCo, new ZDateTime(2013, 2, 1));
			odplUsage.ModuleUsages.AddNew().ModuleCode = "COR";
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with 1 Core seat", true, odplUsage.IsHybrid);

			core.LM_ExpiryDate = odplUsage.PeriodStart;
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with expired Core seat", false, odplUsage.IsHybrid);

			core.LM_ExpiryDate = odplUsage.PeriodStart.AddDays(1);
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with current PUR Core seat", true, odplUsage.IsHybrid);

			core.LM_LicenceType = LicenceTypes.Codes.REN;
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with REN Core seat", true, odplUsage.IsHybrid);

			core.LM_LicenceType = LicenceTypes.Codes.ODM;
			core.LM_ExpiryDate = ZDateTime.Empty;
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with ODM Core seat", true, odplUsage.IsHybrid);

			core.LM_LicenceType = LicenceTypes.Codes.OPN;
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with OPN Core seat", true, odplUsage.IsHybrid);

			core.LM_LicenceType = LicenceTypes.Codes.ODM;
			core.LM_UserCount = 0;
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with 0 Core seat", false, odplUsage.IsHybrid);

			var nonCore = licHeader.Modules.FindByCode(Enterprise.Environment.Env.Licence.Accountant.Name);
			nonCore.LM_UserCount = 1;
			nonCore.LM_LicenceType = LicenceTypes.Codes.ODM;
			odplUsage.CalculateAmount();
			AssertEquals("IsHybrid with 0 Core, some non-Core", false, odplUsage.IsHybrid);
		}

		public void TestGetGeneralSummarySections()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_PricelistVersion = "103L";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, ZString.Empty, ZString.Empty, ZString.Empty, 10m)
				.L7_Description = "Group header";
			BillingTestHelper.AddPriceItem(priceHeader, "AAA", BillingConstants.FeeType.NamedUser, "", 2m)
				.L7_Description = "  AAA";
			BillingTestHelper.AddPriceItem(priceHeader, "BBB", BillingConstants.FeeType.NamedUser, "", 5m)
				.L7_Description = "  BBB";
			BillingTestHelper.AddPriceItem(priceHeader, "CCC", BillingConstants.FeeType.Transactional, "", 5m);

			var periodStart = ZDateTime.Today.Date;
			periodStart = periodStart.AddDays(periodStart.Day - 1);
			OdplUsage odplUsage = CreateUsage(organisation, periodStart);
			OdplModuleUsage odplModule1 = AddModuleUsage(odplUsage, "AAA", 8, 2.5m);
			OdplModuleUsage odplModule2 = AddModuleUsage(odplUsage, "BBB", 16, 10m);

			SummarySection[] summarySections = odplUsage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertEquals("Summary lines in first section", 3, summarySections[0].Lines.Count);
			AssertEquals("Group header", summarySections[0].Lines[0].MainDescription);
			AssertSummaryLine(summarySections[0].Lines[1], odplModule1);
			AssertSummaryLine(summarySections[0].Lines[2], odplModule2);

			SummaryLine summaryHeader = summarySections[0].Header;
			AssertEquals("On Demand Production License Usage", summaryHeader.MainDescription);
			AssertEquals("Fee Basis", summaryHeader.AdditionalDescription);
			AssertEquals("Users", summaryHeader.UnitCount);
			AssertEquals("Price", summaryHeader.UnitPrice);
			AssertEquals("Total", summaryHeader.Amount);
			AssertEquals("180.00", summaryHeader.TotalAmount);

			// Mixed licence
			odplUsage = CreateUsage(organisation, periodStart);

			odplModule1 = AddModuleUsage(odplUsage, "AAA", 8, 2.5m);
			odplModule2 = AddModuleUsage(odplUsage, "BBB", 16, 10m);
			odplModule1.PurchasedStaffCount = 3;
			summarySections = odplUsage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertEquals("Summary lines in first section", 3, summarySections[0].Lines.Count);
			AssertSummaryLine(summarySections[0].Lines[1], odplModule1);
			AssertSummaryLine(summarySections[0].Lines[2], odplModule2);

			summaryHeader = summarySections[0].Header;
			AssertEquals("On Demand Production License Usage", summaryHeader.MainDescription);
			AssertEquals("Fee Basis", summaryHeader.AdditionalDescription);
			AssertEquals("Purchased Users", summaryHeader.PurchasedCount);
			AssertEquals("On Demand Users", summaryHeader.UnitCount);
			AssertEquals("Total Users", summaryHeader.TotalUnitCount);
			AssertEquals("Price", summaryHeader.UnitPrice);
			AssertEquals("Total", summaryHeader.Amount);
			AssertEquals("172.50", summaryHeader.TotalAmount);
		}

		public void TestGetGeneralSummarySectionsNewPricelist()
		{
			var periodStart = ZDateTime.Today.Date;
			periodStart = periodStart.AddDays(1 - periodStart.Day);
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, ZString.Empty, ZString.Empty, ZString.Empty, 10m)
				.L7_Description = "Group header";
			var itemAAA = BillingTestHelper.AddPriceItem(priceHeader, "AAA", BillingConstants.FeeType.NamedUser, "", 2m, 1m);
			itemAAA.L7_Description = "  AAA";
			AssertEquals("Precondition", true, itemAAA.HasLicenceUnits);
			BillingTestHelper.AddPriceItem(priceHeader, "BBB", BillingConstants.FeeType.NamedUser, "", 5m)
				.L7_Description = "  BBB";
			BillingTestHelper.AddPriceItem(priceHeader, "CCC", BillingConstants.FeeType.Transactional, "", 5m);

			OdplUsage odplUsage = CreateUsage(organisation, periodStart);
			OdplModuleUsage odplModule1 = AddModuleUsage(odplUsage, "AAA", 8, 2.5m);
			OdplModuleUsage odplModule2 = AddModuleUsage(odplUsage, "BBB", 16, 10m);

			SummarySection[] summarySections = odplUsage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertEquals("Summary lines in first section", 3, summarySections[0].Lines.Count);
			AssertEquals("Group header", summarySections[0].Lines[0].MainDescription);
			AssertSummaryLine(summarySections[0].Lines[1], odplModule1);
			AssertSummaryLine(summarySections[0].Lines[2], odplModule2);

			SummaryLine summaryHeader = summarySections[0].Header;
			AssertEquals("On Demand Production License Usage", summaryHeader.MainDescription);
			AssertEquals("Fee Basis", summaryHeader.AdditionalDescription);
			AssertEquals("Users", summaryHeader.UnitCount);
			AssertEquals("Price\r\n(AUD)", summaryHeader.UnitPrice);
			AssertEquals("Total Price\r\n(AUD)", summaryHeader.Amount);
			AssertEquals("180.00", summaryHeader.TotalAmount);

			// Mixed licence
			odplUsage = CreateUsage(organisation, periodStart);

			odplModule1 = AddModuleUsage(odplUsage, "AAA", 8, 2.5m);
			odplModule2 = AddModuleUsage(odplUsage, "BBB", 16, 10m);
			odplModule1.PurchasedStaffCount = 3;
			summarySections = odplUsage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertEquals("Summary lines in first section", 3, summarySections[0].Lines.Count);
			AssertSummaryLine(summarySections[0].Lines[1], odplModule1);
			AssertSummaryLine(summarySections[0].Lines[2], odplModule2);

			summaryHeader = summarySections[0].Header;
			AssertEquals("On Demand Production License Usage", summaryHeader.MainDescription);
			AssertEquals("Fee Basis", summaryHeader.AdditionalDescription);
			AssertEquals("Purchased Users", summaryHeader.PurchasedCount);
			AssertEquals("On Demand Users", summaryHeader.UnitCount);
			AssertEquals("Total Users", summaryHeader.TotalUnitCount);
			AssertEquals("Price\r\n(AUD)", summaryHeader.UnitPrice);
			AssertEquals("Total Price\r\n(AUD)", summaryHeader.Amount);
			AssertEquals("172.50", summaryHeader.TotalAmount);
		}

		void AssertSummaryLine(SummaryLine summaryLine, OdplModuleUsage moduleUsage)
		{
			AssertEquals(moduleUsage.ModuleName, summaryLine.MainDescription);
			AssertEquals(moduleUsage.UnitCount.ToString(), summaryLine.UnitCount);
			if (moduleUsage.PurchasedStaffCount > 0)
			{
				AssertEquals(moduleUsage.PurchasedStaffCount.ToString(), summaryLine.PurchasedCount);
				AssertEquals(moduleUsage.MixedUnitCount.ToString(), summaryLine.TotalUnitCount);
			}
			AssertEquals(moduleUsage.FeeTypeDescription, summaryLine.AdditionalDescription);
			AssertEquals(moduleUsage.UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.UnitPrice);
			AssertEquals(moduleUsage.Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.Amount);
		}

		public void TestBuildChildToParentMap()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			var item1 = AddPriceItem(priceHeader, "Core");
			item1.L7_Code = BillingConstants.CoreModuleCode;
			var item2 = AddPriceItem(priceHeader, "  Acc");
			var item3 = AddPriceItem(priceHeader, "    Fax");
			var item4 = AddPriceItem(priceHeader, "  Doc");
			var item5 = AddPriceItem(priceHeader, "For");
			var item6 = AddPriceItem(priceHeader, "Ship");
			var item7 = AddPriceItem(priceHeader, "  Sh1");
			var item8 = AddPriceItem(priceHeader, "    Sh2");
			var item9 = AddPriceItem(priceHeader, "    Sh3");
			var item10 = AddPriceItem(priceHeader, "  Sh4");
			var item11 = AddPriceItem(priceHeader, "Ware");
			var item12 = AddPriceItem(priceHeader, "  Bond");

			var periodStart = ZDateTime.Today.Date;
			periodStart = periodStart.AddDays(periodStart.Day - 1);
			OdplUsage odplUsage = CreateUsage(organisation, periodStart);
			var map = odplUsage.BuildChildToParentMap();

			AssertEquals(false, map.ContainsKey(item1));
			AssertEquals(false, map.ContainsKey(item5));
			AssertEquals(false, map.ContainsKey(item6));
			AssertEquals(false, map.ContainsKey(item11));
			AssertEquals(item1, map[item2]);
			AssertEquals(item2, map[item3]);
			AssertEquals(item1, map[item4]);
			AssertEquals(item6, map[item7]);
			AssertEquals(item7, map[item8]);
			AssertEquals(item7, map[item9]);
			AssertEquals(item6, map[item10]);
			AssertEquals(item11, map[item12]);
		}

		public void TestCoreUsage()
		{
			OdplUsage odplUsage = CreateOdplUsage();
			AssertNull(odplUsage.CoreUsage);

			AddModuleUsage(odplUsage, "BBB", 10, 2);
			AssertNull(odplUsage.CoreUsage);

			AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 10, 1);
			AssertEquals(10, odplUsage.CoreUsage.StaffCount);
		}

		[TestDate(2014, 1, 1)]
		public void TestHasOnDemandUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			OdplUsage odplUsage = CreateUsage(org, new ZDateTime(2010, 10, 1));
			AssertEquals(false, odplUsage.HasOnDemandUsage);

			AddModuleUsage(odplUsage, "BBB", 10, 0).PurchasedStaffCount = 10;
			AssertEquals(false, odplUsage.HasOnDemandUsage);

			AddModuleUsage(odplUsage, "CCC", 10, 0).PurchasedStaffCount = 11;
			AssertEquals(false, odplUsage.HasOnDemandUsage);

			AddModuleUsage(odplUsage, "DDD", 12, 0).PurchasedStaffCount = 11;
			AssertEquals(true, odplUsage.HasOnDemandUsage);

			ClientLicencePriceHeader prices = org.LicCompany.PriceHeaders.AddNew();
			var corePrice = BillingTestHelper.AddPriceItem(prices, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 0m);
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			odplUsage = CreateUsage(org, new ZDateTime(2010, 10, 1));
			AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 12, 0).PurchasedStaffCount = 11;
			AssertEquals("usage has a pricelist", false, odplUsage.HasOnDemandUsage);

			odplUsage = CreateUsage(org, new ZDateTime(2010, 10, 1));
			AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 10, 2).PurchasedStaffCount = 10;
			AssertEquals(false, odplUsage.HasOnDemandUsage);

			odplUsage = CreateUsage(org, new ZDateTime(2010, 10, 1));
			AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 11, 2).PurchasedStaffCount = 10;
			AssertEquals(true, odplUsage.HasOnDemandUsage);
		}

		ClientLicencePriceItem AddPriceItem(ClientLicencePriceHeader priceHeader, string description)
		{
			var item = priceHeader.Items.AddNew();
			item.L7_Description = description;
			item.L7_Order = (short)priceHeader.Items.Count;
			return item;
		}

		OdplUsage CreateUsage(ClientCompany clientCo, ZDateTime periodStart)
		{
			return new OdplUsage(Factory, clientCo.UsageOwnerLicence, periodStart, clientCo);
		}

		OdplUsage CreateUsage(EDIOrgHeader org, ZDateTime periodStart)
		{
			var licHeader = org.LicCompany.LicHeadersForAllDatabases[0];
			ClientCompany clientCo = licHeader.ClientCompany ?? BillingTestHelper.FindOrCreateClientCompany(licHeader);
			return CreateUsage(clientCo, periodStart);
		}

		#region Implementation

		OdplUsage CreateOdplUsage()
		{
			return GetNewBusinessObject() as OdplUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var today = ZDateTime.Today;
			return new OdplUsage(Factory, BillingTestHelper.CreateLicence(Factory, "AAA"), new ZDateTime(today.Year, today.Month, 1), null);
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

		#endregion
	}
}
