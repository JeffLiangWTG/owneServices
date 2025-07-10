using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.DeclarationH7;

public class H7RepresentativeWrapper : PartyIdWrapper, IH7Representative
{
	public H7RepresentativeWrapper(AsycudaBill bill) : base(bill.Header.Representative?.Header)
	{
		this.bill = bill;
		contactPerson = orgHeader?.Contacts.GetContactForAllocation(OrgConstants.ContactAllocationType.CUS);
	}

	protected override ZString IdCore => bill.Header.IsAgentTypeINDOrDCA ? null : orgHeader.GetEOROrNIFCode();

	public IPartyContactProvider ContactInfo
	{
		get
		{
			return contactPerson == null || bill.Header.IsAgentTypeINDOrDCA
				? null
				: PartyContactWrapper.New(contactPerson.OC_ContactName, contactPerson.OC_Email, contactPerson.OC_Mobile);
		}
	}

	public ZInt Status
	{
		get
		{
			ZInt result = bill.Header.AMA_AgentType.ToString() switch
			{
				EUH7AgentTypes.Codes.DIR => 2,
				EUH7AgentTypes.Codes.IND => 3,
				ESH7AgentTypes.Codes.DCA => 4,
				ESH7AgentTypes.Codes.ICA => 5,
				_ => 0
			};

			return result;
		}
	}

	readonly AsycudaBill bill;
	readonly OrgContact contactPerson;
}
