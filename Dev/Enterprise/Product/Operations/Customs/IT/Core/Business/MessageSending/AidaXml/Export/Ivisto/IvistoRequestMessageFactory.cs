using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public class IvistoRequestMessageFactory
{
	public void CreateIvistoRequestMessage(CusEntryHeader entryHeader, BusinessObjectFactory factory)
	{
		var efStatusRequestContext = CreateIvistoRequestContext(entryHeader);
		var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new IvistoRequestMessageCreationStrategy(factory, efStatusRequestContext);
		messageGenerator.GenerateMessage();
	}

	protected virtual IIvistoRequestContext CreateIvistoRequestContext(CusEntryHeader entryHeader)
	{
		var glbCertificateProvider = new GlbCertificateProvider();
		return new IvistoRequestContext(entryHeader, glbCertificateProvider);
	}
}
