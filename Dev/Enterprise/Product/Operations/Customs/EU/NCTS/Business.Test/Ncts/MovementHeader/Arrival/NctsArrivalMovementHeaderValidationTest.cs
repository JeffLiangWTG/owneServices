using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsArrivalMovementHeaderValidationTest : NctsCommonMovementHeaderValidationAbstractTest<INctsArrivalMovementHeaderPhase5ValidationDecider>
{
	public void TestCheckBM_CarnetTotalPages_TR0042() => CombineAssertions(() =>
	{
		var message = "[TR0042] Total Page Number and Discharge TIR must be filled, or both must be empty.";
		testContext.EnableRule(x => x.IsRuleTR0042Active);
		arrivalMovement.Validation.ValidateBM_CarnetTotalPages();
		AssertNoError("Both Discharge and Page Number fields are empty", arrivalMovement.BM_CarnetTotalPagesInfo, message);

		arrivalMovement.BM_CarnetTotalPages = 0;
		AssertNoError("Both Discharge and Page Number fields are empty (0 = empty test)", arrivalMovement.BM_CarnetTotalPagesInfo, message);

		arrivalMovement.BM_CarnetTotalPages = 2;
		AssertHasError("Page Number is filled so an error message is expected", arrivalMovement.BM_CarnetTotalPagesInfo, message);

		arrivalMovement.BM_DischargeType = "FD";
		arrivalMovement.BM_CarnetTotalPages = 0;
		AssertHasError("Discharge is filled so an error message is expected", arrivalMovement.BM_CarnetTotalPagesInfo, message);

		testContext.DisableRule(x => x.IsRuleTR0042Active);
		arrivalMovement.BM_CarnetTotalPages = 0;
		AssertNoError("Rule is disabled", arrivalMovement.BM_CarnetTotalPagesInfo, message);

		testContext.EnableRule(x => x.IsRuleTR0042Active);
		arrivalMovement.BM_CarnetTotalPages = 2;
		AssertNoError("Both fields are filled", arrivalMovement.BM_CarnetTotalPagesInfo, message);
	});

	public void TestCheckBM_DischargeType_TR0042() => CombineAssertions(() =>
	{
		var message = "[TR0042] Total Page Number and Discharge TIR must be filled, or both must be empty.";
		testContext.EnableRule(x => x.IsRuleTR0042Active);
		arrivalMovement.Validation.ValidateBM_DischargeType();
		AssertNoError("Both Discharge and Page Number fields are empty", arrivalMovement.BM_DischargeTypeInfo, message);

		arrivalMovement.BM_CarnetTotalPages = 0;
		arrivalMovement.Validation.ValidateBM_DischargeType();
		AssertNoError("Both Discharge and Page Number fields are empty (0 = empty test)", arrivalMovement.BM_DischargeTypeInfo, message);

		arrivalMovement.BM_DischargeType = "FD";
		AssertHasError("Discharge is filled so an error message is expected", arrivalMovement.BM_DischargeTypeInfo, message);

		arrivalMovement.BM_CarnetTotalPages = 2;
		arrivalMovement.BM_DischargeType = ZString.Empty;
		AssertHasError("Total Pages is filled so an error message is expected", arrivalMovement.BM_DischargeTypeInfo, message);

		testContext.DisableRule(x => x.IsRuleTR0042Active);
		arrivalMovement.Validation.ValidateBM_DischargeType();
		AssertNoError("Rule is disabled", arrivalMovement.BM_DischargeTypeInfo, message);

		testContext.EnableRule(x => x.IsRuleTR0042Active);
		arrivalMovement.BM_DischargeType = "FD";
		AssertNoError("Both fields are filled", arrivalMovement.BM_DischargeTypeInfo, message);
	});

	public void TestArrivalDate_TR0072() => CombineAssertions(() =>
	{
		var messageError = "[TR0072] " + MandatoryValidation.YouHaveNotEnteredMessage(arrivalMovement.BM_ArrivalDateInfo.Description);

		testContext.EnableRule(x => x.IsRuleTR0072Active);
		arrivalMovement.BM_MessageStatus = NctsMessageStatusList.Codes.Unknown;
		arrivalMovement.Validation.ValidateAll();
		AssertHasMessageError("Is phase 5, validation is enabled and TR0072 is enabled", arrivalMovement.BM_ArrivalDateInfo, messageError);

		arrivalMovement.BM_ArrivalDate = ZDateTime.Now;
		AssertNoMessageError("Is phase 5, validation is enabled and TR0072 is enabled (but property is filled)", arrivalMovement.BM_ArrivalDateInfo, messageError);

		testContext.DisableRule(x => x.IsRuleTR0072Active);
		arrivalMovement.BM_MessageStatus = NctsMessageStatusList.Codes.Unknown;
		arrivalMovement.BM_ArrivalDate = ZDateTime.Empty;
		arrivalMovement.Validation.ValidateAll();
		AssertNoMessageError("Is phase 5, validation is enabled and TR0072 is disabled", arrivalMovement.BM_ArrivalDateInfo, messageError);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		arrivalMovement.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
		arrivalMovement.Validation.ValidateAll();
		AssertNoMessageError("Not phase 5 and validation isn't enabled", arrivalMovement.BM_ArrivalDateInfo, messageError);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		arrivalMovement.Validation.ValidateAll();
		AssertNoMessageError("Is phase 5 but validation isn't enabled", arrivalMovement.BM_ArrivalDateInfo, messageError);
	});

	public void TestArrivalDate_TR0022() => CombineAssertions(() =>
	{
		const string message = "[TR0022] Arrival Notification Date/Time must be in the past.";
		var yesterday = ZDateTime.Now.AddDays(-1);
		var tomorrow = ZDateTime.Now.AddDays(1);

		testContext.EnableRule(x => x.IsRuleTR0022Active);
		arrivalMovement.BM_ArrivalDate = ZDateTime.Empty;
		AssertNoMessageError("date is empty", arrivalMovement.BM_ArrivalDateInfo, message);
		arrivalMovement.BM_ArrivalDate = yesterday;
		AssertNoMessageError("date is in the past", arrivalMovement.BM_ArrivalDateInfo, message);
		arrivalMovement.BM_ArrivalDate = tomorrow;
		AssertHasMessageError("date is in the future", arrivalMovement.BM_ArrivalDateInfo, message);
		testContext.DisableRule(x => x.IsRuleTR0022Active);
		arrivalMovement.Validation.ValidateAll();
		AssertNoMessageError("date is in the future, but rule is not active", arrivalMovement.BM_ArrivalDateInfo, message);
	});

	public void TestAuthorizationCode() => CombineAssertions(() =>
	{
		const string message = MandatoryValidation.MustBeEntered;
		arrivalMovement.AuthorizationNumber = "Test";

		arrivalMovement.AuthorizationCode = "ACE";
		AssertNoErrorContaining("has value", arrivalMovement.AuthorizationCodeInfo, message);
		arrivalMovement.AuthorizationCode = string.Empty;
		AssertHasErrorContaining("empty", arrivalMovement.AuthorizationCodeInfo, message);
		arrivalMovement.AuthorizationNumber = string.Empty;
		arrivalMovement.Validation.ValidateAuthorizationCode();
		AssertNoErrorContaining("no value, but no authorization needed", arrivalMovement.AuthorizationCodeInfo, message);
	});

	public void TestAuthorizationNumber() => CombineAssertions(() =>
	{
		const string message = MandatoryValidation.MustBeEntered;
		arrivalMovement.AuthorizationCode = "ACE";

		arrivalMovement.AuthorizationNumber = "Test";
		AssertNoErrorContaining("has value", arrivalMovement.AuthorizationNumberInfo, message);
		arrivalMovement.AuthorizationNumber = string.Empty;
		AssertHasErrorContaining("empty", arrivalMovement.AuthorizationNumberInfo, message);
		arrivalMovement.AuthorizationCode = string.Empty;
		arrivalMovement.Validation.ValidateAuthorizationNumber();
		AssertNoErrorContaining("no value, but no authorization needed", arrivalMovement.AuthorizationNumberInfo, message);

		var orgHeader = Factory.New<MasterFiles.Business.OrgHeader>();
		orgHeader.OH_Code = "ABC";
		arrivalMovement.AuthorizationCode = "ACE";
		arrivalMovement.AuthorizationOwner = orgHeader.PK;
		arrivalMovement.AuthorizationNumber = "123";
		AssertHasWarning("Validate combination of authorization code, number and owner", arrivalMovement.AuthorizationNumberInfo, "Authorization number: 123 doesn't exist for Code: ACE, Owner: ABC");
	});

	public void TestCheckGoodsLocationDescription()
	{
		const string message = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";
		var cusGoodsLocation = arrivalMovement.GoodsLocation;
		cusGoodsLocation.CGL_Qualifier = "Y";
		arrivalMovement.Validation.ValidateGoodsLocationDescription();

		CombineAssertions(() =>
		{
			AssertHasMessageError("There should be an error on GoodsLocationDescription when the linked GoodsLocation has errors.", arrivalMovement.GoodsLocationDescriptionInfo, message);

			cusGoodsLocation.Address.E2_GovRegNum = "ABC";
			arrivalMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("There should be no error on GoodsLocationDescription when the linked GoodsLocation doesn't have errors.", arrivalMovement.GoodsLocationDescriptionInfo, message);

			using (cusGoodsLocation.GetValidationSuspender())
			{
				cusGoodsLocation.Address.E2_Email = "A";
			}
			arrivalMovement.Validation.ValidateGoodsLocationDescription();
			AssertHasError("ValidateGoodsLocationDescription triggers validation on GoodsLocation and GoodsLocationAddress", arrivalMovement.GoodsLocationDescriptionInfo, message);
		});
	}

	public void TestCheckGoodsLocationDescriptionWhenArrivalDetailsAreReadOnly()
	{
		var cusGoodsLocation = arrivalMovement.GoodsLocation;
		cusGoodsLocation.CGL_LocationUse = "ARR";
		cusGoodsLocation.CGL_Qualifier = "Y";
		arrivalMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;

		arrivalMovement.Validation.ValidateGoodsLocationDescription();

		CombineAssertions(() =>
		{
			AssertNoNotifications("The linked GoodsLocation has errors.", arrivalMovement.GoodsLocationDescriptionInfo);

			cusGoodsLocation.Address.E2_GovRegNum = "ABC";
			arrivalMovement.Validation.ValidateGoodsLocationDescription();
			AssertNoNotifications("The linked GoodsLocation doesn't have errors.", arrivalMovement.GoodsLocationDescriptionInfo);
		});
	}

	public void TestCheckRuleNR0009() => CombineAssertions(() =>
	{
		const string errorMessage = "[NR0009]";
		testContext.EnableRule(x => x.IsRuleNR0009Active);

		arrivalMovement.BM_StateOfSealsBoolean = true;

		AssertNoMessageErrorContaining("No Containers", arrivalMovement.BM_StateOfSealsBooleanInfo, errorMessage);

		arrivalMovement.BM_StateOfSealsBoolean = false;

		AssertHasMessageErrorContaining("No Containers", arrivalMovement.BM_StateOfSealsBooleanInfo, errorMessage);
		nctsHeader.ArrivalHeaderContainers.AddNew();
		arrivalMovement.BM_StateOfSealsBoolean = false;

		AssertNoMessageErrorContaining("Has Containers", arrivalMovement.BM_StateOfSealsBooleanInfo, errorMessage);

		testContext.DisableRule(x => x.IsRuleNR0009Active);
		nctsHeader.ArrivalHeaderContainers.RemoveAll();
		arrivalMovement.BM_StateOfSealsBoolean = false;

		AssertNoMessageErrorContaining("Rule is disabled", arrivalMovement.BM_StateOfSealsBooleanInfo,
										errorMessage);
	});

	public void TestCheckRuleNR0028() => CombineAssertions(() =>
	{
		const string errorMessage = "[NR0028] If the 'Seals State Valid' is N then the 'Unloaded cargo conforms to declaration' may not be selected.";

		testContext.EnableRule(x => x.IsRuleNR0028Active);

		arrivalMovement.BM_StateOfSeals = YesNoList.Codes.Yes;
		arrivalMovement.BM_NoChangesToReport = ZBool.False;
		AssertNoMessageError(AssertionMessage(), arrivalMovement.BM_NoChangesToReportInfo, errorMessage);

		arrivalMovement.BM_NoChangesToReport = ZBool.True;
		AssertNoMessageError(AssertionMessage(), arrivalMovement.BM_NoChangesToReportInfo, errorMessage);

		arrivalMovement.BM_StateOfSeals = ZString.Empty;
		arrivalMovement.BM_NoChangesToReport = ZBool.False;
		AssertNoMessageError(AssertionMessage(), arrivalMovement.BM_NoChangesToReportInfo, errorMessage);

		arrivalMovement.BM_NoChangesToReport = ZBool.True;
		AssertNoMessageError(AssertionMessage(), arrivalMovement.BM_NoChangesToReportInfo, errorMessage);

		arrivalMovement.BM_StateOfSeals = YesNoList.Codes.No;
		arrivalMovement.BM_NoChangesToReport = ZBool.False;
		AssertNoMessageError(AssertionMessage(), arrivalMovement.BM_NoChangesToReportInfo, errorMessage);

		arrivalMovement.BM_NoChangesToReport = ZBool.True;
		AssertHasMessageError(AssertionMessage(), arrivalMovement.BM_NoChangesToReportInfo, errorMessage);

		testContext.DisableRule(x => x.IsRuleNR0028Active);
		arrivalMovement.Validation.ValidateBM_NoChangesToReport();
		AssertNoMessageError("NR0028 inactive", arrivalMovement.BM_NoChangesToReportInfo, errorMessage);
		return;

		string AssertionMessage() => $"BM_StateOfSeals={arrivalMovement.BM_StateOfSeals}";
	});

	public void TestCheckRuleTR0034()
	{
		const string message = "[TR0034] This field must have a value and the value must be longer than 4 characters.";
		testContext.EnableRule(x => x.IsRuleTR0034Active);

		arrivalMovement.BM_PaperlessInbondNum = "1234";
		AssertHasMessageError(arrivalMovement.BM_PaperlessInbondNumInfo, message);
		arrivalMovement.BM_PaperlessInbondNum = "12345";
		AssertNoMessageError(arrivalMovement.BM_PaperlessInbondNumInfo, message);

		testContext.DisableRule(x => x.IsRuleTR0034Active);
		arrivalMovement.BM_PaperlessInbondNum = "1234";
		AssertNoMessageError(arrivalMovement.BM_PaperlessInbondNumInfo, message);
	}

	public void TestCheckRuleTR0098()
	{
		testContext.EnableRule(x => x.IsRuleTR0098Active);

		arrivalMovement.BM_PaperlessInbondNum = ZString.Empty;
		AssertHasMessageErrorContaining(arrivalMovement.BM_PaperlessInbondNumInfo, MandatoryValidation.YouHaveNotEntered);
		arrivalMovement.BM_PaperlessInbondNum = "12345";
		AssertNoMessageErrorContaining(arrivalMovement.BM_PaperlessInbondNumInfo, MandatoryValidation.YouHaveNotEntered);

		testContext.DisableRule(x => x.IsRuleTR0098Active);
		arrivalMovement.BM_PaperlessInbondNum = ZString.Empty;
		AssertNoMessageErrorContaining(arrivalMovement.BM_PaperlessInbondNumInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBM_PaperlessInbondNum_EnglishCharactersValidation()
	{
		arrivalMovement.BM_PaperlessInbondNum = "【】";
		AssertHasErrorContaining(arrivalMovement.BM_PaperlessInbondNumInfo, "accepts Western European languages characters");
	}

	public void TestCheckBM_NoChangesToReport_ConditionNR0026() => CombineAssertions(() =>
	{
		var validationRuleMessages = nctsHeader.Configuration.ValidationRuleConfiguration.Messages;
		testContext.EnableRule(x => x.IsRuleNR0026Active);

		arrivalMovement.BM_NoChangesToReport = true;
		arrivalMovement.BM_StateOfSealsBoolean = false;
		arrivalMovement.Validation.ValidateBM_NoChangesToReport();
		AssertHasMessageErrorContaining("With BM_NoChangesToReport set to true and BM_StateOfSeals not being Yes, BM_NoChangesToReport should return the error message NR0026",
										arrivalMovement.BM_NoChangesToReportInfo, validationRuleMessages.NR0026Message);

		arrivalMovement.BM_StateOfSealsBoolean = true;
		arrivalMovement.Validation.ValidateBM_NoChangesToReport();
		AssertNoMessageErrorContaining("With BM_NoChangesToReport set to true and BM_StateOfSeals being Yes, BM_NoChangesToReport should not have an error message",
										arrivalMovement.BM_NoChangesToReportInfo, validationRuleMessages.NR0026Message);

		arrivalMovement.BM_StateOfSeals = ZString.Empty;
		arrivalMovement.Validation.ValidateBM_NoChangesToReport();
		AssertNoMessageErrorContaining("With BM_NoChangesToReport set to true and BM_StateOfSeals being empty, BM_NoChangesToReport should not have an error message",
										arrivalMovement.BM_NoChangesToReportInfo, validationRuleMessages.NR0026Message);

		arrivalMovement.BM_NoChangesToReport = false;
		arrivalMovement.Validation.ValidateBM_NoChangesToReport();
		AssertNoMessageErrorContaining("With BM_NoChangesToReport set to false and BM_StateOfSeals being empty, BM_NoChangesToReport should not have an error message",
										arrivalMovement.BM_NoChangesToReportInfo, validationRuleMessages.NR0026Message);

		arrivalMovement.BM_StateOfSealsBoolean = false;
		arrivalMovement.Validation.ValidateBM_NoChangesToReport();
		AssertNoMessageErrorContaining("With BM_NoChangesToReport set to false and BM_StateOfSeals being No, BM_NoChangesToReport should not have an error message",
										arrivalMovement.BM_NoChangesToReportInfo, validationRuleMessages.NR0026Message);
	});

	public void TestCheckBM_NoChangesToReport_TR0063() => CombineAssertions(() =>
	{
		const string message = @"[TR0063] If ""Unloaded cargo conforms to declaration"" is set to false, at least one Goods Item or one Seal must be set to 'NEW' or 'MIS' or 'DIF'.";

		var bill1 = nctsHeader.Bills.AddNew();
		var bill2 = nctsHeader.Bills.AddNew();
		var goodsitem11 = bill1.ArrivalGoodsItems.AddNew();
		var goodsitem12 = bill1.ArrivalGoodsItems.AddNew();
		var goodsitem21 = bill2.ArrivalGoodsItems.AddNew();

		AssertError("No differences", false);
		AssertError("No differences", true, customsStatus: NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted);
		AssertError("No differences", true, customsStatus: NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);

		AssertError("NoChangesToReport checked", false, noChangesToReport: true);
		AssertError("No container", true, withContainer: false, customsStatus: NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);
		AssertError("Seal NEW", false, sealUnloadedState: NctsUnloadedStateList.Codes.NEW);
		AssertError("Seal MIS", false, sealUnloadedState: NctsUnloadedStateList.Codes.MIS);
		AssertError("Seal DIF", false, sealUnloadedState: NctsUnloadedStateList.Codes.DIF);
		AssertError("Seal other than NEW,MIS,DIF", true, sealUnloadedState: NctsUnloadedStateList.Codes.DEC, customsStatus: NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);
		AssertError("State at 1st bill NEW", false, billUnloadedStates: new[] { NctsUnloadedStateList.Codes.NEW, null });
		AssertError("State at 2nd bill MIS", false, billUnloadedStates: new[] { null, NctsUnloadedStateList.Codes.MIS });
		AssertError("State at 2nd bill DIF", false, billUnloadedStates: new[] { null, NctsUnloadedStateList.Codes.DIF });
		AssertError("State at 1st goods item", false, goodsItemUnloadedStates: new[] { NctsUnloadedStateList.Codes.NEW, null, null });
		AssertError("State as 2nd goods item", false, goodsItemUnloadedStates: new[] { null, NctsUnloadedStateList.Codes.MIS, null });
		AssertError("State at 2nd bill", false, goodsItemUnloadedStates: new[] { null, null, NctsUnloadedStateList.Codes.DIF });

		void AssertError(string assertionMessage, bool messageExpected, bool noChangesToReport = false, bool withContainer = true, string sealUnloadedState = null, string[] billUnloadedStates = null, string[] goodsItemUnloadedStates = null, string customsStatus = "")
		{
			testContext.EnableRule(x => x.IsRuleTR0063Active);

			nctsHeader.ArrivalHeaderContainers.RemoveAll();
			if (withContainer)
			{
				var container = nctsHeader.ArrivalHeaderContainers.AddNew();
				if (sealUnloadedState != null)
				{
					container.Seals.AddNew().BK_UnloadingState = sealUnloadedState;
				}
			}

			bill1.MovementDetail.B9_UnloadedState = billUnloadedStates?[0] ?? ZString.Empty;
			bill2.MovementDetail.B9_UnloadedState = billUnloadedStates?[1] ?? ZString.Empty;

			goodsitem11.BY_UnloadedState = goodsItemUnloadedStates?[0] ?? ZString.Empty;
			goodsitem12.BY_UnloadedState = goodsItemUnloadedStates?[1] ?? ZString.Empty;
			goodsitem21.BY_UnloadedState = goodsItemUnloadedStates?[2] ?? ZString.Empty;
			arrivalMovement.BM_CustomsStatus = customsStatus;
			arrivalMovement.BM_NoChangesToReport = noChangesToReport;
			if (messageExpected)
			{
				AssertHasMessageError($"{assertionMessage} - TR0063 enabled", arrivalMovement.BM_NoChangesToReportInfo, message);
			}
			else
			{
				AssertNoMessageError($"{assertionMessage} - TR0063 enabled", arrivalMovement.BM_NoChangesToReportInfo, message);
			}

			testContext.DisableRule(x => x.IsRuleTR0063Active);
			arrivalMovement.Validation.ValidateBM_NoChangesToReport();
			AssertNoMessageError($"{assertionMessage} - TR0063 disabled", arrivalMovement.BM_NoChangesToReportInfo, message);
		}
	});

	public void TestCheckBM_GONumber_TR0071()
	{
		var messageError = $"[TR0071] If Authorization Nº is filled (I.e. private location), Simplified Procedure must be ticked.";
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		arrivalMovement.AuthorizationNumber = "A";
		arrivalMovement.IsSimplifiedNctsProcedure = ZBool.False;
		CombineAssertions(() =>
		{
			testContext.DisableRule(x => x.IsRuleTR0071Active);
			arrivalMovement.Validation.ValidateBM_GONumber();
			AssertNoMessageErrorContaining("Arrival disable Rule TR0071 expected no message error", arrivalMovement.IsSimplifiedNctsProcedureInfo, messageError);

			testContext.EnableRule(x => x.IsRuleTR0071Active);
			arrivalMovement.Validation.ValidateBM_GONumber();
			AssertHasMessageErrorContaining("Arrival Phase5 Rule TR0071, AuthorizationNumber not empty and AutomaticCompletion false expected message error", arrivalMovement.IsSimplifiedNctsProcedureInfo, messageError);

			arrivalMovement.AuthorizationNumber = ZString.Empty;
			arrivalMovement.Validation.ValidateBM_GONumber();
			AssertNoMessageErrorContaining("Arrival Phase5 Rule TR0071, AuthorizationNumber empty and AutomaticCompletion false expected no message error", arrivalMovement.IsSimplifiedNctsProcedureInfo, messageError);

			arrivalMovement.AuthorizationNumber = "A";
			arrivalMovement.IsSimplifiedNctsProcedure = ZBool.True;
			AssertNoMessageErrorContaining("Arrival Phase5 Rule TR0071, AuthorizationNumber not empty and AutomaticCompletion true expected no message error", arrivalMovement.IsSimplifiedNctsProcedureInfo, messageError);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrivalMovement.Validation.ValidateBM_GONumber();
			AssertNoMessageErrorContaining("Arrival not Phase5 Rule TR0071 expected no message error", arrivalMovement.IsSimplifiedNctsProcedureInfo, messageError);
		});
	}

	public void TestCheckDestinationCustomsOfficeCodeForArrival_NR0010()
	{
		CombineAssertions(() =>
		{
			using (var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleNR0010Active));

				const string messageError = "[NR0010] Destination Office must be equal to the Goods Location Customs Office.";
				var goodsLocation = arrivalMovement.GoodsLocation;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				goodsLocation.CGL_CustomsOffice = "BE000001";
				arrivalMovement.DestinationCustomsOfficeCodeForArrival = "BE000002";

				arrivalMovement.Validation.ValidateDestinationCustomsOfficeCodeForArrival();
				AssertHasMessageError("Different Offices", arrivalMovement.DestinationCustomsOfficeCodeForArrivalInfo, messageError);
				arrivalMovement.DestinationCustomsOfficeCodeForArrival = "BE000001";
				AssertNoMessageError("Offices are the same", arrivalMovement.DestinationCustomsOfficeCodeForArrivalInfo, messageError);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				arrivalMovement.DestinationCustomsOfficeCodeForArrival = "BE000002";
				AssertNoMessageError("CGL_Qualifier is not V", arrivalMovement.DestinationCustomsOfficeCodeForArrivalInfo, messageError);

				testContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleNR0010Active));
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				arrivalMovement.DestinationCustomsOfficeCodeForArrival = "BE000002";
				AssertNoMessageError("Different Offices, but rule NR0010 is not active", arrivalMovement.DestinationCustomsOfficeCodeForArrivalInfo, messageError);
			}
		});
	}

	public void TestCheckDestinationCustomsOfficeCodeForArrival()
	{
		CombineAssertions(() =>
		{
			arrivalMovement.Validation.ValidateDestinationCustomsOfficeCodeForArrival();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(arrivalMovement.DestinationCustomsOfficeCodeForArrivalInfo);

			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(arrivalMovement.DestinationCustomsOfficeCodeForArrivalInfo);
		});
	}

	public void TestBM_GrossWeightUnloaded_TR0091()
	{
		const string messageError = "[TR0091] Total unloaded Gross Weight must be equal or higher than sum of Gross Weights in all House Consignments.";

		var billDIF = nctsHeader.Bills.AddNew();
		billDIF.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		billDIF.B0_GrossWeightUnloaded = 10;
		billDIF.B0_WeightUQ = "KG";
		var billDEC = nctsHeader.Bills.AddNew();
		billDEC.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		billDEC.B0_Weight = 3;
		billDEC.B0_WeightUQ = "KG";
		var billNEW = nctsHeader.Bills.AddNew();
		billNEW.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		billNEW.B0_Weight = 2;
		billNEW.B0_WeightUQ = "KG";
		var billMIS = nctsHeader.Bills.AddNew();
		billMIS.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		billMIS.B0_Weight = 100;
		billMIS.B0_GrossWeightUnloaded = 100;
		billMIS.B0_WeightUQ = "KG";

		CombineAssertions(() =>
		{
			testContext.EnableRule(x => x.IsRuleTR0091Active);
			arrivalMovement.BM_NoChangesToReport = false;
			arrivalMovement.BM_GrossWeightUnloaded = 14;
			AssertHasMessageError("Rule active; BM_NoChangesToReport false; BM_GrossWeightUnloaded less than sum of GrossWeights (15KG)", arrivalMovement.BM_GrossWeightUnloadedInfo, messageError);

			arrivalMovement.BM_GrossWeightUnloaded = 15;
			AssertNoMessageError("Rule active; BM_NoChangesToReport false; BM_GrossWeightUnloaded not less than sum of GrossWeights (15KG)", arrivalMovement.BM_GrossWeightUnloadedInfo, messageError);

			arrivalMovement.BM_NoChangesToReport = true;
			arrivalMovement.BM_GrossWeightUnloaded = 14;
			AssertNoMessageError("Rule active; BM_NoChangesToReport true; BM_GrossWeightUnloaded less than sum of GrossWeights (15KG)", arrivalMovement.BM_GrossWeightUnloadedInfo, messageError);

			testContext.DisableRule(x => x.IsRuleTR0091Active);
			arrivalMovement.BM_NoChangesToReport = false;
			arrivalMovement.Validation.ValidateBM_GrossWeightUnloaded();
			AssertNoMessageError("Rule not active; BM_NoChangesToReport false; BM_GrossWeightUnloaded less than sum of GrossWeights (15KG)", arrivalMovement.BM_GrossWeightUnloadedInfo, messageError);
		});
	}

	public void TestBM_UnloadingDate_NR0076()
	{
		const string messageWarning = "[NR0076] Unloading date should be equal to or later than the Reception Date";
		var testDate = new ZDateTime(2024, 10, 1, 11, 50, 15);

		var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
		entryNum.CE_EntryNum = "21ES00999912345678";
		entryNum.CE_IssueDate = testDate;

		CombineAssertions(() =>
		{
			testContext.EnableRule(x => x.IsRuleNR0076Active);
			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = testDate.ToOffset();
			AssertNoWarning("Rule active; BM_UnloadingDate and CE_IssueDate of MovementReferenceNumber are equal", arrivalMovement.BM_UnloadingDateInfo, messageWarning);

			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = testDate.AddDays(-1).ToOffset();
			AssertHasWarning("Rule active; BM_UnloadingDate are lower than CE_IssueDate of MovementReferenceNumber", arrivalMovement.BM_UnloadingDateInfo, messageWarning);

			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = testDate.AddDays(1).ToOffset();
			AssertNoWarning("Rule active; BM_UnloadingDate are bigger than CE_IssueDate of MovementReferenceNumber", arrivalMovement.BM_UnloadingDateInfo, messageWarning);

			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = testDate.AddDays(-1).ToOffset();
			testContext.DisableRule(x => x.IsRuleNR0076Active);
			arrivalMovement.Validation.ValidateBM_UnloadingDate();
			AssertNoWarning("Rule not active; BM_UnloadingDate are lower than CE_IssueDate of MovementReferenceNumber", arrivalMovement.BM_UnloadingDateInfo, messageWarning);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovement = nctsHeader.ArrivalMovementHeader;
		testContext.AddAutoCacheResetObject(nctsHeader);
		testContext.AddAutoCacheResetObject(arrivalMovement);
	}

	NctsHeader nctsHeader;
	NctsArrivalMovementHeader arrivalMovement;

	protected override NctsCommonMovementHeader GetMovementHeaderForTest() => arrivalMovement;
}
