using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoMovement")]
	public class Movement : Xsd.AutoMovement
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && (Port.IsSpecified || EstimatedDateTime.IsValid || ActualDateTime.IsValid); }
		}
	}
}
