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
	public class LocalExportEntryLine : ILocalExportEntryLine
	{
		public int EntryLineNo { get; set; }
		public string HSCode { get; set; }
		public string InvoiceDescription { get; set; }
		public string GoodsNo { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.LocalExportInvoiceQuantity)]
		public decimal Quantity { get; set; }
		public string QuantityUnit { get; set; }
		public int Packages { get; set; }
		public string PackagesType { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public decimal NetWeight { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public decimal FOBAmount { get; set; }
		public string DocumentNo { get; set; }
		public string DocumentType { get; set; }
		public DateTime InboundDate { get; set; }
		public string PreviousTransactionReferenceNo { get; set; }
		public string PreviousTransactionReferenceNoType { get; set; }
		public string MaterialCode { get; set; }

		ZInt IEntryLine.EntryLineNo => EntryLineNo;
		ZInt ILocalExportEntryLine.EntryLineNo => EntryLineNo;
		ZString ILocalExportEntryLine.HSCode => HSCode;
		ZString ILocalExportEntryLine.InvoiceDescription => InvoiceDescription;
		ZString ILocalExportEntryLine.GoodsNo => GoodsNo;
		ZDecimal ILocalExportEntryLine.Quantity => Quantity;
		ZString ILocalExportEntryLine.QuantityUnit => QuantityUnit;
		ZInt ILocalExportEntryLine.Packages => Packages;
		ZString ILocalExportEntryLine.PackagesType => PackagesType;
		ZDecimal ILocalExportEntryLine.NetWeight => NetWeight;
		ZDecimal ILocalExportEntryLine.FOBAmount => FOBAmount;
		ZString ILocalExportEntryLine.DocumentNo => DocumentNo;
		ZString ILocalExportEntryLine.DocumentType => DocumentType;
		ZDate ILocalExportEntryLine.InboundDate => new ZDate(InboundDate);
		ZString ILocalExportEntryLine.PreviousTransactionReferenceNo => PreviousTransactionReferenceNo;
		ZString ILocalExportEntryLine.PreviousTransactionReferenceNoType => PreviousTransactionReferenceNoType;
		ZString ILocalExportEntryLine.MaterialCode => MaterialCode;
		IEnumerable<IInvoiceLine> IEntryLine.InvoiceLines => Array.Empty<IInvoiceLine>();
	}
}
