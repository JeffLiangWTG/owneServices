using System.ComponentModel;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class ElementsXsd
	{
		public static class XPath
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
			public static class Organisation
			{
				public const string NodeName = "Organisation";
				public const string Name = "OrganisationDetails/Name";
				public const string Location = "OrganisationDetails/Location";
				public const string AddressLine1 = "OrganisationDetails/Addresses/Address/AddressLine1";
				public const string AddressLine2 = "OrganisationDetails/Addresses/Address/AddressLine2";
				public const string CityOrSuburb = "OrganisationDetails/Addresses/Address/CityOrSuburb";
				public const string StateOrProvince = "OrganisationDetails/Addresses/Address/StateOrProvince";
				public const string PostCode = "OrganisationDetails/Addresses/Address/PostCode";
				public const string TelephoneNumbers = "OrganisationDetails/Addresses/Address/TelephoneNumbers/TelephoneNumber";
				public const string Email = "OrganisationDetails/Addresses/Address/Email";
				public const string WebAddress = "OrganisationDetails/WebAddress";
				public const string RegistrationNumbers = "OrganisationDetails/RegistrationNumbers/RegistrationNumber";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static class RegistrationNumber
		{
			public const string CountryOfRegistration = "CountryOfRegistration";
			public const string NumberType = "NumberType";
			public const string Number = "Number";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static class NumberTypes
		{
			public const string Business = "Business";
			public const string Fax = "Fax";
			public const string Mobile = "Mobile";
		}

		public static class Attributes
		{
			public const string EDICode = "EDICode";
			public const string OwnerCode = "OwnerCode";
			public const string NumberType = "NumberType";
			public const string DimensionType = "DimensionType";
			public const string CurrencyCode = "CurrencyCode";
			public const string ShipmentIdentifierType = "ShipmentIdentifierType";
			public const string ConsolIdentifierType = "ConsolIdentifierType";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static class Port
		{
			public const string NodeName = "Port";
			public const string ActualDateTime = "ActualDateTime";
			public const string EstimatedDateTime = "EstimatedDateTime";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static class OrganisationDetails
		{
			public const string NodeName = "OrganisationDetails";
			public const string Name = "Name";
			public const string Location = "Location";
			public const string Addresses = "Addresses";
			public const string Address = "Address";
			public const string AddressLine1 = "AddressLine1";
			public const string AddressLine2 = "AddressLine2";
			public const string CityOrSuburb = "CityOrSuburb";
			public const string StateOrProvince = "StateOrProvince";
			public const string PostCode = "PostCode";
			public const string TelephoneNumbers = "TelephoneNumbers";
			public const string TelephoneNumber = "TelephoneNumber";
		}
	}
}