namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Messaging;

	public interface ISupplementaryCargoReport : ICAEDIFACTMessageAttachee
	{
		// BGM
		ZString DocumentMessageNumber { get; set; }

		//CST
		ZString ServiceOption { get; }

		// G04
		// TDT
		ZString ModeOfTransport { get; }
		ZString CarrierCode { get; }

		// G07
		// DOC
		ZString OriginalCargoControlNumber { get; }
		ZString UniqueConsignmentReference { get; }

		// G08
		// RFF
		ZString SupplementaryReferenceNumber { get; set; }
		// LOC
		ZString DestinationCountryCode { get; }
		ZString DestinationCityName { get; }
		ZString DestinationPortName { get; }
		// GEI
		ZString CustomsProcedureCode { get; }
		// FTX
		ZString SpecialInstructions { get; }

		// G09
		// G10
		// RFF
		ZString BillOfLading { get; }

		// G11/G12
		IAddressForACI Consignee { get; }
		IAddressForACI Consignor { get; }
		IAddressForACI DeliveryParty { get; }
		IAddressForACI NotifyParty { get; }

		// G14
		IEnumerable<ISCRContainer> Containers { get; }

		// G15
		IEnumerable<ISCRLine> GoodsLines { get; }

		// AUT
		ZString Authentication { get; }
	}

	public interface IAddressForACI
	{
		ZString Name { get; }
		ZString Address1 { get; }
		ZString Address2 { get; }
		ZString City { get; }
		ZString State { get; }
		ZString PostCode { get; }
		ZString CountryCode { get; }
		ZString Phone { get; }
		ZString Contact { get; }
	}

	public class AddressForACI : IAddressForACI
	{
		public AddressForACI(ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString phone, ZString contact)
		{
			this.name = name;
			this.address1 = address1;
			this.address2 = address2;
			this.city = city;
			this.state = state;
			this.postCode = postCode;
			this.countryCode = countryCode;
			this.phone = phone;
			this.contact = contact;
		}
		readonly ZString name;
		readonly ZString address1;
		readonly ZString address2;
		readonly ZString city;
		readonly ZString state;
		readonly ZString postCode;
		readonly ZString countryCode;
		readonly ZString phone;
		readonly ZString contact;

		ZString IAddressForACI.Name
		{
			get { return name; }
		}

		ZString IAddressForACI.Address1
		{
			get { return address1; }
		}

		ZString IAddressForACI.Address2
		{
			get { return address2; }
		}

		ZString IAddressForACI.City
		{
			get { return city; }
		}

		ZString IAddressForACI.State
		{
			get { return state; }
		}

		ZString IAddressForACI.PostCode
		{
			get { return postCode; }
		}

		ZString IAddressForACI.CountryCode
		{
			get { return countryCode; }
		}

		ZString IAddressForACI.Contact
		{
			get { return contact; }
		}

		ZString IAddressForACI.Phone
		{
			get { return phone; }
		}
	}
}
