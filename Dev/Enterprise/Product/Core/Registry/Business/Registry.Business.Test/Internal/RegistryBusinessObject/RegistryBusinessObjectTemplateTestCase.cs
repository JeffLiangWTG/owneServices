using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RegistryBusinessObjectTemplate))]
	public abstract class RegistryBusinessObjectTemplateTestCase : RegistryBusinessObjectTemplateTestCase<RegistryBusinessObjectTemplate>
	{
		public static byte[] Serialize<T>(T value)
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(T));

			return dataType.Serialise(value);
		}

		public static T Deserialize<T>(byte[] serializedValue)
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(T));
			return (T)dataType.Deserialise(serializedValue);
		}
	}
}
