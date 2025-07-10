using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoPIRPRulingType")]
	public class PIRPRulingType : Xsd.AutoPIRPRulingType
	{
		public PIRPRulingType()
		{
		}

		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return !Number.IsEmpty || !Type.IsEmpty; }
			set { base.IsSpecified = value; }
		}
	}
}
