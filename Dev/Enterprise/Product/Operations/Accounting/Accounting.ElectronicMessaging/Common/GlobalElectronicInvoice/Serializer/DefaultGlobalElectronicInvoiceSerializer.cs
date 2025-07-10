using System.IO;
using System.Xml;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class DefaultGlobalElectronicInvoiceSerializer : IGlobalElectronicInvoiceSerializer
	{
		public DefaultGlobalElectronicInvoiceSerializer()
		{
			Serializer = new DataObjectSerializer();
		}

		public DefaultGlobalElectronicInvoiceSerializer(XmlWriterSettings writerSettings)
		{
			Serializer = new DataObjectSerializer(writerSettings);
		}

		DataObjectSerializer Serializer { get; }

		public GlobalElectronicInvoicing Deserialize(string serializedEInvoice) =>
			DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(serializedEInvoice);

		public string Serialize(GlobalElectronicInvoicing eInvoice)
		{
			using (var ms = new MemoryStream())
			{
				//Byte Order Mark(BOM) is added by the serialization process.
				Serializer.Serialize(eInvoice, ms);
				ms.Position = 0;
				using (var reader = new StreamReader(ms, true))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
