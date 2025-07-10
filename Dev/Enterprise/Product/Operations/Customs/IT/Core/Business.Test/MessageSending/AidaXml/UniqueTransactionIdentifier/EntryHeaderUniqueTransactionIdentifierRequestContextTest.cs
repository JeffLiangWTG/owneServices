using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier.Testing;

sealed class EntryHeaderUniqueTransactionIdentifierRequestContextTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null",
				() => new EntryHeaderUniqueTransactionIdentifierRequestContext(entryHeader: null,
				certificateProvider,
				"000",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentNullException>("When header.Declaration is null",
				() => new EntryHeaderUniqueTransactionIdentifierRequestContext(Factory.New<CusEntryHeader>(),
				certificateProvider,
				"000",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentNullException>("When certificateProvider is null",
				() => new EntryHeaderUniqueTransactionIdentifierRequestContext(entryHeader,
				certificateProvider: null,
				"000",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentException>("When uniqueTransactionID is empty",
				() => new EntryHeaderUniqueTransactionIdentifierRequestContext(entryHeader,
				certificateProvider,
				"",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentException>("When emMessageNum is empty",
				() => new EntryHeaderUniqueTransactionIdentifierRequestContext(entryHeader,
				certificateProvider,
				"000",
				""));
		});
	}

	public void TestUniqueTransactionID()
	{
		var requestContext = GetUniqueTransactionIdentifierRequestContext();
		AssertEquals(nameof(requestContext.UniqueTransactionID), "20220307D11000328189", requestContext.UniqueTransactionID);
	}

	public void TestMauCertificate()
	{
		var requestContext = GetUniqueTransactionIdentifierRequestContext();
		AssertNotNull(nameof(requestContext.MauCertificate), requestContext.MauCertificate);
	}

	public void TestRequestParent()
	{
		var requestContext = GetUniqueTransactionIdentifierRequestContext();
		AssertSame(nameof(requestContext.RequestParent), entryHeader, requestContext.RequestParent);
	}

	public void TestMessageNumber()
	{
		var requestContext = GetUniqueTransactionIdentifierRequestContext();
		AssertEquals(nameof(requestContext.MessageNumber), "ABCDEF123456", requestContext.MessageNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		certificateProvider = GetCertificateProvider();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	IGlbCertificateProvider certificateProvider;

	IUniqueTransactionIdentifierRequestContext GetUniqueTransactionIdentifierRequestContext()
		=> new EntryHeaderUniqueTransactionIdentifierRequestContext(entryHeader, certificateProvider, "20220307D11000328189", "ABCDEF123456");

	IGlbCertificateProvider GetCertificateProvider()
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid);

		var glbCertificateProvider = new Mock<IGlbCertificateProvider>();
		glbCertificateProvider.Setup(c => c.GetMauCertificatePassword(It.IsAny<string>())).Returns(mauCertificateMock.Object);

		return glbCertificateProvider.Object;
	}
}
