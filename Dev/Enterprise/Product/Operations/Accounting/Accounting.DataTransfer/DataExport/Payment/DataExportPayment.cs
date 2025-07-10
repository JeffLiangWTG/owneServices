using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public abstract class DataExportPayment : AutoDataExportPayment
	{
		public DataExportPayment(BusinessObjectFactory factory, TransactionHeader payment)
			: base(factory)
		{
			AH_AB = payment.AH_AB;
			AH_ChequeOrReference = payment.AH_ChequeOrReference;
			AH_Ledger = payment.AH_Ledger;
			AH_LocalExTaxAmount = payment.AH_LocalExTaxAmount;
			AH_OH = payment.AH_OH;
			AH_OSTotalAmount = payment.AH_OSTotalAmount;
			AH_PostDate = payment.AH_PostDate;
			AH_InvoiceDate = payment.AH_InvoiceDate;
			AH_ReceiptBatchNo = payment.AH_ReceiptBatchNo;
			AH_ReceiptType = payment.AH_ReceiptType;
			AH_RX_NKTransactionCurrency = payment.AH_RX_NKTransactionCurrency;
			AH_TransactionNum = payment.AH_TransactionNum;
			AH_TransactionType = payment.AH_TransactionType;

			if (payment.BankAccount != null)
			{
				BankBSB = payment.BankAccount.AB_BSB;
			}
			IDirectDebitBatchTransaction transaction = payment as IDirectDebitBatchTransaction;

			if (transaction != null)
			{
				if (AH_ReceiptType == ReceiptTypes.DirectDebit || AH_ReceiptType == ReceiptTypes.DirectDebitLine || payment is DirectPayment)
				{
					PayeeName = transaction.AccountTitle;
				}
				else if (payment.Header != null)
				{
					PayeeName = payment.Header.OH_FullNameTruncated;
				}

				BankAccountNumber = transaction.BankAccountNumber;
				PayeeBankAccountNumber = transaction.PayeeBankAccountNumber;
				PayeeBankBranchName = transaction.PayeeBankBranchName;
				PayeeBankAddress1 = transaction.PayeeBankAddress1;
				PayeeBankAddress2 = transaction.PayeeBankAddress2;
				PayeeBankAddress3 = transaction.PayeeBankAddress3;
				PayeeBankBSB = transaction.PayeeBankBSB;
			}

			if (payment.Header != null)
			{
				var address = payment.Header.AddressForSendingAPDocuments;
				if (address != null)
				{
					PayeeAddressCity = address.OA_City;
					PayeeAddressPostalCode = address.OA_PostCode;
					PayeeAddressState = address.OA_State;

					AddressFormatter formatter = new AddressFormatter(Factory, address, GlbCompany.CurrentCompany, false);
					string[] addressParts = formatter.PostalAddress().Split(new string[] { "\n" }, int.MaxValue, StringSplitOptions.None);

					if (addressParts.Length > 0)
					{
						PayeeAddress1 = addressParts[0];
					}

					if (addressParts.Length > 1)
					{
						PayeeAddress2 = addressParts[1];
					}

					if (addressParts.Length > 2)
					{
						PayeeAddress3 = addressParts[2];
					}
				}
				PayeeCountryCode = payment.Header.CountryCode;
			}

			TransactionHeader = payment;

			if (payment is DirectPayment)
			{
				DirectPayment = (DirectPayment)payment;
			}
			else if (payment is Payment)
			{
				Payment paymentAsPayment = payment as Payment;
				this.Payment = paymentAsPayment;
				if (paymentAsPayment.AccountDetails != null)
				{
					PayeeSWIFTID = paymentAsPayment.AccountDetails.A1_BankSwift;
				}
			}
		}

		public DataExportDirectDebitBatchHeader DDRHeader { get; set; }

		public TransactionHeader TransactionHeader { get; set; }

		public DirectPayment DirectPayment { get; set; }

		public Payment Payment { get; set; }

		#region Currency

		public RefCurrency Currency
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, AH_RX_NKTransactionCurrency);
			}
		}

		#endregion

		#region AH_OH

		[List("Organisations")]
		public override ZGuid AH_OH
		{
			get { return base.AH_OH; }
			set { base.AH_OH = value; }
		}

		OrgHeaderCollection fOrganisations;

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}

		#endregion

		#region AH_AB

		[List("BankAccounts")]
		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set { base.AH_AB = value; }
		}

		AccBankAccountCollection fBankAccounts;

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					fBankAccounts = new AccBankAccountCollection(Factory);
				}
				return fBankAccounts;
			}
		}

		#endregion

		public AccTransactionMatchLink[] MatchLinks
		{
			get
			{
				if (fMatchLinks == null)
				{
					fMatchLinks = Array.Empty<AccTransactionMatchLink>();
				}
				return fMatchLinks;
			}
			set
			{
				if (fMatchLinks != value)
				{
					fMatchLinks = value;
				}
			}
		}

		AccTransactionMatchLink[] fMatchLinks;

		List<TransactionHeader> fMatchedTransactions;

		List<TransactionHeader> MatchedTransactionHeaders
		{
			get
			{
				if (fMatchedTransactions == null)
				{
					fMatchedTransactions = new List<TransactionHeader>();
					if (MatchLinks != null)
					{
						foreach (AccTransactionMatchLink matchLink in MatchLinks)
						{
							TransactionHeader header = Factory.Load<TransactionHeader>(matchLink.AP_AH);
							if (header != null)
							{
								fMatchedTransactions.Add(header);
							}
						}
					}
				}
				return fMatchedTransactions;
			}
		}

		#region Related Invoices

		public override ZString RelatedInvoiceNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (TransactionHeader header in MatchedTransactionHeaders)
				{
					if (header != null && (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote))
					{
						if (result != ZString.Empty)
						{
							result += ",";
						}
						result += header.AH_TransactionNum;
					}
				}
				return result;
			}
		}

		#endregion

		#region Helpers

		ZString ConvertListToString(List<ZString> list)
		{
			ZString result = ZString.Empty;
			foreach (ZString str in list)
			{
				if (result != ZString.Empty)
				{
					result += ",";
				}
				result += str;
			}
			return result;
		}

		#endregion

		#region Related Jobs

		public override ZString RelatedJobNumbers
		{
			get
			{
				List<ZString> list = new List<ZString>();
				foreach (TransactionHeader header in MatchedTransactionHeaders)
				{
					if (header != null && (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote))
					{
						TransactionHeaderWithLines transaction = header as TransactionHeaderWithLines;
						if (transaction != null)
						{
							foreach (InvoicingLineBase line in transaction.Lines)
							{
								if (line.AL_JH.IsValid && line.Job != null && line.Job.JH_JobNum != ZString.Empty && !list.Contains(line.Job.JH_JobNum))
								{
									list.Add(line.Job.JH_JobNum);
								}
							}
						}
					}
				}
				return ConvertListToString(list);
			}
		}

		#endregion

		#region RelatedHouseBillNumbers

		public override ZString RelatedHouseBillNumbers
		{
			get
			{
				List<ZString> list = new List<ZString>();
				foreach (TransactionHeader header in MatchedTransactionHeaders)
				{
					if (header != null && (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote))
					{
						TransactionHeaderWithLines transaction = header as TransactionHeaderWithLines;
						if (transaction != null)
						{
							foreach (InvoicingLineBase line in transaction.Lines)
							{
								if (line.AL_JH.IsValid && line.Job != null)
								{
									var genericJob = line.Job.LoadGenericJob<GenericJob>();
									if (genericJob != null && !list.Contains(genericJob.VJ_HouseBillNumber) && genericJob.VJ_HouseBillNumber != ZString.Empty)
									{
										list.Add(genericJob.VJ_HouseBillNumber);
									}
								}
							}
						}
					}
				}
				return ConvertListToString(list);
			}
		}

		#endregion

		#region RelatedMasterBillNumbers

		public override ZString RelatedMasterBillNumbers
		{
			get
			{
				List<ZString> list = new List<ZString>();
				foreach (TransactionHeader header in MatchedTransactionHeaders)
				{
					if (header != null && (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote))
					{
						TransactionHeaderWithLines transaction = header as TransactionHeaderWithLines;
						if (transaction != null)
						{
							foreach (InvoicingLineBase line in transaction.Lines)
							{
								if (line.AL_JH.IsValid && line.Job != null)
								{
									GenericJob genericJob = line.Job.LoadGenericJob<GenericJob>();
									if (genericJob != null && !list.Contains(genericJob.VJ_MasterBillNumber) && genericJob.VJ_MasterBillNumber != ZString.Empty)
									{
										list.Add(genericJob.VJ_MasterBillNumber);
									}
								}
							}
						}
					}
				}
				return ConvertListToString(list);
			}
		}

		#endregion

		#region CurrentCompanyCurrency

		public override ZString CurrentCompanyCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		#endregion

		public override ZInt NumberOfPaidTransactions
		{
			get
			{
				if (fNumberOfPaidTransactions.HasValue)
				{
					return fNumberOfPaidTransactions.Value;
				}
				else
				{
					return MatchedTransactionHeaders.Count;
				}
			}
			set
			{
				fNumberOfPaidTransactions = (int)value;
				NumberOfPaidTransactionsInfo.RefreshBinding();
			}
		}

		int? fNumberOfPaidTransactions;
	}
}
