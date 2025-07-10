using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
	{
		protected override ZBool UCCAdditionalInfosSupportCore(BusinessObject businessObject) => IsUCC6(businessObject);

		protected override ZBool UseUniversalFeeCalculationCore(BusinessObject businessObject) => true;

		protected override ZBool MiscPreviousDocumentsSupportCore(BusinessObject businessObject) => false;

		protected override ZBool MiscGuaranteesSupportCore(BusinessObject businessObject) => !(businessObject is IImportExport importExport) || importExport.IsImport();

		protected override ZBool LockNumberOfEntryLinesForRegisteredEntryCore => true;

		protected override EU.Business.EntryLineConfiguration GetNewEntryLineConfiguration() => new EntryLineConfiguration();

		protected override EU.Business.InvoiceHeaderConfiguration GetNewInvoiceHeaderConfiguration() => new InvoiceHeaderConfiguration();

		protected override EU.Business.InvoiceLineConfiguration GetNewInvoiceLineConfiguration() => new InvoiceLineConfiguration();

		protected override EU.Business.InstructionConfiguration GetNewInstructionConfiguration() => new InstructionConfiguration();

		protected override ZBool IsUCC6Core(BusinessObject businessObject)
		{
			var importExportBO = businessObject as IImportExport;

			return (importExportBO is null && (HasAnyVersionCodeAes() || HasImportUCC6Functionality()))
					|| ((importExportBO?.IsExport() ?? false) && HasAnyVersionCodeAes())
					|| ((importExportBO?.IsImport() ?? false) && (HasImportUCC6Functionality() || HasVersionCodeH1()));

			bool HasAnyVersionCodeAes() => MessageVersionRegistryProvider.IsExportAndAnyVersionAes();
			bool HasVersionCodeH1() => MessageVersionRegistryProvider.IsImportVersionH1();
		}

		protected override ZBool IsTransitionPeriodAES30Core(BusinessObject businessObject)
		{
			return base.IsTransitionPeriodAES30Core(businessObject) || IsExportAES();

			bool IsExportAES()
			{
				return businessObject is JobDeclaration declaration
					&& declaration.IsExport
					&& MessageVersionRegistryProvider.IsExportVersionAes();
			}
		}

		protected override ZBool UseEucdmSupportingDocumentGoodsShipmentCore => false;

		public static bool HasImportUCC6Functionality() =>
			EU.Business.FuncsHelper.IsFunctionalityValid(
				Constants.FunctionalityTypes.ImportMessageVersionUCC6,
				dataGroupingCode: GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
	}
}
