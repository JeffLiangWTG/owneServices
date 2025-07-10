using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSOrganisationSpecificDetailsFDA")]
	public class USOrganisationSpecificDetailsFDA : AutoUSOrganisationSpecificDetailsFDA
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return !Email.IsEmpty ||
						!FirstName.IsEmpty ||
						!LastName.IsEmpty ||
						!PhoneNo.IsEmpty ||
						!FaxNo.IsEmpty ||
						ProducerFirmTypeSpecified ||
						FoodFacilityRegistrationExemptionSpecified ||
						SubmitterFirmTypeSpecified;
			}
		}
	}
}
