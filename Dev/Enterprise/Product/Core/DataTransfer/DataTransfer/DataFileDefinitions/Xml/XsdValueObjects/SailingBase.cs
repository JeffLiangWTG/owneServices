using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoSailingBase")]
	public class SailingBase : AutoSailingBase
	{
		public virtual ZString GetVesselName(IValueObjectImportContext context)
		{
			return ZString.Empty;
		}
	}
}
