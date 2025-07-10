using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class CommunicationProvider : IIdType
	{
		public static CommunicationProvider New(OrgContact orgContact, ZString overridePhone) => orgContact == null && overridePhone.IsEmpty ? null : new CommunicationProvider(orgContact, overridePhone);

		CommunicationProvider(OrgContact contact, ZString overridePhone)
		{
			this.contact = contact;
			this.overridePhone = overridePhone;
		}
		readonly OrgContact contact;
		readonly ZString overridePhone;

		public string Id
		{
			get
			{
				if (!overridePhone.IsEmpty)
				{
					return overridePhone;
				}
				else if (!contact.OC_Email.IsEmpty)
				{
					return contact.OC_Email;
				}
				else if (!contact.OC_Phone.IsEmpty)
				{
					return contact.OC_Phone;
				}
				return string.Empty;
			}
		}

		public string Type
		{
			get
			{
				if (!overridePhone.IsEmpty)
				{
					return PhoneCode;
				}
				else if (!contact.OC_Email.IsEmpty)
				{
					return EmailCode;
				}
				else if (!contact.OC_Phone.IsEmpty)
				{
					return PhoneCode;
				}
				return string.Empty;
			}
		}

		const string PhoneCode = "TE";
		const string EmailCode = "EM";
	}
}
