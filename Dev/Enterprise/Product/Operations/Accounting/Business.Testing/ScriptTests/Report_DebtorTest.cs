using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_DebtorTest : ScriptTest
	{
		public void TestReportData()
		{
			const decimal assumedGST = 10;

			var debtor1 = TestObjectCreator.CreateOrgHeader("DBT1", false, true);
			debtor1.OH_FullName = "Company X";
			debtor1.OH_IsForwarder = true;
			var firstInvDate1 = new ZDateTime(2017, 10, 11, 16, 42, 0);
			var exGstAmount1 = 60m;

			PrepTestInvoicesForDebtor(debtor1, firstInvDate1, exGstAmount1);

			var debtor2 = TestObjectCreator.CreateOrgHeader("DBT2", false, true);
			debtor2.OH_FullName = "Company Y";
			debtor2.OH_IsMiscFreightServices = true;
			debtor2.OH_IsBroker = true;
			var firstInvDate2 = new ZDateTime(2017, 12, 01, 6, 34, 0);
			var exGstAmount2 = 180m;

			PrepTestInvoicesForDebtor(debtor2, firstInvDate2, exGstAmount2);

			Factory.Save();

			var result = RunScript();

			AssertEquals("Result should contain 2 record", 2, result.Rows.Count);

			foreach (DataRow r in result.Rows)
			{
				if (r["OrgCode"].ToString() == debtor1.OH_Code)
				{
					AssertRowIsCorrect(r, debtor1, firstInvDate1, exGstAmount1 * (1 + assumedGST / 100));
				}
				else if (r["OrgCode"].ToString() == debtor2.OH_Code)
				{
					AssertRowIsCorrect(r, debtor2, firstInvDate2, exGstAmount2 * (1 + assumedGST / 100));
				}
				else
				{
					Fail($"Unexpected org code: {r["OrgCode"].ToString()}");
				}
			}
		}

		void AssertRowIsCorrect(DataRow r, OrgHeader debtor, ZDateTime expectedFirstInvoiceDate, ZDecimal expectedFirstInvoiceAmount)
		{
			AssertEquals("Invoice date should be the earliest", expectedFirstInvoiceDate, r["InvoiceDate"]);
			AssertEquals("Local Amount should include GST", expectedFirstInvoiceAmount, r["LocalAmount"]);

			AssertEquals("Organization name should be correct", debtor.OH_FullName, r["OrgName"]);
			AssertEquals("Organization flags should be correct", debtor.CompanyData.OB_IsCreditor ? "Y" : string.Empty, r["Payables"]);
			AssertEquals("Organization flags should be correct", debtor.CompanyData.OB_IsDebtor ? "Y" : string.Empty, r["Receivables"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsConsignee ? "Y" : string.Empty, r["Consignee"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsConsignor ? "Y" : string.Empty, r["Consignor"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsTransportClient ? "Y" : string.Empty, r["TransportClient"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsWarehouseClient ? "Y" : string.Empty, r["WarehouseClient"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsBroker ? "Y" : string.Empty, r["Broker"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsForwarder ? "Y" : string.Empty, r["Forwarder"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsCompetitor ? "Y" : string.Empty, r["Competitor"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsSalesLead ? "Y" : string.Empty, r["Sales"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsMiscFreightServices ? "Y" : string.Empty, r["Services"]);
			AssertEquals("Organization flags should be correct", debtor.OH_IsShippingProvider ? "Y" : string.Empty, r["Carrier"]);
		}

		void PrepTestInvoicesForDebtor(OrgHeader debtor, ZDateTime firstInvDate, ZDecimal exTaxAmount)
		{
			for (var i = 0; i < 10; i++)
			{
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), i.ToString(), TestObjectCreator.AUD, 1, exTaxAmount - i, 10, exTaxAmount - i, 10, debtor, TestObjectCreator.DSBChargeCode.PK);
				arInvoice.AH_InvoiceDate = firstInvDate.AddHours(i);
			}
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, $"SELECT * FROM Report_Debtors('{GlbCompany.CurrentCompany.PK}', '2017-10-01', '2018-01-01')");
		}
	}
}
