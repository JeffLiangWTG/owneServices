using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

public class IvistoRequestMessageFactoryForTest : IvistoRequestMessageFactory
{
	public IvistoRequestMessageFactoryForTest()
	{
	}

	protected override IIvistoRequestContext CreateIvistoRequestContext(CusEntryHeader entryHeader)
	{
		var provider = new GlbCertificateProvider();
		var mauCertificate = provider.GetMauCertificatePassword("1234");

		var contextMock = new Mock<IIvistoRequestContext>();
		contextMock.Setup(ctx => ctx.EntryHeader).Returns(entryHeader);
		contextMock.Setup(ctx => ctx.MauCertificate).Returns(mauCertificate);
		return contextMock.Object;
	}
}
