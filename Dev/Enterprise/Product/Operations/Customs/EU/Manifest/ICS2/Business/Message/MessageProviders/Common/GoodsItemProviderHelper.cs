using System;
using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class GoodsItemProviderHelper
	{
		public GoodsItemProviderHelper(AsycudaPack pack)
		{
			asycudaPack = CargoWise.Common.Argument.NotNull(pack, nameof(pack));
		}

		readonly AsycudaPack asycudaPack;

		public int GoodsItemNumber => asycudaPack.APA_LineNo;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => additionalInformationCollection ??= GetAdditionalInformation();
		IReadOnlyCollection<IAdditionalInformation> additionalInformationCollection;

		IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
		{
			return asycudaPack.AdditionalInfos.ToArray<AdditionalInfo, IAdditionalInformation>(c => new AdditionalInformationProvider(c));
		}

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => additionalSupplyChainActors ??= GetAdditionalSupplyChainActor();
		IReadOnlyCollection<IIdentifierTypePair> additionalSupplyChainActors;

		IReadOnlyCollection<IIdentifierTypePair> GetAdditionalSupplyChainActor()
		{
			return asycudaPack.CusSupplyChainActorReferences.ToArray<CusSupplyChainActorReference, IIdentifierTypePair>(supplyChainActor => new AdditionalSupplyChainActorProvider(supplyChainActor));
		}

		public decimal GrossMass => asycudaPack.GrossWeightInKG;

		public int NumberOfPackages => asycudaPack.APA_PackQty;

		public string DescriptionOfGoods => asycudaPack.PackedItem.API_GoodsDescription;

		public ICommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode, () => CommodityCodeProvider.NewOrNull(asycudaPack.PackedItem.API_Tariff));
		CachedValue<ICommodityCode> commodityCode;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => supportingDocuments ??= MessageProviderHelper.GetSupportingDocuments(asycudaPack.SupportingDocuments);
		IReadOnlyCollection<IIdentifierTypePair> supportingDocuments;

		public IReadOnlyCollection<string> UNDGs => undgs ??= GetUNDGs();
		IReadOnlyCollection<string> undgs;

		IReadOnlyCollection<string> GetUNDGs()
		{
			return asycudaPack.UNDGs.ToArray(c => c.DI_DG_NKSubs.ToString().LeftOrNull(4));
		}

		public IReadOnlyCollection<IPackaging> Packaging => packaging ??= new[] { new PackagingProvider(asycudaPack) };

		IReadOnlyCollection<IPackaging> packaging;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipmentCollection => transportEquipmentCollection ??= GetTransportEquipment();
		IReadOnlyCollection<ITransportEquipment> transportEquipmentCollection;

		IReadOnlyCollection<ITransportEquipment> GetTransportEquipment()
		{
			return asycudaPack.Container?.ToArray<AsycudaContainer, ITransportEquipment>(TransportEquipmentProvider.NewOrNull) ?? Array.Empty<ITransportEquipment>();
		}

		public IReadOnlyCollection<IPassiveBorderTransportMeans> TransportMeansFromPack => transportMeansFromPack ??= GetTransportMeansFromPack();
		IReadOnlyCollection<IPassiveBorderTransportMeans> transportMeansFromPack;

		IReadOnlyCollection<IPassiveBorderTransportMeans> GetTransportMeansFromPack() =>
			asycudaPack.AsycudaTransportMeans.ToArray<AsycudaTransportMeans, IPassiveBorderTransportMeans>(tm => new PassiveBorderTransportMeansProvider(tm));

		public IPostalCharge PostalCharge => CachedValueHelper.GetValue(ref postalCharge, () => PostalChargesProvider.NewOrNull(asycudaPack.PackedItem));
		CachedValue<IPostalCharge> postalCharge;

		public string TypeOfGoods => asycudaPack.PackedItem.API_TypeOfGoods;
	}
}
