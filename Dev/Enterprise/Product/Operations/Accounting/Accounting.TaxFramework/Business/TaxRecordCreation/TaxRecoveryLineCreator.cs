using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxRecoveryLineCreator
	{
		void CreateLines(ITaxRecordPivotProcessor pivotCreator, ITaxRecordParent taxParent, params AccTaxTransaction[] allTaxTransactionsForTaxParent);
		void DeleteLines(ITaxRecordParent taxParent);
	}

	class TaxRecoveryLineCreator : ITaxRecoveryLineCreator
	{
		void ITaxRecoveryLineCreator.DeleteLines(ITaxRecordParent taxParent)
		{
			taxParent.DeleteAllAddedTaxRecoveryLines();
		}

		void ITaxRecoveryLineCreator.CreateLines(ITaxRecordPivotProcessor pivotCreator, ITaxRecordParent taxParent, params AccTaxTransaction[] allTaxTransactionsForTaxParent)
		{
			var taxParentCompany = taxParent.Company;
			var taxParentOrgCompanyDataPK = taxParent.Org.GetCompanyDataForGlbCompany(taxParentCompany).PK;
			var taxRecoveryTaxRecords = GetTaxRecoveryTaxRecords(taxParent.Factory, taxParentOrgCompanyDataPK, allTaxTransactionsForTaxParent);
			var localTaxAmountTotal = taxRecoveryTaxRecords.Sum(taxRecord => taxRecord.ATT_LocalTaxAmount);

			if (localTaxAmountTotal != 0)
			{
				var taxRecoveryLineOrgPK = taxParent.Org.PK;
				var taxRecoveryLineChargeCodePK = GetTaxRecoveryLineChargeCodePK(taxParentCompany.PK);
				var localDecimals = taxParentCompany.GetLocalDecimals();
				var taxableTransactionLinesByLinePK = taxParent.GetLines().ToDictionary(line => line.PK);
				var allTaxRecordsPKs = allTaxTransactionsForTaxParent.Select(taxRecord => taxRecord.PK).ToList();
				var (allTaxRecordPKsByLinePKs, linePKsByTaxRecordPKs) = GetTaxRecordsAndLinesRelationBasedOnAllPivots(taxParent.Factory, allTaxRecordsPKs);

				foreach (var taxGroup in taxRecoveryTaxRecords.GroupBy(taxRecord => new TaxGroupKey(taxRecord.ATT_LocalTaxBaseAmount, taxRecord.ATT_GB, taxRecord.ATT_GE_Department)))
				{
					if (taxGroup.Key.LocalTaxBase.IsEmpty)
					{
						continue;
					}

					var lineValuesForTaxRecoveryLineByTaxRecords = taxGroup.Select(taxRecord => linePKsByTaxRecordPKs[taxRecord.PK])
									.Select(linePKs => linePKs.Select(linePK => taxableTransactionLinesByLinePK[linePK]).ToList())
									.ToList();
					ValidateTaxGroup(lineValuesForTaxRecoveryLineByTaxRecords);

					var lineValuesForTaxRecoveryLine = lineValuesForTaxRecoveryLineByTaxRecords.SelectMany(lineValues => lineValues).ToList();
					var supplyTypeForThisTaxBaseGroup = ZString.Empty;
					var totalLocalAmountForLinesForThisTaxBaseGroup = 0m;
					var lineValuesGroupedByJobPK = new Dictionary<ZGuid, List<ITaxableTransactionLine>>();

					foreach (var lineValue in lineValuesForTaxRecoveryLine)
					{
						if (supplyTypeForThisTaxBaseGroup == ZString.Empty && !lineValue.SupplyType.IsEmpty)
						{
							supplyTypeForThisTaxBaseGroup = lineValue.SupplyType;
						}

						totalLocalAmountForLinesForThisTaxBaseGroup += lineValue.LocalAmount;

						if (!lineValuesGroupedByJobPK.ContainsKey(lineValue.JobPK))
						{
							lineValuesGroupedByJobPK[lineValue.JobPK] = new List<ITaxableTransactionLine>();
						}
						lineValuesGroupedByJobPK[lineValue.JobPK].Add(lineValue);
					}

					if (totalLocalAmountForLinesForThisTaxBaseGroup == 0)
					{
						ErrorReporter.ReportOnce("TotalLocalAmountOfLinesForTaxBaseGroupIsZero", "Total of line amounts for tax base group is 0. Skipping creation of tax expense recovery line for the group.");
						continue;
					}

					GroupLineValuesByJobAndCreateTaxRecoveryLines(taxGroup, totalLocalAmountForLinesForThisTaxBaseGroup, supplyTypeForThisTaxBaseGroup, lineValuesGroupedByJobPK);
				}

				void GroupLineValuesByJobAndCreateTaxRecoveryLines(IGrouping<TaxGroupKey, AccTaxTransaction> taxGroup, ZDecimal totalLocalTaxAmountForLines, ZString supplyType, Dictionary<ZGuid, List<ITaxableTransactionLine>> lineValuesGroupedByJobPK)
				{
					var taxGroupKey = taxGroup.Key;
					var currencyCode = ZString.Empty;
					var taxDate = (ZDate)DateTime.MaxValue;
					var localTaxAmountTotal = 0m;

					foreach (var taxRecord in taxGroup)
					{
						if (currencyCode.IsEmpty)
						{
							currencyCode = taxRecord.ATT_RX_NKOSTaxCurrency;
						}

						if (taxDate > taxRecord.ATT_TaxDate)
						{
							taxDate = taxRecord.ATT_TaxDate;
						}

						localTaxAmountTotal += taxRecord.ATT_LocalTaxAmount;
					}

					var effectiveRate = localTaxAmountTotal / taxGroupKey.LocalTaxBase * -1;
					var totalRecoveryLinesAmount = taxGroupKey.LocalTaxBase * (1 / (1 - effectiveRate) - 1);

					foreach (var lineValuesWithLinkedTaxesByJobPK in lineValuesGroupedByJobPK)
					{
						var jobPK = lineValuesWithLinkedTaxesByJobPK.Key;
						var totalLocalAmountForLinesForThisJobGroup = 0m;
						var linePKsLinkedToThisJobPK = new HashSet<ZGuid>();

						foreach (var lineValue in lineValuesWithLinkedTaxesByJobPK.Value)
						{
							totalLocalAmountForLinesForThisJobGroup += lineValue.LocalAmount;
							linePKsLinkedToThisJobPK.Add(lineValue.PK);
						}

						if (totalLocalAmountForLinesForThisJobGroup == 0)
						{
							continue;
						}

						var recoveryLineAmount = Utilities.Round(totalLocalAmountForLinesForThisJobGroup * totalRecoveryLinesAmount / totalLocalTaxAmountForLines, localDecimals);
						var recoveryLine = taxParent.AddTaxRecoveryLine(taxRecoveryLineChargeCodePK, jobPK, taxGroupKey.BranchPK, taxGroupKey.DepartmentPK, currencyCode, recoveryLineAmount, taxDate, taxRecoveryLineOrgPK, supplyType);

						var accTaxTransactionsLinkedToLine = linePKsLinkedToThisJobPK.SelectMany(linePK => allTaxRecordPKsByLinePKs[linePK]).ToHashSet();
						var taxRecordsRequiringTaxBaseAmountUpdate = allTaxTransactionsForTaxParent.Where(taxRecord => accTaxTransactionsLinkedToLine.Contains(taxRecord.PK)).ToHashSet();

						CreatePivotForTaxRecoveryLine(recoveryLineAmount, recoveryLine, taxRecordsRequiringTaxBaseAmountUpdate);
					}

					void CreatePivotForTaxRecoveryLine(decimal recoveryLineAmount, ITaxableTransactionLine recoveryLine, HashSet<AccTaxTransaction> taxRecordsRequiringTaxBaseAmountUpdate)
					{
						foreach (var taxRecordForUpdate in taxRecordsRequiringTaxBaseAmountUpdate)
						{
							pivotCreator.Create(taxRecordForUpdate, recoveryLine);
							var newLocalTaxBase = taxRecordForUpdate.ATT_LocalTaxBaseAmount + recoveryLineAmount;
							var taxBaseChangeRatio = newLocalTaxBase / taxRecordForUpdate.ATT_LocalTaxBaseAmount;

							taxRecordForUpdate.ATT_OSTaxBaseAmount *= taxBaseChangeRatio;
							taxRecordForUpdate.ATT_LocalTaxBaseAmount = newLocalTaxBase;
							taxRecordForUpdate.CalculateTaxAmountFromTaxBaseAmounts();
						}
					}
				}
			}
		}

		static ZGuid GetTaxRecoveryLineChargeCodePK(ZGuid taxParentCompanyPK)
		{
			var chargeCodeRegistry = AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode;
			ZGuid chargeCodePK = chargeCodeRegistry.GetFallBackValueAtAllLevels(taxParentCompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
			if (chargeCodePK.IsEmpty)
			{
				throw new TaxFrameworkInvalidDataException(ResString.GetMultilingualString("5186F225-7B2D-4DE5-9F90-E193C5F1C931", "A Charge Code must be set in the registry '{0}'.", chargeCodeRegistry.HumanReadableRegistryPath()));
			}

			return chargeCodePK;
		}

		static AccTaxTransaction[] GetTaxRecoveryTaxRecords(BusinessObjectFactory factory, ZGuid orgCompanyDataPK, AccTaxTransaction[] accTaxTransactions)
		{
			//We do not check IsActive flags on tax config and org tax config because if tax records is created then all linked tax configs are active. We do this check when we decide to create tax record.

			var taxConfigQuery = new ZDBOnlyQuery(typeof(AccTaxConfiguration));
			taxConfigQuery.AddToFilter(AccTaxConfigurationSchema.PK, accTaxTransactions.Select(taxRecord => taxRecord.ATT_ETC).Distinct());
			taxConfigQuery.AddToFilter(AccTaxConfigurationSchema.ETC_RecoveryMethod, TaxRecoveryMethods.RecoverTaxExpense.Code);

			var orgTaxConfigQuery = new ZDBOnlySubQuery(typeof(AccOrgTaxConfiguration), AccOrgTaxConfigurationSchema.OTC_ETC);
			orgTaxConfigQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_OB, orgCompanyDataPK);
			orgTaxConfigQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_RecoverTax, true);
			taxConfigQuery.AddSubQuery(orgTaxConfigQuery, JoinCondition.And);

			var taxConfigWithEnabledRecoveryPKs = factory.Load<AccTaxConfiguration>(taxConfigQuery).Select(taxConfig => taxConfig.PK).ToHashSet();

			return accTaxTransactions.Where(taxRecord => taxConfigWithEnabledRecoveryPKs.Contains(taxRecord.ATT_ETC)).ToArray();
		}

		static void ValidateTaxGroup(IReadOnlyCollection<IReadOnlyCollection<ITaxableTransactionLine>> lineValuesForTaxRecords)
		{
			HashSet<ZGuid> prevChargeCodeSet = null;
			foreach (var lineValuesForTaxRecord in lineValuesForTaxRecords)
			{
				var currentChargeCodeSet = lineValuesForTaxRecord.Select(line => line.ChargeCode.PK).ToHashSet();

				var areSetsEqual = prevChargeCodeSet?.SetEquals(currentChargeCodeSet) ?? true;
				if (!areSetsEqual)
				{
					throw new TaxFrameworkInvalidDataException(ResString.GetMultilingualString("9495FA26-E572-44E7-AE95-56EF84B09284", "Each tax for recovery line must have the same set of charge codes on linked lines."));
				}

				prevChargeCodeSet = currentChargeCodeSet;
			}
		}

		static (Dictionary<ZGuid, HashSet<ZGuid>> TaxRecordPksByLinePKs, Dictionary<ZGuid, HashSet<ZGuid>> LinePksByTaxRecordPKs) GetTaxRecordsAndLinesRelationBasedOnAllPivots(BusinessObjectFactory factory, List<ZGuid> allTaxRecordPKs)
		{
			var allTaxRecordsLinePivots = GetTaxRecordTransactionLinePivot(factory, allTaxRecordPKs);
			var taxRecordPKsByLinePKs = new Dictionary<ZGuid, HashSet<ZGuid>>();
			var linePKsByTaxRecordPKs = new Dictionary<ZGuid, HashSet<ZGuid>>();
			foreach (var pivot in allTaxRecordsLinePivots)
			{
				if (!taxRecordPKsByLinePKs.TryGetValue(pivot.ATP_AL_TransactionLine, out var taxRecordPKs))
				{
					taxRecordPKs = new HashSet<ZGuid>();
					taxRecordPKsByLinePKs[pivot.ATP_AL_TransactionLine] = taxRecordPKs;
				}
				taxRecordPKs.Add(pivot.ATP_ATT);

				if (!linePKsByTaxRecordPKs.TryGetValue(pivot.ATP_ATT, out var linePKs))
				{
					linePKs = new HashSet<ZGuid>();
					linePKsByTaxRecordPKs[pivot.ATP_ATT] = linePKs;
				}
				linePKs.Add(pivot.ATP_AL_TransactionLine);
			}

			return (taxRecordPKsByLinePKs, linePKsByTaxRecordPKs);
		}

		record TaxGroupKey(ZDecimal LocalTaxBase, ZGuid BranchPK, ZGuid DepartmentPK);

		static AccTaxRecordTransactionLinePivot[] GetTaxRecordTransactionLinePivot(BusinessObjectFactory factory, List<ZGuid> taxRecordPKs) =>
			factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecordPKs) { FetchOnlyFromLocalCache = true });
	}
}
