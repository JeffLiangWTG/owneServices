using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class PartyProvider : IParty
	{
		public static PartyProvider New(JobDocAddress docAddress, FallBackStyle fallBack = FallBackStyle.None)
		{
			PartyProvider result = null;
			if (docAddress != null)
			{
				result = docAddress.E2_AddressOverride
					? new PartyProvider(docAddress, docAddress.E2_GovRegNum, new ContactProvider(docAddress.E2_Contact, docAddress.E2_Phone, docAddress.E2_Email))
					: New(docAddress.Address, fallBack);
			}
			return result;
		}

		public static PartyProvider New(OrgAddress orgAddress, FallBackStyle fallBack = FallBackStyle.None)
		{
			PartyProvider result = null;
			if (orgAddress != null)
			{
				var header = orgAddress.Header;
				result = new PartyProvider(orgAddress, GetRegNo(orgAddress, fallBack), ContactProvider.New(GetContact(header)));
			}
			return result;
		}

		protected static OrgContact GetContact(OrgHeader header)
		{
			OrgContact result = null;
			if (header != null)
			{
				result = header.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				if (result == null && header.Contacts.Count == 1)
				{
					result = header.Contacts[0];
				}
			}
			return result;
		}

		protected static ZString GetRegNo(OrgAddress orgAddress, FallBackStyle fallBack = FallBackStyle.None)
		{
			var result = ZString.Empty;
			var header = orgAddress.Header;
			if (header != null)
			{
				var eori = fallBack == FallBackStyle.EORIOnly
					? orgAddress.GetEORI(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true)
					: orgAddress.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true);
				if (!eori.IsEmpty)
				{
					result = eori;
				}
				else if (fallBack == FallBackStyle.Consignor)
				{
					result = MessageProviderHelper.GetPartyRegNo(orgAddress);
				}
			}
			return result;
		}

		protected PartyProvider(IDocAddress docAddress, string id, IContact contact)
		{
			this.docAddress = docAddress;
			this.id = id;
			this.Contact = contact;
		}
		readonly IDocAddress docAddress;

		public string Id => id;
		readonly string id;

		public string Name => docAddress.E2_CompanyName;

		public IAddress Address => address ?? (address = AddressProvider.New(docAddress));
		IAddress address;

		public IContact Contact { get; private set; }

		public enum FallBackStyle
		{
			None,
			Consignor,
			EORIOnly
		}
	}
}
