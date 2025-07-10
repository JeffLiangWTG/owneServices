using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class DataObjectSerializer
	{
		public DataObjectSerializer()
			: this(new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Document,
				OmitXmlDeclaration = false,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				Encoding = Encoding.UTF8,
				Indent = true
			})
		{
		}

		public DataObjectSerializer(XmlWriterSettings xmlWriterSettings)
		{
			this.xmlWriterSettings = xmlWriterSettings;
		}

		readonly XmlWriterSettings xmlWriterSettings;

		public void Serialize<TDataObject>(TDataObject dataObject, Stream targetStream)
			where TDataObject : class
		{
			using (var xmlWritter = XmlWriter.Create(targetStream, xmlWriterSettings))
			{
				var serializer = ZXmlSerializer.New(typeof(TDataObject));
				serializer.Serialize(xmlWritter, dataObject);
			}
		}

		public static TDataObject Deserialize<TDataObject>(ZString serializedObj)
			where TDataObject : class
		{
			using (var stream = new StringReader(serializedObj))
			using (var reader = new XmlTextReader(stream))
			{
				var serializer = ZXmlSerializer.New(typeof(TDataObject));
				return CanDeserialize(serializer, reader) ? (TDataObject)serializer.Deserialize(reader) : default;
			}
		}

		static bool CanDeserialize(ZXmlSerializer serializer, XmlReader reader)
		{
			bool result;
			try
			{
				result = serializer.CanDeserialize(reader);
			}
			catch (XmlException)
			{
				result = false;
			}

			return result;
		}
	}
}