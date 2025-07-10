using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ShippingPortMessagingBill))]
	public class ShippingPortMessagingBillingTest : TransactionalSystemBillTestCase<ShippingPortMessagingBill>
	{
		public void TestCreateInvoiceLines()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<ShippingPortMessagingUsage>();
			usageList.Add(new ShippingPortMessagingUsage("SDT", Factory, new UsingParty(org), new ZDateTime(2016, 3, 1)) { TransactionCount = 30 });
			usageList.Add(new ShippingPortMessagingUsage("SPM", Factory, new UsingParty(org), new ZDateTime(2016, 3, 1)) { TransactionCount = 50 });

			var bill = new ShippingPortMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(2, lines.Count);
			AssertEquals("Import Delivery Order - Electronic (EIDO) - 30 Transactions at AUD 0.75 per Transaction", lines[0].Description);
			AssertEquals("Port Authority Messaging (Export Manifest) - 50 Transactions at AUD 0.25 per Transaction", lines[1].Description);
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<ShippingPortMessagingUsage>();
			usageList.Add(new ShippingPortMessagingUsage("SDT", Factory, new UsingParty(org), new ZDateTime(2016, 3, 1)) { TransactionCount = 30 });
			usageList.Add(new ShippingPortMessagingUsage("SPM", Factory, new UsingParty(org), new ZDateTime(2016, 3, 1)) { TransactionCount = 50 });

			var bill = new ShippingPortMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);

			AssertEquals(1, sections.Length);
			var section = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("Shipping Port Messaging Usage", section.Header.MainDescription);
				AssertEquals("Import Delivery Order - Electronic (EIDO)", section.Lines[0].MainDescription);
				AssertEquals("Port Authority Messaging (Export Manifest)", section.Lines[1].MainDescription);
			});
		}

		EDIOrgHeader SetupOrgWithPriceList()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDCOMSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = "DDD";

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.AddPriceItem(priceHeader, "SDT", BillingConstants.FeeType.Transactional, "", 0.75m).L7_Description = "Import Delivery Order - Electronic (EIDO)";
			BillingTestHelper.AddPriceItem(priceHeader, "SPM", BillingConstants.FeeType.Transactional, "", 0.25m).L7_Description = "Port Authority Messaging (Export Manifest)";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		protected override ShippingPortMessagingBill GetNewSystemBill()
		{
			return new ShippingPortMessagingBill(Factory);
		}
	}
}
