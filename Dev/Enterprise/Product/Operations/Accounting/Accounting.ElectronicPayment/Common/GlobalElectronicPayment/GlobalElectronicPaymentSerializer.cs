using System.IO;
using System.Xml;
using Enterprise.Accounting.ElectronicMessaging.Common;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	public class GlobalElectronicPaymentSerializer
	{
		public GlobalElectronicPaymentSerializer()
		{
			Serializer = new DataObjectSerializer();
		}

		public GlobalElectronicPaymentSerializer(XmlWriterSettings writerSettings)
		{
			Serializer = new DataObjectSerializer(writerSettings);
		}

		DataObjectSerializer Serializer { get; }

		public GlobalElectronicPayment.GlobalElectronicPayment Deserialize(string serializedEPayment) =>
			DataObjectSerializer.Deserialize<GlobalElectronicPayment.GlobalElectronicPayment>(serializedEPayment);

		public string Serialize(GlobalElectronicPayment.GlobalElectronicPayment ePayment)
		{
			using (var ms = new MemoryStream())
			{
				//Byte Order Mark(BOM) is added by the serialization process.
				Serializer.Serialize(ePayment, ms);
				ms.Position = 0;
				using (var reader = new StreamReader(ms, true))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
