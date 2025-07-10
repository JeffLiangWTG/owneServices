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
	public class DisplaySequenceMappingStrategy : ITransactionLineMappingStrategy
	{
		public string GetMatchingCriteria(IEnumerable<MatchingCriteria> matchingCriteriaCollection)
		{
			return matchingCriteriaCollection?.FirstOrDefault(x => x?.FieldName.ToString() == "DisplaySequence")?.Value;
		}

		public Charge[] TryMapTransactionLine(BusinessObjectFactory factory, PostingJournal universalLine, InvoicingLineBase line, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			var matchingCriteriaCollection = universalLine.ImportMetaData?.MatchingCriteriaCollection;
			if (matchingCriteriaCollection == null)
			{
				return Array.Empty<Charge>();
			}

			var rawDisplaySequence = GetMatchingCriteria(matchingCriteriaCollection);
			if (rawDisplaySequence == null)
			{
				return Array.Empty<Charge>();
			}

			if (!ZShort.TryParse(rawDisplaySequence, out var displaySequence))
			{
				TransactionImportJobChargeMappingProvider.RecordMappingInfo(factory, line.PK, ZGuid.Invalid, JobChargeMappingInfoType.InvalidKey);
				return Array.Empty<Charge>();
			}

			var jobNumber = universalLine.Job.GetValueSafe(x => x.Key).GetValueOrDefault();
			if (jobNumber.IsEmpty)
			{
				return Array.Empty<Charge>();
			}

			var isConsolApportioned = !string.IsNullOrEmpty(universalLine.CostSource?.Type);
			if (isConsolApportioned)
			{
				additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(line.PK, displaySequence);
				return Array.Empty<Charge>();
			}

			var chargesToSearch = new JobChargesImporter(line).JobChargesCollection;
			var matchedCharges = chargesToSearch.OfType<Charge>().Where(x => x.JR_DisplaySequence == displaySequence).ToArray();

			return matchedCharges;
		}

		public ZQuery GetRelatedApportionedChargesFilter(InvoicingLineBase invoiceLine, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			if (additionalInfoProvider?.GetApportionedChargeDisplaySequence(invoiceLine.PK) is var displaySequence && displaySequence.HasValue)
			{
				var filter = new ZDBOnlyQuery(typeof(ApportionSplitCharge));
				filter.AddToFilter(JobChargeSchema.JR_JH, invoiceLine.Job.PK);
				filter.AddToFilter(JobChargeSchema.JR_DisplaySequence, displaySequence);

				return filter;
			}

			return ZQuery.NoResultQuery;
		}

		public ZQuery GetRelatedConsolCostsFilter(InvoicingLineBase invoiceLine, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			return ZQuery.NoResultQuery;
		}

		public bool IsCritical => false;
	}
}
