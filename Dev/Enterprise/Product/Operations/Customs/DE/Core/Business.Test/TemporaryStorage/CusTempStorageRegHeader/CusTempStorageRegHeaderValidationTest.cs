using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	public class CusTempStorageRegHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSRH_PreviousReferenceType()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.SRH_PreviousReferenceTypeInfo, "~", PreviousReferenceType.Codes._444T1);
		}

		public void CheckCheckSRH_PresentationDate()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.SRH_PresentationDateInfo);
		}

		public void TestCheckSRH_Reference()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.Validation.ValidateSRH_Reference();

			AssertHasErrorContaining(header.SRH_ReferenceInfo, MandatoryValidation.MustBeEntered);

			header.SRH_Reference = "S01";
			AssertNoErrorContaining(header.SRH_ReferenceInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckSRH_PreviousReference()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.Validation.ValidateSRH_PreviousReference();
			AssertNoMessageErrorContaining(header.SRH_PreviousReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			header.SRH_PreviousReferenceType = PreviousReferenceType.Codes._T;
			header.Validation.ValidateSRH_PreviousReference();
			AssertHasMessageErrorContaining(header.SRH_PreviousReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			header.SRH_PreviousReference = "X001";
			AssertNoMessageErrorContaining(header.SRH_PreviousReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			header.SRH_PreviousReferenceType = PreviousReferenceType.Codes._444T1;
			header.SRH_PreviousReference = string.Empty;
			AssertNoMessageErrorContaining(header.SRH_PreviousReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			header.SRH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
			header.Validation.ValidateSRH_PreviousReference();
			AssertNoMessageErrorContaining(header.SRH_PreviousReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			header.SRH_PreviousReferenceType = PreviousReferenceType.Codes._ESUMA;
			header.Validation.ValidateSRH_PreviousReference();
			AssertNoMessageErrorContaining(header.SRH_PreviousReferenceInfo, MandatoryValidation.YouHaveNotEntered);

			var requireMRNMessageError = "When previous reference type is 'ESUMA', a MRN structure is required (18 alphanumeric characters).";
			header.SRH_PreviousReference = "X001";
			AssertHasMessageErrorContaining(header.SRH_PreviousReferenceInfo, requireMRNMessageError);

			header.SRH_PreviousReference = "11DE11111111111115";
			AssertNoMessageErrorContaining(header.SRH_PreviousReferenceInfo, requireMRNMessageError);

			header.SRH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
			header.SRH_PreviousReference = "X001";
			AssertNoMessageErrorContaining(header.SRH_PreviousReferenceInfo, requireMRNMessageError);
		}

		public void TestCheckSRH_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "MainCustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE1", "Germany", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<CusTempStorageRegHeader>();
			header.Validation.ValidateSRH_CustomsOffice();
			AssertHasMessageErrorContaining(header.SRH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.SRH_CustomsOffice = "DE1";
			header.Validation.ValidateSRH_CustomsOffice();
			AssertNoMessageError(header.SRH_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
