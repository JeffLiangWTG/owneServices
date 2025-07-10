using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface ITransactionBranchCalculationDataProviderFromJobCharge
	{
		ZGuid JobBranchPK { get; }

		HashSet<ZGuid> LineBranchPKs { get; }

		bool AnyCharges { get;  }
	}

	public interface IBranchLevelPostingHelper
	{
		void SetTransactionHeaderBranch(TransactionHeaderWithLines transaction, InvoiceProcessingLevelIsAllowingToResetBranch invoiceLevel, ITransactionBranchCalculationDataProviderFromJobCharge charges = null);
		ZGuid GetTransactionHeaderBranchForOverrideBranchAndDepartment(TransactionHeaderWithLines transaction);
	}

	public class BranchLevelPostingHelper : IBranchLevelPostingHelper
	{
		public enum InvoiceProcessingLevelIsAllowingToResetBranch
		{
			Creation,
			TaxTransactionCalculation,
			Saving
		}

		void IBranchLevelPostingHelper.SetTransactionHeaderBranch(TransactionHeaderWithLines transaction, InvoiceProcessingLevelIsAllowingToResetBranch invoiceLevel, ITransactionBranchCalculationDataProviderFromJobCharge charges)
		{
			ObjectFactory.Get<IAccountingDependencyFactory>().GetSingleActionPerTransactionOnDifferentLevels().RunSingleActionPerTransactionOnDifferentLevels(transaction, typeof(BranchLevelPostingHelper).ToString(), () =>
			{
				if (!transaction.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment))
				{
					var invoiceBranch = GetTransactionHeaderBranch(transaction, () => transaction.AH_GB, !transaction.AH_JH.IsEmpty, charges);
					if (invoiceBranch != transaction.AH_GB)
					{
						transaction.AH_GB = invoiceBranch;
					}
				}
			}, (int)invoiceLevel);
		}

		ZGuid IBranchLevelPostingHelper.GetTransactionHeaderBranchForOverrideBranchAndDepartment(TransactionHeaderWithLines transaction) => GetTransactionHeaderBranch(transaction, () => (ZGuid)transaction.AH_GBInfo.OriginalValue, !transaction.AH_JH.IsEmpty);

		static ZGuid GetTransactionHeaderBranch(TransactionHeaderWithLines transaction, Func<ZGuid> getBranchWhenPostingAtCompanyLevel, bool isJobRelated, ITransactionBranchCalculationDataProviderFromJobCharge charges = null)
		{
			var anyLines = charges?.AnyCharges ?? transaction.Lines.Any();
			var enforceBranchLevelPostingRegistryItem = transaction.EnforceBranchLevelPostingRegistryItem;

			if (anyLines && enforceBranchLevelPostingRegistryItem != null && enforceBranchLevelPostingRegistryItem.Value.EnableBranchLevelPosting)
			{
				HashSet<ZGuid> lineBranchPKs;
				ZGuid jobBranchPK;
				if (charges != null)
				{
					lineBranchPKs = charges.LineBranchPKs;
					jobBranchPK = charges.JobBranchPK;
				}
				else
				{
					var transactionLines = transaction.Lines.Cast<DependentTransactionLine>();
					lineBranchPKs = transactionLines.Select(x => x.AL_GB).ToHashSet();
					jobBranchPK = isJobRelated ? ((transaction.Job?.Branch?.PK ?? transactionLines.FirstOrDefault().Job?.Branch?.PK) ?? ZGuid.Empty) : ZGuid.Empty;
				}

				var headerBranch = GetHeaderBranch(lineBranchPKs, jobBranchPK, enforceBranchLevelPostingRegistryItem);
				if (!headerBranch.IsEmpty)
				{
					return headerBranch;
				}
			}
			else if (enforceBranchLevelPostingRegistryItem == null || !AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.Value)
			{
				return getBranchWhenPostingAtCompanyLevel();
			}

			return GlbBranch.CurrentBranch.PK;
		}

		public static ZGuid GetParentBranchPK(BranchLevelPostingConfigurationRegistryItem branchLevelPostingConfigurationRegistryItem, ZGuid branchPK)
		{
			if (!branchLevelPostingConfigurationRegistryItem.Value.EnableBranchLevelPosting)
			{
				throw new ArgumentException("Invalid argument.", nameof(branchLevelPostingConfigurationRegistryItem));
			}

			var branchGroupSettings = branchLevelPostingConfigurationRegistryItem.Value.BranchGroupSettingsCollection.Cast<BranchGroupSettings>();
			var groupNumber = from n in branchGroupSettings
							  where n.BranchPK == branchPK
							  select n.GroupNumber;

			if (groupNumber.Any())
			{
				return (from n in branchGroupSettings
						where n.GroupNumber == groupNumber.First() && n.IsParentBranch
						select n.BranchPK).FirstOrDefault();
			}

			return branchPK;
		}

		public static HashSet<ZGuid> GetAssociatedBranches(BranchLevelPostingConfigurationRegistryItem branchLevelPostingConfigurationRegistryItem, ZGuid branchPK)
		{
			if (!branchLevelPostingConfigurationRegistryItem.Value.EnableBranchLevelPosting)
			{
				throw new ArgumentException("Invalid argument.", nameof(branchLevelPostingConfigurationRegistryItem));
			}

			var result = new HashSet<ZGuid>();

			var branchGroupSettings = branchLevelPostingConfigurationRegistryItem.Value.BranchGroupSettingsCollection.Cast<BranchGroupSettings>();
			var groupNumber = from n in branchGroupSettings
							  where n.BranchPK == branchPK
							  select n.GroupNumber;

			if (groupNumber.Any())
			{
				result = (from n in branchGroupSettings
						  where n.GroupNumber == groupNumber.First()
						  select n.BranchPK).ToHashSet();
			}

			if (!result.Any())
			{
				result.Add(branchPK);
			}

			return result;
		}

		public static bool DoesAllBranchesBelongToSamePostingGroup(BranchLevelPostingConfigurationRegistryItem branchLevelPostingConfigurationRegistryItem, HashSet<ZGuid> branchPKs)
		{
			var branchGroupSettings = branchLevelPostingConfigurationRegistryItem.Value.BranchGroupSettingsCollection.Cast<BranchGroupSettings>();

			var groups = from n in branchGroupSettings
						 where branchPKs.Contains(n.BranchPK)
						 group n by n.GroupNumber into g
						 select g;

			if (!groups.Any() && branchPKs.Count == 1) //there is no group setting
			{
				return true;
			}

			var configurationBranchPKs = (from n in branchGroupSettings
										  select n.BranchPK).ToHashSet();

			if (branchPKs.IsSubsetOf(configurationBranchPKs) && groups.Count() == 1)
			{
				return true;
			}

			return false;
		}

		static ZGuid GetHeaderBranch(HashSet<ZGuid> lineBranchPKs, ZGuid jobBranchPK, BranchLevelPostingConfigurationRegistryItem branchLevelPostingConfigurationRegistryItem)
		{
			if (lineBranchPKs.Count == 1)
			{
				return lineBranchPKs.First();
			}

			var settingsCollection = branchLevelPostingConfigurationRegistryItem.Value.BranchGroupSettingsCollection.Cast<BranchGroupSettings>();
			if (settingsCollection.Any())
			{
				var groups = settingsCollection.GroupBy(x => x.GroupNumber).Select(y => new { GroupNumber = y.Key, Groups = y });
				foreach (var group in groups)
				{
					var configurationBranchPKs = group.Groups.Select(x => x.BranchPK).ToHashSet();

					if (lineBranchPKs.IsSubsetOf(configurationBranchPKs))
					{
						return !jobBranchPK.IsEmpty && lineBranchPKs.Contains(jobBranchPK)
									? jobBranchPK
									: group.Groups.Where(x => x.IsParentBranch).Select(y => y.BranchPK).FirstOrDefault();
					}
				}
			}

			return ZGuid.Empty;
		}

#if DEBUG
		public static ZGuid GetHeaderBranchForTestOnly(HashSet<ZGuid> lineBranchPKs, ZGuid jobBranchPK, BranchLevelPostingConfigurationRegistryItem branchLevelPostingConfigurationRegistryItem)
		{
			return GetHeaderBranch(lineBranchPKs, jobBranchPK, branchLevelPostingConfigurationRegistryItem);
		}
#endif

	}
}
