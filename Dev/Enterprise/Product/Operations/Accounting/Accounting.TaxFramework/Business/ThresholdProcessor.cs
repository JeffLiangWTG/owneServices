using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IThresholdProcessor
	{
		void Process(ITaxRecordPivotProcessor pivotProcessor, ITaxRecordParent taxParent, List<AccTaxTransaction> taxRecords, HashSet<string> thresholdMethods);
		HashSet<AccTaxTransaction> GetTaxRecordsOutsideOfThreshold(ITaxRecordParentBase taxParent, List<AccTaxTransaction> taxRecords, HashSet<string> thresholdMethods);
	}

	class ThresholdAmountProcessor : IThresholdProcessor
	{
		void IThresholdProcessor.Process(ITaxRecordPivotProcessor pivotProcessor, ITaxRecordParent taxParent, List<AccTaxTransaction> taxRecords, HashSet<string> thresholdMethods)
		{
			var taxRecordsToDelete = ((IThresholdProcessor)this).GetTaxRecordsOutsideOfThreshold(taxParent, taxRecords, thresholdMethods);

			taxRecords.RemoveAll(x => taxRecordsToDelete.Contains(x));
			pivotProcessor.DeleteTaxRecordsWithPivots(taxRecordsToDelete.ToArray());
		}

		HashSet<AccTaxTransaction> IThresholdProcessor.GetTaxRecordsOutsideOfThreshold(ITaxRecordParentBase taxParent, List<AccTaxTransaction> taxRecords, HashSet<string> thresholdMethods)
		{
			var additionalTaxConfigurationParameters = GetTaxConfigurations(taxParent, taxRecords, thresholdMethods);

			decimal multiplier = taxParent.Ledger == LedgerTypes.AccountsPayable ? -1 : 1;

			var groupedByCodes = taxRecords.Where(x => additionalTaxConfigurationParameters.ContainsKey(x.ATT_ETC)).
				GroupBy(x => { var item = additionalTaxConfigurationParameters[x.ATT_ETC];
				return (item.ThresholdMethod, item.ThresholdAmount); });

			var taxTransactionsHashSet = new HashSet<AccTaxTransaction>();
			foreach (var group in groupedByCodes)
			{
				IEnumerable<AccTaxTransaction> taxTransactions;
				if (group.Key.ThresholdMethod == ETC_ThresholdMethods.TransactionLevelTaxBase.Code)
				{
					taxTransactions = ProcessLocalTaxBaseAmount(group, group.Key.ThresholdAmount, multiplier);
				}
				else
				{
					var taxTransactionsGrouping = ProcessGroupByThresholdMethod(group.Key.ThresholdMethod, group, additionalTaxConfigurationParameters);
					taxTransactions = ProcessLocalTaxAmountTotal(taxTransactionsGrouping, group.Key.ThresholdAmount, multiplier);
				}
				taxTransactionsHashSet.UnionWith(taxTransactions);
			}

			return taxTransactionsHashSet;
		}

		static IEnumerable<IEnumerable<AccTaxTransaction>> ProcessGroupByThresholdMethod(string thresholdMethod, IEnumerable<AccTaxTransaction> accTaxTransactions, Dictionary<ZGuid, AdditionalTaxConfigurationParameters> additionalTaxConfigurationParameters)
		{
			if (thresholdMethod == ETC_ThresholdMethods.TransactionLevelGroup.Code)
			{
				return accTaxTransactions.GroupBy(x => additionalTaxConfigurationParameters[x.ATT_ETC].ParentID);
			}
			else
			{
				return accTaxTransactions.GroupBy(x => x.ATT_ETC);
			}
		}

		static IEnumerable<AccTaxTransaction> ProcessLocalTaxAmountTotal(IEnumerable<IEnumerable<AccTaxTransaction>> taxTransactions, ZDecimal thresholdAmount, decimal multiplier)
		{
			return taxTransactions.Where(group => group.Sum(x => x.GetLocalTaxAmountWithoutSign()) * multiplier < thresholdAmount)
				.SelectMany(x => x);
		}

		static IEnumerable<AccTaxTransaction> ProcessLocalTaxBaseAmount(IEnumerable<AccTaxTransaction> accTaxTransactions, ZDecimal thresholdAmount, decimal multiplier)
		{
			return accTaxTransactions.Where(x => x.ATT_LocalTaxBaseAmount * multiplier < thresholdAmount);
		}

		static Dictionary<ZGuid, AdditionalTaxConfigurationParameters> GetTaxConfigurations(ITaxRecordParentBase taxParent, List<AccTaxTransaction> taxRecords, HashSet<string> thresholdMethods)
		{
			var additionalTaxConfigurationParameters = new Dictionary<ZGuid, AdditionalTaxConfigurationParameters>();
			var taxConfigurationQuery = new ZDBOnlyQuery(typeof(AccTaxConfiguration));
			taxConfigurationQuery.AddToFilter(AccTaxConfigurationSchema.PK, taxRecords.Select(x => x.ATT_ETC).Distinct());
			taxConfigurationQuery.AddToFilter(AccTaxConfigurationSchema.ETC_ThresholdMethod, thresholdMethods);

			var orgTaxConfigQuery = new ZDBOnlySubQuery(typeof(AccOrgTaxConfiguration), AccOrgTaxConfigurationSchema.OTC_ETC);
			orgTaxConfigQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_OB, taxParent.Org.GetCompanyDataForGlbCompany(taxParent.Company).PK);
			orgTaxConfigQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_IsThresholdUsed, true);

			taxConfigurationQuery.AddSubQuery(orgTaxConfigQuery, JoinCondition.And);

			var taxConfigs = taxParent.Factory.Load<AccTaxConfiguration>(taxConfigurationQuery);
			foreach (var taxConfig in taxConfigs)
			{
				additionalTaxConfigurationParameters.Add(taxConfig.PK, new AdditionalTaxConfigurationParameters(taxConfig.ETC_ParentId, taxConfig.ETC_ThresholdMethod,taxConfig.ETC_ThresholdAmount));
			}
			return additionalTaxConfigurationParameters;
		}

		class AdditionalTaxConfigurationParameters
		{
			public ZGuid ParentID { get; }
			public ZString ThresholdMethod { get; }
			public ZDecimal ThresholdAmount { get; }

			public AdditionalTaxConfigurationParameters(ZGuid parentID, ZString thresholdMethod, ZDecimal thresholdAmount)
			{
				ParentID = parentID;
				ThresholdMethod = thresholdMethod;
				ThresholdAmount = thresholdAmount;
			}
		}
	}
}
