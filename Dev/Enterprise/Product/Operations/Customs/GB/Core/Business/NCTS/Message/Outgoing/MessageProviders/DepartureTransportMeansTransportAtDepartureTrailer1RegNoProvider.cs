namespace Enterprise.Customs.GB.Business.NCTS
{
	public class DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider : DepartureTransportMeansProvider
	{
		public DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider(NctsDepartureMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
		{
		}

		public override int TypeOfIdentification
		{
			get
			{
				if (IsSeaTransportRuleValid(out var typeOfIdentification))
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

		public override string IdentificationNumber => moveHeader.BM_TransportAtDepartureTrailer1RegNo;

		public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality;

		protected override bool IsMainTransportID => false;
	}
}
