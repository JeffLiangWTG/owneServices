using System.IO;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.TWCustoms.IntegrationTests.Helpers
{
	internal static class Serialize
	{
		internal static string ToXml(object toSerialize)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

			using (StringWriter textWriter = new StringWriter())
			{
				xmlSerializer.Serialize(textWriter, toSerialize);
				return textWriter.ToString();
			}
		}

		internal static T FromXML<T>(this string toDeserialize)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using (StringReader textReader = new StringReader(toDeserialize))
			{
				return (T)xmlSerializer.Deserialize(textReader);
			}
		}
	}
}