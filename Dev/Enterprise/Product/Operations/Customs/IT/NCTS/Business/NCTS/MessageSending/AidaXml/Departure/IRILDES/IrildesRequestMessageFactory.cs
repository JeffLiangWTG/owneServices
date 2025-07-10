using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public class IrildesRequestMessageFactory
{
	public void CreateMessage(NctsHeader nctsHeader)
	{
		var irildesRequestContext = CreateIrildesRequestContext(nctsHeader);
		IOutgoingCustomsMessageCreationStrategy messageGenerator = new IrildesRequestMessageCreationStrategy(irildesRequestContext);
		_ = messageGenerator.GenerateMessage();
	}

	protected virtual IIrildesRequestContext CreateIrildesRequestContext(NctsHeader nctsHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new IrildesRequestContext(nctsHeader, glbCertificateProvider);
	}
}
