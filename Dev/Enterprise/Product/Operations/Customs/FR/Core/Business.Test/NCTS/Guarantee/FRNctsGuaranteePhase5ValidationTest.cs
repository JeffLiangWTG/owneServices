using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	sealed class FRNctsGuaranteePhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_BondType_NAT085()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var guarantee = header.MovementHeader.Guarantees.AddNew();

			var messageError = "[NAT085] This guarantee type is not allowed.";
			CombineAssertions("A message error should show when guarantee type is 5, 9, A or J. (FR extension to C0085 rule)", () =>
			{
				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType5;
				AssertHasMessageError(guarantee.PW_BondTypeInfo, messageError);

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType9;
				AssertHasMessageError(guarantee.PW_BondTypeInfo, messageError);

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeA;
				AssertHasMessageError(guarantee.PW_BondTypeInfo, messageError);

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeJ;
				AssertHasMessageError(guarantee.PW_BondTypeInfo, messageError);

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType0;
				AssertNoMessageError(guarantee.PW_BondTypeInfo, messageError);

				guarantee.PW_BondType = "Z";
				AssertListValidationInvalidCodeMessageError(guarantee.PW_BondTypeInfo, true);
			});
		}

		public void TestCheckPW_BondNumber2()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var guarantee = header.MovementHeader.Guarantees.AddNew();

			CombineAssertions("A message error should show when guarantee type is 3 and PW_BondNumber2 is empty. (FR extension to C0086 rule)", () =>
			{
				guarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
				guarantee.PW_BondNumber2 = "1234";
				AssertMandatoryValidationError(guarantee.PW_BondNumber2Info, false);
				guarantee.PW_BondNumber2 = ZString.Empty;
				AssertMandatoryValidationError(guarantee.PW_BondNumber2Info, false);

				guarantee.PW_BondType = EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
				guarantee.PW_BondNumber2 = "1234";
				AssertMandatoryValidationError(guarantee.PW_BondNumber2Info, false);
				guarantee.PW_BondNumber2 = ZString.Empty;
				AssertMandatoryValidationError(guarantee.PW_BondNumber2Info, true);
			});
		}

		public void TestCheckPW_PasswordMandatory_C0086()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var guarantee = header.MovementHeader.Guarantees.AddNew();

			CombineAssertions("Access code is only mandatory for guarantees of type 0, 1, 2, 4, 5 and 9 (last two cases not selectable anyway)", () =>
			{
				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType0;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertHasMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType1;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertHasMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType2;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertHasMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType3;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType4;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertHasMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType6;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType7;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType8;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeB;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeR;
				guarantee.PW_Password = "1234";
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
				guarantee.PW_Password = ZString.Empty;
				AssertNoMessageError(guarantee.PW_PasswordInfo, "[C0086] You have not entered a Access code (GAC).");
			});
		}

		public void TestCheckPW_BondNumberMandatory_C0085()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var guarantee = header.MovementHeader.Guarantees.AddNew();

			CombineAssertions("GRN is only mandatory for guarantees of type 0, 1, 2, 4, 5 and 9 (last two cases not selectable anyway)", () =>
			{
				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType0;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertHasMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType1;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertHasMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType2;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertHasMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType3;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType4;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertHasMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType6;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType7;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeType8;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeB;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");

				guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeR;
				guarantee.PW_BondNumber = "XDFR";
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
				guarantee.PW_BondNumber = ZString.Empty;
				AssertNoMessageError(guarantee.PW_BondNumberInfo, "[C0085] You have not entered a Guarantee Reference Number (GRN).");
			});
		}

		public void TestCheckPW_BondNumberForbiddenForTypeB()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var guarantee = header.MovementHeader.Guarantees.AddNew();

			guarantee.PW_BondType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeList.Codes.GuaranteeTypeB;
			guarantee.PW_BondNumber = "XDFR";
			AssertHasMessageErrorContaining("GRN should be left empty when guarantee type is B.", guarantee.PW_BondNumberInfo, MandatoryValidation.DoNotEntered);
			guarantee.PW_BondNumber = ZString.Empty;
			AssertNoMessageErrorContaining("No message error should show when GRN is empty and guarantee type is B.", guarantee.PW_BondNumberInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckPW_RX_NKCurrency()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var guarantee = header.MovementHeader.Guarantees.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(guarantee.PW_RX_NKCurrencyInfo);
		}

		public void TestCheckPW_BondNumber_R0318()
		{
			var errorMessageNotFlatRateVoucher = "[R0318] Since Guarantee Type is other than \"4\" the GRN should be declared as alphanumeric data 17 characters long.";
			var errorMessageFlatRateVoucher = "[R0318] Since Guarantee Type is \"4\" the GRN should be declared as alphanumeric data 24 characters long.";

			var guaranteeReferenceNumber = GetGuaranteeReferenceNumber();

			CombineAssertions(() =>
			{
				guarantee.PW_BondType = "4";
				guaranteeReferenceNumber.CY_Data = "1234567890123456789012345";
				guarantee.Validation.ValidatePW_BondNumber();
				AssertHasMessageError("BondType equal to 4, BondNumber length not equal to 24.", guarantee.PW_BondNumberInfo, errorMessageFlatRateVoucher);

				guaranteeReferenceNumber.CY_Data = "12345678901234567890123!";
				guarantee.Validation.ValidatePW_BondNumber();
				AssertHasMessageError("BondType equal to 4, BondNumber is not Alphanumeric Data.", guarantee.PW_BondNumberInfo, errorMessageFlatRateVoucher);

				guaranteeReferenceNumber.CY_Data = "123456789012345678901234";
				guarantee.Validation.ValidatePW_BondNumber();
				AssertNoMessageError("BondType equal to 4, BondNumber is Alphanumeric Data and length equal to 24.", guarantee.PW_BondNumberInfo, errorMessageFlatRateVoucher);

				guarantee.PW_BondType = "1";
				guaranteeReferenceNumber.CY_Data = "12345678901234567f8";
				guarantee.Validation.ValidatePW_BondNumber();
				AssertHasMessageError("BondType not equal to 4, BondNumber length not equal to 17.", guarantee.PW_BondNumberInfo, errorMessageNotFlatRateVoucher);

				guaranteeReferenceNumber.CY_Data = "1234567890123456!";
				guarantee.Validation.ValidatePW_BondNumber();
				AssertHasMessageError("BondType not equal to 4, BondNumber is not Alphanumeric Data.", guarantee.PW_BondNumberInfo, errorMessageNotFlatRateVoucher);

				guaranteeReferenceNumber.CY_Data = "12345678901234567";
				guarantee.Validation.ValidatePW_BondNumber();
				AssertNoMessageError("BondType not equal to 4, BondNumber is Alphanumeric Data and length equal to 17.", guarantee.PW_BondNumberInfo, errorMessageNotFlatRateVoucher);
			});
		}

		CusGuaranteeReferenceNumber GetGuaranteeReferenceNumber()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DEC001";
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "DeclarantAddress";
			declarantAddress.Address1 = "Declarant Address";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.Principal.E2_OA_Address = declarantAddress.PK;
			nctsHeader.Declarant.E2_OA_Address = declarantAddress.PK;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = declarant.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guaranteeHeader.CPH_Number = "GRN1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;

			var additionalReference = guaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			additionalReference.CY_Data = "ABC";
			additionalReference.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;

			guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
			Factory.Save();
			return additionalReference;
		}

		NctsGuarantee guarantee;
	}
}
