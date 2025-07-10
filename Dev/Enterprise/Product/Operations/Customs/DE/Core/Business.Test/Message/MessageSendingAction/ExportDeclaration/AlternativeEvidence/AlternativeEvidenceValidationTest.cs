using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AlternativeEvidenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(alternativeEvidence.EvidenceTypeInfo, "XX", AlternativeEvidenceTypeList.Codes._11);
		}

		public void TestCheckDocType_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Code_TD44E, Code_TD44E + " DESC");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(alternativeEvidence.DocTypeInfo, "XX", "01");
		}

		public void TestCheckDocType_MandatoryValidation()
		{
			CombineAssertions(() =>
			{
				alternativeEvidence.EvidenceType = AlternativeEvidenceTypeList.Codes._11;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(alternativeEvidence.DocTypeInfo);

				alternativeEvidence.EvidenceType = AlternativeEvidenceTypeList.Codes._16;
				ValidationTestHelper.AssertFieldIsNotMandatory(alternativeEvidence.DocTypeInfo);

				alternativeEvidence.EvidenceType = AlternativeEvidenceTypeList.Codes._30;
				ValidationTestHelper.AssertFieldIsNotMandatory(alternativeEvidence.DocTypeInfo);

				alternativeEvidence.EvidenceType = AlternativeEvidenceTypeList.Codes._31;
				ValidationTestHelper.AssertFieldIsNotMandatory(alternativeEvidence.DocTypeInfo);
			});
		}

		public void TestCheckReference()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Code_TD44E, Code_TD44E + " DESC");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Code_TD44E, "02", "02 DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date,
				UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Code_TD44E, "03", "03 DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date,
				UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, "N");
			Factory.Save();

			CombineAssertions(() =>
			{
				alternativeEvidence.DocType = "01";
				alternativeEvidence.Validation.ValidateReference();
				AssertNoMessageErrorContaining("No 'Reference' attribute", alternativeEvidence.ReferenceInfo, MandatoryValidation.YouHaveNotEntered);

				alternativeEvidence.DocType = "02";
				alternativeEvidence.Validation.ValidateReference();
				AssertHasMessageErrorContaining("Has 'Reference' attribute with value 'Y'", alternativeEvidence.ReferenceInfo, MandatoryValidation.YouHaveNotEntered);

				alternativeEvidence.DocType = "03";
				alternativeEvidence.Validation.ValidateReference();
				AssertNoMessageErrorContaining("Has 'Reference' attribute with value not 'Y'", alternativeEvidence.ReferenceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			alternativeEvidence = new AlternativeEvidence(Factory);
		}
		AlternativeEvidence alternativeEvidence;
	}
}
