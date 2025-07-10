using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class RootElementRenamingXmlWriterTest : TestCase
	{
		public void TestRenamingRootElement()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				XmlWriter writer = new RootElementRenamingXmlWriter(new XmlTextWriter(stream, Encoding.ASCII), "RootElement");
				try
				{
					writer.WriteStartElement("daspofjasiofdjsaiodas some more garbage weadasda");
					writer.WriteAttributeString("attr", "value");
					writer.WriteStartElement("InnerElement");
					writer.WriteEndElement();
					writer.WriteEndElement();

					writer.Flush();
					stream.Flush();
					string expectedResult = "<RootElement attr=\"value\"><InnerElement /></RootElement>";
					string actualResult = Encoding.ASCII.GetString(stream.ToArray());
					AssertEquals("Should change the name of the root element for you", expectedResult, actualResult);
				}
				finally
				{
					writer.Close();
				}
			}
		}
	}
}
