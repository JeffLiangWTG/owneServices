using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC060CHolderOfTheTransitProcedureProvider
	{
		public CC060CHolderOfTheTransitProcedureProvider(HolderOfTheTransitProcedureType13 holderOfTheTransitProcedureType)
		{
			this.holderOfTheTransitProcedure = Argument.NotNull(holderOfTheTransitProcedureType, nameof(holderOfTheTransitProcedureType));
		}
		readonly HolderOfTheTransitProcedureType13 holderOfTheTransitProcedure;

		public ZString Id => holderOfTheTransitProcedure.IdentificationNumber;

		public ZString Name => holderOfTheTransitProcedure.Name;

		public AddressProvider Address
		{
			get
			{
				var xmlAddress = holderOfTheTransitProcedure.Address;
				return xmlAddress == null ? null : addressCached ?? (addressCached = new AddressProvider
				{
					StreetAndNumber = xmlAddress.StreetAndNumber,
					Postcode = xmlAddress.Postcode,
					City = xmlAddress.City,
					Country = xmlAddress.Country,
				});
			}
		}
		AddressProvider addressCached;

		public ContactProvider Contact
		{
			get
			{
				var xmlContact = holderOfTheTransitProcedure.ContactPerson;
				return xmlContact == null ? null : contactCached ?? (contactCached = new ContactProvider
				{
					Name = xmlContact.Name,
					PhoneNumber = xmlContact.PhoneNumber,
					EmailAddress = xmlContact.EMailAddress,
				});
			}
		}
		ContactProvider contactCached;
	}
}
