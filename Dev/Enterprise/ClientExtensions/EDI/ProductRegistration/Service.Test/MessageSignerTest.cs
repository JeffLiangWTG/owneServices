using System.Xml;
using CargoWise.Licensing;
using NUnit.Framework;

namespace CargoWise.ProductRegistration.Service.Test
{
	public class MessageSignerTest : TestCase
	{
		public void TestSign()
		{
			var unsignedXml = @"
<test>
  <node1>one</node1>
  <node2>two</node2>
</test>";

			var signedXml = MessageSigner.SignXml(unsignedXml);
			var tamperedXml = signedXml.Replace("<node1>one</node1>", "<node1>ten</node1>");
			AssertNotEquals(signedXml, tamperedXml);

			var doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(signedXml);
			AssertEquals(true, SignedMessage.VerifyXml(doc));

			doc.LoadXml(unsignedXml);
			AssertEquals(false, SignedMessage.VerifyXml(doc));

			doc.LoadXml(tamperedXml);
			AssertEquals(false, SignedMessage.VerifyXml(doc));
		}
	}
}
