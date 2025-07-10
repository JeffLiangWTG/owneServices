using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber_TransitionPeriod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var decDoc = declaration.AdditionalInfos.AddNew();
			decDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var invoice = declaration.Invoices.AddNew();
			var invDoc = invoice.AdditionalInfos.AddNew();
			invDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var invLine = invoice.InvoiceLines.AddNew();
			var lineDoc = invLine.AdditionalInfos.AddNew();
			lineDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var instructionDoc = instruction.AdditionalInfos.AddNew();
			instructionDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			string message = "Reference Number of Additional Reference can have up to 35 alpha numeric characters.";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 36);
				CombineAssertions("TransitionPeriodAES30, Export, 36 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertHasMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertHasMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertHasMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});

				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 35);
				CombineAssertions("35 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 36);
				CombineAssertions("TransitionPeriodAES30, Import, 36 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				instructionDoc.CSI_ReferenceNumber = lineDoc.CSI_ReferenceNumber = invDoc.CSI_ReferenceNumber = decDoc.CSI_ReferenceNumber = new ZString('A', 37);
				CombineAssertions("Not TransitionPeriodAES30, Export, 37 characters", () =>
				{
					AssertNoMessageError("decDoc", decDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("invDoc", invDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("lineDoc", lineDoc.CSI_ReferenceNumberInfo, message);
					AssertNoMessageError("instructionDoc", instructionDoc.CSI_ReferenceNumberInfo, message);
				});
			}
		}

		public void TestCheckCSI_ReferenceNumber_RoroAccompanied()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var roroRoroUnaccompaniedTrailer = invoiceHeader.AdditionalInfos.AddNew();
			roroRoroUnaccompaniedTrailer.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			roroRoroUnaccompaniedTrailer.CSI_Code = Constants.AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber;
			roroRoroUnaccompaniedTrailer.CSI_ReferenceNumber = "[]";
			var roRoUnaccompaniedTrailerRegistrationFormat = "Format for registration number must be alphanumeric with minimum 4 characters and max 32 characters.";
			AssertHasMessageError("Incorrect format", roroRoroUnaccompaniedTrailer.CSI_ReferenceNumberInfo, roRoUnaccompaniedTrailerRegistrationFormat);

			roroRoroUnaccompaniedTrailer.CSI_ReferenceNumber = new string('A', 33);
			AssertHasMessageError("Incorrect format", roroRoroUnaccompaniedTrailer.CSI_ReferenceNumberInfo, roRoUnaccompaniedTrailerRegistrationFormat);
			roroRoroUnaccompaniedTrailer.CSI_ReferenceNumber = "A";
			AssertHasMessageError("Incorrect format", roroRoroUnaccompaniedTrailer.CSI_ReferenceNumberInfo, roRoUnaccompaniedTrailerRegistrationFormat);

			roroRoroUnaccompaniedTrailer.CSI_ReferenceNumber = new string('A', 4);
			AssertNoMessageError("Incorrect format", roroRoroUnaccompaniedTrailer.CSI_ReferenceNumberInfo, roRoUnaccompaniedTrailerRegistrationFormat);
			roroRoroUnaccompaniedTrailer.CSI_ReferenceNumber = new string('A', 32);
			AssertNoMessageError("Incorrect format", roroRoroUnaccompaniedTrailer.CSI_ReferenceNumberInfo, roRoUnaccompaniedTrailerRegistrationFormat);
		}

		public void TestCheckCSI_ReferenceNumber_RoroShipId()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var roroShipId = invoiceHeader.AdditionalInfos.AddNew();
			roroShipId.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			roroShipId.CSI_Code = Constants.AdditionalReferenceCodes.RoRoShipID;
			roroShipId.CSI_ReferenceNumber = "[]";
			AssertHasNotifications("Incorrect format", roroShipId.CSI_ReferenceNumberInfo);

			roroShipId.CSI_ReferenceNumber = "1234567";
			AssertNoNotifications("Incorrect format", roroShipId.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_CodeCheckRoro()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, "EU Additional Reference");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference,
				code: Constants.AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber,
				description: "RoRo Un accompanied Trailer Registration Number",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference,
				code: Constants.AdditionalReferenceCodes.RoRoShipID,
				description: "RoRo Ship ID",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var roroShipId = invoiceHeader.AdditionalInfos.AddNew();
			roroShipId.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			roroShipId.CSI_Code = Constants.AdditionalReferenceCodes.RoRoShipID;

			var roRoShipIDMessage = "1D94 for Ro-Ro accompanied can only be used when mode of transport at the border is Road.";
			AssertHasMessageError("1D94 not usable when Job not ROA.", roroShipId.CSI_CodeInfo, roRoShipIDMessage);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			roroShipId.Validation.ValidateCSI_Code();
			AssertNoMessageError("1D94 usable when Job is ROA.", roroShipId.CSI_CodeInfo, roRoShipIDMessage);

			var roroUnaccompanied = invoiceHeader.AdditionalInfos.AddNew();
			roroUnaccompanied.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			roroUnaccompanied.CSI_Code = Constants.AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber;
			var roroUnaccompaniedMessage = "1D95 for Ro-Ro unaccompanied trailer can only be used when mode of transport at the border is Sea.";
			AssertHasMessageError("1D95 not usable when Job not SEA.", roroUnaccompanied.CSI_CodeInfo, roroUnaccompaniedMessage);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			roroUnaccompanied.Validation.ValidateCSI_Code();
			AssertNoMessageError("1D94 usable when Job is ROA.", roroUnaccompanied.CSI_CodeInfo, roroUnaccompaniedMessage);

			var roroShipIdOnInstruction = instruction.AdditionalInfos.AddNew();
			roroShipIdOnInstruction.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			roroShipIdOnInstruction.CSI_Code = Constants.AdditionalReferenceCodes.RoRoShipID;
			AssertHasMessageError("1D94 check for AdditionalReference on Instruction.", roroShipIdOnInstruction.CSI_CodeInfo, roRoShipIDMessage);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var roroShipIdOnInvoiceLine = invoiceLine.AdditionalInfos.AddNew();
			roroShipIdOnInvoiceLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			roroShipIdOnInvoiceLine.CSI_Code = Constants.AdditionalReferenceCodes.RoRoShipID;
			AssertHasMessageError("1D94 check for AdditionalReference on InvoiceLine.", roroShipIdOnInvoiceLine.CSI_CodeInfo, roRoShipIDMessage);
		}
	}
}
