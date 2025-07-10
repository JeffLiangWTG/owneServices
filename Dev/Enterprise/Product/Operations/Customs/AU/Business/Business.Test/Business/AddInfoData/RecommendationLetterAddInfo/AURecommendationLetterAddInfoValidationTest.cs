using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AURecommendationLetterAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZA_LetterNumber()
		{
			AssertEquals("Precondition", EXDOCCommodityCodes.Codes.Meat, quarantineHeader.QH_ProduceType);

			letter.ZA_LetterNumber = ZString.Empty;
			AssertHasMessageErrorContaining(letter.ZA_LetterNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(letter.ZA_LetterNumberInfo, AURecommendationLetterAddInfoValidation.LetterDetailsShouldNotBeEntered);

			letter.ZA_LetterNumber = "123";
			AssertNoMessageErrorContaining(letter.ZA_LetterNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(letter.ZA_LetterNumberInfo, AURecommendationLetterAddInfoValidation.LetterDetailsShouldNotBeEntered);

			letter.Header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			letter.AddInfoValidation.ValidateAll();
			AssertNoMessageErrorContaining(letter.ZA_LetterNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(letter.ZA_LetterNumberInfo, AURecommendationLetterAddInfoValidation.LetterDetailsShouldNotBeEntered);
		}

		public void TestCheckZA_LetterDate()
		{
			AssertEquals("Precondition", EXDOCCommodityCodes.Codes.Meat, quarantineHeader.QH_ProduceType);

			letter.ZA_LetterNumber = ZString.Empty;
			letter.ZA_LetterDate = ZDateTime.Today;
			AssertHasMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterNumberRequired);
			AssertNoMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterDetailsShouldNotBeEntered);

			letter.ZA_LetterNumber = "123";
			AssertNoMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterNumberRequired);
			AssertNoMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterDetailsShouldNotBeEntered);

			letter.Header.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			letter.AddInfoValidation.ValidateAll();
			AssertNoMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterNumberRequired);
			AssertHasMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterDetailsShouldNotBeEntered);

			letter.ZA_LetterNumber = ZString.Empty;
			AssertNoMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterNumberRequired);
			AssertHasMessageErrorContaining(letter.ZA_LetterDateInfo, AURecommendationLetterAddInfoValidation.LetterDetailsShouldNotBeEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			quarantineHeader = helper.Header1.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			letter = quarantineHeader.RecommendationLetters.AddNew();
		}

		QuarantineExDocHeader quarantineHeader;
		RecommendationLetter letter;
	}
}
