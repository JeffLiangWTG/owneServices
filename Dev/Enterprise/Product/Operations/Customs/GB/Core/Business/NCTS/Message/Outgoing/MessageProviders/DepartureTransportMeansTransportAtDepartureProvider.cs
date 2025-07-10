namespace Enterprise.Customs.GB.Business.NCTS
{
	public class DepartureTransportMeansTransportAtDepartureProvider : DepartureTransportMeansProvider
	{
		public DepartureTransportMeansTransportAtDepartureProvider(NctsDepartureMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
		{
		}

		public override int TypeOfIdentification
		{
			get
			{
				if (IsSeaTransportWithVesselRuleValid(out var typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsSeaTransportRuleValid(out typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsRailTransportRuleValid(out typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsRoadTransportRuleValid(out typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsAirTransportRuleValid(out typeOfIdentification))
				{
					return typeOfIdentification;
				}

				if (IsInlandWaterwayTransportWithVesselRuleValid(out typeOfIdentification))
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

				return 0;
			}
		}

		public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureCountry;
	}
}
