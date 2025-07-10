using System;
using System.Xml.Linq;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Native.Utils
{
	public class ObjectXmlSerializer : IXmlSerializer
	{
		public ObjectXmlSerializer(Type type)
		{
			xs = ZXmlSerializer.New(type);
		}
		protected readonly ZXmlSerializer xs;

		public XElement Serialize(object input)
		{
			var document = new XDocument();
			using (var w = document.CreateWriter())
			using (var writer = new CleanXmlWriter(w))
			{
				xs.Serialize(writer, input);
			}
			var e = document.Root;
			e.Remove();
			return e;
		}
	}

	public class ObjectXmlSerializer<T> : ObjectXmlSerializer
	{
		public ObjectXmlSerializer()
			: base(typeof(T))
		{
		}

		public T Deserialize(XElement element)
		{
			T result;

			using (var r = element.CreateReader())
			{
				result = (T)xs.Deserialize(r);
			}
			return result;
		}
	}

	public interface IXmlSerializer
	{
		XElement Serialize(object input);
	}
}