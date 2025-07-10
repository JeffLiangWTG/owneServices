using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public interface IIvistoRequestContext
{
	IGlbMauExternalPassword MauCertificate { get; }

	CusEntryHeader EntryHeader { get; }
}
