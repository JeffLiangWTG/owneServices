using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ExportJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateJE_CustomsOffice()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000001", "BE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Role, UniversalReferenceConstants.Export);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
		var noBLTCodeList = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "noBLT", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Type, UniversalReferenceConstants.DALocatie);
		helper.CreateCusCodeListAttribute(noBLTCodeList.PK, UniversalReferenceConstants.SubType, UniversalReferenceConstants.Kantoor);
		Factory.Save();

		CombineAssertions(() =>
		{
			const string validSelection = "Enter a valid selection.";
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			jobDeclaration.JE_CustomsOffice = "BE000001";
			AssertNoErrorContaining("BLT: BE000001", jobDeclaration.JE_CustomsOfficeInfo, validSelection);
			jobDeclaration.JE_CustomsOffice = "xxx";
			AssertHasErrorContaining("BLT: xxx", jobDeclaration.JE_CustomsOfficeInfo, validSelection);
			jobDeclaration.JE_CustomsOffice = "noBLT";
			AssertHasErrorContaining("BLT: noBLT", jobDeclaration.JE_CustomsOfficeInfo, validSelection);

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			jobDeclaration.JE_CustomsOffice = "BE000001";
			AssertHasMessageErrorContaining("not BLT: BE000001", jobDeclaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError.ToString());
			jobDeclaration.JE_CustomsOffice = "xxx";
			AssertHasMessageErrorContaining("not BLT: xxx", jobDeclaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError.ToString());
			jobDeclaration.JE_CustomsOffice = "noBLT";
			AssertNoMessageErrorContaining("not BLT: noBLT", jobDeclaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});
	}

	public void Test_JE_AircraftRegistrationInland_Air_Mandatory()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
		jobDeclaration.JE_TransportIDInland = "12345567ABC";
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_AircraftRegistrationInlandInfo, "Aircraft ID");
	}

	public void Test_JE_RN_NKTransportNationalityInland_Mandatory()
	{
		CombineAssertions(() =>
		{
			foreach (var transportMode in new ZString[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail })
			{
				var transportModeDescription = $"Transport Mode: {transportMode}";
				jobDeclaration.JE_TransportModeInland = transportMode;
				ValidationTestHelper.AssertFieldIsNotMandatory(jobDeclaration.JE_RN_NKTransportNationalityInfo, transportModeDescription);
				jobDeclaration.JE_TransportIDInland = "ABCD1234";
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, "Nationality", transportModeDescription);
			}
		});
	}

	public void Test_JE_RN_NKTransportNationalityInland_List()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "Export Nationality");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "BE", "BILISHI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, "ZZ", Core.Constants.CountryCodes.Belgium);
	}

	public void Test_JE_TransportIDInland_AllowMixedCase()
	{
		CombineAssertions(() =>
		{
			const string expectedMessageError = "No lower case letters may be specified.";
			foreach (var transportMode in new ZString[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.InlandWaterwayTransport })
			{
				jobDeclaration.JE_TransportModeInland = transportMode;
				jobDeclaration.JE_TransportIDInland = "test1234TEST";
				AssertNoMessageError($"Allow lowercase, Transport Mode: {transportMode}", jobDeclaration.JE_TransportIDInlandInfo, expectedMessageError);
			}

			foreach (var transportMode in new ZString[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail })
			{
				jobDeclaration.JE_TransportModeInland = transportMode;
				jobDeclaration.JE_TransportIDInland = "test1234TEST";
				AssertHasMessageError($"Disallow lowercase, Transport Mode: {transportMode}", jobDeclaration.JE_TransportIDInlandInfo, expectedMessageError);
			}
		});
	}

	public void Test_JE_TransportIDInland_Air()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportIDInlandInfo, "Flight Number");
	}

	public void Test_JE_RN_NKTransportNationalityInland_Air()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.JE_AircraftRegistrationInland = "12345566";
			jobDeclaration.JE_RN_NKTransportNationalityInland = string.Empty;
			AssertHasMessageErrorContaining("Not entered", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Germany;
			AssertNoMessageErrorContaining("Entered", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void Test_JE_RN_NKTransportNationalityInland_Air_JE_AircraftRegistrationInland()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.JE_AircraftRegistrationInland = "12345566";
			jobDeclaration.JE_RN_NKTransportNationalityInland = string.Empty;
			AssertHasMessageErrorContaining("Not entered", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_AircraftRegistrationInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationalityInland();
			AssertNoMessageErrorContaining("No Transport ID Inland", jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void Test_JE_TransportIDInland_VesselID_Mandatory()
	{
		CombineAssertions(() =>
		{
			foreach (var transportMode in new ZString[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.InlandWaterwayTransport })
			{
				jobDeclaration.JE_TransportModeInland = transportMode;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportIDInlandInfo, "Vessel ID", $"Transport Mode: {transportMode}");
			}
		});
	}

	public void Test_JE_TransportMeans_OwnPropulsion_Mandatory()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.OwnPropulsion;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportMeansInfo, "Type of ID");
	}

	public void Test_JE_TransportMeans_List()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_TransportMeansInfo, "AA", ExportInlandTransportTypeList.Codes._10);
	}

	public void Test_JE_TransportIDInland_OwnPropulsion_LowerCaseLettersProhibitedInVesselID()
	{
		const string expectedMessageError = "No lower case letters may be specified.";
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.OwnPropulsion;
		var transportTypesThatAllowMixedCase = new HashSet<string>() { ExportInlandTransportTypeList.Codes._11, ExportInlandTransportTypeList.Codes._81 };
		foreach (var inlandTransportType in new ExportInlandTransportTypeList().GetAllCodes())
		{
			jobDeclaration.JE_TransportMeans = inlandTransportType;
			jobDeclaration.JE_TransportIDInland = "test1234TEST";

			if (transportTypesThatAllowMixedCase.Contains(inlandTransportType))
			{
				AssertNoMessageError(jobDeclaration.JE_TransportIDInlandInfo, expectedMessageError);
			}
			else
			{
				AssertHasMessageError(jobDeclaration.JE_TransportIDInlandInfo, expectedMessageError);
			}
		}
	}

	public void Test_JE_TransportIDInland_Rail_TrainNumber()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportIDInlandInfo, "Train or Wagon Number.");
	}

	public void Test_JE_Trailer1RegNo_Rail_WagonNumber()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_Trailer1RegNoInfo, "Train or Wagon Number.");
	}

	public void Test_TransportIDInland_Rail_TrainAndWagonNumber_MutuallyExclusive()
	{
		const string mutuallyExclusiveError = "You may only enter either a Train or a Wagon Number.";

		CombineAssertions(() =>
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
			jobDeclaration.JE_TransportIDInland = "ABCD1234";
			jobDeclaration.JE_Trailer1RegNo = "1234ABCD";

			AssertHasMessageError("Both Entered", jobDeclaration.JE_Trailer1RegNoInfo, mutuallyExclusiveError);

			jobDeclaration.JE_TransportIDInland = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_Trailer1RegNo();
			AssertNoMessageError("Wagon Entered", jobDeclaration.JE_Trailer1RegNoInfo, mutuallyExclusiveError);

			jobDeclaration.JE_TransportIDInland = "ABCD1234";
			jobDeclaration.JE_Trailer1RegNo = ZString.Empty;
			AssertNoMessageError("Train Entered", jobDeclaration.JE_Trailer1RegNoInfo, mutuallyExclusiveError);
		});
	}

	public void Test_JE_RN_NKTrailer1Nationality_Mandatory()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.JE_Trailer1RegNo = "1234ABCD";
			jobDeclaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
			AssertHasMessageErrorContaining("Trailer Number", jobDeclaration.JE_RN_NKTrailer1NationalityInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_Trailer1RegNo = string.Empty;
			jobDeclaration.Validation.ValidateJE_RN_NKTrailer1Nationality();
			AssertNoMessageErrorContaining("No Trailer Number", jobDeclaration.JE_RN_NKTrailer1NationalityInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void Test_JE_RN_NKTrailer2Nationality_Mandatory()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.JE_Trailer2RegNo = "1234ABCD";
			jobDeclaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
			AssertHasMessageErrorContaining("Trailer Number", jobDeclaration.JE_RN_NKTrailer2NationalityInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_Trailer2RegNo = string.Empty;
			jobDeclaration.Validation.ValidateJE_RN_NKTrailer2Nationality();
			AssertNoMessageErrorContaining("No Trailer Number", jobDeclaration.JE_RN_NKTrailer2NationalityInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void Test_JE_TransportIDInland_Sea_VesselID_ListWarning()
	{
		const string expectedWarningMessage = "Warning: No reference file for this Vessel was found.";
		CombineAssertions(() =>
		{
			jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			jobDeclaration.JE_TransportIDInland = "Sea Shepard";
			AssertHasWarning("Inland vessel", jobDeclaration.JE_TransportIDInlandInfo, expectedWarningMessage);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "MY NEW SHIP";
			jobDeclaration.JE_TransportIDInland = vessel.RV_Name;
			AssertNoWarning("RefVessel", jobDeclaration.JE_TransportIDInlandInfo, expectedWarningMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
	}
	JobDeclaration jobDeclaration;
}
