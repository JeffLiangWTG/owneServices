using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAntiDumping")]
	public class AntiDumping : Xsd.AutoAntiDumping
	{
		public AntiDumping()
		{
		}

		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return !CaseNo.IsEmpty; }
			set { base.IsSpecified = value; }
		}
	}
}
