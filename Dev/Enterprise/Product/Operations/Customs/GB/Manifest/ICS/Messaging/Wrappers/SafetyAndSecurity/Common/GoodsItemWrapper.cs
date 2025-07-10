using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.Interfaces.SafetyAndSecurity;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using AsycudaBill = Enterprise.Customs.EU.Manifest.Business.AsycudaBill;
using AsycudaPack = Enterprise.Customs.EU.Manifest.Business.AsycudaPack;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class GoodsItemWrapper : IGoodsItem
	{
		public GoodsItemWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly AsycudaBill bill;

		public string ItemNumber => bill.ABL_SequenceNumber.ToString();

		public string GoodsDescription => bill.ABL_GoodsDescription;

		public string GoodsDescriptionLNG => ZString.Empty; // Do not send.

		public decimal GrossMass => bill.ABL_GrossWeight;

		public bool IsGrossMassSpecified => true;

		public string TransportChargesOrMethodOfPayment => bill.ABL_PrepaidCollect;

		public string CommercialReferenceNumber => bill.ABL_BillNumber;

		public string UNDangerousGoodsCode => dangerousGoodsCode ??= GetDangerousGoodsCode();
		string dangerousGoodsCode;
		ZString GetDangerousGoodsCode() => bill.Packs
			.Select(x => x.UNDGs.Select(x => x.Substance?.DG_Code ?? ZString.Empty).FirstOrDefault(x => !x.IsEmpty))
			.FirstOrDefault(x => !x.IsEmpty);

		public string PlaceOfLoading => bill.ABL_RL_NKOrigin.IsEmpty ? (bill.Header?.AMA_RL_NKPortOfLoading ?? ZString.Empty) : bill.ABL_RL_NKOrigin;

		public string PlaceOfLoadingLNG => ZString.Empty; // Do not send.

		public string PlaceOfUnloading => bill.ABL_RL_NKFinalDestination.IsEmpty ? (bill.Header?.AMA_RL_NKPortOfDischarge) : bill.ABL_RL_NKFinalDestination;

		public string PlaceOfUnloadingLNG => ZString.Empty; // Do not send.

		public IEnumerable<IProducedDocument> ProducedDocuments => null; // Do not send.

		#region SpecialMentions
		public IEnumerable<ISpecialMention> SpecialMentions => specialMentions ?? (specialMentions = GetSpecialMentions());

		IEnumerable<ISpecialMention> GetSpecialMentions()
		{
			return new ISpecialMention[] { new SpecialMentionWrapper(bill.SpecialMentions) };
		}
		IEnumerable<ISpecialMention> specialMentions;
		#endregion

		#region Consignor
		public ITrader Consignor => consignor ?? (consignor = new TraderWrapper(bill.ABL_ShipperName,
																bill.ABL_ShipperStreet1,
																bill.ABL_ShipperPostcode,
																bill.ABL_ShipperCity,
																bill.ABL_RN_NKShipperCountry,
																ZString.Empty, bill.ABL_ShipperRegNo));
		ITrader consignor;
		#endregion

		public ICommodity Commodity => commodity ??= GetCommodity();

		ICommodity commodity;
		ICommodity GetCommodity() => bill.Packs.Where(x => !x.APA_CommodityCode.IsEmpty).Select(x => new CommodityWrapper(x.APA_CommodityCode)).FirstOrDefault() ?? new CommodityWrapper(ZString.Empty);

		#region Consignee
		public ITrader Consignee => consignee ?? (consignee = new TraderWrapper(bill.ABL_ConsigneeName,
														bill.ABL_ConsigneeStreet1,
														bill.ABL_ConsigneePostcode,
														bill.ABL_ConsigneeCity,
														bill.ABL_RN_NKConsigneeCountry,
														ZString.Empty,
														bill.ABL_ConsigneeRegNo));
		ITrader consignee;
		#endregion

		#region Containers
		public IEnumerable<IContainer> Containers => containers ?? (containers = GetContainers());

		IEnumerable<IContainer> GetContainers()
		{
			var result = new List<IContainer>();

			foreach (AsycudaPack pack in bill.Packs)
			{
				var container = pack.Pivot?.Container;
				if (container != null)
				{
					result.Add(new ContainerWrapper(container.ACN_ContainerNumber));
				}
			}

			return result;
		}
		IEnumerable<IContainer> containers;
		#endregion

		public IEnumerable<IMeansOfTransportIdentity> MeansOfTransportIdentities => bill.Header.AMA_TransportMode == GBSSTransportTypeList.Codes.RailFreight
			? new IMeansOfTransportIdentity[] { new MeansOfTransportIdentityWrapper(ZString.Empty, bill.Header.AMA_Voyage, ZString.Empty) }
			: null;

		#region Packages
		public IEnumerable<IPackage> Packages => packages ??= GetPackages();

		IReadOnlyCollection<IPackage> GetPackages()
		{
			var result = new List<IPackage>();
			foreach (AsycudaPack pack in bill.Packs)
			{
				result.Add(new PackageWrapper(pack));
			}
			return result;
		}
		IReadOnlyCollection<IPackage> packages;
		#endregion

		public ITrader NotifyParty => notifyParty ?? (notifyParty = new TraderWrapper(bill.ABL_NotifyPartyName,
																		bill.ABL_NotifyPartyStreet1,
																		bill.ABL_NotifyPartyPostcode,
																		bill.ABL_NotifyPartyCity,
																		bill.ABL_RN_NKNotifyPartyCountry,
																		ZString.Empty,
																		bill.ABL_NotifyPartyRegNo));
		ITrader notifyParty;
	}
}
