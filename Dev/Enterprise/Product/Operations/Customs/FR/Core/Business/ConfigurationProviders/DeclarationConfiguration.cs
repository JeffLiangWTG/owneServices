using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using JobDeclaration = Enterprise.Customs.FR.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.FR.Business
{
	public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
	{
		protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

		protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

		protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

		protected override EU.Business.EntryHeaderConfiguration GetNewEntryHeaderConfiguration() => new EntryHeaderConfiguration();

		protected override EU.Business.Declaration.MultiLineAddInfos.IAdditionalInfoValidationDecider GetUCC6ImportAdditionalInfoValidationDecider => new UCC6ImportAdditionalInfoValidationDecider();

		protected override EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new UCC6ImportPreviousDocumentValidationDecider();

		protected override EU.Business.EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();

		protected override ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => ZBool.True;

		protected override ZBool IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore => true;

		protected override ZBool IsUCC6Core(BusinessObject businessObject) => businessObject is JobDeclaration jobDeclaration && jobDeclaration.ApplicationExtender.IsUCC6;

		protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => IsUCC6Core(businessObject);

		protected override ZBool DV1DetailsSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration jobDeclaration && !jobDeclaration.ApplicationExtender.IsUCC6;

		protected override ZBool UseEucdmSupportingDocumentGoodsShipmentCore => false;

		protected override EU.Business.Declaration.IDeclarationValidationDecider GetValidationDeciderCore(BusinessObject businessObject)
			=> businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport
			? new UCC6ImportDeclarationValidationDecider(declaration)
			: null;

		protected override bool ShouldCheckLegalByDeclarantTypeCore => true;

		protected override ZBool UseIDDDocumentCore(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsUCC6AndIsImport;
	}
}
