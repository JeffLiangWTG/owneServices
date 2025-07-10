using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LastDbRestoreInfo))]
	sealed class LastDbRestoreInfoTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestReadElements()
		{
			AssertReadElements(CultureInfo.GetCultureInfo("EN-US"));
			AssertReadElements(CultureInfo.GetCultureInfo("ZH-CN"));

			void AssertReadElements(CultureInfo cultureInfo)
			{
				using (Culture.SetTemporarily(cultureInfo))
				{
					var xml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<LastDbRestoreInfo>
    <Operation>RestoreWithRecovery</Operation>
    <ToolVersion>20.8.20.0</ToolVersion>
    <CompletionDate>04/06/2022 11:09:24 AM</CompletionDate>
    <DbSchemaVersionBefore>1.0</DbSchemaVersionBefore>
    <DbSchemaVersionAfter>2.0</DbSchemaVersionAfter>
</LastDbRestoreInfo>";

					using (var stringReader = new StringReader(xml))
					using (var xmlReader = XmlReader.Create(stringReader))
					{
						var info = new LastDbRestoreInfo();
						xmlReader.ReadStartElement();
						((IXmlSerializable)info).ReadXml(xmlReader);

						AssertEquals("RestoreWithRecovery", info.Operation);
						AssertEquals("20.8.20.0", info.ToolVersion);
						AssertEquals(new ZDateTime(2022, 6, 4, 11, 09, 24), info.CompletionDate);
						AssertEquals("1.0", info.DbSchemaVersionBefore);
						AssertEquals("2.0", info.DbSchemaVersionAfter);
					}
				}
			}
		}

		public void TestWriteElements()
		{
			AssertWriteElements(CultureInfo.GetCultureInfo("EN-US"));
			AssertWriteElements(CultureInfo.GetCultureInfo("ZH-CN"));

			void AssertWriteElements(CultureInfo cultureInfo)
			{
				using (Culture.SetTemporarily(cultureInfo))
				{
					using (var stream = new StringWriter())
					using (var writer = new XmlTextWriter(stream))
					{
						var info = new LastDbRestoreInfo();
						info.Operation = "RestoreWithRecovery";
						info.ToolVersion = "20.8.20.0";
						info.CompletionDate = new ZDateTime(2023, 6, 4, 11, 09, 24);
						info.DbSchemaVersionBefore = "1.0";
						info.DbSchemaVersionAfter = "2.0";

						writer.WriteStartElement("LastDbRestoreInfo");
						((IXmlSerializable)info).WriteXml(writer);
						writer.WriteEndElement();
						writer.Flush();

						string xml = "<LastDbRestoreInfo><Operation>RestoreWithRecovery</Operation><ToolVersion>20.8.20.0</ToolVersion><CompletionDate>04/06/2023 11:09:24 AM</CompletionDate><DbSchemaVersionBefore>1.0</DbSchemaVersionBefore><DbSchemaVersionAfter>2.0</DbSchemaVersionAfter></LastDbRestoreInfo>";
						AssertEquals(xml, stream.ToString());
					}
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new LastDbRestoreInfo();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
