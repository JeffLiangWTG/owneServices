using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSCommonPersonReqPresWrapper : PartyIdWrapper, IT2LPOUSCommonPersonReqPres
	{
		public static T2LPOUSCommonPersonReqPresWrapper New(OrgAddress address, ZString contactEmail) => address?.Header == null ? null : new T2LPOUSCommonPersonReqPresWrapper(address, contactEmail);

		protected T2LPOUSCommonPersonReqPresWrapper(OrgAddress orgA, ZString contactEmail) : base(orgA?.Header)
		{
			orgAddress = orgA;
			this.contactEmail = contactEmail;
		}
		protected readonly OrgAddress orgAddress;
		readonly ZString contactEmail;

		public IPartyContactProvider ContactPerson => ContactPersonCore;
		
		protected virtual IPartyContactProvider ContactPersonCore => contactPerson ?? (contactPerson = PartyContactWrapper.New(orgHeader?.OH_FullName ?? ZString.Empty, contactEmail, GetPhoneFromAddress(orgAddress)));
		PartyContactWrapper contactPerson;
	}
}
