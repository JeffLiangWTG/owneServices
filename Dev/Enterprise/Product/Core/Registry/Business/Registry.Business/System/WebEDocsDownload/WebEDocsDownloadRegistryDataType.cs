using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebEDocsDownloadEntryDictionary : SerializableDictionary<string, WebEDocsDownloadEntry>
	{
	}

	public class WebEDocsDownloadRegistryDataType : RegistryDataType<WebEDocsDownloadEntryDictionary>
	{
		public WebEDocsDownloadRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, new WebEDocsDownloadEntryDictionary())
		{
		}

		protected override byte[] SerialiseCore(WebEDocsDownloadEntryDictionary value)
		{
			byte[] result = null;
			var serialiser = ZXmlSerializer.New(DataType);

			using (var stream = new MemoryStream())
			using (var writer = new XmlTextWriter(stream, new UnicodeEncoding(false, false)))
			{
				serialiser.Serialize(writer, value);
				writer.Flush();
				result = stream.ToArray();
			}

			return result;
		}

		protected override WebEDocsDownloadEntryDictionary DeserialiseCore(byte[] value)
		{
			if (value.Length > 0)
			{
				var serialiser = ZXmlSerializer.New(DataType);

				using (var stream = new MemoryStream(value))
				using (var reader = new XmlTextReader(stream))
				{
					return (WebEDocsDownloadEntryDictionary)serialiser.Deserialize(reader);
				}
			}
			else
			{
				return DefaultValue;
			}
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new WebEDocsDownloadEditorInfo();
		}

		protected override WebEDocsDownloadEntryDictionary CloneValue(WebEDocsDownloadEntryDictionary value)
		{
			var newDictionary = new WebEDocsDownloadEntryDictionary();
			foreach (var item in value)
			{
				newDictionary.Add(item.Key, item.Value);
			}
			return newDictionary;
		}
	}
}
