using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class IrildesRequestContextTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var glbCertificateProviderMock = GlbCertificateProviderTestUtil.GetGlbCertificateProviderMock();

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When header is null", () => new IrildesRequestContext(header: null, glbCertificateProviderMock.Object));
			AssertExceptionThrown<ArgumentNullException>("When glbCertificateProvider is null", () => new IrildesRequestContext(nctsHeader, glbCertificateProvider: null));
		});
	}

	public void TestMauCertificate()
	{
		var requestContext = GetIrildesRequestContext();

		var mauCertificate = requestContext.MauCertificate;
		AssertNotNull("MauCertificate", mauCertificate);
		AssertEquals("MauCertificate DeclarantTaxNumber", "ABCDE", mauCertificate.DeclarantTaxNumber);
	}

	public void TestNctsHeader()
	{
		var requestContext = GetIrildesRequestContext();

		var nctsHeader = requestContext.NctsHeader;
		AssertNotNull("NctsHeader", nctsHeader);
		AssertSame("NctsHeader Reference", this.nctsHeader, nctsHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
	}

	IIrildesRequestContext GetIrildesRequestContext()
	{
		var glbCertificateProviderMock = GlbCertificateProviderTestUtil.GetGlbCertificateProviderMock();
		return new IrildesRequestContext(nctsHeader, glbCertificateProviderMock.Object);
	}

	NctsHeader nctsHeader;
}
