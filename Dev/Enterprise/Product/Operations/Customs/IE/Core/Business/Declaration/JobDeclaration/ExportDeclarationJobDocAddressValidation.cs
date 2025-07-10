using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportDeclarationJobDocAddressValidation : DeclarationJobDocAddressValidation
	{
		public ExportDeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
			: base(address, declaration)
		{
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					ValidateOrganisationPKSupplierDocumentary();
					break;
				case DocAddressTypes.Codes.ImporterDocumentaryAddress:
					ValidateImporterDocumentaryOrganisationPKIsRequired();
					break;
			}
		}

		void ValidateOrganisationPKSupplierDocumentary()
		{
			var targetInfo = Parent.OrganisationPKInfo;
			if (Parent.OrganisationPK.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("9143A911-30D8-4BFD-B6E4-EA156647C3A1", "Supplier/Exporter is required for Export Declarations."));
			}

			if (declaration.IsAmendmentValidationMode && !declaration.OriginalSupplier.IsEmpty && declaration.OriginalSupplier != declaration.JE_OH_Supplier)
			{
				targetInfo.AddMessageError(CommonResStrings.ShouldNotAmendThisValue);
			}

			if (Parent.Organisation is OrgHeader supplier && string.IsNullOrEmpty(supplier.GetEORI()))
			{
				targetInfo.AddMessageError(Res.GetString("F6D3E892-DD96-424B-8C1E-FD688C6BD2B4", "Supplier/Exporter's EORI is missing"));
			}
		}
	}
}
