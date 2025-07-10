using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(JapanAFRBill))]
	public class JapanAFRBillTest : TransactionalSystemBillTestCase<JapanAFRBill>
	{
		public void TestCreateInvoiceLines()
		{
			var org1 = SetupOrgWithPriceList("DDDAAASYD");
			var org2 = SetupOrgWithPriceList("DDDBBBMEL");

			var usageList = new List<JapanAFRUsage>();
			usageList.Add(new JapanAFRUsage(Factory, new UsingParty(org1), new ZDateTime(2014, 12, 31)) { TransactionCount = 50 });
			usageList.Add(new JapanAFRUsage(Factory, new UsingParty(org2), new ZDateTime(2014, 12, 31)) { TransactionCount = 20 });

			var bill = new JapanAFRBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(1, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("PDSMFJAFR", lines[0].ChargeCodeName);
				AssertEquals("Pre Departure Sea Manifest Filing - 70 Japan AFR Transactions at AUD 1.50 per Transaction", lines[0].Description);
			});
		}

		EDIOrgHeader SetupOrgWithPriceList(ZString orgCode)
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.AddPriceItem(priceHeader, "AFR", BillingConstants.FeeType.Transactional, "", 1.50m).L7_Description = "Pre Departure Sea Manifest Filing (AFR)";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		protected override JapanAFRBill GetNewSystemBill()
		{
			return new JapanAFRBill(Factory);
		}
	}
}
