using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsInventorySelectionHeader : InventorySelectionHeader
{
	public NctsInventorySelectionHeader(NctsBill parentConsignment) : base(parentConsignment.Header)
	{
		Bill = parentConsignment;
	}

	protected NctsBill Bill { get; }

	protected override void UpdateParentData()
	{
	}

	protected sealed override IWarehouseProductLine CreateProductLineFromWarehouseDataCore(WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine,
		IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity, ZDecimal ratio)
	{
		return CreateGoodsItemProductLineFromWarehouseDataCore(inventoryWrapper, whsReceiveLine, whsBondedWarehouseAttribute, invoiceQuantity, ratio);
	}

	protected virtual NctsDepartureCargoDesc CreateGoodsItemProductLineFromWarehouseDataCore(WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine,
		IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity, ZDecimal ratio)
	{
		var goodsItem = Bill.GoodsItems.AddNew();

		if (inventoryWrapper.relatedOrderLineWrapper is WhsOrderLineWrapper orderLineWrapper)
		{
			goodsItem.BY_BondedWHSOrderNumber = orderLineWrapper.Order.WD_DocketID;
			goodsItem.BY_BondedWHSOrderLineNumber = orderLineWrapper.OrderLine.WE_LineNo;
		}

		goodsItem.BY_BondedWhsQuantity = inventoryWrapper.QuantityToDraw;
		goodsItem.BY_BondedWhsUnitQty = whsBondedWarehouseAttribute.WB_BondedWhsUnitOfQty.IsEmpty ? whsReceiveLine.WE_F3_NKPackType : whsBondedWarehouseAttribute.WB_BondedWhsUnitOfQty;

		goodsItem.BY_WarehouseEntryNumber = whsBondedWarehouseAttribute.WB_EntryKey;
		goodsItem.BY_WarehouseEntryLineNo = whsBondedWarehouseAttribute.WB_EntryLineNo;

		goodsItem.BY_OP_Part = whsReceiveLine.WE_OP;
		goodsItem.BY_Description = inventoryWrapper.ProductDescription;

		goodsItem.BY_GrossWeight = whsBondedWarehouseAttribute.WB_CustomsQty / whsReceiveLine.WE_TransactionQuantity *
									inventoryWrapper.QuantityToDraw;
		goodsItem.BY_GrossWeightUnit = Constants.Weight.Kilograms;

		goodsItem.BY_NetWeight = whsBondedWarehouseAttribute.WB_CustomsQty / whsReceiveLine.WE_TransactionQuantity *
								inventoryWrapper.QuantityToDraw;
		goodsItem.BY_NetWeightUnit = Constants.Weight.Kilograms;

		goodsItem.BY_FormattedHarmonisedTariff = GetTariff(inventoryWrapper);

		goodsItem.BY_MonetaryValue = whsBondedWarehouseAttribute.WB_ValueForDuty / whsReceiveLine.WE_TransactionQuantity *
									inventoryWrapper.QuantityToDraw;
		goodsItem.BY_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
		return goodsItem;
	}

	ZString GetTariff(WhsInventoryWrapper inventoryWrapper)
	{
		var result = ZString.Empty;
		var product = inventoryWrapper.Part;
		if (product != null)
		{
			result = product.GetPivots<CusClassPartPivot>(Bill.Header.CountryCode).SingleOrDefault(x => x.CI_ChildType == Common.Shared.ClassificationTypeList.Codes.Import)?.CI_FormattedTariffNum ?? ZString.Empty;
		}
		return result;
	}

	protected override FilterBusinessObjectDefaults GetFilterDefaultsCore()
	{
		var result = new FilterBusinessObjectDefaults();

		var organisationPk = Bill.Consignor.OrganisationPK.IsEmpty ? Bill.Header.Consignor.OrganisationPK : Bill.Consignor.OrganisationPK;

		if (organisationPk == ZGuid.Empty)
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			organisationPk = orgProxy?.PK ?? ZGuid.Empty;
		}

		result.Add(new FilterBusinessObjectDefault("Client", "Property", organisationPk, false));

		var warehouse = Bill.Header.MovementHeader.WarehouseAddress.GetWhsWarehouse();
		if (warehouse != null)
		{
			result.Add(new FilterBusinessObjectDefault("Warehouse", "Property", warehouse.PK, false));
		}

		return result;
	}
}
