using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(EServicesSystemUsage))]
	class EServicesSystemUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPriceHeader()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			var parentPriceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			parentPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			parentPriceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			parentPriceHeader.L6_RX_NKCurrency = "EUR";
			BillingTestHelper.AddPriceItem(parentPriceHeader, "A01", BillingConstants.FeeType.Transactional, "", 0.10m);

			EDIOrgHeader childOrganisation = BillingTestHelper.CreateDependentOrganisation(organisation, "BBB");
			var childPriceHeader = childOrganisation.LicCompany.PriceHeaders.AddNew();
			childPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			childPriceHeader.L6_ValidFrom = new ZDateTime(2014, 10, 1);
			childPriceHeader.L6_RX_NKCurrency = "EUR";
			BillingTestHelper.AddPriceItem(childPriceHeader, "A01", BillingConstants.FeeType.Transactional, "", 0.20m);

			var usage = new EServicesSystemUsage("DDD", "A01", Factory, new UsingParty(childOrganisation), new ZDateTime(2014, 11, 1));
			AssertEquals("Using own priceheader", childPriceHeader, usage.PriceHeader);

			usage = new EServicesSystemUsage("DDD", "A01", Factory, new UsingParty(childOrganisation), new ZDateTime(2014, 9, 1));
			AssertEquals("Using parent's priceheader", parentPriceHeader, usage.PriceHeader);

			usage = new EServicesSystemUsage("DDD", "A01", Factory, new UsingParty(childOrganisation), new ZDateTime(2013, 12, 1));
			AssertEquals("Both own and parent priceheaders do not have price", null, usage.PriceHeader);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EServicesSystemUsage("DDD", "A01", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
