using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocPaymentBatch : DocBaseWrapper, IGenericTransactionHeaderPlugIn
	{
		protected DocPaymentBatch(AccPaymentBatch paymentBatch, BusinessObjectFactory factory)
			: base(paymentBatch, factory)
		{
			this.paymentBatch = paymentBatch;

			var genericTransactions = new DocGenericTransactionHeaderCollection(Factory);
			var matchingCollection = new IMatchingCollection(factory);
			var approvals = Factory.Load<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.AV_APB_PaymentBatch, paymentBatch.PK));

			foreach (var approval in approvals)
			{
				matchingCollection.Add(approval);
				genericTransactions.Add(DocGenericTransactionHeader.New(approval, Factory));
			}

			GenericTransactions = genericTransactions;
			BankAccountCode = factory.Load<AccBankAccount>(paymentBatch.APB_AB)?.AB_Code ?? ZString.Empty;
			CheckBookCode = factory.Load<AccChequeBook>(paymentBatch.APB_AK)?.AK_Code ?? ZString.Empty;
			TotalLocalInvoiceAmount = new CurrencySummary(matchingCollection).TransactionsLocalAmountTotal;
			CreatedDate = paymentBatch.APB_SystemCreateTimeUtc.ToLocalBranchTime();
			CreatingUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, paymentBatch.APB_SystemCreateUser)?.GS_FullName ?? ZString.Empty;
		}

		readonly AccPaymentBatch paymentBatch;

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocPaymentBatchGenericTransactionSupporter(this)); }
		}
		DocPaymentBatchGenericTransactionSupporter fGenericTransactionSupporter;

		public static DocPaymentBatch New(AccPaymentBatch paymentBatch, BusinessObjectFactory factory)
		{
			return (paymentBatch != null) ? new DocPaymentBatch(paymentBatch, factory) : null;
		}

		public DocGenericTransactionHeaderCollection GenericTransactions { get; }

		public ZString TransactionNumber => paymentBatch.APB_BatchNumber;
		public ZString TransactionAPPaymentMethod => paymentBatch.APB_PaymentType;
		public ZString BankAccountCode { get; }
		public ZString CheckBookCode { get; }
		public ZString CreatingUser { get; }
		public ZDateTime CreatedDate { get; }
		public ZDateTime FullyPaidDate => paymentBatch.APB_PaymentDate;
		public ZDecimal TotalLocalInvoiceAmount { get; }

		class DocPaymentBatchGenericTransactionSupporter : GenericTransactionHeaderSupporter
		{
			public DocPaymentBatchGenericTransactionSupporter(DocPaymentBatch parent)
			{
				Parent = parent;
			}
			readonly DocPaymentBatch Parent;

			protected internal override ZString GetTransactionType()
			{
				return Parent.TransactionType;
			}

			protected internal override ZString GetTransactionNumber()
			{
				return Parent.TransactionNumber;
			}

			protected internal override DocGenericTransactionHeaderCollection GetGenericTransactions()
			{
				return Parent.GenericTransactions;
			}

			protected internal override ZString GetTransactionAPPaymentMethod()
			{
				return Parent.TransactionAPPaymentMethod;
			}

			protected internal override ZString GetBankAccountCode()
			{
				return Parent.BankAccountCode;
			}

			protected internal override ZString GetCheckBookCode()
			{
				return Parent.CheckBookCode;
			}

			protected internal override ZString GetCreatingUser()
			{
				return Parent.CreatingUser;
			}

			protected internal override ZDateTime GetCreatedDate()
			{
				return Parent.CreatedDate;
			}

			protected internal override ZDateTime GetFullyPaidDate()
			{
				return Parent.FullyPaidDate;
			}

			protected internal override ZDecimal GetTotalLocalInvoiceAmount()
			{
				return Parent.TotalLocalInvoiceAmount;
			}
		}

		public ZString TransactionType => "UNA";
	}
}
