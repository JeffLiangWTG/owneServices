using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(NZCustomsBill))]
	public class NZCustomsBillTest : TransactionalSystemBillTestCase<NZCustomsBill>
	{
		public void TestCreateInvoiceLines()
		{
			var org1 = SetupOrgWithPriceList("DDDAAASYD");
			var org2 = SetupOrgWithPriceList("DDDBBBMEL");

			var usageList = new List<NZCustomsUsage>();
			usageList.Add(new NZCustomsUsage("DEC", Factory, new UsingParty(org1), new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new NZCustomsUsage("CAR", Factory, new UsingParty(org1), new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new NZCustomsUsage("RES", Factory, new UsingParty(org1), new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new NZCustomsUsage("NZC", Factory, new UsingParty(org1), new ZDateTime(2014, 11, 30)) { TransactionCount = 100 });
			usageList.Add(new NZCustomsUsage("DEC", Factory, new UsingParty(org2), new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });

			var bill = new NZCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(4, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("NZCUSJOB", lines[0].ChargeCodeName);
				AssertEquals("50 NZ Customs CUSDEC Jobs at NZD 0.25 per Job", lines[0].Description);
				AssertEquals("NZCUSJOB", lines[1].ChargeCodeName);
				AssertEquals("10 NZ Customs CUSCAR Jobs at NZD 0.25 per Job", lines[1].Description);
				AssertEquals("NZCUSJOB", lines[2].ChargeCodeName);
				AssertEquals("10 NZ Customs CUSRES Jobs at NZD 0.25 per Job", lines[2].Description);
				AssertEquals("NZCUSMSG", lines[3].ChargeCodeName);
				AssertEquals("100 NZ Customs Messages at NZD 0.03 per Message", lines[3].Description);
			});
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<NZCustomsUsage>();
			usageList.Add(new NZCustomsUsage("DEC", Factory, new UsingParty(org), new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new NZCustomsUsage("CAR", Factory, new UsingParty(org), new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new NZCustomsUsage("RES", Factory, new UsingParty(org), new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new NZCustomsUsage("NZC", Factory, new UsingParty(org), new ZDateTime(2014, 11, 30)) { TransactionCount = 100 });

			var bill = new NZCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);

			AssertEquals(2, sections.Length);
			var section1 = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("NZ Customs Jobs", section1.Header.MainDescription);
				AssertEquals("NZ Customs CUSDEC Job", section1.Lines[0].MainDescription);
				AssertEquals("NZ Customs CUSCAR Job", section1.Lines[1].MainDescription);
				AssertEquals("NZ Customs CUSRES Job", section1.Lines[2].MainDescription);
			});

			var section2 = sections[1];
			CombineAssertions(() =>
			{
				AssertEquals("NZ Customs Messages", section2.Header.MainDescription);
				AssertEquals("NZ Customs Message", section2.Lines[0].MainDescription);
			});
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
			priceHeader.L6_RX_NKCurrency = "NZD";
			BillingTestHelper.AddPriceItem(priceHeader, "DEC", BillingConstants.FeeType.Transactional, "", 0.25m).L7_Description = "NZ Customs CUSDEC Job";
			BillingTestHelper.AddPriceItem(priceHeader, "CAR", BillingConstants.FeeType.Transactional, "", 0.25m).L7_Description = "NZ Customs CUSCAR Job";
			BillingTestHelper.AddPriceItem(priceHeader, "RES", BillingConstants.FeeType.Transactional, "", 0.25m).L7_Description = "NZ Customs CUSRES Job";
			BillingTestHelper.AddPriceItem(priceHeader, "NZC", BillingConstants.FeeType.Transactional, "", 0.03m).L7_Description = "NZ Customs Message";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		protected override NZCustomsBill GetNewSystemBill()
		{
			return new NZCustomsBill(Factory);
		}
	}
}
