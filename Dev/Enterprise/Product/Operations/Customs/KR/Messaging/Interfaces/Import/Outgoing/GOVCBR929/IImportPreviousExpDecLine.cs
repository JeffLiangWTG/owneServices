using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportPreviousExpDecLine
	{
		[ID()]
		[DataItemID(ImportAmendmentDataItemIDList.Codes.G102)]
		ZString DeclarationNumber { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.G101)]
		ZInt EntryLineNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.G103)]
		ZInt InvoiceLineNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.G105)]
		ZString UQ { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.G104)]
		ZDecimal UsedQty { get; }
		ZShort SequenceNumber { get; }
	}
}
