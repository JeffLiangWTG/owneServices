using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE170ConsignmentProvider : NctsDepartureHeaderMessageProvider, IIE170Consignment
	{
		public IE170ConsignmentProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public bool ContainerIndicator
		{
			get
			{
				if (NctsHeader.DepartureHeaderContainers.Count > 0)
				{
					foreach (var container in NctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>())
					{
						if (!container.BC_ContainerNum.IsEmpty)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public string InlandModeOfTransport => MovementHeader.BM_InlandTransportMode;

		public string ModeOfTransportAtTheBorder => MovementHeader.BM_ExportTransportMode;

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipment
		{
			get
			{
				if (transportEquipment is null)
				{
					transportEquipment = TransportEquipmentWithSealsProvider.GetEquipments(NctsHeader);
				}
				return transportEquipment;
			}
		}
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipment;

		public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoodsCached, () => new LocationOfGoodsProvider(NctsHeader.MovementHeader));
		CachedValue<ILocationOfGoods> locationOfGoodsCached;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = DepartureTransportMeansProvider.GetTransportMeans(MovementHeader));
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IReadOnlyCollection<IActiveTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = new ActiveTransportMeansProvider(MovementHeader).AsReadOnlyCollection());
		IReadOnlyCollection<IActiveTransportMeans> activeBorderTransportMeans;

		public IPort PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, CreatePortProvider);
		CachedValue<IPort> placeOfLoading;

		PortProvider CreatePortProvider()
		{
			var movementHeader = MovementHeader;
			var portOfPresentationCode = movementHeader.BM_PortOfPresentationCode;
			return portOfPresentationCode.Length > 2 ?
				new PortProvider { UNLocode = portOfPresentationCode }
				: new PortProvider { Country = portOfPresentationCode, Location = movementHeader.BM_PlaceOfLoading };
		}

		public IReadOnlyCollection<IIE170HouseConsignment> HouseConsignment => houseConsignment ?? (houseConsignment = new IE170HouseConsignmentProvider(MovementHeader).AsReadOnlyCollection());
		IReadOnlyCollection<IIE170HouseConsignment> houseConsignment;
	}
}
