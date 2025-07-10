using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class DeclarationJobDocAddressValidation : JobDocAddressValidation
	{
		protected DeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration) : base(address)
		{
			this.declaration = declaration;
		}

		protected readonly JobDeclaration declaration;

		protected virtual void ValidateImporterDocumentaryOrganisationPKIsRequired()
		{
			if (Parent.OrganisationPK.IsEmpty)
			{
				Parent.OrganisationPKInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("1B6C50AC-1957-4986-A02C-84BD1BB998BC", "Importer/Consignee")));
			}
		}
	}
}
