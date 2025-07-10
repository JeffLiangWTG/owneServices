using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class PrimaryKeyMappingStrategy : ITransactionLineMappingStrategy
	{
		public string GetMatchingCriteria(IEnumerable<MatchingCriteria> matchingCriteriaCollection)
		{
			return matchingCriteriaCollection?.FirstOrDefault(x => x?.FieldName.ToString() == "PrimaryKey")?.Value;
		}

		public Charge[] TryMapTransactionLine(BusinessObjectFactory factory, PostingJournal universalLine, InvoicingLineBase line, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			var matchingCriteriaCollection = universalLine.ImportMetaData?.MatchingCriteriaCollection;
			if (matchingCriteriaCollection == null)
			{
				return Array.Empty<Charge>();
			}

			var rawPK = GetMatchingCriteria(matchingCriteriaCollection);
			if (rawPK == null)
			{
				return Array.Empty<Charge>();
			}

			var primaryKey = ZGuid.ParseSafe(rawPK);
			if (!primaryKey.IsValid)
			{
				TransactionImportJobChargeMappingProvider.RecordMappingInfo(factory, line.PK, ZGuid.Invalid, JobChargeMappingInfoType.InvalidKey);
				return Array.Empty<Charge>();
			}

			var jobNumber = universalLine.Job.GetValueSafe(x => x.Key).GetValueOrDefault();
			var isConsolApportioned = !string.IsNullOrEmpty(universalLine.CostSource?.Type);

			if (isConsolApportioned)
			{
				if (jobNumber.IsEmpty)
				{
					additionalInfoProvider.MapTransactionLineWithConsolCost(line.PK, primaryKey);
				}
				else
				{
					additionalInfoProvider.MapTransactionLineWithApportionedCharge(line.PK, primaryKey);
				}

				return Array.Empty<Charge>();
			}

			if (jobNumber.IsEmpty)
			{
				return Array.Empty<Charge>();
			}

			var chargesToSearch = new JobChargesImporter(line).JobChargesCollection;
			var matchedCharge = chargesToSearch.FirstOrDefault(x => x.PK == primaryKey) as Charge;

			return matchedCharge == null ? Array.Empty<Charge>() : new[] { matchedCharge };
		}

		public ZQuery GetRelatedApportionedChargesFilter(InvoicingLineBase invoiceLine, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			if (additionalInfoProvider?.GetApportionedChargePK(invoiceLine.PK) is var chargePK && chargePK.HasValue)
			{
				return new ZQuery(JobChargeSchema.PK, chargePK);
			}

			return ZQuery.NoResultQuery;
		}

		public ZQuery GetRelatedConsolCostsFilter(InvoicingLineBase invoiceLine, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			if (additionalInfoProvider?.GetConsolCostPK(invoiceLine.PK) is var consolCostPK && consolCostPK.HasValue)
			{
				return new ZQuery(JobConsolCostSchema.PK, consolCostPK);
			}

			return ZQuery.NoResultQuery;
		}

		public bool IsCritical => true;
	}
}
