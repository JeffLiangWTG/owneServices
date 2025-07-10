using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class EntryLineConfiguration
	{
		public ZBool SupportingDocumentsSupport(BusinessObject businessObject) => SupportingDocumentsSupportCore(businessObject);
		protected virtual ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => false;

		public ZBool MergeJI_RN_NKCountryOfExport(JobDeclaration declaration) => MergeJI_RN_NKCountryOfExportCore(declaration);
		protected virtual ZBool MergeJI_RN_NKCountryOfExportCore(JobDeclaration declaration) => false;

		public IEntryLineValidationDecider GetValidationDecider(CusEntryLine cusEntryLine) => GetValidationDeciderCore(cusEntryLine);

		protected virtual IEntryLineValidationDecider GetValidationDeciderCore(CusEntryLine cusEntryLine)
		{
			var declaration = cusEntryLine.Declaration;
			if ((object)declaration != null && declaration.IsUCC6)
			{
				if ((bool)declaration.IsImport)
				{
					return GetImportValidationDecider();
				}

				if ((bool)declaration.IsExport)
				{
					return GetExportValidationDecider();
				}

				return null;
			}

			return null;
		}

		protected virtual IEntryLineValidationDecider GetImportValidationDecider() => null;

		protected virtual IEntryLineValidationDecider GetExportValidationDecider() => null;

		public ZBool ShouldFilterSupportingDocumentsByMergeKeys() => ShouldFilterSupportingDocumentsByMergeKeysCore();
		protected virtual ZBool ShouldFilterSupportingDocumentsByMergeKeysCore() => true;
	}
}
