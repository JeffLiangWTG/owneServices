using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoSoftwoodLumberType")]
	public class SoftwoodLumberType : Xsd.AutoSoftwoodLumberType
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && (!ExportCharge.IsEmpty || !ExportPrice.IsEmpty || ImporterDec); }
		}

		[XmlIgnore]
		public override bool ShouldCreateElementForEmptyValue
		{
			get { return false; }
		}
	}
}
