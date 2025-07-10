using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class DepartureCusTransportMeansValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTPM_TypeOfIdentification_InvalidCode()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(borderTransportMeans.TPM_TypeOfIdentificationInfo, "AA", NctsTransportTypeOfIdList.Codes._10);
		}

		public void TestCheckTPM_IdentificationNumber_Mandatory()
		{
			var targetInfo = borderTransportMeans.TPM_IdentificationNumberInfo;
			CombineAssertions(() =>
			{
				borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

				borderTransportMeans.TPM_TypeOfIdentification = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			});
		}

		public void TestCheckTPM_IdentificationNumber_LowercaseAllowed()
		{
			const string message = "Must not contain lower case letters.";
			var transportTypesRequiringUpperCaseIDs = new HashSet<string>(new[]
			{
				NctsTransportTypeOfIdList.Codes._10,
				NctsTransportTypeOfIdList.Codes._21,
				NctsTransportTypeOfIdList.Codes._30,
				NctsTransportTypeOfIdList.Codes._40,
				NctsTransportTypeOfIdList.Codes._41,
				NctsTransportTypeOfIdList.Codes._80
			});
			var targetInfo = borderTransportMeans.TPM_IdentificationNumberInfo;
			CombineAssertions(() =>
			{
				borderTransportMeans.TPM_IdentificationNumber = "ABc";
				AssertNoMessageError("BM_ActiveBorderIdentificationType empty, BM_TOLCarrierID has lower case letters", targetInfo, message);

				foreach (var transportType in new NctsTransportTypeOfIdList().GetAllCodes())
				{
					borderTransportMeans.TPM_TypeOfIdentification = transportType;

					borderTransportMeans.TPM_IdentificationNumber = "XyZ123";
					if (transportTypesRequiringUpperCaseIDs.Contains(transportType))
					{
						AssertHasMessageError($"TPM_TypeOfIdentification '{transportType}', TPM_IdentificationNumber has lower case letters", targetInfo, message);
					}
					else
					{
						AssertNoMessageError($"TPM_TypeOfIdentification '{transportType}', TPM_IdentificationNumber has lower case letters", targetInfo, message);
					}

					borderTransportMeans.TPM_IdentificationNumber = "XYZ123";
					AssertNoMessageError($"TPM_TypeOfIdentification '{transportType}', TPM_IdentificationNumber doesn't have lower case letters", targetInfo, message);
				}
			});
		}

		public void TestCheckTPM_IdentificationNumber_MandatoryForAdditionalWagons()
		{
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			bill.DepartureTransportInfos.AddNew();
			var secondWagon = bill.TransportDepartureAdditionalWagonNumbers.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(secondWagon.TPM_IdentificationNumberInfo);
		}

		public void TestCheckTPM_RN_NKTransportNationality_Mandatory()
		{
			var targetInfo = borderTransportMeans.TPM_RN_NKTransportNationalityInfo;
			borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckTPM_RN_NKTransportNationality_MandatoryForIDNumber()
		{
			var targetInfo = borderTransportMeans.TPM_RN_NKTransportNationalityInfo;
			borderTransportMeans.TPM_IdentificationNumber = "12345";

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckTPM_RN_NKTransportNationality_NotMandatoryForIDNumber_Departure()
		{
			var targetInfo = departureTransportMeans.TPM_RN_NKTransportNationalityInfo;
			departureTransportMeans.TPM_IdentificationNumber = "12345";

			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
		}

		public void TestCheckTPM_RN_NKTransportNationality_NotMandatory()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(borderTransportMeans.TPM_RN_NKTransportNationalityInfo);
		}

		public void TestCheckTPM_RN_NKTransportNationality_ValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
			Factory.Save();

			borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;

			ValidationTestHelper.AssertInvalidCodeMessageError(borderTransportMeans.TPM_RN_NKTransportNationalityInfo, "XX", "DE");
		}

		public void TestValidateTPM_RN_NKTransportNationality_WithRuleTR0078()
		{
			const string expectedErrorMessage = "[TR0078] You have not entered a Nationality";

			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			bill.TransportDepartureAdditionalWagonNumbers.AddNew();
			var secondWagon = bill.TransportDepartureAdditionalWagonNumbers.AddNew();

			using var ruleTestContext = new CusTransportMeansValidationDeciderTestContext<IDepartureCusTransportMeansPhase5ValidationDecider>(Factory);
			ruleTestContext.ClearCachedValidationDecider(borderTransportMeans);
			ruleTestContext.EnableRule(x => x.IsRuleTR0078Active);
			CombineAssertions(() =>
			{
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					secondWagon.WagonNumber = "12";
					secondWagon.WagonNationality = "IN";
					AssertNoMessageError("Additional wagon number is present and nationality is filled", secondWagon.WagonNationalityInfo, expectedErrorMessage);

					secondWagon.WagonNationality = ZString.Empty;
					AssertHasMessageError("Additional wagon number is present but nationality is empty", secondWagon.WagonNationalityInfo, expectedErrorMessage);
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					secondWagon.WagonNumber = "12";
					secondWagon.Validation.ValidateTPM_RN_NKTransportNationality();
					AssertNoMessageError("Additional wagon number is present but nationality is empty", secondWagon.WagonNationalityInfo, expectedErrorMessage);
				}

				ruleTestContext.DisableRule(x => x.IsRuleTR0078Active);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					secondWagon.Validation.ValidateTPM_RN_NKTransportNationality();
					AssertNoMessageError("Additional wagon number is present but nationality is empty", secondWagon.WagonNationalityInfo, expectedErrorMessage);
				}
			});
		}

		public void TestCheckBM_CustomsOfficeAtBorder_Mandatory()
		{
			borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._40;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(borderTransportMeans.TPM_CustomsOfficeInfo);
			borderTransportMeans.TPM_TypeOfIdentification = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(borderTransportMeans.TPM_CustomsOfficeInfo);
		}

		public void TestActiveBorderIdentificationTypeOnHeaderRequired()
		{
			var message = "Enter the first Transport in the fields of Transport Border";
			departureMovement.BM_ExportTransportMode = EU.Business.ModeOfTransportList.Codes._4_AirTransport;

			CombineAssertions(() =>
			{
				AssertHasRowError(borderTransportMeans, message);

				departureMovement.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
				borderTransportMeans = departureMovement.AdditionalTransportAtBorderList.AddNew(); // the old record is deleted
				AssertNoRowError(borderTransportMeans, message);
			});
		}

		public void TestCheckTPM_ReferenceNumber()
		{
			departureMovement.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			borderTransportMeans = departureMovement.AdditionalTransportAtBorderList.AddNew(); // the old record is deleted
			borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._40;
			departureMovement.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			const string message = "You have not entered a Conveyance Number.";

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(borderTransportMeans.TPM_ReferenceNumberInfo, message, "TPM_ReferenceNumber 40");
				borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
				ValidationTestHelper.AssertFieldIsNotMandatory(borderTransportMeans.TPM_ReferenceNumberInfo, message, "TPM_ReferenceNumber 10");
			});
		}

		public void TestCheckTPM_TypeOfIdentification_WhenRuleB2101()
		{
			const string expectedErrorMessage = "[B2101] You have not entered a Type of Identification.";
			var typeOfIdentificationInfo = borderTransportMeans.TPM_TypeOfIdentificationInfo;

			using var deciderTestContext = new CusTransportMeansValidationDeciderTestContext<IDepartureCusTransportMeansPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(borderTransportMeans);
			deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
				{
					borderTransportMeans.TPM_TypeOfIdentification = ZString.Empty;
					borderTransportMeans.Validation.ValidateTPM_TypeOfIdentification();
					AssertHasMessageError("TransportBorder>More>'Type Of ID' is empty", typeOfIdentificationInfo, expectedErrorMessage);

					borderTransportMeans.TPM_TypeOfIdentification = "AB";
					AssertNoMessageError("TransportBorder>More>'Type Of ID' is NOT empty", typeOfIdentificationInfo, expectedErrorMessage);
				});
			}

			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
			{
				borderTransportMeans.TPM_TypeOfIdentification = ZString.Empty;
				AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>More>'Type Of ID' is empty", typeOfIdentificationInfo, expectedErrorMessage);
			}

			deciderTestContext.DisableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				borderTransportMeans.Validation.ValidateTPM_TypeOfIdentification();
				AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>More>'Type Of ID' is empty", typeOfIdentificationInfo, expectedErrorMessage);
			}
		}

		public void TestCheckTPM_IdentificationNumber_WhenRuleB2101()
		{
			const string expectedErrorMessage = "[B2101] You have not entered a Transport Identification.";
			var identificationNumberInfo = borderTransportMeans.TPM_IdentificationNumberInfo;

			using var deciderTestContext = new CusTransportMeansValidationDeciderTestContext<IDepartureCusTransportMeansPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(borderTransportMeans);
			deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
				{
					borderTransportMeans.TPM_IdentificationNumber = ZString.Empty;
					borderTransportMeans.Validation.ValidateTPM_IdentificationNumber();
					AssertHasMessageError("TransportBorder>More>'Transport ID' is empty", identificationNumberInfo, expectedErrorMessage);

					borderTransportMeans.TPM_IdentificationNumber = "AB";
					AssertNoMessageError("TransportBorder>More>'Transport ID' is NOT empty", identificationNumberInfo, expectedErrorMessage);
				});
			}

			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
			{
				borderTransportMeans.TPM_IdentificationNumber = ZString.Empty;
				AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>More>'Transport ID' is empty", identificationNumberInfo, expectedErrorMessage);
			}

			deciderTestContext.DisableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				borderTransportMeans.Validation.ValidateTPM_IdentificationNumber();
				AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>More>'Transport ID' is empty", identificationNumberInfo, expectedErrorMessage);
			}
		}

		public void TestCheckTPM_RN_NKTransportNationality_WhenRuleB2101()
		{
			const string expectedErrorMessage = "[B2101] You have not entered a Nationality.";
			var nationalityInfo = borderTransportMeans.TPM_RN_NKTransportNationalityInfo;

			using var deciderTestContext = new CusTransportMeansValidationDeciderTestContext<IDepartureCusTransportMeansPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(borderTransportMeans);
			deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
				{
					borderTransportMeans.TPM_RN_NKTransportNationality = ZString.Empty;
					borderTransportMeans.Validation.ValidateTPM_RN_NKTransportNationality();
					AssertHasMessageError("TransportBorder>More>'Nationality' is empty", nationalityInfo, expectedErrorMessage);

					borderTransportMeans.TPM_RN_NKTransportNationality = "AB";
					AssertNoMessageError("TransportBorder>More>'Nationality' is NOT empty", nationalityInfo, expectedErrorMessage);
				});
			}

			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
			{
				borderTransportMeans.TPM_RN_NKTransportNationality = ZString.Empty;
				AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>More>'Nationality' is empty", nationalityInfo, expectedErrorMessage);
			}

			deciderTestContext.DisableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				borderTransportMeans.Validation.ValidateTPM_RN_NKTransportNationality();
				AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>More>'Nationality' is empty", nationalityInfo, expectedErrorMessage);
			}
		}

		public void TestCheckTPM_CustomsOffice_WhenRuleB2101()
		{
			const string expectedErrorMessage = "[B2101] You have not entered a Customs Office.";
			var customsOfficeInfo = borderTransportMeans.TPM_CustomsOfficeInfo;

			using var deciderTestContext = new CusTransportMeansValidationDeciderTestContext<IDepartureCusTransportMeansPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(borderTransportMeans);
			deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
				{
					borderTransportMeans.TPM_CustomsOffice = ZString.Empty;
					borderTransportMeans.Validation.ValidateTPM_CustomsOffice();
					AssertHasMessageError("TransportBorder>More>'Customs Office' is empty", customsOfficeInfo, expectedErrorMessage);

					borderTransportMeans.TPM_CustomsOffice = "AB";
					AssertNoMessageError("TransportBorder>More>'Customs Office' is NOT empty", customsOfficeInfo, expectedErrorMessage);
				});
			}

			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
			{
				borderTransportMeans.TPM_CustomsOffice = ZString.Empty;
				AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>More>'Customs Office' is empty", customsOfficeInfo, expectedErrorMessage);
			}

			deciderTestContext.DisableRule(x => x.IsRuleB2101Active);
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				borderTransportMeans.Validation.ValidateTPM_CustomsOffice();
				AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>More>'Customs Office' is empty", customsOfficeInfo, expectedErrorMessage);
			}
		}

		public void TestCheckTPM_CustomsOffice_WhenRuleG0789()
		{
			const string expectedMessageError = "[G0789-1] Customs Office at Border should be the same as Customs Office of Transit, or Customs office of Exit, or Customs Office of Destination.";
			var customsOfficeInfo = borderTransportMeans.TPM_CustomsOfficeInfo;
			nctsHeader.MovementHeader.CustomsOffices.RemoveAndDeleteAll();

			using var deciderTestContext = new CusTransportMeansValidationDeciderTestContext<IDepartureCusTransportMeansPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(borderTransportMeans);
			deciderTestContext.EnableRule(x => x.IsRuleG0789_1Active);
			CombineAssertions("When Rule enabled", () =>
			{
				borderTransportMeans.TPM_CustomsOffice = "";
				borderTransportMeans.Validation.ValidateTPM_CustomsOffice();
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder = '' and all other offices are not defined", customsOfficeInfo, expectedMessageError);

				borderTransportMeans.TPM_CustomsOffice = "IT444444";
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and all other offices are not defined", customsOfficeInfo, expectedMessageError);

				var transitCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				var exitCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
				var destinationCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

				borderTransportMeans.TPM_CustomsOffice = "";
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder = '' and all other offices are defined but empty", customsOfficeInfo, expectedMessageError);

				borderTransportMeans.TPM_CustomsOffice = "IT444444";
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and all other offices are defined but empty", customsOfficeInfo, expectedMessageError);

				transitCustomsOffice.CY_Data = "IT111111";
				exitCustomsOffice.CY_Data = "IT222222";
				destinationCustomsOffice.CY_Data = "IT333333";

				borderTransportMeans.TPM_CustomsOffice = "";
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder = '' and all other offices are populated", customsOfficeInfo, expectedMessageError);

				borderTransportMeans.TPM_CustomsOffice = "IT444444";
				AssertHasMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and different to other offices", customsOfficeInfo, expectedMessageError);

				borderTransportMeans.TPM_CustomsOffice = "IT111111";
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and equal to office of transit", customsOfficeInfo, expectedMessageError);

				borderTransportMeans.TPM_CustomsOffice = "IT222222";
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and equal to office of exit", customsOfficeInfo, expectedMessageError);

				borderTransportMeans.TPM_CustomsOffice = "IT333333";
				AssertNoMessageErrorContaining("When BM_CustomsOfficeAtBorder is filled and equal to office of destination", customsOfficeInfo, expectedMessageError);
			});

			deciderTestContext.DisableRule(x => x.IsRuleG0789_1Active);
			borderTransportMeans.TPM_CustomsOffice = "IT444444";
			AssertNoMessageErrorContaining("When Rule disabled, BM_CustomsOfficeAtBorder is filled and different to other offices", customsOfficeInfo, expectedMessageError);
		}

		public void TestValidateAll_R0789_2()
		{
			CombineAssertions(() =>
			{
				const string expectedMessageError = "[R0789-2] If a Customs Office of Transit is not present then no additional Active Border Transport Means are allowed";

				departureTransportMeans.Validation.ValidateAll();
				AssertNoRowMessageError("No TRA office and at Bill level", departureTransportMeans, expectedMessageError);

				borderTransportMeans.Validation.ValidateAll();
				AssertHasRowMessageError("No TRA office and at MovementHeader level", borderTransportMeans, expectedMessageError);

				departureMovement.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "AT275100");
				borderTransportMeans.Validation.ValidateAll();
				AssertNoRowMessageError("Has TRA office and at MovementHeader level", borderTransportMeans, expectedMessageError);

				departureTransportMeans.Validation.ValidateAll();
				AssertNoRowMessageError("Has TRA office and at Bill level", departureTransportMeans, expectedMessageError);
			});
		}

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			bill = nctsHeader.Bills.AddNew();
			departureMovement = nctsHeader.MovementHeader;
			borderTransportMeans = departureMovement.AdditionalTransportAtBorderList.AddNew();
			departureTransportMeans = bill.DepartureTransportInfos.AddNew();
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);

		DepartureCusTransportMeans borderTransportMeans;
		DepartureCusTransportMeans departureTransportMeans;
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
		NctsBill bill;
	}
}
