using System.Collections.Generic;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	internal class CADGoodsShipmentWrapper : ICADMessageDeclarationGoodsShipment
	{
		internal CADGoodsShipmentWrapper(JobComInvoiceHeader header, MessageSubTypes messageSubType, bool isWarehouse)
		{
			this.invoiceHeader = header;
			this.declaration = header.JobDeclaration;
			this.messageSubType = messageSubType;
			this.isWarehouse = isWarehouse;
		}

		readonly JobComInvoiceHeader invoiceHeader;
		readonly JobDeclaration declaration;
		readonly MessageSubTypes messageSubType;
		readonly bool isWarehouse;

		string ICADMessageDeclarationGoodsShipment.ExitDateTime
		{
			get
			{
				var result = ZString.Empty;
				if (!declaration.IsWarehouseNotInType && messageSubType != MessageSubTypes.Change && messageSubType != MessageSubTypes.Amend)
				{
					result = invoiceHeader.JZ_ValuationDateOverride.ToString("yyyyMMdd");
				}
				return result;
			}
		}

		uint ICADMessageDeclarationGoodsShipment.SequenceNumeric
		{
			get
			{
				return (uint)invoiceHeader.JZ_InvoiceDisplaySequence;
			}
		}

		ICADMessageDeclarationGoodsShipmentBuyer ICADMessageDeclarationGoodsShipment.Buyer
		{
			get
			{
				var invoice = invoiceHeader;
				if (buyer == null && invoice.JZ_OH_Buyer != invoice.JobDeclaration?.JE_OH_Importer)
				{
					buyer = new CADDeclarationGoodsShipmentBuyer(invoiceHeader.BuyerDocumentaryAddress);
				}
				return buyer;
			}
		}
		ICADMessageDeclarationGoodsShipmentBuyer buyer;

		string ICADMessageDeclarationGoodsShipment.ExitOfficeID
		{
			get
			{
				var result = ZString.Empty;
				if (!declaration.IsWarehouseNotInType)
				{
					result = invoiceHeader.CA_USPortOfExit;
				}
				return result;
			}
		}

		ICADMessageRegion ICADMessageDeclarationGoodsShipment.ExportCountry
		{
			get
			{
				if (exportCountry == null)
				{
					exportCountry = new CADRegion(invoiceHeader.CA_RN_NKExport, invoiceHeader.CA_USStateOfExport);
				}
				return exportCountry;
			}
		}
		ICADMessageRegion exportCountry;

		IEnumerable<ICADMessageDeclarationGoodsShipmentInvoice> ICADMessageDeclarationGoodsShipment.Invoice
		{
			get
			{
				return new[] { new CADDeclarationGoodsShipmentInvoice(invoiceHeader) };
			}
		}

		ICADMessageDeclarationGoodsShipmentSeller ICADMessageDeclarationGoodsShipment.Seller
		{
			get
			{
				if (seller == null)
				{
					seller = new CADDeclarationGoodsShipmentSeller(invoiceHeader.SupplierDocumentaryAddress);
				}
				return seller;
			}
		}
		ICADMessageDeclarationGoodsShipmentSeller seller;

		IEnumerable<ICADDeclarationGoodsShipmentCommodity> ICADMessageDeclarationGoodsShipment.GovernmentAgencyGoodsItem
		{
			get
			{
				foreach (JobComInvoiceLine line in invoiceHeader.JobComInvoiceLines)
				{
					yield return (new CADGoodsShipmentCommodityWrapper(line.B3EntryLine, messageSubType, isWarehouse));
				}
			}
		}
	}
}
