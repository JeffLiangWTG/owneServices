using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

public sealed class IrildesRequestMessageFactoryForTest : IrildesRequestMessageFactory
{
	public IrildesRequestMessageFactoryForTest()
	{
	}

	protected override IIrildesRequestContext CreateIrildesRequestContext(NctsHeader nctsHeader)
	{
		var provider = new GlbCertificateProvider();
		var mauCertificate = provider.GetMauCertificatePassword("1234");

		var contextMock = new Mock<IIrildesRequestContext>();
		contextMock.Setup(ctx => ctx.NctsHeader).Returns(nctsHeader);
		contextMock.Setup(ctx => ctx.MauCertificate).Returns(mauCertificate);
		return contextMock.Object;
	}
}
