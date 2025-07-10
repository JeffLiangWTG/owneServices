using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderCustomsOfficeRequirementHelper))]
	sealed class NctsHeaderCustomsOfficeRequirementHelperBaseOnlyTest : NctsHeaderCustomsOfficeRequirementHelperAbstractTest<NctsHeaderCustomsOfficeRequirementHelper>
	{
		public void TestGetNCTSOfficeOfTransitRequirement()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
			var officeOfTransit = header.CustomsOfficeRequirementHelper.GetOtherRequirementByRole(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);

			CombineAssertions(() =>
			{
				AssertEquals("Phase4 T2 - should be mandatory office", true, officeOfTransit.IsMandatory);

				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				officeOfTransit = header.CustomsOfficeRequirementHelper.GetOtherRequirementByRole(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				AssertEquals("Phase5 T2 C0030 active - should not be mandatory office", false, officeOfTransit.IsMandatory);
			});
		}

		public void TestGetNCTSOfficeOfTransitRequirement_C0030Inactive()
		{
			var messageErrorC0030 = "[C0030] You have not entered a Customs Office Of Transit Declared (TRA).";
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;

			CombineAssertions(() =>
			{
				using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0030Active)))
				{
					var officeOfTransit = header.CustomsOfficeRequirementHelper.GetOtherRequirementByRole(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					AssertEquals("Phase5 T2 C0030 not active - should be mandatory office", true, officeOfTransit.IsMandatory);
					AssertNotEquals("Phase5 T2 C0030 not active - message error", messageErrorC0030, officeOfTransit.ValidationMessage);
				}
			});
		}

		public void TestExpectedOtherRequirementsIsUpadatedWhenBH_HeaderTypeOrBM_InBondEntryTypeChange()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
			var officeHelper = header.CustomsOfficeRequirementHelper;
			var other1 = officeHelper.OtherRequirements;
			header.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			var other2 = officeHelper.OtherRequirements;

			AssertNotEquals("BM_InBondEntryType type changed", other1, other2);
		}

		public void TestOtherRequirements_NCTS4()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertContainsExactElementsInAnyOrder("All Office Roles"
				, new[] {
					OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
					OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
					OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit,
					OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit
				},
				officeHelper.OtherRequirements.Select(x => x.OfficeRole));
		}

		public void TestOtherRequirements_NCTS5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertContainsExactElementsInAnyOrder("All Office Roles"
				, new[] {
					OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
					OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
					OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit,
					OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit
				},
				officeHelper.OtherRequirements.Select(x => x.OfficeRole));
		}

		public void TestOtherRequirements_Arrival()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			officeHelper = header.CustomsOfficeRequirementHelper;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertContainsExactElementsInAnyOrder("All Office Roles"
				, new[] {
					OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
					OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival
				},
				officeHelper.OtherRequirements.Select(x => x.OfficeRole));
		}
	}
}
