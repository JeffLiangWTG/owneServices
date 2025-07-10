using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class DeclarationConfiguration : EU.Business.DeclarationConfiguration
{
	protected override ZBool MiscAdditionalInfosSupportCore(BusinessObject businessObject) => false;

	protected override ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => true;

	protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

	protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

	protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

	protected override EU.Business.EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();

	protected override ZBool IsUCC6Core(BusinessObject businessObject) => (businessObject is JobDeclaration jobDeclaration) && (jobDeclaration.IsImport || (jobDeclaration.IsExport && jobDeclaration.MessageVersion == MessageVersionList.Codes.XML));

	protected override ZBool IsTransitionPeriodAES30Core(BusinessObject businessObject)
	{
		return !IsExportTxt() && base.IsTransitionPeriodAES30Core(businessObject);

		bool IsExportTxt()
		{
			return businessObject is JobDeclaration declaration
				&& declaration.IsExport
				&& declaration.MessageVersion == MessageVersionList.Codes.TXT;
		}
	}

	protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsExport;

	protected override ZBool MiscPreviousDocumentsSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	protected override ZBool MiscSupportingDocumentsSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	public ZBool IsImportMessageVersionUCC6(BusinessObject businessObject) => IsImportMessageVersionUCC6Core(businessObject);

	bool IsImportMessageVersionUCC6Core(BusinessObject businessObject)
	{
		if (!ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(Universal.Constants.FunctionalityTypes.ImportMessageVersionUCC6, priorityToPilotFunctionality: true))
		{
			return true;
		}

		return businessObject is JobDeclaration declaration
			&& declaration.IsImport
			&& EU.Business.FuncsHelper.IsFunctionalityValid(
				Universal.Constants.FunctionalityTypes.ImportMessageVersionUCC6,
				dataGroupingCode: GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				options: EU.Business.FuncsHelper.ValidationOptions.UseSpecificDataGroupingOnly,
				priorityToPilotFunctionality: true);
	}

	protected override ZBool UseEucdmSupportingDocumentGoodsShipmentCore => false;

	protected override EU.Business.Declaration.IDeclarationValidationDecider GetValidationDeciderCore(BusinessObject businessObject)
		=> businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport
		? new UCC6ImportDeclarationValidationDecider()
		: null;

	protected override EU.Business.Declaration.IPackageValidationDecider UCC6ImportPackageValidationDecider
		=> new UCC6ImportPackageValidationDecider();
}
