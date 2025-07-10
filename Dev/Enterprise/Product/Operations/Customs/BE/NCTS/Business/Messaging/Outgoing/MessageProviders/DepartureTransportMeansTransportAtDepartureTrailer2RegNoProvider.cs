namespace Enterprise.Customs.BE.NCTS.Business
{
	public class DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider : DepartureTransportMeansProvider
	{
		public DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider(NctsDepartureMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
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

				return null;
			}
		}

		public override string IdentificationNumber => moveHeader.BM_TransportAtDepartureTrailer2RegNo;

		public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality;

		protected override bool IsMainTransportID => false;
	}
}
