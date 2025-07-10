using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoMonetaryAmount")]
	public class MonetaryAmount : Xsd.AutoMonetaryAmount
	{
		public MonetaryAmount()
		{
			IsSpecified = true;
		}

		[XmlIgnore]
		public override ZDecimal Value
		{
			get { return base.Value; }
			set { base.Value = Utilities.Round(value, 4); }
		}
	}
}
