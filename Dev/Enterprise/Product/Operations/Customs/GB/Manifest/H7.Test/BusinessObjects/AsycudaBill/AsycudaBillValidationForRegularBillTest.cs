using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll_ShouldValidateGoodsLocationDescription()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;
			bill.Validation.ValidateAll();
			AssertHasMessageErrorContaining(bill.GoodsLocationDescriptionInfo, "You have not entered a Location of Goods.");
		}

		public void TestCheckAdditionalProcedureCodesAsString()
		{
			var bill = Factory.New<AsycudaBill>();
			var validation = bill.Validation as AsycudaBillValidationForRegularBill;
			var additionalProcedureCode = bill.AdditionalProcedureCodes.AddNew();

			var errorMessage = "Either Additional Procedure Code 4000C07 or 4000C08 need to be entered.";

			additionalProcedureCode.CY_Code = "40001RV";
			validation.ValidateAddititionalProcedureCodeAsString();
			AssertHasMessageError("Has error when neither 4000C07 or 4000C08 selected", bill.AdditionalProcedureCodesAsStringInfo, errorMessage);

			additionalProcedureCode.CY_Code = "4000C07";
			validation.ValidateAddititionalProcedureCodeAsString();
			AssertNoMessageError("Has no error when 4000C07 selected", bill.AdditionalProcedureCodesAsStringInfo, errorMessage);

			additionalProcedureCode.CY_Code = "4000C08";
			validation.ValidateAddititionalProcedureCodeAsString();
			AssertNoMessageError("Has no error when 4000C08 selected", bill.AdditionalProcedureCodesAsStringInfo, errorMessage);
		}

		public void TestCheckABL_GrossWeight_WhenGrossWeightIsZero_ShouldNotShowError()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_GrossWeight = 0;
			bill.Validation.ValidateABL_GrossWeight();
			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
		}

		#region GoodsLocationDescription

		public void TestCheckGoodsLocationDescription_WhenGoodsLocationIsValid_ShouldNotShowError()
		{
			CreateCusCode(code: "ADZZZ");

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;

			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.CGL_Type = "B";
			cusGoodsLocation.CGL_AdditionalIdentifier = "ADZZZ";

			bill.ValidateGoodsLocationDescription();
			AssertNoMessageErrors(bill.GoodsLocationDescriptionInfo);
		}

		public void TestCheckGoodsLocationDescription_WhenGoodsLocationIsEmpty_ShouldShowError()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			bill.ValidateGoodsLocationDescription();
			AssertHasMessageErrorContaining(bill.GoodsLocationDescriptionInfo, "You have not entered a Location of Goods.");
		}

		public void TestCheckGoodsLocationDescription_WhenGoodsLocationIsInvalid_ShouldShowError()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";
			cusGoodsLocation.CGL_Qualifier = "U";
			cusGoodsLocation.CGL_Type = "B";
			cusGoodsLocation.CGL_AdditionalIdentifier = "INV";

			bill.ValidateGoodsLocationDescription();
			AssertHasMessageErrorContaining(bill.GoodsLocationDescriptionInfo, "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.");
		}

		#endregion

		#region TransportValue

		public void TestCheckABL_TransportValue_WhenTransportValueIsValid_ShouldNotShowError()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_TransportValue = 123.11;
			bill.Validation.ValidateABL_TransportValue();
			AssertNoMessageErrors(bill.ABL_TransportValueInfo);
		}

		public void TestCheckABL_TransportValue_WhenTransportValueIsZero_ShouldShowError()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_TransportValue = 0;
			bill.Validation.ValidateABL_TransportValue();
			AssertHasWarning(bill.ABL_TransportValueInfo, "You have not entered a Transport Value.");
		}

		public void TestCheckABL_TransportValue_WhenTransportValueIsInvalid_ShouldShowError()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_TransportValue = 1234567890123.11;
			bill.Validation.ValidateABL_TransportValue();
			AssertHasMessageError(bill.ABL_TransportValueInfo, "The number 1,234,567,890,123.11 is too large, the maximum allowed for Transport Value is 999,999,999,999.99.");

			bill.ABL_TransportValue = -1;
			bill.Validation.ValidateABL_TransportValue();
			AssertHasErrorContaining(bill.ABL_TransportValueInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		#endregion

		public void TestCheckABL_RL_NKOrigin()
		{
			SetupRegionName();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "GBBEL";
			var bill1 = header.Bills.AddNew();
			AssertNoMessageErrorContaining("No enter check errors if Origin is not empty as prefilled", bill1.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			bill1.ABL_RL_NKOrigin = ZString.Empty;
			AssertNoMessageErrorContaining("No enter check errors if Origin is empty and PortOfLoading is in GB and admin region is in NORTHERN IRELAND", bill1.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RL_NKPortOfDischarge = "GBLON";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_RL_NKOrigin = ZString.Empty;
			AssertHasMessageErrorContaining("Has enter check errors if Origin is empty and PortOfLoading is in GB and admin region is not in NORTHERN IRELAND", bill2.ABL_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void CreateCusCode(string dataGroupingCode = "CDS", string codeType = "PORT", string code = "ADZZZ")
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeList(dataGroupingCode, codeType, code, yesterday, tomorrow);

			Factory.Save();
		}

		public void TestCheckABL_ShipperName()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.Validation.ValidateABL_ShipperName();

			AssertHasMessageError(bill.ABL_ShipperNameInfo, "Please enter an Exporter Name.");

			bill.ABL_ShipperName = "aaa";
			bill.Validation.ValidateABL_ShipperName();
			AssertNoMessageErrors("No message errors", bill.ABL_ShipperNameInfo);
		}

		public void TestCheckABL_ShipperStreet1()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.Validation.ValidateABL_ShipperStreet1();

			AssertHasMessageError(bill.ABL_ShipperStreet1Info, "Please enter an Exporter Street Address.");

			bill.ABL_ShipperStreet1 = "ABCDEFGHIJKLMNOPQRST";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrors("No message errors", bill.ABL_ShipperStreet1Info);

			bill.ABL_ShipperStreet1 = ZString.Empty;
			bill.ABL_ShipperStreet2 = "ABCDEFGHIJKLMNOPQRST";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrors("No message errors", bill.ABL_ShipperStreet1Info);
		}

		public void TestCheckABL_ShipperStreet2()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.Validation.ValidateABL_ShipperStreet2();

			AssertHasMessageError(bill.ABL_ShipperStreet2Info, "Please enter an Exporter Street Address.");

			bill.ABL_ShipperStreet2 = "ABCDEFGHIJKLMNOPQRST";
			bill.Validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors("No message errors", bill.ABL_ShipperStreet2Info);

			bill.ABL_ShipperStreet1 = "ABCDEFGHIJKLMNOPQRST";
			bill.ABL_ShipperStreet2 = ZString.Empty;
			bill.Validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors("No message errors", bill.ABL_ShipperStreet2Info);
		}

		public void TestCheckABL_ShipperCity()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.Validation.ValidateABL_ShipperCity();

			AssertHasMessageError(bill.ABL_ShipperCityInfo, "Please enter an Exporter City.");

			bill.ABL_ShipperCity = "city";
			bill.Validation.ValidateABL_ShipperCity();
			AssertNoMessageErrors("No message errors", bill.ABL_ShipperCityInfo);
		}

		public void TestCheckABL_RN_NKShipperCountry()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.Validation.ValidateABL_RN_NKShipperCountry();

			AssertHasMessageError(bill.ABL_RN_NKShipperCountryInfo, "Please enter an Exporter Country/Region.");

			bill.ABL_RN_NKShipperCountry = "CN";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("No message errors", bill.ABL_RN_NKShipperCountryInfo);
		}

		public void TestCheckABL_ShipperPostcode()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.Validation.ValidateABL_ShipperPostcode();

			AssertHasMessageError(bill.ABL_ShipperPostcodeInfo, "Please enter an Exporter Postcode.");

			bill.ABL_ShipperPostcode = "246113";
			bill.Validation.ValidateABL_ShipperPostcode();
			AssertNoMessageErrors("No message errors", bill.ABL_ShipperPostcodeInfo);
		}

		public void TestValidateAdditionalInfos()
		{
			SetupRegionName();
			var nidomRequiredMessage = "Additional Info Code ‘NIDOM’ is required when Load Port is in mainland Great Britain and Discharge Port is in Northern Ireland.";
			var niimpRequiredMessage = "Additional Info Code ‘NIIMP’ is required when Load Port is not in Great Britain and Discharge Port is in Northern Ireland.";
			var mutuallyExclusiveMessage = "Additional Info Codes ‘NIIMP’ and ‘NIDOM’ are mutually exclusive.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var additionalInfo = bill.AdditionalInfos.AddNew();

			additionalInfo.CSI_Type = "HAI";
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "GBBEL";

			bill.Validation.ValidateAll();
			Assert(bill.RowMessageErrors.ContainsNotificationContaining(niimpRequiredMessage));
			Assert(!bill.RowMessageErrors.ContainsNotificationContaining(nidomRequiredMessage));

			header.AMA_RL_NKPortOfLoading = "GBLON";
			header.AMA_RL_NKPortOfDischarge = "GBBEL";

			bill.Validation.ValidateAll();
			Assert(!bill.RowMessageErrors.ContainsNotificationContaining(niimpRequiredMessage));
			Assert(bill.RowMessageErrors.ContainsNotificationContaining(nidomRequiredMessage));

			var additionalInfo1 = bill.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "NIIMP";
			var additionalInfo2 = bill.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "NIDOM";

			bill.Validation.ValidateAll();
			CombineAssertions("Mutually exclusive message added if both NIIMP and NIDPM additional info exists", () =>
			{
				Assert(additionalInfo1.RowMessageErrors.ContainsNotificationContaining(mutuallyExclusiveMessage));
				Assert(additionalInfo2.RowMessageErrors.ContainsNotificationContaining(mutuallyExclusiveMessage));
			});

			additionalInfo2.Delete();
			bill.Validation.ValidateAll();
			Assert("Row message cleared after delete NIDOM additional info", !additionalInfo1.RowMessageErrors.ContainsNotificationContaining(mutuallyExclusiveMessage));

			var additionalInfo3 = bill.AdditionalInfos.AddNew();
			additionalInfo3.CSI_Code = "NIDOM";

			bill.Validation.ValidateAll();
			Assert("Pre-condition: additionalInfo3 has row message", additionalInfo3.RowMessageErrors.ContainsNotificationContaining(mutuallyExclusiveMessage));

			additionalInfo1.Delete();
			bill.Validation.ValidateAll();
			Assert("Row message cleared after delete NIIMP additional info", !additionalInfo3.RowMessageErrors.ContainsNotificationContaining(mutuallyExclusiveMessage));
		}

		public void TestValidateAdditionalInfos_WithAddtionalProcedureCodeAndIOSSNumber()
		{
			var procedureCodeAndIOSSNumberReqErrorMessage = "If ‘Seller IOSS Number’ has been provided and ‘Additional Procedure Code’ contains the value ‘F48’ then ‘Additional Info’ Code must contain the value ‘NIIMP’.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var validation = bill.Validation as AsycudaBillValidationForRegularBill;
			var additionalProcedureCode = bill.AdditionalProcedureCodes.AddNew();
			var additionalInfo = bill.AdditionalInfos.AddNew();

			additionalProcedureCode.CY_Code = "4000F48";
			validation.ValidateAll();
			Assert("Has no target error when procedure code contains 'F48' provided but IOSS number not provided", !bill.RowMessageErrors.ContainsNotificationContaining(procedureCodeAndIOSSNumberReqErrorMessage));

			additionalProcedureCode.CY_Code = "4000C08";
			bill.ABL_SellerRegNo = "IM1234567890";
			validation.ValidateAll();
			Assert("Has no target error when procedure code contains 'F48' not provided but IOSS number provided", !bill.RowMessageErrors.ContainsNotificationContaining(procedureCodeAndIOSSNumberReqErrorMessage));

			additionalProcedureCode.CY_Code = "4000F48";
			bill.ABL_SellerRegNo = "IM1234567890";
			validation.ValidateAll();
			Assert("Has target error when when NIIMP code not provided and procedure code contains 'F48' provided and IOSS number provided", bill.RowMessageErrors.ContainsNotificationContaining(procedureCodeAndIOSSNumberReqErrorMessage));

			additionalInfo.CSI_Code = "NIIMP";
			validation.ValidateAll();
			Assert("Has no target error when NIIMP code provided", !bill.RowMessageErrors.ContainsNotificationContaining(procedureCodeAndIOSSNumberReqErrorMessage));
		}

		public void TestCheckABL_SellerRegNo_WithAddtionalProcedureCodeAndAddtionalInfos()
		{
			var notEnteredErrorMessage = "You have not entered a Seller IOSS number.";
			var invalidFormatErrorMessage = "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOSS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var additionalInfo = bill.AdditionalInfos.AddNew();
			var additionalProcedureCode = bill.AdditionalProcedureCodes.AddNew();

			additionalProcedureCode.CY_Code = "4000F48";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertNoMessageError("Has no target error when no procedure code contains 'F48' provided but NIIMP code not provided", bill.ABL_SellerRegNoInfo, notEnteredErrorMessage);

			additionalProcedureCode.CY_Code = "4000C08";
			additionalInfo.CSI_Code = "NIIMP";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertNoMessageError("Has no target error when no procedure code contains 'F48' provided but NIIMP code provided", bill.ABL_SellerRegNoInfo, notEnteredErrorMessage);

			additionalProcedureCode.CY_Code = "4000F48";
			additionalInfo.CSI_Code = "NIIMP";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertHasMessageError("Has target error when procedure code contains 'F48' provided and NIIMP code provided but IOSS number not provided", bill.ABL_SellerRegNoInfo, notEnteredErrorMessage);

			bill.ABL_SellerRegNo = "IM7890";
			bill.Validation.ValidateABL_SellerRegNo();
			CombineAssertions(() =>
			{
				AssertNoMessageError("Has no target error when procedure code contains 'F48' provided and NIIMP code provided and IOSS number provided", bill.ABL_SellerRegNoInfo, notEnteredErrorMessage);
				AssertHasMessageError("Has target error when IOSS number is not in the correct format", bill.ABL_SellerRegNoInfo, invalidFormatErrorMessage);
			});

			bill.ABL_SellerRegNo = "IM1234567890";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertNoMessageError("Has no target error when IOSS number is in the correct format", bill.ABL_SellerRegNoInfo, invalidFormatErrorMessage);
		}

		public void TestValidatePostponedVatAccountingCheck_WithIOSSNumber()
		{
			var conflictErrorMessage = "Importer VAT number and Seller IOSS number cannot be combined on a single customs declaration.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var validation = bill.Validation as AsycudaBillValidationForRegularBill;

			bill.ABL_SellerRegNo = "IM1234567890";
			bill.PostponedVatAccountingCheck = true;
			validation.ValidatePostponedVatAccountingCheck();
			AssertHasMessageError("Has target error when IOSS number provided", bill.PostponedVatAccountingCheckInfo, conflictErrorMessage);

			bill.ABL_SellerRegNo = ZString.Empty;
			bill.PostponedVatAccountingCheck = true;
			validation.ValidatePostponedVatAccountingCheck();
			AssertNoMessageError("Has no target error when IOSS number not provided", bill.PostponedVatAccountingCheckInfo, conflictErrorMessage);
		}

		void SetupRegionName()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "XXX";
				refCountryStates.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = refCountryStates.PK;
			}

			var london = new RefUNLOCO.Loader(Factory).Load("GBLON");
			if (london.CountryStates == null || string.Compare(london.CountryStates.RW_RegionName, "ENGLAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "YYY";
				refCountryStates.RW_RegionName = "ENGLAND";
				london.RL_RW = refCountryStates.PK;
			}
		}
	}
}
