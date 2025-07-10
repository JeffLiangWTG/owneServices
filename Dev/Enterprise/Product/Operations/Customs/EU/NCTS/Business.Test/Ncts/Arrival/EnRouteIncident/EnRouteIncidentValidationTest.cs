using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class EnRouteIncidentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBN_Information()
		{
			CombineAssertions(() =>
			{
				incident.Validation.ValidateBN_Information();
				AssertHasMessageErrorContaining("Incident false is mandatory", incident.BN_InformationInfo, MandatoryValidation.YouHaveNotEntered);

				incident.BN_Information = "123";
				AssertNoMessageErrorContaining("Incident false info entered", incident.BN_InformationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckBN_EventPlace()
		{
			incident.BN_EventPlace = ZString.Empty;
			AssertHasMessageErrorContaining(incident.BN_EventPlaceInfo, MandatoryValidation.YouHaveNotEntered);

			incident.BN_EventPlace = "ABC";
			AssertNoMessageErrorContaining(incident.BN_EventPlaceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBN_EventCountryCode()
		{
			var eventCountryCodeinfo = incident.BN_EventCountryCodeInfo;
			CombineAssertions(() =>
			{
				incident.BN_EventCountryCode = ZString.Empty;
				AssertHasMessageErrorContaining("Mandatory", eventCountryCodeinfo, MandatoryValidation.YouHaveNotEntered);

				incident.BN_EventCountryCode = Core.Constants.CountryCodes.Bangladesh;
				AssertNoMessageErrorContaining("Entered", eventCountryCodeinfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError("Invalid Code", eventCountryCodeinfo, ListValidation.InvalidCodeMessageError);

				incident.BN_EventCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNoMessageError("Valid Code", eventCountryCodeinfo, ListValidation.InvalidCodeMessageError);

				incident.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				incident.BN_EventCountryCode = "";
				AssertNoMessageError("Mandatory", eventCountryCodeinfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckBN_IncidentCode()
		{
			var error = $"The Incident Code you have selected is not in the list: {incident.Lookups.IncidentCodeList.CodesAsString}.";

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfInvalidCode(incident.BN_IncidentCodeInfo, "X", IncidentCodeList.Codes._1, error);

				incident.BN_IncidentCode = string.Empty;
				AssertNoError("BN_IncidentCode is empty", incident.BN_IncidentCodeInfo, error);
			});
		}

		public void TestCheckBN_IncidentCode_MandatoryRuleTR0010()
		{
			const string messageError = "[TR0010] You have not entered an Incident Code.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					var incident = CreateIncidentWithPhase5Header();

					deciderTestContext.EnableRule(c => c.IsRuleTR0010Active);

					incident.BN_IncidentCode = IncidentCodeList.Codes._1;

					AssertNoMessageError("BN_IncidentCode is not empty", incident.BN_IncidentCodeInfo, messageError);

					incident.BN_IncidentCode = ZString.Empty;

					AssertHasMessageError("Rule Enabled and BN_IncidentCode is empty", incident.BN_IncidentCodeInfo, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleTR0010Active);

					incident.Validation.ValidateBN_IncidentCode();
					AssertNoMessageError("Rule disabled", incident.BN_IncidentCodeInfo, messageError);
				}
			});
		}

		public void TestCheckBN_TransportAtDepartureType_C0240_1Rule()
		{
			var message = "[C0240-1] You have not entered a Type of ID.";
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleC0240_1Active);
					incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._3;
					incident.Validation.ValidateBN_TransportAtDepartureType();
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(incident.BN_TransportAtDepartureTypeInfo, message);

					incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._6;
					incident.Validation.ValidateBN_TransportAtDepartureType();
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(incident.BN_TransportAtDepartureTypeInfo, message);

					incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._1;
					incident.Validation.ValidateBN_TransportAtDepartureType();
					ValidationTestHelper.AssertFieldIsNotMandatory(incident.BN_TransportAtDepartureTypeInfo, message);

					deciderTestContext.DisableRule(c => c.IsRuleC0240_1Active);
					incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._3;
					incident.Validation.ValidateBN_TransportAtDepartureType();
					ValidationTestHelper.AssertFieldIsNotMandatory(incident.BN_TransportAtDepartureTypeInfo, message);
				}
			});
		}

		public void TestCheckBN_TransportAtDepartureID_C0240_1Rule()
		{
			var message = "[C0240-1] You have not entered an Identification Number.";
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleC0240_1Active);
					incident.Validation.ValidateBN_TransportAtDepartureID();
					ValidationTestHelper.AssertFieldIsNotMandatory(incident.BN_TransportAtDepartureIDInfo, message);

					incident.BN_TransportAtDepartureType = "11";
					incident.Validation.ValidateBN_TransportAtDepartureID();
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(incident.BN_TransportAtDepartureIDInfo, message);

					deciderTestContext.DisableRule(c => c.IsRuleC0240_1Active);
					incident.BN_TransportAtDepartureType = "11";
					incident.Validation.ValidateBN_TransportAtDepartureID();
					ValidationTestHelper.AssertFieldIsNotMandatory(incident.BN_TransportAtDepartureIDInfo, message);
				}
			});
		}

		public void TestCheckBN_RN_NKTransportAtDepartureIDNationality_C0240_1Rule()
		{
			var message = "[C0240-1] You have not entered a Nationality.";
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleC0240_1Active);
					incident.Validation.ValidateBN_RN_NKTransportAtDepartureIDNationality();
					ValidationTestHelper.AssertFieldIsNotMandatory(incident.BN_RN_NKTransportAtDepartureIDNationalityInfo, message);

					incident.BN_TransportAtDepartureType = "11";
					incident.Validation.ValidateBN_RN_NKTransportAtDepartureIDNationality();
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(incident.BN_RN_NKTransportAtDepartureIDNationalityInfo, message);

					deciderTestContext.DisableRule(c => c.IsRuleC0240_1Active);
					incident.BN_TransportAtDepartureType = "11";
					incident.Validation.ValidateBN_RN_NKTransportAtDepartureIDNationality();
					ValidationTestHelper.AssertFieldIsNotMandatory(incident.BN_RN_NKTransportAtDepartureIDNationalityInfo, message);
				}
			});
		}

		public void TestCheckGoodsLocationDescription()
		{
			incident.GoodsLocation.CGL_Qualifier = "U";
			incident.Validation.ValidateAll();
			AssertNoMessageErrorContaining(incident.GoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			incident.GoodsLocation.CGL_Qualifier = ZString.Empty;
			incident.Validation.ValidateAll();
			AssertHasMessageErrorContaining(incident.GoodsLocationDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBN_Information_ArrivalDetailsReadOnly()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			incident.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			CombineAssertions(() =>
			{
				incident.Validation.ValidateBN_Information();
				AssertNoNotifications("Incident false info not entered", incident.BN_InformationInfo);

				incident.BN_Information = "123";
				AssertNoNotifications("Incident false info entered", incident.BN_InformationInfo);
			});
		}

		public void TestCheckBN_EventCountryCode_ArrivalDetailsReadOnly()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			incident.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			var eventCountryCodeinfo = incident.BN_EventCountryCodeInfo;
			CombineAssertions(() =>
			{
				incident.BN_EventCountryCode = ZString.Empty;
				AssertNoNotifications("Mandatory", eventCountryCodeinfo);

				incident.BN_EventCountryCode = Core.Constants.CountryCodes.Bangladesh;
				AssertNoNotifications("Entered", eventCountryCodeinfo);
				AssertNoNotifications("Invalid Code", eventCountryCodeinfo);

				incident.BN_EventCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				AssertNoNotifications("Valid Code", eventCountryCodeinfo);

				incident.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				incident.BN_EventCountryCode = "";
				AssertNoNotifications("Mandatory", eventCountryCodeinfo);
			});
		}

		public void TestCheckBN_EndorsementCountryCode()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				incident.Header.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
				ValidationTestHelper.AssertInvalidCodeMessageError(incident.BN_EndorsementCountryCodeInfo, Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.UnitedKingdom);
				ValidationTestHelper.AssertFieldIsNotMandatory(incident.BN_EndorsementCountryCodeInfo, MandatoryValidation.YouHaveNotEntered, "ArrivalDetailsReadOnly false => run List validation without mandatory validation");

				incident.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				incident.Validation.ValidateBN_EndorsementCountryCode();
				AssertNoMessageErrorContaining("ArrivalDetailsReadOnly true => don't run validation", incident.BN_EndorsementCountryCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckBN_IncidentCode_SealsMandatoryWhenIncidentCode24_ArrivalDetailsReadOnly()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			incident.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			var targetInfo = incident.BN_IncidentCodeInfo;
			CombineAssertions(() =>
			{
				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._2;
				AssertNoNotifications("BN_IncidentCode 2, no seals", targetInfo);

				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._4;
				AssertNoNotifications("BN_IncidentCode 4, no seals", targetInfo);

				incident.Seals.AddNew();
				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._2;
				AssertNoNotifications("BN_IncidentCode 2, has seals linked to incident", targetInfo);

				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._4;
				AssertNoNotifications("BN_IncidentCode 4, has seals linked to incident", targetInfo);

				incident.Seals.RemoveAndDeleteAll();
				var container = incident.IncidentContainers.AddNew();
				container.Seals.AddNew();
				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._2;
				AssertNoNotifications("BN_IncidentCode 2, has seals linked to container", targetInfo);

				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._4;
				AssertNoNotifications("BN_IncidentCode 4, has seals linked to container", targetInfo);
			});
		}

		public void TestCheckBN_IncidentCode_SealsMustBeEmptyWhenIncidentCode15_ArrivalDetailsReadOnly()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			incident.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			var targetInfo = incident.BN_IncidentCodeInfo;
			CombineAssertions(() =>
			{
				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._1;
				AssertNoNotifications("BN_IncidentCode 1, no seals", targetInfo);

				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._5;
				AssertNoNotifications("BN_IncidentCode 5, no seals", targetInfo);

				incident.Seals.AddNew();
				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._1;
				AssertNoNotifications("BN_IncidentCode 1, has seals linked to incident", targetInfo);

				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._5;
				AssertNoNotifications("BN_IncidentCode 5, has seals linked to incident", targetInfo);

				incident.Seals.RemoveAndDeleteAll();
				var container = incident.IncidentContainers.AddNew();
				container.Seals.AddNew();
				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._1;
				AssertNoNotifications("BN_IncidentCode 1, has seals linked to container", targetInfo);

				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._5;
				AssertNoNotifications("BN_IncidentCode 5, has seals linked to container", targetInfo);
			});
		}

		public void TestCheckBN_IncidentCode_C0240_3Rule_WhenActive()
		{
			const string message = "[C0240-3] You have not captured any Containers/Equipment.";
			var targetInfo = incident.BN_IncidentCodeInfo;

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0240_3Active)))
			{
				CombineAssertions(() =>
				{
					incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._1;
					AssertNoMessageError("BN_IncidentCode = 1, containers are not mandatory", targetInfo, message);

					incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._2;
					AssertHasMessageError("BN_IncidentCode = 2, Message error if no container captured", targetInfo, message);

					incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._4;
					AssertHasMessageError("BN_IncidentCode = 4, Message error if no container captured", targetInfo, message);

					incident.IncidentContainers.AddNew();
					incident.Validation.ValidateBN_IncidentCode();
					AssertNoMessageError("No message error if container captured", targetInfo, message);
				});
			}
		}

		public void TestCheckBN_IncidentCode_C0240_3Rule_WhenNotActive()
		{
			const string message = "[C0240-3] You have not captured any Containers/Equipment.";
			var targetInfo = incident.BN_IncidentCodeInfo;

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0240_3Active)))
			{
				incident.BN_IncidentCode = CusInBondEventIncidentCodeList.Codes._2;
				AssertEquals(false, targetInfo.HasMessageError(message));
			}
		}

		public void TestCheckGoodsLocationDescription_ArrivalDetailsReadOnly()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			incident.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			CombineAssertions(() =>
			{
				incident.GoodsLocation.CGL_Qualifier = "U";
				incident.Validation.ValidateAll();
				AssertNoNotifications(incident.GoodsLocationDescriptionInfo);

				incident.GoodsLocation.CGL_Qualifier = ZString.Empty;
				incident.Validation.ValidateAll();
				AssertNoNotifications(incident.GoodsLocationDescriptionInfo);
			});
		}

		public void TestCheckBN_TransportAtDepartureID_MaxLength()
		{
			incident.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthDuringTransitionPeriod(incident.BN_TransportAtDepartureIDInfo, 27);
			});
		}

		public void TestCheckBN_EndorsementDate_MandatoryTR0012()
		{
			const string messageError = "[TR0012] You have not entered a Date of Endorsement.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					var incident = CreateIncidentWithPhase5Header();

					deciderTestContext.EnableRule(c => c.IsRuleTR0012Active);
					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementDateInfo, incident.BN_EndorsementAuthorityInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementDateInfo, incident.BN_EndorsementPlaceInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementDateInfo, incident.BN_EndorsementCountryCodeInfo, combineAssertions: false, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleTR0012Active);

					incident.BN_EndorsementAuthority = "ABC";
					incident.Validation.ValidateBN_EndorsementDate();
					AssertNoMessageError("BN_EndorsementAuthority not empty, rule disabled", incident.BN_EndorsementDateInfo, messageError);
					incident.BN_EndorsementAuthority = ZString.Empty;

					incident.BN_EndorsementPlace = "ABC";
					incident.Validation.ValidateBN_EndorsementDate();
					AssertNoMessageError("BN_EndorsementPlace not empty, rule disabled", incident.BN_EndorsementDateInfo, messageError);
					incident.BN_EndorsementPlace = ZString.Empty;

					incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Germany;
					incident.Validation.ValidateBN_EndorsementDate();
					AssertNoMessageError("BN_EndorsementCountryCode not empty, rule disabled", incident.BN_EndorsementDateInfo, messageError);

					deciderTestContext.EnableRule(c => c.IsRuleTR0012Active);
				}
			});
		}

		public void TestCheckBN_EndorsementAuthority_MandatoryTR0013()
		{
			const string messageError = "[TR0013] You have not entered an Authority of Endorsement.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					var incident = CreateIncidentWithPhase5Header();

					deciderTestContext.EnableRule(c => c.IsRuleTR0013Active);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementAuthorityInfo, incident.BN_EndorsementDateInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementAuthorityInfo, incident.BN_EndorsementPlaceInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementAuthorityInfo, incident.BN_EndorsementCountryCodeInfo, combineAssertions: false, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleTR0013Active);

					incident.BN_EndorsementDate = ZDateTime.Today;
					incident.Validation.ValidateBN_EndorsementAuthority();
					AssertNoMessageError("BN_EndorsementDate not empty, rule disabled", incident.BN_EndorsementAuthorityInfo, messageError);
					incident.BN_EndorsementDate = ZDateTime.Empty;

					incident.BN_EndorsementPlace = "ABC";
					incident.Validation.ValidateBN_EndorsementAuthority();
					AssertNoMessageError("BN_EndorsementPlace not empty, rule disabled", incident.BN_EndorsementAuthorityInfo, messageError);
					incident.BN_EndorsementPlace = ZString.Empty;

					incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Germany;
					incident.Validation.ValidateBN_EndorsementAuthority();
					AssertNoMessageError("BN_EndorsementCountryCode not empty, rule disabled", incident.BN_EndorsementAuthorityInfo, messageError);
				}
			});
		}

		public void TestCheckBN_EndorsementPlaceTR0014()
		{
			const string messageError = "[TR0014] You have not entered a Place where incident was reported.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					var incident = CreateIncidentWithPhase5Header();

					deciderTestContext.EnableRule(c => c.IsRuleTR0014Active);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementPlaceInfo, incident.BN_EndorsementDateInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementPlaceInfo, incident.BN_EndorsementAuthorityInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementPlaceInfo, incident.BN_EndorsementCountryCodeInfo, combineAssertions: false, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleTR0014Active);

					incident.BN_EndorsementDate = ZDateTime.BrettsBirthday;
					incident.Validation.ValidateBN_EndorsementPlace();
					AssertNoMessageError("BN_EndorsementDate not empty, rule disabled", incident.BN_EndorsementPlaceInfo, messageError);
					incident.BN_EndorsementDate = ZDateTime.Empty;

					incident.BN_EndorsementAuthority = "ABC";
					incident.Validation.ValidateBN_EndorsementPlace();
					AssertNoMessageError("BN_EndorsementAuthority not empty, rule disabled", incident.BN_EndorsementPlaceInfo, messageError);
					incident.BN_EndorsementAuthority = ZString.Empty;

					incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Germany;
					incident.Validation.ValidateBN_EndorsementPlace();
					AssertNoMessageError("BN_EndorsementCountryCode not empty, rule disabled", incident.BN_EndorsementPlaceInfo, messageError);
				}
			});
		}

		public void TestCheckBN_EndorsementCountryCodeTR0015()
		{
			const string messageError = "[TR0015] You have not entered a Country/Region where incident was reported.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new EnRouteIncidentValidationDeciderTestContext<IEnRouteIncidentValidationDecider>(Factory))
				{
					var incident = CreateIncidentWithPhase5Header();

					deciderTestContext.EnableRule(c => c.IsRuleTR0015Active);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementCountryCodeInfo, incident.BN_EndorsementDateInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementCountryCodeInfo, incident.BN_EndorsementAuthorityInfo, combineAssertions: false, messageError);

					ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(incident.BN_EndorsementCountryCodeInfo, incident.BN_EndorsementPlaceInfo, combineAssertions: false, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleTR0015Active);

					incident.BN_EndorsementDate = ZDateTime.BrettsBirthday;
					incident.Validation.ValidateBN_EndorsementCountryCode();
					AssertNoMessageError("BN_EndorsementDate not empty, rule disabled", incident.BN_EndorsementCountryCodeInfo, messageError);
					incident.BN_EndorsementDate = ZDateTime.Empty;

					incident.BN_EndorsementAuthority = Core.Constants.CountryCodes.Germany;
					incident.Validation.ValidateBN_EndorsementCountryCode();
					AssertNoMessageError("BN_EndorsementAuthority not empty, rule disabled", incident.BN_EndorsementCountryCodeInfo, messageError);
					incident.BN_EndorsementAuthority = ZString.Empty;

					incident.BN_EndorsementPlace = "ABC";
					incident.Validation.ValidateBN_EndorsementCountryCode();
					AssertNoMessageError("BN_EndorsementPlace not empty, rule disabled", incident.BN_EndorsementCountryCodeInfo, messageError);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			incident = header.EnRouteIncidents.AddNew();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("C0009", "C0009");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "C0009", "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
		EnRouteIncident incident;

		EnRouteIncident CreateIncidentWithPhase5Header()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header.EnRouteIncidents.AddNew();
		}
	}
}
