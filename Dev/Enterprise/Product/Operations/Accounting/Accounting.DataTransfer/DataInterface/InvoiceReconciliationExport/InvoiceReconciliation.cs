using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class InvoiceReconciliation : AutoInvoiceReconciliation
	{
		public InvoiceReconciliation()
		{
		}

		public InvoiceReconciliation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void SetReconciliationValue(VoucherKingDeeK3 voucher)
		{
			VoucherDate = voucher.VoucherDate;
			TransDate = voucher.VoucherDate;
			Period = Convert.ToInt32(voucher.Period.ToString().Substring(4, 2), CultureInfo.InvariantCulture);
			VoucherTypeNumber = voucher.VoucherTypeNumber;
			VoucherNumber = voucher.VoucherNumber;
			VoucherLineNumber = voucher.VoucherLineNumber;
			VoucherDescription = voucher.VoucherDescription;
			VoucherDocNumber = voucher.VoucherDocNumber;
			CurrencyCode = voucher.CurrencyCode;
			ExchangeRate = voucher.ExRate;
			DebitOrCredit = voucher.DebitCurrencyAmount > 0 ? "D" : "C";
			LocalAmount = voucher.DebitAmountLocalCurrency > 0 ? voucher.DebitAmountLocalCurrency : voucher.CreditAmountLocalCurrency;
			DebitCurrencyAmount = voucher.DebitCurrencyAmount;
			CreditCurrencyAmount = voucher.CreditCurrencyAmount;
			BankDeposit = ListVoucherType.Contains(voucher.VoucherType) ? Res.GetString("f97921f0-8560-447b-b635-b65f460a9cbc", "银行存款") : string.Empty;
			PreparedBy = voucher.PreparedBy;
			CheckedBy = voucher.Reviwer;
			Attachments = voucher.Attachments;
			PostDate = voucher.PostDate;
			InvoiceDate = voucher.InvoiceDate;
			AccountingItem = voucher.AccountingItem;
			Year = voucher.FinancialYear;
			TransactionIndex = voucher.TransactionIndex;
			GLAccountNumber = FormatGLAccountNum(voucher.GLAccountNumber);
			CalculatedCurrencyCode = voucher.CalculatedCurrencyCode;
			CalculatedExchangeRate = voucher.CalculatedExchangeRate;
			CalculatedDebitAmount = voucher.DebitAmountLocalCurrency;
			CalculatedCreditAmount = voucher.CreditAmountLocalCurrency;
			CalculatedAmount = voucher.CalculatedAmount;
		}

		public List<string> ListVoucherType
		{
			get
			{
				if (listVoucherType == null)
				{
					listVoucherType = new List<string>() { "ARREC", "APREC", "CBDRC" };
				}
				return listVoucherType;
			}
		}
		List<string> listVoucherType;

		#region Implementation

		ZString FormatGLAccountNum(ZString accountNum)
		{
			ZString result = accountNum;
			if (accountNum.EndsWith("0000", StringComparison.Ordinal))
			{
				result = accountNum.Substring(0, accountNum.Length - 4);
			}
			else if (accountNum.EndsWith("00", StringComparison.Ordinal))
			{
				result = accountNum.Substring(0, accountNum.Length - 2);
			}

			return result;
		}

		#endregion
	}
}