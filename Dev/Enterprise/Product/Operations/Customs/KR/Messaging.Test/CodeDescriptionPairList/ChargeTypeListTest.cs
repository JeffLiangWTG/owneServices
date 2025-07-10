namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class ChargeTypeListTest : NUnit.Framework.TestCase
	{
		public void TestGetCorrespondingChargeType()
		{
			AssertEquals(ChargeTypeList.Codes.Duty, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.CUD));

			AssertEquals(ChargeTypeList.Codes.VAT, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.VAT));

			AssertEquals(ChargeTypeList.Codes.LiquorTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.TAC));
			AssertEquals(ChargeTypeList.Codes.LiquorTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.ACT));

			AssertEquals(ChargeTypeList.Codes.AgricultureTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.CAP));
			AssertEquals(ChargeTypeList.Codes.AgricultureTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5DC));
			AssertEquals(ChargeTypeList.Codes.AgricultureTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5CL));

			AssertEquals(ChargeTypeList.Codes.SpecialConsumptionTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.CST));
			AssertEquals(ChargeTypeList.Codes.SpecialConsumptionTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.IND));

			AssertEquals(ChargeTypeList.Codes.TransportationTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes.ENV));
			AssertEquals(ChargeTypeList.Codes.TransportationTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AA));

			AssertEquals(ChargeTypeList.Codes.EducationTax, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AB));

			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AT));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AD));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AE));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AF));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AG));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AH));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AI));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AJ));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AU));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AV));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AW));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AX));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AS));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AZ));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5BA));
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5BB));

			AssertEquals(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AK));
			AssertEquals(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5CT));
			AssertEquals(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AC));
			AssertEquals(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AY));
			AssertEquals(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5CS));
		}

		public void TestDomesticTaxTypes()
		{
			AssertEquals(6, ChargeTypeList.GetDomesticTaxTypes().Length);
			AssertEquals(ChargeTypeList.Codes.LiquorTax, ChargeTypeList.GetDomesticTaxTypes()[0]);
			AssertEquals(ChargeTypeList.Codes.AgricultureTax, ChargeTypeList.GetDomesticTaxTypes()[1]);
			AssertEquals(ChargeTypeList.Codes.TransportationTax, ChargeTypeList.GetDomesticTaxTypes()[2]);
			AssertEquals(ChargeTypeList.Codes.SpecialConsumptionTax, ChargeTypeList.GetDomesticTaxTypes()[3]);
			AssertEquals(ChargeTypeList.Codes.EducationTax, ChargeTypeList.GetDomesticTaxTypes()[4]);
			AssertEquals(ChargeTypeList.Codes.VAT, ChargeTypeList.GetDomesticTaxTypes()[5]);
		}
	}
}
