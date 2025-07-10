using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC060CRepresentativeProvider
	{
		public CC060CRepresentativeProvider(RepresentativeType04 representativeType04)
		{
			this.representativeType = Argument.NotNull(representativeType04, nameof(representativeType04));
		}
		readonly RepresentativeType04 representativeType;

		public ZString Id => representativeType.IdentificationNumber;

		public ContactProvider Contact
		{
			get
			{
				var xmlContact = representativeType.ContactPerson;
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
