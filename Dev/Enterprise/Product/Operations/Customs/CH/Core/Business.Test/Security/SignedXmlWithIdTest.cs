using System.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class SignedXmlWithIdTest : TestCase
{
	public void TestGetIdElement()
	{
		var xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(TestingData.InputEvvResponseVAT());
		var signedXmlWithId = new SignedXmlWithId(xmlDoc);

		var xmlElement = signedXmlWithId.GetIdElement(xmlDoc, "Timestamp-75121");

		var expectedXml = @"<wsu:Timestamp wsu:Id=""Timestamp-75121"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd""><wsu:Created>2022-12-29T08:25:46.430Z</wsu:Created><wsu:Expires>2022-12-29T08:30:46.430Z</wsu:Expires></wsu:Timestamp>";

		CombineAssertions(() =>
		{
			AssertEquals("Timestamp Element Name", "wsu:Timestamp", xmlElement.Name);
			AssertEquals("Timestamp Element XML", expectedXml, xmlElement.OuterXml);
			AssertEquals("Timestamp Element Text", "2022-12-29T08:25:46.430Z2022-12-29T08:30:46.430Z", xmlElement.InnerText);
		});
	}
}
