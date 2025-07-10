using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForRegularBill))]
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_ConsigneeName()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			bill.ABL_ConsigneeName = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeName();
			AssertHasMessageError(bill.ABL_ConsigneeNameInfo, "You have not entered an Importer Name.");

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
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet1Info, "You have not entered an Importer Street Address.");
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet2Info, "You have not entered an Importer Street Address.");
		}

		public void TestCheckABL_ConsigneeStreet2()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ConsigneeStreet2();
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet1Info, "You have not entered an Importer Street Address.");
			AssertHasMessageError("Both fields will have message error", bill.ABL_ConsigneeStreet2Info, "You have not entered an Importer Street Address.");
		}

		public void TestCheckABL_ConsigneePostCode()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertHasMessageError(bill.ABL_ConsigneePostcodeInfo, "You have not entered an Importer Postcode.");

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
			AssertHasMessageError(bill.ABL_ConsigneeCityInfo, "You have not entered an Importer City.");

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
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			const string mustBeESMsg = "Importer Country/Region must be ‘ES’.";
			const string mustBeEUMsg = "Importer Country/Region must be an EU Country Code.";

			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C08;
			bill.ABL_RN_NKConsigneeCountry = Constants.CountryCodes.Germany;
			AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, mustBeESMsg);
			bill.ABL_RN_NKConsigneeCountry = Constants.CountryCodes.Spain;
			AssertNoMessageError(bill.ABL_RN_NKConsigneeCountryInfo, mustBeESMsg);

			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C07F48;
			bill.ABL_RN_NKConsigneeCountry = Constants.CountryCodes.Brazil;
			AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, mustBeEUMsg);

			var euCountryCodes = Constants.CountryCodes.GetAll().Where(c => Constants.CountryCodes.IsInEuropeanCustomsUnion(c) || c == Constants.CountryCodes.Spain);
			foreach (var countryCode in euCountryCodes)
			{
				bill.ABL_RN_NKConsigneeCountry = countryCode;
				AssertNoMessageError(bill.ABL_RN_NKConsigneeCountryInfo, mustBeEUMsg);
			}
		}

		public void TestCheckABL_BillNumber_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = ZString.Empty;
			AssertHasMessageError(bill.ABL_BillNumberInfo, "You have not entered a Bill Number.");
		}

		public void TestCheckABL_BillNumber_FormatCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			const string formatWarningMsg = "When ES Customs processes G3 messages, non alphanumeric characters will be ignored, and lowercase letters will be accepted but will be converted to uppercase. Eg: Test / 00-1* will be converted and recorded as TEST001.";

			bill.ABL_BillNumber = "123456abc";
			AssertHasWarning(bill.ABL_BillNumberInfo, formatWarningMsg);

			bill.ABL_BillNumber = "123456+-";
			AssertHasWarning(bill.ABL_BillNumberInfo, formatWarningMsg);

			bill.ABL_BillNumber = "123456ABC";
			AssertNoWarning(bill.ABL_BillNumberInfo, formatWarningMsg);
		}

		public void TestCheckABL_Procedure_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_Procedure = ZString.Empty;
			AssertHasMessageError(bill.ABL_ProcedureInfo, "You have not entered an Additional Procedure(s).");
		}

		public void TestCheckABL_ConsigneeRegNo_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			bill.ABL_OA_Consignee = orgAddress.PK;
			const string msg = "You have not entered an Importer Identification No.";

			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C08;

			if (additionalProcedures.Contains(bill.ABL_Procedure))
			{
				Assert("Make sure Additional Procedure(s) is NOT ‘C07+F48’, ‘C35’ or ‘C36’", false);
			}

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, msg);

			bill.ABL_ConsigneeRegNo = "123";
			AssertNoMessageError(bill.ABL_ConsigneeRegNoInfo, msg);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C07F48;
			bill.ABL_ConsigneeRegNoType = "XXX";
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, msg);

			bill.ABL_ConsigneeRegNo = "123";
			AssertNoMessageError(bill.ABL_ConsigneeRegNoInfo, msg);

			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertNoMessageError(bill.ABL_ConsigneeRegNoInfo, msg);
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			const string msg = "Importer ID No. Type must be ‘EOR’ or ‘NIF’.";

			bill.ABL_ConsigneeRegNo = "Test";
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "You have not entered an Importer ID No. Type.");

			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C08;

			if (additionalProcedures.Contains(bill.ABL_Procedure))
			{
				Assert("Make sure Additional Procedure(s) is NOT ‘C07+F48’, ‘C35’ or ‘C36’", false);
			}

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			bill.ABL_OA_Consignee = orgAddress.PK;

			bill.ABL_ConsigneeRegNoType = ESH7ImporterIdentificationTypes.Codes.TIN;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, msg);

			bill.ABL_ConsigneeRegNoType = ESH7ImporterIdentificationTypes.Codes.EOR;
			AssertNoMessageError(bill.ABL_ConsigneeRegNoTypeInfo, msg);

			bill.ABL_ConsigneeRegNoType = ESH7ImporterIdentificationTypes.Codes.NIF;
			AssertNoMessageError(bill.ABL_ConsigneeRegNoTypeInfo, msg);
		}

		public void TestCheckABL_ConsigneePhoneAndABL_ConsigneeEmail_ShouldNotBeBothBlank()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var msg = "You have not entered an Importer Phone Number and/or Importer Email Address.";

			bill.ABL_ConsigneePhone = ZString.Empty;
			bill.ABL_ConsigneeEmail = ZString.Empty;
			CombineAssertions("Both fields should have the same message error.", () =>
			{
				AssertHasMessageError(bill.ABL_ConsigneePhoneInfo, msg);
				AssertHasMessageError(bill.ABL_ConsigneeEmailInfo, msg);
			});

			bill.ABL_ConsigneePhone = "123456";
			CombineAssertions("ConsigneePhone should NOT have the message error, while the ConsigneeEmail should", () =>
			{
				AssertNoMessageError(bill.ABL_ConsigneePhoneInfo, msg);
				AssertHasMessageError(bill.ABL_ConsigneeEmailInfo, msg);
			});

			bill.ABL_ConsigneeEmail = "123@email.com";
			CombineAssertions("Both fields should Not have the same message error.", () =>
			{
				AssertNoMessageError(bill.ABL_ConsigneePhoneInfo, msg);
				AssertNoMessageError(bill.ABL_ConsigneeEmailInfo, msg);
			});
		}

		public void TestCheckABL_ShipperName_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipperName = ZString.Empty;
			AssertHasMessageError(bill.ABL_ShipperNameInfo, "You have not entered an Exporter Name.");
		}

		public void TestCheckABL_ShipperStreet1AndABL_ShipperStreet2_ShouldNotBeBothBlank()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var msg = "You have not entered an Exporter Street Address.";

			bill.ABL_ShipperStreet1 = ZString.Empty;
			bill.ABL_ShipperStreet2 = ZString.Empty;

			CombineAssertions("Both fields should have the same message error.", () =>
			{
				AssertHasMessageError(bill.ABL_ShipperStreet1Info, msg);
				AssertHasMessageError(bill.ABL_ShipperStreet2Info, msg);
			});

			bill.ABL_ShipperStreet1 = "Street1";
			CombineAssertions("Street1 should NOT have the message error while street2 should", () =>
			{
				AssertNoMessageError(bill.ABL_ShipperStreet1Info, msg);
				AssertHasMessageError(bill.ABL_ShipperStreet2Info, msg);
			});

			bill.ABL_ShipperStreet2 = "Street2";
			CombineAssertions("Both fields should Not have the same message error.", () =>
			{
				AssertNoMessageError(bill.ABL_ShipperStreet1Info, msg);
				AssertNoMessageError(bill.ABL_ShipperStreet2Info, msg);
			});
		}

		public void TestCheckABL_ShipperCity_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var msg = "You have not entered an Exporter City.";

			bill.ABL_ShipperCity = ZString.Empty;
			AssertHasMessageError(bill.ABL_ShipperCityInfo, msg);

			bill.ABL_ShipperCity = "city";
			AssertNoMessageError(bill.ABL_ShipperCityInfo, msg);
		}

		public void TestCheckABL_RN_NKShipperCountry_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var msg = "You have not entered an Exporter Country/Region.";

			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			AssertHasMessageError(bill.ABL_RN_NKShipperCountryInfo, msg);

			bill.ABL_RN_NKShipperCountry = Constants.CountryCodes.Spain;
			AssertNoMessageError(bill.ABL_RN_NKShipperCountryInfo, msg);
		}

		public void TestCheckABL_ShipperPostcode_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var msg = "You have not entered an Exporter Postcode.";

			bill.ABL_ShipperPostcode = ZString.Empty;
			AssertHasMessageError(bill.ABL_ShipperPostcodeInfo, msg);

			bill.ABL_ShipperPostcode = "123456";
			AssertNoMessageError(bill.ABL_ShipperPostcodeInfo, msg);
		}

		public void TestCheckSupportingDocuments_Type1018IsRequired()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_Procedure = "C07";
			header.AMA_AgentType = "DIR";
			bill.ABL_ConsigneeRegNo = "123456";

			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when no supporting document", bill, "Please enter a Supporting Documents Reference Number with Type '1018'.");

			var document = bill.SupportingDocuments.AddNew();
			document.CSI_Code = "1234";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("do not need to validate supporting documents as it will be validated by document itself", bill, "Please enter a Supporting Documents Reference Number with Type '1018'.");
			AssertHasMessageErrorContaining("it is validated by document.Validation.CheckCSI_Code()", document.CSI_CodeInfo, "Please enter a Supporting Documents Reference Number with Type '1018'.");
		}

		public void TestCheckSupportingDocuments_RequireAtLeastOneTypeWithN325OrN380()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_Procedure = "C07";

			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when no supporting document", bill, "Please enter a Supporting Documents Reference Number with Type 'N325' and/or 'N380'.");

			var document = bill.SupportingDocuments.AddNew();
			document.CSI_Code = "1234";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("do not need to validate supporting documents as it will be validated by document itself", bill, "Please enter a Supporting Documents Reference Number with Type 'N325' and/or 'N380'.");
			AssertHasMessageErrorContaining("it is validated by document.Validation.CheckCSI_Code()", document.CSI_CodeInfo, "Please enter a Supporting Documents Reference Number with Type 'N325' and/or 'N380'.");
		}

		public void TestCheckSupportingDocuments_RequireAtLeast2SupportingDocumentType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_Procedure = "C07";

			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when no supporting document", bill, "Please enter at least two Supporting Documents Reference Numbers.");

			var document = bill.SupportingDocuments.AddNew();
			document.CSI_Code = "1234";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("do not need to validate supporting documents as it will be validated by document itself", bill, "Please enter at least two Supporting Documents Reference Numbers.");
			AssertHasMessageErrorContaining("it is validated by document.Validation.CheckCSI_Code()", document.CSI_CodeInfo, "Please enter at least two Supporting Documents Reference Numbers.");
		}

		public void TestCheckABL_GrossWeight()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_GrossWeight = 10m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Tonnes;

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_GrossWeight = 500m;
			packedItem1.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_GrossWeight = 3m;
			packedItem2.API_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			var packedItem3 = bill.PackedItems.AddNew();
			packedItem3.API_GrossWeight = 100000m;
			packedItem3.API_GrossWeightUQ = Core.Constants.Weight.Grams;

			Factory.Save();

			bill.Validation.ValidateABL_GrossWeight();
			AssertHasMessageError("There is an error in the field ABL_GrossWeight.", bill.ABL_GrossWeightInfo, string.Format("The sum of gross weight {0} {1} in Items does not match with the total gross mass of the Bill.", 3.6m, Core.Constants.Weight.Tonnes));

			packedItem2.API_GrossWeight = 9.4m;
			bill.Validation.ValidateABL_GrossWeight();
			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
		}

		readonly IZType[] additionalProcedures =
		[
			(ZString)ESH7AdditionalProcedureCodeList.Codes.C07F48,
			(ZString)ESH7AdditionalProcedureCodeList.Codes.C35,
			(ZString)ESH7AdditionalProcedureCodeList.Codes.C36
		];
	}
}
