using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface ITransactionLineMappingStrategy
	{
		string GetMatchingCriteria(IEnumerable<MatchingCriteria> matchingCriteriaCollection);

		Charge[] TryMapTransactionLine(BusinessObjectFactory factory, PostingJournal universalLine, InvoicingLineBase line, TransactionImportAdditionalInfoProvider additionalInfoProvider);

		ZQuery GetRelatedApportionedChargesFilter(InvoicingLineBase invoiceLine, TransactionImportAdditionalInfoProvider additionalInfoProvider);

		ZQuery GetRelatedConsolCostsFilter(InvoicingLineBase invoiceLine, TransactionImportAdditionalInfoProvider additionalInfoProvider);

		bool IsCritical { get; }
	}
}
