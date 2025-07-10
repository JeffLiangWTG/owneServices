using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(PremiumServiceUsage))]
	internal class PremiumServiceUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUnitPrice()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var ldsPriceList1 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList1.L6_PricelistVersion = "V1";
			ldsPriceList1.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList1.L6_RX_NKCurrency = "USD";
			ldsPriceList1.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(ldsPriceList1, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 100m);
			priceItem1.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";

			var ldsPriceList2 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList2.L6_PricelistVersion = "V2";
			ldsPriceList2.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList2.L6_RX_NKCurrency = "USD";
			ldsPriceList2.L6_ValidFrom = new ZDateTime(2019, 1, 1);

			var priceItem2 = BillingTestHelper.AddPriceItem(ldsPriceList2, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 110m);
			priceItem2.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			var rate2 = priceItem2.CurrencyRates.AddNew();
			rate2.PIR_RX_NKCurrency = "AUD";
			rate2.PIR_Price = 145m;

			var org = BillingTestHelper.CreateOrganisation(Factory, "FOT");
			BillingTestHelper.SetInvoiceCurrency(org, "AUD");

			var mainPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			mainPriceHeader.L6_RX_NKCurrency = "AUD";
			mainPriceHeader.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var service = Factory.New<ClientPremiumService>();
			service.CPS_Type = "SCC";
			service.CPS_Units = 10;
			service.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;
			service.CPS_StartDate = new ZDateTime(2018, 1, 1);

			var usage1 = new PremiumServiceUsage(Factory, new UsingParty(org), new ZDateTime(2018, 2, 1), new ClientPremiumService[] { service });
			var usage2 = new PremiumServiceUsage(Factory, new UsingParty(org), new ZDateTime(2019, 2, 1), new ClientPremiumService[] { service });

			AssertEquals("no price defined in AUD", 0m, usage1.UnitPrice);
			AssertEquals(145m, usage2.UnitPrice);
		}

		public void TestSystemCode()
		{
			var service = Factory.New<ClientPremiumService>();
			service.CPS_Type = "BBB";

			var periodStart = EdiDateTest.MonthToday;
			var usage = new PremiumServiceUsage(Factory, new UsingParty(), periodStart, new ClientPremiumService[] { service });
			AssertEquals(BillingConstants.BillingSystem.Service, usage.SystemCode);

			service.CPS_Type = "AAA";
			AssertEquals(BillingConstants.BillingSystem.Service, usage.SystemCode);

			usage = new PremiumServiceUsage(Factory, new UsingParty(), periodStart, System.Array.Empty<ClientPremiumService>());
			AssertEquals(BillingConstants.BillingSystem.Service, usage.SystemCode);
		}

		public void TestPriceItemCode()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var ldsPriceList1 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList1.L6_PricelistVersion = "V1";
			ldsPriceList1.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList1.L6_RX_NKCurrency = "USD";
			ldsPriceList1.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(ldsPriceList1, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 100m);
			priceItem1.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			var rate1 = priceItem1.CurrencyRates.AddNew();
			rate1.PIR_RX_NKCurrency = "AUD";
			rate1.PIR_Price = 125m;

			var goldenTaxPriceList = stdLicCompany.PriceHeaders.AddNew();
			goldenTaxPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.GoldenTax;
			goldenTaxPriceList.L6_RX_NKCurrency = "AUD";
			goldenTaxPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			var item2 = goldenTaxPriceList.Items.AddNew();
			item2.L7_Code = "GTD";

			var org = BillingTestHelper.CreateOrganisation(Factory, "FOT");
			BillingTestHelper.SetInvoiceCurrency(org, "AUD");

			var mainPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			mainPriceHeader.L6_RX_NKCurrency = "AUD";
			mainPriceHeader.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var lic = BillingTestHelper.CreateLicence(Factory, "GHJ");

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var periodStart = EdiDateTest.MonthToday;
			var service1 = BillingTestHelper.CreatePremiumService(lic.Database, "SCC", periodStart, ZDateTime.Empty);
			service1.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;
			var service2 = BillingTestHelper.CreatePremiumService(lic.Database, "GTD", periodStart, ZDateTime.Empty);
			service2.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.GoldenTax;

			var usage1 = new PremiumServiceUsage(Factory, new UsingParty(lic), periodStart, new ClientPremiumService[] { service1 });
			var usage2 = new PremiumServiceUsage(Factory, new UsingParty(lic), periodStart, new ClientPremiumService[] { service2 });
			var usage3 = new PremiumServiceUsage(Factory, new UsingParty(lic), periodStart, System.Array.Empty<ClientPremiumService>());

			AssertEquals("SCC", usage1.PriceItemCodeForTest);
			AssertEquals(priceItem1.PK, usage1.PriceItem.PK);

			AssertEquals("GTD", usage2.PriceItemCodeForTest);
			AssertEquals(item2.PK, usage2.PriceItem.PK);

			AssertEquals("", usage3.PriceItemCodeForTest);
		}

		public void TestServices()
		{
			var service1 = Factory.New<ClientPremiumService>();
			var service2 = Factory.New<ClientPremiumService>();
			var service3 = Factory.New<ClientPremiumService>();

			var periodStart = EdiDateTest.MonthToday;
			var usage = new PremiumServiceUsage(Factory, new UsingParty(), periodStart, new ClientPremiumService[] { service1, service2 });
			var services = usage.Services;
			AssertEquals(2, services.Count());
			AssertEquals(service1, services.First());
			AssertEquals(service2, services.Last());
		}

		public void TestGetGeneralSummarySections()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "GHJ");
			BillingTestHelper.SetInvoiceCurrency(org, "USD");
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var price = BillingTestHelper.AddPriceItem(priceHeader, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 2m);
			price.L7_Description = "Price Item BBB";

			var service1 = Factory.New<ClientPremiumService>();
			service1.CPS_Type = "BBB";
			service1.CPS_Units = 100;

			var periodStart = EdiDateTest.MonthToday;
			PremiumServiceUsage usage = new PremiumServiceUsage(Factory, new UsingParty(org), periodStart, new ClientPremiumService[] { service1 });
			usage.SummaryHeaderDescription = "Some Service";
			usage.CalculateAmount();

			SummarySection[] summarySections = usage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			AssertEquals("Summary lines in first section", 1, summarySections[0].Lines.Count);
			var summaryLine = summarySections[0].Lines[0];
			AssertEquals("Price Item BBB", summaryLine.MainDescription);
			AssertEquals("", summaryLine.AdditionalDescription);
			AssertEquals(100.ToString(), summaryLine.UnitCount);
			AssertEquals(2m.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.UnitPrice);
			AssertEquals((2m * 100).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.Amount);

			SummaryLine summaryHeader = summarySections[0].Header;
			AssertEquals("Some Service", summaryHeader.MainDescription);
			AssertEquals("", summaryHeader.AdditionalDescription);
			AssertEquals("Units", summaryHeader.UnitCount);
			AssertEquals("Price", summaryHeader.UnitPrice);
			AssertEquals("Total", summaryHeader.Amount);
			AssertEquals(summaryLine.Amount, summaryHeader.TotalAmount);
		}

		public void TestGetGeneralSummarySections_GlobalPrice()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var ldsPriceList1 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList1.L6_PricelistVersion = "V1";
			ldsPriceList1.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList1.L6_RX_NKCurrency = "USD";
			ldsPriceList1.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(ldsPriceList1, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 100m);
			priceItem1.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			var rate1 = priceItem1.CurrencyRates.AddNew();
			rate1.PIR_RX_NKCurrency = "AUD";
			rate1.PIR_Price = 125m;

			var ldsPriceList2 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList2.L6_PricelistVersion = "V2";
			ldsPriceList2.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList2.L6_RX_NKCurrency = "USD";
			ldsPriceList2.L6_ValidFrom = new ZDateTime(2016, 5, 1);

			var priceItem2 = BillingTestHelper.AddPriceItem(ldsPriceList2, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 110m);
			priceItem2.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			var rate2 = priceItem2.CurrencyRates.AddNew();
			rate2.PIR_RX_NKCurrency = "AUD";
			rate2.PIR_Price = 145m;

			var org = BillingTestHelper.CreateOrganisation(Factory, "FOT");
			BillingTestHelper.SetInvoiceCurrency(org, "AUD");

			var mainPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			mainPriceHeader.L6_RX_NKCurrency = "AUD";
			mainPriceHeader.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var service = Factory.New<ClientPremiumService>();
			service.CPS_Type = "SCC";
			service.CPS_Units = 10;
			service.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;
			service.CPS_StartDate = new ZDateTime(2016, 1, 1);

			var usage1 = new PremiumServiceUsage(Factory, new UsingParty(org), new ZDateTime(2016, 4, 1), new ClientPremiumService[] { service });
			usage1.CalculateAmount();
			var summaryLine1 = usage1.GetGeneralSummarySections()[0].Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)", summaryLine1.MainDescription);
				AssertEquals("10", summaryLine1.UnitCount);
				AssertEquals("125.00", summaryLine1.UnitPrice);
				AssertEquals("1,250.00", summaryLine1.Amount);
			});

			var usage2 = new PremiumServiceUsage(Factory, new UsingParty(org), new ZDateTime(2016, 5, 1), new ClientPremiumService[] { service });
			usage2.CalculateAmount();
			var summaryLine2 = usage2.GetGeneralSummarySections()[0].Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("CurrencyCode matches main price header currency", "AUD", usage2.CurrencyCode);
				AssertEquals("Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)", summaryLine2.MainDescription);
				AssertEquals("10", summaryLine2.UnitCount);
				AssertEquals("145.00", summaryLine2.UnitPrice);
				AssertEquals("1,450.00", summaryLine2.Amount);
			});
		}

		public void TestPriceItemIsCorrectFeeType()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "GHJ");
			var org = lic.Company.Header;
			BillingTestHelper.SetInvoiceCurrency(org, "USD");
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			var priceItem = BillingTestHelper.AddPriceItem(priceHeader, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 2m);

			var service1 = Factory.New<ClientPremiumService>();
			service1.CPS_Type = "BBB";
			service1.CPS_Units = 100;

			var list = BillingConstants.GetFeeTypeList();

			var periodStart = EdiDateTest.MonthToday;
			foreach (var feeType in list.Cast<ICodeDescription>().Select(x => x.Code))
			{
				priceItem.L7_FeeType = feeType;
				PremiumServiceUsage usage = new PremiumServiceUsage(Factory, new UsingParty(org), periodStart, new ClientPremiumService[] { service1 });
				AssertNotNull(priceItem.L7_FeeType, usage.PriceItem);
			}
		}

		internal static PremiumServiceUsage CreatePremiumServiceUsage(BusinessObjectFactory factory, EDIOrgHeader org, ZDateTime periodStart)
		{
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			IEnumerable<ClientPremiumService> services = db.PremiumServices.GetMatched(periodStart).GroupBy(x => x.CPS_Type).First();
			return new PremiumServiceUsage(factory, new UsingParty(org), periodStart, services.ToArray());
		}

		#region Implementation

		PremiumServiceUsage CreateUsage()
		{
			var periodStart = EdiDateTest.MonthToday;
			return new PremiumServiceUsage(Factory, new UsingParty(), periodStart, System.Array.Empty<ClientPremiumService>());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateUsage();
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
