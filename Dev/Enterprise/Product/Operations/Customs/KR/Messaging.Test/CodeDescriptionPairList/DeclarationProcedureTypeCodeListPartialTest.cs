namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class DeclarationProcedureTypeCodeListPartialTest : NUnit.Framework.TestCase
	{
		public void TestImpEntryNumberCheckDigit()
		{
			AssertEquals(Constants.EntryNumberCheckDigit.B, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._12, ImportDealingTypeCodeList.Codes._11));
			AssertEquals(Constants.EntryNumberCheckDigit.B, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._27, ImportDealingTypeCodeList.Codes._11));
			AssertEquals(Constants.EntryNumberCheckDigit.B, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._31, ImportDealingTypeCodeList.Codes._11));

			AssertEquals(Constants.EntryNumberCheckDigit.S, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._18, ImportDealingTypeCodeList.Codes._11));
			AssertEquals(Constants.EntryNumberCheckDigit.S, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._24, ImportDealingTypeCodeList.Codes._11));
			AssertEquals(Constants.EntryNumberCheckDigit.S, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._30, ImportDealingTypeCodeList.Codes._11));
			AssertEquals(Constants.EntryNumberCheckDigit.S, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._33, ImportDealingTypeCodeList.Codes._11));

			AssertEquals(Constants.EntryNumberCheckDigit.F, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._14, ImportDealingTypeCodeList.Codes._11));
			AssertEquals(Constants.EntryNumberCheckDigit.F, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._35, ImportDealingTypeCodeList.Codes._11));
			AssertEquals(Constants.EntryNumberCheckDigit.F, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._37, ImportDealingTypeCodeList.Codes._11));

			AssertEquals(Constants.EntryNumberCheckDigit.H, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._39, ImportDealingTypeCodeList.Codes._69));
			AssertEquals(Constants.EntryNumberCheckDigit.M, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._39, ImportDealingTypeCodeList.Codes._11));

			AssertEquals(Constants.EntryNumberCheckDigit.M, DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(DeclarationProcedureTypeCodeList.Codes._11, ImportDealingTypeCodeList.Codes._69));
		}
	}
}
