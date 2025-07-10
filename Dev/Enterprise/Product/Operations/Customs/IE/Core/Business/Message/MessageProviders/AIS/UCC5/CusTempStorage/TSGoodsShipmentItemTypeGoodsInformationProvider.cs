using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TSGoodsShipmentItemTypeGoodsInformationProvider : ITSGoodsShipmentItemTypeGoodsInformation
	{
		public static TSGoodsShipmentItemTypeGoodsInformationProvider New(TemporaryStoragePackedItem packedItem) => packedItem == null ? null : new TSGoodsShipmentItemTypeGoodsInformationProvider(packedItem);

		TSGoodsShipmentItemTypeGoodsInformationProvider(TemporaryStoragePackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly TemporaryStoragePackedItem packedItem;

		public string GoodsDescription => packedItem.API_GoodsDescription;

		public decimal GrossMass
			=> Core.Constants.Weight.ConvertSafe(packedItem.API_GrossWeight, packedItem.API_GrossWeightUQ, Core.Constants.Weight.Kilograms);

		public IReadOnlyCollection<IPackaging> Packages => packages ??= AISPackagingProvider.GetCollection(packedItem);
		IReadOnlyCollection<IPackaging> packages;

		public string CusCode => packedItem.API_ChemicalSubstanceCode;

		public string CommodityCode => packedItem.API_Tariff.Left(6);
	}
}
