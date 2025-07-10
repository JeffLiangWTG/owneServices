using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5ULEntryLine
	{
		ZInt RefundLineNo { get; }
		ZString ImportDeclarationNumber { get; }
		ZString SoABillNumber { get; }
		ZInt VersionNumber5WN { get; }
		ZDate VATDecisionDate { get; }
		ZInt ImportEntryLineNo { get; }
		ZString CancelReasonCode { get; }
		IEnumerable<IImport5ULTaxItem> TaxItems { get; }
		IEnumerable<IImport5ULTaxItem> OtherTaxItems { get; }
		ZDecimal TotalOtherTaxItemAmount { get; }
		ZString ExportDeclarationNumber { get; }
		ZInt ExportEntryLineNo { get; }
		ZString DisposalNumber { get; }
		ZDate DisposalDate { get; }
		ZString GoodsLocationDescription { get; }
		ZString ResidualSubstanceDescription { get; }
		ZString DamageSituation { get; }
		IEnumerable<IImport5ULInvoiceLine> InvoiceLines { get; }
	}
}
