using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusGuaranteeHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_SubType()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = "COM";

			CombineAssertions(() =>
			{
				guaranteeHeader.Validation.ValidateCPH_SubType();
				AssertNoMessageErrors("CPH_Type isn't TRA", guaranteeHeader.CPH_SubTypeInfo);

				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(guaranteeHeader.CPH_SubTypeInfo);
			});
		}

		public void TestCheckCPH_SubType_MainAccessCode()
		{
			const string message = "You have not entered a Main Access Code.";
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;

			CombineAssertions(() =>
			{
				guaranteeHeader.Validation.ValidateCPH_SubType();
				AssertNoMessageErrorContaining("CPH_SubType is empty", guaranteeHeader.CPH_SubTypeInfo, message);

				guaranteeHeader.CPH_SubType = GuaranteeSubTypeList.Codes._0;
				AssertHasMessageErrorContaining("CPH_SubType is 0 and MainAccessCode is empty", guaranteeHeader.CPH_SubTypeInfo, message);

				guaranteeHeader.MainAccessCode = "123";
				guaranteeHeader.Validation.ValidateCPH_SubType();
				AssertNoMessageErrorContaining("CPH_SubType is 0 and MainAccessCode isn't empty", guaranteeHeader.CPH_SubTypeInfo, message);
			});
		}

		public void TestCheckCPH_OH_PermitHolder()
		{
			const string message = "The Guarantee Holder must have a Registration Number / Code of Type 'EOR'.";
			var header = Factory.New<OrgHeader>();
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = "COM";

			CombineAssertions(() =>
			{
				guaranteeHeader.CPH_OH_PermitHolder = header.PK;
				AssertNoMessageErrorContaining("CPH_Type isn't TRA", guaranteeHeader.CPH_OH_PermitHolderInfo, message);

				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				guaranteeHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertHasMessageErrorContaining("CPH_Type is TRA", guaranteeHeader.CPH_OH_PermitHolderInfo, message);

				header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789012345", Core.Constants.CountryCodes.Greece);
				guaranteeHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertNoMessageErrorContaining("CPH_Type is TRA and Holder has a Registration Number", guaranteeHeader.CPH_OH_PermitHolderInfo, message);
			});
		}

		public void TestCheckMainAccessCode()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.MainAccessCode = "123";
			guaranteeHeader.Validation.ValidateMainAccessCode();
			AssertHasMessageError(guaranteeHeader.MainAccessCodeInfo, "The Main Access Code must have 4 digits.");

			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.MainAccessCode = "1234";
			guaranteeHeader.Validation.ValidateMainAccessCode();
			AssertNoMessageError(guaranteeHeader.MainAccessCodeInfo, "The Main Access Code must have 4 digits.");

			guaranteeHeader.CPH_Type = "COM";
			guaranteeHeader.MainAccessCode = "123";
			guaranteeHeader.Validation.ValidateMainAccessCode();
			AssertNoMessageError(guaranteeHeader.MainAccessCodeInfo, "The Main Access Code must have 4 digits.");
		}

		public void TestCheckCPH_Number_ApplyRuleTR0301_17Characters()
		{
			const string messageError17Characters = "Guarantee reference must be 17 characters long";
			const string invalidNumber = "1";
			var validNumber17Characters = new string('1', 17);
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			CombineAssertions(() =>
			{
				foreach (var subType in new[] { GuaranteeSubTypeList.Codes._0, GuaranteeSubTypeList.Codes._1, GuaranteeSubTypeList.Codes._2 })
				{
					guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
					guaranteeHeader.CPH_SubType = subType;
					guaranteeHeader.CPH_Number = invalidNumber;
					AssertHasMessageErrorContaining($"TRA-{subType}: invalid", guaranteeHeader.CPH_NumberInfo, messageError17Characters);

					guaranteeHeader.CPH_Number = validNumber17Characters;
					AssertNoMessageErrorContaining($"TRA-{subType}: valid", guaranteeHeader.CPH_NumberInfo, messageError17Characters);

					guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.COD;
					guaranteeHeader.CPH_Number = invalidNumber;
					AssertNoMessageErrorContaining($"COD-{subType}", guaranteeHeader.CPH_NumberInfo, messageError17Characters);
				}
			});
		}

		public void TestCheckCPH_Number_ApplyRuleTR0301_24Characters()
		{
			const string messageError24Characters = "Guarantee reference must be 24 characters long";
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			CombineAssertions(() =>
			{
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				guaranteeHeader.CPH_SubType = GuaranteeSubTypeList.Codes._4;
				guaranteeHeader.CPH_Number = new string('1', 24);
				AssertNoMessageErrorContaining($"TRA-4: valid", guaranteeHeader.CPH_NumberInfo, messageError24Characters);

				guaranteeHeader.CPH_Number = "1";
				AssertHasMessageErrorContaining($"TRA-4: invalid", guaranteeHeader.CPH_NumberInfo, messageError24Characters);

				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.COD;
				guaranteeHeader.CPH_Number = "1";
				AssertNoMessageErrorContaining($"COD-4", guaranteeHeader.CPH_NumberInfo, messageError24Characters);
			});
		}
	}
}
