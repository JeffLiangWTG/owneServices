using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportEntryLine : IEntryLine
	{
		[ID()]
		new ZString EntryLineNo { get; }
		[DataItemID("B104")]
		ZString HSCode { get; }
		[DataItemID("B101")]
		ZString HSDescription { get; }
		[DataItemID("B102")]
		ZString TradeName { get; }
		[DataItemID("B103")]
		ZString BrandName { get; }
		[DataItemID("B105")]
		ZString InvoiceNo { get; }
		[DataItemID("B501")]
		ZString CountryOfOrigin { get; }
		[DataItemID("B502")]
		ZString CountryOfOriginDeterminationRule { get; }
		[DataItemID("B503")]
		ZString CountryOfOriginLabelLocation { get; }
		[DataItemID("B504")]
		ZString CertificateOfOriginIssued { get; }
		[DataItemID("B505")]
		ZString FTAType { get; }
		[DataItemID("B201")]
		ZDecimal NetWeightInKG { get; }
		[DataItemID("B202")]
		ZString NetWeightUQ { get; }
		[DataItemID("B204")]
		ZString QtyUnit { get; }
		[DataItemID("B203")]
		ZDecimal Qty { get; }
		[DataItemID("B601")]
		ZInt PackQty { get; }
		[DataItemID("B602")]
		ZString PackType { get; }
		[DataItemID("B701")]
		ZString DocumentAttached { get; }
		[DataItemID("B301")]
		ZDecimal CustomsValue { get; }
		[DataItemID("B401")]
		ZString ImportDeclarationNumber { get; }
		[DataItemID("B402")]
		ZString ImportEntryLineNo { get; }
		[DataItemID("B106")]
		ZString SkipManifestReporting { get; }
		new IEnumerable<IExportInvoiceLine> InvoiceLines { get; }
		[DataItemID("H101")]
		ZString PreApprovalType { get; }
		[DataItemID("H102")]
		ZString PreApprovalNo { get; }
		[DataItemID("H103")]
		ZDate PreApprovalEffectiveFromDate { get; }
		[DataItemID("H104")]
		ZDate PreApprovalEffectiveToDate { get; }
	}
}
