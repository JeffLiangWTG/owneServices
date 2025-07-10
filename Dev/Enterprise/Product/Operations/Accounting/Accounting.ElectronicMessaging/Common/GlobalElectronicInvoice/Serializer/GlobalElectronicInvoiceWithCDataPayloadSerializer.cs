using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GlobalElectronicInvoiceWithCDataPayloadSerializer : IGlobalElectronicInvoiceSerializer
	{
		public GlobalElectronicInvoiceWithCDataPayloadSerializer()
		{
			Serializer = new DataObjectSerializer();
		}

		public GlobalElectronicInvoiceWithCDataPayloadSerializer(XmlWriterSettings writerSettings)
		{
			Serializer = new DataObjectSerializer(writerSettings);
		}

		DataObjectSerializer Serializer { get; }

		public GlobalElectronicInvoicing Deserialize(string serializedEInvoice) =>
			DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(serializedEInvoice);

		public string Serialize(GlobalElectronicInvoicing eInvoice)
		{
			var placeHolder = "___Payload___Placeholder___";
			var actualPayload = AddCDATATag(eInvoice.Payload);
			eInvoice.Payload = placeHolder;

			using (var ms = new MemoryStream())
			{
				//Byte Order Mark(BOM) is added by the serialization process.
				Serializer.Serialize(eInvoice, ms);
				ms.Position = 0;
				using (var reader = new StreamReader(ms, true))
				{
					//All reserved Xml characters inside the CData section are escaped by our serializer which is unnecessary.
					//To stop that from happening, we need this hack.
					return reader.ReadToEnd().Replace(placeHolder, actualPayload);
				}
			}
		}

		#region CData Tag handler functions

		static string AddCDATATag(string text) =>
			GetCDATAExp().IsMatch(text) ? text : FormattableString.Invariant($"<![CDATA[{text}]]>"); // XML tag.

		static Regex GetCDATAExp() => new Regex(@"<!\[CDATA\[(.|\s)*]]>"); // XML tag.

		#endregion
	}
}