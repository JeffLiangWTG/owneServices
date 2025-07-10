using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.JobInvoicingARAPIntegration.JobChargeMapping
{
	[Immutable]
	public class TransactionLineMappingStrategyFactory
	{
		public IEnumerable<ITransactionLineMappingStrategy> CreateStrategies(ZGuid linePK, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			var matchingCriteriaCollection = additionalInfoProvider?.GetMatchingCriteriaCollection(linePK);
			if (matchingCriteriaCollection == null)
			{
				return Array.Empty<ITransactionLineMappingStrategy>();
			}

			var candidateStrategies = new ITransactionLineMappingStrategy[]
			{
				new PrimaryKeyMappingStrategy(),
				new DisplaySequenceMappingStrategy()
			};

			var availableStrategies = candidateStrategies.Where(s => s.GetMatchingCriteria(matchingCriteriaCollection) != null);
			var criticalStrategy = availableStrategies.FirstOrDefault(s => s.IsCritical);

			return criticalStrategy != null ? new[] { criticalStrategy } : availableStrategies;
		}
	}
}
