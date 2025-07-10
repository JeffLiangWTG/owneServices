using System.IO;
using System.Xml.Linq;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.ZArchitecture.Environment.DialogDefault
{
	public interface ISerializer<T>
	{
		XElement Serialize(T obj);

		T Deserialize(XElement node);
	}

	public class ZXmlSerializerWrapper<T> : ISerializer<T>
	{
		ZXmlSerializer serializer;
		ZXmlSerializer Serializer
		{
			get { return serializer ?? (serializer = ZXmlSerializer.New(typeof(T))); }
		}

		public XElement Serialize(T obj)
		{
			using (var memoryStream = new MemoryStream())
			{
				Serializer.Serialize(memoryStream, obj);
				memoryStream.Seek(0, SeekOrigin.Begin);

				return XElement.Load(memoryStream);
			}
		}

		public T Deserialize(XElement node)
		{
			using (var reader = node.CreateReader())
			{
				return (T)Serializer.Deserialize(reader);
			}
		}
	}
}
