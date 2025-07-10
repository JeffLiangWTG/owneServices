using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class IvistoRequestContextTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var glbCertificateProviderMock = GetGlbCertificateProviderMock();

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null", () => new IvistoRequestContext(header: null, glbCertificateProviderMock.Object));
			AssertExceptionThrown<ArgumentNullException>("When header.Declaration is null", () => new IvistoRequestContext(Factory.New<CusEntryHeader>(), glbCertificateProviderMock.Object));
			AssertExceptionThrown<ArgumentNullException>("When glbCertificateProvider is null", () => new IvistoRequestContext(entryHeader, glbCertificateProvider: null));
		});
	}

	public void TestMauCertificate()
	{
		var requestContext = GetIvistoRequestContext();

		var mauCertificate = requestContext.MauCertificate;
		AssertNotNull("MauCertificate", mauCertificate);
		AssertEquals("MauCertificate DeclarantTaxNumber", "ABCDE", mauCertificate.DeclarantTaxNumber);
	}

	public void TestEntryHeader()
	{
		var requestContext = GetIvistoRequestContext();

		var entryHeader = requestContext.EntryHeader;
		AssertNotNull("EntryHeader", entryHeader);
		AssertSame("EntryHeader Reference", this.entryHeader, entryHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	IIvistoRequestContext GetIvistoRequestContext()
	{
		var glbCertificateProviderMock = GetGlbCertificateProviderMock();
		return new IvistoRequestContext(entryHeader, glbCertificateProviderMock.Object);
	}

	Mock<IGlbCertificateProvider> GetGlbCertificateProviderMock()
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid);
		mauCertificateMock.Setup(m => m.DeclarantTaxNumber).Returns("ABCDE");

		var glbCertificateProvider = new Mock<IGlbCertificateProvider>();
		glbCertificateProvider.Setup(c => c.GetMauCertificatePassword(It.IsAny<string>())).Returns(mauCertificateMock.Object);

		return glbCertificateProvider;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
