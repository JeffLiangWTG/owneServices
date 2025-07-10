using System.Collections.Generic;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using static Enterprise.Customs.CA.Business.MessageBuilders.B3ImportMessageWrapper;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	internal class CADLVSGoodsShipmentWrapper : ICADMessageDeclarationGoodsShipment
	{
		internal CADLVSGoodsShipmentWrapper(IB3SubHeader b3SubHeader, MessageSubTypes messageSubType)
		{
			this.B3SubHeader = b3SubHeader;
			this.messageSubType = messageSubType;
			if (b3SubHeader is B3SubHeader subHeader)
			{
				this.invoiceHeader = subHeader.InvoiceHeader;
			}
			this.lines = B3SubHeader.GetB3SubHeaderLines();
		}
		IB3SubHeader B3SubHeader { get; }
		readonly MessageSubTypes messageSubType;
		readonly JobComInvoiceHeader invoiceHeader;
		readonly IEnumerable<IClassificationLine1> lines;

		string ICADMessageDeclarationGoodsShipment.ExitDateTime => ZString.Empty;

		uint ICADMessageDeclarationGoodsShipment.SequenceNumeric
		{
			get
			{
				return System.Convert.ToUInt32(B3SubHeader.B3SubHeaderNumber);
			}
		}

		ICADMessageDeclarationGoodsShipmentBuyer ICADMessageDeclarationGoodsShipment.Buyer
		{
			get
			{
				if (buyer == null && invoiceHeader != null && invoiceHeader.JZ_OH_Buyer != invoiceHeader.JobDeclaration?.JE_OH_Importer)
				{
					buyer = new CADDeclarationGoodsShipmentBuyer(invoiceHeader.BuyerDocumentaryAddress);
				}
				return buyer;
			}
		}
		ICADMessageDeclarationGoodsShipmentBuyer buyer;

		string ICADMessageDeclarationGoodsShipment.ExitOfficeID => "1001";

		ICADMessageRegion ICADMessageDeclarationGoodsShipment.ExportCountry
		{
			get
			{
				if (exportCountry == null)
				{
					var tariffTreatmentCode = B3SubHeader.TariffTreatmentCode;
					if (tariffTreatmentCode == TariffTreatmentCodes.Codes.MostFavouredNation
						|| tariffTreatmentCode == TariffTreatmentCodes.Codes.UnitedStates
						|| tariffTreatmentCode == TariffTreatmentCodes.Codes.CanadaIsraelAgreement)
					{
						exportCountry = new CADRegion(Core.Constants.CountryCodes.UnitedStates, USStatesList.Codes.NewYork);
					}
					else if (invoiceHeader != null)
					{
						exportCountry = new CADRegion(invoiceHeader.CA_RN_NKExport, invoiceHeader.CA_USStateOfExport);
					}
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
				if (seller == null && invoiceHeader != null)
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
				foreach (var line in lines)
				{
					if (line != null)
					{
						yield return new CADGoodsShipmentCommodityWrapper(line, messageSubType, false);
					}
				}
			}
		}
	}
}
