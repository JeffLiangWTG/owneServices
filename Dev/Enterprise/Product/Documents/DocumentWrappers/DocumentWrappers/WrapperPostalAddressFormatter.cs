using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class WrapperPostalAddressFormatter : AddressFormatter
	{
		public WrapperPostalAddressFormatter(DocAddress address, DocCompany sender)
			: base(address.Factory, (OrgHeader)null, null, true)
		{
			this.Address = address;
			this.Sender = sender;
		}

		public WrapperPostalAddressFormatter(DocAddress address, DocCompany sender, bool includeCountryEvenIfSame)
			: base(address.Factory, ((OrgAddress)address.WrappedObject).Header, (GlbCompany)sender.WrappedObject, includeCountryEvenIfSame)
		{
			this.Address = address;
			this.Sender = sender;
		}

		public WrapperPostalAddressFormatter(DocDeliveryContact recipient, DocCompany sender)
			: base(recipient.Factory, (OrgHeader)null, null, true)
		{
			this.Contact = recipient;
			this.Sender = sender;
		}

		public WrapperPostalAddressFormatter(DocOrganisation recipient, DocCompany sender)
			: base(recipient.Factory, (OrgHeader)null, null, true)
		{
			this.Organisation = recipient;
			this.Sender = sender;
		}

		public WrapperPostalAddressFormatter(DocOrganisation recipient, DocCompany sender, bool includeCountryEvenIfSame)
			: base(recipient.Factory, (OrgHeader)((OrgHeaderSource)recipient.WrappedObject).WrappedObject, (GlbCompany)sender.WrappedObject, includeCountryEvenIfSame)
		{
			this.Organisation = recipient;
			this.Sender = sender;
		}

		public WrapperPostalAddressFormatter(DocDocAddress recipient, DocCompany sender)
			: base(recipient.Factory, (OrgHeader)null, null, true)
		{
			this.Anyone = recipient;
			this.Sender = sender;
		}

		public WrapperPostalAddressFormatter(DocDocAddress recipient, DocCompany sender, bool includeCountryEvenIfSame)
			: base(recipient.Factory, GetOrganisation(recipient), (GlbCompany)sender.WrappedObject, includeCountryEvenIfSame)
		{
			this.Anyone = recipient;
			this.Sender = sender;
		}

		static OrgHeader GetOrganisation(DocDocAddress recipient)
		{
			OrgHeader result = null;
			if (recipient.Organisation != null)
			{
				var wrappedObject = ((DocBaseWrapper)recipient.Organisation.WrappedObject).WrappedObject;
				result = wrappedObject as OrgHeader;
				if (result == null)
				{
					var address = wrappedObject as JobDocAddress;
					result = address != null ? address.Organisation : null;
				}
			}
			return result;
		}

		protected DocDocAddress Anyone;
		protected DocAddress Address;
		protected DocOrganisation Organisation;
		protected DocDeliveryContact Contact;
		protected DocCompany Sender;

		protected override void RetrieveOrganisationDetails()
		{
			if (Organisation != null)
			{
				SetPostalAddressDetails(Organisation.Name, Organisation.AdditionalAddressInformation, Organisation.Address1, Organisation.Address2, Organisation.City, Organisation.State, Organisation.PostCode, Organisation.Country);
			}
			else if (Address != null)
			{
				SetPostalAddressDetails(Address.CompanyName, Address.Address1, Address.Address2, Address.City, Address.State, Address.PostCode, Address.Country);
			}
			else if (Anyone != null && !((JobDocAddress)(Anyone.WrappedObject)).IsDeleted)
			{
				SetPostalAddressDetails(Anyone.CompanyName, Anyone.Address1, Anyone.Address2, Anyone.City, Anyone.State, Anyone.PostCode, Anyone.Country);
			}
			else if (Contact != null)
			{
				ZString contactAddress1 = ZString.Empty;
				ZString contactAddress2 = ZString.Empty;
				ZString contactCity = ZString.Empty;
				ZString contactState = ZString.Empty;
				ZString contactPostCode = ZString.Empty;
				ZString contactName = Contact.Name + "\n" + Contact.CompanyName;
				DocCountry country = null;
				contactAddress1 = Contact.Address1;
				contactAddress2 = Contact.Address2;
				contactCity = Contact.City;
				contactState = Contact.State;
				contactPostCode = Contact.PostCode;

				var mostRelevantPostalCountry = Contact.MostRelevantPostalCountry;
				if (mostRelevantPostalCountry != null)
				{
					country = DocCountry.New(Contact.MostRelevantPostalCountry, Contact.Factory);
				}
				else if (Contact.UNLOCO != null)
				{
					country = DocCountry.New(Contact.UNLOCO.Country, Contact.Factory);
				}

				SetPostalAddressDetails(contactName, contactAddress1, contactAddress2, contactCity, contactState, contactPostCode, country);
			}
		}

		protected void SetPostalAddressDetails(ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, DocCountry country)
		{
			base.Name = name;
			base.Address1 = address1;
			base.Address2 = address2;
			base.City = city;
			base.State = state;
			base.PostCode = postCode;
			if (country != null)
			{
				base.CountryName = EnglishOnly ? (NoResString)country.Name.GetUnresolvedString() : country.Name;
				base.CountryCode = country.Code;
			}
		}

		protected void SetPostalAddressDetails(ZString name, ZString additionalAddressInformation, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, DocCountry country)
		{
			SetPostalAddressDetails(name, address1, address2, city, state, postCode, country);
			base.AdditionalAddressInformation = additionalAddressInformation;
		}

		protected override void RetrieveCompanyDetails()
		{
			SenderCountry = Sender != null && Sender.Country != null ? Sender.Country.Name : ZString.Empty;
		}

		public bool EnglishOnly { get; set; }
	}
}
