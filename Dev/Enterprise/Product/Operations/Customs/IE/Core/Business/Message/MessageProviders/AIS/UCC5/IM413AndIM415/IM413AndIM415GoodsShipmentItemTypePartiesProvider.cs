using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using IAdditionalSupplyChainActor = CargoWise.Customs.IE.MessageContracts.Interfaces.IAdditionalSupplyChainActor;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentItemTypePartiesProvider : IGoodsShipmentItemTypeParties
	{
		public IM413AndIM415GoodsShipmentItemTypePartiesProvider(JobComInvoiceLine randomInvoiceLine)
		{
			this.randomInvoiceLine = randomInvoiceLine;
		}
		readonly JobComInvoiceLine randomInvoiceLine;

		public IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => PartyProvider.New(randomInvoiceLine.ExporterAddress));
		CachedValue<IParty> exporterCached;

		public IParty Seller => CachedValueHelper.GetValue(ref sellerCached, () => PartyProvider.New(randomInvoiceLine.SellerDocAddress));
		CachedValue<IParty> sellerCached;

		public IParty Buyer => CachedValueHelper.GetValue(ref buyerCached, () => PartyProvider.New(randomInvoiceLine.BuyerDocAddress));
		CachedValue<IParty> buyerCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> SupplyChainActor => additionalSupplyChainActors ?? (additionalSupplyChainActors =
		randomInvoiceLine.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
		.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalFiscalReference => additionalFiscalReferences ?? (additionalFiscalReferences =
		randomInvoiceLine.FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>()
		.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalFiscalReferences;
	}
}
