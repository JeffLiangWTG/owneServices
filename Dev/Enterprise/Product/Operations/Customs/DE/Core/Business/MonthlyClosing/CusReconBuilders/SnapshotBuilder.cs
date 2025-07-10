using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	abstract class SnapshotBuilder<T>
	{
		public abstract T GenerateMessage();

		public string GetXMLMessage()
		{
			return Serialize(GenerateMessage());
		}

		public static string Serialize(T xmlObject)
		{
			var nameSpaces = new XmlSerializerNamespaces();
			nameSpaces.Add("", NameSpace);
			var settings = new XmlWriterSettings()
			{
				OmitXmlDeclaration = true
			};

			var result = string.Empty;

			using (var stream = new StringWriter())
			{
				using (var xmlWriter = XmlWriter.Create(stream, settings))
				{
					var xmlSerialiser = ZXmlSerializer.New(typeof(T));
					xmlSerialiser.Serialize(xmlWriter, xmlObject, nameSpaces);
					result = stream.ToString();
				}
			}

			return result;
		}

		public static T Deserialize(string xmlString)
		{
			T result = default;

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
			{
				var xmlSerialiser = ZXmlSerializer.New(typeof(T));
				result = (T)xmlSerialiser.Deserialize(stream);
			}

			return result;
		}

		const string NameSpace = "http://www.cargowise.com/Schemas/DEMonthlyClosing";
	}
}
