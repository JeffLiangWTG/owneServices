using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	internal class GoodsItemWrapper : IGoodsItem
	{
		readonly AsycudaBill bill;
		readonly ZString configCode;

		public GoodsItemWrapper(AsycudaBill bill, ZString configCode)
		{
			this.bill = bill;
			this.configCode = configCode;
		}

		ZShort IGoodsItem.ItemNumber => bill.ABL_SequenceNumber;

		ZString IGoodsItem.GoodsDescription => bill.ABL_GoodsDescription;

		ZString IGoodsItem.GoodsDescriptionLNG => ZString.Empty; // Do not send.

		ZDecimal IGoodsItem.GrossMass => bill.ABL_GrossWeight;

		ZBool IGoodsItem.IsGrossMassSpecified => true;

		ZString IGoodsItem.TransportChargesOrMethodOfPayment => ZString.Empty; // Do not send.

		ZString IGoodsItem.CommercialReferenceNumber => bill.ABL_BillNumber;

		ZString IGoodsItem.UNDangerousGoodsCode => ZString.Empty; // Do not send.

		ZString IGoodsItem.PlaceOfLoading => ZString.Empty; // DO NOT SEND, send at header instead.

		ZString IGoodsItem.PlaceOfLoadingLNG => ZString.Empty; // Do not send.

		ZString IGoodsItem.PlaceOfUnloading => ZString.Empty; // DO NOT SEND, send at header instead.

		ZString IGoodsItem.PlaceOfUnloadingLNG => ZString.Empty; // Do not send.

		IEnumerable<IProducedDocument> IGoodsItem.ProducedDocuments => null; // Do not send.

		IEnumerable<ISpecialMention> IGoodsItem.SpecialMentions => specialMentions ?? (specialMentions = GetSpecialMentions());

		IEnumerable<ISpecialMention> GetSpecialMentions()
		{
			return new ISpecialMention[] { new SpecialMentionWrapper(bill.Header.SpecialMentions) };
		}

		IEnumerable<ISpecialMention> specialMentions;

		ITrader IGoodsItem.Consignor => consignor ?? (consignor = new TraderWrapper(bill.ABL_ShipperName, bill.ABL_ShipperStreet1,
				bill.ABL_ShipperPostcode, bill.ABL_ShipperCity, bill.ABL_RN_NKShipperCountry, ZString.Empty, configCode));
		ITrader consignor;

		ICommodity IGoodsItem.Commodity => null; // Do not send.

		ITrader IGoodsItem.Consignee => consignee ?? (consignee = new TraderWrapper(bill.ABL_ConsigneeName, bill.ABL_ConsigneeStreet1,
				bill.ABL_ConsigneePostcode, bill.ABL_ConsigneeCity, bill.ABL_RN_NKConsigneeCountry, ZString.Empty, configCode));
		ITrader consignee;

		IEnumerable<IContainer> IGoodsItem.Containers => containers ?? (containers = GetContainers());

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

		IEnumerable<IMeansOfTransportIdentity> IGoodsItem.MeansOfTransportIdentities => null; // Do not send this segment as we send at header.

		IEnumerable<IPackage> IGoodsItem.Packages => packages ?? (packages = GetPackages());

		IEnumerable<IPackage> GetPackages()
		{
			var result = new List<IPackage>();
			foreach (AsycudaPack pack in bill.Packs)
			{
				result.Add(new PackageWrapper(pack));
			}
			return result;
		}

		IEnumerable<IPackage> packages;

		ITrader IGoodsItem.NotifyParty => notifyParty ?? (notifyParty = new TraderWrapper(bill.ABL_NotifyPartyName, bill.ABL_NotifyPartyStreet1,
				bill.ABL_NotifyPartyPostcode, bill.ABL_NotifyPartyCity, bill.ABL_RN_NKNotifyPartyCountry, ZString.Empty, configCode));
		ITrader notifyParty;
	}
}
