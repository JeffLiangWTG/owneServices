using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoLandedCostingHeader")]
	public class LandedCostingHeader : Xsd.AutoLandedCostingHeader
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && LandedCostingDateSpecified && LandedCostingGroupHeaders.Count > 0; }
		}
	}
}
