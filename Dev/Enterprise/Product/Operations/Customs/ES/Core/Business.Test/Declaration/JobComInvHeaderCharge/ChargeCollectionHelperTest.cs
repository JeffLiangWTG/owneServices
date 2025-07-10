using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class ChargeCollectionHelperTest : TestCaseWithFactory
	{
		public void TestAmountToAddToITOTForStatisticalChargesES()
		{
			invoice.JZ_IncoTerm = "FOB";
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.EuropeanUnion;
			CombineAssertions(() =>
			{
				var charge = AddNewCharge(ESCustomsChargeTypeList.Codes.InsuranceUntilESBorder, false, false, true, 15);
				AssertEquals("Statistical Value Calculator for ChargeCollection for Export Charge Addition (15)", 15m, invoiceLine.Charges.AmountToAddToITOTForStatisticalChargesES(invoice.LocalCurrency, invoiceLine.Charges.CurrencyConverter));
				ClearAllCharges();
				charge = AddNewCharge(ESCustomsChargeTypeList.Codes.InternationalFreightExp, true, true, true, 20);
				AssertEquals("Statistical Value Calculator for ChargeCollection for Export Charge Deduction (-20)", -20m, invoiceLine.Charges.AmountToAddToITOTForStatisticalChargesES(invoice.LocalCurrency, invoiceLine.Charges.CurrencyConverter));
				ClearAllCharges();
				charge = AddNewCharge(ESCustomsChargeTypeList.Codes.TransportCostsUntilESBorder, false, false, false, 25);
				AssertEquals("Statistical Value Calculator for ChargeCollection for Charge Ignored (0)", 0m, invoiceLine.Charges.AmountToAddToITOTForStatisticalChargesES(invoice.LocalCurrency, invoiceLine.Charges.CurrencyConverter));
			});
		}

		void ClearAllCharges()
		{
			foreach (var chargeKey in invoiceLine.Charges.AllChargeKeys)
			{
				invoiceLine.Charges.ClearCharge(chargeKey.ChargeKey);
			}
		}

		InvoiceLineCharge AddNewCharge(string chargeCode, bool isDutiable, bool isIncludedInTOT, bool isStatisticalValueApplicable, ZDecimal amount)
		{
			var newCharge = invoiceLine.Charges.AddNew();
			newCharge.J7_ChargeType = chargeCode;
			newCharge.J7_IsDutiable = isDutiable;
			newCharge.J7_IsIncludedInITOT = isIncludedInTOT;
			newCharge.J7_Amount = amount;
			newCharge.J7_IsStatisticalValueApplicable = isStatisticalValueApplicable;
			newCharge.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			return newCharge;
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			invoice = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
