using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoClassificationRelatedOrg")]
	public class ClassificationRelatedOrg : Xsd.AutoClassificationRelatedOrg
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return !Code.IsEmpty; }
		}

		[XmlIgnore]
		public override bool ShouldCreateElementForEmptyValue
		{
			get { return false; }
		}
	}
}
