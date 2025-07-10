using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CusOtherLawReferenceValidation))]
	sealed class CusOtherLawReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "TestDescritpion", Core.Constants.CountryCodes.Japan);
			var c1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "C1", "Description1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(c1.PK, "IsImport", "Y");
			Factory.Save();

			var instruction = Factory.New<CusEntryInstructionForTest>();
			instruction.CEI_Style = "A";
			instruction.CEI_SubStyle = "IMP";
			var guarantees = new CusOtherLawReferenceCollection<CusOtherLawReference>(instruction);
			var cusOtherLawReference = guarantees.AddNew();
			var targetInfo = cusOtherLawReference.CFR_ReferenceInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "C2", "C1", "The selected value is invalid.");

			instruction.CEI_Style = "B";
			cusOtherLawReference.CFR_Reference = "C3";
			AssertNoErrors(targetInfo);
		}

		public void TestCheckCFR_Reference_Duplicated()
		{
			const string expectedMessageError = "The same information has been entered.";
			var instruction = Factory.New<CusEntryInstructionForTest>();
			instruction.CEI_Style = "A";
			var guarantees = new CusOtherLawReferenceCollection<CusOtherLawReference>(instruction);
			var firstGuarantee = guarantees.AddNew();
			var secondGuarantee = guarantees.AddNew();
			var targetInfo = secondGuarantee.CFR_ReferenceInfo;

			firstGuarantee.CFR_Reference = "12";
			secondGuarantee.CFR_Reference = "12";
			AssertHasMessageError(targetInfo, expectedMessageError);

			secondGuarantee.CFR_Reference = "34";
			AssertNoMessageError(targetInfo, expectedMessageError);

			instruction.CEI_Style = "B";
			secondGuarantee.CFR_Reference = "12";
			AssertNoMessageError(targetInfo, expectedMessageError);
		}
	}
}
