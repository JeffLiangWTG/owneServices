using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ClientMappingUsage))]
	internal class ClientMappingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPriceItem()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(licHeader);
			var licCompany = licHeader.Company;
			var org = licCompany.Header;

			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;
			var price1 = prices.Items.AddNew();
			price1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			price1.L7_Ref4 = "Interface1";
			price1.L7_Price = 11;

			var price2 = prices.Items.AddNew();
			price2.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			price2.L7_Ref4 = "Interface2";
			price2.L7_Price = 12;

			var priceGeneric = prices.Items.AddNew();
			priceGeneric.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceGeneric.L7_Price = 12;

			Factory.Save();

			var usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "INTERFACE1";
			AssertEquals(price1.PK, usage.PriceItem.PK);

			usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "interface2";
			AssertEquals(price2.PK, usage.PriceItem.PK);

			usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "Other";
			AssertEquals(priceGeneric.PK, usage.PriceItem.PK);
		}

		public void TestPriceItemFallbackToOnDemandPriceList()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(licHeader);
			var licCompany = licHeader.Company;
			var org = licCompany.Header;

			var prices = licCompany.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;
			var price1 = prices.Items.AddNew();
			price1.L7_Description = "Interface1 Desc";
			price1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			price1.L7_Ref4 = "Interface1";
			price1.L7_Price = 11;

			var price2 = prices.Items.AddNew();
			price2.L7_Description = "Interface2 Desc";
			price2.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			price2.L7_Ref4 = "Interface2";
			price2.L7_Price = 12;

			var pricesOnDemand = licCompany.PriceHeaders.AddNew();
			pricesOnDemand.L6_RX_NKCurrency = "AUD";
			pricesOnDemand.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			pricesOnDemand.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;

			var priceOnDemand = pricesOnDemand.Items.AddNew();
			priceOnDemand.L7_Description = "eHub Transactions";
			priceOnDemand.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceOnDemand.L7_Price = 13;

			Factory.Save();

			var usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "INTERFACE1";
			AssertEquals(price1.PK, usage.PriceItem.PK);
			AssertEquals("Interface1 Desc", usage.GetGeneralSummarySections()[0].Lines[0].MainDescription);

			usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "Interface2";
			AssertEquals(price2.PK, usage.PriceItem.PK);
			AssertEquals("Interface2 Desc", usage.GetGeneralSummarySections()[0].Lines[0].MainDescription);

			usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "New Interface";
			AssertEquals(priceOnDemand.PK, usage.PriceItem.PK);

			AssertEquals("New Interface", usage.GetGeneralSummarySections()[0].Lines[0].MainDescription);
		}

		public void TestPriceItemFallbackToOnDemandPriceList_NoHubPriceList()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(licHeader);
			var licCompany = licHeader.Company;
			var org = licCompany.Header;

			var pricesOnDemand = licCompany.PriceHeaders.AddNew();
			pricesOnDemand.L6_RX_NKCurrency = "AUD";
			pricesOnDemand.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			pricesOnDemand.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;

			var priceOnDemand = pricesOnDemand.Items.AddNew();
			priceOnDemand.L7_Description = "eHub Transactions";
			priceOnDemand.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			priceOnDemand.L7_Price = 13;

			Factory.Save();

			var usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "INTERFACE1";
			AssertEquals(priceOnDemand.PK, usage.PriceItem.PK);
			AssertEquals("INTERFACE1", usage.GetGeneralSummarySections()[0].Lines[0].MainDescription);

			usage = new ClientMappingUsage(BillingConstants.BillingSystem.ClientMapping, Factory, user, new ZDateTime(2015, 1, 1));
			usage.SubCode = "Interface2";
			AssertEquals(priceOnDemand.PK, usage.PriceItem.PK);
			AssertEquals("Interface2", usage.GetGeneralSummarySections()[0].Lines[0].MainDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ClientMappingUsage("AAA", Factory, new UsingParty(), EdiDateTest.MonthToday);
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
