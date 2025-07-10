using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Netting;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public static class TxnHeaderBuilder
	{
		public static void SetLedger(TxnHeader txnHeader, ZString ledger, INotificationManager notifier)
		{
			try
			{
				txnHeader.Ledger = TxnHeaderMapper.GetTxnHeaderLedger(ledger);
			}
			catch (ArgumentException)
			{
				string message = Res.GetString("81ede7f5-7667-46fc-92b5-2491fcc00018", "The Ledger Type you supplied ({0}) is not recognized by {1}.", ledger, Core.Constants.ProductName) + " \r\n";
				message += Res.GetString("f2b33927-9a0d-4229-b1f4-4894792b8739", "Please use one of the following Ledger Types: AR, AP");
				notifier.AddErrorToNotifications(message);
			}
		}

		public static void SetDebtorOrCreditor(TxnHeader txnHeader, ZString orgCode, ZString orgName)
		{
			if (!string.IsNullOrEmpty(orgCode))
			{
				txnHeader.DebtorOrCreditor = new Organisation();
				txnHeader.DebtorOrCreditor.EDICode = orgCode;
				if (!string.IsNullOrEmpty(orgName))
				{
					txnHeader.DebtorOrCreditor.OrganisationDetails = new OrganisationDetail();
					txnHeader.DebtorOrCreditor.OrganisationDetails.Name = orgName;
				}
			}
		}

		public static void SetIsDisbursement(TxnHeader txnHeader, ZString isDisbursement)
		{
			if (!string.IsNullOrEmpty(isDisbursement))
			{
				if (isDisbursement == Core.Constants.BooleanTrueString)
				{
					txnHeader.DisbursementFlag = true;
					txnHeader.DisbursementFlagSpecified = true;
				}
			}
		}

		public static void SetTransactionType(TxnHeader txnHeader, ZString transactionType, INotificationManager notifier)
		{
			try
			{
				txnHeader.TxnType = TxnHeaderMapper.GetTxnHeaderTxnType(transactionType);
			}
			catch (ArgumentException)
			{
				string message = Res.GetString("e735ee84-ac1b-49a8-8b9e-0288b8db5d06", "The Transaction Type you supplied ({0}) is not recognized by {1}.", transactionType, Core.Constants.ProductName) + " \r\n";
				message += Res.GetString("28d17ac6-8af7-4f1d-9587-833c0d811688", "Please use one of the following: INV, CRD, ADJ");
				notifier.AddErrorToNotifications(message);
			}
		}

		public static void SetReceiptPaymentType(TxnHeader txnHeader, ZString receiptPaymentType, INotificationManager notifier)
		{
			try
			{
				txnHeader.ReceiptPaymentType = TxnHeaderMapper.GetTxnHeaderReceiptPaymentType(receiptPaymentType, notifier.NotificationSubscriber);
			}
			catch (ArgumentException)
			{
				string message = Res.GetString("8493e7db-a209-4565-b24a-349fe93714eb", "The Payment Type you supplied ({0}) is not recognized by {1}.", receiptPaymentType, Core.Constants.ProductName);
				notifier.AddErrorToNotifications(message);
			}
		}

		public static void SetTransactionNum(TxnHeader txnHeader, ZString transacstionNumber)
		{
			txnHeader.TxnNumber = transacstionNumber;
		}

		public static void SetThirdPartyReference(TxnHeader txnHeader, ZString thirdPartyReference)
		{
			txnHeader.ThirdPartyReference = thirdPartyReference;
		}

		public static void SetDescription(TxnHeader txnHeader, ZString transacstionDescription)
		{
			if (!transacstionDescription.IsEmpty)
			{
				txnHeader.Description = transacstionDescription;
			}
		}

		public static void SetPostDate(TxnHeader txnHeader, ZDateTime date)
		{
			txnHeader.PostDate = date;
		}

		public static void SetInvoiceDate(TxnHeader txnHeader, ZDateTime date, INotificationManager notifier = null)
		{
			if (date.IsEmpty && notifier != null)
			{
				notifier.AddErrorToNotifications(Res.GetString("57AB2A9D-A61D-4460-A57E-F24AF036CE2E", "Invoice Date cannot be empty."));
			}

			txnHeader.InvoiceDate = date;
		}

		public static void SetDueDate(TxnHeader txnHeader, ZDateTime date)
		{
			if (!date.IsEmpty)
			{
				txnHeader.DueDate = date;
			}
		}

		public static void SetMatchStatus(TxnHeader txnHeader, ZString matchStatus)
		{
			if (!matchStatus.IsEmpty)
			{
				txnHeader.MatchStatus = matchStatus;
			}
		}

		public static void SetMatchStatusReasonCode(TxnHeader txnHeader, ZString matchReasonCode)
		{
			if (!matchReasonCode.IsEmpty)
			{
				txnHeader.MatchStatusReasonCode = matchReasonCode;
			}
		}

		public static void SetOverrideSystemExchangeRate(TxnHeader txnHeader, ZBool overrideSystemExchangeRate)
		{
			txnHeader.OverrideSystemExchangeRate = overrideSystemExchangeRate;
		}

		public static void SetOsInvoiceAmtInclTax(TxnHeader txnHeader, ZDecimal value, ZString currencyCode, BusinessObjectFactory factory, INotificationManager notifier)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			if (currency != null)
			{
				txnHeader.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(value, currency, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			}
			else
			{
				notifier.AddErrorToNotifications(Res.GetString("d7269542-ef6c-4ed8-8925-b2b6ac5ef020", "Unrecognized currency code '{0}'", currencyCode));
			}
		}

		public static void SetLocalInvoiceAmtInclTax(TxnHeader txnHeader, ZDecimal value, ZString currencyCode, BusinessObjectFactory factory, INotificationManager notifier)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			if (currency != null)
			{
				txnHeader.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(value, currency, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			}
			else
			{
				notifier.AddErrorToNotifications(Res.GetString("d7269542-ef6c-4ed8-8925-b2b6ac5ef020", "Unrecognized currency code '{0}'", currencyCode));
			}
		}

		public static void SetOsInvoiceAmtExclTax(TxnHeader txnHeader, ZDecimal value, ZString currencyCode, BusinessObjectFactory factory, INotificationManager notifier)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			if (currency != null)
			{
				txnHeader.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(value, currency, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			}
			else
			{
				notifier.AddErrorToNotifications(Res.GetString("d7269542-ef6c-4ed8-8925-b2b6ac5ef020", "Unrecognized currency code '{0}'", currencyCode));
			}
		}

		public static void SetOsTaxAmount(TxnHeader txnHeader, ZDecimal value, ZString currencyCode, BusinessObjectFactory factory, INotificationManager notifier)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			if (currency != null)
			{
				txnHeader.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(value, currency, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			}
			else
			{
				notifier.AddErrorToNotifications(Res.GetString("d7269542-ef6c-4ed8-8925-b2b6ac5ef020", "Unrecognized currency code '{0}'", currencyCode));
			}
		}

		public static void SetLocalInvoiceAmtExclTax(TxnHeader txnHeader, ZDecimal value, ZString currencyCode, BusinessObjectFactory factory, INotificationManager notifier)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			if (currency != null)
			{
				txnHeader.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(value, currency, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			}
			else
			{
				notifier.AddErrorToNotifications(Res.GetString("d7269542-ef6c-4ed8-8925-b2b6ac5ef020", "Unrecognized currency code '{0}'", currencyCode));
			}
		}

		public static void SetOsCurrencyEmptyFlag(TxnHeader txnHeader, ZBool flag)
		{
			txnHeader.OsCurrencyEmptyFlag = flag;
		}

		public static void SetAmountPaidThisPayment(TxnHeader txnHeader, ZDecimal value, ZString currencyCode, BusinessObjectFactory factory, INotificationManager notifier)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			if (currency != null)
			{
				SetAmountPaidThisPayment(txnHeader, value, currency);
			}
			else
			{
				notifier.AddErrorToNotifications(Res.GetString("d7269542-ef6c-4ed8-8925-b2b6ac5ef020", "Unrecognized currency code '{0}'", currencyCode));
			}
		}

		public static void SetAmountPaidThisPayment(TxnHeader txnHeader, ZDecimal value, RefCurrency currency)
		{
			if (currency != null)
			{
				txnHeader.AmountPaidThisPayment = TxnHeaderMapper.GetXmlFinancialValue(value, currency, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			}
		}

		public static void SetBranchCode(TxnHeader txnHeader, ZString code)
		{
			if (!code.IsEmpty)
			{
				txnHeader.Branch = code;
			}
		}

		public static void SetDepartmentCode(TxnHeader txnHeader, ZString code)
		{
			if (!code.IsEmpty)
			{
				txnHeader.Department = code;
			}
		}

		public static void SetBankAccount(TxnHeader txnHeader, ZString bankCode)
		{
			if (!bankCode.IsEmpty)
			{
				txnHeader.BankCode = bankCode;
			}
		}

		public static void SetChequeBook(TxnHeader txnHeader, ZString chequeBook)
		{
			if (txnHeader.TxnType == TxnType.PAY && !chequeBook.IsEmpty)
			{
				txnHeader.ChequeBook = chequeBook;
			}
		}

		public static void SetChequeOrReference(TxnHeader txnHeader, ZString chequeOrReference)
		{
			if (!chequeOrReference.IsEmpty)
			{
				txnHeader.ChequeOrReference = chequeOrReference;
			}
		}

		public static void SetPaymentReference(TxnHeader txnHeader, ZString paymentReference)
		{
			if (!paymentReference.IsEmpty)
			{
				txnHeader.PaymentReference = paymentReference;
			}
		}

		public static void SetChequeDrawer(TxnHeader txnHeader, ZString chequeDrawer)
		{
			if (txnHeader.TxnType == TxnType.REC && !chequeDrawer.IsEmpty)
			{
				txnHeader.ChequeDrawer = chequeDrawer;
			}
		}

		public static void SetChequeDrawerBank(TxnHeader txnHeader, ZString drawerBank)
		{
			if (txnHeader.TxnType == TxnType.REC && !drawerBank.IsEmpty)
			{
				txnHeader.DrawerBank = drawerBank;
			}
		}

		public static void SetChequeDrawerBankBranch(TxnHeader txnHeader, ZString drawerBankBranch)
		{
			if (txnHeader.TxnType == TxnType.REC && !drawerBankBranch.IsEmpty)
			{
				txnHeader.DrawerBankBranch = drawerBankBranch;
			}
		}

		public static void SetOverrideAddress(TxnHeader txnHeader, ZString addressShortCode)
		{
			if (txnHeader.TxnType == TxnType.PAY && !addressShortCode.IsEmpty)
			{
				var address = new Enterprise.DataTransfer.Xml.XsdVersion1.OrgAddress();
				address.AddressCode = addressShortCode;
				txnHeader.TxnOverrideAddress = address;
			}
		}

		public static void SetOverrideContact(TxnHeader txnHeader, ZString contactName)
		{
			if (txnHeader.TxnType == TxnType.PAY && !contactName.IsEmpty)
			{
				var contact = new Enterprise.DataTransfer.Xml.XsdVersion1.OrgContact();
				contact.Name = contactName;
				txnHeader.TxnOverrideContact = contact;
			}
		}

		public static OrgAddress GetOrgAddress(UniversalObjectFactory factoryCasted, OrganizationAddress orgAddress, IXmlImportLogger logger) => orgAddress != null ? new OrganisationDataObjectReader(orgAddress, logger, factoryCasted).GetMatched() : null;

		public static void BuildReceiptPaymentHeader(TxnHeader txnHeader, IPaymentReceiptHeader paymentReceipt, BusinessObjectFactory factory, INotificationManager notifier)
		{
			var rowType = paymentReceipt.Type;

			SetLedger(txnHeader, paymentReceipt.Ledger, notifier);
			SetTransactionType(txnHeader, paymentReceipt.TransactionType, notifier);

			SetInvoiceDate(txnHeader, GetZDateTimeFromField(paymentReceipt.InvoiceDate, notifier));
			SetPostDate(txnHeader, GetZDateTimeFromField(paymentReceipt.PostDate, notifier));
			SetDebtorOrCreditor(txnHeader, paymentReceipt.OrganizationCode, null);
			SetDescription(txnHeader, paymentReceipt.Description);
			SetReceiptPaymentType(txnHeader, paymentReceipt.ReceiptPaymentType, notifier);
			SetBankAccount(txnHeader, paymentReceipt.BankAccountCode);
			if (rowType == AccountingConstants.RemittanceFileRowTypes.Payment)
			{
				SetChequeBook(txnHeader, paymentReceipt.CheckBookCode);
			}
			SetChequeOrReference(txnHeader, paymentReceipt.CheckOrReference);

			var curencyCode = paymentReceipt.OSCurrencyCode;
			var localCurrency = paymentReceipt.LocalCurrencyCode;
			SetOsInvoiceAmtInclTax(txnHeader, paymentReceipt.OSTotal, curencyCode, factory, notifier);
			SetOsInvoiceAmtExclTax(txnHeader, paymentReceipt.OSExGSTVATAmount, curencyCode, factory, notifier);
			SetLocalInvoiceAmtInclTax(txnHeader, paymentReceipt.LocalTotal, localCurrency, factory, notifier);
			SetLocalInvoiceAmtExclTax(txnHeader, paymentReceipt.LocalExVATAmount, localCurrency, factory, notifier);

			SetBranchCode(txnHeader, paymentReceipt.BranchCode);
			SetDepartmentCode(txnHeader, paymentReceipt.DepartmentCode);

			SetThirdPartyReference(txnHeader, paymentReceipt.ThirdPartyReference);

			if (rowType == AccountingConstants.RemittanceFileRowTypes.Receipt)
			{
				SetChequeDrawer(txnHeader, paymentReceipt.CheckDrawer);
				SetChequeDrawerBank(txnHeader, paymentReceipt.DrawerBank);
				SetChequeDrawerBankBranch(txnHeader, paymentReceipt.DrawerBranch);
			}

			if (rowType == AccountingConstants.RemittanceFileRowTypes.Payment)
			{
				if (paymentReceipt.IsContainOverrideAddress)
				{
					SetOverrideAddress(txnHeader, paymentReceipt.OverrideAddress);
				}

				if (paymentReceipt.IsContainOverrideContact)
				{
					SetOverrideContact(txnHeader, paymentReceipt.OverrideContact);
				}
			}
		}

		public static void BuidPaidTransaction(TxnHeader txnHeader, IPaymentReceiptMatchLine matchLine, BusinessObjectFactory factory, INotificationManager notifier)
		{
			SetLedger(txnHeader, matchLine.Ledger, notifier);
			SetTransactionType(txnHeader, matchLine.TransactionType, notifier);
			SetTransactionNum(txnHeader, matchLine.TransactionNumber);
			SetOsCurrencyEmptyFlag(txnHeader, matchLine.OSCurrencyCode.IsEmpty);
			txnHeader.AmountPaidThisPayment = TxnHeaderMapper.GetXmlFinancialValue(matchLine.AmountPaid, null, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			txnHeader.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(matchLine.AmountPaid, null, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			var miscellaneousTrnasacitonType = new List<TxnType> { TxnType.OVP, TxnType.EXX, TxnType.DSC, TxnType.JNL };

			if (miscellaneousTrnasacitonType.Contains(txnHeader.TxnType))
			{
				txnHeader.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(matchLine.AmountPaid, null, TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			}

			var shouldNotBeForeignCurrencyType = new List<TxnType> { TxnType.EXX, TxnType.DSC };

			var currencyCode = matchLine.OSCurrencyCode;
			txnHeader.OsInvoiceAmtInclTax.CurrencyCode = currencyCode;
			if (!currencyCode.IsEmpty && RefCurrency.LoadFromCurrencyCode(factory, currencyCode) == null)
			{
				notifier.AddErrorToNotifications(Res.GetString("d7269542-ef6c-4ed8-8925-b2b6ac5ef020", "Unrecognized currency code '{0}'", currencyCode));
			}
			else if (shouldNotBeForeignCurrencyType.Contains(txnHeader.TxnType) && !currencyCode.IsEmpty && currencyCode != matchLine.LocalCurrencyCode)
			{
				notifier.AddErrorToNotifications(Res.GetString("02BD64F5-F87D-432F-9405-ED02B1B7D0F5", "Exchange difference and discount transactions must be in local currency."));
			}

			ZString orgCode = ZString.Empty;
			if (txnHeader.TxnType == TxnType.JNL)
			{
				var eHubID = matchLine.OrganizationCode;
				if (!eHubID.IsEmpty)
				{
					var org = NettingHelper.GetOrgHeaderByEhubID(eHubID, factory);
					orgCode = org != null ? org.OH_Code : eHubID;
				}
			}
			else
			{
				orgCode = matchLine.OrganizationCode;
			}
			SetDebtorOrCreditor(txnHeader, orgCode, null);
			SetPaymentReference(txnHeader, matchLine.PaymentReference);
			SetDescription(txnHeader, matchLine.Description);
			SetInvoiceDate(txnHeader, matchLine.InvoiceDate);
			SetPostDate(txnHeader, matchLine.PostDate);
			SetDueDate(txnHeader, matchLine.DueDate);
			if (matchLine.AmountPaidInLocalCurrency != 0 && !matchLine.LocalCurrencyCode.IsEmpty)
			{
				SetLocalInvoiceAmtInclTax(txnHeader, matchLine.AmountPaidInLocalCurrency, matchLine.LocalCurrencyCode, factory, notifier);
				if (miscellaneousTrnasacitonType.Contains(txnHeader.TxnType))
				{
					SetLocalInvoiceAmtExclTax(txnHeader, matchLine.AmountPaidInLocalCurrency, matchLine.LocalCurrencyCode, factory, notifier);
				}
			}
			SetMatchStatus(txnHeader, matchLine.MatchStatus);
			SetMatchStatusReasonCode(txnHeader, matchLine.MatchStatusReasonCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "We really want a 4 character year")]
		public static ZDateTime GetZDateTimeFromField(ZDateTime dateTime, INotificationManager notifier)
		{
			var resultDate = dateTime;

			if (resultDate.IsEmpty)
			{
				notifier.AddErrorToNotifications(Res.GetString("5e1502df-40fe-48a5-acb9-2bb9c4d6cd8b", "Invalid Date format. Only YYYYMMDD format is supported."));
			}

			if (!resultDate.IsValidSmallDateTime)
			{
				notifier.AddErrorToNotifications(Res.GetString("0D3B8D21-8488-47B8-8B22-FC920EF9ACB2", "Date out of range: {0}. The date must be between {1} and {2}",
					resultDate.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture), // We really want a 4 character year
					ZDateTime.MinSmallDateTimeValue.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture), // We really want a 4 character year
					ZDateTime.MaxSmallDateTimeValue.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture))); // We really want a 4 character year
			}

			return resultDate;
		}

		public static ZBool IsSupportedPaidTransactionType(ZString transactionType, INotificationManager notifier)
		{
			var result = false;
			if (transactionType == ZArchitecture.Core.TransactionTypes.Invoice ||
									transactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
									transactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote ||
									transactionType == ZArchitecture.Core.TransactionTypes.Journal ||
									transactionType == ZArchitecture.Core.TransactionTypes.Receipt ||
									transactionType == ZArchitecture.Core.TransactionTypes.Payment ||
									transactionType == ZArchitecture.Core.TransactionTypes.ExchangeDifference ||
									transactionType == ZArchitecture.Core.TransactionTypes.Discount ||
									transactionType == ZArchitecture.Core.TransactionTypes.Overpayment)
			{
				result = true;
			}
			else
			{
				var message = Res.GetString("1d0a36f6-1a72-4414-9000-f31747998a28", "Unrecognized transaction type was detected. Only '{0}', '{1}', '{2}', '{3}', '{4}', '{5}, '{6}', '{7}' and '{8}' types are used for import. The following transaction type will be ignored: '{9}'.",
						ZArchitecture.Core.TransactionTypes.Invoice,
						ZArchitecture.Core.TransactionTypes.CreditNote,
						ZArchitecture.Core.TransactionTypes.AdjustmentNote,
						ZArchitecture.Core.TransactionTypes.Journal,
						ZArchitecture.Core.TransactionTypes.Receipt,
						ZArchitecture.Core.TransactionTypes.Payment,
						ZArchitecture.Core.TransactionTypes.ExchangeDifference,
						ZArchitecture.Core.TransactionTypes.Discount,
						ZArchitecture.Core.TransactionTypes.Overpayment,
						transactionType) + "\r\n";
				notifier.AddWarningToNotifications(message);
			}

			return result;
		}
	}
}
