using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsArrivalCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateNewRecordHasDataSpecified()
		{
			const string message = "No data was specified.";
			CombineAssertions(() =>
			{
				var arrivalCargoDesc = Factory.New<NctsArrivalCargoDesc>();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowError("DIF Goods item", arrivalCargoDesc, message);

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.Validation.ValidateAll();
				AssertHasRowError("NEW Goods item", arrivalCargoDesc, message);

				arrivalCargoDesc.BY_Description = "Test";
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowError("Fill in BY_Description", arrivalCargoDesc, message);

				arrivalCargoDesc.BY_Description = ZString.Empty;
				arrivalCargoDesc.BY_CusC4Number = "1";
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowError("Fill in BY_CusC4Number", arrivalCargoDesc, message);

				arrivalCargoDesc.BY_CusC4Number = ZString.Empty;
				arrivalCargoDesc.BY_GrossWeight = 1;
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowError("Fill in BY_GrossWeight", arrivalCargoDesc, message);

				arrivalCargoDesc.BY_GrossWeight = ZDecimal.Zero;
				arrivalCargoDesc.BY_NetWeight = 1;
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowError("Fill in BY_NetWeight", arrivalCargoDesc, message);

				arrivalCargoDesc.BY_NetWeight = ZDecimal.Zero;
				arrivalCargoDesc.BY_CommodityCode = "1";
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowError("Fill in BY_CommodityCode", arrivalCargoDesc, message);
			});
		}

		public void TestCheckBY_UnloadedState()
		{
			var arrivalCargoDesc = Factory.New<NctsArrivalCargoDesc>();
			var unloadedStateInfo = arrivalCargoDesc.BY_UnloadedStateInfo;

			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = ZString.Empty;
				AssertHasErrorContaining("Empty value should have an error", unloadedStateInfo, MandatoryValidation.MustBeEntered);

				arrivalCargoDesc.BY_UnloadedState = "XXX";
				AssertHasErrorContaining("XXX should have an error", unloadedStateInfo, ListValidation.InvalidCodeError);

				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertNoNotifications("DEC should not have an error", unloadedStateInfo);

				arrivalCargoDesc.BY_UnloadedState = "NEW";
				AssertNoNotifications("NEW should not have an error (even though it shouldn't be selectable)", unloadedStateInfo);
			});
		}

		public void TestCheckBY_UnloadedState_DEC()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalCargoDesc = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			var package = arrivalCargoDesc.Packages.AddNew();
			var unloadedStateInfo = arrivalCargoDesc.BY_UnloadedStateInfo;
			var message = $"The goods item {arrivalCargoDesc.BY_LineNo} of house consignment {arrivalCargoDesc.Bill.MovementDetail.B9_SeqNo} has an unloaded state DEC, while there were changes in the related documents or packages. These changes will not be sent to customs unless you change the unloaded state to DIF.";

			CombineAssertions(() =>
			{
				package.B5_TypeOfDifference = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertHasMessageErrorContaining("DEC should have message error when Package differs from DEC", unloadedStateInfo, message);

				package.B5_TypeOfDifference = "MIS";
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertHasMessageErrorContaining("DEC should have message error when Package differs from DEC", unloadedStateInfo, message);

				package.B5_TypeOfDifference = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertNoNotifications("DEC should not have an error when Package is also DEC", unloadedStateInfo);

				var supportingDoc = arrivalCargoDesc.SupportingDocuments.AddNew();
				supportingDoc.CSI_Status = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertHasMessageErrorContaining("DEC should have message error when Supporting Doc differs from DEC", unloadedStateInfo, message);

				supportingDoc.CSI_Status = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertNoNotifications("DEC should not have an error when Supporting Doc is also DEC", unloadedStateInfo);

				var additionalDocuments = arrivalCargoDesc.AdditionalInfos.AddNew();
				additionalDocuments.CSI_Status = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertHasMessageErrorContaining("DEC should have message error when AdditionalDocuments differs from DEC", unloadedStateInfo, message);

				additionalDocuments.CSI_Status = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "DEC";
				AssertNoNotifications("DEC should not have an error when AdditionalDocuments is also DEC", unloadedStateInfo);
			});
		}

		public void TestCheckBY_UnloadedState_DIF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalCargoDesc = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.BY_HarmonisedTariff = "12345121";
			arrivalCargoDesc.BY_CusC4Number = "C4";
			arrivalCargoDesc.BY_Description = "DE";
			arrivalCargoDesc.BY_GrossWeight = 2;
			arrivalCargoDesc.BY_GrossWeightUnit = "KG";
			arrivalCargoDesc.BY_NetWeight = 1;
			arrivalCargoDesc.BY_NetWeightUnit = "KG";
			arrivalCargoDesc.BY_UnloadedState = "DIF";
			var unloadedGoodsItem = arrivalCargoDesc.UnloadedGoodsItem;
			unloadedGoodsItem.BY_HarmonisedTariff = "1234512345";
			var unloadedStateInfo = arrivalCargoDesc.BY_UnloadedStateInfo;
			var message = $"The goods item {arrivalCargoDesc.BY_LineNo} of house consignment {arrivalCargoDesc.Bill.MovementDetail.B9_SeqNo} has an unloaded state DIF, while there are no changes in the related documents or packages. This might potentially give an error by customs. Please change the status to DEC.";

			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertNoNotifications("Unloaded have a difference to the declared values", unloadedStateInfo);

				arrivalCargoDesc.BY_HarmonisedTariff = "12345123";
				var supportingDoc = arrivalCargoDesc.SupportingDocuments.AddNew();
				supportingDoc.CSI_Status = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertHasMessageErrorContaining("Unloaded have no difference to the declared values with Supporting doc DEC", unloadedStateInfo, message);

				supportingDoc.CSI_Status = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertNoNotifications("Unloaded have no difference to the declared values with Supporting doc NEW", unloadedStateInfo);

				arrivalCargoDesc.SupportingDocuments.Delete(supportingDoc);
				var additionalDocuments = arrivalCargoDesc.AdditionalInfos.AddNew();
				additionalDocuments.CSI_Status = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertHasMessageErrorContaining("Unloaded have no difference to the declared values with AdditionalDocument doc DEC", unloadedStateInfo, message);

				additionalDocuments.CSI_Status = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertNoNotifications("Unloaded have no difference to the declared values with AdditionalDocument doc NEW", unloadedStateInfo);

				arrivalCargoDesc.AdditionalInfos.Delete(additionalDocuments);
				var package = arrivalCargoDesc.Packages.AddNew();
				package.B5_TypeOfDifference = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertHasMessageErrorContaining("Unloaded have no difference to the declared values with Supporting doc DEC, AdditionalDocument doc DEC and Package DEC", unloadedStateInfo, message);

				package.B5_TypeOfDifference = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "DIF";
				AssertNoNotifications("Unloaded have no difference to the declared values with Package NEW", unloadedStateInfo);
			});
		}

		public void TestCheckBY_UnloadedState_MIS()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalCargoDesc = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			var package = arrivalCargoDesc.Packages.AddNew();
			var unloadedStateInfo = arrivalCargoDesc.BY_UnloadedStateInfo;
			var message = $"The goods item {arrivalCargoDesc.BY_LineNo} of house consignment {arrivalCargoDesc.Bill.MovementDetail.B9_SeqNo} has an unloaded state MIS, while there are changes (NEW) in the related documents or packages. These changes will not be sent to customs because the entire house will be sent as “missing”. Please change the unloaded state of the house consignment to DIF or delete the newly created goods items or documents.";

			CombineAssertions(() =>
			{
				package.B5_TypeOfDifference = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "MIS";
				AssertHasMessageErrorContaining("MIS should have message error when Package is NEW", unloadedStateInfo, message);

				package.B5_TypeOfDifference = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "MIS";
				AssertNoNotifications("MIS should not have an error when Package is DEC", unloadedStateInfo);

				var supportingDoc = arrivalCargoDesc.SupportingDocuments.AddNew();
				supportingDoc.CSI_Status = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "MIS";
				AssertHasMessageErrorContaining("MIS should have message error when Supporting Doc is NEW", unloadedStateInfo, message);

				supportingDoc.CSI_Status = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "MIS";
				AssertNoNotifications("MIS should not have an error when Supporting Doc is DEC", unloadedStateInfo);

				var additionalInfo = arrivalCargoDesc.AdditionalInfos.AddNew();
				additionalInfo.CSI_Status = "NEW";
				arrivalCargoDesc.BY_UnloadedState = "MIS";
				AssertHasMessageErrorContaining("MIS should have message error when additionalInfo is NEW", unloadedStateInfo, message);

				additionalInfo.CSI_Status = "DEC";
				arrivalCargoDesc.BY_UnloadedState = "MIS";
				AssertNoNotifications("MIS should not have an error when additionalInfo is DEC", unloadedStateInfo);
			});
		}

		public void TestCheckBY_HarmonisedTariff_ConditionA060()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var lvTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Latvia, Constants.TariffTypes.Export).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Latvia, lvTariffTypePK, "01000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Latvia, lvTariffTypePK, "010001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			var harmonisedTariffInfo = arrivalCargoDesc.BY_HarmonisedTariffInfo;

			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_HarmonisedTariff = "99999999";
				AssertHasMessageErrors("Code 99999999 does not exists, error should be throw", harmonisedTariffInfo);

				arrivalCargoDesc.BY_HarmonisedTariff = "01000001";
				AssertNoMessageErrors("Commodity Code '01000001' is valid", harmonisedTariffInfo);

				arrivalCargoDesc.BY_HarmonisedTariff = "01000002";
				AssertHasMessageErrors("Code 01000002 does not exists, error should be thrown", harmonisedTariffInfo);

				arrivalCargoDesc.BY_HarmonisedTariff = "010002";
				AssertHasMessageErrors("010002 doesn't exist and no other code starting with 010002 exists", harmonisedTariffInfo);

				arrivalCargoDesc.BY_HarmonisedTariff = "010001";
				AssertListValidationInvalidCodeMessageError(harmonisedTariffInfo, false);

				var previousDocument = bill.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = PreviousDocumentTypes.GoodsDeclarationForExportation;
				arrivalCargoDesc.BY_HarmonisedTariff = "010001";
				AssertHasMessageError(harmonisedTariffInfo, "Commodity code must be 8 positions long because the previous document is an export document.(A060)");
			});
		}

		public void TestCheckBY_HarmonisedTariff_NR0004()
		{
			var message = "[NR0004] Commodity Code must be 6 or 8 digits of the full commodity code.(A060)";
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			var harmonisedTariffInfo = arrivalCargoDesc.BY_HarmonisedTariffInfo;

			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleNR0004Active);

				foreach (var testCase in new string[] { "999" }.SelectMany(GetRuleCase).ToArray())
				{
					testCase.Invoke();
					AssertHasMessageError("Commodity Code '999' is too short", harmonisedTariffInfo, message);
				}
			}

			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.DisableRule(rule => rule.IsRuleNR0004Active);

				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				bill = header.Bills.AddNew();
				arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				harmonisedTariffInfo = arrivalCargoDesc.BY_HarmonisedTariffInfo;
				foreach (var testCase in new string[] { "short", "010001", "01000001" }.SelectMany(GetRuleCase).ToArray())
				{
					testCase.Invoke();
					AssertNoMessageError("Commodity Code is valid", harmonisedTariffInfo, message);
				}
			}

			IEnumerable<Action> GetRuleCase(string commodityCode)
			{
				yield return () =>
				{
					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					arrivalCargoDesc.BY_HarmonisedTariff = commodityCode;
				};

				yield return () =>
				{
					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
					arrivalCargoDesc.BY_HarmonisedTariff = commodityCode;
				};
			}
		}

		public void TestCheckBY_HarmonisedTariff_NR0055()
		{
			var message = "[NR0055] Commodity Code must be 8 or 10 digits.";

			CombineAssertions(() =>
			{
				using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
				{
					ruleTestContext.DisableRule(rule => rule.IsRuleNR0055Active);
					var arrivalCargoDesc = GetNctsArrivalGoodItem();
					AssertNoMessageErrorContaining("Rule Disable: Commodity Code has no error if rule is not active", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);

					ruleTestContext.EnableRule(rule => rule.IsRuleNR0055Active);
					arrivalCargoDesc = GetNctsArrivalGoodItem();
					arrivalCargoDesc.Validation.ValidateBY_HarmonisedTariff();
					AssertHasMessageErrorContaining("Rule Enable: Commodity Code has error cause is not 8 or 10 chr", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);

					arrivalCargoDesc.BY_HarmonisedTariff = ZString.Empty;
					AssertNoMessageErrorContaining("Rule Enable: Commodity Code empty has no error cause is empty", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);

					arrivalCargoDesc.BY_HarmonisedTariff = "1";
					AssertHasMessageErrorContaining("Rule Enable: Commodity Code has error cause is not 8 or 10 chr", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);

					arrivalCargoDesc.BY_HarmonisedTariff = "12345678";
					AssertNoMessageErrorContaining("Rule Enable: Commodity Code has no error cause is 8", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);

					arrivalCargoDesc.BY_HarmonisedTariff = "123456";
					AssertHasMessageErrorContaining("Rule Enable: Commodity Code has error cause is not 8 or 10 chr", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);

					arrivalCargoDesc.BY_HarmonisedTariff = "1234567890";
					AssertNoMessageErrorContaining("Rule Enable: Commodity Code has no error cause is 10", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
					arrivalCargoDesc.BY_HarmonisedTariff = "1";
					AssertNoMessageErrorContaining("Rule Enable: Commodity Code has no error cause is not NEW or DIF", arrivalCargoDesc.BY_HarmonisedTariffInfo, message);
				}
			});

			NctsArrivalCargoDesc GetNctsArrivalGoodItem()
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var bill = header.Bills.AddNew();
				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_HarmonisedTariff = "123456789";

				return arrivalCargoDesc;
			}
		}

		public void TestCheckBY_FormattedHarmonisedTariff()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var lvTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Latvia, Constants.TariffTypes.Export).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Latvia, lvTariffTypePK, "01000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();

			CombineAssertions(NctsUnloadedStateList.Codes.NEW, () =>
			{
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "";
				AssertHasMessageErrorContaining(arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);

				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "01000001";
				AssertNoMessageErrors(arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo);

				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "11000001";
				AssertHasMessageErrorContaining(arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, "There is no commodity code starting with");

				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "010000";
				AssertHasMessageErrorContaining(arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, "There is no commodity code starting with");
			});

			CombineAssertions(NctsUnloadedStateList.Codes.DEC, () =>
			{
				AssertFormattedHarminisedTariffNoErrorOnUnloadingState(arrivalCargoDesc, NctsUnloadedStateList.Codes.DEC);
			});

			CombineAssertions(NctsUnloadedStateList.Codes.DIF, () =>
			{
				AssertFormattedHarminisedTariffNoErrorOnUnloadingState(arrivalCargoDesc, NctsUnloadedStateList.Codes.DIF);
			});

			CombineAssertions(NctsUnloadedStateList.Codes.MIS, () =>
			{
				AssertFormattedHarminisedTariffNoErrorOnUnloadingState(arrivalCargoDesc, NctsUnloadedStateList.Codes.MIS);
			});
		}

		public void TestCheckBY_FormattedHarmonisedTariff_AtLeast6Digits()
		{
			var arrivalCargoDesc = Factory.New<NctsArrivalCargoDesc>();
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			const string message = "Commodity Code must have at least 6 digits";
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "12345";
				AssertHasMessageError("Less than 6 digits (=7 characters)", arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, message);

				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "123456";
				AssertNoMessageError("6 digits (=7 characters)", arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, message);

				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "1234567";
				AssertNoMessageError("More than 6 digits (=7 characters)", arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, message);
			});
		}

		public void TestCheckBY_HarmonisedTariff_6DigitsInWcoGroup()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, parent: grouping);
			var tariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, Constants.TariffTypes.HarmonizedSystem).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				tariffTypePK, "013456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "12345";
				AssertHasMessageErrorContaining("Invalid Code", arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, "There is no commodity code starting with");

				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "013456";
				AssertNoMessageErrors("ValidCode", arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo);
			});
		}

		public void TestCheckBY_CusC4Number()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "CUS Codes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000001", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "11000001");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000002", "CUSCode 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "21000002");
			Factory.Save();

			CombineAssertions(() =>
			{
				var arrivalCargoDesc = Factory.New<NctsArrivalCargoDesc>();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_CusC4Number = "99999999";
				AssertHasMessageError(arrivalCargoDesc.BY_CusC4NumberInfo, "The code you have selected is not in the list.");

				arrivalCargoDesc.BY_HarmonisedTariff = "11000001";
				arrivalCargoDesc.BY_CusC4Number = "01000001";
				AssertNoMessageErrors(arrivalCargoDesc.BY_CusC4NumberInfo);

				arrivalCargoDesc.BY_CusC4Number = "01000002";
				AssertHasMessageError(arrivalCargoDesc.BY_CusC4NumberInfo, "CUS Code does not belong to the commodity code");

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				arrivalCargoDesc.Validation.ValidateBY_CusC4Number();
				AssertNoMessageErrors(arrivalCargoDesc.BY_CusC4NumberInfo);
			});
		}

		public void TestCheckBY_Description()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var bill = header.Bills.AddNew();
				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_Description = "";
				AssertHasMessageError(arrivalCargoDesc.BY_DescriptionInfo, "You have not entered a Goods Description.");

				arrivalCargoDesc.BY_Description = "TEST";
				AssertNoMessageErrors(arrivalCargoDesc.BY_DescriptionInfo);

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				arrivalCargoDesc.BY_Description = "";
				AssertNoMessageErrors("No validation when Unloaded state is not NEW", arrivalCargoDesc.BY_DescriptionInfo);
			});
		}

		public void TestCheckBY_NetWeight()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var bill = header.Bills.AddNew();
				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_NetWeight = 100.0;
				AssertNoMessageErrors(arrivalCargoDesc.BY_NetWeightInfo);

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				arrivalCargoDesc.BY_NetWeight = ZDecimal.Zero;
				AssertNoMessageErrors("No validation when Unloaded state is not NEW", arrivalCargoDesc.BY_NetWeightInfo);
			});
		}

		public void TestCheckBY_GrossWeightUnit()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var bill = header.Bills.AddNew();
				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_GrossWeightUnit = "";
				AssertHasMessageError(arrivalCargoDesc.BY_GrossWeightUnitInfo, "You have not entered a Units.");

				arrivalCargoDesc.BY_GrossWeightUnit = "KG";
				AssertNoMessageErrors(arrivalCargoDesc.BY_GrossWeightUnitInfo);

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				arrivalCargoDesc.BY_GrossWeightUnit = "";
				AssertNoMessageErrors("No validation when Unloaded state is not NEW", arrivalCargoDesc.BY_GrossWeightUnitInfo);
			});
		}

		public void TestCheckBY_NetWeightUnit()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var bill = header.Bills.AddNew();
				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_NetWeightUnit = "";
				AssertHasMessageError(arrivalCargoDesc.BY_NetWeightUnitInfo, "You have not entered a Units.");

				arrivalCargoDesc.BY_NetWeightUnit = "KG";
				AssertNoMessageErrors(arrivalCargoDesc.BY_NetWeightUnitInfo);

				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				arrivalCargoDesc.BY_NetWeightUnit = "";
				AssertNoMessageErrors("No validation when Unloaded state is not NEW", arrivalCargoDesc.BY_NetWeightUnitInfo);
			});
		}

		public void TestValidateDifferenceBetweenDeclaredAndUnloadedValue()
		{
			const string message = "You have not captured any differences for Unloaded Value.";
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "CUS Codes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000001", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "11000001");
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			arrivalCargoDesc.BY_HarmonisedTariff = "1100000100";
			arrivalCargoDesc.BY_CusC4Number = "01000001";
			arrivalCargoDesc.BY_Description = "TEST DESCRIPTION";
			arrivalCargoDesc.BY_GrossWeight = 1000;
			arrivalCargoDesc.BY_GrossWeightUnit = "KG";
			arrivalCargoDesc.BY_NetWeight = 995;
			arrivalCargoDesc.BY_NetWeightUnit = "KG";
			var unloadedCargoDesc = arrivalCargoDesc.UnloadedGoodsItem;
			unloadedCargoDesc.BY_HarmonisedTariff = "11000001";
			unloadedCargoDesc.BY_CusC4Number = "01000001";
			unloadedCargoDesc.BY_Description = "TEST DESCRIPTION";
			unloadedCargoDesc.BY_GrossWeight = 1000;
			unloadedCargoDesc.BY_GrossWeightUnit = "KG";
			unloadedCargoDesc.BY_NetWeight = 995;
			unloadedCargoDesc.BY_NetWeightUnit = "KG";
			var supportingDoc1 = arrivalCargoDesc.SupportingDocuments.AddNew();
			supportingDoc1.CSI_Status = "DEC";
			var additionalDoc1 = arrivalCargoDesc.AdditionalInfos.AddNew();
			additionalDoc1.CSI_Status = "DEC";
			var package1 = arrivalCargoDesc.Packages.AddNew();
			package1.B5_TypeOfDifference = "DEC";

			CombineAssertions(() =>
			{
				arrivalCargoDesc.Validation.ValidateAll();
				AssertHasRowMessageError("No change in Unloaded Goods Item", arrivalCargoDesc, message);

				unloadedCargoDesc.BY_GrossWeight = 1001;
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowMessageError("Change in Gross Weight", arrivalCargoDesc, message);

				unloadedCargoDesc.BY_GrossWeight = 1000;
				unloadedCargoDesc.BY_HarmonisedTariff = "11000002";
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowMessageError("Change in Tariff", arrivalCargoDesc, message);

				unloadedCargoDesc.BY_HarmonisedTariff = "11000001";
				var supportingDoc2 = arrivalCargoDesc.SupportingDocuments.AddNew();
				supportingDoc2.CSI_Status = "NEW";
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowMessageError("New Supporting Doc not DEC", arrivalCargoDesc, message);

				arrivalCargoDesc.SupportingDocuments.Delete(supportingDoc2);
				var additionalDoc2 = arrivalCargoDesc.AdditionalInfos.AddNew();
				additionalDoc2.CSI_Status = "NEW";
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowMessageError("New Additional Doc not DEC", arrivalCargoDesc, message);

				arrivalCargoDesc.AdditionalInfos.Delete(additionalDoc2);
				var package2 = arrivalCargoDesc.Packages.AddNew();
				package2.B5_TypeOfDifference = "NEW";
				arrivalCargoDesc.Validation.ValidateAll();
				AssertNoRowMessageError("New Package not DEC", arrivalCargoDesc, message);
			});
		}

		public void TestCheckBY_Description_ConditionNR0029()
		{
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleNR0029Active);

				const string errorMessage = "[NR0029] If Goods Item Unloaded State is NEW, Goods Description cannot be empty.";

				var arrivalCargoDesc = SetUpSimpleNctsArrivalCargoDesc();

				CombineAssertions(() =>
				{
					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					arrivalCargoDesc.BY_Description = ZString.Empty;
					arrivalCargoDesc.Validation.ValidateBY_Description();
					AssertHasMessageError("If BY_UnloadedState is set to NEW and BY_Description is empty, then the error message NR0029 should be displayed",
						arrivalCargoDesc.BY_DescriptionInfo, errorMessage);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
					arrivalCargoDesc.Validation.ValidateBY_Description();
					AssertNoMessageError("If BY_UnloadedState is set to MIS and BY_Description is empty, then the error message NR0029 should not be displayed",
						arrivalCargoDesc.BY_DescriptionInfo, errorMessage);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					arrivalCargoDesc.BY_Description = "1";
					arrivalCargoDesc.Validation.ValidateBY_Description();
					AssertNoMessageError("If BY_UnloadedState is set to NEW and BY_Description is not empty, then the error message NR0029 should not be displayed",
						arrivalCargoDesc.BY_DescriptionInfo, errorMessage);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
					arrivalCargoDesc.Validation.ValidateBY_Description();
					AssertNoMessageError("If BY_UnloadedState is set to MIS and BY_Description is not empty, then the error message NR0029 should not be displayed",
						arrivalCargoDesc.BY_DescriptionInfo, errorMessage);
				});
			}
		}

		public void TestCheckBY_UnloadedState_ConditionNR0029()
		{
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleNR0029Active);

				var errorMessage = "[NR0029] If Goods Item Unloaded State is NEW, at least one Package is required.";

				var arrivalCargoDesc = SetUpSimpleNctsArrivalCargoDesc();

				CombineAssertions(() =>
				{
					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					arrivalCargoDesc.Validation.ValidateBY_UnloadedState();
					AssertHasMessageError("If BY_UnloadedState is set to NEW and Packages is empty, then the error message NR0029 should be displayed",
						arrivalCargoDesc.BY_UnloadedStateInfo, errorMessage);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
					arrivalCargoDesc.Validation.ValidateBY_UnloadedState();
					AssertNoMessageError("If BY_UnloadedState is set to MIS and Packages is empty, then the error message NR0029 should not be displayed",
						arrivalCargoDesc.BY_UnloadedStateInfo, errorMessage);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
					arrivalCargoDesc.Packages.AddNew();
					arrivalCargoDesc.Validation.ValidateBY_UnloadedState();
					AssertNoMessageError("If BY_UnloadedState is set to NEW and Packages is not empty, then the error message NR0029 should not be displayed",
						arrivalCargoDesc.BY_DescriptionInfo, errorMessage);

					arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
					arrivalCargoDesc.Validation.ValidateBY_UnloadedState();
					AssertNoMessageError("If BY_UnloadedState is set to MIS and Packages is not empty, then the error message NR0029 should not be displayed",
						arrivalCargoDesc.BY_DescriptionInfo, errorMessage);
				});
			}
		}

		NctsArrivalCargoDesc SetUpSimpleNctsArrivalCargoDesc(bool needTestForLiabilityAmount = false)
		{
			var header = Factory.New<NctsHeader>();

			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			if (needTestForLiabilityAmount)
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
			}

			var bill = header.Bills.AddNew();
			return bill.ArrivalGoodsItems.AddNew();
		}

		void AssertFormattedHarminisedTariffNoErrorOnUnloadingState(NctsArrivalCargoDesc arrivalCargoDesc, string unloadedState)
		{
			arrivalCargoDesc.BY_UnloadedState = unloadedState;
			arrivalCargoDesc.BY_FormattedHarmonisedTariff = "";
			AssertNoMessageErrorContaining(arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, MandatoryValidation.YouHaveNotEntered);
			arrivalCargoDesc.BY_FormattedHarmonisedTariff = "11000001";
			AssertNoMessageErrorContaining(arrivalCargoDesc.BY_FormattedHarmonisedTariffInfo, "There is no commodity code starting with");
		}

		public void TestCheckBY_GrossWeight_RuleE1109_1()
		{
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, nctsHeader);
				ruleTestContext.EnableRule(x => x.IsRuleE1109_1Active);

				const string message = "[E1109-1] Entered Gross Weight exceeding the max value supported Inside Transition Period (Total 11 digits with maximum of 3 decimals).";

				var arrivalCargoDesc = SetUpSimpleNctsArrivalCargoDesc();
				var propertyInfo = arrivalCargoDesc.BY_GrossWeightInfo;

				UniversalValidationHelperTest.AssertCheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(propertyInfo, message);

				CombineAssertions(() =>
				{
					using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Today, true))
					{
						ruleTestContext.DisableRule(x => x.IsRuleE1109_1Active);
						propertyInfo.Value = new ZDecimal(12345678901.1);
						AssertNoMessageErrorContaining("Length of Max value is exceeding 11, max length of decimal is not exceeding 3", propertyInfo, message);
						propertyInfo.Value = new ZDecimal(1234567.1234);
						AssertNoMessageErrorContaining("Length of Max value is not exceeding 11, max length of decimal is exceeding 3", propertyInfo, message);
					}
				});
			}
		}

		public void TestCheckBY_NetWeight_RuleE1109_1()
		{
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, nctsHeader);
				ruleTestContext.EnableRule(x => x.IsRuleE1109_1Active);

				const string message = "[E1109-1] Entered Net Weight exceeding the max value supported Inside Transition Period (Total 11 digits with maximum of 3 decimals).";

				var arrivalCargoDesc = SetUpSimpleNctsArrivalCargoDesc();
				var propertyInfo = arrivalCargoDesc.BY_NetWeightInfo;

				UniversalValidationHelperTest.AssertCheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(propertyInfo, message);

				CombineAssertions(() =>
				{
					using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Today, true))
					{
						ruleTestContext.DisableRule(x => x.IsRuleE1109_1Active);
						propertyInfo.Value = new ZDecimal(12345678901.1);
						AssertNoMessageErrorContaining("Length of Max value is exceeding 11, max length of decimal is not exceeding 3", propertyInfo, message);
						propertyInfo.Value = new ZDecimal(1234567.1234);
						AssertNoMessageErrorContaining("Length of Max value is not exceeding 11, max length of decimal is exceeding 3", propertyInfo, message);
					}
				});
			}
		}

		public void TestCheckLiabilityTariffLenght()
		{
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				var arrivalCargoDesc = SetUpSimpleNctsArrivalCargoDesc(true);
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_HarmonisedTariff = "12345678";
				const string message = "Please, for duties calculation, enter a 10-digits Tariff Code";
				CombineAssertions(() =>
				{
					AssertHasWarning("Less than 10 digits", arrivalCargoDesc.LiabilityTariffInfo, message);

					arrivalCargoDesc.LiabilityTariff = "1234567890";
					AssertNoWarning("10 digits", arrivalCargoDesc.LiabilityTariffInfo, message);

					arrivalCargoDesc.LiabilityTariff = "12345678901";
					AssertNoWarning("More than 10 digits", arrivalCargoDesc.LiabilityTariffInfo, message);
				});
			}
		}

		public void TestCheckLiabilityListValidation()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var lvTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Latvia, Constants.TariffTypes.Export).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Latvia, lvTariffTypePK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				var arrivalCargoDesc = SetUpSimpleNctsArrivalCargoDesc(true);
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				arrivalCargoDesc.BY_HarmonisedTariff = "0987654321";
				var messageError = "The code you have selected is not in the list.";
				CombineAssertions(() =>
				{
					AssertHasMessageErrorContaining("Tariff is not valid", arrivalCargoDesc.LiabilityTariffInfo, messageError);

					arrivalCargoDesc.LiabilityTariff = "1234567890";
					AssertNoMessageErrorContaining("Tariff is valid", arrivalCargoDesc.LiabilityTariffInfo, messageError);
				});
			}
		}

		public void TestCheckBY_RN_NKCountryOfOrigin()
		{
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, nctsHeader);
				NCTSTestHelper.AssertCheckBY_RN_NKCountryOfOriginIsValid(goodsItem);
			}
		}

		public void TestCheckBY_CustomsSecondUnitQty()
		{
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, nctsHeader);
				NCTSTestHelper.TestCheckBY_CustomsSecondUnitQtyIsValid(Factory, goodsItem);
			}
		}

		public void TestCheckBY_CustomsSecondQuantity()
		{
			NCTSTestHelper.AssertCheckBY_CustomsSecondQuantity_RuleTR0084(Factory, goodsItem);
		}

		public void TestCheckBY_CustomsThirdQuantity()
		{
			NCTSTestHelper.AssertCheckBY_CustomsThirdQuantity_RuleTR0084(Factory, goodsItem);
		}

		public void TestCheckBY_CustomsFourthQuantity()
		{
			NCTSTestHelper.AssertCheckBY_CustomsFourthQuantity_RuleTR0084(Factory, goodsItem);
		}

		public void TestCheckBY_RN_NKCountryOfOriginNR0058()
		{
			NCTSTestHelper.AssertCheckBY_RN_NKCountryOfOriginNR0058(Factory, goodsItem, validationRuleConfiguration, new ArrivalCargoDescValidationDeciderTestContextForLiabilityCalculation(Factory));
		}

		public void TestCheckBY_MonetaryValueNR0059()
		{
			NCTSTestHelper.AssertCheckBY_MonetaryValueNR0059(Factory, goodsItem, validationRuleConfiguration, new ArrivalCargoDescValidationDeciderTestContextForLiabilityCalculation(Factory));
		}

		public void TestCheckBY_SupplementsNR0060()
		{
			var goodsItemArrival = NCTSTestHelper.GetArrivalCargoDescTest(Factory);
			NCTSTestHelper.AssertCheckBY_SupplementsNR0060(Factory, goodsItemArrival, validationRuleConfiguration, new ArrivalCargoDescValidationDeciderTestContextForLiabilityCalculation(Factory));
		}

		public void TestCheckBY_Supplements()
		{
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, nctsHeader);
				NCTSTestHelper.AssertCheckBY_Supplements_HasChildValidationNotification(goodsItem);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		}
		NctsHeader nctsHeader;
		NctsArrivalCargoDesc goodsItem;

		readonly ValidationRuleConfiguration validationRuleConfiguration = new ValidationRuleConfiguration();
	}
}
