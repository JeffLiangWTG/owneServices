using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoOrganisationDetailCountrySpecificDetails")]
	public class OrganisationDetailCountrySpecificDetails : AutoOrganisationDetailCountrySpecificDetails
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return USOrganisationSpecificDetailsSpecified;
			}
		}
	}
}
