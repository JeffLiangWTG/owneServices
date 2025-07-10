using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsUniqueTransactionIdentifierRequestContextTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null",
				() => new NctsUniqueTransactionIdentifierRequestContext(null,
				certificateProvider,
				"000",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentNullException>("When certificateProvider is null",
				() => new NctsUniqueTransactionIdentifierRequestContext(header,
				certificateProvider: null,
				"000",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentException>("When uniqueTransactionID is empty",
				() => new NctsUniqueTransactionIdentifierRequestContext(header,
				certificateProvider,
				"",
				"ABCDEF123456"));

			AssertExceptionThrown<ArgumentException>("When emMessageNum is empty",
				() => new NctsUniqueTransactionIdentifierRequestContext(header,
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
		AssertSame(nameof(requestContext.RequestParent), movementHeader, requestContext.RequestParent);
	}

	public void TestMessageNumber()
	{
		var requestContext = GetUniqueTransactionIdentifierRequestContext();
		AssertEquals(nameof(requestContext.MessageNumber), "ABCDEF123456", requestContext.MessageNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = "NC5";
		movementHeader = header.MovementHeader;
		certificateProvider = GetCertificateProvider();
	}

	NctsHeader header;
	NctsDepartureMovementHeader movementHeader;
	IGlbCertificateProvider certificateProvider;

	IUniqueTransactionIdentifierRequestContext GetUniqueTransactionIdentifierRequestContext()
		=> new NctsUniqueTransactionIdentifierRequestContext(header, certificateProvider, "20220307D11000328189", "ABCDEF123456");

	IGlbCertificateProvider GetCertificateProvider()
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid);

		var glbCertificateProvider = new Mock<IGlbCertificateProvider>();
		glbCertificateProvider.Setup(c => c.GetMauCertificatePassword(It.IsAny<string>())).Returns(mauCertificateMock.Object);

		return glbCertificateProvider.Object;
	}
}
