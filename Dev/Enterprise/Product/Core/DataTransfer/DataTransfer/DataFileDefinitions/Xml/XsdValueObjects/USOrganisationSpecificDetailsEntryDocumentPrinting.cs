using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSOrganisationSpecificDetailsEntryDocumentPrinting")]
	public class USOrganisationSpecificDetailsEntryDocumentPrinting : AutoUSOrganisationSpecificDetailsEntryDocumentPrinting
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return CustomAttribute1Specified ||
					CustomAttribute2Specified ||
					CustomAttribute3Specified ||
					ProductCodeSpecified;
			}
		}
	}
}
