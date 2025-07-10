using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(PortMessagingBill))]
	public class PortMessagingBillTest : TransactionalSystemBillTestCase<PortMessagingBill>
	{
		public void TestCreateInvoiceLines()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<PortMessagingUsage>();
			usageList.Add(new PortMessagingUsage("PM2", Factory, new UsingParty(org), new ZDateTime(2015, 4, 30)) { TransactionCount = 50 });
			usageList.Add(new PortMessagingUsage("PM1", Factory, new UsingParty(org), new ZDateTime(2015, 4, 30)) { TransactionCount = 30 });
			usageList.Add(new PortMessagingUsage("PM3", Factory, new UsingParty(org), new ZDateTime(2015, 4, 30)) { TransactionCount = 10 });

			var bill = new PortMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(3, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Forwarding Port Messaging - 30 Port Order with HDS at EUR 1.55 per Transaction", lines[0].Description);
				AssertEquals("Forwarding Port Messaging - 50 Other Message at EUR 0.78 per Transaction", lines[1].Description);
				AssertEquals("Forwarding Port Messaging - 10 Status Message at EUR 0.24 per Transaction", lines[2].Description);
			});
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<PortMessagingUsage>();
			usageList.Add(new PortMessagingUsage("PM2", Factory, new UsingParty(org), new ZDateTime(2015, 4, 30)) { TransactionCount = 50 });
			usageList.Add(new PortMessagingUsage("PM1", Factory, new UsingParty(org), new ZDateTime(2015, 4, 30)) { TransactionCount = 30 });
			usageList.Add(new PortMessagingUsage("PM3", Factory, new UsingParty(org), new ZDateTime(2015, 4, 30)) { TransactionCount = 10 });

			var bill = new PortMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);

			AssertEquals(1, sections.Length);
			var section = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage", section.Header.MainDescription);
				AssertEquals("Port Order with HDS", section.Lines[0].MainDescription);
				AssertEquals("Other Message", section.Lines[1].MainDescription);
				AssertEquals("Status Message", section.Lines[2].MainDescription);
			});
		}

		EDIOrgHeader SetupOrgWithPriceList()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDCOMSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceHeader.L6_RX_NKCurrency = "EUR";
			BillingTestHelper.AddPriceItem(priceHeader, "PM1", BillingConstants.FeeType.Transactional, "", 1.55m).L7_Description = "Port Messaging Data Record / Original Entry";
			BillingTestHelper.AddPriceItem(priceHeader, "PM2", BillingConstants.FeeType.Transactional, "", 0.78m).L7_Description = "Port Other Messages";
			BillingTestHelper.AddPriceItem(priceHeader, "PM3", BillingConstants.FeeType.Transactional, "", 0.24m).L7_Description = "Port Status Messages";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		protected override PortMessagingBill GetNewSystemBill()
		{
			return new PortMessagingBill(Factory);
		}
	}
}
