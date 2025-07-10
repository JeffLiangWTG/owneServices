using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext))]
sealed class TemporaryStorageHeaderUniqueTransactionIdentifierRequestContextTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null",
				() => new TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext(null,
				certificateProvider,
				"000",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentNullException>("When certificateProvider is null",
				() => new TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext(header,
				certificateProvider: null,
				"000",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentException>("When uniqueTransactionID is empty",
				() => new TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext(header,
				certificateProvider,
				"",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentException>("When emMessageNum is empty",
				() => new TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext(header,
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
		AssertSame(nameof(requestContext.RequestParent), header, requestContext.RequestParent);
	}

	public void TestMessageNumber()
	{
		var requestContext = GetUniqueTransactionIdentifierRequestContext();
		AssertEquals(nameof(requestContext.MessageNumber), "ABCDEF123456", requestContext.MessageNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		certificateProvider = GetCertificateProvider();
	}

	TemporaryStorageHeader header;
	IGlbCertificateProvider certificateProvider;

	IUniqueTransactionIdentifierRequestContext GetUniqueTransactionIdentifierRequestContext()
		=> new TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext(header, certificateProvider, "20220307D11000328189", "ABCDEF123456");

	IGlbCertificateProvider GetCertificateProvider()
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid);

		var glbCertificateProvider = new Mock<IGlbCertificateProvider>();
		glbCertificateProvider.Setup(c => c.GetMauCertificatePassword(It.IsAny<string>())).Returns(mauCertificateMock.Object);

		return glbCertificateProvider.Object;
	}
}
