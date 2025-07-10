using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.DE.NCTS.Business.Testing.NCTSConditionalFunctionalityTestHelper;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(previousDocument.CSI_ReferenceNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference);
		}

		public void TestCheckCSI_ReferenceNumber_WithLevel()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(previousDocument.CSI_ReferenceNumberInfo,
				DEReferenceConstants.RefCusCodeListAttributes.Name.Reference + ";" + DEReferenceConstants.RefCusCodeListAttributes.Value.Item);
		}

		public void TestCheckCSI_ReferenceNumber_N337_Format()
		{
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("CSI_SubType != REG", previousDocument.CSI_ReferenceNumberInfo);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("CSI_SubType == REG", previousDocument.CSI_ReferenceNumberInfo);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATLAS_9DEZ()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

			CombineAssertions(() =>
			{
				previousDocument.Status = ZBool.False;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("9DEZ, Status false", previousDocument.CSI_ReferenceNumberInfo);

				previousDocument.Status = ZBool.True;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("9DEZ, Status true", previousDocument.CSI_ReferenceNumberInfo);
			});
		}

		public void TestCheckCSI_ReferenceNumber_ATLAS_9DEY()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;

			CombineAssertions(() =>
			{
				previousDocument.Status = ZBool.False;
				DE.Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("9DEY, Status false", previousDocument.CSI_ReferenceNumberInfo);

				previousDocument.Status = ZBool.True;
				DE.Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("9DEY, Status true", previousDocument.CSI_ReferenceNumberInfo);
			});
		}

		public void TestCheckCSI_Tariff_Mandatory()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_TariffInfo);
		}

		public void TestCheckCSI_Tariff_Length()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

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
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

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
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

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
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

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

		public void TestCheckCSI_Code()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, "AAA", "Document Type AAA",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_CodeInfo, "BAD", "AAA", "The code you have selected is not in the list.");
		}

		public void TestCheckCSI_Code_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_Mandatory_Procedure()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			ValidationTestHelper.AssertFieldIsNotMandatory(previousDocument.CSI_CodeInfo);
		}

		public void TestCheckCSI_SubType()
		{
			var messageError = "Please enter a Class.";
			var messageError2 = "Type 'REG' must not be used at the same time with other Types within one item.";
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				previousDocument.Validation.ValidateCSI_SubType();
				AssertNoMessageError("CSI_SubType isn't available", previousDocument.CSI_SubTypeInfo, messageError);
				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				previousDocument.Validation.ValidateCSI_SubType();
				AssertHasMessageError("CSI_SubType is available", previousDocument.CSI_SubTypeInfo, messageError);
				previousDocument.CSI_SubType = SimplifiedGrantAuthorizationList.Codes.J;
				AssertNoMessageError("Entered", previousDocument.CSI_SubTypeInfo, messageError);

				var previousProcedure = goodsItem.PreviousProcedures.AddNew();
				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				var previousProcedures2 = goodsItem.PreviousProcedures.AddNew();
				previousProcedures2.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				previousProcedure.Validation.ValidateCSI_SubType();
				AssertHasMessageError("Contain other previousDocument whose CSI_SubType is not REG", previousProcedure.CSI_SubTypeInfo, messageError2);

				previousProcedures2.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				previousProcedure.Validation.ValidateCSI_SubType();
				AssertNoMessageError(previousDocument.CSI_SubTypeInfo, messageError2);
			});
		}

		public void TestCheckRowMaxCount_999()
		{
			const string messageError = "You are only allowed a maximum of 999 Previous Document here.";
			while (goodsItem.PreviousDocuments.Count < 999)
			{
				goodsItem.PreviousDocuments.AddNew();
			}
			CombineAssertions(() =>
			{
				foreach (var procedureCode in new NctsPreviousProcedureList().GetAllCodes())
				{
					previousDocument.CSI_Procedure = procedureCode;
					previousDocument.Validation.ValidateAll();
					AssertNoRowMessageError(procedureCode + " No Row Error", previousDocument, messageError);
				}
			});

			while (goodsItem.PreviousDocuments.Count < 1001)
			{
				goodsItem.PreviousDocuments.AddNew();
			}
			CombineAssertions(() =>
			{
				foreach (var procedureCode in new NctsPreviousProcedureList().GetAllCodes())
				{
					previousDocument.CSI_Procedure = procedureCode;
					previousDocument.Validation.ValidateAll();
					AssertHasRowMessageError(procedureCode + " Has Row Error", previousDocument, messageError);
				}
			});
		}

		public void TestValidateAuthorizationNumber()
		{
			const string messageError = "You have not entered an Authorization Number.";
			CombineAssertions(() =>
			{
				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				AssertNoMessageError("AuthorizationNumber isn't available", previousDocument.AuthorizationNumberInfo, messageError);

				previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				previousDocument.Validation.ValidateAuthorizationNumber();
				AssertHasMessageError("Mandatory Procedure Code", previousDocument.AuthorizationNumberInfo, messageError);
				previousDocument.AuthorizationNumber = "123456";
				AssertNoMessageError("Entered", previousDocument.AuthorizationNumberInfo, messageError);
			});
		}

		public void TestCheckCSI_UnitOfQuantity_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ1", "UQ1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_UnitOfQuantityInfo, "UQ2", "UQ1");
		}

		public void TestCheckCSI_UnitOfQuantity_Mandatory()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(previousDocument.CSI_UnitOfQuantityInfo);

				previousDocument.CSI_Quantity = 10;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEnteredMessage("Commercial UQ"));
			});
		}

		public void TestCheckCSI_Quantity2()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_Quantity2Info);
		}

		public void TestCheckCSI_UnitOfQuantity2_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ1", "UQ1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertInvalidCodeMessageError(previousDocument.CSI_UnitOfQuantity2Info, "UQ2", "UQ1");
		}

		public void TestCheckCSI_UnitOfQuantity2_Mandatory()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_UnitOfQuantity2Info, MandatoryValidation.YouHaveNotEnteredMessage("Debit UQ"));
		}

		public void TestCheckCSI_LineNo_N337()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			CombineAssertions(() =>
			{
				previousDocument.CSI_SubType = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(previousDocument.CSI_LineNoInfo);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_LineNoInfo);
			});
		}

		public void TestCheckCSI_LineNo_9DEZ()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_LineNoInfo);
		}

		public void TestCheckCSI_LineNo_9DEY()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_LineNoInfo);
		}

		public void TestCheckCSI_Quantity_N337()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_QuantityInfo);
		}

		public void TestCheckCSI_Quantity_9DEZ()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertFieldIsNotMandatory(previousDocument.CSI_QuantityInfo);
		}

		public void TestCheckCSI_Description_9DEY()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(previousDocument.CSI_DescriptionInfo);
		}

		public void TestCheckCSI_Description_9DEZ()
		{
			previousDocument.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			ValidationTestHelper.AssertFieldIsNotMandatory(previousDocument.CSI_DescriptionInfo);
		}

		public void TestCheckCSI_ItemNumber()
		{
			const string message = "You have not entered an Item Number (1-99999).";
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(previousDocument.CSI_ItemNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber, message);
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(previousDocument.CSI_ReferenceNumber2Info, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
		}

		public void TestCheckCSI_ReferenceNumber2_WithLevel()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(previousDocument.CSI_ReferenceNumber2Info,
				DEReferenceConstants.RefCusCodeListAttributes.Name.Complement + ";" + DEReferenceConstants.RefCusCodeListAttributes.Value.Item);
		}

		public void TestCheckCSI_ReferenceNumber2MaxLength_Procedure()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var cargoDesc = bill.GoodsItems.AddNew();
			var previousProcedure = cargoDesc.PreviousProcedures.AddNew();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var previousDoc = cargoDesc.PreviousDocuments.AddNew();

			RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var maxLengthOfPreviousDoc = previousDoc.CSI_ReferenceNumber2Info.MaxLength;
				previousProcedure.CSI_ReferenceNumber2 = new ZString('X', maxLengthOfPreviousDoc + 1);
				var message = $"Length of {previousDoc.CSI_ReferenceNumber2Info.HumanReadableName} must not exceed {maxLengthOfPreviousDoc} characters.";
				AssertEquals(false, previousProcedure.CSI_ReferenceNumber2Info.Notifications.Contains(message));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousDocument previousDocument;
		const string InvalidTariffMessageError = "The entered Commodity Code is not valid.";

		void AssertPropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName, string notificationText = MandatoryValidation.YouHaveNotEntered)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, Factory, attributeName);
			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo, notificationText);

				previousDocument.CSI_Code = refCusCodeList2.ZZD_Code;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				previousDocument.CSI_Code = refCusCodeList3.ZZD_Code;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				previousDocument.CSI_Code = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);
			});
		}
	}
}
