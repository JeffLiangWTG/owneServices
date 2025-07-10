using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.GB.Business
{
	public static class CTCExtensions
	{
		public static T Deserialize<T>(ZString serializedObj)
		{
			using (var stream = new StringReader(serializedObj))
			{
				using (var reader = new XmlTextReader(stream))
				{
					var serializer = ZXmlSerializer.New(typeof(T));
					return (T)serializer.Deserialize(reader);
				}
			}
		}
	}
}
