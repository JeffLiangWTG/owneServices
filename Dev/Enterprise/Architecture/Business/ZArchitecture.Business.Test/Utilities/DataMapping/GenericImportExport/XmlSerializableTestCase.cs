using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	public abstract class XmlSerializableTestCase<T> : TestCase where T : XmlSerializableSetting
	{
		protected string Serialize(T t)
		{
			return t.AsXml();
		}

		protected T Deserialize(string xml)
		{
			return XmlSerializableSetting.FromXml<T>(xml);
		}
	}
}
