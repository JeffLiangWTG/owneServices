using System;
using System.Linq;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class OrgUsageTest : TestCaseWithFactory
	{
		public void TestOrgUsage_LongFileName()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			SetupUsages(org);
			var usage = new OrgUsage(org, new ZDateTime(2017, 11, 1), null);
			AssertEquals(1, usage.StlUsages.Count);
			var stlUsage = usage.StlUsages.First();
			AssertEquals(2, stlUsage.UsageData.Count());
			var stlUsageData1 = stlUsage.UsageData.First();
			var stlUsageData2 = stlUsage.UsageData.ElementAt(1);
			CombineAssertions(() =>
			{
				AssertEquals("Core, Pack,", stlUsageData1.Description);
				AssertEquals("Shipment and booking (include Consol & Spot Quotes for free)", stlUsageData2.Description);
				var linksAsText = string.Join("\r\n", stlUsageData1.ReportLinks.Union(stlUsageData2.ReportLinks).SelectMany(x => new[] { x.CsvLinkUrl, x.PdfLinkUrl }).Where(x => !string.IsNullOrEmpty(x)).Select(x => HttpUtility.ParseQueryString(x)).Select(x => new SecureQueryString(x.GetValues(0)[0])).Select(x => x["FileName"]).Distinct().OrderBy(x => x));
				AssertEquals(@"201711_DDD-SYD_ABC_Core Pack
201711_DDD-SYD_ABC_Shipment
201711_DDD-SYD_ALL_Core Pack
201711_DDD-SYD_ALL_Shipment", linksAsText);
			});
		}

		public void TestOrgUsage_ReportUrlPeriodStartFormat()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			SetupUsages(org);
			var usage = new OrgUsage(org, new ZDateTime(2017, 11, 1), null);
			var stlUsage = usage.StlUsages.First();
			var stlUsageData = stlUsage.UsageData.First();
			var stlLinkUrl = stlUsageData.ReportLinks.First();
			var stlQueryStringPart = stlLinkUrl.CsvLinkUrl.Split(new char[] { '=' }, 2)[1];
			var stlSecureQueryString = new SecureQueryString(WebUtility.UrlDecode(stlQueryStringPart));
			AssertEquals("Period Start", "2017-11-01", stlSecureQueryString[StlUsageReportRequestHelper.Constants.PeriodStart]);
			var odplUsage = usage.OdplUsages.First();
			var odplUsageData = odplUsage.UsageData.First();
			var odplLinkUrl = odplUsageData.ReportLinks.First();
			var odplQueryStringPart = odplLinkUrl.CsvLinkUrl.Split(new char[] { '=' }, 2)[1];
			var odplSecureQueryString = new SecureQueryString(WebUtility.UrlDecode(odplQueryStringPart));
			AssertEquals("Period Start", "2017-11-01", odplSecureQueryString[OdplUsageReportRequestHelper.Constants.PeriodStart]);
		}

		public void TestOrgUsage_ValidStlUsageCodesOnOdplPricelists()
		{
			var periodStart = new ZDateTime(2017, 11, 1);
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "PRD");
			var prices = BillingTestHelper.CreatePriceList(lic);
			var shpPrice = prices.Items.AddNew();
			shpPrice.L7_Code = "SHP";
			shpPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			shpPrice.L7_Price = 5.00;
			shpPrice.L7_Description = "SHP (STL)";
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", periodStart, lic.ClientCompany, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "HOS", "#HE", periodStart, lic.ClientCompany, 3000);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic.ClientCompany, 50);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHX", periodStart, lic.ClientCompany, 40);
			Factory.Save();
			var regValue = new CodeDescriptionPairList();
			regValue.AddPair("SHP", "");
			EDIDataRegistry.Instance.ValidStlUsageCodesOnOdplPricelists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);
			var usage = new OrgUsage(lic.Company.Header, periodStart, null);
			AssertEquals(0, usage.StlUsages.Count);
			AssertEquals(1, usage.OdplUsages.Count);
			var odplUsage = usage.OdplUsages.First();
			AssertEquals(3, odplUsage.UsageData.Count());
			var odplUsageData1 = odplUsage.UsageData.First();
			var odplUsageData2 = odplUsage.UsageData.ElementAt(1);
			var odplUsageData3 = odplUsage.UsageData.ElementAt(2);
			CombineAssertions(() =>
			{
				AssertEquals("On Demand", odplUsageData1.Description);
				AssertEquals("SHP (STL)", odplUsageData2.Description);
				AssertEquals("WiseCloud Storage", odplUsageData3.Description);
				var linksAsText = string.Join("\r\n", odplUsageData1.ReportLinks.Union(odplUsageData2.ReportLinks).Union(odplUsageData3.ReportLinks).SelectMany(x => new[] { x.CsvLinkUrl, x.PdfLinkUrl }).Where(x => !string.IsNullOrEmpty(x)).Select(x => HttpUtility.ParseQueryString(x)).Select(x => new SecureQueryString(x.GetValues(0)[0])).Select(x => x["FileName"]).Distinct().OrderBy(x => x));
				AssertEquals(@"201711_ENTCOM Company_On Demand_PRD_COM
201711_ENTCOM Company_SHP (STL)_PRD_COM
201711_ENTCOM Company_WiseCloud Storage_PRD_COM", linksAsText);
			});
		}

		public void TestOrgUsage_GenericUsage()
		{
			var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
			categories.AddPair("ABC", "ABC");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.PriceListCode = "ABC";
			priceList1.RawUsageCategory = "ABC";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = "STL";
			lic1.Database.LD_Product = "ABC";
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			Factory.Save();

			var org = lic1.Company.LicEnterprise.Organisation;
			SetupGenericUsages(org);
			var usage = new OrgUsage(org, new ZDateTime(2017, 11, 1), "ABC");
			AssertEquals(1, usage.StlUsages.Count);
			AssertEquals(0, usage.OdplUsages.Count);
			AssertEquals(0, usage.BorderWiseUsages.Count);
			var stlUsage = usage.StlUsages.First();
			AssertEquals(2, stlUsage.UsageData.Count());
			var stlUsageData1 = stlUsage.UsageData.First();
			var stlUsageData2 = stlUsage.UsageData.ElementAt(1);
			CombineAssertions(() =>
			{
				AssertEquals("Core, Pack,", stlUsageData1.Description);
				AssertEquals("Shipment and booking (include Consol & Spot Quotes for free)", stlUsageData2.Description);
				var linksAsText = string.Join("\r\n", stlUsageData1.ReportLinks.Union(stlUsageData2.ReportLinks).SelectMany(x => new[] { x.CsvLinkUrl, x.PdfLinkUrl }).Where(x => !string.IsNullOrEmpty(x)).Select(x => HttpUtility.ParseQueryString(x)).Select(x => new SecureQueryString(x.GetValues(0)[0])).Select(x => x["FileName"]).Distinct().OrderBy(x => x));
				AssertEquals(@"201711_AAA-AAA_AAA_Core Pack
201711_AAA-AAA_AAA_Shipment
201711_AAA-AAA_ALL_Core Pack
201711_AAA-AAA_ALL_Shipment", linksAsText);
			});
		}

		void SetupUsages(EDIOrgHeader org)
		{
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;
			db.LD_LicenceType = "PRD";
			db.LD_Product = "CW1";
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "AU");
			var priceHeader1 = org.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_SystemCode = "STL";
			var price1 = priceHeader1.Items.AddNew();
			price1.L7_Category = BillingConstants.BillingSystem.STL;
			price1.L7_Code = "P01";
			price1.L7_Description = "Core, Pack,";
			var price2 = priceHeader1.Items.AddNew();
			price2.L7_Category = BillingConstants.BillingSystem.STL;
			price2.L7_Code = "P02";
			price2.L7_Description = "Shipment and booking (include Consol & Spot Quotes for free)";
			Factory.Save();
			var link = BillingTestHelper.CreatePriceLink(db, priceHeader1, new ZDateTime(2017, 1, 1));
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P01", new ZDateTime(2017, 11, 1), clientCompany, 23);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P02", new ZDateTime(2017, 11, 1), clientCompany, 189);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", new ZDateTime(2017, 11, 1), org.LicCompany.PK, 30);
			Factory.Save();
		}

		void SetupGenericUsages(EDIOrgHeader org)
		{
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;
			db.LD_LicenceType = "PRD";
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "AU");
			var priceHeader1 = org.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_ValidFrom = new ZDateTime(2017, 1, 1);
			priceHeader1.L6_SystemCode = db.LD_Product;
			var price1 = priceHeader1.Items.AddNew();
			price1.L7_Category = "ABC";
			price1.L7_Code = "P01";
			price1.L7_Description = "Core, Pack,";
			var price2 = priceHeader1.Items.AddNew();
			price2.L7_Category = "ABC";
			price2.L7_Code = "P02";
			price2.L7_Description = "Shipment and booking (include Consol & Spot Quotes for free)";
			Factory.Save();
			var link = BillingTestHelper.CreatePriceLink(db, priceHeader1, new ZDateTime(2017, 1, 1));
			BillingTestHelper.CreateChargeableUsage(Factory, db.LD_Product, "P01", new ZDateTime(2017, 11, 1), clientCompany, 23);
			BillingTestHelper.CreateChargeableUsage(Factory, db.LD_Product, "P02", new ZDateTime(2017, 11, 1), clientCompany, 189);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			EServicesBillingTestHelper.CreateTable();
		}

		public override void RunBare()
		{
			base.RunBare();
			EServicesBillingTestHelper.DropTable();
		}
	}
}
