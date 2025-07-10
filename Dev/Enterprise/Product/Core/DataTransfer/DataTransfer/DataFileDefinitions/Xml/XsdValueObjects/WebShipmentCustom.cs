using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoWebShipmentCustom")]
	public class WebShipmentCustom : Xsd.AutoWebShipmentCustom
	{
		[XmlIgnoreAttribute]
		public override bool IsSpecified
		{
			get
			{
				return
					!CustomAttrib1.IsEmpty ||
					!CustomAttrib2.IsEmpty ||
					CustomDate1.IsValid ||
					CustomDate2.IsValid ||
					CustomDecimal1 != 0m ||
					CustomDecimal2 != 0m ||
					CustomFlag1 || CustomFlag2;
			}
		}
	}
}
