using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business.Testing;

public class AsycudaBillSSValidationForMasterChildTest : BusinessObjectValidationTestCase
{
	public void TestCheckABL_BillNumber()
	{
		var bill = header.MasterBill;
		header.AMA_TransportMode = GBSSTransportTypeList.Codes.AirFreight;
		header.Validation.ValidateAll();
		AssertHasMessageErrorContaining(bill.ABL_BillNumberInfo, "Please enter Master Airway Bill (MAWB). This is a required field when Transport Mode = AIR.");

		header.AMA_MasterBill = "AA123456789";
		header.Validation.ValidateAll();
		AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, "Please enter Master Airway Bill (MAWB). This is a required field when Transport Mode = AIR.");
	}

	public void TestPortOfLoadingValidation()
	{
		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();
		bill1.ABL_RL_NKOrigin = "GB123";
		bill2.ABL_RL_NKOrigin = "GB456";

		header.Validation.ValidateAll();
		AssertNoNotifications(header.AMA_RL_NKPortOfLoadingInfo);

		bill2.ABL_RL_NKOrigin = ZString.Empty;
		header.Validation.ValidateAll();
		AssertHasMessageError(header.AMA_RL_NKPortOfLoadingInfo, ASYCUDA.Business.ValidationConstants.ManifestMustGoThruSupportedCountries);

		header.AMA_RL_NKPortOfLoading = "GB123";
		bill2.ABL_RL_NKOrigin = ZString.Empty;
		header.Validation.ValidateAll();
		AssertNoMessageError(header.AMA_RL_NKPortOfLoadingInfo, ASYCUDA.Business.ValidationConstants.ManifestMustGoThruSupportedCountries);

		header.AMA_RL_NKPortOfDischarge = "FR123";
		AssertNoMessageErrorContaining(header.AMA_RL_NKPortOfLoadingInfo, "You have not entered");

		header.AMA_RL_NKPortOfLoading = ZString.Empty;
		AssertHasMessageErrorContaining(header.AMA_RL_NKPortOfLoadingInfo, "You have not entered");
	}

	public void TestPortOfDischargeValidation()
	{
		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();
		bill1.ABL_RL_NKFinalDestination = "GB123";
		bill2.ABL_RL_NKFinalDestination = "GB456";

		header.Validation.ValidateAll();
		AssertNoNotifications(header.AMA_RL_NKPortOfDischargeInfo);

		bill2.ABL_RL_NKFinalDestination = ZString.Empty;
		header.Validation.ValidateAll();
		AssertHasMessageError(header.AMA_RL_NKPortOfDischargeInfo, ASYCUDA.Business.ValidationConstants.ManifestMustGoThruSupportedCountries);

		header.AMA_RL_NKPortOfDischarge = "GB123";
		bill2.ABL_RL_NKFinalDestination = ZString.Empty;
		header.Validation.ValidateAll();
		AssertNoMessageError(header.AMA_RL_NKPortOfDischargeInfo, ASYCUDA.Business.ValidationConstants.ManifestMustGoThruSupportedCountries);

		header.AMA_RL_NKPortOfLoading = "FR123";
		AssertNoMessageErrorContaining(header.AMA_RL_NKPortOfDischargeInfo, "You have not entered");

		header.AMA_RL_NKPortOfDischarge = ZString.Empty;
		AssertHasMessageErrorContaining(header.AMA_RL_NKPortOfDischargeInfo, "You have not entered");
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeaderSS>();
		header.AMA_ManifestType = "S&S";

		var packageTypeCodes = new List<string> { "1A", "PX", "ZZ" };

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes,
			"Package types for test");
		packageTypeCodes.ForEach(li => helper.CreateCusCodeList(
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
}
