using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class GoodsItemProvider(AsycudaPack pack) : IGoodsItem
	{
		protected readonly GoodsItemProviderHelper Helper = new(pack);
		protected readonly AsycudaPack Pack = Argument.NotNull(pack, nameof(pack));

		public int GoodsItemNumber => Helper.GoodsItemNumber;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => Helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => Helper.AdditionalSupplyChainActors;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => Helper.SupportingDocuments;

		public IReadOnlyCollection<string> UNDGs => Helper.UNDGs;

		public IReadOnlyCollection<IPackaging> Packaging => Helper.Packaging;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipmentCollection => Helper.TransportEquipmentCollection;

		public decimal GrossMass => Helper.GrossMass;

		public int NumberOfPackages => Helper.NumberOfPackages;

		public string DescriptionOfGoods => Helper.DescriptionOfGoods;

		public ICommodityCode CommodityCode => Helper.CommodityCode;

		public IPostalCharge PostalCharge => Helper.PostalCharge;

		public string TypesOfGoods => Helper.TypeOfGoods;

		public string CUSCode => Pack.PackedItem.API_ChemicalSubstanceCode.GetNullIfEmpty();
	}
}
