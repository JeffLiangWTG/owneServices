using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(ShowEditNoteActionMethodSettings))]
	sealed class ShowEditNoteActionMethodSettingsTest : OperationalActionMethodSettingsTest<ShowEditNoteActionMethodSettings>
	{
		public void TestReadXml()
		{
			const string sourceXml = "<Settings><NoteDescription>Xyz</NoteDescription></Settings>";

			var settings = new ShowEditNoteActionMethodSettings();

			using (var stream = new StringReader(sourceXml))
			using (var reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)settings).ReadXml(reader);
			}

			AssertEquals("Xyz", settings.NoteDescription);
		}

		public void TestWriteXml()
		{
			const string expectedXml = "<Settings><NoteDescription>Xyz</NoteDescription></Settings>";

			var settings = new ShowEditNoteActionMethodSettings();
			settings.NoteDescription = "Xyz";

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
			return new ShowEditNoteActionMethodSettings();
		}

		#endregion
	}
}
