using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobDeclarationEntryNumValidationTest : TestCaseWithFactory
	{
		public void TestCheckCE_EntryNum()
		{
			const string dtdMessage = "DTD number should be 16 alphanumeric";
			const string gclMessage = "GCL number should be 13 alphanumeric";
			var declaration = Factory.New<JobDeclaration>();
			var dtdNum = declaration.AdditionalReferenceNumbers.AddNew();
			dtdNum.CE_EntryType = AdditionalReferenceNumberTypes.Codes.DTDPreNumber;
			dtdNum.CE_EntryNum = "ABC123";
			AssertHasMessageErrorContaining(dtdNum.CE_EntryNumInfo, dtdMessage);
			dtdNum.CE_EntryNum = "1234567890ABCDEF";
			AssertNoMessageErrorContaining(dtdNum.CE_EntryNumInfo, dtdMessage);
			var gclNum = declaration.AdditionalReferenceNumbers.AddNew();
			gclNum.CE_EntryType = AdditionalReferenceNumberTypes.Codes.GoodsCarriedListNo;
			gclNum.CE_EntryNum = "ABC123";
			AssertHasMessageErrorContaining(gclNum.CE_EntryNumInfo, gclMessage);
			gclNum.CE_EntryNum = "1234567890ABC";
			AssertNoMessageErrorContaining(gclNum.CE_EntryNumInfo, gclMessage);
		}

		public void TestValidationModeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var dtdNum = declaration.AdditionalReferenceNumbers.AddNew();
			var validation = dtdNum.Validation as JobDeclarationEntryNumValidation;
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, validation.ValidationModeProvider);

			var dtdNum2 = Factory.New<CusEntryNumber>();
			validation = new JobDeclarationEntryNumValidation(dtdNum2);
			AssertNull(validation.ValidationModeProvider);
		}
	}
}
