using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoOrderOrderLineOrderLineDeliveryDeliveryDetailsCustom")]
	public class OrderOrderLineOrderLineDeliveryDeliveryDetailsCustom : Xsd.AutoOrderOrderLineOrderLineDeliveryDeliveryDetailsCustom
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified &&
					(
						Decimal1Specified || Decimal2Specified || Decimal3Specified || Decimal4Specified || Decimal5Specified
						|| !Text1.IsEmpty || !Text2.IsEmpty || !Text3.IsEmpty || !Text4.IsEmpty || !Text5.IsEmpty
						|| Flag1Specified || Flag2Specified || Flag3Specified || Flag4Specified || Flag5Specified
						|| Date1.IsValid || Date2.IsValid || Date3.IsValid || Date4.IsValid || Date5.IsValid
					);
			}
		}
	}
}
