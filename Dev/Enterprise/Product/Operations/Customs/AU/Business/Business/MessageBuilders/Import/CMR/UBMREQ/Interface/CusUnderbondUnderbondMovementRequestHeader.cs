using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusUnderbondUnderbondMovementRequestHeader : IUnderbondMovementRequestHeader
	{
		public CusUnderbondUnderbondMovementRequestHeader(CusUnderbond underbond)
		{
			this.underbond = underbond;
		}

		#region IUnderbondMovementRequestHeader Members

		public ZString ResponsiblePartyID
		{
			get { return underbond.C4_ResponsiblePartyID; }
		}

		public ZString RequestReasonCode
		{
			get { return underbond.C4_MovementReason; }
		}

		public ZString ModeOfTransport
		{
			get { return underbond.C4_ModeOfMovement; }
		}

		public ZString UnderbondBySeaVoyageNumber
		{
			get { return underbond.C4_UnderbondBySeaVoyage; }
		}

		public ZString UnderbondBySeaVesselID
		{
			get { return underbond.C4_UnderbondBySeaLloydsIMONum; }
		}

		public ZString DestaintionEstablishmentID
		{
			get { return underbond.C4_DestinationPremiseID; }
		}

		public ZString OriginatingEstablishmentID
		{
			get { return underbond.C4_OriginPremiseID; }
		}

		public ZString DischargeEstablishmentID
		{
			get
			{
				ZString result = underbond.C4_DischargePremiseID;
				if (result.IsEmpty)
				{
					result = underbond.C4_IsMoveFromDischarge ? underbond.C4_OriginPremiseID : underbond.C4_RL_NKDischargePort;
				}
				return result;
			}
		}

		public ZString UnderbondBySeaOverseasRoutingPort
		{
			get { return ZString.Empty; }
		}

		public abstract bool IsBureau { get; }
		public abstract ZString VoyageNumber { get; }
		public abstract ZString VesselID { get; }
		public abstract ZString TranshipmentOverseasDestinationPort { get; }
		public abstract IUnderbondMovementRequestLine Line { get; }

		public ZString FlightNumber
		{
			get
			{
				ZString result = underbond.C4_FlightNo;
				if (result.IsEmpty)
				{
					result = FlightNumberDefault;
				}

				return result;
			}
		}

		protected virtual ZString FlightNumberDefault
		{
			get { return ""; }
		}

		public ZDateTime EstimatedDateOfArrival
		{
			get
			{
				ZDateTime result = underbond.C4_ArrivalDate;
				if (result.IsEmpty)
				{
					result = EstimatedDateOfArrivalDefault;
				}

				return result;
			}
		}

		protected virtual ZDateTime EstimatedDateOfArrivalDefault
		{
			get { return ZDateTime.Empty; }
		}

		public virtual ZInt NumberOfPackages
		{
			get { return underbond.C4_PiecesManifested; }
		}

		public virtual ZString PackageType
		{
			get { return underbond.C4_PackageType; }
		}

		#endregion

		#region Implementation

		protected CusUnderbond underbond;

		#endregion
	}
}
