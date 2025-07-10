using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ImportH1CommonRepresentativeWrapperWithContactPerson : CommonRepresentativeWrapperWithContactPerson
{
	public static new ImportH1CommonRepresentativeWrapperWithContactPerson New(JobDeclaration jobDeclaration)
	{
		var orgAddress = GetRepresentative(jobDeclaration);
		return orgAddress == null ? null : new ImportH1CommonRepresentativeWrapperWithContactPerson(jobDeclaration, orgAddress);
	}

	ImportH1CommonRepresentativeWrapperWithContactPerson(JobDeclaration jobDeclaration, OrgAddress orgA) : base(jobDeclaration, orgA)
	{
	}

	protected override ZString ContactEmail => GetContactEmailForImportH1(declaration);

	protected override ZString PhoneNumber => orgAddress.OA_Phone;
}
