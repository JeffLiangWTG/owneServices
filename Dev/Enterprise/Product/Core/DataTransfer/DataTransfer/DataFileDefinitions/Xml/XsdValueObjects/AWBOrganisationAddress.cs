using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAWBOrganisationAddress")]
	public class AWBOrganisationAddress : Xsd.AutoAWBOrganisationAddress
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified && (
				 (Item is AWBOrgAddressOverride && !((AWBOrgAddressOverride)Item).AddressLine1.IsEmpty) ||
				 (Item is AWBOrgDocAddress && !((AWBOrgDocAddress)Item).CompanyName.IsEmpty));
			}
		}
	}
}
