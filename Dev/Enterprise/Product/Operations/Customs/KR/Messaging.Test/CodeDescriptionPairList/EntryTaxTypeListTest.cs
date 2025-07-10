namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class EntryTaxTypeListTest : NUnit.Framework.TestCase
	{
		public void TestGetCorrespondingChargeTypeCW1ToKRCFor5UL()
		{
			AssertEquals(EntryTaxTypeList.Codes.CUD, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.Duty));
			AssertEquals(EntryTaxTypeList.Codes._5AB, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.EducationTax));
			AssertEquals(EntryTaxTypeList.Codes.CAP, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.AgricultureTax));
			AssertEquals(EntryTaxTypeList.Codes.VAT, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.VAT));
			AssertEquals(EntryTaxTypeList.Codes.ACT, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.LiquorTax));
			AssertEquals(EntryTaxTypeList.Codes.IND, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.SpecialConsumptionTax));
			AssertEquals(EntryTaxTypeList.Codes.ENV, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.TransportationTax));
			AssertEquals(EntryTaxTypeList.Codes._5AC, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.PenaltyForLateDeclaration));
			AssertEquals(EntryTaxTypeList.Codes._5AY, EntryTaxTypeList.GetCorrespondingChargeTypeCW1ToKRCFor5UL(ChargeTypeList.Codes.PenaltyForMissedDeclaration));
		}
		public void TestIs5ULPenaltyChargeType()
		{
			Assert(EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AD));
			Assert(EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AE));
			Assert(EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AF));
			Assert(EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AG));
			Assert(EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AH));
			Assert(EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AI));
			Assert(EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AJ));
			Assert(!EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AV));
			Assert(!EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5AW));
			Assert(!EntryTaxTypeList.Is5ULPenaltyChargeType(EntryTaxTypeList.Codes._5CS));
		}
		public void TestGetCustomsChargeTypeByPenaltyType()
		{
			AssertEquals(EntryTaxTypeList.Codes.CUD, EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(EntryTaxTypeList.Codes._5AD));
			AssertEquals(EntryTaxTypeList.Codes.IND, EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(EntryTaxTypeList.Codes._5AE));
			AssertEquals(EntryTaxTypeList.Codes.ACT, EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(EntryTaxTypeList.Codes._5AF));
			AssertEquals(EntryTaxTypeList.Codes.ENV, EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(EntryTaxTypeList.Codes._5AG));
			AssertEquals(EntryTaxTypeList.Codes.VAT, EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(EntryTaxTypeList.Codes._5AH));
			AssertEquals(EntryTaxTypeList.Codes._5AB, EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(EntryTaxTypeList.Codes._5AI));
			AssertEquals(EntryTaxTypeList.Codes.CAP, EntryTaxTypeList.GetCustomsChargeTypeByPenaltyType(EntryTaxTypeList.Codes._5AJ));
		}
	}
}
