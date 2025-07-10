using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Messaging.Testing;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestsSubclassesOf(typeof(XMLMessageBuilder<IESEDIMessageCollectionProvider, object>))]
public abstract class XMLMessageBuilderTest<TMessageBuilder, TProvider, T> : MessageBuilderTest<TMessageBuilder, TProvider>
	where TProvider : class, IESEDIMessageCollectionProvider
	where TMessageBuilder : XMLMessageBuilder<TProvider, T>
{
	protected sealed override void AssertUnsignedMessageText(ZString messageText)
	{
		var testFileContent = GetUnsignedMessageTestFileContent();
#if NET
		testFileContent = testFileContent.Replace("<q1:", "<").Replace("</q1:", "</").Replace("xmlns:q1", "xmlns");
#endif
		AssertContains(testFileContent.Trim(), messageText);
	}

	protected sealed override void AssertSignedMessageText(ZString messageText)
	{
		AssertContains(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>", messageText);

		var testFileContent = GetSignedMessageTestFileContent();
#if NET
		testFileContent = testFileContent.Replace("<q1:", "<").Replace("</q1:", "</").Replace("xmlns:q1", "xmlns");
#endif
		AssertContains(testFileContent.Trim(), messageText);

		AssertEndsWith("soapenv end", @"
  </soapenv:Body>
</soapenv:Envelope>", messageText);
	}

	protected abstract ZString GetSignedMessageTestFileContent();
	protected abstract ZString GetUnsignedMessageTestFileContent();

	protected string GetTestFileContents(string testFilePath, string fileName)
	{
		return TestFileReader.GetEmbeddedFileText(testFilePath, fileName);
	}

	protected virtual TestFileReader TestFileReader => testFileReader ?? (testFileReader = new TestFileReader(typeof(EDIFACTMessageBuilderTest<,,>)));
	TestFileReader testFileReader;

	protected Mock<TProvider> mockProvider;

	protected override void SetUp()
	{
		mockProvider = new Mock<TProvider>();
		mockProvider.Setup(m => m.Factory).Returns(Factory);
		mockProvider.Setup(m => m.IsTest).Returns(ZBool.True);
		mockProvider.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory));
		mockProvider.Setup(m => m.BrokerCode).Returns("AZ");
		mockProvider.Setup(m => m.CertificateName).Returns("CertName");
		mockProvider.Setup(m => m.CertificateThumbPrint).Returns("CertThumbPrint");
		mockProvider.Setup(m => m.CertificateBytes).Returns(BuilderHelperTest.GetCertificateBytes());
		mockProvider.Setup(m => m.DecryptedCertificatePassphrase).Returns(BuilderHelperTest.CertificatePassword);
		mockProvider.Setup(m => m.BusinessObjectReference).Returns("Reference");

		var certificate = Factory.New<MasterFiles.Business.GlbExternalPassword>();
		mockProvider.Setup(m => m.CertificatePK).Returns(certificate.PK);
	}

	protected TMessageBuilder MockRandomGenerator(TMessageBuilder messageBuilder)
	{
		RandomGeneratorHelper.MockRandomGenerator(messageBuilder, 4560);
		return messageBuilder;
	}
}
