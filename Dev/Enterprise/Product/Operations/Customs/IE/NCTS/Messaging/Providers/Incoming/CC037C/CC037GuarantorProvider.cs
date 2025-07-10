using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037GuarantorProvider
	{
		public CC037GuarantorProvider(GuarantorType01 guarantor)
		{
			this.guarantor = Argument.NotNull(guarantor, nameof(guarantor));
		}
		readonly GuarantorType01 guarantor;

		public ZString Id => guarantor.IdentificationNumber;

		public ZString Name => guarantor.Name;

		public AddressProvider Address
		{
			get
			{
				var xmlAddress = guarantor.Address;
				return xmlAddress == null ? null : addressCached ?? (addressCached = new AddressProvider
				{
					StreetAndNumber = xmlAddress.StreetAndNumber,
					Postcode = xmlAddress.Postcode,
					City = xmlAddress.City,
					Country = xmlAddress.Country.ToString().ToUpper(),
				});
			}
		}
		AddressProvider addressCached;

		public ContactProvider Contact
		{
			get
			{
				var xmlContact = guarantor.ContactPerson;
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
