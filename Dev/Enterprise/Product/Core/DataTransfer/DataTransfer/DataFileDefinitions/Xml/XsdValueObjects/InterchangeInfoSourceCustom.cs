using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoInterchangeInfoSourceCustom")]
	public class InterchangeInfoSourceCustom : Xsd.AutoInterchangeInfoSourceCustom
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified && (!FieldLabel1.IsEmpty || !FieldLabel2.IsEmpty ||
				!FieldLabel3.IsEmpty || !FieldLabel4.IsEmpty || !FieldLabel5.IsEmpty ||
				!Value1.IsEmpty || !Value2.IsEmpty || !Value3.IsEmpty || !Value4.IsEmpty || !Value5.IsEmpty);
			}
		}
	}
}
