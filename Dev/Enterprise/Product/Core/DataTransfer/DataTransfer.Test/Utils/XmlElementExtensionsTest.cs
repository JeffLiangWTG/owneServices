using System.Xml;
using NUnit.Framework;

namespace CargoWise.Xml.Testing
{
	sealed class XmlElementExtensionsTest : TestCase
	{
		public void TestOuterXmlLowMemory()
		{
			XmlDocument d = new XmlDocument();
			d.LoadXml(xml);
			AssertEquals(d.DocumentElement.OuterXml, d.DocumentElement.OuterXmlLowMemory());
		}

		public void TestInnerXmlLowMemory()
		{
			XmlDocument d = new XmlDocument();
			d.LoadXml(xml);
			AssertEquals(d.DocumentElement.InnerXml, d.DocumentElement.InnerXmlLowMemory());
		}

		const string xml = "<OuterNode><InnerNode attribute=\"1\" attribute2=\"2\"></InnerNode></OuterNode>";

		public void TestCreateReader()
		{
			XmlDocument d = new XmlDocument();
			d.LoadXml(xml);

			XmlDocument d2 = new XmlDocument();
			using (var reader = d.DocumentElement.CreateReader())
			{
				d2.Load(reader);
			}
			AssertEquals(xml, d2.DocumentElement.OuterXml);
		}
	}
}
