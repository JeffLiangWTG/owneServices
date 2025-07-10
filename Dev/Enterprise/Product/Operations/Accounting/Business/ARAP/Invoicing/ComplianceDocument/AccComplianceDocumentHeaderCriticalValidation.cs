using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class AccComplianceDocumentHeaderCriticalValidation : CriticalValidation<AccComplianceDocumentHeader>
	{
		public AccComplianceDocumentHeaderCriticalValidation(AccComplianceDocumentHeader parent) : base(parent)
		{
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			if (Parent.AddressOverride != null && (!Parent.IsInDatabase || Parent.ADH_OA_AddressOverrideInfo.HasChanges || Parent.ADH_OH_OrganisationInfo.HasChanges) && Parent.ADH_OH_Organisation != Parent.AddressOverride.Header.PK)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var info = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderCallStack);
				yield return new CriticalValidationResult(CriticalValidationErrorType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeader_1,
					CriticalValidationMessageTemplate.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderErrorMessage,
					Parent.GetAccComplianceDocumentHeaderInfo(),
					info);
			}

			if (!Parent.ADH_DocumentNumber.IsEmpty && (!Parent.IsInDatabase || Parent.ADH_DocumentNumberInfo.HasChanges))
			{
				var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, Parent.ADH_DocumentNumber);
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, Parent.ADH_Ledger);
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_XD_ComplianceBook, Parent.ADH_XD_ComplianceBook);
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_GC_Company, Parent.ADH_GC_Company);
				var matchedComplianceDocuments = Parent.Factory.Load<AccComplianceDocumentHeader>(query);

				if (matchedComplianceDocuments.Any())
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.AccComplianceDocumentNumberAlreadyInUse, CriticalValidationMessageTemplate.AccComplianceDocumentNumberAlreadyInUseErrorMessage);
				}
			}
		}
	}
}
