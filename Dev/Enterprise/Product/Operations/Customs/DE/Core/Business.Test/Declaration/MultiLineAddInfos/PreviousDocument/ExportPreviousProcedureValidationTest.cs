using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ExportPreviousProcedureValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			const string mandatoryReference = "You have not entered a Reference.";
			CombineAssertions(() =>
			{
				validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageError("Mandatory", previousProcedure.CSI_ReferenceNumberInfo, mandatoryReference);

				previousProcedure.CSI_ReferenceNumber = "REFNUM";
				AssertNoMessageError("Entered", previousProcedure.CSI_ReferenceNumberInfo, mandatoryReference);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATAV_ShouldHaveAtlasStructure()
		{
			const string message = "Structure does not correspond to an ATLAS Registration Reference for Inward Processing.";
			previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			previousProcedure.Status = true;
			CombineAssertions(() =>
			{
				previousProcedure.CSI_ReferenceNumber = ZString.Empty.PadLeft(21, 'A');
				AssertHasMessageError("Invalid reference", previousProcedure.CSI_ReferenceNumberInfo, message);

				previousProcedure.CSI_ReferenceNumber = "ATC020123456789012345";
				AssertNoMessageError("Valid reference", previousProcedure.CSI_ReferenceNumberInfo, message);

				previousProcedure.Status = false;
				previousProcedure.CSI_ReferenceNumber = ZString.Empty.PadLeft(21, 'A');
				AssertNoMessageError("Not via ATLAS", previousProcedure.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATZL_ShouldHaveAtlasStructure()
		{
			const string message = "Structure does not correspond to an ATLAS Registration Reference for Bonded Warehouse.";
			previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			previousProcedure.Status = true;
			CombineAssertions(() =>
			{
				previousProcedure.CSI_ReferenceNumber = ZString.Empty.PadLeft(21, 'A');
				AssertHasMessageError("Invalid reference", previousProcedure.CSI_ReferenceNumberInfo, message);

				previousProcedure.CSI_ReferenceNumber = "ATC710123456789012345";
				AssertNoMessageError("Valid reference", previousProcedure.CSI_ReferenceNumberInfo, message);

				previousProcedure.Status = false;
				previousProcedure.CSI_ReferenceNumber = ZString.Empty.PadLeft(21, 'A');
				AssertNoMessageError("Not via ATLAS", previousProcedure.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATAV_Format()
		{
			previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			var referenceNumberInfo = previousProcedure.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				previousProcedure.Status = ZBool.False;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("ATAV, Status False", referenceNumberInfo);

				previousProcedure.Status = ZBool.True;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("ATAV, Status True", referenceNumberInfo);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATZL_Format()
		{
			previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			var referenceNumberInfo = previousProcedure.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				previousProcedure.Status = ZBool.False;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("ATZL, Status False", referenceNumberInfo);

				previousProcedure.Status = ZBool.True;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("ATZL, Status True", referenceNumberInfo);
			});
		}

		public void TestCheckCSI_LineNo_ValidateReferenceAndLineNoCombinationIfNeeded()
		{
			var expectedMessage = "The combination of Reference and Line No. has already been entered.";
			CombineAssertions(() =>
			{
				previousProcedure.CSI_ReferenceNumber = "VWG";
				previousProcedure.CSI_LineNo = 2;
				var previousProcedure2 = invoiceLine.PreviousProcedures.AddNew();
				previousProcedure2.CSI_ReferenceNumber = "VWG";
				previousProcedure2.CSI_LineNo = 2;
				AssertHasMessageError("Duplicate Reference/Line No combination", previousProcedure2.CSI_LineNoInfo, expectedMessage);
				previousProcedure2.CSI_LineNo = 3;
				AssertNoMessageError("No Duplicate Reference/Line No combination", previousProcedure2.CSI_LineNoInfo, expectedMessage);
			});
		}

		public void TestCheckCSI_LineNo()
		{
			const string lineNoRange = "Line No. should be between 1 and 99999.";
			CombineAssertions(() =>
			{
				previousProcedure.CSI_LineNo = 0;
				AssertHasMessageError("Zero value", previousProcedure.CSI_LineNoInfo, lineNoRange);

				previousProcedure.CSI_LineNo = 23;
				AssertNoMessageError("Valid", previousProcedure.CSI_LineNoInfo, lineNoRange);

				previousProcedure.CSI_LineNo = 100000;
				AssertHasMessageError("Too large", previousProcedure.CSI_LineNoInfo, lineNoRange);
			});
		}

		public void TestCheckCSI_Tariff_Mandatory()
		{
			CombineAssertions(() =>
			{
				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				validation.ValidateCSI_Tariff();
				AssertNoMessageError("Non Mandatory Procedure", previousProcedure.CSI_TariffInfo, MandatoryCommodityCode);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				validation.ValidateCSI_Tariff();
				AssertHasMessageError("Mandatory Procedure", previousProcedure.CSI_TariffInfo, MandatoryCommodityCode);

				previousProcedure.CSI_Tariff = "12345678912";
				AssertNoMessageError("Entered", previousProcedure.CSI_ReferenceNumber2Info, MandatoryCommodityCode);
			});
		}

		public void TestCheckCSI_Tariff_Length()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var tooShortTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "123456789", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffInfo = previousProcedure.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousProcedure.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousProcedure.CSI_Tariff = tooShortTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining("Invalid length", tariffInfo, InvalidTariffMessageError);

				var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				previousProcedure.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("valid length", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Tariff_IsNumeric()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var nonNumericTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "1234efghijk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffInfo = previousProcedure.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousProcedure.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousProcedure.CSI_Tariff = nonNumericTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining("not numeric", tariffInfo, InvalidTariffMessageError);

				var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				previousProcedure.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("numeric", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Tariff_IsValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariffInfo = previousProcedure.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousProcedure.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousProcedure.CSI_Tariff = "10123456789";
				AssertHasMessageErrorContaining("invalid code", tariffInfo, InvalidTariffMessageError);

				previousProcedure.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("valid code", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Tariff_EffectiveDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeImport = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			var outOfDateTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678901", ZDateTime.MinSmallDateTimeValue, ZDateTime.Now.AddDays(-1));

			var tariffInfo = previousProcedure.CSI_TariffInfo;
			CombineAssertions(() =>
			{
				previousProcedure.CSI_Tariff = ZString.Empty;
				AssertNoMessageErrorContaining("empty Tariff", tariffInfo, InvalidTariffMessageError);

				previousProcedure.CSI_Tariff = outOfDateTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining("out of date", tariffInfo, InvalidTariffMessageError);

				var validTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffTypeImport.PK, "12345678902", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				previousProcedure.CSI_Tariff = validTariff.ZZ1_TariffCode;
				AssertNoMessageErrorContaining("valid code", tariffInfo, InvalidTariffMessageError);
			});
		}

		public void TestCheckCSI_Quantity_Mandatory()
		{
			CombineAssertions(() =>
			{
				previousProcedure.UsualProcessingFlag = true;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousProcedure.CSI_QuantityInfo);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				ValidationTestHelper.AssertFieldIsNotMandatory(previousProcedure.CSI_QuantityInfo);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				previousProcedure.UsualProcessingFlag = false;
				ValidationTestHelper.AssertFieldIsNotMandatory(previousProcedure.CSI_QuantityInfo);
			});
		}

		public void TestCheckCSI_Quantity_IntegerUQ()
		{
			CombineAssertions(() =>
			{
				foreach (var unitOfQuantity in PreviousDocumentHelperTest.UnitOfQuantitiesThatRequireIntegerValues)
				{
					previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
					previousProcedure.CSI_UnitOfQuantity = unitOfQuantity;
					previousProcedure.CSI_Quantity = 3.69m;
					AssertNoMessageError($"Procedure: ATAV, UQ: {unitOfQuantity}, Quantity: 3.69", previousProcedure.CSI_QuantityInfo, IntegerValuesRequired);

					previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
					previousProcedure.Validation.ValidateCSI_Quantity();
					AssertHasMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3.69", previousProcedure.CSI_QuantityInfo, IntegerValuesRequired);

					previousProcedure.CSI_Quantity = 3;
					AssertNoMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3", previousProcedure.CSI_QuantityInfo, IntegerValuesRequired);

					previousProcedure.CSI_UnitOfQuantity = "KGME";
					previousProcedure.CSI_Quantity = 3.69m;
					AssertNoMessageError($"Procedure: ATZL, UQ: KGME, Quantity: 3.69", previousProcedure.CSI_QuantityInfo, IntegerValuesRequired);
				}
			});
		}

		public void TestCSI_UnitOfQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DAP", "Dekatonne Magic", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				previousProcedure.CSI_UnitOfQuantity = "1";
				AssertNoMessageError("Non Validation Procedure", previousProcedure.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				validation.ValidateCSI_UnitOfQuantity();
				AssertHasMessageError("Invalid", previousProcedure.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

				previousProcedure.CSI_UnitOfQuantity = "DAP";
				AssertNoMessageError("Valid", previousProcedure.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCSI_Quantity2_Mandatory()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousProcedure.CSI_Quantity2Info);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				ValidationTestHelper.AssertFieldIsNotMandatory(previousProcedure.CSI_Quantity2Info);
			});
		}

		public void TestCheckCSI_Quantity2_IntegerUQ()
		{
			CombineAssertions(() =>
			{
				foreach (var unitOfQuantity in PreviousDocumentHelperTest.UnitOfQuantitiesThatRequireIntegerValues)
				{
					previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
					previousProcedure.CSI_UnitOfQuantity2 = unitOfQuantity;
					previousProcedure.CSI_Quantity2 = 3.69m;
					AssertNoMessageError($"Procedure: ATAV, UQ: {unitOfQuantity}, Quantity: 3.69", previousProcedure.CSI_Quantity2Info, IntegerValuesRequired);

					previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
					previousProcedure.Validation.ValidateCSI_Quantity2();
					AssertHasMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3.69", previousProcedure.CSI_Quantity2Info, IntegerValuesRequired);

					previousProcedure.CSI_Quantity2 = 3;
					AssertNoMessageError($"Procedure: ATZL, UQ: {unitOfQuantity}, Quantity: 3", previousProcedure.CSI_Quantity2Info, IntegerValuesRequired);

					previousProcedure.CSI_UnitOfQuantity2 = "KGME";
					previousProcedure.CSI_Quantity2 = 3.69m;
					AssertNoMessageError($"Procedure: ATZL, UQ: KGME, Quantity: 3.69", previousProcedure.CSI_Quantity2Info, IntegerValuesRequired);
				}
			});
		}

		public void TestCSI_UnitOfQuantity2_Mandatory()
		{
			const string mandatoryMeasurementUnitDebitAmount = "You have not entered a UQ (Debit Qty.).";
			CombineAssertions(() =>
			{
				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				validation.ValidateCSI_UnitOfQuantity2();
				AssertNoMessageError("Non Validation procedure", previousProcedure.CSI_UnitOfQuantity2Info, mandatoryMeasurementUnitDebitAmount);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				validation.ValidateCSI_UnitOfQuantity2();
				AssertHasMessageError("Mandatory", previousProcedure.CSI_UnitOfQuantity2Info, mandatoryMeasurementUnitDebitAmount);

				previousProcedure.CSI_UnitOfQuantity2 = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems;
				AssertNoMessageError("Valid UQ", previousProcedure.CSI_UnitOfQuantity2Info, mandatoryMeasurementUnitDebitAmount);
			});
		}

		public void TestCSI_UnitOfQuantity2_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DHS", "Kilogram Magic", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				previousProcedure.CSI_UnitOfQuantity2 = "1";
				AssertHasMessageError("Invalid", previousProcedure.CSI_UnitOfQuantity2Info, ListValidation.InvalidCodeMessageError);

				previousProcedure.CSI_UnitOfQuantity2 = "DHS";
				AssertNoMessageError("Valid", previousProcedure.CSI_UnitOfQuantity2Info, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCSI_TariffIsAvailableForATZL()
		{
			CombineAssertions(() =>
			{
				validation.ValidateCSI_Tariff();
				AssertHasMessageError("ATZL", previousProcedure.CSI_TariffInfo, MandatoryCommodityCode);

				previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
				validation.ValidateCSI_Tariff();
				AssertNoMessageError("ATA", previousProcedure.CSI_TariffInfo, MandatoryCommodityCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			previousProcedure = invoiceLine.PreviousProcedures.AddNew();
			previousProcedure.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			validation = new ExportPreviousProcedureValidation(previousProcedure);
		}
		JobComInvoiceLine invoiceLine;
		PreviousDocument previousProcedure;
		ExportPreviousProcedureValidation validation;

		const string InvalidTariffMessageError = "The entered Commodity Code is not valid.";
		const string MandatoryCommodityCode = "You have not entered a Commodity Code.";
		const string IntegerValuesRequired = "Only integer values are allowed for this Unit of Measurement.";
	}
}
