using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(RunProgramActionMethodSettings))]
	sealed class RunProgramActionMethodSettingsTest : OperationalActionMethodSettingsTest<RunProgramActionMethodSettings>
	{
		public void TestReadXml()
		{
			const string sourceXml = @"<Settings><Path>C:\Windows\system32\notepad.exe</Path><Arguments>test.txt</Arguments></Settings>";

			var settings = new RunProgramActionMethodSettings();

			using (var stream = new StringReader(sourceXml))
			using (var reader = new XmlTextReader(stream))
			{
				((IXmlSerializable)settings).ReadXml(reader);
			}

			AssertEquals(@"C:\Windows\system32\notepad.exe", settings.Path);
			AssertEquals(@"test.txt", settings.Arguments);
		}

		public void TestWriteXml()
		{
			const string expectedXml = @"<Settings><Path>C:\Windows\system32\notepad.exe</Path><Arguments>test.txt</Arguments></Settings>";

			var settings = new RunProgramActionMethodSettings();
			settings.Path = @"C:\Windows\system32\notepad.exe";
			settings.Arguments = @"test.txt";

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
			return new RunProgramActionMethodSettings();
		}

		#endregion
	}
}
