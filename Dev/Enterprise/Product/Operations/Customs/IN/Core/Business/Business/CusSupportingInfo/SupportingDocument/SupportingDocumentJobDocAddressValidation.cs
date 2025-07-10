using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business;

public sealed class SupportingDocumentJobDocAddressValidation : JobDocAddressValidation
{
	public SupportingDocumentJobDocAddressValidation(JobDocAddress parent)
		: base(parent)
	{
	}

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();
		var description = Res.GetString("444C2A31-CED9-4836-866D-9132C712601B", "Document Issuing Party/Organization");
		MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, description);
	}
}

