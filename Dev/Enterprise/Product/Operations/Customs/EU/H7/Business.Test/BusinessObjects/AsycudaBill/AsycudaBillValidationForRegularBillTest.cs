using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_ConsigneeRegNo()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "PL", "Poland", yesterday, tomorrow);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_ConsigneeRegNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors("No message errors as ABL_ConsigneeRegNoType is not EOR", bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_ConsigneeRegNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

			bill.ABL_ConsigneeRegNo = "testing";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, br3181RuleEUMemberStateMessage);

			bill.ABL_ConsigneeRegNo = "PL123456789123456";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors("No message errors as ABL_ConsigneeRegNoType is EOR and ABL_ConsigneeRegNoTypeInfo is valid ", bill.ABL_ConsigneeRegNoInfo);
		}

		public void TestCheckABL_ConsigneeName()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			bill.ABL_ConsigneeName = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeName();
			AssertHasMessageError(bill.ABL_ConsigneeNameInfo, "Please enter an Importer Name or declare a valid Identification No.");

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			bill.ABL_ConsigneeName = "FilledConsigneeName";
			bill.Validation.ValidateABL_ConsigneeName();
			AssertNoMessageErrors("No message errors as ConsigneeName is filled", bill.ABL_ConsigneeNameInfo);

			bill.ABL_ConsigneeRegNo = "Testing";
			bill.ABL_ConsigneeName = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeName();
			AssertNoMessageErrors("No message errors as ABL_ConsigneeRegNo is filled", bill.ABL_ConsigneeNameInfo);
		}

		public void TestCheckABL_ConsigneeStreet1()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ConsigneeStreet1();
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet1Info, "Please enter an Importer Street Address or declare a valid Identification No.");
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet2Info, "Please enter an Importer Street Address or declare a valid Identification No.");
		}

		public void TestCheckABL_ConsigneeStreet2()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ConsigneeStreet2();
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet1Info, "Please enter an Importer Street Address or declare a valid Identification No.");
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet2Info, "Please enter an Importer Street Address or declare a valid Identification No.");
		}

		public void TestCheckABL_ConsigneePostCode()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertHasMessageError(bill.ABL_ConsigneePostcodeInfo, "Please enter an Importer Postcode or declare a valid Identification No.");

			bill.ABL_ConsigneeRegNo = "ABCDEFGHIJKLMNOPQ";
			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertNoMessageErrors("No message errors as ABL_ConsigneeRegNo is not blank and not invalid", bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeRegNo = null;
			bill.ABL_ConsigneePostcode = "123456";
			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertNoMessageErrors("No message errors as ABL_ConsigneePostcodeInfo is not blank", bill.ABL_ConsigneePostcodeInfo);
		}

		public void TestCheckABL_ConsigneeCity()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_ConsigneeRegNo = "";
			bill.ABL_ConsigneeCity = "";
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertHasMessageError(bill.ABL_ConsigneeCityInfo, "Please enter an Importer City or declare a valid Identification No.");

			bill.ABL_ConsigneeRegNo = "TestConsigneeRegN";
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrors("No message errors as ABL_ConsigneeRegNo is not blank and not invalid", bill.ABL_ConsigneeCityInfo);

			bill.ABL_ConsigneeRegNo = null;
			bill.ABL_ConsigneeCity = "Test City";
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrors("No message errors as ABL_ConsigneeCity is not blank", bill.ABL_ConsigneeCityInfo);
		}

		public void TestCheckABL_RN_NKConsigneeCountry()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Ireland, "AI008", "VA", "Vatican City", yesterday, tomorrow);

			Factory.Save();

			var bill = Factory.New<AsycudaBill>();

			bill.ABL_RN_NKConsigneeCountry = "VA";
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrors("No message errors as ABL_RN_NKConsigneeCountry is valid", bill.ABL_RN_NKConsigneeCountryInfo);

			bill.ABL_ConsigneeRegNo = "TestConsigneeRegN";
			bill.ABL_RN_NKConsigneeCountry = null;
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrors("No message errors as ABL_ConsigneeRegNo is not blank and not invalid", bill.ABL_RN_NKConsigneeCountryInfo);

			bill.ABL_ConsigneeRegNo = null;
			bill.ABL_RN_NKConsigneeCountry = null;
			bill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, "Please enter an Importer Country/Region or declare a valid Identification No.");

			bill.ABL_ConsigneeRegNo = null;
			bill.ABL_RN_NKConsigneeCountry = "AB";
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, "Please enter a valid Importer Country/Region.");
		}

		public void TestCheckABL_RN_NKShipperCountry()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("No message errors as it is not mandatory for EUCDM", bill.ABL_RN_NKShipperCountryInfo);

			bill.ABL_RN_NKShipperCountry = "AU";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("No message errors as AU is in the list", bill.ABL_RN_NKShipperCountryInfo);

			bill.ABL_RN_NKShipperCountry = "XY";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasMessageError(bill.ABL_RN_NKShipperCountryInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckABL_ShipperStreet1()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrors("No message errors as sum of street 1 and 2 are less than 70 characters", bill.ABL_ShipperStreet1Info);

			bill.ABL_ShipperStreet1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWX";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrors("No message errors as the above is exactly 50 characters (max length of street 1)", bill.ABL_ShipperStreet1Info);

			bill.ABL_ShipperStreet2 = "ABCDEFGHIJKLMNOPQRST";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrors("No message errors as the sum is exactly 70 characters (street 2 is 20 characters)", bill.ABL_ShipperStreet1Info);

			bill.ABL_ShipperStreet2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertHasMessageError("Both fields will have message error", bill.ABL_ShipperStreet1Info, "The total characters for Street 1 and Street 2 cannot exceed 70.");
			AssertHasMessageError("Both fields will have message error", bill.ABL_ShipperStreet2Info, "The total characters for Street 1 and Street 2 cannot exceed 70.");
		}

		public void TestCheckABL_ShipperStreet2()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors("No message errors as sum of street 1 and 2 are less than 70 characters", bill.ABL_ShipperStreet2Info);

			bill.ABL_ShipperStreet2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWX";
			bill.Validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors("No message errors as the above is exactly 50 characters (max length of street 2)", bill.ABL_ShipperStreet2Info);

			bill.ABL_ShipperStreet1 = "ABCDEFGHIJKLMNOPQRST";
			bill.Validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors("No message errors as the sum is exactly 70 characters (street 1 is 20 characters)", bill.ABL_ShipperStreet2Info);

			bill.ABL_ShipperStreet1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			bill.Validation.ValidateABL_ShipperStreet2();
			AssertHasMessageError("Both fields will have message error", bill.ABL_ShipperStreet1Info, "The total characters for Street 1 and Street 2 cannot exceed 70.");
			AssertHasMessageError("Both fields will have message error", bill.ABL_ShipperStreet2Info, "The total characters for Street 1 and Street 2 cannot exceed 70.");
		}

		public void TestCheckABL_SellerRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			var haveNotEnteredIOSSNumberMessage = "You have not entered a Seller IOSS number.";
			var formatErrorMessage = "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOSS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit.";

			bill.ABL_OA_Seller = ZGuid.Empty;
			bill.Validation.ValidateABL_SellerRegNo();
			AssertNoMessageErrors("No message errors", bill.ABL_SellerRegNoInfo);

			bill.ABL_Procedure = "C08";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertNoMessageErrors("ABL_Procedure is not C07+F48", bill.ABL_SellerRegNoInfo);

			bill.ABL_Procedure = "C07+F48";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertHasMessageError("ABL_Procedure is C07+F48 and SellerRegNo is empty", bill.ABL_SellerRegNoInfo, haveNotEnteredIOSSNumberMessage);

			bill.ABL_SellerRegNo = "111111111";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertHasMessageError("Format is incorrect", bill.ABL_SellerRegNoInfo, formatErrorMessage);

			bill.ABL_SellerRegNo = "IM1231231231";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertNoMessageErrors("Format is correct", bill.ABL_SellerRegNoInfo);

			var seller = Factory.New<OrgAddress>();
			bill.ABL_OA_Seller = seller.PK;
			bill.Validation.ValidateABL_SellerRegNo();
			AssertHasMessageError("Seller is added but no IOSS number", bill.ABL_SellerRegNoInfo, haveNotEnteredIOSSNumberMessage);

			bill.ABL_SellerRegNo = "abc";
			bill.Validation.ValidateABL_SellerRegNo();
			AssertNoMessageErrors("ABL_SellerRegNo is entered and org party is added", bill.ABL_SellerRegNoInfo);
		}

		public void TestGoodsLocationDescription()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = bill.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "ABL";
			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;
			bill.ValidateGoodsLocationDescription();
			AssertHasMessageErrorContaining(bill.GoodsLocationDescriptionInfo, "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.");
		}

		public void TestABL_TransportValue()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_TransportValue = 1234567890123.11;

			bill.Validation.ValidateABL_TransportValue();
			AssertHasMessageError(bill.ABL_TransportValueInfo, "The number 1,234,567,890,123.11 is too large, the maximum allowed for Transport Value is 999,999,999,999.99.");

			bill.ABL_TransportValue = -1;
			bill.Validation.ValidateABL_TransportValue();
			AssertHasErrorContaining(bill.ABL_TransportValueInfo, MandatoryValidation.ValueCannotBeNegative);

			bill.ABL_TransportValue = 123.11;
			bill.Validation.ValidateABL_TransportValue();
			AssertNoMessageErrors(bill.ABL_TransportValueInfo);
		}

		public void TestABL_InsuranceValue()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_InsuranceValue = 1234567890123.11;

			bill.Validation.ValidateABL_InsuranceValue();
			AssertHasMessageError(bill.ABL_InsuranceValueInfo, "The number 1,234,567,890,123.11 is too large, the maximum allowed for Insurance Value is 999,999,999,999.99.");

			bill.ABL_InsuranceValue = -1;
			bill.Validation.ValidateABL_InsuranceValue();
			AssertHasErrorContaining(bill.ABL_InsuranceValueInfo, MandatoryValidation.ValueCannotBeNegative);

			bill.ABL_InsuranceValue = 123.11;
			bill.Validation.ValidateABL_InsuranceValue();
			AssertNoMessageErrors(bill.ABL_InsuranceValueInfo);
		}

		public void TestTransportAndInsuranceValue_ShouldNotBothBeBlank()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var msg = "You have not entered a Transport Value and/or Insurance Value.";

			bill.Validation.ValidateABL_TransportValue();
			bill.Validation.ValidateABL_InsuranceValue();
			CombineAssertions("When both are 0, there should be a warning message", () =>
			{
				AssertHasWarning(bill.ABL_TransportValueInfo, msg);
				AssertHasWarning(bill.ABL_InsuranceValueInfo, msg);
			});

			bill.ABL_InsuranceValue = 1;
			bill.Validation.ValidateABL_TransportValue();
			bill.Validation.ValidateABL_InsuranceValue();
			CombineAssertions("When ABL_InsuranceValue is not equal to 0, the warning message will disappear", () =>
			{
				AssertNoWarning(bill.ABL_TransportValueInfo, msg);
				AssertNoWarning(bill.ABL_InsuranceValueInfo, msg);
			});

			bill.ABL_InsuranceValue = 0;
			bill.ABL_TransportValue = 1;
			bill.Validation.ValidateABL_TransportValue();
			bill.Validation.ValidateABL_InsuranceValue();
			CombineAssertions("When ABL_TransportValue is not equal to 0, the warning message will disappear", () =>
			{
				AssertNoWarning(bill.ABL_TransportValueInfo, msg);
				AssertNoWarning(bill.ABL_InsuranceValueInfo, msg);
			});
		}

		public void TestABL_RX_NKTransportValueCurrency()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_TransportValue = 10;
			bill.ABL_RX_NKTransportValueCurrency = string.Empty;
			bill.Validation.ValidateABL_RX_NKTransportValueCurrency();
			AssertHasWarningContaining(bill.ABL_RX_NKTransportValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RX_NKTransportValueCurrency = "ABC";
			bill.Validation.ValidateABL_RX_NKTransportValueCurrency();
			CombineAssertions(() =>
			{
				AssertNoWarningContaining(bill.ABL_RX_NKTransportValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasErrorContaining(bill.ABL_RX_NKTransportValueCurrencyInfo, ListValidation.InvalidCodeError);
			});

			var refCurr = Factory.New<RefCurrency>();
			refCurr.RX_Code = "ABC";
			bill.ABL_RX_NKTransportValueCurrency = "ABC";
			bill.Validation.ValidateABL_RX_NKTransportValueCurrency();
			AssertNoMessageErrors(bill.ABL_RX_NKTransportValueCurrencyInfo);
		}

		public void TestABL_RX_NKInsuranceValueCurrency()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_InsuranceValue = 10;
			bill.ABL_RX_NKInsuranceValueCurrency = string.Empty;
			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			AssertHasWarningContaining(bill.ABL_RX_NKInsuranceValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RX_NKInsuranceValueCurrency = "ABC";
			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			CombineAssertions(() =>
			{
				AssertNoWarningContaining(bill.ABL_RX_NKInsuranceValueCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasErrorContaining(bill.ABL_RX_NKInsuranceValueCurrencyInfo, ListValidation.InvalidCodeError);
			});

			var refCurr = Factory.New<RefCurrency>();
			refCurr.RX_Code = "ABC";
			bill.ABL_RX_NKInsuranceValueCurrency = "ABC";
			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			AssertNoMessageErrors(bill.ABL_RX_NKInsuranceValueCurrencyInfo);
		}

		public void TestCheckABL_GrossWeight()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.Validation.ValidateABL_GrossWeight();
			AssertHasMessageError(bill.ABL_GrossWeightInfo, "You have not entered a Gross Mass.");
		}

		public void TestValidateAtLeastOneItem()
		{
			var bill = Factory.New<AsycudaBill>();
			var message = "You have not entered an Item. At least one Item per Bill is required.";

			bill.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(bill, message);

			bill.PackedItems.AddNew();
			bill.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(bill, message);
		}

		const string br3181RuleEUMemberStateMessage = "[BR3181] The first 2 characters must correspond to a member state of the EU.";
	}
}
