using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	[TestedType(typeof(HostingUsage))]
	internal class HostingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "PRD", false);
			lic.Database.LD_OH_BillingParty = lic.Company.LC_OH;
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, new ZDateTime(2015, 2, 1), lic, 1234);
			usage.U1_LC = ZGuid.Empty;
			usage.U1_LCC = ZGuid.Empty;

			var hostingUsage = new HostingUsage(Factory, usage);
			AssertEquals("org is set from database owner", lic.Company.LC_OH, hostingUsage.OrganisationPK);
			AssertEquals("lic company is set", lic.Company.PK, hostingUsage.LicCompany.PK);
			AssertEquals("server code", "PRD", hostingUsage.ServerCode);
		}

		public void TestSystemCode()
		{
			HostingUsage hostingUsage = CreateHostingUsage();
			AssertEquals(BillingConstants.BillingSystem.HostingStorage, hostingUsage.SystemCode);
		}

		public void TestPriceHeader()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			ClientLicencePriceHeader parentPriceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			parentPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(parentPriceHeader, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 10m);

			EDIOrgHeader childOrganisation = BillingTestHelper.CreateDependentOrganisation(organisation, "BBB");
			ClientLicencePriceHeader childPriceHeader = childOrganisation.LicCompany.PriceHeaders.AddNew();
			childPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(childPriceHeader, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 10m);

			HostingUsage hostingUsage = new HostingUsage(Factory, new UsingParty(childOrganisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 2000);
			AssertEquals("Using own priceheader 'cause it has a price", childPriceHeader, hostingUsage.PriceHeader);

			childOrganisation.LicCompany.InvoiceDeliveries[0].L9_UseParentPrices = true;
			hostingUsage = new HostingUsage(Factory, new UsingParty(childOrganisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 2000);
			AssertEquals("Using parent priceheader according to L9_UseParentPrices", parentPriceHeader, hostingUsage.PriceHeader);

			childOrganisation.LicCompany.InvoiceDeliveries[0].L9_UseParentPrices = false;
			childPriceHeader.Items.DeleteAll();
			BillingTestHelper.AddPriceItem(childPriceHeader, "XXX", BillingConstants.FeeType.Module, "", 10m);

			hostingUsage = new HostingUsage(Factory, new UsingParty(childOrganisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 2000);
			AssertEquals("Using parent's priceheader, because child has no price", parentPriceHeader, hostingUsage.PriceHeader);

			parentPriceHeader.Items.DeleteAll();
			hostingUsage = new HostingUsage(Factory, new UsingParty(childOrganisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 2000);
			AssertEquals("Both own and parent priceheaders do not have price => can't use them", null, hostingUsage.PriceHeader);
		}

		public void TestAmount()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			HostingUsage hostingUsage = new HostingUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20 * BillingConstants.Hosting.MBperGB);
			AssertEquals("Precondition: no priceheader", 0m, hostingUsage.Amount);

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20 * BillingConstants.Hosting.MBperGB);
			AssertEquals("Precondition: price not found", 0m, hostingUsage.Amount);

			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 16m);
			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20 * BillingConstants.Hosting.MBperGB);
			AssertEquals("amount", 2 * 16m, hostingUsage.Amount);
		}

		public void TestCurrencyCode()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			HostingUsage hostingUsage = new HostingUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20480);
			AssertEquals("No price header, no currency", "", hostingUsage.CurrencyCode);

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 10m);
			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20480);
			AssertEquals("AUD", hostingUsage.CurrencyCode);

			priceHeader.L6_RX_NKCurrency = "MDL";
			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20480);
			AssertEquals("MDL", hostingUsage.CurrencyCode);
		}

		public virtual void TestGetGeneralSummarySections()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			HostingUsage hostingUsage = new HostingUsage(Factory, new UsingParty(organisation), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20480);

			SummarySection[] summarySections = hostingUsage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 0, summarySections.Length);
		}

		public void TestGetInvoiceLineDescription()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "PRD");
			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 10m)
				.L7_Description = "WiseCloud Premium - High Speed Data Storage";
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.eDocsStorageCode, BillingConstants.FeeType.PerGBPerMonth, "", 7m)
				.L7_Description = "WiseCloud Premium - Image Storage";
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.RemoteDevicesCode, BillingConstants.FeeType.PerDevicePerMonth, "", 13m)
				.L7_Description = "WiseCloud Premium - Remote Device";
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.DataAccessCode, BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 0.01m)
				.L7_Description = "  WiseCloud Data Access";
			var wiseCloudUserItem = BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.Hosting.WiseCloudUserFeeCode, BillingConstants.FeeType.NamedUser, "", 7m);
			wiseCloudUserItem.L7_Description = "WiseCloud Licensing (3rd party costs) - Per Registered User";
			wiseCloudUserItem.L7_ChargeBasis = "Per Active WiseCloud User Record";

			HostingUsage hostingUsage = new HostingUsage(Factory, new UsingParty(organisation, "PRD"), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20480);
			string expected = "WiseCloud Premium - High Speed Data Storage (AAASYD-PRD Oct 2010)\r\n20.000 GB @ AUD 10.00 " + BillingConstants.FeeTypeDescriptions.Per10GBPerMonthMin1GB;
			AssertEquals(expected, hostingUsage.GetInvoiceLineDescription());

			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation, "PR2"), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.eDocsStorageCode, 20480);
			expected = "WiseCloud Premium - Image Storage (AAASYD-PR2 Oct 2010)\r\n20.000 GB @ AUD 7.00 " + BillingConstants.FeeTypeDescriptions.PerGBPerMonth;
			AssertEquals(expected, hostingUsage.GetInvoiceLineDescription());

			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation, "PRD"), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingRemoteDevices, BillingConstants.Hosting.RemoteDevicesCode, 4);
			expected = "WiseCloud Premium - Remote Device (AAASYD-PRD Oct 2010)\r\n4 unit(s) @ AUD 13.00 " + BillingConstants.FeeTypeDescriptions.PerDevicePerMonth;
			AssertEquals(expected, hostingUsage.GetInvoiceLineDescription());

			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation, "PRD"), new ZDateTime(2010, 10, 01), BillingConstants.BillingSystem.HostingDataAccess, BillingConstants.Hosting.DataAccessCode, 2000);
			expected = "WiseCloud Data Access (AAASYD-PRD Oct 2010)\r\n2000 MB @ AUD 0.01 " + BillingConstants.FeeTypeDescriptions.PerMBPerMonthMin1GB;
			AssertEquals(expected, hostingUsage.GetInvoiceLineDescription());

			hostingUsage = new HostingUsage(Factory, new UsingParty(organisation, "PRD"), new ZDateTime(2018, 10, 1), BillingConstants.BillingSystem.WiseCloudUser, BillingConstants.Hosting.WiseCloudUserFeeCode, 50);
			expected = "WiseCloud Licensing (3rd party costs) - Per Registered User (AAASYD-PRD Oct 2018)\r\n50 unit(s) @ AUD 7.00";
			AssertEquals(expected, hostingUsage.GetInvoiceLineDescription());
		}

		#region Implementation

		HostingUsage CreateHostingUsage()
		{
			return new HostingUsage(Factory, new UsingParty(), EdiDateTest.MonthToday, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 20480);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateHostingUsage();
		}

		#endregion
	}
}
