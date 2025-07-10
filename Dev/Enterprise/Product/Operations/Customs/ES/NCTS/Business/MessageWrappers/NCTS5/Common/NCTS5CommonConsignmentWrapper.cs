using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using CusSeal = Enterprise.Customs.EU.NCTS.Business.CusSeal;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonConsignmentWrapper : INCTSCommonConsignment
	{
		public NCTS5CommonConsignmentWrapper(NctsHeader header, bool isArrivalDeclaration = false)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			this.isArrivalDeclaration = isArrivalDeclaration;
			if (this.isArrivalDeclaration)
			{
				arrivalMovement = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
			}
			else
			{
				departureMovement = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
			}
		}
		protected readonly NctsHeader nctsHeader;
		protected readonly bool isArrivalDeclaration;
		protected readonly NctsDepartureMovementHeader departureMovement;
		protected readonly NctsArrivalMovementHeader arrivalMovement;

		public IReadOnlyCollection<INCTSCommonTransportEquipment> TransportEquipment
		{
			get
			{
				if (transportEquipment == null)
				{
					var transportList = new List<NCTS5CommonTransportEquipmentWrapper>();

					if (isArrivalDeclaration)
					{
						var allContainers = nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>()
																				.Where(x => x.BC_UnloadedState.IsUnloadingStateMISorDIF()
																							|| x.Seals.Cast<CusSeal>().Any(s => s.BK_UnloadingState.IsUnloadingStateMISorDIF())
																							|| ((x.BC_UnloadedState.IsUnloadingStateNEW()
																									|| x.Seals.Cast<CusSeal>().Any(s => s.BK_UnloadingState.IsUnloadingStateNEW()))
																								&& ContainerSelectedInItems(x.BC_ContainerNum)))
																				.OrderBy(x => x.BC_SequenceNumber);
						foreach (var container in allContainers)
						{
							transportList.Add(new NCTS5CommonTransportEquipmentWrapper(container));
						}
					}
					else
					{
						var allContainers = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Where(x => ContainerSelectedInItems(x.BC_ContainerNum)).OrderBy(x => x.BC_SequenceNumber);
						foreach (var container in allContainers)
						{
							transportList.Add(new NCTS5CommonTransportEquipmentWrapper(container));
						}
					}

					transportEquipment = transportList.AsReadOnly();
				}
				return transportEquipment;
			}
		}
		IReadOnlyCollection<NCTS5CommonTransportEquipmentWrapper> transportEquipment;

		public IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = ((!ShouldDeclareDepartureTransportMeansInHouse || ShouldDeclareDepartureTransportMeansInConsignment()) ? GetDepartureTransportMeans() : new List<CommonDepartureTransportMeansWrapper>().AsReadOnly()));
		IReadOnlyCollection<CommonDepartureTransportMeansWrapper> departureTransportMeans;

		protected virtual bool ShouldDeclareDepartureTransportMeansInHouse => false;

		ZBool ContainerSelectedInItems(ZString containerNum)
		{
			if (isArrivalDeclaration)
			{
				return nctsHeader.Bills.Any(x => x.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>().Any(y => y.ContainersSelected.Contains(containerNum)));
			}
			else
			{
				return nctsHeader.Bills.Any(x => x.GoodsItems.Cast<NctsDepartureCargoDesc>().Any(y => y.ContainersSelected.Contains(containerNum)));
			}
		}

		IReadOnlyCollection<CommonDepartureTransportMeansWrapper> GetDepartureTransportMeans()
		{
			if (isArrivalDeclaration)
			{
				return GetDepartureTransportMeansForArrivalHeader();
			}
			else
			{
				return NCTS5WrappersHelper.GetDepartureTransportMeansForDepartureHeader(departureMovement.InlandTransportModeAtDeparture, departureMovement.TransportTypeAtDeparture,
					departureMovement.TransportAtDeparture, departureMovement.TransportCountryAtDeparture, departureMovement.VesselNameAtDeparture, departureMovement.VesselCountryAtDeparture,
					departureMovement.Trailer1IDAtDeparture, departureMovement.Trailer1NationalityAtDeparture, departureMovement.Trailer2IDAtDeparture, departureMovement.Trailer2NationalityAtDeparture);
			}
		}

		IReadOnlyCollection<CommonDepartureTransportMeansWrapper> GetDepartureTransportMeansForArrivalHeader()
		{
			var departureTransportMeans = new List<CommonDepartureTransportMeansWrapper>();

			var transportInfos = arrivalMovement.ArrivalTransportInfos.Where(x => x.TPM_TransportState.IsUnloadingStateNEWorMISorDIF()).OrderBy(x => x.TPM_SequenceNumber);
			foreach (var transportInfo in transportInfos)
			{
				var statusIsNEWorDIF = transportInfo.TPM_TransportState.IsUnloadingStateNEWorDIF();
				departureTransportMeans.Add(new CommonDepartureTransportMeansWrapper(statusIsNEWorDIF ? transportInfo.TPM_TypeOfIdentification : ZString.Empty,
																					statusIsNEWorDIF ? transportInfo.TPM_IdentificationNumber : ZString.Empty,
																					statusIsNEWorDIF ? transportInfo.TPM_RN_NKTransportNationality : ZString.Empty,
																					transportInfo.TPM_SequenceNumber));
			}

			return departureTransportMeans.AsReadOnly();
		}

		protected ZBool ShouldDeclareDepartureTransportMeansInConsignment()
		{
			var transportTypeAtDepartureConsignment = departureMovement.TransportTypeAtDeparture;
			var transportAtDepartureConsignment = departureMovement.TransportAtDeparture;
			var transportCountryConsignment = departureMovement.TransportCountryAtDeparture;
			var vesselConsignment = departureMovement.VesselNameAtDeparture;
			var vesselCountryConsignment = departureMovement.VesselCountryAtDeparture;
			var trailer1Consignment = departureMovement.Trailer1IDAtDeparture;
			var trailer1NationalityConsignment = departureMovement.Trailer1NationalityAtDeparture;
			var trailer2Consignment = departureMovement.Trailer2IDAtDeparture;
			var trailer2NationalityConsignment = departureMovement.Trailer2NationalityAtDeparture;

			var transportMeansDifferentInHouses = nctsHeader.Bills.Any(b => (!b.TransportTypeAtDeparture.IsEmpty && b.TransportTypeAtDeparture != transportTypeAtDepartureConsignment)
																			|| (!b.TransportAtDeparture.IsEmpty && b.TransportAtDeparture != transportAtDepartureConsignment)
																			|| (!b.TransportCountryAtDeparture.IsEmpty && b.TransportCountryAtDeparture != transportCountryConsignment)
																			|| (!b.VesselNameAtDeparture.IsEmpty && b.VesselNameAtDeparture != vesselConsignment)
																			|| (!b.VesselCountryAtDeparture.IsEmpty && b.VesselCountryAtDeparture != vesselCountryConsignment)
																			|| (!b.Trailer1IDAtDeparture.IsEmpty && b.Trailer1IDAtDeparture != trailer1Consignment)
																			|| (!b.Trailer1NationalityAtDeparture.IsEmpty && b.Trailer1NationalityAtDeparture != trailer1NationalityConsignment)
																			|| (!b.Trailer2IDAtDeparture.IsEmpty && b.Trailer2IDAtDeparture != trailer2Consignment)
																			|| (!b.Trailer2NationalityAtDeparture.IsEmpty && b.Trailer2NationalityAtDeparture != trailer2NationalityConsignment));
			return !transportMeansDifferentInHouses;
		}
	}
}
