namespace Enterprise.Customs.BE.NCTS.Business
{
	public class DepartureTransportMeansAircraftIDAtDepartureProvider : DepartureTransportMeansProvider
	{
		public DepartureTransportMeansAircraftIDAtDepartureProvider(NctsDepartureMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
		{
		}

		public override int? TypeOfIdentification
		{
			get
			{
				if (IsSeaTransportRuleValid(out var typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsAirTransportRuleValid(out typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsInlandWaterwayTransportRuleValid(out typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsOwnPropulsionRuleValid(out typeOfIdentification))
				{
					return typeOfIdentification;
				}

				return null;
			}
		}

		public override string IdentificationNumber => moveHeader.BM_AircraftIDAtDeparture;

		public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureCountry;

		protected override bool IsMainTransportID => false;
	}
}
