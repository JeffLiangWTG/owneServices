using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code_Mandatory_ExportInvoiceLine()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lineAdditionalInfo.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_Mandatory_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(additionalInfo.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_ImportInvoiceLine_NotMandatory()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(lineAdditionalInfo.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_ExportInvoiceHeader_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(headerAdditionalInfo.CSI_CodeInfo);
		}

		public void TestCheckCSI_Code_ExportInvoiceHeader_ListValidation()
		{
			headerAdditionalInfo.CSI_Status = AdditionalInfoIssuerList.Codes.Customs;
			headerAdditionalInfo.CSI_Code = "INVALID";
			CombineAssertions(() =>
			{
				AssertHasMessageError("CSI_Status 'CUS'", headerAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

				headerAdditionalInfo.CSI_Status = AdditionalInfoIssuerList.Codes.Other;
				headerAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("CSI_Status 'ZZZ'", headerAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCSI_Code_ExportInvoiceLine_ListValidation()
		{
			lineAdditionalInfo.CSI_Status = AdditionalInfoIssuerList.Codes.Customs;
			lineAdditionalInfo.CSI_Code = "INVALID";
			CombineAssertions(() =>
			{
				AssertHasMessageError("CSI_Status 'CUS'", lineAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

				lineAdditionalInfo.CSI_Status = AdditionalInfoIssuerList.Codes.Other;
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("CSI_Status 'ZZZ'", lineAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCSI_Code_ExitDetail_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X, "AI44X");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X, "DE03", "DE03 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var exitDetailAddInfo = Factory.New<CusExitDetail>().AdditionalInfos.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(exitDetailAddInfo.CSI_CodeInfo, new ZString[] { "DE04" }, new ZString[] { "DE01", "DE02", "DE03" });
		}

		public void TestCheckCSI_Code_ExportInvoiceLine_C019()
		{
			const string message = "For the selected Type (Procedure) and CPC, only 'C019' can be selected.";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "10482";
			lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			var targetInfo = lineAdditionalInfo.CSI_CodeInfo;

			CombineAssertions(() =>
			{
				lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.X0000;
				AssertHasMessageError("CSI_Code <> 'C019'", targetInfo, message);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("CEI_Style doesn't start with '12'", targetInfo, message);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.JI_Procedure = "10472";
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("Digit 3-4 of JI_Procedure <> '48'", targetInfo, message);

				invoiceLine.JI_Procedure = "10482";
				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("CSI_SubType <> 'AUT'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019;
				AssertNoMessageError("CSI_Code = 'C019'", targetInfo, message);

				lineAdditionalInfo.CSI_Code = ZString.Empty;
				AssertNoMessageError("CSI_Code empty", targetInfo, message);
			});
		}

		public void TestCheckCSI_Code_REFInvalidCodes_ExportInvoiceLine()
		{
			AssertCheckCSI_Code_REFInvalidCodes(lineAdditionalInfo);
		}

		public void TestCheckCSI_Code_REFInvalidCodes_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			AssertCheckCSI_Code_REFInvalidCodes(additionalInfo);
		}

		public void TestCheckCSI_Code_INFMutuallyExclusiveCodes_ExportInvoiceLine()
		{
			AssertCheckCSI_Code_INFMutuallyExclusiveCodes(lineAdditionalInfo, invoiceLine.AdditionalInfos);
		}

		public void TestCheckCSI_Code_INFMutuallyExclusiveCodes_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			AssertCheckCSI_Code_INFMutuallyExclusiveCodes(additionalInfo, additionalInfo.ParentAsCusClassPartPivot.AdditionalInfos);
		}

		public void TestCheckCSI_Code_ExportInvoiceHeader_CodeX0004()
		{
			var errorMessage = @"Type 'X0004' is only valid for Destination Country/Region Code 'QQ'.";

			headerAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			headerAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.X0004;
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Germany;

			headerAdditionalInfo.Validation.ValidateCSI_Code();

			AssertHasMessageError(headerAdditionalInfo.CSI_CodeInfo, errorMessage);

			declaration.JE_GoodsDestination = "QQ";

			headerAdditionalInfo.Validation.ValidateCSI_Code();

			AssertNoMessageError(headerAdditionalInfo.CSI_CodeInfo, errorMessage);
		}

		public void TestCheckCSI_Status_NotMandatory_ExportInvoiceLine()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(lineAdditionalInfo.CSI_StatusInfo);
		}

		public void TestCheckCSI_Status_NotMandatory_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			ValidationTestHelper.AssertFieldIsNotMandatory(additionalInfo.CSI_StatusInfo);
		}

		public void TestCheckCSI_Status_ImportInvoiceLine_NotMandatory()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(lineAdditionalInfo.CSI_StatusInfo);
		}

		public void TestCheckCSI_Status_ExportInvoiceHeader_NotMandatory()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(headerAdditionalInfo.CSI_StatusInfo);
		}

		public void TestCheckCSI_Code_ExportInvoiceHeader_Unique()
		{
			AssertCheckCSI_Code_Unique(headerAdditionalInfo, invoice.AdditionalInfos);
		}

		public void TestCheckCSI_Description_ExportInvoiceHeader_Mandatory_CodeX0000()
		{
			headerAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.X0000;
			CombineAssertions(() =>
			{
				headerAdditionalInfo.Validation.ValidateCSI_Description();
				AssertHasMessageErrorContaining("CSI_Code 'X0000", headerAdditionalInfo.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				headerAdditionalInfo.CSI_Code = "XYZ";
				headerAdditionalInfo.Validation.ValidateCSI_Description();
				AssertNoMessageErrorContaining("CSI_Code 'XYZ", headerAdditionalInfo.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_Code_Unique_ExportInvoiceLine()
		{
			AssertCheckCSI_Code_Unique(lineAdditionalInfo, invoiceLine.AdditionalInfos);
		}

		public void TestCheckCSI_Code_Unique_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			AssertCheckCSI_Code_Unique(additionalInfo, additionalInfo.ParentAsCusClassPartPivot.AdditionalInfos);
		}

		public void TestCheckCSI_Description_ExportInvoiceLine_Mandatory_CodeX0000()
		{
			lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalInfoCodes.X0000;
			CombineAssertions(() =>
			{
				lineAdditionalInfo.Validation.ValidateCSI_Description();
				AssertHasMessageErrorContaining("CSI_Code 'X0000", lineAdditionalInfo.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				lineAdditionalInfo.CSI_Code = "XYZ";
				lineAdditionalInfo.Validation.ValidateCSI_Description();
				AssertNoMessageErrorContaining("CSI_Code 'XYZ", lineAdditionalInfo.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_ReferenceNumber_Mandatory()
		{
			AssertPropertyMandatoryIfAttributeExists(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, x => x.CSI_ReferenceNumberInfo, x => x.Validation.ValidateCSI_ReferenceNumber(), lineAdditionalInfo);
		}

		public void TestCheckCSI_ReferenceNumber_C019()
		{
			AssertReferenceNumberAuthorizationType(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
		}

		public void TestCheckCSI_ReferenceNumber_C516()
		{
			AssertReferenceNumberAuthorizationType(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C516, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
		}

		public void TestCheckCSI_ReferenceNumber_C601()
		{
			AssertReferenceNumberAuthorizationType(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
		}

		public void TestCheckCSI_ReferenceNumber_C601_Procedure51()
		{
			const string message = "For the selected Full Type and CPC, digit 1-2 of Reference must be 'DE'";
			lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601;
			invoiceLine.JI_Procedure = "10512";
			var targetInfo = lineAdditionalInfo.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				lineAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageError("CSI_Reference empty", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber = "GRIPO123";
				AssertHasMessageError("Digit 3-4 of JI_Procedure = '51', Digit 1-2 of CSI_ReferenceNumber <> 'DE'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				lineAdditionalInfo.CSI_ReferenceNumber = "GRIPO123";
				AssertNoMessageError("CSI_SubType <> 'AUT'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAdditionalInfo.CSI_Code = "XYZ";
				lineAdditionalInfo.CSI_ReferenceNumber = "GRIPO123";
				AssertNoMessageError("CSI_Code <> 'C601'", targetInfo, message);

				lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601;
				invoiceLine.JI_Procedure = "10542";
				lineAdditionalInfo.CSI_ReferenceNumber = "GRIPO123";
				AssertNoMessageError("Digit 3-4 of JI_Procedure <> '51'", targetInfo, message);

				invoiceLine.JI_Procedure = "10512";
				lineAdditionalInfo.CSI_ReferenceNumber = "GRIPO123";
				AssertHasMessageError("Digit 3-4 of JI_Procedure = '51', Digit 1-2 of CSI_ReferenceNumber <> 'DE'", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber = "DEIPO123";
				AssertNoMessageError("Digit 1-2 of CSI_ReferenceNumber = 'DE'", targetInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_C601_Procedure54()
		{
			const string message = "For the selected Full Type and CPC, digit 1-2 of Reference must NOT be 'DE'";
			lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601;
			invoiceLine.JI_Procedure = "10542";
			var targetInfo = lineAdditionalInfo.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				lineAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageError("CSI_Reference empty", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber = "DEIPO123";
				AssertHasMessageError("Digit 3-4 of JI_Procedure = '54', Digit 1-2 of CSI_ReferenceNumber = 'DE'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				lineAdditionalInfo.CSI_ReferenceNumber = "DEIPO123";
				AssertNoMessageError("CSI_SubType <> 'AUT'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAdditionalInfo.CSI_Code = "XYZ";
				lineAdditionalInfo.CSI_ReferenceNumber = "DEIPO123";
				AssertNoMessageError("CSI_Code <> 'C601'", targetInfo, message);

				lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C601;
				invoiceLine.JI_Procedure = "10512";
				lineAdditionalInfo.CSI_ReferenceNumber = "DEIPO123";
				AssertNoMessageError("Digit 3-4 of JI_Procedure <> '54'", targetInfo, message);

				invoiceLine.JI_Procedure = "10542";
				lineAdditionalInfo.CSI_ReferenceNumber = "DEIPO123";
				AssertHasMessageError("Digit 3-4 of JI_Procedure = '51', Digit 1-2 of CSI_ReferenceNumber = 'DE'", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber = "GRIPO123";
				AssertNoMessageError("Digit 1-2 of CSI_ReferenceNumber <> 'DE'", targetInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_C626()
		{
			AssertReferenceNumberAuthorizationType(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626, EU.Business.UniversalReferenceConstants.CusAuthorisationHeaderType.BindingTariffInformation);
		}

		public void TestCheckCSI_ReferenceNumber_C627()
		{
			AssertReferenceNumberAuthorizationType(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C627, EU.Business.UniversalReferenceConstants.CusAuthorisationHeaderType.BindingOriginInformation);
		}

		public void TestCheckCSI_ReferenceNumber2_Mandatory()
		{
			AssertPropertyMandatoryIfAttributeExists(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail, x => x.CSI_ReferenceNumber2Info, x => x.Validation.ValidateCSI_ReferenceNumber2(), lineAdditionalInfo);
		}

		public void TestCheckCSI_ReferenceNumber2_AUT_C626()
		{
			AssertCSI_ReferenceNumber2_AUT_EoriNumber(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626);
		}

		public void TestCheckCSI_ReferenceNumber2_AUT_C627()
		{
			AssertCSI_ReferenceNumber2_AUT_EoriNumber(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C627);
		}

		public void TestCheckCSI_ReferenceNumber2_MaxLength_AUT_C626()
		{
			AssertCSI_ReferenceNumber2_AUT_MaxLength(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C626);
		}

		public void TestCheckCSI_ReferenceNumber2_MaxLength_AUT_C627()
		{
			AssertCSI_ReferenceNumber2_AUT_MaxLength(UniversalReferenceConstants.RefCusCodeList.Codes.Code_C627);
		}

		public void TestCheckCSI_SubType_ImportInvoiceLine_NotMandatory()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(lineAdditionalInfo.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_SubType_ImportInvoiceHeader_NotMandatory()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(headerAdditionalInfo.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_SubType_ExportInvoiceHeader()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(headerAdditionalInfo.CSI_SubTypeInfo, "INV", AdditionalDocTypeList.Codes.TransportDocuments);
		}

		public void TestCheckCSI_SubType_ExportInvoiceLine()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(lineAdditionalInfo.CSI_SubTypeInfo, "INV", AdditionalDocTypeList.Codes.AdditionalInformation);
		}

		public void TestCheckCSI_SubType_Maximum9Records_ExportInvoiceLine()
		{
			AssertCheckCSI_SubType_Maximum9Records(invoiceLine.AdditionalInfos);
		}

		public void TestCheckCSI_SubType_Maximum9Records_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			AssertCheckCSI_SubType_Maximum9Records(additionalInfo.ParentAsCusClassPartPivot.AdditionalInfos);
		}

		public void TestCheckCSI_RX_NKCurrency_Mandatory_ExportInvoiceLine()
		{
			AssertPropertyMandatoryIfAttributeExists(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value, x => x.CSI_RX_NKCurrencyInfo, x => x.Validation.ValidateCSI_RX_NKCurrency(), lineAdditionalInfo);
		}

		public void TestCheckCSI_RX_NKCurrency_Mandatory_ExportCusClassPartPivot()
		{
			var additionalInfo = AdditionalInfoTest.CreateAdditionalInfoWithPivotParent(Factory);
			AssertPropertyMandatoryIfAttributeExists(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value, x => x.CSI_RX_NKCurrencyInfo, x => x.Validation.ValidateCSI_RX_NKCurrency(), additionalInfo);
		}

		public void TestCheckCSI_RX_NKCurrency_ListValidation()
		{
			var message = ListValidation.InvalidCodeMessageError.ToString();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CURRE, "CURRE");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CURRE, "TRY", "Türkische Lire", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				lineAdditionalInfo.CSI_RX_NKCurrency = "AUD";
				AssertNoMessageErrorContaining("Import invoice line, invalid currency", lineAdditionalInfo.CSI_RX_NKCurrencyInfo, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				lineAdditionalInfo.Validation.ValidateCSI_RX_NKCurrency();
				AssertHasMessageErrorContaining("Export invoice line, invalid currency", lineAdditionalInfo.CSI_RX_NKCurrencyInfo, message);

				lineAdditionalInfo.CSI_RX_NKCurrency = "TRY";
				AssertNoMessageErrorContaining("Export invoice line, valid currency", lineAdditionalInfo.CSI_RX_NKCurrencyInfo, message);
			});
		}

		public void TestCheckCSI_ValueAmountIntegerDuringTransitionPeriod()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000110;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var targetInfo = lineAdditionalInfo.CSI_ValueInfo;

			const string message = "Amount should be an integer value.";
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					lineAdditionalInfo.CSI_Value = 10.5;
					AssertHasMessageError("CSI_Value non-integer during transition period", targetInfo, message);
					entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110410;
					lineAdditionalInfo.CSI_Value = 10.5;
					AssertNoMessageError("CSI_Value 110410 transition period", targetInfo, message);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000110;
					lineAdditionalInfo.CSI_Value = 10.5;
					AssertNoMessageError("CSI_Value non-integer after transition period", targetInfo, message);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageStatus = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			headerAdditionalInfo = invoice.AdditionalInfos.AddNew();
			lineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		AdditionalInfo headerAdditionalInfo;
		AdditionalInfo lineAdditionalInfo;

		void AssertPropertyMandatoryIfAttributeExists(string attributeName, Func<AdditionalInfo, ZPropertyInfo> propertyInfoGetter, Action<AdditionalInfo> propertyValidator, AdditionalInfo additionalInfo)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "AI44E");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Level, "Level", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, Core.Constants.CountryCodes.Germany);

			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44E, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			var propertyInfo = propertyInfoGetter.Invoke(additionalInfo);
			CombineAssertions(() =>
			{
				propertyValidator.Invoke(additionalInfo);
				AssertNoMessageErrorContaining("CSI_Code empty", propertyInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_Code = "DE01";
				propertyValidator.Invoke(additionalInfo);
				AssertNoMessageErrorContaining("CSI_Code doesn't have attribute 'Reference'='Y'", propertyInfo, MandatoryValidation.YouHaveNotEntered);

				additionalInfo.CSI_Code = "DE02";
				propertyValidator.Invoke(additionalInfo);
				AssertHasMessageErrorContaining("CSI_Code has attribute 'Reference'='Y'", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		void AssertReferenceNumberAuthorizationType(string code, string authorizationType)
		{
			var message = $"For the selected Full Type, digit 3-5 of Reference must be '{authorizationType}'";
			lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			lineAdditionalInfo.CSI_Code = code;
			var targetInfo = lineAdditionalInfo.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				lineAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageError("CSI_Reference empty", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber = "GRZZZ123";
				AssertHasMessageError($"Digit 3-5 of CSI_ReferenceNumber <> '{authorizationType}'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				lineAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("CSI_SubType <> 'AUT'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAdditionalInfo.CSI_Code = "XYZ";
				lineAdditionalInfo.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError($"CSI_Code <> '{code}'", targetInfo, message);

				lineAdditionalInfo.CSI_Code = code;
				lineAdditionalInfo.CSI_ReferenceNumber = $"GR{authorizationType}123";
				AssertNoMessageError($"Digit 3-5 of CSI_ReferenceNumber = '{authorizationType}'", targetInfo, message);
			});
		}

		void AssertCSI_ReferenceNumber2_AUT_EoriNumber(string csiCode)
		{
			const string message = "Please enter a valid EORI-Number (Country/Region code followed by up to 15 alphanumeric characters).";
			var targetInfo = lineAdditionalInfo.CSI_ReferenceNumber2Info;
			CombineAssertions(() =>
			{
				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				lineAdditionalInfo.CSI_Code = csiCode;
				lineAdditionalInfo.CSI_ReferenceNumber2 = "XYZ";
				AssertNoMessageError($"Kind = 'REF', Code = '{csiCode}'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019;
				lineAdditionalInfo.Validation.ValidateCSI_ReferenceNumber2();
				AssertNoMessageError("Kind = 'AUT', Code = 'C019'", targetInfo, message);

				lineAdditionalInfo.CSI_Code = csiCode;
				lineAdditionalInfo.Validation.ValidateCSI_ReferenceNumber2();
				AssertHasMessageError($"Kind = 'AUT', Code = '{csiCode}', invalid format", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber2 = "DE012345678901234";
				AssertNoMessageError($"Kind = 'AUT', Code = '{csiCode}', valid format", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber2 = "DE01234A-35C";
				AssertNoMessageError($"Kind = 'AUT', Code = '{csiCode}', alphanumeric", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber2 = "de012345678901234";
				AssertHasMessageError($"Kind = 'AUT', Code = '{csiCode}', country code not upper-case", targetInfo, message);

				lineAdditionalInfo.CSI_ReferenceNumber2 = ZString.Empty;
				AssertHasMessageError($"Kind = 'AUT', Code = '{csiCode}', CSI_ReferenceNumber2 empty", targetInfo, message);
			});
		}

		void AssertCSI_ReferenceNumber2_AUT_MaxLength(string csiCode)
		{
			const string message = "The maximum length of Detail (12 characters) has been exceeded.";
			var valueExceedingMaxLength = ZString.Empty.PadLeft(17, 'A');
			var targetInfo = lineAdditionalInfo.CSI_ReferenceNumber2Info;
			CombineAssertions(() =>
			{
				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				lineAdditionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C019;
				lineAdditionalInfo.CSI_ReferenceNumber2 = valueExceedingMaxLength;
				AssertHasMessageError("Kind = 'AUT', Code = 'C019'", targetInfo, message);

				lineAdditionalInfo.CSI_Code = csiCode;
				lineAdditionalInfo.CSI_ReferenceNumber2 = valueExceedingMaxLength;
				AssertNoMessageError($"Kind = 'AUT', Code = '{csiCode}'", targetInfo, message);

				lineAdditionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				lineAdditionalInfo.CSI_ReferenceNumber2 = valueExceedingMaxLength;
				AssertHasMessageError($"Kind = 'REF', Code = '{csiCode}'", targetInfo, message);
			});
		}

		void AssertCheckCSI_Code_REFInvalidCodes(AdditionalInfo additionalInfo)
		{
			const string message = "Additional References of Type 9ZZX, 9ZZY and 9ZZZ are currently not valid.";

			var targetInfo = additionalInfo.CSI_CodeInfo;
			CombineAssertions(() =>
			{
				foreach (var code in new[]
				{
					UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZX,
					UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZY,
					UniversalReferenceConstants.RefCusCodeList.Codes.Code_9ZZZ
				})
				{
					additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
					additionalInfo.CSI_Code = code;
					AssertHasMessageError($"CSI_Code '{code}'", targetInfo, message);

					additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
					additionalInfo.Validation.ValidateCSI_Code();
					AssertNoMessageError($"CSI_Code '{code}', CSI_SubType <> 'REF'", targetInfo, message);
				}

				additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.Codes.Code_C651;
				AssertNoMessageError("CSI_Code not in ('9ZZX', '9ZZY', '9ZZZ')", targetInfo, message);
			});
		}

		void AssertCheckCSI_Code_INFMutuallyExclusiveCodes(AdditionalInfo additionalInfo, AdditionalInfoCollection additionalInfos)
		{
			const string message = "You only may enter one of the Additional Information Types 00700, 00800 or 00900 at the same time.";
			var mutuallyExclusiveCodes = new[]
			{
				UniversalReferenceConstants.RefCusCodeList.Codes.Code_00700,
				UniversalReferenceConstants.RefCusCodeList.Codes.Code_00800,
				UniversalReferenceConstants.RefCusCodeList.Codes.Code_00900
			};
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var targetInfo = additionalInfo.CSI_CodeInfo;
			var additionalInfo2 = additionalInfos.AddNew();

			CombineAssertions(() =>
			{
				foreach (var code in mutuallyExclusiveCodes)
				{
					additionalInfo.CSI_Code = code;

					foreach (var anotherCode in mutuallyExclusiveCodes.Except(new[] { code }))
					{
						additionalInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
						additionalInfo2.CSI_Code = anotherCode;
						additionalInfo.Validation.ValidateCSI_Code();
						AssertHasMessageError($"Code1 '{code}', Code2 '{anotherCode}'", targetInfo, message);

						additionalInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
						additionalInfo.Validation.ValidateCSI_Code();
						AssertNoMessageError($"Code1 '{code}', Code2 '{anotherCode}' but CSI_SubType <> 'INF'", targetInfo, message);

						additionalInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
						additionalInfo2.CSI_Code = "XYZ";
						additionalInfo.Validation.ValidateCSI_Code();
						AssertNoMessageError($"Code1 '{code}', Code2 not in (00700, 00800, 00900)", targetInfo, message);
					}
				}
			});
		}

		void AssertCheckCSI_Code_Unique(AdditionalInfo additionalInfo, AdditionalInfoCollection additionalInfos)
		{
			const string message = "An Additional Document of Type CODE has already been entered.";
			additionalInfo.CSI_Code = "CODE";
			var additionalInfo2 = additionalInfos.AddNew();
			additionalInfo2.CSI_Code = "CODE";
			CombineAssertions(() =>
			{
				AssertHasMessageError("Code has duplicate", additionalInfo2.CSI_CodeInfo, message);

				additionalInfo2.CSI_Code = "XYZ";
				AssertNoMessageError("Code is unique", additionalInfo2.CSI_CodeInfo, message);
			});
		}

		void AssertCheckCSI_SubType_Maximum9Records(AdditionalInfoCollection additionalInfos)
		{
			const string message = "You are only allowed a maximum of 9 Authorization records";

			CombineAssertions(() =>
			{
				for (var i = 0; i < 9; i++)
				{
					var autAddInfo = additionalInfos.AddNew();
					autAddInfo.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
					AssertNoMessageError($"AUT #{i}", autAddInfo.CSI_SubTypeInfo, message);
				}

				var autAddInfo10 = additionalInfos.AddNew();
				autAddInfo10.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
				AssertHasMessageError("AUT #10", autAddInfo10.CSI_SubTypeInfo, message);

				var addInfo = additionalInfos.AddNew();
				addInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				AssertNoMessageError("Not AUT", addInfo.CSI_SubTypeInfo, message);
			});
		}
	}
}
