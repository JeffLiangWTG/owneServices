using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(OceanTracingBill))]
	public class OceanTracingBillTest : TransactionalSystemBillTestCase<OceanTracingBill>
	{
		public void TestCreateInvoiceLines()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<OceanTracingUsage>();
			usageList.Add(new OceanTracingUsage("OCT", Factory, new UsingParty(org), new ZDateTime(2015, 3, 31)) { TransactionCount = 30 });

			var bill = new OceanTracingBill("OCT", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(1, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("OCECONTRA", lines[0].ChargeCodeName);
				AssertEquals("Ocean Container Movement - 30 Transactions at NZD 0.05 per Transaction", lines[0].Description);
			});
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<OceanTracingUsage>();
			usageList.Add(new OceanTracingUsage("OCT", Factory, new UsingParty(org), new ZDateTime(2015, 3, 31)) { TransactionCount = 30 });

			var bill = new OceanTracingBill("OCT", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);

			AssertEquals(1, sections.Length);
			var section = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("Ocean Tracing Usage", section.Header.MainDescription);
				AssertEquals("Ocean Container Movement", section.Lines[0].MainDescription);
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
			priceHeader.L6_RX_NKCurrency = "NZD";
			BillingTestHelper.AddPriceItem(priceHeader, "OCT", BillingConstants.FeeType.Transactional, "", 0.05m).L7_Description = "Ocean Container Movement";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		protected override OceanTracingBill GetNewSystemBill()
		{
			return new OceanTracingBill("OCT", Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OceanTracingBill("OCT", Factory);
		}
	}
}
