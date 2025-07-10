using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class ISFContainerCollection : Xsd.AutoISFContainerCollection
	{
		public Xsd.ISFContainer GetContainerFromCollection(ZString containerNumber)
		{
			foreach (Xsd.ISFContainer container in this)
			{
				if (container.ContainerNumber == containerNumber)
				{
					return container;
				}
			}

			return null;
		}
	}
}
