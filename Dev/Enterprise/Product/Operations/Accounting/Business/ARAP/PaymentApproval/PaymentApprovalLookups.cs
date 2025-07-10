using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.EPaymentStatusCodes;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalLookups : AccPaymentApprovalLookups
	{
		public PaymentApprovalLookups(PaymentApprovalBase parent)
			: base(parent)
		{
		}

		#region PaymentReasons

		public CodeDescriptionPairList PaymentReasons
		{
			get
			{
				var providerCode = Parent.BankAccount?.AB_PaymentProvider ?? ZString.Empty;
				return EPaymentPropertyHelper.GetPaymentReasonsForProvider(Parent.AV_GC.ToGuid(), providerCode);
			}
		}

		#endregion

		public override AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					ZQuery branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, Parent.AV_GB);
					branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, Parent.AV_GC);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					bankFilter.AddToFilter(branchFilter, JoinCondition.And);

					fBankAccounts = new AccBankAccountCollection(Factory, bankFilter);
				}

				return fBankAccounts;
			}
		}

		AccBankAccountCollection fBankAccounts;

		public AccBankAccountCollection CreditCardBankAccounts
		{
			get
			{
				if (fCreditCardBankAccounts == null)
				{
					ZQuery branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, GlbBranch.CurrentBranch.PK);
					branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					bankFilter.AddToFilter(branchFilter, JoinCondition.And);

					ZQuery creditCardFilter = new ZQuery(AccBankAccountSchema.AB_AccountType, AccountTypeCodeDescriptionPairList.Codes.CCD);
					creditCardFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_AccountType, AccountTypeCodeDescriptionPairList.Codes.LNK);
					creditCardFilter.AddToFilter(bankFilter, JoinCondition.And);

					fCreditCardBankAccounts = new AccBankAccountCollection(Factory, creditCardFilter);
				}

				return fCreditCardBankAccounts;
			}
		}

		AccBankAccountCollection fCreditCardBankAccounts;

		public override AccChequeBookCollection ChequeBooks
		{
			get
			{
				if (fChequeBooks == null)
				{
					ZQuery filter = new ZQuery(AccChequeBookSchema.AK_GB, GlbBranch.CurrentBranch.PK);
					filter.AddToFilter(AccChequeBookSchema.AK_AB, Parent.AV_AB);
					fChequeBooks = new ActiveChequeBookCollection(Factory, filter);
					fChequeBooks.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("f5759ee6-63c8-48dc-a857-c225485016a1", "This check book cannot be chosen because it belongs to another bank account, another branch or/and is inactive. Please choose another check book"));
				}

				return fChequeBooks;
			}
		}

		AccChequeBookCollection fChequeBooks;

		public void ResetChequeBookCollection()
		{
			fChequeBooks = null;
		}

		protected new PaymentApprovalBase Parent
		{
			get { return (PaymentApprovalBase)base.Parent; }
		}

		public CodeDescriptionPairList PaymentMethods
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.PaymentMethod); }
		}

		public override OrgHeaderCollection Headers
		{
			get
			{
				return Parent.Ledger == LedgerTypes.AccountsPayable ? FindboxLookupCollections.GetCreditorCollection(Factory) :
						FindboxLookupCollections.GetDebtorCollection(Factory);
			}
		}

		public override AccTransactionHeaderCollection TransactionHeaders
		{
			get
			{
				if (Parent.Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					return new APTransactionHeaderCollection(Factory);
				}
				else
				{
					return new ARTransactionHeaderCollection(Factory);
				}
			}
		}

		public OrgAddressDependentCollection Addresses
		{
			get
			{
				return new OrgAddressDependentCollection(Factory);
			}
		}

		public static CodeDescriptionPairList EPaymentStatusList
		{
			get
			{
				var codes = new CodeDescriptionPairList();
				codes.AddPair(Deal.Queued, ResString.GetMultilingualString("932d15d7-0a35-4d8f-ba0b-c915d4e11e8d", "Queued"));
				codes.AddPair(Deal.Pending, ResString.GetMultilingualString("5cff5af1-b75f-4884-a3f7-74e11e1586e4", "Pending"));
				codes.AddPair(Deal.ReadyToSend, ResString.GetMultilingualString("acbd4969-5d64-4d70-acb5-ad4e93354e0a", "Ready To Send"));
				codes.AddPair(Deal.Requested, ResString.GetMultilingualString("ec4403d7-22c6-4526-8d47-bb6603f1a7a7", "Requested"));
				codes.AddPair(Deal.Accepted, ResString.GetMultilingualString("51c1ca2d-1a9f-4d02-a02b-bdf0e148a397", "Provider Accepted"));
				codes.AddPair(Deal.InProgress, ResString.GetMultilingualString("8f70375c-bfc5-4080-a16f-f21f9313e913", "Payment in Progress"));
				codes.AddPair(Deal.Paid, ResString.GetMultilingualString("9a1fd57a-6ce2-40d8-a21b-b933bbe26292", "Paid"));
				codes.AddPair(Deal.SubmissionFailed, ResString.GetMultilingualString("927ec0ec-e18a-4af4-884a-67df5166b818", "Submission Failed"));
				codes.AddPair(Deal.Cancelled, ResString.GetMultilingualString("12dfa193-8735-466e-bce9-7b74e33f60c6", "Canceled"));
				codes.AddPair(Deal.Declined, ResString.GetMultilingualString("b7221648-393b-42f9-beaa-5dabe1ae1a8c", "Provider Declined"));
				codes.AddPair(Deal.Failed, ResString.GetMultilingualString("acdebe61-1207-4864-b080-c561b2bf1d30", "Failed"));

				return codes;
			}
		}
	}
}
