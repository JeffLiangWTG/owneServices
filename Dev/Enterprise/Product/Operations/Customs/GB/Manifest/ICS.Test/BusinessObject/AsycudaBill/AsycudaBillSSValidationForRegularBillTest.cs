using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	public class AsycudaBillSSValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_ManifestUQ()
		{
			bill.ABL_ManifestUQ = "PX";
			AssertNoMessageErrors(bill.ABL_ManifestUQInfo);
		}

		public void TestCheckABL_RL_NKOrigin()
		{
			CombineAssertions(() =>
			{
				bill.ABL_RL_NKOrigin = "AR1";
				AssertNoMessageErrors(bill.ABL_RL_NKOriginInfo);
				bill.ABL_RL_NKOrigin = ZString.Empty;
				AssertNoMessageErrors(bill.ABL_RL_NKOriginInfo);
				bill.ABL_RL_NKOrigin = "123";
				AssertHasMessageErrorContaining(bill.ABL_RL_NKOriginInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckABL_RL_NKFinalDestination()
		{
			CombineAssertions(() =>
			{
				bill.ABL_RL_NKFinalDestination = "AR1";
				AssertNoMessageErrors(bill.ABL_RL_NKFinalDestinationInfo);
				bill.ABL_RL_NKFinalDestination = ZString.Empty;
				AssertNoMessageErrors(bill.ABL_RL_NKFinalDestinationInfo);
				bill.ABL_RL_NKFinalDestination = "123";
				AssertHasMessageErrorContaining(bill.ABL_RL_NKFinalDestinationInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckABL_NotifyPartyName()
		{
			CombineAssertions(() =>
			{
				bill.ABL_OA_NotifyParty = ZGuid.Empty;
				bill.ABL_NotifyPartyPhone = "123456789";
				bill.ABL_NotifyPartyName = ZString.Empty;
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, "The field Notify Party Name is required for a valid Notify Party submission");

				bill.ABL_NotifyPartyName = "Test";
				AssertNoMessageErrors(bill.ABL_NotifyPartyNameInfo);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				bill.ABL_OA_NotifyParty = address.PK;
				bill.ABL_NotifyPartyName = ZString.Empty;
				bill.ABL_NotifyPartyPhone = ZString.Empty;
				AssertNoMessageErrors(bill.ABL_NotifyPartyNameInfo);
			});
		}

		public void TestCheckABL_NotifyPartyStreet1()
		{
			CombineAssertions(() =>
			{
				bill.ABL_OA_NotifyParty = ZGuid.Empty;
				bill.ABL_NotifyPartyRegNo = "1234";
				bill.ABL_NotifyPartyStreet1 = ZString.Empty;
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, "The field Notify Party Street 1 is required for a valid Notify Party submission");

				bill.ABL_NotifyPartyStreet1 = "Test";
				AssertNoMessageErrors(bill.ABL_NotifyPartyStreet1Info);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				bill.ABL_OA_NotifyParty = address.PK;
				bill.ABL_NotifyPartyStreet1 = ZString.Empty;
				bill.ABL_NotifyPartyRegNo = ZString.Empty;
				AssertNoMessageErrors(bill.ABL_NotifyPartyStreet1Info);
			});
		}

		public void TestCheckABL_NotifyPartyCity()
		{
			CombineAssertions(() =>
			{
				bill.ABL_OA_NotifyParty = ZGuid.Empty;
				bill.ABL_NotifyPartyState = "ST";
				bill.ABL_NotifyPartyCity = ZString.Empty;
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, "The field Notify Party City is required for a valid Notify Party submission");

				bill.ABL_NotifyPartyCity = "CI";
				AssertNoMessageErrors(bill.ABL_NotifyPartyCityInfo);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				bill.ABL_OA_NotifyParty = address.PK;
				bill.ABL_NotifyPartyCity = ZString.Empty;
				bill.ABL_NotifyPartyState = ZString.Empty;
				AssertNoMessageErrors(bill.ABL_NotifyPartyCityInfo);
			});
		}

		public void TestCheckABL_RN_NKNotifyPartyCountry()
		{
			CombineAssertions(() =>
			{
				bill.ABL_OA_NotifyParty = ZGuid.Empty;
				bill.ABL_NotifyPartyPostcode = "AB1CD2";
				bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
				AssertHasMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, "The field Notify Party Country/Region is required for a valid Notify Party submission");

				bill.ABL_RN_NKNotifyPartyCountry = "GB";
				AssertNoMessageErrors(bill.ABL_RN_NKNotifyPartyCountryInfo);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				bill.ABL_OA_NotifyParty = address.PK;
				bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
				bill.ABL_NotifyPartyPostcode = ZString.Empty;
				AssertNoMessageErrors(bill.ABL_RN_NKNotifyPartyCountryInfo);
			});
		}

		public void TestCheckABL_NotifyPartyPostcode()
		{
			CombineAssertions(() =>
			{
				bill.ABL_OA_NotifyParty = ZGuid.Empty;
				bill.ABL_NotifyPartyName = "Name";
				bill.ABL_NotifyPartyPostcode = ZString.Empty;
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, "The field Notify Party Postcode is required for a valid Notify Party submission");

				bill.ABL_NotifyPartyPostcode = "AB1CD2";
				AssertNoMessageErrors(bill.ABL_NotifyPartyPostcodeInfo);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				bill.ABL_OA_NotifyParty = address.PK;
				bill.ABL_NotifyPartyPostcode = ZString.Empty;
				bill.ABL_NotifyPartyName = ZString.Empty;
				AssertNoMessageErrors(bill.ABL_NotifyPartyPostcodeInfo);
			});
		}

		public void TestCheckABL_OA_Consignee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var zzCon = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, "Consignee", "A Consignee is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zzCon.PK, "MANDATORY", "");
			Factory.Save();

			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			bill.ABL_OA_Consignee = ZGuid.Empty;

			void AssertAllFieldsHaveError(string message)
			{
				bill.Validation.ValidateAll();
				CombineAssertions(message, () =>
				{
					AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, "A Consignee is required");
					AssertHasMessageErrorContaining(bill.ABL_ConsigneeNameInfo, "A Consignee is required");
					AssertHasMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, "A Consignee is required");
					AssertHasMessageErrorContaining(bill.ABL_ConsigneeCityInfo, "A Consignee is required");
					AssertHasMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, "A Consignee is required");
					AssertHasMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, "A Consignee is required");
				});
			}

			AssertAllFieldsHaveError("Error when neither notify party nor consignee are set");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			bill.ABL_OA_NotifyParty = address.PK;

			void AssertAllFieldsDoNotHaveError(string message)
			{
				bill.Validation.ValidateAll();
				CombineAssertions(message, () =>
				{
					AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);
					AssertNoMessageErrors(bill.ABL_ConsigneeNameInfo);
					AssertNoMessageErrors(bill.ABL_ConsigneeStreet1Info);
					AssertNoMessageErrors(bill.ABL_ConsigneeCityInfo);
					AssertNoMessageErrors(bill.ABL_RN_NKConsigneeCountryInfo);
					AssertNoMessageErrors(bill.ABL_ConsigneePostcodeInfo);
				});
			}

			AssertAllFieldsDoNotHaveError("No error when org is used for notify party");

			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			bill.ABL_NotifyPartyName = "Test";
			AssertAllFieldsHaveError("Error, only notify party Name field is populated");
			bill.ABL_NotifyPartyStreet1 = "Test";
			AssertAllFieldsHaveError("Error, only notify party Name+Street1 fields populated");
			bill.ABL_NotifyPartyCity = "Test";
			AssertAllFieldsHaveError("Error, only notify party Name+Street1+city fields populated");
			bill.ABL_RN_NKNotifyPartyCountry = "GB";
			AssertAllFieldsHaveError("Error, only notify party Name+Street1+city+country fields populated");
			bill.ABL_NotifyPartyPostcode = "AB1CD2";
			AssertAllFieldsDoNotHaveError("No error when notify party Name, Street1, City, and Country fields are populated");
		}

		public void TestWarningAgainstBothConsigneeAndNotifyParty()
		{
			const string message = "The manifest will be rejected if both a consignee and notify party are present";
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = orgHeader1.Addresses.AddNew();
			bill.ABL_OA_Consignee = address1.PK;
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = orgHeader2.Addresses.AddNew();
			bill.ABL_OA_NotifyParty = address2.PK;

			bill.Validation.ValidateAll();
			CombineAssertions("Both consignee and notify party OA fields set", () =>
			{
				AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, message);
			});

			bill.ABL_OA_Consignee = ZGuid.Empty;

			bill.Validation.ValidateAll();
			CombineAssertions("Only OA_Consignee set", () =>
			{
				AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, message);
				AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, message);
			});

			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			bill.ABL_OA_Consignee = address1.PK;

			bill.Validation.ValidateAll();
			CombineAssertions("Only ABL_OA_NotifyParty set", () =>
			{
				AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, message);
				AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, message);
			});

			bill.ABL_NotifyPartyName = "Test";
			bill.ABL_NotifyPartyStreet1 = "Test";
			bill.ABL_NotifyPartyCity = "Test";
			bill.ABL_RN_NKNotifyPartyCountry = "GB";
			bill.ABL_NotifyPartyPostcode = "AB1CD2";

			bill.Validation.ValidateAll();
			CombineAssertions("OA_Consignee set, notify party fields set", () =>
			{
				AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, message);
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, message);
			});

			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_OA_NotifyParty = address2.PK;
			bill.ABL_ConsigneeName = "Test";
			bill.ABL_ConsigneeStreet1 = "Test";
			bill.ABL_ConsigneeCity = "Test";
			bill.ABL_RN_NKConsigneeCountry = "GB";
			bill.ABL_ConsigneePostcode = "AB1CD2";

			bill.Validation.ValidateAll();
			CombineAssertions("OA_NotifyParty set, consignee fields set", () =>
			{
				AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_ConsigneeNameInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, message);
				AssertHasMessageErrorContaining(bill.ABL_ConsigneeCityInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, message);
				AssertHasMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeaderSS>();
			header.AMA_ManifestType = "S&S";
			bill = (AsycudaBillSS)header.Bills.AddNew();

			var packageTypeCodes = new List<string> { "1A", "PX", "ZZ" };

			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes,
				"Package types for test");
			packageTypeCodes.ForEach(li => helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				li,
				$"Package type {li} for test",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime));

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AR1";
			unloco.RL_IATA = "XX1";

			Factory.Save();
		}

		AsycudaManifestHeaderSS header;
		AsycudaBillSS bill;
	}
}
