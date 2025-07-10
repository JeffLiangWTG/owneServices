using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		internal static DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper NewOrNull(JobComInvoiceLine invoiceLine)
			=> invoiceLine == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtWrapper(invoiceLine);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.CustomsBookType => CodeTypeWrapper.NewOrNull(Constants.CustomsDeclaration.DmExtensionsCustomsBookTypeImport);

		decimal? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.DeferredCustomsTax => null;

		decimal? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.DeferredCustomsTaxValue => null;

		decimal? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.DeferredPurchaseTax => null;

		decimal? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.DeferredPurchaseTaxValue => null;

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount> IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.GoodsItemAmount
			=> new List<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount> {
				DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper
				.NewOrNull(
					Constants.JobDeclaration.BaseAmount,
					invoiceLine.CusEntryLine.CL_InvoiceAmount,
					invoiceLine.CusEntryLine.CL_RX_NKInvoiceAmountCurrency)
			}.WhereNotNull().ToCollection();

		string IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.InvoiceLineNumbers => null;

		bool? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.IsUsed => null;

		bool? IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.IsUsedValue => null;

		IOptionalTamaType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.OptionalTama => null;

		IIDType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.PreferenceDocumentNumber => null;

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.SalesTaxExemptionType => null;

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.TaxExemptCode => null;

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtVehicleProductIdentification> IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.Vehicle => null;

		ICollection<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtVehicleValuationAdjustment> IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExt.VehicleValuationAdjustment => null;

		readonly JobComInvoiceLine invoiceLine;
	}
}
