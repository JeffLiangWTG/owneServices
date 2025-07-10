using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	public static class DocARInvoiceCommonTestingExtentions
	{
		public static IDocARInvoiceLine[] GetLinesByOSEXTaxAmount(this DocARInvoiceLineCollection collection, decimal amount)
		{
			List<IDocARInvoiceLine> lines = new List<IDocARInvoiceLine>();
			foreach (IDocARInvoiceLine line in collection)
			{
				if (line.OSExTaxAmount == amount)
				{
					lines.Add(line);
				}
			}
			return lines.ToArray();
		}

		public static InvoicingLineBase[] GetLinesByChargeCodeAndOSExTaxAmount(this InvoicingLineBaseCollection collection, string code, decimal amount)
		{
			List<InvoicingLineBase> lines = new List<InvoicingLineBase>();
			foreach (InvoicingLineBase line in collection)
			{
				if (string.Compare(line.ChargeCode.AC_Code, code, StringComparison.InvariantCultureIgnoreCase) == 0 && line.AL_OSExTaxAmount == amount)
				{
					lines.Add(line);
				}
			}
			return lines.ToArray();
		}

		public static Charge AddNewWithSellValues(this ChargeCollection collection, ZGuid jR_AC, ZGuid jR_OH_SellAccount, ZDecimal jR_OSSellAmt, ZString jR_RX_NKSellCurrency, ZDecimal jR_OSSellExRate, string jR_Desc)
		{
			var newCharge = collection.AddNew();
			newCharge.JR_AC = jR_AC;
			newCharge.JR_OH_SellAccount = jR_OH_SellAccount;
			newCharge.JR_RX_NKSellCurrency = jR_RX_NKSellCurrency;
			newCharge.JR_Desc = jR_Desc;
			newCharge.JR_OSSellAmt = jR_OSSellAmt;
			//NewCharge.JR_OSSellExRate = JR_OSSellExRate;
			newCharge.RevenueExchangeRate?.SetBaseRate(jR_OSSellExRate);
			NUnit.Framework.Assertion.AssertNotEquals(newCharge.JR_LocalSellAmt, newCharge.JR_OSSellAmt);
			return newCharge;
		}

		public static ARInvoiceLine AddNewBasedOnChargeSellValues(this InvoicingLineBaseCollection collection, Charge sellCharge)
		{
			ARInvoiceLine line = (ARInvoiceLine)collection.AddNew();
			line.AL_Desc = sellCharge.JR_Desc;
			line.AL_OSExTaxAmount = sellCharge.JR_OSSellAmt;
			line.AL_AC = sellCharge.JR_AC;
			line.AL_RX_NKTransactionCurrency = sellCharge.SellCurrency.RX_Code;
			line.AL_ExchangeRate = sellCharge.JR_OSSellExRate;
			sellCharge.JR_AL_ARLine = line.PK;
			return line;
		}
	}
}
