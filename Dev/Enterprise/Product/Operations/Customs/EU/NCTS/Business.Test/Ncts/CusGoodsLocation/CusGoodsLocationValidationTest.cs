using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		const string ErrorMessage = "required";

		public void TestCheckCGL_AdditionalIdentifier_U()
		{
			cusGoodsLocation.CGL_Qualifier = "U";

			CombineAssertions(() =>
			{
				cusGoodsLocation.CGL_AdditionalIdentifier = "ABC";
				AssertNoMessageErrorContaining("When the additional identifier is filled, no error for mandatory fields", cusGoodsLocation.CGL_AdditionalIdentifierInfo, ErrorMessage);

				cusGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertHasMessageErrorContaining("The additional identifier should have a mandatory validation for qualifier U", cusGoodsLocation.CGL_AdditionalIdentifierInfo, ErrorMessage);
			});
		}

		public void TestCheckCGL_CustomsOffice_RuleC0394()
		{
			cusGoodsLocation.CGL_Qualifier = "V";
			var errorMessage = "[C0394] Customs Office required when qualifier is 'V'.";
			CombineAssertions(() =>
			{
				cusGoodsLocation.CGL_CustomsOffice = "ABC";
				AssertNoMessageError("When the customs Office is filled, no error for mandatory fields", cusGoodsLocation.CGL_CustomsOfficeInfo, errorMessage);

				cusGoodsLocation.CGL_CustomsOffice = ZString.Empty;
				AssertHasMessageError("The customs Office should have a mandatory validation for qualifier V", cusGoodsLocation.CGL_CustomsOfficeInfo, errorMessage);
			});
		}

		[TestDate(2022, 07, 01)]
		public void TestCheckCGL_AdditionalIdentifier_T_RuleC0382()
		{
			const string errorMessage = "[C0382] Additional Identifier required when qualifier is 'T'.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL148, "CL148 Desc.");
			helper.CreateCusCodeList("EUN", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL148, "AT", "Austria", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "CY", "Cyprus", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			Factory.Save();

			var cusGoodsLocationDeparture = GetCusGoodsLocationWithDepartureHeader();
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			cusGoodsLocation.CGL_Qualifier = "T";
			cusGoodsLocationDeparture.CGL_Qualifier = "T";

			CombineAssertions(() =>
			{
				using (var testContext = new CusGoodsLocationValidationDeciderTestContext<ICusGoodsLocationValidationDecider>(Factory))
				{
					testContext.EnableRule(r => r.IsRuleC0382Active);

					cusGoodsLocation.CGL_AdditionalIdentifier = "ABC";
					AssertNoMessageErrorContaining("When the additional identifier is filled, no error for mandatory fields", cusGoodsLocation.CGL_AdditionalIdentifierInfo, errorMessage);

					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = "ABC";
					AssertNoMessageErrorContaining("(Departure) When the additional identifier is filled, no error for mandatory fields", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, errorMessage);

					cusGoodsLocation.Address.E2_RN_NKCountryCode = "CY";
					cusGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
					AssertHasMessageErrorContaining("Country with optional post code address", cusGoodsLocation.CGL_AdditionalIdentifierInfo, errorMessage);

					cusGoodsLocationDeparture.Address.E2_RN_NKCountryCode = "CY";
					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = ZString.Empty;
					AssertHasMessageErrorContaining("(Departure) Country with optional post code address", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, errorMessage);

					cusGoodsLocation.Address.E2_RN_NKCountryCode = "AT";
					cusGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageErrorContaining("Country with mandatory post code address", cusGoodsLocation.CGL_AdditionalIdentifierInfo, errorMessage);

					cusGoodsLocationDeparture.Address.E2_RN_NKCountryCode = "AT";
					cusGoodsLocationDeparture.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageErrorContaining("(Departure) Country with mandatory post code address", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, errorMessage);

					testContext.DisableRule(r => r.IsRuleC0382Active);

					cusGoodsLocation.Address.E2_RN_NKCountryCode = "CY";
					cusGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
					AssertNoMessageErrorContaining("Rule disabled", cusGoodsLocation.CGL_AdditionalIdentifierInfo, errorMessage);

					cusGoodsLocationDeparture.Address.E2_RN_NKCountryCode = "CY";
					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = ZString.Empty;
					AssertNoMessageErrorContaining("(Departure) Rule disabled", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, errorMessage);
				}
			});
		}

		public void TestCheckCGL_Qualifier()
		{
			var locationAddress = cusGoodsLocation.Address;
			CombineAssertions(() =>
			{
				cusGoodsLocation.CGL_Qualifier = "Y";
				AssertHasMessageErrorContaining("The qualifier Y should display an error on GovRegNum", locationAddress.E2_GovRegNumInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier Y should display no error on Address1 and Address2", locationAddress.E2_Address1AndE2_Address2Info, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier Y should display no error on City", locationAddress.E2_CityInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier Y should display no error on Country", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier Y should display an error on Postcode", locationAddress.E2_PostcodeInfo, ErrorMessage);

				cusGoodsLocation.CGL_Qualifier = "A";
				AssertNoMessageErrorContaining("The qualifier A should display no error on GovRegNum", locationAddress.E2_GovRegNumInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier A should display no error on Address1 and Address2", locationAddress.E2_Address1AndE2_Address2Info, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier A should display no error on City", locationAddress.E2_CityInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier A should display no error on Country", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier A should display an error on Postcode", locationAddress.E2_PostcodeInfo, ErrorMessage);

				cusGoodsLocation.CGL_Qualifier = "X";
				AssertHasMessageErrorContaining("The qualifier X should display an error on GovRegNum", locationAddress.E2_GovRegNumInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier X should display no error on Address1 and Address2", locationAddress.E2_Address1AndE2_Address2Info, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier X should display no error on City", locationAddress.E2_CityInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier X should display no error on Country", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier X should display an error on Postcode", locationAddress.E2_PostcodeInfo, ErrorMessage);

				cusGoodsLocation.CGL_Qualifier = "Z";
				AssertNoMessageErrorContaining("The qualifier Z should display no error on GovRegNum", locationAddress.E2_GovRegNumInfo, ErrorMessage);
				AssertHasMessageErrorContaining("The qualifier Z should display an error on Address1 and Address2", locationAddress.E2_Address1AndE2_Address2Info, ErrorMessage);
				AssertHasMessageErrorContaining("The qualifier Z should display an error on City", locationAddress.E2_CityInfo, ErrorMessage);
				AssertHasMessageErrorContaining("The qualifier Z should display an error on Country", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier Z should display an error on Postcode", locationAddress.E2_PostcodeInfo, ErrorMessage);

				cusGoodsLocation.CGL_Qualifier = "T";
				AssertNoMessageErrorContaining("The qualifier T should display no error on GovRegNum", locationAddress.E2_GovRegNumInfo, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier T should display an error on Address1 and Address2", locationAddress.E2_Address1AndE2_Address2Info, ErrorMessage);
				AssertNoMessageErrorContaining("The qualifier T should display an error on City", locationAddress.E2_CityInfo, ErrorMessage);
				AssertHasMessageErrorContaining("The qualifier T should display an error on Country", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);
				AssertHasMessageErrorContaining("The qualifier T should display an error on Postcode", locationAddress.E2_PostcodeInfo, ErrorMessage);
			});
		}

		public void TestCheckCGL_Qualifier_NR0011_OnArrivalMovementHeader()
		{
			CheckCGL_Qualifier_NR0011(cusGoodsLocation);
		}

		public void TestCheckCGL_Qualifier_NR0011_OnIncident()
		{
			var incident = arrivalMovementHeader.Header.EnRouteIncidents.AddNew();
			cusGoodsLocation.Parent = incident;
			CheckCGL_Qualifier_NR0011(cusGoodsLocation);
		}

		void CheckCGL_Qualifier_NR0011(CusGoodsLocation cgsLocation) => CombineAssertions(cgsLocation.CGL_ParentTableCode, () =>
		{
			const string message = "[NR0011] The location Qualifier must be U.";

			using (var ruleTestContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(r => r.IsRuleNR0011Active);

				var parentTableCode = cgsLocation.CGL_ParentTableCode;
				arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;

				cgsLocation.CGL_Qualifier = ZString.Empty;
				switch (parentTableCode)
				{
					case CusInBondEventSchema.Constants.Prefix:
						AssertNoMessageErrors("Rule enabled, AuthorizationCode = 'ACE', CGL_Qualifier empty", cgsLocation.CGL_QualifierInfo);
						break;
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						AssertHasMessageError("Rule enabled, AuthorizationCode = 'ACE', CGL_Qualifier empty", cgsLocation.CGL_QualifierInfo, message);
						break;
				}

				cgsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				switch (parentTableCode)
				{
					case CusInBondEventSchema.Constants.Prefix:
						AssertNoMessageErrors("Rule enabled, AuthorizationCode = 'ACE', CGL_Qualifier <> 'U'", cgsLocation.CGL_QualifierInfo);
						break;
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						AssertHasMessageError("Rule enabled, AuthorizationCode = 'ACE', CGL_Qualifier <> 'U'", cgsLocation.CGL_QualifierInfo, message);
						break;
				}

				cgsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertNoMessageErrors("Rule enabled, AuthorizationCode = 'ACE', CGL_Qualifier = 'U'", cgsLocation.CGL_QualifierInfo);

				arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
				cgsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				AssertNoMessageErrors("Rule enabled, AuthorizationCode <> 'ACE', CGL_Qualifier <> 'U'", cgsLocation.CGL_QualifierInfo);

				ruleTestContext.DisableRule(r => r.IsRuleNR0011Active);

				cgsLocation.CGL_Qualifier = ZString.Empty;
				AssertNoMessageErrors("Rule disabled, AuthorizationCode = 'ACE', CGL_Qualifier empty", cgsLocation.CGL_QualifierInfo);

				cgsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				AssertNoMessageErrors("Rule disabled, AuthorizationCode = 'ACE', CGL_Qualifier <> 'U'", cgsLocation.CGL_QualifierInfo);
			}
		});

		public void TestCheckCGL_Type_NR0012_OnArrivalMovementHeader()
		{
			CheckCGL_Type_NR0012(cusGoodsLocation);
		}

		public void TestCheckCGL_Type_NR0012_OnIncident()
		{
			var incident = arrivalMovementHeader.Header.EnRouteIncidents.AddNew();
			cusGoodsLocation.Parent = incident;
			CheckCGL_Type_NR0012(cusGoodsLocation);
		}

		void CheckCGL_Type_NR0012(CusGoodsLocation cgsLocation) => CombineAssertions(cgsLocation.CGL_ParentTableCode, () =>
		{
			const string message = "[NR0012] The location Type must be C.";

			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.EnableRule(r => r.IsRuleNR0012Active);

				var parentTableCode = cgsLocation.CGL_ParentTableCode;
				arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;

				cgsLocation.CGL_Type = ZString.Empty;
				switch (parentTableCode)
				{
					case CusInBondEventSchema.Constants.Prefix:
						AssertNoMessageErrors("Rule enabled, AuthorizationCode = 'ACE', CGL_Type empty", cgsLocation.CGL_TypeInfo);
						break;
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						AssertHasMessageError("Rule enabled, AuthorizationCode = 'ACE', CGL_Type empty", cgsLocation.CGL_TypeInfo, message);
						break;
				}

				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
				switch (parentTableCode)
				{
					case CusInBondEventSchema.Constants.Prefix:
						AssertNoMessageErrors("Rule enabled, AuthorizationCode = 'ACE', CGL_Type <> 'C'", cgsLocation.CGL_TypeInfo);
						break;
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						AssertHasMessageError("Rule enabled, AuthorizationCode = 'ACE', CGL_Type <> 'C'", cgsLocation.CGL_TypeInfo, message);
						break;
				}

				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				AssertNoMessageErrors("Rule enabled, AuthorizationCode = 'ACE', CGL_Type = 'C'", cgsLocation.CGL_TypeInfo);

				arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
				AssertNoMessageErrors("Rule enabled, AuthorizationCode <> 'ACE', CGL_Type <> 'C'", cgsLocation.CGL_TypeInfo);

				testContext.DisableRule(r => r.IsRuleNR0012Active);

				cgsLocation.CGL_Type = ZString.Empty;
				AssertNoMessageErrors("Rule disabled, AuthorizationCode = 'ACE', CGL_Type empty", cgsLocation.CGL_TypeInfo);

				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
				AssertNoMessageErrors("Rule disabled, AuthorizationCode = 'ACE', CGL_Type <> 'C'", cgsLocation.CGL_TypeInfo);
			}
		});

		public void TestCheckCGL_Type_NR0075_OnArrivalMovementHeader()
		{
			CheckCGL_Type_NR0075(cusGoodsLocation, "ACE");
			CheckCGL_Type_NR0075(cusGoodsLocation, "ACT");
		}

		public void TestCheckCGL_Type_NR0075_OnIncident()
		{
			var incident = arrivalMovementHeader.Header.EnRouteIncidents.AddNew();
			cusGoodsLocation.Parent = incident;
			CheckCGL_Type_NR0075(cusGoodsLocation, "ACE");
			CheckCGL_Type_NR0075(cusGoodsLocation, "ACT");
		}

		void CheckCGL_Type_NR0075(CusGoodsLocation cgsLocation, ZString authorizationCode) => CombineAssertions(cgsLocation.CGL_ParentTableCode, () =>
		{
			const string message = "[NR0075] Type of location must be A if authorization ACE nor ACT is used";

			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.EnableRule(r => r.IsRuleNR0075Active);

				var parentTableCode = cgsLocation.CGL_ParentTableCode;
				arrivalMovementHeader.AuthorizationCode = authorizationCode;

				cgsLocation.CGL_Type = ZString.Empty;
				AssertNoMessageErrors("Rule enabled, AuthorizationCode = " + authorizationCode + ", CGL_Type empty", cgsLocation.CGL_TypeInfo);

				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				AssertNoMessageErrors("Rule enabled, AuthorizationCode = " + authorizationCode + ", CGL_Type <> 'A'", cgsLocation.CGL_TypeInfo);

				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
				AssertNoMessageErrors("Rule enabled, AuthorizationCode = " + authorizationCode + ", CGL_Type = 'A'", cgsLocation.CGL_TypeInfo);

				arrivalMovementHeader.AuthorizationCode = "X";
				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				switch (parentTableCode)
				{
					case CusInBondEventSchema.Constants.Prefix:
						AssertNoMessageErrors("Rule enabled, AuthorizationCode <> " + authorizationCode + ", CGL_Type <> 'A'", cgsLocation.CGL_TypeInfo);
						break;
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						AssertHasMessageError("Rule enabled, AuthorizationCode <> " + authorizationCode + ", CGL_Type <> 'A'", cgsLocation.CGL_TypeInfo, message);
						break;
				}

				testContext.DisableRule(r => r.IsRuleNR0075Active);

				cgsLocation.CGL_Type = ZString.Empty;
				AssertNoMessageErrors("Rule disabled, AuthorizationCode <> " + authorizationCode + " CGL_Type empty", cgsLocation.CGL_TypeInfo);

				cgsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				AssertNoMessageErrors("Rule disabled, AuthorizationCode <> " + authorizationCode + ", CGL_Type <> 'A'", cgsLocation.CGL_TypeInfo);
			}
		});

		public void TestCheckCGL_AdditionalIdentifier_NR0013_OnArrivalMovementHeader()
		{
			CheckCGL_AdditionalIdentifier_NR0013(cusGoodsLocation);
		}

		public void TestCheckCGL_AdditionalIdentifier_NR0013_OnIncident()
		{
			var incident = arrivalMovementHeader.Header.EnRouteIncidents.AddNew();
			cusGoodsLocation.Parent = incident;
			CheckCGL_AdditionalIdentifier_NR0013(cusGoodsLocation);
		}

		public void TestCheck_CustomsOffice_RuleTR0061_ArrivalMovement()
		{
			AssertCustomsOfficeRuleTR0061(cusGoodsLocation);
		}

		public void TestCheck_CustomsOffice_RuleTR0061_DepartureMovement()
		{
			AssertCustomsOfficeRuleTR0061(GetCusGoodsLocationWithDepartureHeader());
		}

		void CheckCGL_AdditionalIdentifier_NR0013(CusGoodsLocation cgsLocation) => CombineAssertions(cgsLocation.CGL_ParentTableCode, () =>
		{
			const string message = "[NR0013] Locations is not mentioned in the authorization ACE/C522. Please fill in correct UN/LOCODE out of authorization.";

			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.EnableRule(r => r.IsRuleNR0013Active);

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var authorization1 = Factory.New<CusAuthorisationHeader>();
				authorization1.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
				authorization1.CPH_OH_PermitHolder = orgHeader.PK;
				authorization1.CPH_Number = "ACE001";
				var rule = authorization1.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				rule.CPR_ValueFrom = "1";
				var authorization2 = Factory.New<CusAuthorisationHeader>();
				authorization2.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
				authorization2.CPH_OH_PermitHolder = orgHeader.PK;
				authorization2.CPH_Number = "ACE002";
				Factory.Save();

				var parentTableCode = cgsLocation.CGL_ParentTableCode;
				arrivalMovementHeader.AuthorizationCode = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;

				cgsLocation.CGL_AdditionalIdentifier = ZString.Empty;
				AssertNoMessageErrors("Rule enabled, CGL_AdditionalIdentifier empty", cgsLocation.CGL_AdditionalIdentifierInfo);

				cgsLocation.CGL_AdditionalIdentifier = "1";
				switch (parentTableCode)
				{
					case CusInBondEventSchema.Constants.Prefix:
						AssertNoMessageErrors("Rule enabled, No authorization", cgsLocation.CGL_AdditionalIdentifierInfo);
						break;
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						AssertHasMessageError("Rule enabled, No authorization", cgsLocation.CGL_AdditionalIdentifierInfo, message);
						break;
				}

				arrivalMovementHeader.AuthorizationOwner = orgHeader.PK;
				arrivalMovementHeader.AuthorizationNumber = "ACE001";
				cgsLocation.CGL_AdditionalIdentifier = "1";
				AssertNoMessageErrors("Rule enabled, Has authorization with matching CPR_ValueFrom = CGL_AdditionalIdentifier", cgsLocation.CGL_AdditionalIdentifierInfo);

				arrivalMovementHeader.AuthorizationNumber = "ACE002";
				cgsLocation.CGL_AdditionalIdentifier = "1";
				switch (parentTableCode)
				{
					case CusInBondEventSchema.Constants.Prefix:
						AssertNoMessageErrors("Rule enabled, Authorization doesn't have matching CPR_ValueFrom = CGL_AdditionalIdentifier", cgsLocation.CGL_AdditionalIdentifierInfo);
						break;
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						AssertHasMessageError("Rule enabled, Authorization doesn't have matching CPR_ValueFrom = CGL_AdditionalIdentifier", cgsLocation.CGL_AdditionalIdentifierInfo, message);
						break;
				}

				testContext.DisableRule(r => r.IsRuleNR0013Active);

				cgsLocation.CGL_AdditionalIdentifier = "1";
				AssertNoMessageErrors("Rule disabled, No authorization", cgsLocation.CGL_AdditionalIdentifierInfo);

				arrivalMovementHeader.AuthorizationNumber = "ACE002";
				cgsLocation.CGL_AdditionalIdentifier = "1";
				AssertNoMessageErrors("Rule disabled, Authorization doesn't have matching CPR_ValueFrom = CGL_AdditionalIdentifier", cgsLocation.CGL_AdditionalIdentifierInfo);
			}
		});

		public void TestRuleNR0023()
		{
			cusGoodsLocation.CGL_Qualifier = "Y";
			cusGoodsLocation.CGL_Type = "B";
			var cusGoodsLocationDeparture = GetCusGoodsLocationWithDepartureHeader();
			cusGoodsLocationDeparture.CGL_Qualifier = "Y";
			cusGoodsLocationDeparture.CGL_Type = "B";

			const string expectedErrorMessage = "[NR0023] This field must be filled.";

			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IDeparturePhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.EnableRule(r => r.IsRuleNR0023Active);
				CombineAssertions(() =>
				{
					cusGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
					AssertNoErrorContaining("Arrival", cusGoodsLocation.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					cusGoodsLocationDeparture.CGL_Type = "A";
					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = ZString.Empty;
					AssertNoErrorContaining("Phase 5 departure but location type is not B or C", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					cusGoodsLocationDeparture.CGL_Type = "B";
					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = "abc";
					AssertNoErrorContaining("Phase 5 departure and location type is B, and place code is filled", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					cusGoodsLocationDeparture.CGL_Type = "B";
					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = ZString.Empty;
					AssertHasMessageErrorContaining("Phase 5 departure and location type is B, place code is empty", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					cusGoodsLocationDeparture.CGL_Type = "B";
					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = "abc";
					AssertNoErrorContaining("Phase 5 departure and location type is C, and place code is filled", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					cusGoodsLocationDeparture.CGL_Type = "C";
					cusGoodsLocationDeparture.CGL_AdditionalIdentifier = ZString.Empty;
					AssertHasMessageErrorContaining("Phase 5 departure and location type is C, place code is empty", cusGoodsLocationDeparture.CGL_AdditionalIdentifierInfo, expectedErrorMessage);
				});
			}
		}

		public void TestCheckAdditionalIdentifierRuleNR0050()
		{
			const string expectedErrorMessage = "[NR0050] You have not entered an Additional Identifier in Location of Goods.";
			var departureGoodsLocation = GetCusGoodsLocationWithDepartureHeader();
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IDeparturePhase5CusGoodsLocationValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					testContext.EnableRule(r => r.IsRuleNR0050Active);
					departureGoodsLocation.DepartureMovementHeader.IsSimplifiedNctsProcedure = true;
					departureGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
					departureGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
					AssertNoMessageError("Phase5Departure: RuleNR0050 enabled, IsSimplifiedProcedure, CGL_Qualifier != Y, CGL_AdditionalIdentifier = ''", departureGoodsLocation.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					departureGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
					departureGoodsLocation.CGL_AdditionalIdentifier = "123";
					AssertNoMessageError("Phase5Departure: RuleNR0050 enabled, IsSimplifiedProcedure, CGL_Qualifier = Y, CGL_AdditionalIdentifier != ''", departureGoodsLocation.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					departureGoodsLocation.DepartureMovementHeader.IsSimplifiedNctsProcedure = false;
					departureGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
					AssertNoMessageError("Phase5Departure: RuleNR0050 enabled, Is not SimplifiedProcedure, CGL_Qualifier = Y, CGL_AdditionalIdentifier = ''", departureGoodsLocation.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					departureGoodsLocation.DepartureMovementHeader.IsSimplifiedNctsProcedure = true;
					departureGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertHasMessageError("Phase5Departure: RuleNR0050 enabled, IsSimplifiedProcedure, CGL_Qualifier = Y, CGL_AdditionalIdentifier = ''", departureGoodsLocation.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					testContext.DisableRule(r => r.IsRuleNR0050Active);
					departureGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageError("Phase5Departure: RuleNR0050 disabled, IsSimplifiedProcedure, CGL_Qualifier = Y, CGL_AdditionalIdentifier = ''", departureGoodsLocation.CGL_AdditionalIdentifierInfo, expectedErrorMessage);

					testContext.EnableRule(r => r.IsRuleNR0050Active);
					nctsHeader.ArrivalMovementHeader.IsSimplifiedNctsProcedure = true;
					cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
					cusGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
					AssertNoMessageError("Phase5Arrival: RuleNR0050 enabled, IsSimplifiedProcedure, CGL_Qualifier = Y, CGL_AdditionalIdentifier = ''", cusGoodsLocation.CGL_AdditionalIdentifierInfo, expectedErrorMessage);
				});
			}
		}

		public void TestRuleNR0053() => CombineAssertions(() =>
		{
			var goodsLocation = GetCusGoodsLocationWithDepartureHeader();
			var messageError = "[NR0053] When additional declaration type is A, the qualifier of the location must be U and its type must be C. When the additional declaration type is D, the qualifier must be V or U or blank and its type must be A or C or blank.";
			AssertEquals("RuleNR0053ErrorMessage", messageError, goodsLocation.Header?.Configuration.ValidationRuleConfiguration.Messages.NR0053Message);
			var combinations = new List<(string additionalDeclarationType, string locationQualifier, string type, bool errorExpected)>
			{
				("A", "U", "C", false),
				("D", "V", "A", false),
				("D", "V", "C", false),
				("D", "V", "", false),
				("D", "U", "A", false),
				("D", "U", "C", false),
				("D", "U", "", false),
				("D", "", "A", false),
				("D", "", "C", false),
				("D", "", "", false),
				("A", "", "", true),
				("A", "V", "C", true),
				("A", "", "C", true),
				("A", "U", "A", true),
				("A", "U", "", true),
				("D", "X", "", true),
				("D", "", "X", true),
			};
			var movementHeader = (NctsDepartureMovementHeader)goodsLocation.Parent;

			using (var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				foreach (var combination in combinations)
				{
					movementHeader.BM_AdditionalDeclarationType = combination.additionalDeclarationType;
					goodsLocation.CGL_Type = combination.type;
					goodsLocation.CGL_Qualifier = combination.locationQualifier;
					AssertNoMessageError($"Rule Inactive, Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", goodsLocation.CGL_QualifierInfo, messageError);
					AssertEquals($"(IsRuleNR0053Violated) Rule Inactive, Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", false, goodsLocation.Validation.IsRuleNR0053Violated());
				}
				testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleNR0053Active));
				foreach (var combination in combinations)
				{
					movementHeader.BM_AdditionalDeclarationType = combination.additionalDeclarationType;
					goodsLocation.CGL_Type = combination.type;
					goodsLocation.CGL_Qualifier = combination.locationQualifier;
					if (combination.errorExpected)
					{
						AssertHasMessageError($"Rule Active, Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", goodsLocation.CGL_QualifierInfo, messageError);
						AssertEquals($"(IsRuleNR0053Violated) Rule Active, Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", true, goodsLocation.Validation.IsRuleNR0053Violated());
					}
					else
					{
						AssertNoMessageError($"Rule Active, Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", goodsLocation.CGL_QualifierInfo, messageError);
						AssertEquals($"(IsRuleNR0053Violated) Rule Active, Additional Declaration Type '{combination.additionalDeclarationType}', Location Qualifier '{combination.locationQualifier}', Type '{combination.type}'", false, goodsLocation.Validation.IsRuleNR0053Violated());
					}
				}
			}
		});

		public void TestCheckAdditionalIdentifierRuleTR0069()
		{
			var arrivalGoodsLocation = cusGoodsLocation;
			arrivalGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
			var messageError = $"[TR0069] You have not entered an {arrivalGoodsLocation.CGL_AdditionalIdentifierInfo.HumanReadableName}.";
			var departureGoodsLocation = GetCusGoodsLocationWithDepartureHeader();
			departureGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;

			using (var ruleTestContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.DisableRule(r => r.IsRuleTR0069Active);
					departureGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageError("Departure Goods Location Rule TR0069 disabled expected no message error", departureGoodsLocation.CGL_AdditionalIdentifierInfo, messageError);

					arrivalGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageError("Arrival Goods Location Rule TR0069 disabled expected no message error", arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, messageError);

					ruleTestContext.EnableRule(r => r.IsRuleTR0069Active);
					departureGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageError("Departure Goods Location Rule TR0069 enabled expected no message error", departureGoodsLocation.CGL_AdditionalIdentifierInfo, messageError);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					arrivalGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageError("Arrival Goods Location no Phase5 Rule TR0069 enabled expected no message error", arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, messageError);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					arrivalGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertHasMessageError("Arrival Goods Location Phase5 Rule TR0069 enabled expected message error", arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, messageError);

					arrivalGoodsLocation.CGL_AdditionalIdentifier = "A";
					arrivalGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
					AssertNoMessageError("Arrival Goods Location Phase5 Rule TR0069 enabled and filled expected no message error", arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, messageError);
				});
			}
		}

		public void TestRuleNR0063()
		{
			var expectedMessageError = "[NR0063] When there is an authorization with type ACR, the type of location must be B. If there is no authorization with type ACR, the type of location must be A.";
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			var goodsLocation = movementHeader.GoodsLocation;
			var authorization = movementHeader.CusAuthorizationUsages.AddNew();

			using (var ruleTestContext = new CusGoodsLocationValidationDeciderTestContext<IDeparturePhase5CusGoodsLocationValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					ruleTestContext.DisableRule(r => r.IsRuleNR0063Active);
					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertNoMessageError("Authorization code 'ACR' & GoodsLocation type 'A', no message error expected (rule disabled)", goodsLocation.CGL_TypeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertNoMessageError("Authorization code <> 'ACR' & GoodsLocation type 'A', no message error expected", goodsLocation.CGL_TypeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertNoMessageError("Authorization code 'ACR' & GoodsLocation type 'B', no message error expected", goodsLocation.CGL_TypeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertNoMessageError("Authorization code <> 'ACR' & GoodsLocation type 'B', no message error expected (rule disabled)", goodsLocation.CGL_TypeInfo, expectedMessageError);

					ruleTestContext.EnableRule(r => r.IsRuleNR0063Active);
					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertHasMessageError("Authorization code 'ACR' & GoodsLocation type 'A', message error expected", goodsLocation.CGL_TypeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertNoMessageError("Authorization code <> 'ACR' & GoodsLocation type 'A', no message error expected", goodsLocation.CGL_TypeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertNoMessageError("Authorization code 'ACR' & GoodsLocation type 'B', no message error expected", goodsLocation.CGL_TypeInfo, expectedMessageError);

					authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
					goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
					goodsLocation.Validation.ValidateCGL_Type();
					AssertHasMessageError("Authorization code <> 'ACR' & GoodsLocation type 'B', message error expected", goodsLocation.CGL_TypeInfo, expectedMessageError);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_LocationUse = "ARR";
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			cusGoodsLocation.Parent = arrivalMovementHeader;
		}

		CusGoodsLocation GetCusGoodsLocationWithDepartureHeader()
		{
			var cusGoodsLocationDeparture = Factory.New<CusGoodsLocation>();
			cusGoodsLocationDeparture.CGL_LocationUse = "ARR";
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			cusGoodsLocationDeparture.Parent = nctsHeader.MovementHeader;

			return cusGoodsLocationDeparture;
		}

		NctsHeader nctsHeader;
		CusGoodsLocation cusGoodsLocation;
		NctsArrivalMovementHeader arrivalMovementHeader;

		#region Implementation

		void AssertCustomsOfficeRuleTR0061(CusGoodsLocation cusGoodsLocation)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MainCustomsOffice", "MainCustomsOffice", "CUSOF", "LV", "CUSOF", false, false);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ROLE", "ROLE", "CUSOF", "LV", "CUSOF", false, false);
			helper.CreateCusCodeList("LV", "CUSOF", "LV008734", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			cusGoodsLocation.CGL_Qualifier = "V";
			cusGoodsLocation.CGL_CustomsOffice = "XXX";

			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<ICusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleTR0061Active);
				cusGoodsLocation.Validation.ValidateCGL_CustomsOffice();
				AssertNoMessageError("When NCTS5, RuleTR0061: Disabled, CustomsOffice: XXX, Qualifier: V",
					cusGoodsLocation.CGL_CustomsOfficeInfo, ruleTR0061ExpectedMessageError);

				testContext.EnableRule(r => r.IsRuleTR0061Active);
				cusGoodsLocation.Validation.ValidateCGL_CustomsOffice();
				AssertHasMessageError("When NCTS5, RuleTR0061: Active, CustomsOffice: XXX, Qualifier: V",
					cusGoodsLocation.CGL_CustomsOfficeInfo, ruleTR0061ExpectedMessageError);

				cusGoodsLocation.CGL_CustomsOffice = "LV008734";
				AssertNoMessageError("CustomsOffice is in the list",
					cusGoodsLocation.CGL_CustomsOfficeInfo, ruleTR0061ExpectedMessageError);

				cusGoodsLocation.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				cusGoodsLocation.Validation.ValidateCGL_CustomsOffice();
				AssertNoMessageError("When NCTS4, RuleTR0061: Active, CustomsOffice: XXX, Qualifier: V",
					cusGoodsLocation.CGL_CustomsOfficeInfo, ruleTR0061ExpectedMessageError);
			}
		}

		const string ruleTR0061ExpectedMessageError = "[TR0061] Customs Office code is not in the list";

		#endregion
	}
}
