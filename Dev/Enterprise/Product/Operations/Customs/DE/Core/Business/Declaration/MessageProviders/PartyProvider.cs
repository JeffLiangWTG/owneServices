using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class PartyProvider : PartyIDProvider, IAESParty
	{
		public static PartyProvider NewOrNull(OrgAddress address, OrgContact contact) => address != null ? new PartyProvider(address, contact) : null;

		public static PartyProvider NewOrNull(OrgAddress address, GlbStaff staff) => address != null ? new PartyProvider(address, staff) : null;

		public new static PartyProvider NewOrNull(OrgAddress address) => NewOrNull(address, contact: null);

		PartyProvider(OrgAddress address, OrgContact contact) : base(address)
		{
			ContactPerson = PartyContactPersonProvider.NewOrNull(contact);
		}

		PartyProvider(OrgAddress address, GlbStaff staff) : base(address)
		{
			ContactPerson = PartyContactPersonGlbStaffProvider.NewOrNull(staff);
		}

		public string Type => null;

		public string Name => CachedValueHelper.GetValue(ref name, () =>
		{
			var result = address.OA_CompanyNameOverride;
			if (result.IsEmpty)
			{
				result = (CargoWise.Types.ZString)(org?.OH_FullName);
			}
			return result;
		});
		CachedValue<string> name;

		public string Address => address.Address1;

		public string City => address.City;

		public string Postcode => address.Postcode;

		public string Country => address.OA_RN_NKCountryCode;

		public string AdditionalAddressInfo => address.OA_AdditionalAddressInformation;

		public IAESPartyContactPerson ContactPerson { get; }

		public string Address2 => address.Address2;
	}
}
