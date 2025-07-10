namespace CargoWise.Common.Testing
{
	using System.IO;
	using NUnit.Framework;

	public class TestXmlUtils : TestCase
	{
		public void TestIsValidXml()
		{
			// Arrange
			const string invalidXmlText1 = "<xml><a></xml>";
			const string invalidXmlText2 = "\u0001\u0003";
			const string validXmlText = "<abc></abc>";
			// Act & Assert
			Assert(!XmlUtils.IsValidXml(invalidXmlText1));
			Assert(!XmlUtils.IsValidXml(invalidXmlText2));
			Assert(XmlUtils.IsValidXml(validXmlText));

			Assert(!XmlUtils.IsValidXml(invalidXmlText1, out var xmlDocument1));
			Assert(!xmlDocument1.HasChildNodes);
			Assert(!XmlUtils.IsValidXml(invalidXmlText2, out var xmlDocument2));
			Assert(!xmlDocument2.HasChildNodes);
			Assert(XmlUtils.IsValidXml(validXmlText, out var xmlDocument3));
			Assert(xmlDocument3.HasChildNodes);
		}

		public void TestIsValidXml_TextReader()
		{
			using (var reader = new StringReader("<xml><a></xml>"))
			{
				AssertEquals("Invalid xml text", false, XmlUtils.IsValidXml(reader, out var xmlDocument));
				AssertEquals("Invalid xml text - HasChildNodes", false, xmlDocument.HasChildNodes);
			}
			using (var reader = new StringReader("\u0001\u0003"))
			{
				AssertEquals("Invalid xml text 2", false, XmlUtils.IsValidXml(reader, out var xmlDocument));
				AssertEquals("Invalid xml text 2 - HasChildNodes", false, xmlDocument.HasChildNodes);
			}
			using (var reader = new StringReader("<abc></abc>"))
			{
				AssertEquals("Valid xml text", true, XmlUtils.IsValidXml(reader, out var xmlDocument));
				AssertEquals("Valid xml text - HasChildNodes", true, xmlDocument.HasChildNodes);
			}
		}
	}
}
