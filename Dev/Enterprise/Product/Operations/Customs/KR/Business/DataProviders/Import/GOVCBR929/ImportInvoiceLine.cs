using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportInvoiceLine : IImportInvoiceLine
	{
		public int InvoiceLineNo { get; set; }
		public string ItemDescription { get; set; }
		public string Ingredient { get; set; }
		public string InvoiceUnitOfQuantiy { get; set; }

		[DecimalPlaces(DecimalPlacesConstants.QtyOrWeight)]
		public decimal InvoiceQuantity { get; set; }

		[DecimalPlaces(DecimalPlacesConstants.UnitPrice)]
		public decimal UnitPrice { get; set; }

		[DecimalPlaces(DecimalPlacesConstants.Amount)]
		public decimal Amount { get; set; }
		public string PartNumber { get; set; }
		public ImportGAApprovalDocument[] GAApprovalDocuments { get; set; }

		ZInt IInvoiceLine.InvoiceLineNo => InvoiceLineNo;
		ZInt IImportInvoiceLine.InvoiceLineNo => InvoiceLineNo;
		ZString IImportInvoiceLine.ItemDescription => ItemDescription;
		ZString IImportInvoiceLine.Ingredient => Ingredient;
		ZString IImportInvoiceLine.InvoiceUnitOfQuantiy => InvoiceUnitOfQuantiy;
		ZDecimal IImportInvoiceLine.InvoiceQuantity => InvoiceQuantity;
		ZDecimal IImportInvoiceLine.UnitPrice => UnitPrice;
		ZDecimal IImportInvoiceLine.Amount => Amount;
		ZString IImportInvoiceLine.PartNumber => PartNumber;
		IEnumerable<IImportGAApprovalDocument> IImportInvoiceLine.GAApprovalDocuments => GAApprovalDocuments;
	}
}
