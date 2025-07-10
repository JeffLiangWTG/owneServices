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
	[TestedType(typeof(RailincBill))]
	public class RailincBillTest : TransactionalSystemBillTestCase<RailincBill>
	{
		public void TestChargeCode()
		{
			var bill = new RailincBill(BillingConstants.BillingSystem.RailincByMessage, Factory);
			var systemCode = BillingConstants.BillingSystem.RailincByMessage;
			AssertEquals(EDIDataRegistry.Instance.TransactionChargeCodes.Value.GetDescriptionFromCode(systemCode), bill.GetAmountChargeCodeName(null));
		}

		public void TestCreateInvoiceLines()
		{
			var org = SetupOrgWithPriceList("DDDAAASYD");

			var usageList = new List<RailincUsage>();
			usageList.Add(new RailincUsage(Factory, new UsingParty(org), new ZDateTime(2014, 12, 31)) { TransactionCount = 50 });

			var bill = new RailincBill(BillingConstants.BillingSystem.RailincByMessage, Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(1, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Rail Container Event - 50 Transactions at USD 0.11 per Transaction", lines[0].Description);
				AssertEquals(5.5m, lines[0].Amount);
			});
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<RailincUsage>();
			usageList.Add(new RailincUsage(Factory, new UsingParty(org), new ZDateTime(2014, 12, 31)) { TransactionCount = 700 });

			var bill = new RailincBill(BillingConstants.BillingSystem.RailincByMessage, Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);
			AssertEquals(1, sections.Length);
			AssertEquals("Rail Container Event", sections[0].Header.MainDescription);
			AssertEquals("RAILINC Usage", sections[0].Lines[0].MainDescription);
		}

		EDIOrgHeader SetupOrgWithPriceList(string orgCode = "DDDCOMSYD")
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader, "RIC", BillingConstants.FeeType.Transactional, "", 0.11m).L7_Description = "Rail Container Event";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		protected override RailincBill GetNewSystemBill()
		{
			return new RailincBill(BillingConstants.BillingSystem.RailincByMessage, Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RailincBill(BillingConstants.BillingSystem.RailincByMessage, Factory);
		}
	}
}
