using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportDHRInvoiceLine
	{
		ZInt EntryLineNo { get; }
		[ID()]
		ZInt InvoiceLineNo { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._41)]
		ZString CertificateOfOriginNo { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._42)]
		ZInt CertificateOfOriginSeqNo { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._43)]
		ZDecimal CertificateOfOriginUsedQuantity { get; }
		[DataItemID(FTAAmendmentDataItemIDList.Codes._44)]
		ZString CertificateOfOriginUsedUQ { get; }
	}
}
