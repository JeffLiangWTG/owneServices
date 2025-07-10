using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.ExitControl.Business;

public class EALAESDeclarantWrapperWithContactPerson : PartyIdWrapper, IPartyIdProviderWithContactPerson
{
	public static EALAESDeclarantWrapperWithContactPerson New(CusExitHeader exitHeader)
	{
		var orgAddress = exitHeader?.Carrier;
		return orgAddress?.Header == null ? null : new EALAESDeclarantWrapperWithContactPerson(orgAddress);
	}

	protected EALAESDeclarantWrapperWithContactPerson(OrgAddress orgA) : base(orgA?.Header)
	{
		orgAddress = orgA;
	}
	readonly OrgAddress orgAddress;

	public IPartyContactProvider ContactPerson => contactPerson ?? (contactPerson = PartyContactWrapper.New(orgAddress));
	PartyContactWrapper contactPerson;
}
