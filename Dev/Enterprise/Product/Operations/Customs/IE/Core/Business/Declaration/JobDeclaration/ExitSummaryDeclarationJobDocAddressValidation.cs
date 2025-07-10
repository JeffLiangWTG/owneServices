using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExitSummaryDeclarationJobDocAddressValidation : DeclarationJobDocAddressValidation
	{
		public ExitSummaryDeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
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
			if (!Parent.OrganisationPK.IsEmpty && (Parent.Organisation?.GetEuIdentificationNumber().IsEmpty ?? true))
			{
				Parent.OrganisationPKInfo.AddMessageError(Res.GetString("E393379B-B2E8-4F57-B30F-2B74AB164159", "An EORI number is required for Exit Summary Declarations."));
			}
		}
	}
}
