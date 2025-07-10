using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public static class TxnHeaderMapper
	{
		public static Type GetBizObjTypeFromIValueObject(Xsd.TxnHeader xmlTxnHeader, bool crossLedgerTransaction)
		{
			string ledger = xmlTxnHeader.Ledger.ToString();

			if (crossLedgerTransaction)
			{
				ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			}

			return GetBizObjTypeFromValueObjectInternal(ledger, xmlTxnHeader.TxnType.ToString());
		}

		public static Type GetBizObjTypeFromIValueObject(Xsd.TxnHeader xmlTxnHeader)
		{
			return GetBizObjTypeFromValueObjectInternal(xmlTxnHeader.Ledger.ToString(), xmlTxnHeader.TxnType.ToString());
		}

		static Type GetBizObjTypeFromValueObjectInternal(ZString ledger, ZString transactionType)
		{
			Type typeToReturn = null;

			if (ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
				switch (transactionType)
				{
					case ZArchitecture.Core.TransactionTypes.Invoice:
						typeToReturn = typeof(ARInvoice);
						break;

					case ZArchitecture.Core.TransactionTypes.CreditNote:
						typeToReturn = typeof(ARCreditNote);
						break;

					case ZArchitecture.Core.TransactionTypes.AdjustmentNote:
						typeToReturn = typeof(ARAdjustmentNote);
						break;

					case ZArchitecture.Core.TransactionTypes.Journal:
						typeToReturn = typeof(ARJournal);
						break;

					case TransactionTypes.Payment:
						typeToReturn = typeof(ARPayment);
						break;

					case TransactionTypes.Receipt:
						typeToReturn = typeof(ARReceipt);
						break;

					case TransactionTypes.Overpayment:
						typeToReturn = typeof(AROverpayment);
						break;

					case TransactionTypes.Discount:
						typeToReturn = typeof(ARDiscount);
						break;

					case TransactionTypes.ExchangeDifference:
						typeToReturn = typeof(ARExchangeDifference);
						break;
				}
			}
			else if (ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
			{
				switch (transactionType)
				{
					case ZArchitecture.Core.TransactionTypes.Invoice:
						typeToReturn = typeof(APInvoice);
						break;

					case ZArchitecture.Core.TransactionTypes.CreditNote:
						typeToReturn = typeof(APCreditNote);
						break;

					case ZArchitecture.Core.TransactionTypes.AdjustmentNote:
						typeToReturn = typeof(APAdjustmentNote);
						break;

					case ZArchitecture.Core.TransactionTypes.Journal:
						typeToReturn = typeof(APJournal);
						break;

					case TransactionTypes.Payment:
						typeToReturn = typeof(APPayment);
						break;

					case TransactionTypes.Receipt:
						typeToReturn = typeof(APReceipt);
						break;

					case TransactionTypes.Overpayment:
						typeToReturn = typeof(APOverpayment);
						break;

					case TransactionTypes.Discount:
						typeToReturn = typeof(APDiscount);
						break;

					case TransactionTypes.ExchangeDifference:
						typeToReturn = typeof(APExchangeDifference);
						break;
				}
			}
			else if (ledger == ZArchitecture.Core.LedgerTypes.CashBook)
			{
				switch (transactionType)
				{
					case ZArchitecture.Core.TransactionTypes.DirectPayment:
						typeToReturn = typeof(DirectPayment);
						break;

					case ZArchitecture.Core.TransactionTypes.DirectReceipt:
						typeToReturn = typeof(DirectReceipt);
						break;
				}
			}
			else if (ledger == ZArchitecture.Core.LedgerTypes.TransactionsPendingAllocation)
			{
				typeToReturn = typeof(TransactionPendingAllocation);
			}

			return typeToReturn;
		}

		public static Xsd.TxnLedgerType GetTxnHeaderLedger(ZString transactionLedger)
		{
			Xsd.TxnLedgerType result;

			switch (transactionLedger)
			{
				case ZArchitecture.Core.LedgerTypes.AccountsPayable:
					result = Xsd.TxnLedgerType.AP;
					break;

				case ZArchitecture.Core.LedgerTypes.AccountsReceivable:
					result = Xsd.TxnLedgerType.AR;
					break;

				case ZArchitecture.Core.LedgerTypes.CashBook:
					result = Xsd.TxnLedgerType.CB;
					break;

				case ZArchitecture.Core.LedgerTypes.TransactionsPendingAllocation:
					result = Xsd.TxnLedgerType.PA;
					break;

				default:
					throw new ArgumentException("The following Ledger type is not recognised: '" + transactionLedger + "'", nameof(transactionLedger));
			}

			return result;
		}

		public static Xsd.TxnType GetTxnHeaderTxnType(ZString transactionType)
		{
			Xsd.TxnType result;

			switch (transactionType)
			{
				case ZArchitecture.Core.TransactionTypes.Invoice:
					result = Xsd.TxnType.INV;
					break;

				case ZArchitecture.Core.TransactionTypes.CreditNote:
					result = Xsd.TxnType.CRD;
					break;

				case ZArchitecture.Core.TransactionTypes.AdjustmentNote:
					result = Xsd.TxnType.ADJ;
					break;

				case ZArchitecture.Core.TransactionTypes.Contra:
					result = Xsd.TxnType.CTR;
					break;

				case ZArchitecture.Core.TransactionTypes.Journal:
					result = Xsd.TxnType.JNL;
					break;

				case ZArchitecture.Core.TransactionTypes.OpeningPayment:
					result = Xsd.TxnType.OPY;
					break;

				case ZArchitecture.Core.TransactionTypes.OpeningReceipt:
					result = Xsd.TxnType.ORC;
					break;

				case ZArchitecture.Core.TransactionTypes.Payment:
					result = Xsd.TxnType.PAY;
					break;

				case ZArchitecture.Core.TransactionTypes.Receipt:
					result = Xsd.TxnType.REC;
					break;

				case ZArchitecture.Core.TransactionTypes.Transfer:
					result = Xsd.TxnType.TRF;
					break;

				case ZArchitecture.Core.TransactionTypes.DirectReceipt:
					result = Xsd.TxnType.DRC;
					break;

				case ZArchitecture.Core.TransactionTypes.DirectPayment:
					result = Xsd.TxnType.DPY;
					break;

				case ZArchitecture.Core.TransactionTypes.Discount:
					result = Xsd.TxnType.DSC;
					break;

				case ZArchitecture.Core.TransactionTypes.InvoicePendingAllocation:
					result = Xsd.TxnType.IPA;
					break;

				case ZArchitecture.Core.TransactionTypes.CreditNotePendingAllocation:
					result = Xsd.TxnType.CPA;
					break;
				case ZArchitecture.Core.TransactionTypes.Overpayment:
					result = Xsd.TxnType.OVP;
					break;
				case ZArchitecture.Core.TransactionTypes.ExchangeDifference:
					result = Xsd.TxnType.EXX;
					break;

				default:
					throw new ArgumentException("The following Transaction Type is not recognised: '" + transactionType + "'", nameof(transactionType));
			}

			return result;
		}

		public static Xsd.TxnHeaderReceiptPaymentType GetTxnHeaderReceiptPaymentType(ZString receiptPaymentType, INotifications notify)
		{
			return TxnHeaderReceiptPaymentTypeXmlMapping.Instance.GetExternalCode(receiptPaymentType, Res.GetString("92cf7334-1ce0-4d98-9861-6cbf23e9dfe2", "Receipt Payment Type"), notify);
		}

		public static Xsd.TxnLineConsolOrJobType GetTxnLineConsolOrJobType(ZString jobTypeDescription, INotifications notify)
		{
			Xsd.TxnLineConsolOrJobType result;

			result = TransactionLineConsolOrJobTypeXmlMapping.Instance.GetExternalCode(jobTypeDescription, Res.GetString("abdbbf32-5f10-495e-a7f9-1a70ca3490b1", "Consol or Job Type"), notify);

			if (result == Xsd.TxnLineConsolOrJobType.UNK) // Unknown Job Type
			{
				throw new ArgumentException("The following Job Type is not recognised: '" + jobTypeDescription + "'", nameof(jobTypeDescription));
			}

			return result;
		}

		public static ZString GetCodeForJobInvoicingConsumerType(Xsd.TxnLineConsolOrJobType jobType, INotifications notify)
		{
			return TransactionLineConsolOrJobTypeXmlMapping.Instance.GetEnterpriseCode(jobType, Res.GetString("abdbbf32-5f10-495e-a7f9-1a70ca3490b1", "Consol or Job Type"), notify);
		}

		public static Xsd.TxnLineLineType GetTxnLineLineType(ZString lineType)
		{
			Xsd.TxnLineLineType result;

			switch (lineType)
			{
				case ZArchitecture.Core.TransactionLineTypes.Cost:
					result = Xsd.TxnLineLineType.CST;
					break;

				case ZArchitecture.Core.TransactionLineTypes.Revenue:
					result = Xsd.TxnLineLineType.REV;
					break;

				case ZArchitecture.Core.TransactionTypes.DirectPayment:
					result = Xsd.TxnLineLineType.DPY;
					break;

				case ZArchitecture.Core.TransactionTypes.DirectReceipt:
					result = Xsd.TxnLineLineType.DRC;
					break;

				default:
					throw new ArgumentException("This Line Type is not recognised: '" + lineType + "'", nameof(lineType));
			}

			return result;
		}

		public static ZDecimal MultiplierForImportAndExport(Type type)
		{
			if (typeof(ARCreditNote).IsAssignableFrom(type) ||
				typeof(ARCreditNoteLine).IsAssignableFrom(type) ||
				typeof(APInvoice).IsAssignableFrom(type) ||
				typeof(APInvoiceLine).IsAssignableFrom(type) ||
				typeof(APAdjustmentNote).IsAssignableFrom(type) ||
				typeof(APAdjustmentNoteLine).IsAssignableFrom(type) ||
				typeof(APTransferToRow).IsAssignableFrom(type) ||
				typeof(ARTransferFromRow).IsAssignableFrom(type) ||
				typeof(ARContraRow).IsAssignableFrom(type) ||
				typeof(Receipt).IsAssignableFrom(type))
			{
				return -1;
			}
			else
			{
				return 1;
			}
		}

		public static Xsd.FinancialValue GetXmlFinancialValue(ZDecimal value, RefCurrency currency, Type transactionType)
		{
			var decimalPlaces = currency != null ? currency.Decimals : RefCurrency.MAX_DECIMAL;
			var currencyCode = currency != null ? currency.RX_Code : ZString.Empty;
			ZDecimal result = ZArchitecture.Core.Utilities.Round(value * MultiplierForImportAndExport(transactionType), decimalPlaces);
			return Xsd.FinancialValue.FromAmountAndCurrencyCode(result, currencyCode);
		}

		public static Xsd.FinancialValue GetXmlFinancialValue(ZDecimal value, RefCurrency currency)
		{
			var decimalPlaces = currency != null ? currency.Decimals : RefCurrency.MAX_DECIMAL;
			var currencyCode = currency != null ? currency.RX_Code : ZString.Empty;
			ZDecimal result = ZArchitecture.Core.Utilities.Round(value, decimalPlaces);
			return Xsd.FinancialValue.FromAmountAndCurrencyCode(result, currencyCode);
		}

		public static ZDecimal GetDecimalFromXmlFinancialValue(BusinessObjectFactory factory, Xsd.FinancialValue financialValue, Type transactionType, bool roundToCurrency = true)
		{
			ZDecimal amount = financialValue.Value * MultiplierForImportAndExport(transactionType);
			if (roundToCurrency)
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, financialValue.CurrencyCode);
				amount = ZArchitecture.Core.Utilities.Round(amount, currency?.Decimals ?? GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
			return amount;
		}

		public static ZDecimal GetDecimalFromXmlFinancialValue(BusinessObjectFactory factory, Xsd.FinancialValue financialValue)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, financialValue.CurrencyCode);
			if (currency == null)
			{
				return ZArchitecture.Core.Utilities.Round(financialValue.Value, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
			else
			{
				return ZArchitecture.Core.Utilities.Round(financialValue.Value, currency.Decimals);
			}
		}

		public static Xsd.FinancialValue SwapXmlFinancialValueSign(Xsd.FinancialValue financialValue)
		{
			ZDecimal result = financialValue.Value * new ZDecimal(-1.0);
			return Xsd.FinancialValue.FromAmountAndCurrencyCode(result, financialValue.CurrencyCode);
		}

		public static Xsd.TxnHeaderCashBasisTaxIndicator GetCashBasisTaxIndicator(bool isGSTCashBasis)
		{
			return isGSTCashBasis ? Xsd.TxnHeaderCashBasisTaxIndicator.Y : Xsd.TxnHeaderCashBasisTaxIndicator.N;
		}
	}
}
