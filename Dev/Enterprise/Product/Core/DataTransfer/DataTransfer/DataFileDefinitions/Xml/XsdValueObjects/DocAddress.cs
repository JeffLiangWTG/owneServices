using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoDocAddress")]
	public class DocAddress : Xsd.AutoDocAddress
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return IsAdditionalRequirementForDocAddressValidityMet; }
		}

		bool IsAdditionalRequirementForDocAddressValidityMet
		{
			get { return AddressReference.IsSpecified || !AddressLine1.IsEmpty || !CompanyName.IsEmpty || !CityOrSuburb.IsEmpty || !StateOrProvince.IsEmpty || !AddressLine2.IsEmpty || !AddressCode.IsEmpty || !PostCode.IsEmpty || TelephoneNumbers.IsSpecified || !Email.IsEmpty || !Language.IsEmpty || !CountryCode.IsEmpty || !ContactName.IsEmpty || IsResidentialSpecified || RegistrationNumberSpecified; }
		}
	}
}
