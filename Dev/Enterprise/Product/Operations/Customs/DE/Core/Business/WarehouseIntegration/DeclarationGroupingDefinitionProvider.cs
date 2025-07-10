using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business
{
	public class DeclarationGroupingDefinitionProvider
	{
		public DeclarationGroupingDefinitionProvider(WhsInventoryWrapper inventoryWrapper) : this(GetIWhsBondedWarehouseAttribute(inventoryWrapper), inventoryWrapper.Factory)
		{
			InventoryWrapper = inventoryWrapper;
		}

		public DeclarationGroupingDefinitionProvider(IWhsBondedWarehouseAttribute bwhAttribute, BusinessObjectFactory factory)
		{
			this.bwhAttribute = Argument.NotNull(bwhAttribute, nameof(bwhAttribute));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly IWhsBondedWarehouseAttribute bwhAttribute;
		readonly BusinessObjectFactory factory;

		Dictionary<ZString, ZString> AddInfoDictionary => addInfoDictionary ?? (addInfoDictionary = AddInfoParser.CreateDictionaryWithAddInfoString(bwhAttribute.WB_AddInfo));
		Dictionary<ZString, ZString> addInfoDictionary;

		public ZGuid SupplierAddressPK => BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(factory, AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Supplier))?.PK ?? ZGuid.Empty;

		public ZGuid ImporterAddressPK => BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(factory, AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Importer))?.PK ?? ZGuid.Empty;

		public ZString PortOfLoading => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.PortOfLoading);

		public ZString FirstEUArrival => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.FirstEUArrival);

		public ZString Transport => AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Transport);

		public ZGuid Buyer => BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(factory, AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Buyer))?.PK ?? ZGuid.Empty;

		public ZGuid Seller => BondedWarehousingHelper.GetOrgAddressFromBondedWarehouseAttributeAddInfo(factory, AddInfoDictionary.GetValueSafe(BondedWarehousingHelper.Constants.WarehouseCustomsLineDetailsAddInfoKeys.Seller))?.PK ?? ZGuid.Empty;

		public WhsInventoryWrapper InventoryWrapper
		{
			get;
		}

		static IWhsBondedWarehouseAttribute GetIWhsBondedWarehouseAttribute(WhsInventoryWrapper inventoryWrapper)
		{
			Argument.NotNull(inventoryWrapper, nameof(inventoryWrapper));
			return BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsInventoryWrapper(inventoryWrapper);
		}
	}
}
