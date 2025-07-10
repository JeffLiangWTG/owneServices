using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public abstract class DepartureTransportMeansProvider : IDepartureTransportMeans
	{
		protected readonly NctsDepartureMovementHeader moveHeader;

		protected virtual bool IsMainTransportID => true;

		public DepartureTransportMeansProvider(NctsDepartureMovementHeader moveHeader, int sequenceNumber)
		{
			this.moveHeader = Argument.NotNull(moveHeader, nameof(moveHeader));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public abstract int? TypeOfIdentification { get; }

		public virtual string IdentificationNumber => moveHeader.BM_TransportAtDeparture;

		public abstract string Nationality { get; }

		protected bool IsSeaTransportRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._1_SeaTransport)
			{
				typeOfIdentification = 11;
				return true;
			}

			typeOfIdentification = null;
			return false;
		}

		protected bool IsSeaTransportWithVesselRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._1_SeaTransport)
			{
				var vesselExists = moveHeader.Factory.Exists(typeof(RefVessel), new ZQuery(RefVesselSchema.RV_LloydsNumber, moveHeader.BM_TransportAtDeparture));
				if (vesselExists)
				{
					typeOfIdentification = 10;
					return true;
				}
			}

			typeOfIdentification = null;
			return false;
		}

		protected bool IsRailTransportRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport)
			{
				typeOfIdentification = IsMainTransportID ? 21 : 20;
				return true;
			}

			typeOfIdentification = null;
			return false;
		}

		protected bool IsRoadTransportRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport)
			{
				typeOfIdentification = IsMainTransportID ? 30 : 31;
				return true;
			}

			typeOfIdentification = null;
			return false;
		}

		protected bool IsAirTransportRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._4_AirTransport)
			{
				typeOfIdentification = IsMainTransportID ? 40 : 41;
				return true;
			}

			typeOfIdentification = null;
			return false;
		}

		protected bool IsInlandWaterwayTransportRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._8_InlandWaterwayTransport)
			{
				typeOfIdentification = 81;
				return true;
			}

			typeOfIdentification = null;
			return false;
		}

		protected bool IsInlandWaterwayTransportWithVesselRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._8_InlandWaterwayTransport)
			{
				var vesselExists = moveHeader.Factory.Exists(typeof(RefVessel), new ZQuery(RefVesselSchema.RV_LloydsNumber, moveHeader.BM_TransportAtDeparture));
				if (vesselExists)
				{
					typeOfIdentification = 80;
					return true;
				}
			}

			typeOfIdentification = null;
			return false;
		}

		protected bool IsOwnPropulsionRuleValid(out int? typeOfIdentification)
		{
			if (moveHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._9_OwnPropulsion)
			{
				typeOfIdentification = int.TryParse(moveHeader.BM_TransportAtDepartureType, out var value) ? value : null;
				return true;
			}

			typeOfIdentification = null;
			return false;
		}
	}
}
