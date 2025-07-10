using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IComplianceDocumentNumberProvider
	{
		string AllocateComplianceDocumentNumberErrorMessage(IEnumerable<ARComplianceDocumentHeader> selectedComplianceDocuments);

		void CheckCanAllocateComplianceDocumentNumberForSomeComplianceDocument(List<string> possiblereasonsForNotAllocatings, List<ARComplianceDocumentHeader> complianceDocumentsToAllocate);

		List<ZString> GetComplianceDocumentNumbers(InvoicingLineBaseCollection lines);
	}
}
