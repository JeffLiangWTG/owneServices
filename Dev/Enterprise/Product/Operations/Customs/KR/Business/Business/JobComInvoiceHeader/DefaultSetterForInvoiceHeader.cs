using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(JobComInvoiceHeader newElement, JobDeclaration declaration)
			: base(newElement, declaration)
		{
		}

		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;
		protected new JobComInvoiceHeader newElement => (JobComInvoiceHeader)base.newElement;

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();

			if (declaration.JE_SimpleDRWApp == ApplicationForSimpleDrawbackCodeList.Codes.AD)
			{
				newElement.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Manufacturer;
			}
		}
		protected override void SetDefaultsForFirstInvoiceCore()
		{
			base.SetDefaultsForFirstInvoiceCore();
			if (declaration.JE_OH_Importer.IsValid && !declaration.IsImport)
			{
				newElement.JZ_OH_Buyer = declaration.JE_OH_Importer;
			}

			if (declaration.JE_OH_Manufacturer.IsValid)
			{
				newElement.JZ_OH_Manufacturer = declaration.JE_OH_Manufacturer;
			}

			if (declaration.JE_OA_ManufacturerAddress.IsValid)
			{
				newElement.JZ_OA_ManufacturerAddress = declaration.JE_OA_ManufacturerAddress;
			}

			if (declaration.JE_OH_Supplier.IsValid)
			{
				newElement.SetDefaultSupplierFromDeclaration();
			}
		}

		protected override void SetDefaultsForAdditionalInvoiceCore(BaseJobComInvoiceHeader previousInvoice)
		{
			base.SetDefaultsForAdditionalInvoiceCore(previousInvoice);

			if (previousInvoice.JZ_OH_Buyer.IsValid)
			{
				newElement.JZ_OH_Buyer = previousInvoice.JZ_OH_Buyer;
			}

			if (previousInvoice.JZ_OH_Manufacturer.IsValid)
			{
				newElement.JZ_OH_Manufacturer = previousInvoice.JZ_OH_Manufacturer;
			}

			if (previousInvoice.JZ_OA_ManufacturerAddress.IsValid)
			{
				newElement.JZ_OA_ManufacturerAddress = previousInvoice.JZ_OA_ManufacturerAddress;
			}

			if (previousInvoice.JZ_OH_Supplier.IsValid)
			{
				newElement.JZ_OH_Supplier = previousInvoice.JZ_OH_Supplier;
			}

			if (previousInvoice.JZ_OA_SupplierAddress.IsValid)
			{
				newElement.JZ_OA_SupplierAddress = previousInvoice.JZ_OA_SupplierAddress;
			}
		}
	}
}
