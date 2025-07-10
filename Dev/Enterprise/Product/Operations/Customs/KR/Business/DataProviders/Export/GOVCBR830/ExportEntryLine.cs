using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ExportEntryLine : IExportEntryLine
	{
		public string EntryLineNo { get; set; }
		public string HSCode { get; set; }
		public string HSDescription { get; set; }
		public string TradeName { get; set; }
		public string BrandName { get; set; }
		public string InvoiceNo { get; set; }
		public string CountryOfOrigin { get; set; }
		public string CountryOfOriginDeterminationRule { get; set; }
		public string CountryOfOriginLabelLocation { get; set; }
		public string CertificateOfOriginIssued { get; set; }
		public string FTAType { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal NetWeightInKG { get; set; }
		public string NetWeightUQ { get; set; }
		public string QtyUnit { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Qty)]
		public decimal Qty { get; set; }
		public int PackQty { get; set; }
		public string PackType { get; set; }
		public string DocumentAttached { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public decimal CustomsValue { get; set; }
		public string ImportDeclarationNumber { get; set; }
		public string ImportEntryLineNo { get; set; }
		public string SkipManifestReporting { get; set; }
		public ExportInvoiceLine[] InvoiceLines { get; set; }
		public string PreApprovalType { get; set; }
		public string PreApprovalNo { get; set; }
		public DateTime PreApprovalEffectiveFromDate { get; set; }
		public DateTime PreApprovalEffectiveToDate { get; set; }

		ZInt IEntryLine.EntryLineNo => Convert.ToInt32(EntryLineNo);
		ZString IExportEntryLine.EntryLineNo => EntryLineNo;
		ZString IExportEntryLine.HSCode => HSCode;
		ZString IExportEntryLine.HSDescription => HSDescription;
		ZString IExportEntryLine.TradeName => TradeName;
		ZString IExportEntryLine.BrandName => BrandName;
		ZString IExportEntryLine.InvoiceNo => InvoiceNo;
		ZString IExportEntryLine.CountryOfOrigin => CountryOfOrigin;
		ZString IExportEntryLine.CountryOfOriginDeterminationRule => CountryOfOriginDeterminationRule;
		ZString IExportEntryLine.CountryOfOriginLabelLocation => CountryOfOriginLabelLocation;
		ZString IExportEntryLine.CertificateOfOriginIssued => CertificateOfOriginIssued;
		ZString IExportEntryLine.FTAType => FTAType;
		ZDecimal IExportEntryLine.NetWeightInKG => NetWeightInKG;
		ZString IExportEntryLine.NetWeightUQ => NetWeightUQ;
		ZString IExportEntryLine.QtyUnit => QtyUnit;
		ZDecimal IExportEntryLine.Qty => Qty;
		ZInt IExportEntryLine.PackQty => PackQty;
		ZString IExportEntryLine.PackType => PackType;
		ZString IExportEntryLine.DocumentAttached => DocumentAttached;
		ZDecimal IExportEntryLine.CustomsValue => CustomsValue;
		ZString IExportEntryLine.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IExportEntryLine.ImportEntryLineNo => ImportEntryLineNo;
		ZString IExportEntryLine.SkipManifestReporting => SkipManifestReporting;
		IEnumerable<IExportInvoiceLine> IExportEntryLine.InvoiceLines => InvoiceLines;
		IEnumerable<IInvoiceLine> IEntryLine.InvoiceLines => InvoiceLines;
		ZString IExportEntryLine.PreApprovalType => PreApprovalType;
		ZString IExportEntryLine.PreApprovalNo => PreApprovalNo;
		ZDate IExportEntryLine.PreApprovalEffectiveFromDate => (ZDate)PreApprovalEffectiveFromDate;
		ZDate IExportEntryLine.PreApprovalEffectiveToDate => (ZDate)PreApprovalEffectiveToDate;
	}
}
