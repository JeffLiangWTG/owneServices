using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed class DepotAddressColorSoundForTest : DepotAddressColorSound
	{
		public DepotAddressColorSoundForTest(BusinessObjectFactory factory) : base(factory) { }

		public void WriteElementsForTest(XmlWriter writer)
		{
			writer.WriteStartDocument(true);
			writer.WriteStartElement("TEST");
			WriteElements(writer);
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		public void ReadElementsForTest(XmlReaderWrapper wrapper)
		{
			ReadElements(wrapper);
		}
	}
}
