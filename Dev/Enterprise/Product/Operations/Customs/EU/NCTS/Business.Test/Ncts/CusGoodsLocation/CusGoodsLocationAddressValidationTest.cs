using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
	{
		const string ErrorMessage = "required";

		public void TestCheckE2_GovRegNum_RuleC0394_WhenActive()
		{
			CombineAssertions(() =>
			{
				using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
				{
					testContext.EnableRule(r => r.IsRuleC0394Active);

					cusGoodsLocation.CGL_Qualifier = "Y";
					locationAddress.E2_GovRegNum = ZString.Empty;
					AssertHasMessageErrorContaining("The govRegNum should have a mandatory validation for qualifier Y", locationAddress.E2_GovRegNumInfo, "Authorization Number required when qualifier is 'Y'.");
					AssertHasMessageErrorContaining("The govRegNum should have a mandatory validation for qualifier Y", locationAddress.E2_GovRegNumInfo, ErrorMessage);
					AssertHasMessageWithRuleNumberC0394(locationAddress.E2_GovRegNumInfo);

					locationAddress.E2_GovRegNum = "ABC";
					AssertNoMessageErrorContaining("When the govRegNum is filled, no error for mandatory fields", locationAddress.E2_GovRegNumInfo, ErrorMessage);

					cusGoodsLocation.CGL_Qualifier = "X";
					locationAddress.E2_GovRegNum = ZString.Empty;
					AssertHasMessageErrorContaining("The govRegNum should have a mandatory validation for qualifier X", locationAddress.E2_GovRegNumInfo, "EORI Number required when qualifier is 'X'.");
					AssertHasMessageErrorContaining("The govRegNum should have a mandatory validation for qualifier X", locationAddress.E2_GovRegNumInfo, ErrorMessage);
					AssertHasMessageWithRuleNumberC0394(locationAddress.E2_GovRegNumInfo);

					locationAddress.E2_GovRegNum = "ABC";
					AssertNoMessageErrorContaining("When the govRegNum is filled, no error for mandatory fields", locationAddress.E2_GovRegNumInfo, ErrorMessage);
				}
			});
		}

		public void TestCheckE2_GovRegNum_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "Y";
				locationAddress.E2_GovRegNum = ZString.Empty;
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_GovRegNumInfo.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestCheckE2_Address1AndE2_Address2_RuleC0394_WhenActive()
		{
			cusGoodsLocation.CGL_Qualifier = "Z";

			CombineAssertions(() =>
			{
				using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
				{
					testContext.EnableRule(r => r.IsRuleC0394Active);

					locationAddress.E2_Address1AndE2_Address2 = "ABC";
					AssertNoMessageErrorContaining("When the street and number field is filled, no error for mandatory fields", locationAddress.E2_Address1AndE2_Address2Info, ErrorMessage);

					locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
					AssertHasMessageErrorContaining("The street and number field should have a mandatory validation for qualifier Z", locationAddress.E2_Address1AndE2_Address2Info, ErrorMessage);
					AssertHasMessageWithRuleNumberC0394(locationAddress.E2_Address1AndE2_Address2Info);
				}
			});
		}

		public void TestCheckE2_Address1AndE2_Address2_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "Z";
				locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_Address1AndE2_Address2Info.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestCheckE2_City_RuleC0394_WhenActive()
		{
			cusGoodsLocation.CGL_Qualifier = "Z";

			CombineAssertions(() =>
			{
				using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
				{
					testContext.EnableRule(r => r.IsRuleC0394Active);
					locationAddress.E2_City = "ABC";
					AssertNoMessageErrorContaining("When the city is filled, no error for mandatory fields", locationAddress.E2_CityInfo, ErrorMessage);

					locationAddress.E2_City = ZString.Empty;
					AssertHasMessageErrorContaining("The city should have a mandatory validation for qualifier Z", locationAddress.E2_CityInfo, ErrorMessage);
					AssertHasMessageWithRuleNumberC0394(locationAddress.E2_CityInfo);
				}
			});
		}

		public void TestCheckE2_City_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "Z";
				locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_CityInfo.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestCheckE2_RN_NKCountryCode_Z_RuleC0394_WhenActive()
		{
			cusGoodsLocation.CGL_Qualifier = "Z";

			CombineAssertions(() =>
			{
				using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
				{
					testContext.EnableRule(r => r.IsRuleC0394Active);
					locationAddress.E2_RN_NKCountryCode = "DE";
					AssertNoMessageErrorContaining("When the country is filled, no error for mandatory fields", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);

					locationAddress.E2_RN_NKCountryCode = ZString.Empty;
					AssertHasMessageErrorContaining("The country should have a mandatory validation for qualifier Z", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);
					AssertHasMessageWithRuleNumberC0394(locationAddress.E2_RN_NKCountryCodeInfo);
				}
			});
		}

		public void TestCheckE2_RN_NKCountryCode_T_RuleC0394_WhenActive()
		{
			cusGoodsLocation.CGL_Qualifier = "T";

			CombineAssertions(() =>
			{
				using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
				{
					testContext.EnableRule(r => r.IsRuleC0394Active);
					locationAddress.E2_RN_NKCountryCode = "UA";
					AssertNoMessageErrorContaining("When the country is filled, no error for mandatory fields", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);

					locationAddress.E2_RN_NKCountryCode = ZString.Empty;
					AssertHasMessageErrorContaining("The country should have a mandatory validation for qualifier T", locationAddress.E2_RN_NKCountryCodeInfo, ErrorMessage);
					AssertHasMessageWithRuleNumberC0394(locationAddress.E2_RN_NKCountryCodeInfo);
				}
			});
		}

		public void TestCheckE2_RN_NKCountryCode_Z_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "Z";
				locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_RN_NKCountryCodeInfo.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestCheckE2_RN_NKCountryCode_T_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "T";
				locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_RN_NKCountryCodeInfo.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestCheckE2_PostCode_RuleC0394_WhenActive()
		{
			cusGoodsLocation.CGL_Qualifier = "T";

			CombineAssertions(() =>
			{
				using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
				{
					testContext.EnableRule(r => r.IsRuleC0394Active);

					locationAddress.E2_Postcode = "ABC";
					AssertNoMessageErrorContaining("When the country is filled, no error for mandatory fields", locationAddress.E2_PostcodeInfo, ErrorMessage);

					locationAddress.E2_Postcode = ZString.Empty;
					AssertHasMessageErrorContaining("The post code should have a mandatory validation for qualifier T", locationAddress.E2_PostcodeInfo, ErrorMessage);
					AssertHasMessageWithRuleNumberC0394(locationAddress.E2_PostcodeInfo);
				}
			});
		}
		public void TestCheckE2_PostCode_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "T";
				locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_PostcodeInfo.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestCheckE2_PostCode_RuleE1102Active_InPhase5TransitionPeriod()
		{
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102Active)))
			{
				const string e1102Msg = "[E1102] Postcode should be less than or equal to 9 Char.";

				cusGoodsLocationDeparture.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				CombineAssertions(() =>
				{
					using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, true))
					{
						locationAddressDeparture.E2_Postcode = "123456789";
						AssertNoMessageError("Length of E2_PostCode not greater than 9 char", locationAddressDeparture.E2_PostcodeInfo, e1102Msg);

						locationAddress.E2_Postcode = "1234567895";
						AssertNoMessageError("Length of E2_PostCode greater than 9 char but in Arrival", locationAddress.E2_PostcodeInfo, e1102Msg);

						locationAddressDeparture.E2_Postcode = "1234567893";
						AssertHasMessageError("Length of E2_PostCode greater than 9 char and In Phase5 Transition Period", locationAddressDeparture.E2_PostcodeInfo, e1102Msg);
					}
				});
			}
		}

		public void TestCheckE2_PostCode_RuleE1102Active_NotInPhase5TransitionPeriod()
		{
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1102Active)))
			{
				const string e1102Msg = "[E1102] Postcode should be less than or equal to 9 Char.";

				cusGoodsLocationDeparture.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, false))
				{
					locationAddressDeparture.E2_Postcode = "1234567890";
					AssertEquals("Length of E2_PostCode greater than 9 char but Not In Phase5 Transition Period", false, locationAddressDeparture.E2_PostcodeInfo.Notifications.Contains(e1102Msg));
				}
			}
		}

		public void TestCheckE2_PostCode_RuleE1102Inactive()
		{
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, true))
			{
				const string e1102Msg = "[E1102] Postcode should be less than or equal to 9 Char.";

				ValidationRuleConfigurationTestHelper.AssertNoNotificationsWithInactiveRule(Factory, locationAddressDeparture.E2_PostcodeInfo, e1102Msg, nameof(ValidationRuleConfiguration.IsRuleE1102Active),
				() =>
				{
					cusGoodsLocationDeparture.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
					locationAddressDeparture.E2_Postcode = "1234567894";
				}
			);
			}
		}

		public void TestCheckE2_Postcode_RuleE1102_1()
		{
			const string expectedWarningMessage = "Postcode is longer than 9 characters, it will be truncated in the message";
			cusGoodsLocationDeparture.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;

			var postcodeInfo = locationAddressDeparture.E2_PostcodeInfo;
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1102_1Active));

				using (SetTransitionPeriod(true))
				{
					locationAddressDeparture.E2_Postcode = "0123456789";
					AssertHasWarning("When E1102_1 Is Active and Transit period is on and Postcode length is greater than 9", postcodeInfo, expectedWarningMessage);

					locationAddressDeparture.E2_Postcode = "123456789";
					AssertNoWarning("When E1102_1 Is Active and Transit period is on and Postcode less than 10", postcodeInfo, expectedWarningMessage);

					locationAddressDeparture.E2_Postcode = "";
					AssertNoWarning("When E1102_1 Is Active and Transit period is on Postcode is Empty", postcodeInfo, expectedWarningMessage);
				}

				using (SetTransitionPeriod(false))
				{
					locationAddressDeparture.E2_Postcode = "0123456789";
					AssertNoWarning("When E1102_1 Is Active and Transit period is OFF and Postcode length is greater than 9", postcodeInfo, expectedWarningMessage);
				}

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1102_1Active));
				using (SetTransitionPeriod(true))
				{
					locationAddressDeparture.E2_Postcode = "0123456789";
					AssertNoWarning("When E1102_1 Is Disable and Transit period is on and Postcode length is greater than 9", postcodeInfo, expectedWarningMessage);
				}
			}
		}

		public void TestCheckE2_PostCode_C0505()
		{
			cusGoodsLocation.CGL_Qualifier = "Z";
			const string messageErrorPrefix = "C0505";

			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0505Active));
					locationAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
					locationAddress.E2_Postcode = ZString.Empty;

					locationAddress.Country.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
					locationAddress.Validation.ValidateE2_Postcode();
					AssertNoMessageErrorContaining("C0505 should not be shown", locationAddress.E2_PostcodeInfo, messageErrorPrefix);

					locationAddress.Country.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
					locationAddress.Validation.ValidateE2_Postcode();
					AssertHasMessageErrorContaining("The post code should not be null", locationAddress.E2_PostcodeInfo, messageErrorPrefix);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleC0505Active));
					locationAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
					locationAddress.Country.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
					locationAddress.E2_Postcode = ZString.Empty;
					locationAddress.Validation.ValidateE2_Postcode();
					AssertNoMessageErrorContaining("C0505 rule is not applied", locationAddress.E2_PostcodeInfo, messageErrorPrefix);
				}
			});
		}

		public void TestCheckE2_Latitude_RuleC0394_WhenActive() => CombineAssertions(() =>
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.EnableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "W";
				locationAddress.E2_Latitude = ZDecimal.Zero;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertHasMessageErrorContaining("CGL_Qualifier=W, Latitude=Zero", locationAddress.E2_LatitudeInfo, ErrorMessage);
				AssertHasMessageWithRuleNumberC0394(locationAddress.E2_LatitudeInfo);

				locationAddress.E2_Latitude = 1.2m;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertNoMessageErrorContaining("CGL_Qualifier=W, Latitude=1.2", locationAddress.E2_LatitudeInfo, ErrorMessage);

				locationAddress.E2_Latitude = ZDecimal.Zero;
				cusGoodsLocation.CGL_Qualifier = "Z";
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertNoMessageErrorContaining("CGL_Qualifier=Z, Latitude=Zero", locationAddress.E2_LatitudeInfo, ErrorMessage);
			}
		});

		public void TestCheckE2_Latitude_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "W";
				locationAddress.E2_Latitude = ZDecimal.Zero;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_LatitudeInfo.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestCheckE2_Longitude_RuleC0394_WhenActive() => CombineAssertions(() =>
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.EnableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "W";
				locationAddress.E2_Longitude = ZDecimal.Zero;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertHasMessageErrorContaining("CGL_Qualifier=W, Longitude=Zero", locationAddress.E2_LongitudeInfo, ErrorMessage);
				AssertHasMessageWithRuleNumberC0394(locationAddress.E2_LongitudeInfo);

				locationAddress.E2_Longitude = 1.2m;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertNoMessageErrorContaining("CGL_Qualifier=W, Longitude=1.2", locationAddress.E2_LongitudeInfo, ErrorMessage);

				locationAddress.E2_Longitude = ZDecimal.Zero;
				cusGoodsLocation.CGL_Qualifier = "Z";
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertNoMessageErrorContaining("CGL_Qualifier=Z, Longitude=Zero", locationAddress.E2_LongitudeInfo, ErrorMessage);
			}
		});

		public void TestCheckE2_Longitude_RuleC0394_WhenNotActive()
		{
			using (var testContext = new CusGoodsLocationValidationDeciderTestContext<IArrivalPhase5CusGoodsLocationValidationDecider>(Factory))
			{
				testContext.DisableRule(r => r.IsRuleC0394Active);

				cusGoodsLocation.CGL_Qualifier = "W";
				locationAddress.E2_Latitude = ZDecimal.Zero;
				locationAddress.Validation.ValidateE2_GeoLocation();
				AssertCollectionNotContains($"Expected notifications would not contain {ValidationRuleCodeConstants.C0394}", locationAddress.E2_LongitudeInfo.Notifications.Select(e => e.Message), x => x.Contains(ValidationRuleCodeConstants.C0394));
			}
		}

		public void TestRuleNR0022()
		{
			cusGoodsLocation.CGL_Qualifier = "Y";
			cusGoodsLocationDeparture.CGL_Qualifier = "Y";
			cusGoodsLocation.CGL_AdditionalIdentifier = new string('b', 5);
			cusGoodsLocationDeparture.CGL_AdditionalIdentifier = new string('b', 5);

			const string expectedErrorMessage = "[NR0022] The sum of [(Authorization Number) + (Place Code) must be less than or equal to 34 char.";

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleNR0022Active)))
			{
				CombineAssertions(() =>
				{
					locationAddress.AuthorisationNumber = new string('a', 30);
					AssertNoErrorContaining("Phase 5, arrival", locationAddress.AuthorisationNumberInfo, expectedErrorMessage);

					locationAddressDeparture.AuthorisationNumber = new string('a', 28);
					AssertNoErrorContaining("Phase 5 departure but Combined length less than 34", locationAddressDeparture.AuthorisationNumberInfo, expectedErrorMessage);

					locationAddressDeparture.AuthorisationNumber = new string('a', 29);
					AssertNoErrorContaining("Phase 5 departure and Combined length is equal to 34", locationAddressDeparture.AuthorisationNumberInfo, expectedErrorMessage);

					locationAddressDeparture.AuthorisationNumber = new string('a', 30);
					AssertHasMessageErrorContaining("Phase 5 departure and Combined length is more than 34", locationAddressDeparture.AuthorisationNumberInfo, expectedErrorMessage);
				});
			}
		}

		public void TestCheckE2_Address1AndE2_Address2_RuleE1104_1()
		{
			using (SetTransitionPeriod(true))
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));

					cusGoodsLocationDeparture.CGL_Qualifier = "Z";
					AssertHasWarningWhenMaxLengthExceeded(locationAddressDeparture);

					cusGoodsLocation.CGL_Qualifier = "Z";
					AssertNoWarningWhenMaxLengthExceeded(locationAddress, reason: "not Departure");

					cusGoodsLocationDeparture.CGL_Qualifier = "W";
					AssertNoWarningWhenMaxLengthExceeded(locationAddressDeparture, reason: "qualifier is not Z");

					headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertNoWarningWhenMaxLengthExceeded(locationAddressDeparture, reason: "header is not Phase 5");

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
					cusGoodsLocationDeparture.CGL_Qualifier = "Z";
					AssertNoWarningWhenMaxLengthExceeded(locationAddressDeparture, reason: "Rule E1104_1 is disabled");
				}
			}

			using (SetTransitionPeriod(false))
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1104_1Active));
					cusGoodsLocationDeparture.CGL_Qualifier = "Z";
					AssertNoWarningWhenMaxLengthExceeded(locationAddressDeparture, reason: "Not in transition period");
				}
			}
		}

		public void TestAuthorizationNumberEditable()
		{
			var holder = Factory.NewWithValidTestData<OrgHeader>();
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.Address.IdentificationHolderPK = ZGuid.Empty;
			AssertEquals("AuthorizationNumber should be editable when CGL_Qualifier is 'Y' and IdentificationHolderPK is empty.",
				false,
				locationAddress.AuthorisationNumberInfo.ReadOnly);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertEquals("AuthorisationNumber readonly when CGL_Qualifier not Y",
				true,
				locationAddress.AuthorisationNumberInfo.ReadOnly);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.Address.IdentificationHolderPK = holder.PK;
			AssertEquals("AuthorizationNumber should be editable when CGL_Qualifier is 'Y' and IdentificationHolderPK is Valid.",
				false,
				locationAddress.AuthorisationNumberInfo.ReadOnly);
		}

		void AssertNoWarningWhenMaxLengthExceeded(CusGoodsLocationAddress locationAddress, string reason)
		{
			CombineAssertions(() =>
			{
				locationAddress.E2_Address1AndE2_Address2 = new string('a', 10);

				AssertNoWarning("When street and number field length is less than maximum allowed, no warning is expected", locationAddress.E2_Address1AndE2_Address2Info, StreetAndNumberExceedsMaximumAllowedLength);

				locationAddress.E2_Address1AndE2_Address2 = new string('a', 36);
				AssertNoWarning($"When {reason}, no warning is expected", locationAddress.E2_Address1AndE2_Address2Info, StreetAndNumberExceedsMaximumAllowedLength);
			});
		}

		void AssertHasWarningWhenMaxLengthExceeded(CusGoodsLocationAddress locationAddress)
		{
			CombineAssertions(() =>
			{
				locationAddress.E2_Address1AndE2_Address2 = new string('a', 10);

				AssertNoWarning("When street and number field length is less than maximum allowed, no warning is expected", locationAddress.E2_Address1AndE2_Address2Info, StreetAndNumberExceedsMaximumAllowedLength);

				locationAddress.E2_Address1AndE2_Address2 = new string('a', 36);
				AssertHasWarning("When Street and Number field exceeds the maximum allowed length, warning is expected", locationAddress.E2_Address1AndE2_Address2Info, StreetAndNumberExceedsMaximumAllowedLength);
			});
		}

		void AssertHasMessageWithRuleNumberC0394(ZPropertyInfo targetPropertyInfo)
		{
			AssertHasMessageErrorContaining("Rule \"C0394\" text Must be present", targetPropertyInfo, ValidationRuleCodeConstants.C0394);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_LocationUse = "ARR";
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;
			cusGoodsLocation.Parent = movementHeader;
			locationAddress = cusGoodsLocation.Address;

			cusGoodsLocationDeparture = Factory.New<CusGoodsLocation>();
			cusGoodsLocationDeparture.CGL_LocationUse = "ARR";
			headerDeparture = Factory.NewWithValidTestData<NctsHeader>();
			headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeaderDeparture = headerDeparture.MovementHeader;
			cusGoodsLocationDeparture.Parent = movementHeaderDeparture;
			locationAddressDeparture = cusGoodsLocationDeparture.Address;
		}

		IDisposable SetTransitionPeriod(bool value) => Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, value);

		CusGoodsLocation cusGoodsLocation;
		CusGoodsLocationAddress locationAddress;
		CusGoodsLocation cusGoodsLocationDeparture;
		CusGoodsLocationAddress locationAddressDeparture;
		NctsHeader headerDeparture;

		const string StreetAndNumberExceedsMaximumAllowedLength = "[E1104-1] Location: Street + Number exceeds the maximum allowed length in the declaration message (35 characters). Excess characters will be truncated.";
	}
}
