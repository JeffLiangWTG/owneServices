using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	public interface ILocalExportEntryLine : IEntryLine
	{
		[ID()]
		new ZInt EntryLineNo { get; }
		[DataItemID("23")]
		ZString HSCode { get; }
		[DataItemID("21")]
		ZString InvoiceDescription { get; }
		[DataItemID("22")]
		ZString GoodsNo { get; }
		[DataItemID("24A")]
		[DecimalPlaces(DecimalPlacesConstants.LocalExportInvoiceQuantity)]
		ZDecimal Quantity { get; }
		[DataItemID("24B")]
		ZString QuantityUnit { get; }
		[DataItemID("27")]
		ZInt Packages { get; }
		[DataItemID("28")]
		ZString PackagesType { get; }
		[DataItemID("25")]
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		ZDecimal NetWeight { get; }
		[DataItemID("26")]
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		ZDecimal FOBAmount { get; }
		[DataItemID("30")]
		ZString DocumentNo { get; }
		[DataItemID("29")]
		ZString DocumentType { get; }
		[DataItemID("31")]
		ZDate InboundDate { get; }
		[DataItemID("32A")]
		ZString PreviousTransactionReferenceNo { get; }
		[DataItemID("32B")]
		ZString PreviousTransactionReferenceNoType { get; }
		[DataItemID("37")]
		ZString MaterialCode { get; }
	}
}
