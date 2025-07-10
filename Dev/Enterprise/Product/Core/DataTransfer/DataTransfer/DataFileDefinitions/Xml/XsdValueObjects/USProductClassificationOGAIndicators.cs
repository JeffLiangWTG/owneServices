using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSProductClassificationOGAIndicators")]
	public class USProductClassificationOGAIndicators : Xsd.AutoUSProductClassificationOGAIndicators
	{
		[XmlIgnore]
		public override bool ShouldCreateElementForEmptyValue
		{
			get { return false; }
		}

		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return FCCIndicatorSpecified || FDAIndicatorSpecified || DOTIndicatorSpecified; }
			set { base.IsSpecified = value; }
		}
	}
}
