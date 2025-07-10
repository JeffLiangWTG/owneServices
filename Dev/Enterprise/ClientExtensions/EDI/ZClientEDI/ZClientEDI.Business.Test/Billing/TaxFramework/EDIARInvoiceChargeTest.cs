using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business.Accounting;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class EDIARInvoiceChargeTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var org = testObjectCreator.Debtor;

			var currency = testObjectCreator.USD;
			var exchangeRate = 0.25m;
			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("AR001", currency, exchangeRate, org);
			var amount = 15;

			for (var idx = 0; idx < 5; idx++)
			{
				testObjectCreator.CreateInvoiceLine(invoice, currency, exchangeRate, amount);
				amount += 5;
			}

			Factory.Save();
			AssertEquals(5, invoice.Lines.Count);

			foreach (ARInvoiceLine line in invoice.Lines)
			{
				AssertCharge(new EDIARInvoiceCharge(line), line);
			}
		}

		void AssertCharge(EDIARInvoiceCharge charge, ARInvoiceLine line)
		{
			AssertEquals(ZDecimal.Zero, charge.OsExTaxAmount);
			AssertEquals(ZDecimal.Zero, charge.OsTaxAmount);
			AssertEquals(line.Invoice.TransactionCurrency, charge.SellCurrency);
			AssertEquals(line.Invoice.Header, charge.Debtor);
			AssertEquals(ZGuid.Empty, charge.DebtorAddressPK);
			AssertEquals(ZGuid.Empty, charge.DebtorContactPK);
			AssertEquals(line.AL_AT, charge.TaxRate);
			AssertEquals(line.AL_OSExTaxAmount, charge.OSSellAmount);
			AssertEquals(line.AL_LocalExTaxAmount, charge.LocalSellAmount);
			AssertEquals(line.AL_AT, charge.SellGSTRate);
			AssertEquals(line.AL_TaxDate, charge.SellTaxDate);
			AssertEquals(ZGuid.Empty, charge.SellTaxMessage);
			AssertEquals(line.AL_AW, charge.SellWHTRate);
			AssertEquals(line.AL_PreventInvoicePrintGrouping, charge.PreventInvoicePrintGrouping);
			AssertEquals(line.AL_ExchangeRate, charge.SellExchangeRate);
			AssertEquals(line.Invoice.AH_ExchangeRate, charge.InvoiceSellExchangeRate);
			AssertEquals(line.AL_OSTaxAmount, charge.OSSellTaxAmount);
			AssertEquals(line.AL_LocalTaxAmount, charge.LocalSellTaxAmount);
			AssertEquals(line.AL_OSWHTAmount, charge.OSSellWHTAmount);
			AssertEquals(ZDecimal.Zero, charge.CFXAmount);
			AssertEquals(false, charge.IsLocalClientCharge);
			AssertEquals(false, charge.IsAgentCharge);
			AssertEquals(false, charge.IsDeferredCharge);
			AssertEquals(false, charge.IsRevenuePosted);
			AssertEquals(false, charge.IsParentJobWorkOnHold);
			AssertEquals(false, charge.IsParentJobInvoicingOnHold);
			AssertEquals(line.Invoice.InvoiceDate, charge.ARInvoiceDate);
			AssertEquals(ZString.Empty, charge.SellReference);
			AssertEquals(line.TaxRate?.AT_PostingGroupId ?? ZShort.Zero, charge.TaxRatePostingGroupId);
			AssertEquals(line.IsCommentCharge, charge.IsCommentChargeCode);
			AssertEquals(line.HasErrors, charge.HasErrors);
			AssertEquals(true, charge.IsAllowedToPostSellCharge);
			AssertEquals(line.AL_GovtChargeCode, charge.GovtChargeCode);
			AssertEquals(line.AL_PlaceOfSupply, charge.SellPlaceOfSupply);
			AssertEquals(line.AL_SupplyType, charge.SellSupplyType);
			AssertEquals(line.AL_GB_TaxBranch, charge.SellTaxBranch);
			AssertEquals(line.Invoice.AH_TransactionCategory, charge.InvoiceType);
			AssertEquals(line.ChargeCode?.AC_ChargeType ?? ZString.Empty, charge.ChargeType);
			AssertEquals(line.ChargeCode?.AC_ChargeGroup ?? ZString.Empty, charge.ChargeGroup);
			AssertEquals(false, charge.IsDisbursementCharge);
			AssertEquals(null, charge.Job);
			AssertEquals(line.AL_AC, charge.ChargeCode);
			AssertEquals(line.AL_RX_NKTransactionCurrency == line.Invoice.Company.GC_RX_NKLocalCurrency, charge.BillInLocalCurrency);
			AssertEquals(line.AL_GB, charge.Branch);
			AssertEquals(line.AL_GE, charge.Department);
			AssertEquals(line.AL_Sequence, charge.DisplaySequence);

			charge.CreateCFXTransactionLine(null, ZDateTime.Empty);
			charge.InitializeSellAddressContact();
			AssertEquals(false, charge.IsSisterCompanyCharge(false));
			charge.SetRevenueTransactionLine(line);
			AssertEquals(null, charge.SuspendAutoCalculations());
			charge.ValidateAll();

			var complianceDescription = (ISellComplianceDescription)charge;
			AssertEquals(line.AL_Desc, complianceDescription.Description);
			AssertEquals(ZString.Empty, complianceDescription.SellComplianceDescription);
		}
	}
}
