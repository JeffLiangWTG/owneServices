using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public interface IIrildesRequestContext
{
	IGlbMauExternalPassword MauCertificate { get; }

	NctsHeader NctsHeader { get; }
}
