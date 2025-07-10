using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlGeneratorTest : TestCase
	{
		public void TestXmlFragment()
		{
			XmlGenerator generator = new XmlGenerator();
			AssertEquals("<Test>Data</Test>\n", generator.XmlFragment("Data", "Test"));
		}
	}
}
