using System.Security.Cryptography.X509Certificates;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public class XMLSignatureTest : TestCaseWithFactory
	{
		public void TestSign()
		{
			var testFileReader = new TestFileReader(GetType());
			ZString message = testFileReader.GetEmbeddedFileText(XMLTestFileConstants.TestFilePath, "ENSExampleForSignature.xml");
			XmlDocument xmlForSignature = new XmlDocument();
			xmlForSignature.LoadXml(message);

			var certificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);

			var signedMessage = XMLSignature.Sign(xmlForSignature, certificate);

			AssertContains(@"<Signature Id=""Firma"" xmlns=""http://www.w3.org/2000/09/xmldsig#"">", signedMessage);
			AssertContains(@"CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments"" />", signedMessage);
			AssertContains(@"<SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />", signedMessage);
			AssertContains(@"<Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" /></Transforms>", signedMessage);
		}
		public const string MessageBuilderDirectory = Messaging.Testing.Constants.ProjectRelativePath + @"MessageBuilders\";
	}
}
