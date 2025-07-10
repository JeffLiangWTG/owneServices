using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryBusinessObjectTestCase : TestCase
	{
		public void TestReadElements()
		{
			string xml =
@"<Table1>
<CodeMaxLength>3</CodeMaxLength>
<Code>CAN</Code>
<Description>My description for CAN</Description>
</Table1>";

			using (var stringReader = new StringReader(xml))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				var bizObj = new DummyRegistryBusinessObject();
				xmlReader.ReadStartElement();
				((IXmlSerializable)bizObj).ReadXml(xmlReader);

				AssertEquals(3, bizObj.CodeMaxLength);
				AssertEquals("CAN", bizObj.Code);
				AssertEquals("My description for CAN", bizObj.Description);
			}

			string xml2 =
@"<Table1>
<CodeMaxLength>3</CodeMaxLength>
<Code>LONGCODE</Code>
<Description>My description for LONGCODE</Description>
</Table1>";

			using (var stringReader = new StringReader(xml2))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				var bizObj = new DummyRegistryBusinessObject();
				xmlReader.ReadStartElement();
				((IXmlSerializable)bizObj).ReadXml(xmlReader);

				AssertEquals(3, bizObj.CodeMaxLength);
				AssertEquals("Code should be empty when code is longer than CodeMaxLength", "", bizObj.Code);
				AssertEquals("Description should be empty when code is longer than CodeMaxLength", "", bizObj.Description);
			}

			string xml3 =
@"<Table1>
<CodeMaxLength>8</CodeMaxLength>
<Code>LONGCODE</Code>
<Description>This note is more than 35 characters</Description>
</Table1>";

			using (var stringReader = new StringReader(xml3))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				var bizObj = new DummyRegistryBusinessObject();
				xmlReader.ReadStartElement();

				AssertNoExceptionThrown("No exception throw", () => ((IXmlSerializable)bizObj).ReadXml(xmlReader));
				AssertEquals(bizObj.Description_MaxLength, bizObj.Description.ToString().Length);
			}
		}

		public void TestWriteElements()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("F904177E-FD80-43B8-A541-B0A553838FF6", new ResourceStringData("F904177E-FD80-43B8-A541-B0A553838FF6", "测试"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var bizObj = new DummyRegistryBusinessObject();
					bizObj.Code = "ABC";
					bizObj.CodeMaxLength = 10;
					bizObj.Description = (NoResString)"Test";
					string xml;
					using (var stream = new StringWriter())
					using (var writer = new XmlTextWriter(stream))
					{
						writer.WriteStartElement("Elements");
						((IXmlSerializable)bizObj).WriteXml(writer);
						writer.WriteEndElement();
						writer.Flush();
						xml = stream.ToString();
					}

					var expectedXml = @"<Elements>
<CodeMaxLength>10</CodeMaxLength>
<Code>ABC</Code>
<Description>Test</Description>
</Elements>";

					AssertMultilineASCIIEquals("", expectedXml.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));
				}
			}
		}
	}
}
