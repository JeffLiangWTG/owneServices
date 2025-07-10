using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSCommonPersonReqPresWithAddressWrapper : T2LPOUSCommonPersonReqPresWrapper, IT2LPOUSCommonPersonReqPresWithAddress
	{
		public static T2LPOUSCommonPersonReqPresWithAddressWrapper New(OrgAddress address, ZString contactEmail, ZBool isRepresentativeDeclared) => address?.Header == null ? null : new T2LPOUSCommonPersonReqPresWithAddressWrapper(address, contactEmail, isRepresentativeDeclared);

		T2LPOUSCommonPersonReqPresWithAddressWrapper(OrgAddress orgA, ZString contactEmail, ZBool isRepresentativeDeclared) : base(orgA, contactEmail)
		{
			this.isRepresentativeDeclared = isRepresentativeDeclared;
		}
		readonly ZBool isRepresentativeDeclared;

		public IPartyAddressProvider Address => address ??= GetAddressForNaturalPerson(orgAddress, PartyAddressWrapper.New(orgAddress));
		PartyAddressWrapper address;

		protected override ZString IdCore
		{
			get
			{
				var (id, isNaturalPersonIndividual) = GetIdForNaturalPerson();
				return isNaturalPersonIndividual ? id : base.IdCore;
			}
		}

		protected override IPartyContactProvider ContactPersonCore => contactPerson ?? (contactPerson = (PartyContactWrapper)(isRepresentativeDeclared ? null : base.ContactPersonCore));
		PartyContactWrapper contactPerson;
	}
}
