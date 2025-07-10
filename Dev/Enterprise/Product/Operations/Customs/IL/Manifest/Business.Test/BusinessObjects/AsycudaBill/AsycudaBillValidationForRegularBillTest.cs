using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_ConsigneeRegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "The VAT No. should not empty when Manifest Nature is Import.");

			bill.ABL_ConsigneeRegNo = "1234";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageError(bill.ABL_ConsigneeRegNoInfo, "The VAT No. should not empty when Manifest Nature is Import.");

			bill.ABL_ConsigneeRegNo = "";
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageError(bill.ABL_ConsigneeRegNoInfo, "The VAT No. should not empty when Manifest Nature is Import.");
		}

		public void TestValidateAtLeastOnPackageIsRequired()
		{
			const string expectedMessageError = "At least one package record is required.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Validation.ValidateAll();
			AssertHasRowMessageError("When Packs.Count == 0", bill, expectedMessageError);

			bill.Packs.AddNew();
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("When Packs.Count > 0", bill, expectedMessageError);
		}

		public void TestCheckABL_SequenceNumber_IsMandatory()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			short expectedValue = 1;
			var bill = header.Bills.AddNew();
			AssertEquals("Expected ABL_SequenceNumber: 1, automatically generated", expectedValue, bill.ABL_SequenceNumber);
			bill.Validation.ValidateABL_SequenceNumber();
			AssertNoMessageErrorContaining("No error expected when ABL_SequenceNumber != 0", bill.ABL_SequenceNumberInfo, "You have not entered");

			bill.ABL_SequenceNumber = 0;
			AssertHasMessageErrorContaining("Error expected when ABL_SequenceNumber = 0", bill.ABL_SequenceNumberInfo, "You have not entered");
		}

		public void TestCheckABL_SequenceNumber_MaxThreeDigits()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_SequenceNumber = 1111;
			bill.Validation.ValidateABL_SequenceNumber();
			AssertHasMessageErrorContaining("Max length of ABL_SequenceNumber is 3 digits", bill.ABL_SequenceNumberInfo, "Sequence number must be up to 3 digits");
			bill.ABL_SequenceNumber = 111;
			AssertNoMessageErrorContaining("No error should be when length is of 3 digits or less", bill.ABL_SequenceNumberInfo, "Sequence number must be up to 3 digits");
		}

		public void TestCheckABL_ManifestQty_IsMandatory()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Validation.ValidateABL_ManifestQty();
			AssertHasMessageErrorContaining("ABL_ManifestQty should have blue error when not entered", bill.ABL_ManifestQtyInfo, "You have not entered");
			bill.ABL_ManifestQty = 111;
			AssertNoMessageErrorContaining("ABL_ManifestQty should not have blue error when entered", bill.ABL_ManifestQtyInfo, "You have not entered");
		}

		public void TestCheckABL_ManifestQty_MaxEightDigits()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 111111111;
			bill.Validation.ValidateABL_ManifestQty();
			AssertHasMessageErrorContaining("Max length of ABL_ManifestQty is 8 digits", bill.ABL_ManifestQtyInfo, "Quantity must be up to 8 digits");
			bill.ABL_ManifestQty = 11111111;
			AssertNoMessageErrorContaining("No error should be when length is of 8 digits or less", bill.ABL_ManifestQtyInfo, "Quantity must be up to 8 digits");
		}

		public void TestCheckABL_RL_NKOrigin_IsMandatory()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Validation.ValidateABL_RL_NKOrigin();
			AssertHasMessageErrorContaining("ABL_RL_NKOrigin should have blue error when not entered", bill.ABL_RL_NKOriginInfo, "You have not entered");
			bill.ABL_RL_NKOrigin = "QQQ";
			AssertNoMessageErrorContaining("ABL_RL_NKOrigin should not have blue error when entered", bill.ABL_RL_NKOriginInfo, "You have not entered");
		}

		public void TestCheckABL_RL_NKPortOfDischarge_IsMandatory()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Validation.ValidateABL_RL_NKPortOfDischarge();
			AssertHasMessageErrorContaining("ABL_RL_NKPortOfDischarge should have blue error when not entered", bill.ABL_RL_NKPortOfDischargeInfo, "You have not entered");
			bill.ABL_RL_NKPortOfDischarge = "QQQ";
			AssertNoMessageErrorContaining("ABL_RL_NKPortOfDischarge should not have blue error when entered", bill.ABL_RL_NKPortOfDischargeInfo, "You have not entered");
		}

		public void TestCheckABL_OA_Consignee_IsraelVat()
		{
			TestValidateVATNumberForPartner(
				ShipmentTypeList.Codes.Import23,
				"Consignee",
				(bill, orgAddress) => bill.ABL_OA_Consignee = orgAddress.PK,
				bill => bill.Validation.ValidateABL_OA_Consignee(),
				bill => bill.ABL_OA_ConsigneeInfo,
				"VAT Number is missing for Consignee - Update organization config tab."
			);
		}

		public void TestCheckABL_OA_Shipper_IsraelVat()
		{
			TestValidateVATNumberForPartner(
				ShipmentTypeList.Codes.Export22,
				"Consignor",
				(bill, orgAddress) => bill.ABL_OA_Shipper = orgAddress.PK,
				bill => bill.Validation.ValidateABL_OA_Shipper(),
				bill => bill.ABL_OA_ShipperInfo,
				"VAT Number is missing for Consignor - Update organization config tab."
			);
		}

		public void TestCheckABL_OA_Shipper_IsMandatory()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Validation.ValidateABL_OA_Shipper();
			AssertHasMessageErrorContaining("ABL_OA_Shipper should have blue error when not entered", bill.ABL_OA_ShipperInfo, "You have not entered");
			bill.ABL_OA_Shipper = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining("ABL_OA_Shipper should not have blue error when entered", bill.ABL_OA_ShipperInfo, "You have not entered");
		}

		public void TestValidateAdditionInfos()
		{
			const string expectedMessageError = "Each Bill must include one additional info record with Statement Type = 2 and Consignee 'VAT'";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			bill.Validation.ValidateAll();

			AssertHasRowMessageError("When no Consignee Vat Number additional Info ", bill, expectedMessageError);

			var asycudaAdditionalInfo = bill.AdditionalInfos.AddNew();
			asycudaAdditionalInfo.CSI_Code = "2";
			bill.Validation.ValidateAll();
			AssertHasRowMessageError("When no Valid Consignee Vat Number additional Info ", bill, expectedMessageError);

			asycudaAdditionalInfo.CSI_Description = "565001882";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("When Valid Consignee Vat Number additional Info ", bill, expectedMessageError);
		}

		public void TestCheckABL_Condition_List()
		{
			var expectedErrorMessage = "The code you have selected is not in the list.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_Condition = "YY";
			AssertNoMessageErrorContaining(bill.ABL_ConditionInfo, "You have not entered");
			AssertHasMessageError(bill.ABL_ConditionInfo, expectedErrorMessage);

			bill.ABL_Condition = "";
			AssertHasMessageErrorContaining(bill.ABL_ConditionInfo, "You have not entered");

			bill.ABL_Condition = ILBillConditionList.Codes.DoorToDoor;
			AssertNoMessageErrorContaining(bill.ABL_ConditionInfo, "You have not entered");
			AssertNoMessageError(bill.ABL_ConditionInfo, expectedErrorMessage);
		}

		public void TestCheckABL_ShipperRegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Sea;
			var bill = header.Bills.AddNew();

			CombineAssertions("Pre-condition", () =>
			{
				AssertEquals("Bill does not have ABL_ShipperRegNo", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertNoMessageError(bill.ABL_ShipperRegNoInfo, "Consignor's Reg # is mandatory for road manifest, please update organization’s config tab");
			});

			bill.RunPreSaveValidation();

			CombineAssertions("After validation when empty and SEA", () =>
			{
				AssertEquals("Bill does not have ABL_ShipperRegNo", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertNoMessageError(bill.ABL_ShipperRegNoInfo, "Consignor's Reg # is mandatory for road manifest, please update organization’s config tab");
			});

			header.AMA_TransportMode = TransportModes.Road;

			bill.RunPreSaveValidation();

			CombineAssertions("After validation when empty and ROA", () =>
			{
				AssertEquals("Bill does not have ABL_ShipperRegNo", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertHasMessageError(bill.ABL_ShipperRegNoInfo, "Consignor's Reg # is mandatory for road manifest, please update organization’s config tab");
			});

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TestOrg1";

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "1234", CountryCodes.Latvia);

			var address1 = orgHeader1.Addresses.AddNew();
			address1.OA_RN_NKCountryCode = CountryCodes.Israel;
			address1.Address1 = "Address1";
			address1.OA_OH = orgHeader1.PK;
			bill.ABL_OA_Shipper = address1.PK;

			bill.RunPreSaveValidation();

			CombineAssertions("After validation when Org without RegNo and ROA", () =>
			{
				AssertEquals("Bill does not have ABL_ShipperRegNo", ZString.Empty, bill.ABL_ShipperRegNo);
				AssertHasMessageError(bill.ABL_ShipperRegNoInfo, "Consignor's Reg # is mandatory for road manifest, please update organization’s config tab");
			});

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TestOrg2";
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "1234", CountryCodes.Israel);
			var address2 = orgHeader2.Addresses.AddNew();
			address2.OA_RN_NKCountryCode = CountryCodes.Israel;
			address2.Address1 = "Address2";
			address2.OA_OH = orgHeader2.PK;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			bill.ABL_OA_Shipper = address2.PK;
			bill.RunPreSaveValidation();

			CombineAssertions("After validation when not empty and Import road manifest", () =>
			{
				AssertEquals("Bill does not have ABL_ShipperRegNo", new ZString("1234"), bill.ABL_ShipperRegNo);
				AssertNoMessageError(bill.ABL_ShipperRegNoInfo, "Consignor's Reg # is mandatory for road manifest, please update organization’s config tab");
			});
		}

		public void TestValidateAtLeastOnItemIsRequired()
		{
			const string expectedMessageError = "At least one item record is required.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			bill.Validation.ValidateAll();
			AssertHasRowMessageError("When PackedItems.Count == 0", bill, expectedMessageError);

			var asycudaPackedItem = bill.PackedItems.AddNew();
			asycudaPackedItem.API_Tariff = "Tariff";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("When PackedItems.Count > 0", bill, expectedMessageError);
		}

		public void TestValidateAtLeastOnItemRequired()
		{
			const string expectedMessageError = "A Transport Document record of type IL3 must be provided in road Manifest.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_TransportMode = TransportModes.Road;
			bill.Validation.ValidateAll();
			AssertHasRowMessageError("When no IL3 transport document.", bill, expectedMessageError);

			var transportDocumentInfo = bill.TransportDocuments.AddNew();
			transportDocumentInfo.CSI_Code = "IL3";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("When IL3 transport document exists.", bill, expectedMessageError);
		}

		public void TestValidatePackageFeeTypeExistsWhenRoad()
		{
			const string expectedMessageError = "In ROA Manifest an additional info record 18 must be provided.";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_TransportMode = TransportModes.Road;
			bill.Validation.ValidateAll();
			AssertHasRowMessageError("When no additional info 18", bill, expectedMessageError);

			var additionalInfo = bill.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "18";
			additionalInfo.CSI_Description = "2";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("When no additional info 18 exists", bill, expectedMessageError);
		}

		void TestValidateVATNumberForPartner(
			ZString nature,
			string orgCode,
			Action<AsycudaBill, OrgAddress> updatePartner,
			Action<AsycudaBill> validatePartner,
			Func<AsycudaBill, ZPropertyInfo> getPartnerPropertyInfo,
			string expectedMessageError)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = nature;
			var bill = header.Bills.AddNew();
			var targetPropertyInfo = getPartnerPropertyInfo(bill);
			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = orgCode;
			orgAddress.OA_OH = orgHeader.PK;

			CombineAssertions($"When {nature}", () =>
			{
				AssertNoMessageError($"When {orgCode} is empty", targetPropertyInfo, expectedMessageError);

				updatePartner(bill, orgAddress);
				AssertHasMessageError($"When {orgCode} without VAT", targetPropertyInfo, expectedMessageError);

				orgHeader.CustomsCodes.AddNew("VAT", "1", "IN");
				validatePartner(bill);
				AssertHasMessageError($"When {orgCode} with VAT but not for Israel", targetPropertyInfo, expectedMessageError);

				orgHeader.CustomsCodes.AddNew("VAT", "", "IL");
				validatePartner(bill);
				AssertHasMessageError($"When {orgCode} with VAT for Israel without Reg No", targetPropertyInfo, expectedMessageError);
				orgHeader.CustomsCodes.RemoveAndDeleteAll();

				orgHeader.CustomsCodes.AddNew("VAT", "520017146", "IL");
				validatePartner(bill);
				AssertNoMessageError($"When {orgCode} with VAT for Israel with Reg No", targetPropertyInfo, expectedMessageError);
			});

			header.AMA_Nature = nature == ShipmentTypeList.Codes.Import23 ? ShipmentTypeList.Codes.Export22 : ShipmentTypeList.Codes.Import23;
			CombineAssertions($"When {header.AMA_Nature}", () =>
			{
				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				validatePartner(bill);
				AssertNoMessageError($"When {orgCode} without VAT", targetPropertyInfo, expectedMessageError);
			});
		}
	}
}
