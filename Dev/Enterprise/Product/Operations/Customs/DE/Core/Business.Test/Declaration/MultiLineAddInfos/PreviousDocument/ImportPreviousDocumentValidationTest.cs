using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class ImportPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_LineNo_ValidateReferenceAndLineNoCombinationIfNeeded()
		{
			const string expectedMessage = "The combination of Reference and Line No. has already been entered.";
			CombineAssertions(() =>
			{
				instruction.PreviousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				var previousDocument1 = instruction.PreviousDocuments.AddNew();
				previousDocument1.CSI_ReferenceNumber = "VWG";
				previousDocument1.CSI_LineNo = 2;

				var previousDocument2 = instruction.PreviousDocuments.AddNew();
				previousDocument2.CSI_ReferenceNumber = "VWG";
				previousDocument2.CSI_LineNo = 2;
				AssertHasMessageError("Duplicate Reference/Line No combination", previousDocument2.CSI_LineNoInfo, expectedMessage);

				previousDocument2.CSI_LineNo = 3;
				AssertNoMessageError("No Duplicate Reference/Line No combination", previousDocument2.CSI_LineNoInfo, expectedMessage);
			});
		}

		public void TestCheckCSI_LineNo_ATAV_ATZL()
		{
			CombineAssertions(() =>
			{
				foreach (var procedure in new[] { PreviousProcedureList.Codes._ATAV, PreviousProcedureList.Codes._ATZL })
				{
					previousDocument.CSI_Procedure = procedure;
					previousDocument.CSI_LineNo = -1;
					AssertHasMessageErrorContaining(procedure + " Negative", previousDocument.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
					previousDocument.CSI_LineNo = 0;
					AssertNoMessageErrorContaining(procedure + " Not Negative", previousDocument.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
					AssertHasMessageErrorContaining(procedure + " Zero", previousDocument.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);
					previousDocument.CSI_LineNo = 1;
					AssertNoMessageErrorContaining(procedure + " Positive", previousDocument.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);
				}
			});
		}

		public void TestCheckCSI_LineNo_ATNEU()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				AssertRange(previousDocument.CSI_LineNoInfo, new ZInt(1), new ZInt(9999), "Line No. should be between 1 and 9999.");

				previousDocument.CSI_SubType = OwnerReferenceTypeList.Codes.AWB;
				previousDocument.CSI_LineNo = 1;
				AssertHasMessageError("Not empty", previousDocument.CSI_LineNoInfo, "Line No. should be empty.");

				previousDocument.CSI_LineNo = ZInt.Zero;
				AssertNoMessageError("Empty", previousDocument.CSI_LineNoInfo, "Line No. should be empty.");
			});
		}

		public void TestCSI_SubTypeCore()
		{
			const string messageError = "You can't enter different types for Previous Procedure ATNEU";
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				var previousDocument1 = instruction.PreviousDocuments.AddNew();
				var previousDocument2 = instruction.PreviousDocuments.AddNew();

				AssertNoMessageError("Both Empty 1", previousDocument1.CSI_SubTypeInfo, messageError);
				AssertNoMessageError("Both Empty 2", previousDocument1.CSI_SubTypeInfo, messageError);

				previousDocument1.CSI_SubType = OwnerReferenceTypeList.Codes.AWB;
				previousDocument2.CSI_SubType = OwnerReferenceTypeList.Codes.REG;
				AssertHasMessageError("Different CSI_SubType", previousDocument2.CSI_SubTypeInfo, messageError);

				previousDocument2.CSI_SubType = OwnerReferenceTypeList.Codes.AWB;
				AssertNoMessageError("Same CSI_SubType", previousDocument2.CSI_SubTypeInfo, messageError);

				previousDocument2.CSI_SubType = ZString.Empty;
				AssertHasMessageError("Two is empty", previousDocument2.CSI_SubTypeInfo, messageError);
				AssertNoMessageError("One is entered", previousDocument1.CSI_SubTypeInfo, messageError);
			});
		}

		public void TestCheckCSI_SubType_ATNEU_List()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			var subTypeInfo = previousDocument.CSI_SubTypeInfo;

			CombineAssertions(() =>
			{
				previousDocument.CSI_SubType = "AAA";
				AssertHasMessageError("Invalid CSI_SubType", subTypeInfo, ListValidation.InvalidCodeMessageError);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertNoMessageError("Valid CSI_SubType", subTypeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCSI_ReferenceNumber_Mandatory()
		{
			previousDocument.Status = false;
			CombineAssertions(() =>
			{
				foreach (var procedureType in new[] { PreviousProcedureList.Codes._ATNEU, PreviousProcedureList.Codes._ATZL, PreviousProcedureList.Codes._ATAV })
				{
					previousDocument.CSI_Procedure = procedureType;
					previousDocument.CSI_ReferenceNumber = ZString.Empty;
					AssertHasMessageErrorContaining($"{procedureType}, Empty", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
					previousDocument.CSI_ReferenceNumber = "ABC";
					AssertNoMessageErrorContaining($"{procedureType}, Not Empty", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public void TestCheckCSI_ReferenceNumber_Format()
		{
			const string invalidReferenceError = "When Previous Procedure is 'T1' or 'T2' or 'ESUMA',";

			CombineAssertions(() =>
			{
				foreach (var procedureType in new[] { PreviousProcedureList.Codes._T1, PreviousProcedureList.Codes._T2, PreviousProcedureList.Codes._ESUMA })
				{
					previousDocument.CSI_Procedure = procedureType;
					previousDocument.CSI_ReferenceNumber = "INVALIDCODE";
					AssertHasMessageErrorContaining($"{previousDocument}, Invalid Reference", previousDocument.CSI_ReferenceNumberInfo, invalidReferenceError);
					previousDocument.CSI_ReferenceNumber = "11DE11111111111115";
					AssertNoMessageErrorContaining($"{previousDocument}, Valid Reference", previousDocument.CSI_ReferenceNumberInfo, invalidReferenceError);
				}
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATNEU_Format()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
			var referenceNumberInfo = previousDocument.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("ATNEU, SubType AWB", referenceNumberInfo);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("ATNEU, SubType ULD", referenceNumberInfo);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("ATNEU, SubType REG", referenceNumberInfo);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATAV_Format()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var referenceNumberInfo = previousDocument.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				previousDocument.Status = ZBool.False;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("ATAV, Status False", referenceNumberInfo);

				previousDocument.Status = ZBool.True;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("ATAV, Status True", referenceNumberInfo);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATAV_ShouldNotHaveAtlasStructure()
		{
			const string message = "The entered Reference Number has an ATLAS structure. Please tick the 'Entry via ATLAS' Flag or change the Reference Number.";
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			previousDocument.Status = false;
			previousDocument.CSI_ReferenceNumber = "ATC020123456789012345";
			CombineAssertions(() =>
			{
				AssertHasMessageError("CSI_Status 'N'", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.Status = true;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("CSI_Status 'Y'", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "24DE5875GCH0002ER0";
				AssertNoMessageError("CSI_Status 'Y' MRN", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.Status = false;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageError("CSI_Status 'N' MRN", previousDocument.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATAV_ShouldHaveAtlasStructure()
		{
			const string message = "Structure does not correspond to an ATLAS Registration Reference for Inward Processing.";
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			previousDocument.Status = true;
			CombineAssertions(() =>
			{
				previousDocument.CSI_ReferenceNumber = ZString.Empty.PadLeft(21, 'A');
				AssertHasMessageError("Invalid reference", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "ATC020123456789012345";
				AssertNoMessageError("Valid reference", previousDocument.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATZL_Format()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var referenceNumberInfo = previousDocument.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				previousDocument.Status = ZBool.False;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("ATZL, Status False", referenceNumberInfo);

				previousDocument.Status = ZBool.True;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("ATZL, Status True", referenceNumberInfo);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATZL_ShouldNotHaveAtlasStructure()
		{
			const string message = "The entered Reference Number has an ATLAS structure. Please tick the 'Entry via ATLAS' Flag or change the Reference Number.";
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument.Status = false;
			previousDocument.CSI_ReferenceNumber = "ATC710123456789012345";
			CombineAssertions(() =>
			{
				AssertHasMessageError("CSI_Status 'N'", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.Status = true;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("CSI_Status 'Y'", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "24DE5875GCM0002XR7";
				AssertNoMessageError("CSI_Status 'Y' MRN", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.Status = false;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageError("CSI_Status 'N' MRN", previousDocument.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATZL_ShouldHaveAtlasStructure()
		{
			const string message = "Structure does not correspond to an ATLAS Registration Reference for Bonded Warehouse.";
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument.Status = true;
			CombineAssertions(() =>
			{
				previousDocument.CSI_ReferenceNumber = ZString.Empty.PadLeft(21, 'A');
				AssertHasMessageError("Invalid reference", previousDocument.CSI_ReferenceNumberInfo, message);

				previousDocument.CSI_ReferenceNumber = "ATC710123456789012345";
				AssertNoMessageError("Valid reference", previousDocument.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_Quantity_ATNEU()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_QuantityInfo);
				AssertRange(previousDocument.CSI_QuantityInfo, new ZDecimal(1), new ZDecimal(99999), "Package Qty. should be between 1 and 99999.");
			});
		}

		public void TestCheckCSI_Quantity_ATZL()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument.UsualProcessingFlag = true;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_QuantityInfo);
			AssertRange(previousDocument.CSI_QuantityInfo, new ZDecimal(0.001), new ZDecimal(999999999.999), "Commercial Qty. should be between 0.001 and 999999999.999.");
		}

		public void TestCheckCSI_Quantity_IntegerUQ()
		{
			const string integerValuesRequired = "Only integer values are allowed for this Commercial Qty. Unit.";
			CombineAssertions(() =>
			{
				foreach (var unitOfQuantity in PreviousDocumentHelperTest.UnitOfQuantitiesThatRequireIntegerValues)
				{
					previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
					previousDocument.CSI_UnitOfQuantity = unitOfQuantity;
					previousDocument.CSI_Quantity = 3.69m;
					AssertNoMessageError($"Procedure: ATAV, UQ: {unitOfQuantity}, Quantity: 3.69", previousDocument.CSI_QuantityInfo, integerValuesRequired);

					previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
					previousDocument.Validation.ValidateCSI_Quantity();
					AssertHasMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3.69", previousDocument.CSI_QuantityInfo, integerValuesRequired);

					previousDocument.CSI_Quantity = 3;
					AssertNoMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3", previousDocument.CSI_QuantityInfo, integerValuesRequired);

					previousDocument.CSI_UnitOfQuantity = "KGME";
					previousDocument.CSI_Quantity = 3.69m;
					AssertNoMessageError($"Procedure: ATZL, UQ: KGME, Quantity: 3.69", previousDocument.CSI_QuantityInfo, integerValuesRequired);
				}
			});
		}

		public void TestCheckCSI_ReferenceNumber2_AWB()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				previousDocument.CSI_SubType = OwnerReferenceTypeList.Codes.AWB;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_ReferenceNumber2Info);
			});
		}

		public void TestCheckCSI_ReferenceNumber2_ULD()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
				previousDocument.CSI_SubType = OwnerReferenceTypeList.Codes.ULD;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_ReferenceNumber2Info);
			});
		}

		public void TestCheckCSI_Tariff_Mandatory()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_TariffInfo);

				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				ValidationTestHelper.AssertFieldIsNotMandatory(previousDocument.CSI_TariffInfo);
			});
		}

		public void TestCheckCSI_Tariff_Length()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var tooShortTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "123456789", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffInfo = previousDocument.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousDocument.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousDocument.CSI_Tariff = tooShortTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining("Invalid length", tariffInfo, InvalidTariffMessageError);

				var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				previousDocument.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("valid length", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Tariff_IsNumeric()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var nonNumericTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "1234efghijk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffInfo = previousDocument.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousDocument.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousDocument.CSI_Tariff = nonNumericTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining("not numeric", tariffInfo, InvalidTariffMessageError);

				var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				previousDocument.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("numeric", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Tariff_IsValidCode()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffInfo = previousDocument.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousDocument.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousDocument.CSI_Tariff = "10123456789";
				AssertHasMessageErrorContaining("invalid code", tariffInfo, InvalidTariffMessageError);

				previousDocument.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("valid code", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Tariff_EffectiveDate()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var outOfDateTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.Now.AddDays(-1));

			var tariffInfo = previousDocument.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousDocument.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousDocument.CSI_Tariff = outOfDateTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining("out of date", tariffInfo, InvalidTariffMessageError);

				var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678902", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				previousDocument.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("valid code", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Quantity2_Mandatory()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_Quantity2Info);
		}

		public void TestCheckCSI_Quantity2_Range()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			AssertRange(previousDocument.CSI_Quantity2Info, new ZDecimal(0.001), new ZDecimal(999999999.99), "Debit Qty. should be between 0.001 and 999999999.99.");
		}

		public void TestCheckCSI_UnitOfQuantity2_Mandatory()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument.CSI_Quantity2 = 1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_UnitOfQuantity2Info);
		}

		public void TestCheckCSI_UnitOfQuantity2_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "NAR", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument.CSI_Quantity2 = 1;
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_UnitOfQuantity2Info, "X", "NAR");
		}

		public void TestCheckCSI_Quantity2_IntegerUQ()
		{
			const string integerValuesRequired = "Only integer values are allowed for this Debit Qty. Unit.";
			CombineAssertions(() =>
			{
				foreach (var unitOfQuantity in PreviousDocumentHelperTest.UnitOfQuantitiesThatRequireIntegerValues)
				{
					previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
					previousDocument.CSI_UnitOfQuantity2 = unitOfQuantity;
					previousDocument.CSI_Quantity2 = 3.69m;
					AssertNoMessageError($"Procedure: ATAV, UQ: {unitOfQuantity}, Quantity: 3.69", previousDocument.CSI_Quantity2Info, integerValuesRequired);

					previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
					previousDocument.Validation.ValidateCSI_Quantity2();
					AssertHasMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3.69", previousDocument.CSI_Quantity2Info, integerValuesRequired);

					previousDocument.CSI_Quantity2 = 3;
					AssertNoMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3", previousDocument.CSI_Quantity2Info, integerValuesRequired);

					previousDocument.CSI_UnitOfQuantity2 = "KGME";
					previousDocument.CSI_Quantity2 = 3.69m;
					AssertNoMessageError($"Procedure: ATZL, UQ: KGME, Quantity: 3.69", previousDocument.CSI_Quantity2Info, integerValuesRequired);
				}
			});
		}

		public void TestCheckCSI_UnitOfQuantity_Mandatory()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument.CSI_Quantity = 1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_UnitOfQuantityInfo);
		}

		public void TestCheckCSI_UnitOfQuantity_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "NAR", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousDocument.CSI_Quantity = 1;
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_UnitOfQuantityInfo, "X", "NAR");
		}

		public void TestCheckCSI_Description()
		{
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_DescriptionInfo);
		}

		public void TestCheckCSI_ItemNumberString_CannotBeZero()
		{
			const string message = "Invoice Line No. cannot be zero.";
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateAll(); // Test that ValidateAll validates CSI_ItemNumberString
				AssertHasMessageError("CSI_ItemNumberString not set", previousDocument.CSI_ItemNumberStringInfo, message);

				previousDocument.CSI_ItemNumberString = "0";
				AssertHasMessageError("CSI_ItemNumberString set to '0'", previousDocument.CSI_ItemNumberStringInfo, message);

				previousDocument.CSI_ItemNumberString = "abc";
				AssertHasMessageError("CSI_ItemNumberString set to 'abc'", previousDocument.CSI_ItemNumberStringInfo, message);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
				previousDocument.Validation.ValidateCSI_ItemNumberString();
				AssertNoMessageError("CEI_Style not 'LUZ'", previousDocument.CSI_ItemNumberStringInfo, message);
			});
		}

		public void TestCheckCSI_ItemNumberString_ListValidation()
		{
			var anotherInstructionPK = declaration.CustomsEntryInstructions.AddNew().PK;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_CEI = anotherInstructionPK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CEI = anotherInstructionPK;
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 2;
			invoiceLine3.JI_CEI = instruction.PK;

			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_ItemNumberStringInfo, "1", "2");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			previousDocument = instruction.PreviousDocuments.AddNew();
		}
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		PreviousDocument previousDocument;

		const string InvalidTariffMessageError = "The entered Commodity Code is not valid.";

		void AssertRange(ZPropertyInfo propertyInfo, IZType minValue, IZType maxValue, ZString expectedErrorMessage)
		{
			propertyInfo.Value = MinMinusOne();
			AssertHasMessageError("Less than min", propertyInfo, expectedErrorMessage);
			propertyInfo.Value = minValue;
			AssertNoMessageError("Min value", propertyInfo, expectedErrorMessage);
			propertyInfo.Value = MaxPlusOne();
			AssertHasMessageError("Greater than Max", propertyInfo, expectedErrorMessage);
			propertyInfo.Value = maxValue;
			AssertNoMessageError("Max value", propertyInfo, expectedErrorMessage);

			IZType MinMinusOne()
			{
				if (minValue is ZShort shortValue)
				{
					return shortValue - 1;
				}
				else if (minValue is ZDecimal decimalValue)
				{
					return (ZDecimal)(decimalValue - 0.01m);
				}
				return ZInt.Zero;
			}

			IZType MaxPlusOne()
			{
				if (maxValue is ZShort shortValue)
				{
					return shortValue + 1;
				}
				else if (maxValue is ZDecimal decimalValue)
				{
					return (ZDecimal)(decimalValue + 0.01m);
				}
				return ZInt.Zero;
			}
		}
	}
}
