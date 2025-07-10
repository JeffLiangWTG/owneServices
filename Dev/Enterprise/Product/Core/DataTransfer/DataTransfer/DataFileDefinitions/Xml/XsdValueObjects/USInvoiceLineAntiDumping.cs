using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSInvoiceLineAntiDumping")]
	public class USInvoiceLineAntiDumping : Xsd.AutoUSInvoiceLineAntiDumping
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified && !CaseNo.IsEmpty;
			}
		}
	}
}
