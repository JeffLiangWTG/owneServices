using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Business
{
	public class InvoiceLineConfiguration
	{
		public ZBool SecondQuotaVisible(BusinessObject businessObject) => SecondQuotaVisibleCore(businessObject);
		protected virtual ZBool SecondQuotaVisibleCore(BusinessObject businessObject) => false;

		public ZBool MethodOfPaymentVisibleOnImportControl(BusinessObject businessObject) => MethodOfPaymentVisibleOnImportControlCore(businessObject);
		protected virtual ZBool MethodOfPaymentVisibleOnImportControlCore(BusinessObject businessObject) => false;

		public ZBool AdditionalInfosSupport(BusinessObject businessObject) => !IsProductClassificationBoth(businessObject) && AdditionalInfosSupportCore(businessObject);
		protected virtual ZBool AdditionalInfosSupportCore(BusinessObject businessObject) => true;

		public ZBool SupportingDocumentsSupport(BusinessObject businessObject) => !IsProductClassificationBoth(businessObject) && SupportingDocumentsSupportCore(businessObject);
		protected virtual ZBool SupportingDocumentsSupportCore(BusinessObject businessObject) => true;

		public ZBool PreviousDocumentsSupport(BusinessObject businessObject) => !IsProductClassificationBoth(businessObject) && PreviousDocumentsSupportCore(businessObject);
		protected virtual ZBool PreviousDocumentsSupportCore(BusinessObject businessObject) => true;

		public ZBool TaxSupport(BusinessObject businessObject) => !IsProductClassificationBoth(businessObject) && TaxSupportCore(businessObject);
		protected virtual ZBool TaxSupportCore(BusinessObject businessObject) => false;

		static bool IsProductClassificationBoth(BusinessObject businessObject) => businessObject is CusClassPartPivot partPivot && partPivot.IsClassificationBoth;

		public ZBool CountryOfDestinationVisibleOnImportControl(BusinessObject businessObject) => CountryOfDestinationVisibleOnImportControlCore(businessObject);
		protected virtual ZBool CountryOfDestinationVisibleOnImportControlCore(BusinessObject businessObject) => false;

		public ZBool CountryOfDestinationVisibleOnExportControl(BusinessObject businessObject) => CountryOfDestinationVisibleOnExportControlCore(businessObject);
		protected virtual ZBool CountryOfDestinationVisibleOnExportControlCore(BusinessObject businessObject) => true;

		public ZBool FiscalReferencesSupport(BusinessObject businessObject) => FiscalReferencesSupportCore(businessObject);
		protected virtual ZBool FiscalReferencesSupportCore(BusinessObject businessObject)
		{
			var declaration = (JobDeclaration)businessObject;
			return declaration.IsUCC6AndIsImport;
		}

		public ZBool DefaultCountryOfSupplyFromSupplier(BusinessObject businessObject) => DefaultCountryOfSupplyFromSupplierCore(businessObject);
		protected virtual ZBool DefaultCountryOfSupplyFromSupplierCore(BusinessObject businessObject) => false;

		public ZBool CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(BusinessObject businessObject) => CountryOfSupplyMustBeTheSameForAllLinesOnInstructionCore(businessObject);
		protected virtual ZBool CountryOfSupplyMustBeTheSameForAllLinesOnInstructionCore(BusinessObject businessObject) => false;

		public ZBool VehicleSupport(BusinessObject businessObject) => VehicleSupportCore(businessObject);
		protected virtual ZBool VehicleSupportCore(BusinessObject businessObject) => false;

		public ZBool AuthorisationsSupportForInvoiceLine(JobDeclaration declaration) => AuthorisationsSupportForInvoiceLineCore(declaration);
		protected virtual ZBool AuthorisationsSupportForInvoiceLineCore(JobDeclaration declaration) => declaration.Configuration.IsUCC6(declaration);

		public ZBool OrganizationsSupport(BusinessObject businessObject) => OrganizationsSupportCore(businessObject);
		protected virtual ZBool OrganizationsSupportCore(BusinessObject businessObject) => businessObject is JobDeclaration declaration && declaration.IsUCC6;

		public ZBool ValueIndicatorsSupport(BusinessObject businessObject) => ValueIndicatorsSupportCore(businessObject);
		protected virtual ZBool ValueIndicatorsSupportCore(BusinessObject businessObject)
		{
			var declaration = (JobDeclaration)businessObject;
			return declaration.IsUCC6AndIsImport;
		}

		public ZBool InvoiceLinePaymentSupport(BusinessObject businessObject) => InvoiceLinePaymentSupportCore(businessObject);
		protected virtual ZBool InvoiceLinePaymentSupportCore(BusinessObject businessObject) => false;

		public ZBool AdditionalSupplyChainActorSupport(BusinessObject businessObject) => AdditionalSupplyChainActorSupportCore(businessObject);
		protected virtual ZBool AdditionalSupplyChainActorSupportCore(BusinessObject businessObject) => true;

		public ZBool InflateItemPriceByValuationMarkup(BusinessObject businessObject) => InflateItemPriceByValuationMarkupCore(businessObject);
		protected virtual ZBool InflateItemPriceByValuationMarkupCore(BusinessObject businessObject) => false;

		public ZBool UseMultipleVatFields => UseMultipleVatFieldsCore;

		protected virtual ZBool UseMultipleVatFieldsCore => true;

		public IInvoiceLineValidationDecider GetValidationDecider(JobComInvoiceLine invoiceLine) => GetValidationDeciderCore(invoiceLine);

		protected virtual IInvoiceLineValidationDecider GetValidationDeciderCore(JobComInvoiceLine invoiceLine) => invoiceLine.Declaration?.IsUCC6AndIsImport ?? false ? new UCC6ImportInvoiceLineValidationDecider() : (invoiceLine.Declaration?.IsUCC6AndIsExport ?? false ? new UCC6ExportInvoiceLineValidationDecider() : null);

		public ISupportingDocumentValidationDecider GetSupportingDocumentValidationDecider(JobComInvoiceLine invoiceLine) => GetSupportingDocumentValidationDeciderCore(invoiceLine);
		protected virtual ISupportingDocumentValidationDecider GetSupportingDocumentValidationDeciderCore(JobComInvoiceLine invoiceLine) => (invoiceLine as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportSupportingDocumentValidationDecider : null;
		protected virtual ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new UCC6ImportSupportingDocumentValidationDecider();

		public IAdditionalInfoValidationDecider GetAdditionalInfoValidationDecider(JobComInvoiceLine invoiceLine) => GetAdditionalInfoValidationDeciderCore(invoiceLine);
		protected virtual IAdditionalInfoValidationDecider GetAdditionalInfoValidationDeciderCore(JobComInvoiceLine invoiceLine) =>
			invoiceLine is IUcc6ValueProvider ucc6ValueProvider && ucc6ValueProvider.IsUCC6AndIsImport()
			? UCC6ImportAdditionalInfoValidationDecider : null;
		protected virtual IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new UCC6ImportAdditionalInfoValidationDecider();

		public ICusFiscalReferenceValidationDecider GetCusFiscalReferenceValidationDecider(JobComInvoiceLine invoiceLine) => GetCusFiscalReferenceValidationDeciderCore(invoiceLine);
		protected virtual ICusFiscalReferenceValidationDecider GetCusFiscalReferenceValidationDeciderCore(JobComInvoiceLine invoiceLine) => (invoiceLine as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportCusFiscalReferenceValidationDecider : null;
		protected virtual ICusFiscalReferenceValidationDecider UCC6ImportCusFiscalReferenceValidationDecider => new UCC6ImportCusFiscalReferenceValidationDecider();

		public ICusAuthorizationUsageValidationDecider GetCusAuthorizationUsageValidationDecider(JobComInvoiceLine invoiceLine) => GetCusAuthorizationUsageValidationDeciderCore(invoiceLine);
		protected virtual ICusAuthorizationUsageValidationDecider GetCusAuthorizationUsageValidationDeciderCore(JobComInvoiceLine invoiceLine) => (invoiceLine as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportCusAuthorizationUsageValidationDecider : null;
		protected virtual ICusAuthorizationUsageValidationDecider UCC6ImportCusAuthorizationUsageValidationDecider => new UCC6ImportCusAuthorizationUsageValidationDecider();

		public IPreviousDocumentValidationDecider GetPreviousDocumentValidationDecider(JobComInvoiceLine invoiceLine) => GetPreviousDocumentValidationDeciderCore(invoiceLine);
		protected virtual IPreviousDocumentValidationDecider GetPreviousDocumentValidationDeciderCore(JobComInvoiceLine invoiceLine) => (invoiceLine as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportPreviousDocumentValidationDecider : null;
		protected virtual IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new UCC6ImportPreviousDocumentValidationDecider();
	}
}
