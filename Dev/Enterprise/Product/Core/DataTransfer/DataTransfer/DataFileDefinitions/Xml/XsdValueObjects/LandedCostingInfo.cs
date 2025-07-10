using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoLandedCostingInfo")]
	public class LandedCostingInfo : Xsd.AutoLandedCostingInfo
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && (LCGroupCharges.Count > 0 && !LineType.IsEmpty); }
		}
	}
}
