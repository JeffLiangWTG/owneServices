using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class DocumentManagementServiceRequestContextBaseOnlyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var glbCertificateProviderMock = GetGlbCertificateProviderMock();

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null", () => new Ucc6ElectronicFolderStatusRequestContext(header: null, glbCertificateProviderMock.Object));
			AssertExceptionThrown<ArgumentNullException>("When glbCertificateProvider is null", () => new Ucc6ElectronicFolderStatusRequestContext(entryHeader, glbCertificateProvider: null));
		});
	}

	public void TestXmlSigner()
	{
		var requestContext = GetDocumentManagementServiceRequestContext();
		AssertNotNull(nameof(requestContext.XmlSigner), requestContext.XmlSigner);
		AssertType<AidaXmlSigner>($"{nameof(requestContext.XmlSigner)} Type", requestContext.XmlSigner);
	}

	public void TestCryptokiCertificate()
	{
		var requestContext = GetDocumentManagementServiceRequestContext();
		AssertNotNull(nameof(requestContext.CryptokiCertificate), requestContext.CryptokiCertificate);
		AssertEquals(nameof(ICryptokiGlbExternalPassword.GP_Name), "BIT4ID", requestContext.CryptokiCertificate.GP_Name);
	}

	public void TestMauCertificate()
	{
		var requestContext = GetDocumentManagementServiceRequestContext();
		AssertNotNull(nameof(requestContext.CryptokiCertificate), requestContext.CryptokiCertificate);
	}

	public void TestBusinessObject()
	{
		var requestContext = GetDocumentManagementServiceRequestContext();
		AssertNotNull(nameof(requestContext.BusinessObject), requestContext.BusinessObject);
		Assert("Parent Header Ref", object.ReferenceEquals(entryHeader, requestContext.BusinessObject));
	}

	public void TestMessageParent()
	{
		var requestContext = GetDocumentManagementServiceRequestContext();
		AssertNotNull(nameof(requestContext.MessageParent), requestContext.MessageParent);
		Assert("Message Parent", object.ReferenceEquals(entryHeader, requestContext.MessageParent));
	}

	public void TestServiceId()
	{
		var requestContext = GetDocumentManagementServiceRequestContext();
		AssertEquals(nameof(requestContext.ServiceId), "ServiceIdForTest", requestContext.ServiceId);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_CustomsProfile = "1234-DEC1";
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	IDocumentManagementServiceRequestContext<CusEntryHeader, CusEntryHeader> GetDocumentManagementServiceRequestContext()
	{
		var glbCertificateProviderMock = GetGlbCertificateProviderMock();
		return new DocumentManagementServiceRequestContextForTest(entryHeader, glbCertificateProviderMock.Object);
	}

	Mock<IGlbCertificateProvider> GetGlbCertificateProviderMock()
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid);

		var cryptokeiCertificateMock = new Mock<ICryptokiGlbExternalPassword>();
		cryptokeiCertificateMock.Setup(c => c.GP_Name).Returns("BIT4ID");
		cryptokeiCertificateMock.Setup(c => c.GP_CertificateSerialNumber).Returns("SR123");
		cryptokeiCertificateMock.Setup(c => c.TokenPinStore.GetPin()).Returns("PIN1");

		var glbCertificateProvider = new Mock<IGlbCertificateProvider>();
		glbCertificateProvider.Setup(c => c.GetCryptokiCertificate()).Returns(cryptokeiCertificateMock.Object);
		glbCertificateProvider.Setup(c => c.GetMauCertificatePassword(It.IsAny<string>())).Returns(mauCertificateMock.Object);

		return glbCertificateProvider;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	class DocumentManagementServiceRequestContextForTest : DocumentManagementServiceRequestContext<CusEntryHeader>
	{
		public DocumentManagementServiceRequestContextForTest(CusEntryHeader entryHeader, IGlbCertificateProvider glbCertificateProvider) : base(entryHeader, glbCertificateProvider)
		{
		}

		protected override string ServiceId => "ServiceIdForTest";
	}
}
