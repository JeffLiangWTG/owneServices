namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class TaxPenaltyTypeCodeListTest : NUnit.Framework.TestCase
	{
		public void TestGetDutyPenaltyOrFeeType()
		{
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateMissedDeclaration, TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._01));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralMissedDeclaration, TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._02));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateMissedDeclaration, TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._0A));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration, TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._03));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration, TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._04));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration, TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._0B));
			AssertEquals("", TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._05));
		}

		public void TestGetTaxPenaltyOrFeeType()
		{
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateMissedDeclaration, TaxPenaltyTypeCodeList.GetTaxPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._01));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralMissedDeclaration, TaxPenaltyTypeCodeList.GetTaxPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._02));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateMissedDeclaration, TaxPenaltyTypeCodeList.GetTaxPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._0A));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration, TaxPenaltyTypeCodeList.GetTaxPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._03));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration, TaxPenaltyTypeCodeList.GetTaxPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._04));
			AssertEquals(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration, TaxPenaltyTypeCodeList.GetTaxPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._0B));
			AssertEquals("", TaxPenaltyTypeCodeList.GetDutyPenaltyOrFeeType(TaxPenaltyTypeCodeList.Codes._05));
		}

		public void TestGetCustomsPenaltyTypeByCW1()
		{
			AssertEquals(TaxPenaltyTypeCodeList.Codes._01, TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(TaxPenaltyTypeCodeList.Codes._01));
			AssertEquals(TaxPenaltyTypeCodeList.Codes._02, TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(TaxPenaltyTypeCodeList.Codes._02));
			AssertEquals(TaxPenaltyTypeCodeList.Codes._01, TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(TaxPenaltyTypeCodeList.Codes._0A));
			AssertEquals(TaxPenaltyTypeCodeList.Codes._03, TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(TaxPenaltyTypeCodeList.Codes._03));
			AssertEquals(TaxPenaltyTypeCodeList.Codes._04, TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(TaxPenaltyTypeCodeList.Codes._04));
			AssertEquals(TaxPenaltyTypeCodeList.Codes._03, TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(TaxPenaltyTypeCodeList.Codes._0B));
			AssertEquals(TaxPenaltyTypeCodeList.Codes._05, TaxPenaltyTypeCodeList.GetCustomsPenaltyTypeByCW1(TaxPenaltyTypeCodeList.Codes._05));
		}

		public void TestIsDomesticTaxReduction()
		{
			AssertEquals(false, TaxPenaltyTypeCodeList.IsEligibleForDutyReduction(TaxPenaltyTypeCodeList.Codes._01));
			AssertEquals(false, TaxPenaltyTypeCodeList.IsEligibleForDutyReduction(TaxPenaltyTypeCodeList.Codes._02));
			AssertEquals(false, TaxPenaltyTypeCodeList.IsEligibleForDutyReduction(TaxPenaltyTypeCodeList.Codes._0A));
			AssertEquals(true, TaxPenaltyTypeCodeList.IsEligibleForDutyReduction(TaxPenaltyTypeCodeList.Codes._03));
			AssertEquals(true, TaxPenaltyTypeCodeList.IsEligibleForDutyReduction(TaxPenaltyTypeCodeList.Codes._04));
			AssertEquals(true, TaxPenaltyTypeCodeList.IsEligibleForDutyReduction(TaxPenaltyTypeCodeList.Codes._0B));
			AssertEquals(false, TaxPenaltyTypeCodeList.IsEligibleForDutyReduction(TaxPenaltyTypeCodeList.Codes._05));
		}
	}
}
