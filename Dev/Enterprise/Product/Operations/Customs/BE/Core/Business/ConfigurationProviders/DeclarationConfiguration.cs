using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.BE.Business;

public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
{
	protected override ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => true;

	protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

	protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

	protected override ZBool IsUCC6Core(BusinessObject businessObject) => true;

	protected override ZBool DV1DetailsSupportCore(BusinessObject businessObject) => (businessObject as ICanBeImportOrExport)?.IsImport ?? false;

	protected override ZBool MiscAdditionalInfosSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	protected override ZBool MiscPreviousDocumentsSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	protected override ZBool MiscSupportingDocumentsSupportCore(BusinessObject businessObject) => !IsUCC6(businessObject);

	protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration;

	protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

	protected override ZBool UseEucdmSupportingDocumentGoodsShipmentAndItemCore => true;
}
