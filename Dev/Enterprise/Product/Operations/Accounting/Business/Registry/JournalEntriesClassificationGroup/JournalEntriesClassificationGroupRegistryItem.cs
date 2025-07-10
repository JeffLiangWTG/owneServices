using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JournalEntriesClassificationGroupRegistryItem : StronglyTypedRegistryItem<JournalEntriesClassificationGroupCollection>
	{
		public JournalEntriesClassificationGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new JournalEntriesClassificationGroupRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		public class JournalEntriesClassificationGroupRegistryItemImpl : RegistryItemImpl
		{
			public JournalEntriesClassificationGroupRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new JournalEntriesClassificationGroupRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var collection = new JournalEntriesClassificationGroupCollection();
				collection.AddRange(GetJournalEntriesClassificationGroupCollectionForARAP());
				collection.AddRange(GetJournalEntriesClassificationGroupCollectionForCashBook());
				collection.AddRange(GetJournalEntriesClassificationGroupCollectionForJobCosting());
				collection.AddRange(GetJournalEntriesClassificationGroupCollectionForGLJournal());

				return collection;
			}

			JournalEntriesClassificationGroupCollection GetJournalEntriesClassificationGroupCollectionForARAP()
			{
				var collection = new JournalEntriesClassificationGroupCollection();
				var transactionTypeForARAP = new string[]
				{
					TransactionTypes.Invoice,
					TransactionTypes.CreditNote,
					TransactionTypes.AdjustmentNote,
					TransactionTypes.Journal,
					TransactionTypes.Transfer,
					TransactionTypes.Contra,
					TransactionTypes.Receipt,
					TransactionTypes.Payment,
					TransactionTypes.Overpayment,
					TransactionTypes.Discount,
					TransactionTypes.ExchangeDifference
				};

				foreach (var ledger in new string[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable })
				{
					foreach (var transactionType in transactionTypeForARAP)
					{
						var journalEntriesClassificationGroupARAP = collection.AddNew();
						journalEntriesClassificationGroupARAP.Ledger = ledger;
						journalEntriesClassificationGroupARAP.TransactionType = transactionType;
					}
				}

				return collection;
			}

			JournalEntriesClassificationGroupCollection GetJournalEntriesClassificationGroupCollectionForCashBook()
			{
				var collection = new JournalEntriesClassificationGroupCollection();

				var journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.CashBook;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.DirectReceipt;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.CashBook;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.DirectPayment;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.CashBook;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.Transfer;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.CashBook;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.ExchangeDifference;

				return collection;
			}

			JournalEntriesClassificationGroupCollection GetJournalEntriesClassificationGroupCollectionForJobCosting()
			{
				var collection = new JournalEntriesClassificationGroupCollection();

				var journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.JobCosting;
				journalEntriesClassificationGroup.TransactionType = TransactionLineTypes.Accrual;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.JobCosting;
				journalEntriesClassificationGroup.TransactionType = TransactionLineTypes.WIP;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.JobCosting;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.Journal;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.JobCosting;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.JobRevenueJournal;

				return collection;
			}

			JournalEntriesClassificationGroupCollection GetJournalEntriesClassificationGroupCollectionForGLJournal()
			{
				var collection = new JournalEntriesClassificationGroupCollection();

				var journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.General;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.GLStandardJournal;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.General;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.GLReversingJournal;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.General;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.GLAutoJournal;
				journalEntriesClassificationGroup = collection.AddNew();
				journalEntriesClassificationGroup.Ledger = LedgerTypes.General;
				journalEntriesClassificationGroup.TransactionType = TransactionTypes.GLNoteJournal;

				return collection;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JournalEntriesClassificationGroupRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class JournalEntriesClassificationGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JournalEntriesClassificationGroupCollection>
	{
		public JournalEntriesClassificationGroupRegistryDataType()
		{
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;

		protected override void ValidateCore(IRegistryItem registryItem, JournalEntriesClassificationGroupCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			proposedValue.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);
			proposedValue.Cast<JournalEntriesClassificationGroup>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK));
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, JournalEntriesClassificationGroupCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			var customizationSetting = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			if (customizationSetting?.IsOptionGen ?? false)
			{
				throw new RegistryValidationException(Res.GetString("3DD94623-F121-4F9B-A94B-66E01285E371",
					"The Journal Entries Classification Group cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry."));
			}
		}
	}
}
