using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend50GoodsItemProvider : CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend50.IGoodsItem
	{
		public SendAndAmend50GoodsItemProvider(AsycudaPack pack)
		{
			helper = new GoodsItemProviderHelper(Argument.NotNull(pack, nameof(pack)));
		}

		protected readonly GoodsItemProviderHelper helper;

		public int GoodsItemNumber => helper.GoodsItemNumber;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public string DescriptionOfGoods => helper.DescriptionOfGoods;

		public string CUSCode => null;

		public ICommodityCode CommodityCode => helper.CommodityCode;

		public IReadOnlyCollection<string> UNDGs => helper.UNDGs;

		public decimal GrossMass => helper.GrossMass;

		public IReadOnlyCollection<IPackaging> Packaging => helper.Packaging;

		public IReadOnlyCollection<IPassiveBorderTransportMeans> PassiveBorderTransportMeans => helper.TransportMeansFromPack;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipmentCollection => helper.TransportEquipmentCollection;
	}
}
