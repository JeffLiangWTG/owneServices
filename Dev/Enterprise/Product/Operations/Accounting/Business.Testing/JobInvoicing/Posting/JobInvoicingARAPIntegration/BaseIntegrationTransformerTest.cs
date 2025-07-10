using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public abstract class BaseIntegrationTransformerTest : TransactionCreatorBaseTest
	{
		[ExpectNoExceptions]
		public void TestClearLinksFromNonJobRelatedInvoice()
		{
			APInvoice invoice = CreateAPInvoice("Z1000", Creditor1, USD, .7M, "Invoice Z1000");
			APInvoiceLine line1 = CreateAPInvoiceLine(invoice, null, CC5, USD, .7M, "Charge Code 5", 200M, false);
			APInvoiceLine line2 = CreateAPInvoiceLine(invoice, null, CC1, USD, .7M, "Charge Code 5", 150M, false);

			//make sure it doesn't die if the invoice is not job related
			GetTransformer(Factory).Transform(invoice);
		}

		#region Implementation

		protected abstract BaseIntegrationTransformer GetTransformer(BusinessObjectFactory factory);

		protected virtual Type GetInvoiceType()
		{
			return typeof(APInvoice);
		}

		protected virtual Type GetCreditNoteType()
		{
			return typeof(APCreditNote);
		}

		#region Creation of AP CreditNote
		protected APCreditNote CreateAPCreditNote(string creditNoteNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc)
		{
			var creditNote = (APCreditNote)Factory.New(GetCreditNoteType());
			creditNote.AH_InvoiceDate = ZDateTime.Today;
			creditNote.AH_TransactionNum = creditNoteNumber;
			creditNote.AH_OH = account.PK;
			creditNote.AH_RX_NKTransactionCurrency = currency.RX_Code;
			creditNote.AH_ExchangeRate = exchangeRate;
			creditNote.AH_Desc = desc;
			creditNote.AH_PostDate = ZDateTime.Today;
			creditNote.AH_DueDate = ZDateTime.Today.AddMonths(1);
			return creditNote;
		}

		protected APCreditNoteLine CreateAPCreditNoteLine(APCreditNote parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal aH_OSExTaxAmount, bool createJobCharge, bool isFinal = false)
		{
			var newLine = (APCreditNoteLine)parent.Lines.AddNew();

			if (job != null)
			{
				newLine.AL_JH = job.PK;
				if (createJobCharge)
				{
					TestObjectCreator.CreateJobCharge(newLine, job, chargeCode, currency);
				}
			}

			newLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			newLine.AL_AC = chargeCode.PK;
			if (desc != null)
			{
				newLine.AL_Desc = desc;
			}

			newLine.AL_GB = GlbBranch.CurrentBranch.PK;
			newLine.AL_GE = TestObjectCreator.FESDepartment.PK;
			newLine.AL_RX_NKTransactionCurrency = currency.RX_Code;
			newLine.AL_ExchangeRate = exchangeRate;
			parent.AH_ExchangeRate = exchangeRate;

			newLine.AL_OSExTaxAmount = aH_OSExTaxAmount;
			newLine.AL_AT = chargeCode.GSTRate.PK;
			newLine.AL_AW = chargeCode.WithholdingTaxRate.PK;
			newLine.AL_IsFinalCharge = isFinal;

			return newLine;
		}

		#endregion

		#region Creation of AP Invoice

		protected APInvoice CreateAPInvoice(string invoiceNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc, BusinessObjectFactory factory = null)
		{
			var invoice = (APInvoice)(factory ?? Factory).New(GetInvoiceType());
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_TransactionNum = invoiceNumber;
			invoice.AH_OH = account.PK;
			invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			invoice.AH_ExchangeRate = exchangeRate;
			invoice.AH_Desc = desc;
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_DueDate = ZDateTime.Today.AddMonths(1);
			return invoice;
		}

		protected APInvoiceLine CreateAPInvoiceLine(APInvoice parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal aH_OSExTaxAmount, bool isFinal)
		{
			var newLine = (APInvoiceLine)parent.Lines.AddNew();
			if (job != null)
			{
				newLine.AL_JH = job.PK;
			}

			newLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			newLine.AL_AC = chargeCode.PK;
			if (desc != null)
			{
				newLine.AL_Desc = desc;
			}

			newLine.AL_GB = GlbBranch.CurrentBranch.PK;
			newLine.AL_GE = TestObjectCreator.FESDepartment.PK;
			newLine.AL_RX_NKTransactionCurrency = currency.RX_Code;
			newLine.AL_ExchangeRate = exchangeRate;

			newLine.AL_OSExTaxAmount = aH_OSExTaxAmount;
			newLine.AL_AT = chargeCode.GSTRate.PK;
			newLine.AL_AW = chargeCode.WithholdingTaxRate.PK;

			newLine.AL_IsFinalCharge = isFinal;
			newLine.AL_PostDate = parent.AH_PostDate;

			return newLine;
		}

		#endregion

		#region Creation of AR Invoice

		protected ARInvoice CreateARInvoice(string invoiceNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc)
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_TransactionNum = invoiceNumber;
			invoice.AH_OH = account.PK;
			invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			invoice.AH_ExchangeRate = exchangeRate;
			if (desc != null)
			{
				invoice.AH_Desc = desc;
			}

			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_DueDate = ZDateTime.Today.AddMonths(1);
			return invoice;
		}

		protected ARInvoiceLine CreateARInvoiceLine(ARInvoice parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal aH_OSExTaxAmount)
		{
			var newLine = (ARInvoiceLine)parent.Lines.AddNew();
			if (job != null)
			{
				newLine.AL_JH = job.PK;
			}

			newLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			newLine.AL_AC = chargeCode.PK;
			if (desc != null)
			{
				newLine.AL_Desc = desc;
			}

			newLine.AL_GB = GlbBranch.CurrentBranch.PK;
			newLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			newLine.AL_RX_NKTransactionCurrency = currency.RX_Code;
			newLine.AL_ExchangeRate = exchangeRate;

			newLine.AL_OSExTaxAmount = aH_OSExTaxAmount;
			newLine.AL_AT = chargeCode.GSTRate.PK;
			newLine.AL_AW = chargeCode.WithholdingTaxRate.PK;

			return newLine;
		}

		#endregion

		#region Creation of AR Credit Note

		protected ARCreditNote CreateARCreditNote(string creditNoteNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc)
		{
			var creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_InvoiceDate = ZDateTime.Today;
			creditNote.AH_TransactionNum = creditNoteNumber;
			creditNote.AH_OH = account.PK;
			creditNote.AH_RX_NKTransactionCurrency = currency.RX_Code;
			creditNote.AH_ExchangeRate = exchangeRate;
			creditNote.AH_Desc = desc;
			creditNote.AH_PostDate = ZDateTime.Today;
			creditNote.AH_DueDate = ZDateTime.Today.AddMonths(1);
			return creditNote;
		}

		protected ARCreditNoteLine CreateARCreditNoteLine(ARCreditNote parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal aH_OSExTaxAmount)
		{
			var newLine = (ARCreditNoteLine)parent.Lines.AddNew();
			if (job != null)
			{
				newLine.AL_JH = job.PK;
			}

			newLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			newLine.AL_AC = chargeCode.PK;
			newLine.AL_Desc = desc;
			newLine.AL_GB = GlbBranch.CurrentBranch.PK;
			newLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			newLine.AL_RX_NKTransactionCurrency = currency.RX_Code;
			newLine.AL_ExchangeRate = exchangeRate;

			newLine.AL_OSExTaxAmount = aH_OSExTaxAmount;
			newLine.AL_AT = chargeCode.GSTRate.PK;
			newLine.AL_AW = chargeCode.WithholdingTaxRate.PK;

			return newLine;
		}
		#endregion

		protected void PostChargeRevenue(Charge charge, string invoiceNumber)
		{
			ARInvoice revenueInvoice = CreateARInvoice(invoiceNumber, charge.SellAccount, charge.SellCurrency, charge.JR_OSSellExRate, null);
			ARInvoiceLine revenueLine = CreateARInvoiceLine(revenueInvoice, charge.InvoicingJob, charge.ChargeCode, charge.SellCurrency, charge.JR_OSSellExRate, charge.JR_Desc, charge.JR_OSSellAmt);

			((IReceivablesPostingCharge)charge).SetRevenueTransactionLine(revenueLine);
		}

		protected void AssertCostChargeValues(Charge charge, AccChargeCode chargeCode, string description, OrgHeader creditor, bool isCostPosted,
			RefCurrency oSCostCurrency, decimal costExchangeRate, decimal oSCostAmount, decimal localCostAmt, AccTaxRate costTaxRate, decimal oSCostTaxAmount, AccWithholding costWHTRate, decimal oSCostWHTAmount,
			string aPInvoiceNumber)
		{
			AssertEquals("Charge Code", chargeCode.PK, charge.JR_AC);
			AssertEquals("Description", description ?? chargeCode.AC_Desc, charge.JR_Desc);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, charge.JR_GB);
			AssertEquals("Department", TestObjectCreator.FESDepartment.PK, charge.JR_GE);

			AssertEquals("Cost Currency", oSCostCurrency != null ? oSCostCurrency.RX_Code : ZString.Empty, charge.JR_RX_NKCostCurrency);
			AssertEquals("Cost Exchange Rate", costExchangeRate, charge.JR_OSCostExRate);
			AssertEquals("OS Cost Amount", oSCostAmount, charge.JR_OSCostAmt);
			AssertEquals("Creditor", creditor != null ? creditor.PK : ZGuid.Empty, charge.JR_OH_CostAccount);
			AssertEquals("Cost Posted", isCostPosted, charge.IsCostPosted);
			AssertEquals("Cost Tax Rate", costTaxRate != null ? costTaxRate.PK : ZGuid.Empty, charge.JR_AT_CostGSTRate);
			AssertEquals("OS Cost Tax Amount", oSCostTaxAmount, charge.JR_OSCostGSTAmt_Calc);
			AssertEquals("Cost WHT Rate", costWHTRate != null ? costWHTRate.PK : ZGuid.Empty, charge.JR_AW_CostWHTRate);
			AssertEquals("OS Cost WHT Amount", oSCostWHTAmount, charge.JR_OSCostWHTAmt);
			AssertEquals("Local Cost Amount", localCostAmt, charge.JR_LocalCostAmt);

			AssertEquals("AP Invoice Number", aPInvoiceNumber, charge.JR_APInvoiceNum);
		}

		protected void AssertCostChargeValues(Charge charge, AccChargeCode chargeCode, string description, OrgHeader creditor, bool isCostPosted,
			RefCurrency oSCostCurrency, decimal costExchangeRate, decimal oSCostAmount, decimal localCostAmt, AccTaxRate costTaxRate, decimal oSCostTaxAmount, AccWithholding costWHTRate, decimal oSCostWHTAmount,
			string aPInvoiceNumber, ZDateTime aPInvoiceDate, ZDateTime aPDueDate)
		{
			AssertCostChargeValues(charge, chargeCode, description, creditor, isCostPosted, oSCostCurrency, costExchangeRate, oSCostAmount,
				localCostAmt, costTaxRate, oSCostTaxAmount, costWHTRate, oSCostWHTAmount, aPInvoiceNumber);

			AssertEquals("AP Invoice Date", aPInvoiceDate, charge.JR_APInvoiceDate);
			AssertEquals("AP Due Date", aPDueDate, charge.JR_PaymentDate);
		}

		protected void AssertSellChargeValues(Charge charge, AccChargeCode chargeCode, string description, OrgHeader debtor, bool isSellPosted,
			RefCurrency oSSellCurrency, decimal oSSellExchangeRate, decimal oSSellAmount, decimal localSellAmount, AccTaxRate sellTaxRate, decimal oSSellTaxAmount, AccWithholding sellWHTRate, decimal oSSellWHTAmount, decimal estimatedRevenue)
		{
			AssertSellChargeValues(charge, chargeCode, description, debtor, isSellPosted, oSSellCurrency, oSSellExchangeRate, oSSellAmount, localSellAmount, sellTaxRate, oSSellTaxAmount, sellWHTRate, oSSellWHTAmount);

			AssertEquals("Estimated Revenue Amount", estimatedRevenue, charge.JR_EstimatedRevenue);
		}

		protected void AssertSellChargeValues(Charge charge, AccChargeCode chargeCode, string description, OrgHeader debtor, bool isSellPosted,
			RefCurrency oSSellCurrency, decimal oSSellExchangeRate, decimal oSSellAmount, decimal localSellAmount, AccTaxRate sellTaxRate, decimal oSSellTaxAmount, AccWithholding sellWHTRate, decimal oSSellWHTAmount,
			OrgAddress sellAddress = null, OrgContact sellContact = null)
		{
			AssertEquals("Charge Code", chargeCode.PK, charge.JR_AC);
			AssertEquals("Description", description ?? chargeCode.AC_Desc, charge.JR_Desc);
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, charge.JR_GB);
			AssertEquals("Department", TestObjectCreator.FESDepartment.PK, charge.JR_GE);

			AssertEquals("Sell Currency", oSSellCurrency != null ? oSSellCurrency.RX_Code : ZString.Empty, charge.JR_RX_NKSellCurrency);
			AssertEquals("OS Sell Exchange Rate", oSSellExchangeRate, charge.JR_OSSellExRate);
			AssertEquals("OS Sell Amount", oSSellAmount, charge.JR_OSSellAmt);
			AssertEquals("Debtor", debtor != null ? debtor.PK : ZGuid.Empty, charge.JR_OH_SellAccount);
			AssertEquals("JR_OA_SellInvoiceAddress", sellAddress != null ? sellAddress.PK : ZGuid.Empty, charge.JR_OA_SellInvoiceAddress);
			AssertEquals("JR_OC_SellInvoiceContact", sellContact != null ? sellContact.PK : ZGuid.Empty, charge.JR_OC_SellInvoiceContact);
			AssertEquals("Sell Posted", isSellPosted, charge.IsRevenuePosted);
			AssertEquals("Sell Tax Rate", sellTaxRate != null ? sellTaxRate.PK : ZGuid.Empty, charge.JR_AT_SellGSTRate);
			AssertEquals("OS Sell Tax Amount", oSSellTaxAmount, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Sell WHT Rate", sellWHTRate != null ? sellWHTRate.PK : ZGuid.Empty, charge.JR_AW_SellWHTRate);
			AssertEquals("OS Sell WHT Amount", oSSellWHTAmount, charge.JR_OSSellWHTAmt);
			AssertEquals("Local Sell Amount", localSellAmount, charge.JR_LocalSellAmt);
		}
		#endregion
	}
}