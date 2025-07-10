using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class JobComInvChargeHelperTests : TestCaseWithFactory
	{
		public void TestGetCDSChargeDeductions()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			var charge2 = invoice.Charges.AddNew();
			var charge3 = invoice.Charges.AddNew();
			var charge4 = invoice.Charges.AddNew();
			var charge5 = invoice.Charges.AddNew();
			var charge6 = invoice.Charges.AddNew();
			var charge7 = invoice.Charges.AddNew();
			var charge8 = invoice.Charges.AddNew();
			InvChargeTestHelper.SetUpCharge(charge1, "CBR", false, 1.1m, string.Empty, true, 1m); //AB
			InvChargeTestHelper.SetUpCharge(charge2, "CBR", false, 1.1m, string.Empty, false, 2m); //AB
			InvChargeTestHelper.SetUpCharge(charge3, "OFT", true, 0m, "VAL", false, 3m); //AP
			InvChargeTestHelper.SetUpCharge(charge4, "OFT", true, 0m, "VAL", true, 4m); //BA
			InvChargeTestHelper.SetUpCharge(charge5, "OFT", true, 0m, "VAL", false, 5m); //AP
			InvChargeTestHelper.SetUpCharge(charge6, "CEA", false, 0m, string.Empty, false, 6m); //BB
			charge6.J7_IsGSTApplicable = true;
			InvChargeTestHelper.SetUpCharge(charge7, "CEA", false, 0m, string.Empty, false, 7m); //BB
			charge7.J7_IsStatisticalValueApplicable = true;
			InvChargeTestHelper.SetUpCharge(charge8, "OFT", true, 0m, "VAL", false, 5m); //AP
			charge8.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.China;

			var deductions = JobComInvChargeHelper.GetCDSChargeDeductions(invoice.Charges.Cast<InvoiceCharge>());

			CombineAssertions(() =>
			{
				AssertEquals("Count deductions", 5, deductions.Count);
				AssertEquals("AB", 3m, deductions["AB.GBP"].Amount);
				AssertEquals("AP", 8m, deductions["AP.GBP"].Amount);
				AssertEquals("BB", 13m, deductions["BB.GBP"].Amount);
				AssertEquals("AP", 5m, deductions["AP.CNY"].Amount);
				AssertEquals("BA", 4m, deductions["BA.GBP"].Amount);
			});
		}

		public void TestFreightDeductionChargesAreAggregated()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			var charge2 = invoice.Charges.AddNew();
			var charge3 = invoice.Charges.AddNew();
			var charge4 = invoice.Charges.AddNew();
			InvChargeTestHelper.SetUpCharge(charge1, "AFT", true, 1m, "VAL", false, 140m);
			InvChargeTestHelper.SetUpCharge(charge2, "AFT", false, 1m, "VAL", false, 60m);
			InvChargeTestHelper.SetUpCharge(charge3, "OFT", true, 1m, "VAL", false, 20m);
			InvChargeTestHelper.SetUpCharge(charge4, "OFT", false, 1m, "VAL", false, 30m);

			var incoTerms = new[]
			{
				Core.Constants.IncoTerms.ExWorks,
				Core.Constants.IncoTerms.FreeCarrier,
				Core.Constants.IncoTerms.FreeAlongsideShip,
				Core.Constants.IncoTerms.FreeOnBoard
			};

			CombineAssertions(() =>
			{
				foreach (var incoTerm in incoTerms)
				{
					invoice.JZ_IncoTerm = incoTerm;
					var deductions = JobComInvChargeHelper.GetCDSChargeDeductions(invoice.Charges.Cast<InvoiceCharge>());
					AssertEquals($"{incoTerm} Count", 2, deductions.Count);
					AssertEquals($"{incoTerm} AR", 200m, deductions["AR.GBP"].Amount);
					AssertEquals($"{incoTerm} AP", 50m, deductions["AP.GBP"].Amount);
				}
			});
		}

		public void TestAllFreightDeductionChargesAreAggregatedWhenIncluded()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			var charge2 = invoice.Charges.AddNew();
			var charge3 = invoice.Charges.AddNew();
			var charge4 = invoice.Charges.AddNew();
			InvChargeTestHelper.SetUpCharge(charge1, "AFT", true, 1m, "VAL", true, 140m);
			InvChargeTestHelper.SetUpCharge(charge2, "AFT", false, 1m, "VAL", true, 60m);
			InvChargeTestHelper.SetUpCharge(charge3, "OFT", true, 1m, "VAL", true, 20m);
			InvChargeTestHelper.SetUpCharge(charge4, "OFT", false, 1m, "VAL", true, 30m);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var deductions = JobComInvChargeHelper.GetCDSChargeDeductions(invoice.Charges.Cast<InvoiceCharge>());

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, deductions.Count);
				AssertEquals("BR", 200m, deductions["BR.GBP"].Amount);
				AssertEquals("BA", 50m, deductions["BA.GBP"].Amount);
			});
		}
	}
}
