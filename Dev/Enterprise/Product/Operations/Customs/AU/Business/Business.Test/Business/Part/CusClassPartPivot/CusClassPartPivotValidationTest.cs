using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusClassPartPivotValidationTest : Customs.Business.Testing.CusClassPartPivotValidationTest
	{
		public override void TestCheckCI_CC()
		{
			var inactiveClassificationIMP1 = Factory.New<Classification>();
			inactiveClassificationIMP1.CC_ClassificationType = ClassificationType.IMP;
			inactiveClassificationIMP1.CC_LookupCode = "CLI";
			inactiveClassificationIMP1.CC_IsActive = false;
			var classificationIMP1 = Factory.New<Classification>();
			classificationIMP1.CC_ClassificationType = ClassificationType.IMP;
			classificationIMP1.CC_LookupCode = "CL1";
			var classificationIMP2 = Factory.New<Classification>();
			classificationIMP2.CC_ClassificationType = ClassificationType.IMP;
			classificationIMP2.CC_LookupCode = "CL2";
			var classificationEXP = Factory.New<Classification>();
			classificationEXP.CC_ClassificationType = ClassificationType.EXP;
			classificationEXP.CC_LookupCode = "CL3";
			var part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "PartNum";

			var pivot1 = Factory.New<CusClassPartPivot>();
			pivot1.CI_OP = part.PK;
			pivot1.CI_RN_NKCountry = "AU";
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;

			pivot1.CI_CC = classificationIMP1.PK;
			AssertNoNotifications(pivot1.CI_CCInfo);
			pivot1.CI_CC = classificationIMP2.PK;
			AssertNoNotifications(pivot1.CI_CCInfo);
			pivot1.CI_CC = ZGuid.Empty;
			AssertHasErrorContaining(pivot1.CI_CCInfo, "");
			pivot1.CI_CC = inactiveClassificationIMP1.PK;
			AssertNoError(pivot1.CI_CCInfo, "");
			AssertHasWarningContaining(pivot1.CI_CCInfo, "The selected classification is not Active");
			pivot1.CI_CC = classificationEXP.PK;
			AssertNoNotifications(pivot1.CI_CCInfo);
		}

		public void TestCheckCI_TariffNumWarnsIncorrectTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "04052000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "DAIRY SPREADS", taxOrFeeCode: "GST");

			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0405200019", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "DAIRY SPREADS", taxOrFeeCode: "GST", compositeKey: "01.04..05.20.00.19");

			helper.CreateNomenclatureGroupType("AU", "Australian nomenclature");
			helper.CreateNomenclatureGroup("AU", "0405", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Dairy spreads", compositeKey: "01.04..05", nomenclatureGroupType: "AU");
			helper.CreateNomenclatureGroup("AU", "0405200019", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Dairy spreads", compositeKey: "01.04..05.20.00.19", nomenclatureGroupType: "AU");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var part = Factory.New<AUOrgSupplierPart>();
				part.OP_PartNum = "PartNum";

				var pivot1 = Factory.New<CusClassPartPivot>();
				pivot1.CI_OP = part.PK;
				pivot1.CI_RN_NKCountry = "AU";

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				pivot1.CI_TariffNum = "1234";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				pivot1.CI_TariffNum = "1234";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				pivot1.CI_TariffNum = "0405";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				pivot1.CI_TariffNum = "0405";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				pivot1.CI_TariffNum = "0405.20.00 19";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoNotifications(pivot1.CI_TariffNumInfo);

				pivot1.CI_TariffNum = "0405.20.00 21";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				pivot1.CI_TariffNum = "0405.20.00";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoNotifications(pivot1.CI_TariffNumInfo);

				pivot1.CI_TariffNum = "0405.20.01";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);
			}
		}

		public void TestCheckCI_TariffNumWarnsIncorrectTariff_AHECC_AUCClass()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var part = Factory.New<AUOrgSupplierPart>();
				part.OP_PartNum = "PartNum";

				var pivot1 = Factory.New<CusClassPartPivot>();
				pivot1.CI_OP = part.PK;
				pivot1.CI_RN_NKCountry = "AU";

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				pivot1.CI_TariffNum = "1234";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				pivot1.CI_TariffNum = "1234";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				pivot1.CI_TariffNum = "0405";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				pivot1.CI_TariffNum = "0405";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				pivot1.CI_TariffNum = "0405.20.00 19";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoNotifications(pivot1.CI_TariffNumInfo);

				pivot1.CI_TariffNum = "0405.20.00 21";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);

				pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				pivot1.CI_TariffNum = "0405.20.00";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertNoNotifications(pivot1.CI_TariffNumInfo);

				pivot1.CI_TariffNum = "0405.20.01";
				pivot1.Validation.ValidateCI_TariffNum();
				AssertHasMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffDoesNotExist);
				AssertNoMessageError(pivot1.CI_TariffNumInfo, CusClassPartPivotValidation.TariffIsPartial);
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionForInvalidData()
		{
			var classificationIMP1 = Factory.New<Classification>();
			classificationIMP1.CC_ClassificationType = ClassificationType.IMP;
			classificationIMP1.CC_LookupCode = "CL1";
			classificationIMP1.CC_RN_NKCountryCode = "NZ";
			var classificationIMP2 = Factory.New<Classification>();
			classificationIMP2.CC_ClassificationType = ClassificationType.IMP;
			classificationIMP2.CC_LookupCode = "CL2";
			var part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "PART NUMBER";
			var pivot1 = Factory.New<CusClassPartPivot>();
			pivot1.CI_OP = part.PK;
			pivot1.CI_RN_NKCountry = "AU";
			pivot1.CI_CC = classificationIMP1.PK;
			var pivot2 = Factory.New<CusClassPartPivot>();
			pivot2.CI_OP = part.PK;
			pivot2.CI_RN_NKCountry = "AU";
			pivot2.CI_CC = classificationIMP2.PK;
			AssertNoNotifications(pivot2.CI_CCInfo);
			pivot1.Validation.ValidateCI_CC();
			AssertEquals("Part (PART NUMBER) has a AU pivot with a NZ classification attached.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
