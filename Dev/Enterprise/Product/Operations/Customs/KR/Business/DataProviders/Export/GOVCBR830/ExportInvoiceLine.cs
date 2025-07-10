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
	public class ExportInvoiceLine : IExportInvoiceLine
	{
		public string InvoiceLineNo { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.QtyOrWeight)]
		public decimal QtyOrWeight { get; set; }
		public string QtyOrWeightUnit { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.UnitPrice)]
		public decimal UnitPrice { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Amount)]
		public decimal Amount { get; set; }
		public string Ingredient { get; set; }
		public string DetailDescription { get; set; }
		public string LotNumber { get; set; }
		public ExportGAApprovalDocument[] GAApprovalDocuments { get; set; }
		public ExportVehicleNo[] VehicleNumbers { get; set; }

		ZString IExportInvoiceLine.InvoiceLineNo => InvoiceLineNo;
		ZInt IInvoiceLine.InvoiceLineNo => Convert.ToInt32(InvoiceLineNo);
		ZDecimal IExportInvoiceLine.QtyOrWeight => QtyOrWeight;
		ZString IExportInvoiceLine.QtyOrWeightUnit => QtyOrWeightUnit;
		ZDecimal IExportInvoiceLine.UnitPrice => UnitPrice;
		ZDecimal IExportInvoiceLine.Amount => Amount;
		ZString IExportInvoiceLine.Ingredient => Ingredient;
		ZString IExportInvoiceLine.DetailDescription => DetailDescription;
		ZString IExportInvoiceLine.LotNumber => LotNumber;
		IEnumerable<IExportGAApprovalDocument> IExportInvoiceLine.GAApprovalDocuments => GAApprovalDocuments;
		IEnumerable<IExportVehicleNo> IExportInvoiceLine.VehicleNumbers => VehicleNumbers;
	}
}
