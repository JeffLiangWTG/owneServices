using System.Xml.Serialization;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataFileDefinitions.Xml.XsdValueObjects
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoPackageCustom")]
	public class PackageCustom : Xsd.AutoPackageCustom
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return Date1Specified || Date2Specified ||
				Text1Specified || Text2Specified || Text3Specified || Text4Specified ||
				Decimal1Specified || Decimal2Specified ||
				Flag1Specified || Flag2Specified;
			}
		}
	}
}
