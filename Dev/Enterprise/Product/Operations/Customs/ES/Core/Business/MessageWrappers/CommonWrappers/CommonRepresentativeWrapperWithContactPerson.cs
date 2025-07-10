using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonRepresentativeWrapperWithContactPerson : CommonRepresentativeWrapper, ICommonRepresentativeWithContactPerson
{
	public static new CommonRepresentativeWrapperWithContactPerson New(JobDeclaration jobDeclaration)
	{
		var orgAddress = GetRepresentative(jobDeclaration);
		return orgAddress == null ? null : new CommonRepresentativeWrapperWithContactPerson(jobDeclaration, orgAddress);
	}

	protected CommonRepresentativeWrapperWithContactPerson(JobDeclaration jobDeclaration, OrgAddress orgA) : base(orgA)
	{
		declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		orgAddress = orgA;
	}
	protected readonly JobDeclaration declaration;
	protected readonly OrgAddress orgAddress;

	public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = orgAddress?.Header != null
		? PartyContactWrapper.New(orgAddress.Header.OH_FullName, ContactEmail, PhoneNumber)
		: null);
	PartyContactWrapper contactPerson;

	protected virtual ZString ContactEmail => GetEmailFromAddress(orgAddress, declaration);

	protected virtual ZString PhoneNumber => GetPhoneFromAddress(orgAddress);
}
