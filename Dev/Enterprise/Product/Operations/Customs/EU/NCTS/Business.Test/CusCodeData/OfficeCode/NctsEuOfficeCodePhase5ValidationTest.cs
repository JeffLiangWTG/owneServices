using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsEuOfficeCodePhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsPhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
			Assert("Phase5", customsOffice.IsPhase5);

			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			customsOffice = header.CustomsOffices.AddNew();
			Assert("Phase4", !customsOffice.IsPhase5);
		}

		public void TestCheckCY_Data_DestinationCustomsOfficeForArrival()
		{
			var arrival = Factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);

			var arrivalMovementHeader = arrival.ArrivalMovementHeader;
			arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = ZString.Empty;
			var destinationCustomsOffice = (NctsEuOfficeCode)arrivalMovementHeader.DestinationCustomsOffice;
			var message = MandatoryValidation.YouHaveNotEnteredMessage(arrivalMovementHeader.DestinationCustomsOfficeCodeForArrivalInfo.HumanReadableName);

			CombineAssertions(() =>
			{
				destinationCustomsOffice.Validation.ValidateCY_Data();
				AssertHasMessageError("Destination Office is empty", destinationCustomsOffice.CY_DataInfo, message);
				destinationCustomsOffice.CY_Data = "FR000010";
				AssertNoMessageError("Destination Office is entered", destinationCustomsOffice.CY_DataInfo, message);
			});
		}

		public void TestCheckCY_Code_Max9TXT()
		{
			const string messageError = "[TR0003] The maximum number of 9 Customs Offices with Purpose 'TXT' has exceeded.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0003Active));

				var movementHeader = header.MovementHeader;
				movementHeader.CustomsOffices.RemoveAndDeleteAll();
				for (var i = 0; i < 8; i++)
				{
					movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
				}

				CombineAssertions(() =>
				{
					var officeTXT9th = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
					officeTXT9th.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'TXT' #9", officeTXT9th.CY_CodeInfo, messageError);

					var officeTXT10th = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
					officeTXT10th.Validation.ValidateCY_Code();
					AssertHasMessageError("Customs Office 'TXT' #10", officeTXT10th.CY_CodeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0003Active));
					ClearOtherRequirementsCache();
					officeTXT10th.Validation.ValidateCY_Code();
					AssertNoMessageError("Rule TR0003 is disabled", officeTXT10th.CY_CodeInfo, messageError);
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0003Active));
					ClearOtherRequirementsCache();

					var officeDES = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
					officeDES.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'DES'", officeDES.CY_CodeInfo, messageError);
				});
			}
		}

		public void TestCheckCY_Code_Max9TRA()
		{
			const string messageError = "[TR0002] The maximum number of 9 Customs Offices with Purpose 'TRA' has exceeded.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0002Active));

				var movementHeader = header.MovementHeader;
				movementHeader.CustomsOffices.RemoveAndDeleteAll();
				for (var i = 0; i < 8; i++)
				{
					movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				}

				CombineAssertions(() =>
				{
					var officeTRA9th = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					officeTRA9th.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'TRA' #9", officeTRA9th.CY_CodeInfo, messageError);

					var officeTRA10th = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					officeTRA10th.Validation.ValidateCY_Code();
					AssertHasMessageError("Customs Office 'TRA' #10", officeTRA10th.CY_CodeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0002Active));
					ClearOtherRequirementsCache();
					officeTRA10th.Validation.ValidateCY_Code();
					AssertNoMessageError("Rule TR0002 is disabled", officeTRA10th.CY_CodeInfo, messageError);
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0002Active));
					ClearOtherRequirementsCache();

					var officeDES = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
					officeDES.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'DES'", officeDES.CY_CodeInfo, messageError);
				});
			}
		}

		public void TestCheckCY_Code_Max1DES()
		{
			const string messageError = "[TR0008] The maximum number of 1 Customs Office with Purpose 'DES' has exceeded.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0008Active));

				var movementHeader = header.MovementHeader;
				movementHeader.CustomsOffices.RemoveAndDeleteAll();

				CombineAssertions(() =>
				{
					var officeDES1st = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
					officeDES1st.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'DES' #1", officeDES1st.CY_CodeInfo, messageError);

					var officeDES2nd = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
					officeDES2nd.Validation.ValidateCY_Code();
					AssertHasMessageError("Customs Office 'DES' #2", officeDES2nd.CY_CodeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0008Active));
					ClearOtherRequirementsCache();
					officeDES2nd.Validation.ValidateCY_Code();
					AssertNoMessageError("Rule TR0008 is disabled", officeDES2nd.CY_CodeInfo, messageError);
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0008Active));
					ClearOtherRequirementsCache();

					var officeTRA = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					officeTRA.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'TRA'", officeTRA.CY_CodeInfo, messageError);
				});
			}
		}

		public void TestCheckCY_Code_Max1DEP()
		{
			const string messageError = "[TR0009] The maximum number of 1 Customs Office with Purpose 'DEP' has exceeded.";

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0009Active));

				var movementHeader = header.MovementHeader;
				movementHeader.CustomsOffices.RemoveAndDeleteAll();

				CombineAssertions(() =>
				{
					var officeDEP1st = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
					officeDEP1st.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'DEP' #1", officeDEP1st.CY_CodeInfo, messageError);

					var officeDEP2nd = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
					officeDEP2nd.Validation.ValidateCY_Code();
					AssertHasMessageError("Customs Office 'DEP' #2", officeDEP2nd.CY_CodeInfo, messageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0009Active));
					ClearOtherRequirementsCache();
					officeDEP2nd.Validation.ValidateCY_Code();
					AssertNoMessageError("Rule TR0009 is disabled", officeDEP2nd.CY_CodeInfo, messageError);
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0009Active));
					ClearOtherRequirementsCache();

					var officeTRA = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					officeTRA.Validation.ValidateCY_Code();
					AssertNoMessageError("Customs Office 'TRA'", officeTRA.CY_CodeInfo, messageError);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}

		void ClearOtherRequirementsCache() => Factory.ClearCachedValue<IEnumerable<CustomsOfficeRequirement>>(
			string.Join(".", header.IsPhase5 ? "NctsMovementHeaderCustomsOfficeRequirementHelper.OtherRequirements" : "NctsHeaderCustomsOfficeRequirementHelper.OtherRequirements", header.BH_HeaderType, header.MovementHeader?.BM_InBondEntryType, header.BH_ApplicationCode));

		NctsHeader header;
	}
}
