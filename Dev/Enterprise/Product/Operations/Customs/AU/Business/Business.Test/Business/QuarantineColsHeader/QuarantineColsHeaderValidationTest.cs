using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineColsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckQCH_LateLodgementReason()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("COLLR", "COLS - Late Lodgement Reason", "AU");
			_ = helper.CreateNewOrGetExistingCusCodeList("AU", "COLLR", "3", "Awaiting shipping details", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			ValidationTestHelper.AssertInvalidCodeMessageError(colsHeader.QCH_LateLodgementReasonInfo, "ZZ", "Awaiting shipping details");
		}

		public void TestQCH_DeliveryClassification_ListValidation()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			ValidationTestHelper.AssertInvalidCodeMessageError(colsHeader.QCH_DeliveryClassificationInfo, "ZZ", COLSDeliveryClassificationList.Codes.Metro);
		}

		public void TestCheckDeliveryClassificationIsMandatoryForSplitPostcodes() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("AUPC", "AQIS Postcodes", "AU");
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2001", "Sydney", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2444", "Port Macquarie", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PostcodeDeliveryClassification", "Postcode Delivery Classification", "AUPC", "AU");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, "PostcodeDeliveryClassification", "Metro");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, "PostcodeDeliveryClassification", "Split");
			Factory.Save();

			const string message = "Delivery Classification is mandatory for split postcodes.";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var deliveryOrUnpackAddress = colsHeader.DeliveryOrUnpack;
			deliveryOrUnpackAddress.E2_AddressOverride = true;
			deliveryOrUnpackAddress.E2_Postcode = "2444";
			AssertEquals("Precondition: LRN is empty", ZString.Empty, colsHeader.LRN);
			AssertEquals("Precondition: DeliveryOrUnpack postcode classification is 'Split'", COLSDeliveryClassificationList.Split, colsHeader.DeliveryOrUnpackAddressPostcodeClassification);
			colsHeader.Validation.ValidateQCH_DeliveryClassification();
			AssertHasMessageError("LRN is empty & DeliveryOrUnpackClassification = 'Split'", colsHeader.QCH_DeliveryClassificationInfo, message);

			deliveryOrUnpackAddress.E2_Postcode = "2001";
			AssertEquals("Precondition: DeliveryOrUnpack postcode classification is not 'Split'", COLSDeliveryClassificationList.Codes.Metro, colsHeader.DeliveryOrUnpackAddressPostcodeClassification);
			colsHeader.Validation.ValidateQCH_DeliveryClassification();
			AssertNoMessageError("LRN is empty & DeliveryOrUnpackClassification != 'Split'", colsHeader.QCH_DeliveryClassificationInfo, message);

			var cusEntryNumber1 = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			cusEntryNumber1.CE_EntryNum = "LRN1111";
			deliveryOrUnpackAddress.E2_Postcode = "2444";
			AssertEquals("Precondition: LRN is not empty", "LRN1111", colsHeader.LRN);
			AssertEquals("Precondition: DeliveryOrUnpack postcode classification is 'Split'", COLSDeliveryClassificationList.Split, colsHeader.DeliveryOrUnpackAddressPostcodeClassification);
			colsHeader.Validation.ValidateQCH_DeliveryClassification();
			AssertNoMessageError("LRN is not empty & DeliveryOrUnpackClassification = 'Split'", colsHeader.QCH_DeliveryClassificationInfo, message);
		});

		public void TestCheckDeliveryAddressIsMandatoryForRuralPostcodes() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("AUPC", "AQIS Postcodes", "AU");
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2001", "Sydney", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2770", "Bidwill", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode3 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2444", "Port Macquarie", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PostcodeDeliveryClassification", "Postcode Delivery Classification", "AUPC", "AU");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, "PostcodeDeliveryClassification", "Metro");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, "PostcodeDeliveryClassification", "Rural");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3.PK, "PostcodeDeliveryClassification", "Split");

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var deliveryAddress = deliveryOrg.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "100 Back Laneway";
			deliveryAddress.OA_PostCode = "2444";
			Factory.Save();

			const string message = "Unpack location is mandatory for rural postcodes.";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var deliveryOrUnpackAddress = colsHeader.DeliveryOrUnpack;
			deliveryOrUnpackAddress.E2_AddressOverride = true;
			deliveryOrUnpackAddress.E2_Postcode = "2770";

			AssertEquals("Precondition: QCH_DeliveryClassification is 'Rural'", COLSDeliveryClassificationList.Codes.Rural, colsHeader.QCH_DeliveryClassification);
			AssertEquals("Precondition: CompanyName is empty", ZString.Empty, colsHeader.DeliveryOrUnpackAddressCompanyName);
			AssertHasMessageError("QCH_DeliveryClassification is 'Rural' and CompanyName is empty", colsHeader.QCH_DeliveryClassificationInfo, message);

			deliveryOrUnpackAddress.E2_CompanyName = "MyComp Pty Ltd";
			AssertEquals("Precondition: CompanyName is not empty", "MyComp Pty Ltd", colsHeader.DeliveryOrUnpackAddressCompanyName);
			colsHeader.Validation.ValidateQCH_DeliveryClassification();
			AssertNoMessageError("QCH_DeliveryClassification is 'Rural' and CompanyName is not empty", colsHeader.QCH_DeliveryClassificationInfo, message);

			deliveryOrUnpackAddress.E2_AddressOverride = false;
			AssertEquals("Precondition: DeliveryOrUnpackAddress is empty", ZGuid.Empty, colsHeader.DeliveryOrUnpack.E2_OA_Address);
			colsHeader.QCH_DeliveryClassification = COLSDeliveryClassificationList.Codes.Rural;
			AssertHasMessageError("QCH_DeliveryClassification is 'Rural' and DeliveryOrUnpackAddress is empty", colsHeader.QCH_DeliveryClassificationInfo, message);

			deliveryOrUnpackAddress.E2_OA_Address = deliveryAddress.PK;
			AssertEquals("Precondition: QCH_DeliveryClassification is empty", ZString.Empty, colsHeader.QCH_DeliveryClassification);
			AssertEquals("Precondition: CompanyName is empty", ZString.Empty, colsHeader.DeliveryOrUnpackAddressCompanyName);
			colsHeader.QCH_DeliveryClassification = COLSDeliveryClassificationList.Codes.Metro;
			AssertNoMessageError("QCH_DeliveryClassification is not 'Rural' and CompanyName is empty", colsHeader.QCH_DeliveryClassificationInfo, message);

			colsHeader.QCH_DeliveryClassification = COLSDeliveryClassificationList.Codes.Rural;
			AssertHasMessageError("QCH_DeliveryClassification is not 'Rural' and CompanyName is empty", colsHeader.QCH_DeliveryClassificationInfo, message);
		});

		public void TestCheckQCH_AlsoNotifyEmail()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();

			// check that empty email is accepted
			colsHeader.QCH_AlsoNotifyEmail = "";
			AssertNoErrors("Empty email", colsHeader.QCH_AlsoNotifyEmailInfo);

			// check that valid email is accepted
			colsHeader.QCH_AlsoNotifyEmail = "amy.tester@example.com";
			AssertNoErrors("Valid email", colsHeader.QCH_AlsoNotifyEmailInfo);

			// check that invalid email is rejected
			colsHeader.QCH_AlsoNotifyEmail = "amy.tester.example.com";

			Assert("Invalid email", colsHeader.QCH_AlsoNotifyEmailInfo.HasError("Email Address is not valid ."));
		}
	}
}
