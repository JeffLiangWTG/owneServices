using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ExportJobDeclarationValidation))]
sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ExportJobDeclarationValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Export;

	protected override ExportJobDeclarationValidation GetValidation() => new ExportJobDeclarationValidation(jobDeclaration);

	public void TestCustomsOfficeValueNotInListValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_CustomsOffice = "ABC";
		var expectedMessageError = ListValidation.InvalidCodeMessageError;
		AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, expectedMessageError);

		declaration.JE_CustomsOffice = CustomsOfficesList.Codes.DouaneSchipholAirport;
		AssertNoErrorContaining(declaration.JE_CustomsOfficeInfo, expectedMessageError);
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
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "NL", "BILISHI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RN_NKTransportNationalityInlandInfo, "ZZ", Core.Constants.CountryCodes.Netherlands);
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
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		jobDeclaration.JE_CustomsOffice = "NL000123";
		jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
		jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
		entryInstruction.CEI_SubStyle = "V";
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
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		jobDeclaration.JE_CustomsOffice = "NL000123";
		jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
		jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
		entryInstruction.CEI_SubStyle = "V";

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
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		jobDeclaration.JE_CustomsOffice = "NL000123";
		jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
		jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
		entryInstruction.CEI_SubStyle = "V";
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportMeansInfo, "Type of ID");
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
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		jobDeclaration.JE_CustomsOffice = "NL000123";
		jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
		jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
		entryInstruction.CEI_SubStyle = "V";
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

	public void Test_JE_RN_NKTrailer1Nationality_List()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RN_NKTrailer1NationalityInfo, "ZZ", Core.Constants.CountryCodes.Germany);
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

	public void Test_JE_RN_NKTrailer2Nationality_List()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_RN_NKTrailer2NationalityInfo, "ZZ", Core.Constants.CountryCodes.Germany);
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

	public void TestCheckJE_TransportIDInlandForUC9009()
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_CustomsOffice = "NL000123";
			jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
			jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
			entryInstruction.CEI_SubStyle = "V";
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertHasMessageErrorContaining("Transport means error with procedure 76, Customs office not equal to EXT office and atleast a SubStyle not in D, E or F", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "77";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertHasMessageErrorContaining("Transport means error with procedure 77, Customs office not equal to EXT office and atleast a SubStyle not in D, E or F", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76";
			jobDeclaration.JE_TransportMode = TransportModes.Mail;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 76", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "77";
			jobDeclaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 77", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");
		});
	}

	public void TestCheckJE_TransportIDInlandForUC9008()
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76EEEE";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_CustomsOffice = "NL000123";
			jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
			jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
			entryInstruction.CEI_SubStyle = "V";
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertHasMessageErrorContaining("Transport means error with procedure 76EEEE, Customs office not equal to EXT office and atleast a SubStyle not D, E or F", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "10";
			jobDeclaration.JE_TransportMode = TransportModes.Mail;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 10 and transport mode mail", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "23";
			jobDeclaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 23 and transport mode fixedtransportinstallations", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "31";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 31 and transport mode rail", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "11";
			jobDeclaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			jobDeclaration.JE_TransportIDInland = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportIDInland();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 11 and transport mode fixedtransportinstallations", jobDeclaration.JE_TransportIDInlandInfo, "Transport ID details required");
		});
	}

	public void TestCheckJE_TransportMeansForUC9009()
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.JE_CustomsOffice = "NL000123";
			jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
			jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
			entryInstruction.CEI_SubStyle = "D";
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means error with procedure 76, Customs Office not equal to EXT office but no SubStyle is other than D, E or F.", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_SubStyle = "V";
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertHasMessageErrorContaining("Transport means error with procedure 76, Customs Office not equal to EXT office and SubStyle is V.", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.CustomsOffices[0].CY_Data = "NL000123";
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Customs Office equal to EXT Office", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.CustomsOffices[0].CY_Data = "NL000125";
			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "77";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertHasMessageErrorContaining("Transport means error with procedure 77", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76";
			jobDeclaration.JE_TransportMode = TransportModes.Mail;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 76", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "77";
			jobDeclaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 77", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_TransportMeansForUC9008()
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "76EEEE";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_CustomsOffice = "NL000123";
			jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
			jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
			entryInstruction.CEI_SubStyle = "D";
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means error with procedure 76EEEE, Customs Office not equal to EXT office but no SubStyle is other than D, E or F.", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_SubStyle = "V";
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertHasMessageErrorContaining("Transport means error with procedure 76EEEE, Customs Office not equal to EXT office and SubStyle is V.", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.CustomsOffices[0].CY_Data = "NL000123";
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Customs Office equal to EXT Office", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "10";
			jobDeclaration.JE_TransportMode = TransportModes.Mail;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 10 and transport mode mail", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "23";
			jobDeclaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 23 and transport mode fixedtransportinstallations", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "31";
			jobDeclaration.JE_TransportMode = TransportModes.Rail;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 31 and transport mode rail", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.FilteredInvoiceLines[0].JI_FormattedProcedure = "11";
			jobDeclaration.JE_TransportMode = TransportModes.FixedTransportInstallations;
			jobDeclaration.JE_TransportMeans = string.Empty;
			jobDeclaration.Validation.ValidateJE_TransportMeans();
			AssertNoMessageErrorContaining("Transport means no validation with procedure 11 and transport mode fixedtransportinstallations", jobDeclaration.JE_TransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	readonly List<string> transportModesC9008 = new string[] { ModeOfTransportCodeList.Codes._IWT, ModeOfTransportCodeList.Codes._OWN, ModeOfTransportCodeList.Codes._ROA, ModeOfTransportCodeList.Codes._SEA }.ToList();
	readonly List<string> proceduresC9008 = new string[] { "1050", "1120", "2340", "3180" }.ToList();

	public void TestCheckJE_TransportMeansForUC9015()
	{
		var messageSent = "[UC9015] Inland MOT/Code sent in original 515 message.";
		var transportMeansInfo = jobDeclaration.JE_TransportMeansInfo;
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		CombineAssertions(() =>
		{
			jobDeclaration.JE_TransportMeans = "35";
			AssertNoMessageErrorContaining("TransportMeans filled, no mrn", transportMeansInfo, messageSent);

			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
			mrnEntryNumber.CE_EntryNum = "NL123";
			AssertNoMessageErrorContaining("TransportMeans filled, mrn, no message", transportMeansInfo, messageSent);

			var message = entryHeader.Messages.AddNew();
			message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
			message.EM_MessageSubType = ExportSendMessageTypes.Codes.DEC;
			message.EM_MessageNum = "1";
			message.EM_ReceiveTransmit = Messaging.Business.EDIInterchange.Direction.Receive;
			message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
			message.EM_LinkedObject = entryHeader;

			message.EM_MessageText = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MetaData xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:wco:datamodel:WCO:DMS.Declaration:1\"><WCOTypeCode>CC515C</WCOTypeCode><Declaration><GoodsShipment><SequenceNumeric>1</SequenceNumeric><Consignment><DepartureTransportMeans><IdentificationTypeCode>30</IdentificationTypeCode></DepartureTransportMeans></Consignment></GoodsShipment></Declaration></MetaData>";
			jobDeclaration.JE_TransportMeans = "30";
			AssertNoMessageErrorContaining("TransportMeans filled, and in original message", transportMeansInfo, messageSent);

			jobDeclaration.JE_TransportMeans = "";
			AssertHasMessageErrorContaining("TransportMeans not filled, and in original message", transportMeansInfo, messageSent);
		});
	}

	public void TestCheckJE_VesselName()
	{
		var expectedError = "[C9008] Vessel is required.";
		var invoice = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			foreach (var procedure in proceduresC9008)
			{
				invoiceLine.JI_Procedure = procedure;
				foreach (var transportMode in new ModeOfTransportCodeList().GetAllCodes())
				{
					jobDeclaration.JE_TransportMode = transportMode;
					jobDeclaration.JE_VesselName = string.Empty;
					jobDeclaration.Validation.ValidateJE_VesselName();
					if (transportModesC9008.Contains(transportMode))
					{
						AssertHasMessageError($"Border MOT is '{transportMode}', Procedure is '{procedure}' and Transport ID is empty", jobDeclaration.JE_VesselNameInfo, expectedError);
						jobDeclaration.JE_VesselName = "TransportID";
						jobDeclaration.Validation.ValidateJE_VesselName();
						AssertNoMessageError($"Border MOT is '{transportMode}', Procedure is '{procedure}' and Transport ID is filled", jobDeclaration.JE_VesselNameInfo, expectedError);
					}
					else
					{
						AssertNoMessageError($"Border MOT ({transportMode}) should not trigger message", jobDeclaration.JE_VesselNameInfo, expectedError);
					}
				}
			}

			invoiceLine.JI_Procedure = "4280";
			foreach (var transportMode in new ModeOfTransportCodeList().GetAllCodes())
			{
				jobDeclaration.JE_TransportMode = transportMode;
				jobDeclaration.JE_VesselName = string.Empty;
				jobDeclaration.Validation.ValidateJE_VesselName();
				AssertNoMessageError($"Procedure on invoiceline does not start with 10, 11, 23 or 31, no message should be shown", jobDeclaration.JE_VesselNameInfo, expectedError);
			}

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.JE_TransportMode = ModeOfTransportCodeList.Codes._ROA;
			invoiceLine.JI_Procedure = "1000";
			AssertNoMessageError("On Import-declarations, no message should be shown.", jobDeclaration.JE_VesselNameInfo, expectedError);
		});
	}

	public void TestCheckJE_VoyageFlightNo()
	{
		var expectedError = "[C9008] Flight is required.";
		var invoice = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			foreach (var procedure in proceduresC9008)
			{
				invoiceLine.JI_Procedure = procedure;
				foreach (var transportMode in new ModeOfTransportCodeList().GetAllCodes())
				{
					jobDeclaration.JE_TransportMode = transportMode;
					jobDeclaration.JE_VoyageFlightNo = string.Empty;
					jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
					if (transportMode == ModeOfTransportCodeList.Codes._AIR)
					{
						AssertHasMessageError($"Border MOT is '{transportMode}', Procedure is '{procedure}' and Transport ID is empty", jobDeclaration.JE_VoyageFlightNoInfo, expectedError);
						jobDeclaration.JE_VoyageFlightNo = "92352";
						jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
						AssertNoMessageError($"Border MOT is '{transportMode}', Procedure is '{procedure}' and Transport ID is filled", jobDeclaration.JE_VoyageFlightNoInfo, expectedError);
					}
					else
					{
						AssertNoMessageError($"Border MOT ({transportMode}) should not trigger message", jobDeclaration.JE_VoyageFlightNoInfo, expectedError);
					}
				}
			}

			invoiceLine.JI_Procedure = "4280";
			foreach (var transportMode in new ModeOfTransportCodeList().GetAllCodes())
			{
				jobDeclaration.JE_TransportMode = transportMode;
				jobDeclaration.JE_VoyageFlightNo = string.Empty;
				jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
				AssertNoMessageError($"Procedure on invoiceline does not start with 10, 11, 23 or 31, no message should be shown", jobDeclaration.JE_VoyageFlightNoInfo, expectedError);
			}

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.JE_TransportMode = ModeOfTransportCodeList.Codes._ROA;
			invoiceLine.JI_Procedure = "1000";
			AssertNoMessageError("On Import-declarations, no message should be shown.", jobDeclaration.JE_VoyageFlightNoInfo, expectedError);
		});
	}

	public void TestValidateSupplierDocumentaryAddress_Mandatory()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			jobDeclaration.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Supplier not entered", jobDeclaration.SupplierDocumentaryAddress.E2_OA_AddressInfo, "Supplier is required");
			var supplier = Factory.New<OrgHeader>();
			jobDeclaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.Addresses.AddNew().PK;
			jobDeclaration.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining("Supplier entered", jobDeclaration.SupplierDocumentaryAddress.E2_OA_AddressInfo, "Supplier is required");
		});
	}

	public void TestValidateSupplierDocumentaryAddress_DoesMasterDataMatchDeclarantType()
	{
		Assert_DoesMasterDataMatchDeclarantType(jobDeclaration.SupplierDocumentaryAddress.E2_OA_AddressInfo);
	}

	public void TestCheckJE_DeclarantType_DoesMasterDataMatchDeclarantType()
	{
		Assert_DoesMasterDataMatchDeclarantType(jobDeclaration.JE_DeclarantTypeInfo);
	}

	public void TestCheckJE_DeclarantType__SelfRepresentation()
	{
		CombineAssertions(() =>
		{
			var supplier = Factory.New<OrgHeader>();
			jobDeclaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.Addresses.AddNew().PK;
			jobDeclaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
			jobDeclaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageErrorContaining("Supplier is different from login company", jobDeclaration.JE_DeclarantTypeInfo, "For representation type SEL the supplier must be equal to the login company");
			jobDeclaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			jobDeclaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageErrorContaining("Supplier is the same as login company", jobDeclaration.JE_DeclarantTypeInfo, "For representation type SEL the supplier must be equal to the login company");
		});
	}

	void Assert_DoesMasterDataMatchDeclarantType(ZPropertyInfo propertyInfo)
	{
		CombineAssertions(() =>
		{
			var supplier = Factory.New<OrgHeader>();
			jobDeclaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.Addresses.AddNew().PK;

			jobDeclaration.JE_DeclarantType = "DIR";
			jobDeclaration.SupplierDocumentaryAddress.Validation.ValidateAll();
			jobDeclaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageErrorContaining("Declarant type does not match master data for direct representation", propertyInfo, "Representation type doesn't match the agreed Representation type stored in the Customs defaults");

			jobDeclaration.JE_DeclarantType = "IND";
			jobDeclaration.SupplierDocumentaryAddress.Validation.ValidateAll();
			jobDeclaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageErrorContaining("Declarant type matches master data for direct representation", propertyInfo, "Representation type doesn't match the agreed Representation type stored in the Customs defaults");
		});
	}

	public void TestIsTransportInlandFieldsMandatory()
	{
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();

		using (var context = new DeclarationValidationDeciderTestContext(jobDeclaration))
		{
			context.DisableRule(r => r.IsRuleC0843Active);

			jobDeclaration.JE_CustomsOffice = "NL000123";
			jobDeclaration.CustomsOffices[0].CY_Code = "EXT";
			jobDeclaration.CustomsOffices[0].CY_Data = "NL000234";
			entryInstruction.CEI_SubStyle = "V";
			var isTransportInlandFieldsMandatoryProperty = typeof(ExportJobDeclarationValidation).GetProperty("IsTransportInlandFieldsMandatory", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals("Rule C0843 is Deactive", true, isTransportInlandFieldsMandatoryProperty.GetValue(jobDeclaration.Validation));

			context.EnableRule(r => r.IsRuleC0843Active);

			entryInstruction.CEI_SubStyle = "D";
			AssertEquals("Rule C0843 is Active and no SubStyle is other than D, E or F.", false, isTransportInlandFieldsMandatoryProperty.GetValue(jobDeclaration.Validation));

			entryInstruction.CEI_SubStyle = "V";
			AssertEquals("Rule C0843 is Active, SubStyle is V, and declaration office is not equal to exit office", true, isTransportInlandFieldsMandatoryProperty.GetValue(jobDeclaration.Validation));

			jobDeclaration.CustomsOffices[0].CY_Data = "NL000123";
			AssertEquals("Rule C0843 is Active, TransportModeInland is empty, SubStyle is V, but declaration office is equal to exit office", false, isTransportInlandFieldsMandatoryProperty.GetValue(jobDeclaration.Validation));
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
	}
}
