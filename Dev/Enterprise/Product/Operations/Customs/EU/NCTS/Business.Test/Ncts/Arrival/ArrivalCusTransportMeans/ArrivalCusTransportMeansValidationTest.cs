using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class ArrivalCusTransportMeansValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTPM_TransportState_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(arrivalCusTransportMeans.TPM_TransportStateInfo);
		}

		public void TestCheckTPM_TransportState_New()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Should be NEW at start", "NEW", arrivalCusTransportMeans.TPM_TransportState);
				AssertNoErrorContaining("Being NEW at start shouldn't give an error", arrivalCusTransportMeans.TPM_TransportStateInfo, ListValidation.InvalidCodeError);
				Factory.Save();
				arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
				AssertNoErrorContaining("Changing to NEW shouldn't give an error when the previous value was NEW", arrivalCusTransportMeans.TPM_TransportStateInfo, ListValidation.InvalidCodeError);
				arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
				Factory.Save();
				arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
				AssertHasErrorContaining("Switching from a non NEW value to NEW should give an error", arrivalCusTransportMeans.TPM_TransportStateInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckTPM_TransportState_List()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(arrivalCusTransportMeans.TPM_TransportStateInfo, NctsUnloadedStateList.Codes.DIF, NctsUnloadedStateList.Codes.MIS);

			ValidationTestHelper.AssertErrorIfInvalidCode(arrivalCusTransportMeans.TPM_TransportStateInfo, NctsUnloadedStateList.Codes.DAM, NctsUnloadedStateList.Codes.DEC);
		}

		public void TestCheckTPM_TypeOfIdentification()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(arrivalCusTransportMeans.TPM_TypeOfIdentificationInfo, "1", NctsTransportTypeOfIdList.Codes._10);
		}

		public void TestCheckTPM_RN_NKTransportNationality()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
			Factory.Save();

			ValidationTestHelper.AssertErrorIfInvalidCode(arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo, "XX", "DE");
		}

		public void TestCheckRuleNR0081()
		{
			const string errorMessage = "[NR0081] If one Transport at Departure is reported as missing, at least one additional transport must be captured with unloaded state 'NEW'.";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var transportMeans = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			var newTransportMeans = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new ArrivalCusTransportMeansValidationDeciderTestContext<IArrivalCusTransportMeansPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(x => x.IsRuleNR0081Active);

					transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
					newTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
					transportMeans.TPM_TypeOfIdentification = "20";
					transportMeans.Validation.ValidateAll();
					AssertHasRowMessageError(AssertionMessage(), transportMeans, errorMessage);

					transportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._21;
					transportMeans.Validation.ValidateAll();
					AssertHasRowMessageError(AssertionMessage(), transportMeans, errorMessage);

					deciderTestContext.DisableRule(x => x.IsRuleNR0081Active);
					transportMeans.Validation.ValidateAll();
					AssertNoRowMessageError("NR0081 inactive", transportMeans, errorMessage);

					deciderTestContext.EnableRule(x => x.IsRuleNR0081Active);

					transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
					transportMeans.Validation.ValidateAll();
					AssertNoRowMessageError("NR0081 TPM_TransportState = NEW,Rule Active, TypeOFIDentification =21", transportMeans, errorMessage);

					transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
					newTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
					transportMeans.Validation.ValidateAll();
					AssertNoRowMessageError(AssertionMessage(), transportMeans, errorMessage);

					transportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._20;
					transportMeans.Validation.ValidateAll();
					AssertNoRowMessageError("NR0081 TPM_TransportState = NEW,Rule Active, TypeOFIDentification =20", transportMeans, errorMessage);
				}

				string AssertionMessage() => $"TPM_TransporState={nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.GetFieldValues("TPM_TransportState")}, TPM_TypeOfIdentification={nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.GetFieldValues("TPM_TypeOfIdentification")}";
			});
		}

		public void TestCheckRuleNR0082()
		{
			const string errorMessage = "[NR0082] State 'DIF' requires at least one value to be captured that is different to the declared value in the departure declaration. Unchanged columns must remain empty.";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var transportMeans = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new ArrivalCusTransportMeansValidationDeciderTestContext<IArrivalCusTransportMeansPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(x => x.IsRuleNR0082Active);

					transportMeans.TPM_TypeOfIdentification = ZString.Empty;
					transportMeans.TPM_IdentificationNumber = ZString.Empty;
					transportMeans.TPM_RN_NKTransportNationality = ZString.Empty;
					transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
					AssertHasMessageError(AssertionMessage(), transportMeans.TPM_TransportStateInfo, errorMessage);

					deciderTestContext.DisableRule(x => x.IsRuleNR0082Active);
					transportMeans.Validation.ValidateTPM_TransportState();
					AssertNoMessageError("NR0082 inactive", transportMeans.TPM_TransportStateInfo, errorMessage);

					deciderTestContext.EnableRule(x => x.IsRuleNR0082Active);

					transportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._21;
					transportMeans.Validation.ValidateTPM_TransportState();
					AssertNoMessageError(AssertionMessage(), transportMeans.TPM_TransportStateInfo, errorMessage);

					transportMeans.TPM_TypeOfIdentification = ZString.Empty;
					transportMeans.TPM_IdentificationNumber = "ABc";
					transportMeans.Validation.ValidateTPM_TransportState();
					AssertNoMessageError(AssertionMessage(), transportMeans.TPM_TransportStateInfo, errorMessage);

					transportMeans.TPM_IdentificationNumber = ZString.Empty;
					transportMeans.TPM_RN_NKTransportNationality = "DE";
					transportMeans.Validation.ValidateTPM_TransportState();
					AssertNoMessageError(AssertionMessage(), transportMeans.TPM_TransportStateInfo, errorMessage);

					string AssertionMessage() => $"TPM_TransporState={transportMeans.TPM_TransportState}, TPM_TypeOfIdentification={transportMeans.TPM_TypeOfIdentification} , TPM_IdentificationNumber={transportMeans.TPM_IdentificationNumber}, TPM_RN_NKTransportNationality={transportMeans.TPM_RN_NKTransportNationality}";
				}
			});
		}

		public void TestCheckTPM_TypeOfIdentification_RuleCodeTR0036()
		{
			var messageError = "[TR0036] You have not entered a Type of Identification.";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var transportInfoForMovementHeader = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0036Active));

					transportInfoForMovementHeader.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
					ValidationTestHelper.AssertFieldIsNotMandatory(transportInfoForMovementHeader.TPM_TypeOfIdentificationInfo, messageError, "Rule TR0036 active, TypeOfIdentification is not mandatory for Transport State 'DIF'");

					transportInfoForMovementHeader.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportInfoForMovementHeader.TPM_TypeOfIdentificationInfo, messageError, "Rule TR0036 active, TypeOfIdentification is mandatory for Transport State 'NEW'");

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0036Active));
					transportInfoForMovementHeader.TPM_TypeOfIdentification = ZString.Empty;
					AssertNoMessageError("Rule TR0036 inactive, TypeOfIdentification is empty for Transport State 'NEW'", transportInfoForMovementHeader.TPM_TypeOfIdentificationInfo, messageError);
				}
			});
		}

		public void TestCheckTPM_IdentificationNumber_RuleCodeTR0037()
		{
			var messageError = "[TR0037] You have not entered a Transport Identification.";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var transportInfoForMovementHeader = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0037Active));

					transportInfoForMovementHeader.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
					ValidationTestHelper.AssertFieldIsNotMandatory(transportInfoForMovementHeader.TPM_IdentificationNumberInfo, messageError, "Rule TR0037 active, IdentificationNumber is not mandatory for Transport State 'DIF'");

					transportInfoForMovementHeader.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(transportInfoForMovementHeader.TPM_IdentificationNumberInfo, messageError, "Rule TR0037 active, IdentificationNumber is mandatory for Transport State 'NEW'");

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0037Active));
					transportInfoForMovementHeader.TPM_IdentificationNumber = ZString.Empty;
					AssertNoMessageError("Rule TR0037 inactive, IdentificationNumber is empty for Transport State 'NEW'", transportInfoForMovementHeader.TPM_IdentificationNumberInfo, messageError);
				}
			});
		}

		public void TestCheckTPM_RN_NKTransportNationality_RuleCodeTR0038()
		{
			var messageError = "[TR0038] You have not entered a Nationality.";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0038Active));
					arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
					ValidationTestHelper.AssertFieldIsNotMandatory(arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo, messageError, "Rule TR0038 active, TransportNationality is not mandatory for Transport State 'DIF'");

					arrivalCusTransportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo, messageError, "Rule TR0038 active, TransportNationality is mandatory for Transport State 'NEW'");

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0038Active));
					arrivalCusTransportMeans.TPM_RN_NKTransportNationality = ZString.Empty;
					AssertNoMessageError("Rule TR0038 inactive, TransportNationality is empty for Transport State 'NEW'", arrivalCusTransportMeans.TPM_RN_NKTransportNationalityInfo, messageError);
				}
			});
		}

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalCusTransportMeans = nctsHeader.Bills.AddNew().ArrivalTransportInfos.AddNew();
		}
		NctsHeader nctsHeader;
		ArrivalCusTransportMeans arrivalCusTransportMeans;
	}
}
