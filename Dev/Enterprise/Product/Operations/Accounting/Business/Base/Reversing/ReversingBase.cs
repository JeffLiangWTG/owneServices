using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class ReversingBase
	{
		public ReversingBase(IReversing originalTransaction)
		{
			if (originalTransaction == null)
			{
				throw new ArgumentNullException(nameof(originalTransaction));
			}

			this.OriginalTransaction = originalTransaction;
			if (originalTransaction.Factory != null)
			{
				this.OriginalTransaction.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		public void Reverse()
		{
			DoReverseTransaction();

			if (ReverseTransaction != null)
			{
				DoReverseTaxTransaction();
			}
		}

		public bool CanReverseTransaction
		{
			get { return CanTransactionBeReversed(); }
		}

		public ZString CantReverseErrorMessage
		{
			get { return GenerateCantReverseErrorMessage(); }
		}

		public IReversing ReverseTransaction
		{
			get { return fReverseTransaction; }
		}

		/// <summary>
		/// Security Checkpoints required to reverse this transaction, in addition to the Controller checkpoint.
		/// May be many (eg: if this transaction is part of a match group).
		/// </summary>
		public IEnumerable<SecurityCheckpoint> CheckpointsToReverse => CheckpointsToReverseCore;

		#region Implementation

		public readonly IReversing OriginalTransaction;
		protected IReversing fReverseTransaction;

		public virtual bool ShouldShowReverseConfirmationMessage()
		{
			return false;
		}

		public virtual ZString GetReverseConfirmationMessage()
		{
			return ZString.Empty;
		}

		protected virtual bool CanTransactionBeReversed()
		{
			return !OriginalTransaction.IsReversed && !IsTransactionForInactiveOrganisation && !IsTransactionAttachedToCollectionBatch(OriginalTransaction) && CanReverseWhenRelatedJobIsReadyForFinancialClosure;
		}

		protected virtual ZString GenerateCantReverseErrorMessage()
		{
			ZString result = ZString.Empty;
			if (OriginalTransaction.IsReversed)
			{
				result = AlreadyReversedErrorMessage;
			}
			if (IsTransactionForInactiveOrganisation)
			{
				result = TransactionForInactiveOrganisationErrorMessage;
			}
			if (IsTransactionAttachedToCollectionBatch(OriginalTransaction))
			{
				result = TransactionAttachedToCollectionBatchErrorMessage;
			}
			if (!CanReverseWhenRelatedJobIsReadyForFinancialClosure)
			{
				result = RelatedJobIsReadyForFinancialClosureAndCantReversedErrorMessage;
			}
			return result;
		}

		protected virtual IEnumerable<SecurityCheckpoint> CheckpointsToReverseCore
			=> GetAllRelatedUnmatchingRows()
				.SelectMany(x => x.CheckpointsToUnmatch)
				.Where(cp => cp != null)
				.Distinct();

		protected virtual IEnumerable<UnmatchingRow> GetAllRelatedUnmatchingRows() => Enumerable.Empty<UnmatchingRow>();

		protected virtual void DoReverseTransaction()
		{
			if (OriginalTransaction != null)
			{
				UnmatchRelatedMatchingSessions();
				GenerateReverseTransactions();

				if (ReverseTransaction != null)
				{
					SetReversingDescriptionOnTransactions();

					SetTransactionBelongsToGroupOnTransactionsToReverse();

					CreateTasksAndMilestonesForReverseTransaction();  //It's important to apply workflow template before the cancellation flag is set

					SetCancellationFlagOnTransactionsToReverse();
				}

				HookFactorySaveToSetNumberFountainAndDateFields();
				AddSuspendersToTheFactory();
			}
		}

		protected virtual void DoReverseTaxTransaction()
		{
		}

		protected virtual void AddSuspendersToTheFactory()
		{
		}

		protected virtual void HookFactorySaveToSetNumberFountainAndDateFields()
		{
			OriginalTransaction.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetOtherNumberFountainFields);
		}

		protected virtual void SetOtherNumberFountainFields(BusinessObjectFactory factory)
		{
			factory.Saving -= new BusinessObjectFactory.SavingEventHandler(SetOtherNumberFountainFields);
		}

		protected virtual void UnmatchRelatedMatchingSessions()
		{
		}

		protected virtual void GenerateReverseTransactions()
		{
			OriginalTransaction.GenerateReverseTransaction(true);
			fReverseTransaction = OriginalTransaction.ReverseTransaction;
		}

		protected virtual void SetCancellationFlagOnTransactionsToReverse()
		{
			OriginalTransaction.SetCancellationFlag(true);
			ReverseTransaction.SetCancellationFlag(true);
		}

		protected virtual void CreateTasksAndMilestonesForReverseTransaction()
		{
			OriginalTransaction.ApplyWorkflowTemplatesOnReverseTransaction();
		}

		protected virtual void SetReversingDescriptionOnTransactions()
		{
			ZString reverseTransactionDesc = string.Format(AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ReversalRelated,
												  Res.GetString("754e5d67-6ea0-4aff-bcbb-6b2eb0d4ffcb", "Reversal related to")) + " {0}", OriginalTransaction.TransactionNumber);
			ReverseTransaction.SetDescription(reverseTransactionDesc);
			ReverseTransaction.SetNumberOfSupportingDocuments(AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.ReversalRelated, 0));
		}

		protected virtual void SetTransactionBelongsToGroupOnTransactionsToReverse()
		{
			ZGuid groupingGuid = ZGuid.NewZGuid();
			OriginalTransaction.SetTransactionBelongsToGroupField(groupingGuid);
		}

		protected virtual void OnFactorySaved(bool savedSuccessfully)
		{
			if (OriginalTransaction != null && OriginalTransaction.ReverseTransaction != null && ReverseTransaction != null && savedSuccessfully)
			{
				SendTransactionReversedEmail();
			}
		}

		protected virtual void SendTransactionReversedEmail()
		{
			ZTransactionReversedEmail email = new ZTransactionReversedEmail(OriginalTransaction);
			email.Send();
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			OnFactorySaved(savedSuccessfully);
		}

		protected virtual ZString RelatedJobIsReadyForFinancialClosureAndCantReversedErrorMessage
		{
			get
			{
				return Res.GetString("0DBCFA85-E627-45C8-8D4E-D8AA840D5C6A", "This transaction cannot be {0} because related job has Ready For Financial Closure status.",
					GetOperationNameInPastParticiple);
			}
		}

		protected virtual ZString AlreadyReversedErrorMessage
		{
			get
			{
				return Res.GetString("935ff984-3076-418f-af07-7b45e8b8d769", "This transaction cannot be {0} because it has already been reversed or is a reversal of another transaction.",
					GetOperationNameInPastParticiple);
			}
		}

		protected ZString MatchedAndCantReverseErrorMessage
		{
			get
			{
				return Res.GetString("bde9b49c-3e35-4096-8d2d-7d065c7a6649", "This transaction cannot be {0} because it has been matched with other transactions.",
					GetOperationNameInPastParticiple);
			}
		}

		protected ZString JournalAssociatedWithInvoiceCantReverseErrorMessage
		{
			get
			{
				var result = ZString.Empty;
				var journal = OriginalTransaction as Journal;
				if (journal != null && journal.AH_TransactionBelongsToGroup.IsValid)
				{
					var relatedInvoice = journal.Factory.Load<InvoicingBase>(journal.AH_TransactionBelongsToGroup);
					if (relatedInvoice != null)
					{
						result = Res.GetString("2a3565b7-5bc9-4a84-86ae-7ed9920fa5e0", "This journal is associated with Invoice {0}. Reversing Invoice {0} will reverse this journal.", relatedInvoice.AH_TransactionNum);
					}
				}
				return result;
			}
		}

		protected ZString ClearedInCashBookErrorMessage
		{
			get
			{
				return Res.GetString("30a9bdcc-18b1-40f3-9e24-952598900d4e", "This transaction cannot be {0} because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing.",
					GetOperationNameInPastParticiple);
			}
		}

		protected ZString TransactionAttachedToCollectionBatchErrorMessage
		{
			get
			{
				return Res.GetString("76eb3152-29d4-4928-a2c7-7d309562ed68", "This transaction cannot be reversed because it is attached to an active collection batch order.");
			}
		}

		protected virtual ZString GetOperationNameInPastParticiple
		{
			get { return Res.GetString("2ca9f199-3d07-4825-a256-70659c927c9d", "reversed"); }
		}

		protected bool IsTransactionForInactiveOrganisation
		{
			get
			{
				ZQuery inactiveOrgFilter = new ZQuery(OrgHeaderSchema.PK, OriginalTransaction.Organization);
				inactiveOrgFilter.AddToFilter(OrgHeaderSchema.OH_IsActive, false);
				return OriginalTransaction.Factory.ExistsInDatabase(OrgHeaderSchema.Constants.TableName, inactiveOrgFilter);
			}
		}

		protected bool CanReverseWhenRelatedJobIsReadyForFinancialClosure => !((OriginalTransaction as InvoicingBase)?.ExistLineWhichRelatedJobIsReadyForFinancialClosureWithoutPostSecurity ?? false);

		protected string TransactionForInactiveOrganisationErrorMessage
		{
			get { return Res.GetString("e2efefa6-2b23-482f-a35e-ad1df6fd6644", "This transaction cannot be reversed because it is for an inactive organization"); }
		}

		bool IsTransactionAttachedToCollectionBatch(IReversing transaction)
		{
			if (transaction is ARInvoice || transaction is ARCreditNote || transaction is ARAdjustmentNote || transaction is ARJournal)
			{
				ZQuery query = new ZQuery(AccCollectionOrderLineSchema.AOL_AH, transaction.PK);
				query.AddToFilter(AccCollectionOrderLineSchema.AOL_IsCancelled, false);
				return OriginalTransaction.Factory.ExistsInDatabase(AccCollectionOrderLineSchema.Constants.TableName, query);
			}
			else
			{
				return false;
			}
		}

		public bool IsPartOfMultipleReversing { get; set; }

		#endregion
	}
}
