using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public abstract class DataExportDirectDebitBatch : AutoDataExportDirectDebitBatch
	{
		public DataExportDirectDebitBatch(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			: base(factory)
		{
			Header = header;
			AH_AB = header.AH_AB;
			AH_ChequeOrReference = header.AH_ChequeOrReference;
			AH_GB = header.AH_GB;
			AH_GE = header.AH_GE;
			AH_InvoiceAmount = header.AH_InvoiceAmount;
			AH_OSExTaxAmount = header.AH_OSExTaxAmount;
			AH_PostDate = header.AH_PostDate;
			AH_InvoiceDate = header.AH_InvoiceDate;
			AH_ReceiptType = header.AH_ReceiptType;
			AH_RX_NKTransactionCurrency = header.AH_RX_NKTransactionCurrency;
			AH_TransactionNum = header.AH_TransactionNum;
		}

		#region Header

		public readonly DirectDebitBatchHeader Header;

		#endregion

		#region AH_GB

		[List("Branches")]
		public override ZGuid AH_GB
		{
			get { return base.AH_GB; }
			set { base.AH_GB = value; }
		}

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		#endregion

		#region Currency

		public RefCurrency Currency
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, AH_RX_NKTransactionCurrency);
			}
		}

		#endregion

		#region AH_GE

		[List("Departments")]
		public override ZGuid AH_GE
		{
			get { return base.AH_GE; }
			set { base.AH_GE = value; }
		}

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}
				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

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

		#region NumberOfPayments

		public override ZInt NumberOfPayments
		{
			get { return Header.Lines.Count; }
		}

		#endregion

		#region FileNumber

		public override ZInt FileNumber
		{
			get { return Header.FileNumber; }
		}

		#endregion

		#region FileID

		public override ZString FileID
		{
			get
			{
				if (FileNumber <= 10)
				{
					return (FileNumber - 1).ToString();
				}
				return Convert.ToChar(Convert.ToInt32('A') + (FileNumber - 11)).ToString();
			}
		}

		#endregion

		#region SequenceNumberOffset

		public override ZString SequenceNumberOffset
		{
			get
			{
				return Header.SequenceNumberOffset;
			}
		}

		#endregion

		#region CompanyName

		public override ZString CompanyName
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		#endregion

		[ResourceStringData("DataExportDirectDebitBatch|NumberOfPaidTransactions", Caption = "Number of Paid Transactions")]
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
					ZInt total = 0;

					foreach (TransactionHeader payment in Header.Lines)
					{
						DataExportPayment exportPayment = new DataExportPaymentHeader(Factory, payment);
						exportPayment.DDRHeader = new DataExportDirectDebitBatchHeader(Factory, Header);
						AccTransactionMatchLink[] allMatchLinks = null;
						if (payment is Payment)
						{
							AccTransactionMatchLink paymentMatchLink = payment.Factory.LoadTop1<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment.PK));
							allMatchLinks = DirectDebitBatchDataExportAdapter.GetAllMatchlinksExcludingEXX((Payment)payment, paymentMatchLink);
							exportPayment.MatchLinks = allMatchLinks;
						}

						total += exportPayment.NumberOfPaidTransactions;
					}
					return total;
				}
			}
			set
			{
				fNumberOfPaidTransactions = (int)value;
				NumberOfPaidTransactionsInfo.RefreshBinding();
			}
		}

		int? fNumberOfPaidTransactions;

		[ResourceStringData("DataExportDirectDebitBatch|NumberOfPaymentsAndPaidTransactions", Caption = "Number of Payments and Paid Transactions")]
		public override ZInt NumberOfPaymentsAndPaidTransactions
		{
			get
			{
				return NumberOfPayments + NumberOfPaidTransactions;
			}
		}

		[ResourceStringData("DataExportDirectDebitBatch|NordeaFormatSpacingRecords", Caption = "Nordea Format Spacing Records")]
		public override ZString NordeaFormatSpacingRecords
		{
			get
			{
				return "".PadRight(94, '9');
			}
		}
	}
}
