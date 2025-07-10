using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class AdditionalCustomsInformationCollection : Xsd.AutoAdditionalCustomsInformationCollection
	{
		public Xsd.AdditionalCustomsInformation FindByType(string customDetailType)
		{
			foreach (Xsd.AdditionalCustomsInformation info in this)
			{
				if (info.CustomsDetailType == customDetailType)
				{
					return info;
				}
			}
			return null;
		}
	}
}
