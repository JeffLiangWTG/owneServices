using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5ULEntryLine : IImport5ULEntryLine
	{
		public int RefundLineNo { get; set; }
		public string ImportDeclarationNumber { get; set; }
		public string SoABillNumber { get; set; }
		public int VersionNumber5WN { get; set; }
		public DateTime VATDecisionDate { get; set; }
		public int ImportEntryLineNo { get; set; }
		public string CancelReasonCode { get; set; }
		public Import5ULTaxItem[] TaxItems { get; set; }
		public Import5ULTaxItem[] OtherTaxItems { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.DutyTaxFee)]
		public decimal TotalOtherTaxItemAmount { get; set; }
		public string ExportDeclarationNumber { get; set; }
		public int ExportEntryLineNo { get; set; }
		public string DisposalNumber { get; set; }
		public DateTime DisposalDate { get; set; }
		public string GoodsLocationDescription { get; set; }
		public string ResidualSubstanceDescription { get; set; }
		public string DamageSituation { get; set; }
		public Import5ULInvoiceLine[] InvoiceLines { get; set; }

		ZInt IImport5ULEntryLine.RefundLineNo => RefundLineNo;
		ZString IImport5ULEntryLine.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImport5ULEntryLine.SoABillNumber => SoABillNumber;
		ZInt IImport5ULEntryLine.VersionNumber5WN => VersionNumber5WN;
		ZDate IImport5ULEntryLine.VATDecisionDate => (ZDate)VATDecisionDate;
		ZInt IImport5ULEntryLine.ImportEntryLineNo => ImportEntryLineNo;
		ZString IImport5ULEntryLine.CancelReasonCode => CancelReasonCode;
		IEnumerable<IImport5ULTaxItem> IImport5ULEntryLine.TaxItems => TaxItems;
		IEnumerable<IImport5ULTaxItem> IImport5ULEntryLine.OtherTaxItems => OtherTaxItems;
		ZDecimal IImport5ULEntryLine.TotalOtherTaxItemAmount => TotalOtherTaxItemAmount;
		ZString IImport5ULEntryLine.ExportDeclarationNumber => ExportDeclarationNumber;
		ZInt IImport5ULEntryLine.ExportEntryLineNo => ExportEntryLineNo;
		ZString IImport5ULEntryLine.DisposalNumber => DisposalNumber;
		ZDate IImport5ULEntryLine.DisposalDate => (ZDate)DisposalDate;
		ZString IImport5ULEntryLine.GoodsLocationDescription => GoodsLocationDescription;
		ZString IImport5ULEntryLine.ResidualSubstanceDescription => ResidualSubstanceDescription;
		ZString IImport5ULEntryLine.DamageSituation => DamageSituation;
		IEnumerable<IImport5ULInvoiceLine> IImport5ULEntryLine.InvoiceLines => InvoiceLines;
	}
}
