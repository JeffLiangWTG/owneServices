using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItem
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItem NewOrNull(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemWrapper(invoiceLine);

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument> IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.AdditionalDocument
			=> invoiceLine.Permits
				.Select(permit => DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocumentWrapper.NewOrNull(permit))
				.WhereNotNull()
				.ToList().AsReadOnly();

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Commodity => DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityWrapper.NewOrNull(invoiceLine);

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.DmExtensions => DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper.NewOrNull(invoiceLine);

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure> IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.GoodsMeasure
			=> new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureProvider(invoiceLine).GetGoodsMeasure();

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturer IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Manufacturer => DeclarationGoodsShipmentGovernmentAgencyGoodsItemManufacturerWrapper.NewOrNull(invoiceLine);

		IDeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Origin => DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper.NewOrNull(invoiceLine);

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemPreviousDocument> IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.PreviousDocument => null;

		decimal? IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.SequenceNumeric => invoiceLine.CusEntryLine.CL_LineNumber;

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment> IDeclarationGoodsShipmentGovernmentAgencyGoodsItem.ValuationAdjustment => invoiceLine.Charges.Cast<InvoiceLineCharge>().Select(charge => DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper.NewOrNull(charge)).ToArray();

		readonly JobComInvoiceLine invoiceLine;
	}
}
