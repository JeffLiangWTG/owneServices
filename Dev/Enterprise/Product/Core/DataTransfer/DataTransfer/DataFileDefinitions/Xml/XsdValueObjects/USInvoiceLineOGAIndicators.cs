using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoUSInvoiceLineOGAIndicators")]
	public class USInvoiceLineOGAIndicators : Xsd.AutoUSInvoiceLineOGAIndicators
	{
		[XmlIgnore]
		public override bool ShouldCreateElementForEmptyValue
		{
			get { return false; }
		}

		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return DOTIndicatorSpecified ||
						FCCIndicatorSpecified ||
						FDAIndicatorSpecified;
			}
			set { base.IsSpecified = value; }
		}
	}
}
