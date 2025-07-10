using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(OpenURLActionMethodSettings))]
	sealed class OpenURLActionMethodSettingsTest : OperationalActionMethodSettingsTest<OpenURLActionMethodSettings>
	{
		public void TestReadXml()
		{
			const string sourceXml = "<Settings><URL>http://www.cargowise.com</URL></Settings>";

			var settings = new OpenURLActionMethodSettings();

			using (var stream = new StringReader(sourceXml))
			using (var reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)settings).ReadXml(reader);
			}

			AssertEquals("http://www.cargowise.com", settings.URL);
		}

		public void TestWriteXml()
		{
			const string expectedXml = "<Settings><URL>http://www.cargowise.com</URL></Settings>";

			var settings = new OpenURLActionMethodSettings();
			settings.URL = "http://www.cargowise.com";

			string actualXml;

			using (var stream = new StringWriter())
			using (var writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("Settings");
				((IXmlSerializable)settings).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				actualXml = stream.ToString();
			}

			AssertEquals(expectedXml, actualXml);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OpenURLActionMethodSettings();
		}

		#endregion
	}
}
