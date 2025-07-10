using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusDV1DetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDV1_Relationship()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(dv1Detail.DV1_RelationshipInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(dv1Detail.DV1_RelationshipInfo, "A", YesNoList.Codes.Yes);
			});
		}

		public void TestCheckDV1_PriceInfluence()
		{
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(dv1Detail.DV1_PriceInfluenceInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_Relationship = YesNoList.Codes.Yes;
				AssertNoMessageErrorContaining("Default", dv1Detail.DV1_PriceInfluenceInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoErrorContaining("Invalid Code", dv1Detail.DV1_PriceInfluenceInfo, "Enter a valid");

				dv1Detail.DV1_PriceInfluence = ZString.Empty;
				AssertHasMessageErrorContaining("Empty", dv1Detail.DV1_PriceInfluenceInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoErrorContaining("Invalid Code", dv1Detail.DV1_PriceInfluenceInfo, "Enter a valid");

				dv1Detail.DV1_PriceInfluence = "A";
				AssertNoMessageErrorContaining("Default", dv1Detail.DV1_PriceInfluenceInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasErrorContaining("Invalid Code", dv1Detail.DV1_PriceInfluenceInfo, "Enter a valid");
			});
		}

		public void TestCheckDV1_Restrictions()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(dv1Detail.DV1_RestrictionsInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(dv1Detail.DV1_RestrictionsInfo, "A", YesNoList.Codes.Yes);
			});
		}

		public void TestCheckDV1_Consideration()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(dv1Detail.DV1_ConsiderationInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(dv1Detail.DV1_ConsiderationInfo, "A", YesNoList.Codes.Yes);
			});
		}

		public void TestCheckDV1_RestrictionConsiderationDetails()
		{
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(dv1Detail.DV1_RestrictionConsiderationDetailsInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_Restrictions = YesNoList.Codes.Yes;
				dv1Detail.DV1_Consideration = YesNoList.Codes.No;
				dv1Detail.Validation.ValidateDV1_RestrictionConsiderationDetails();
				AssertHasMessageErrorContaining(dv1Detail.DV1_RestrictionConsiderationDetailsInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_Restrictions = YesNoList.Codes.Yes;
				dv1Detail.DV1_Consideration = YesNoList.Codes.Yes;
				dv1Detail.Validation.ValidateDV1_RestrictionConsiderationDetails();
				AssertHasMessageErrorContaining(dv1Detail.DV1_RestrictionConsiderationDetailsInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_RestrictionConsiderationDetails = "Restriction Details";
				AssertNoMessageErrorContaining(dv1Detail.DV1_RestrictionConsiderationDetailsInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckDV1_RoyaltiesLicence()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(dv1Detail.DV1_RoyaltiesLicenceInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(dv1Detail.DV1_RoyaltiesLicenceInfo, "A", YesNoList.Codes.Yes);
			});
		}

		public void TestCheckDV1_RoyaltiesLicenceDetails()
		{
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(dv1Detail.DV1_RoyaltiesLicenceDetailsInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.Yes;
				dv1Detail.Validation.ValidateDV1_RoyaltiesLicenceDetails();
				AssertHasMessageErrorContaining(dv1Detail.DV1_RoyaltiesLicenceDetailsInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_RoyaltiesLicenceDetails = "Licence Details";
				AssertNoMessageErrorContaining(dv1Detail.DV1_RoyaltiesLicenceDetailsInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckDV1_Resale()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(dv1Detail.DV1_ResaleInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(dv1Detail.DV1_ResaleInfo, "A", YesNoList.Codes.Yes);
			});
		}

		public void TestCheckDV1_ResaleDetails()
		{
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(dv1Detail.DV1_ResaleDetailsInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_Resale = YesNoList.Codes.Yes;
				dv1Detail.Validation.ValidateDV1_ResaleDetails();
				AssertHasMessageErrorContaining(dv1Detail.DV1_ResaleDetailsInfo, MandatoryValidation.YouHaveNotEntered);

				dv1Detail.DV1_ResaleDetails = "Resale Details";
				AssertNoMessageErrorContaining(dv1Detail.DV1_ResaleDetailsInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			dv1Detail = declaration.DV1Details.AddNew();
		}
		CusDV1Detail dv1Detail;
	}
}
