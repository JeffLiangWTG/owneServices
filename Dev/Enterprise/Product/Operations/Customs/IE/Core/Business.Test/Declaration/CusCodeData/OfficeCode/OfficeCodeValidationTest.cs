using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using OfficeCode = Enterprise.Customs.IE.Business.Declaration.OfficeCode;

namespace Enterprise.Customs.IE.Business.Testing
{
	class OfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code_ErrorIfNotEntered()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(officeCode.CY_CodeInfo);
		}

		public void TestCheckCY_Code_ErrorIfInvalidCode()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(officeCode.CY_CodeInfo, "~", EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		}

		public void TestCheckCY_Data_ErrorIfNotWesternEuropean()
		{
			var message = EnglishCharactersValidation.GetNotificationMessage(officeCode.CY_DataInfo);
			CombineAssertions(() =>
			{
				officeCode.CY_Data = "abc \u069A";
				AssertHasError("Not Western European", officeCode.CY_DataInfo, message);
				officeCode.CY_Data = "IE000001";
				AssertNoError("Western European", officeCode.CY_DataInfo, message);
			});
		}

		public void TestDuplicatePurpose()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
			var customsOffice2 = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
			var duplicateMessage = "Only one office of type PRE is allowed";
			AssertHasError(customsOffice2.CY_CodeInfo, duplicateMessage);
			customsOffice2.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			AssertNoError(customsOffice2.CY_CodeInfo, duplicateMessage);
		}

		public void TestCheckCY_Data_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE000001", "IE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var emptyText = "An office code is needed. Example: FR000010.";
			var invalidText = "Entered office code is not a valid office";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			officeCode = declaration.CustomsOffices.GetPresentationOffice();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(officeCode.CY_DataInfo, emptyText);
			ValidationTestHelper.AssertInvalidCodeMessageError(officeCode.CY_DataInfo, "IE999999", "IE000001", invalidText);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			officeCode.CY_Data = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(officeCode.CY_DataInfo, emptyText);
			ValidationTestHelper.AssertInvalidCodeMessageError(officeCode.CY_DataInfo, "IE999999", "IE000001", invalidText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			officeCode = declaration.CustomsOffices.AddNew();
			officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		}

		JobDeclaration declaration;
		OfficeCode officeCode;
	}
}
