using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	public sealed class JobComInvoiceChargeHelperTests : TestCaseWithFactory
	{
		public void TestGetCDSChargeDeductions()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "V1";
			var invoice = dec.Invoices.AddNew();
			var charge1 = invoice.Charges.AddNew();
			var charge2 = invoice.Charges.AddNew();
			var charge3 = invoice.Charges.AddNew();
			var charge4 = invoice.Charges.AddNew();
			var charge5 = invoice.Charges.AddNew();
			var charge6 = invoice.Charges.AddNew();
			var charge7 = invoice.Charges.AddNew();
			var charge8 = invoice.Charges.AddNew();
			SetUpCharge(charge1, "1X", false, 1.1m, string.Empty, true, 1m);
			SetUpCharge(charge2, "1X", false, 1.1m, string.Empty, false, 2m);
			SetUpCharge(charge3, "AK", true, 0m, "VAL", false, 3m);
			SetUpCharge(charge4, "BA", true, 0m, "VAL", true, 4m);
			SetUpCharge(charge5, "AK", true, 0m, "VAL", false, 5m);
			SetUpCharge(charge6, "BB", false, 0m, string.Empty, false, 6m);
			charge6.J7_IsGSTApplicable = true;
			SetUpCharge(charge7, "BB", false, 0m, string.Empty, false, 7m);
			charge7.J7_IsStatisticalValueApplicable = true;
			SetUpCharge(charge8, "AK", true, 0m, "VAL", false, 5m);
			charge8.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.China;

			var deductions = JobComInvoiceChargeHelper.GetChargeDeductions(invoice.Charges.Cast<InvoiceCharge>());

			CombineAssertions(() =>
			{
				AssertEquals("Count deductions", 5, deductions.Count);
				AssertEquals("BA", 4m, deductions["BA.EUR"].Amount);
				AssertEquals("AK", 8m, deductions["AK.EUR"].Amount);
				AssertEquals("1X", 1m, deductions["1X.EUR"].Amount);
				AssertEquals("AK", 5m, deductions["AK.CNY"].Amount);
			});
		}

		void SetUpCharge(BaseJobComInvHeaderCharge charge, ZString chargeType, bool isDutiable, ZDecimal percentage,
			ZString distributeBy, bool isIncludeInvoiceLine, ZDecimal amount)
		{
			SetUp(charge, chargeType, isDutiable, percentage, distributeBy);
			charge.J7_IsIncludedInITOT = isIncludeInvoiceLine;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			charge.J7_Amount = amount;
		}

		void SetUp(BaseJobComInvHeaderCharge charge, ZString chargeType, bool isDutiable, ZDecimal percentage, ZString distributeBy)
		{
			charge.J7_ChargeType = chargeType;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_Percentage = percentage;
			charge.J7_DistributeBy = distributeBy;
		}
	}
}
