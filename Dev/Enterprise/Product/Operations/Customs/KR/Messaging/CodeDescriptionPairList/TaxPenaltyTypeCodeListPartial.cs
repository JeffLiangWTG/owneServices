namespace Enterprise.Customs.KR.Messaging
{
	partial class TaxPenaltyTypeCodeList
	{
		public static string GetDutyPenaltyOrFeeType(string code)
		{
			switch (code)
			{
				case Codes._01:
					return Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateMissedDeclaration;
				case Codes._02:
					return Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralMissedDeclaration;
				case Codes._0A:
					return Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateMissedDeclaration;
				case Codes._03:
					return Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration;
				case Codes._04:
					return Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration;
				case Codes._0B:
					return Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration;
				default:
					return "";
			}
		}

		public static string GetTaxPenaltyOrFeeType(string code)
		{
			switch (code)
			{
				case Codes._01:
					return Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateMissedDeclaration;
				case Codes._02:
					return Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralMissedDeclaration;
				case Codes._0A:
					return Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateMissedDeclaration;
				case Codes._03:
					return Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration;
				case Codes._04:
					return Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration;
				case Codes._0B:
					return Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration;
				default:
					return "";
			}
		}

		public static string GetCustomsPenaltyTypeByCW1(string code)
		{
			switch (code)
			{
				case Codes._01:
				case Codes._02:
				case Codes._03:
				case Codes._04:
				case Codes._05:
					return code;
				case Codes._0A:
					return Codes._01;
				case Codes._0B:
					return Codes._03;
				default:
					return "";
			}
		}

		public static bool IsEligibleForDutyReduction(string code)
		{
			return code == Codes._03
				|| code == Codes._04
				|| code == Codes._0B;
		}
	}
}
