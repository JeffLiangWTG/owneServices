using System.Reflection;
using System.Xml.Serialization;
using NUnit.Framework;

namespace CargoWise.Application.InversionOfControl.Testing
{
	public class ObjectDefinitionsTest : TestCase
	{
		public void TestXmlSerializeIsSgened()
		{
			var serializer = new XmlSerializer(typeof(ObjectDefinitions));
			var fieldName = "_tempAssembly";
#if NETFRAMEWORK
			fieldName = "tempAssembly";
#endif
			var tempAssembly = serializer.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(serializer);
			fieldName = "_assembly";
#if NETFRAMEWORK
			fieldName = "assembly";
#endif
			var assembly = (Assembly)tempAssembly.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempAssembly);
			AssertEquals("XmlSeralizer ObjectDefinitions should be sgned.", "CargoWise.ApplicationContext.XmlSerializers", assembly.GetName().Name);
		}
	}
}
