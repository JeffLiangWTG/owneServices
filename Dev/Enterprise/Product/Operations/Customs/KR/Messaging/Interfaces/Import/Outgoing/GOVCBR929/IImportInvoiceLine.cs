using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportInvoiceLine : IInvoiceLine
	{
		[ID()]
		new ZInt InvoiceLineNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.C102)]
		ZString ItemDescription { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.C103)]
		ZString Ingredient { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.C106)]
		ZString InvoiceUnitOfQuantiy { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.C105)]
		ZDecimal InvoiceQuantity { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.C107)]
		ZDecimal UnitPrice { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.C108)]
		ZDecimal Amount { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.C104)]
		ZString PartNumber { get; }
		IEnumerable<IImportGAApprovalDocument> GAApprovalDocuments { get; }
	}
}
