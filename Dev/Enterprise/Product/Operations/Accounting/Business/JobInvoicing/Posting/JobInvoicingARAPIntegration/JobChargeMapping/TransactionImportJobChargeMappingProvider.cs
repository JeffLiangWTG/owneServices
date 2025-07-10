using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.JobInvoicingARAPIntegration.JobChargeMapping;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	//Please read the Wiki before modifying this class: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13596/Select-accrual-for-reversal-during-AP-Invoice-XUT-Import
	public static class TransactionImportJobChargeMappingProvider
	{
		public static TransactionImportAdditionalInfoProvider Initialize(BusinessObjectFactory factory, TransactionInfo universalTransaction)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.Value)
			{
				return null;
			}

			var ledger = universalTransaction.Ledger.GetValueOrDefault();
			var isCancelled = universalTransaction.IsCancelled.GetValueOrDefault();
			var transactionType = universalTransaction.TransactionType.GetValueOrDefault().ToString();
			if (ledger != LedgerTypes.AccountsPayable || isCancelled)
			{
				return null;
			}

			if (transactionType != TransactionTypes.Invoice && transactionType != TransactionTypes.CreditNote)
			{
				return null;
			}

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			factory.ServiceContainer.RemoveService<TransactionImportAdditionalInfoProvider>();
			factory.ServiceContainer.AddService<TransactionImportAdditionalInfoProvider>(additionalInfoProvider);

			return additionalInfoProvider;
		}

		public static void MapTransactionLine(BusinessObjectFactory factory, PostingJournal universalLine, InvoicingLineBase line)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.Value)
			{
				return;
			}

			var additionalInfoProvider = factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>();
			if (additionalInfoProvider == null)
			{
				return;
			}

			var matchingCriteriaCollection = universalLine.ImportMetaData?.MatchingCriteriaCollection;
			if (matchingCriteriaCollection == null || !matchingCriteriaCollection.Any())
			{
				return;
			}
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(line.PK, matchingCriteriaCollection);

			var mappingStrategies = new TransactionLineMappingStrategyFactory().CreateStrategies(line.PK, additionalInfoProvider);
			if (!mappingStrategies.Any())
			{
				return;
			}

			IEnumerable<Charge> charges = null;
			foreach (var strategy in mappingStrategies)
			{
				var matchedCharges = strategy.TryMapTransactionLine(factory, universalLine, line, additionalInfoProvider);
				charges = charges == null ? matchedCharges : charges.Intersect(matchedCharges);
			}

			if (charges != null && charges.Count() == 1)
			{
				MatchLineAndCharge(line, charges.First());
			}
		}

		public static ApportionSplitCharge GetRelatedApportionCharge(BusinessObjectFactory factory, ZGuid consolPK, InvoicingLineBase invoiceLine)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.Value)
			{
				return null;
			}

			var additionalInfoProvider = factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>();
			if (additionalInfoProvider == null)
			{
				return null;
			}

			var mappingStrategies = new TransactionLineMappingStrategyFactory().CreateStrategies(invoiceLine.PK, additionalInfoProvider);
			if (!mappingStrategies.Any())
			{
				return null;
			}

			var chargeQuery = new ZQuery();
			foreach(var strategy in mappingStrategies)
			{
				var strategyFilter = strategy.GetRelatedApportionedChargesFilter(invoiceLine, additionalInfoProvider);
				chargeQuery.AddToFilter(strategyFilter);
			}

			var matchedCharges = factory.Load<ApportionSplitCharge>(chargeQuery).Where(x => x.ParentConsolCost?.E6_ParentID == consolPK);
			return matchedCharges.Count() == 1 ? matchedCharges.First() : null;
		}

		public static bool IsRelatedApportionChargeMatchedConsolCost(JobConsolCost targetConsolCost, ApportionSplitCharge relatedApportionCharge)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.Value)
			{
				return true;
			}

			return targetConsolCost.RelatedConsolCostPK == (relatedApportionCharge?.JR_E6 ?? ZGuid.Empty);
		}

		public static void SetRelatedApportionCharge(BusinessObjectFactory factory, ZGuid linePK, JobConsolCost targetConsolCost, IEnumerable<ApportionSplitCharge> relatedApportionCharges)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.Value)
			{
				return;
			}

			if (relatedApportionCharges == null || !relatedApportionCharges.Any())
			{
				return;
			}

			var chargesToUpdate = targetConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>().ToList();

			foreach (var charge in chargesToUpdate)
			{
				if (charge.RelatedApportionChargeFromDB == null)
				{
					var validRelatedCharge = relatedApportionCharges.FirstOrDefault(x => charge.ValidateIfRelatedApportionChargeCanBeMarkedAsImported(x));
					charge.RelatedApportionChargeFromDB = validRelatedCharge;

					if (validRelatedCharge != null)
					{
						TransactionImportJobChargeMappingProvider.RecordMappingInfo(factory, linePK, validRelatedCharge.PK, JobChargeMappingInfoType.KeyMatched);
					}
				}
			}
		}

		public static void SetRelatedConsolCost(JobConsolCost targetConsolCost, ApportionSplitCharge relatedApportionCharge)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.Value)
			{
				return;
			}

			if (relatedApportionCharge == null)
			{
				return;
			}

			targetConsolCost.RelatedConsolCostPK = relatedApportionCharge.JR_E6;
		}

		public static void SetRelatedConsolCostAndApportionCharge(JobConsolCost consolCost, InvoicingLineBase lineForConsolCost)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.Value)
			{
				return;
			}

			var factory = lineForConsolCost.Factory;
			var additionalInfoProvider = factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>();
			if (additionalInfoProvider == null)
			{
				return;
			}

			var mappingStrategies = new TransactionLineMappingStrategyFactory().CreateStrategies(lineForConsolCost.PK, additionalInfoProvider);
			if (!mappingStrategies.Any())
			{
				return;
			}

			var consolCostQuery = new ZQuery();
			consolCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			consolCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, consolCost.E6_ParentID);

			foreach(var strategy in mappingStrategies)
			{
				var strategyFilter = strategy.GetRelatedConsolCostsFilter(lineForConsolCost, additionalInfoProvider);
				consolCostQuery.AddToFilter(strategyFilter);
			}

			var matchedConsolCosts = factory.Load<JobConsolCost>(consolCostQuery);
			var associatedConsolCost = matchedConsolCosts.Length == 1 ? matchedConsolCosts.First() : null;

			if (associatedConsolCost == null)
			{
				return;
			}

			consolCost.RelatedConsolCostPK = associatedConsolCost.PK;
			SetRelatedApportionCharge(factory, lineForConsolCost.PK, consolCost, associatedConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>().ToList());
		}

		public static void RecordMappingInfo(BusinessObjectFactory factory, ZGuid linePk, ZGuid targetPK, JobChargeMappingInfoType infoType)
		{
			var additionalInfoProvider = factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>();
			if (additionalInfoProvider == null)
			{
				return;
			}

			additionalInfoProvider.TransactionLineMappingInfoCollection.Add((linePk, targetPK, infoType));
		}

		public static bool Validate(BusinessObjectFactory factory)
		{
			var additionalInfoProvider = factory.ServiceContainer.GetService<TransactionImportAdditionalInfoProvider>();
			if (additionalInfoProvider == null)
			{
				return true;
			}

			var hasInvalidKey = additionalInfoProvider.TransactionLineMappingInfoCollection.Any(x => x.InfoType == JobChargeMappingInfoType.InvalidKey);
			if (hasInvalidKey)
			{
				return false;
			}

			var matchedLines = additionalInfoProvider.TransactionLineMappingInfoCollection.Where(x => x.InfoType == JobChargeMappingInfoType.KeyMatched).Select(x => x.LinePK).Distinct();
			var sourceLines = additionalInfoProvider.TransactionLineMatchingCriteriaCollectionMapper.Keys;
			var hasLineNotMatched = matchedLines.ToHashSet().IsProperSubsetOf(sourceLines);
			if (hasLineNotMatched)
			{
				return false;
			}

			var hasDuplicatedTarget = additionalInfoProvider.TransactionLineMappingInfoCollection.Where(x => x.InfoType == JobChargeMappingInfoType.KeyMatched).GroupBy(x => x.TargetPK).Any(x => x.Distinct().Count() > 1);
			if (hasDuplicatedTarget)
			{
				return false;
			}

			return true;
		}

		static void MatchLineAndCharge(InvoicingLineBase line, Charge charge)
		{
			if (charge != null && line.ValidateIfLineCanBeMarkedAsImported(charge))
			{
				(line.TransactionHeader as InvoicingBase)?.PreImportJobChargesValidation(new[] { charge });
				line.OriginalJobCharge = charge;

				TransactionImportJobChargeMappingProvider.RecordMappingInfo(line.Factory, line.PK, charge.PK, JobChargeMappingInfoType.KeyMatched);
			}
		}
	}
}
