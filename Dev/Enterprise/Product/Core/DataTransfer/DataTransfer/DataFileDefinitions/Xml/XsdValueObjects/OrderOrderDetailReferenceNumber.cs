using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoOrderOrderDetailReferenceNumber")]
	public class OrderOrderDetailReferenceNumber : Xsd.AutoOrderOrderDetailReferenceNumber
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return !this.Value.IsEmpty; }
		}

		public static Xsd.OrderOrderDetailReferenceNumber FromReferenceNumberType(Xsd.OrderOrderDetailReferenceNumberType referenceType, ZString value)
		{
			Xsd.OrderOrderDetailReferenceNumber referenceNumber = new OrderOrderDetailReferenceNumber();
			referenceNumber.Value = value;
			referenceNumber.Type = referenceType;
			return referenceNumber;
		}
	}
}
