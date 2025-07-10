using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class TaiwanComplianceDocumentNumberProvider : IComplianceDocumentNumberProvider
	{
		public string AllocateComplianceDocumentNumberErrorMessage(IEnumerable<ARComplianceDocumentHeader> selectedComplianceDocuments)
		{
			if (IsRegistriesEnabled && selectedComplianceDocuments.All(IsTXEWithAmountZero))
			{
				return TXEWithAmountZeroErrorMessage;
			}

			return string.Empty;
		}

		public void CheckCanAllocateComplianceDocumentNumberForSomeComplianceDocument(List<string> possiblereasonsForNotAllocatings, List<ARComplianceDocumentHeader> complianceDocumentsToAllocate)
		{
			if (IsRegistriesEnabled && complianceDocumentsToAllocate.Any(IsTXEWithAmountZero))
			{
				complianceDocumentsToAllocate.RemoveAll(IsTXEWithAmountZero);
				possiblereasonsForNotAllocatings.Add(TXEWithAmountZeroErrorMessage);
			}
		}

		public List<ZString> GetComplianceDocumentNumbers(InvoicingLineBaseCollection lines)
		{
			return lines.Cast<InvoicingLineBase>().Select(x => x.ComplianceDocumentNumber).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
		}

		bool IsRegistriesEnabled => AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value && AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value;

		bool IsTXEWithAmountZero(ARComplianceDocumentHeader complianceDocument) => complianceDocument.ComplianceSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE
			&& complianceDocument.TotalAmount == 0;

		string TXEWithAmountZeroErrorMessage => Res.GetString("BD6D940D-C982-4A2C-BE4D-3DDEBE5ADFCA", "Compliance document number cannot be allocated to TXE compliance document as the total amount of the compliance document is zero.");
	}
}
