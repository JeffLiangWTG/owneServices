using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class ContainerCollection : Xsd.AutoContainerCollection
	{
		public Xsd.Container GetContainerFromCollection(ZString containerNumber)
		{
			foreach (Xsd.Container container in this)
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
