using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillConsigneeJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRuleC0001()
		{
			const string messageError = "[C0001] You have not entered Consignee. It is required either on Declaration or House Consignment.";

			using (var ruleTestContext = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(c => c.IsRuleC0001Active);
				ruleTestContext.DisableRule(c => c.IsRuleB1823Active);

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var nctsBill = nctsHeader.Bills.AddNew();
				var consignee = nctsBill.Consignee;
				var organisation = Factory.New<OrgHeader>();
				var movementHeader = nctsHeader.MovementHeader;
				var propertyInfo = nctsBill.Consignee.OrganisationPKInfo;

				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When TP off", () =>
					{
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Empty BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						ruleTestContext.DisableRule(c => c.IsRuleC0001Active);
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule C0001 disabled", propertyInfo, messageError);
						ruleTestContext.EnableRule(c => c.IsRuleC0001Active);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);

						var nctsBillDocAddress = nctsBill.DocAddresses.FindOrCreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
						nctsBillDocAddress.OrganisationPK = organisation.PK;
						AssertNoMessageError("nctsBill consignee is not empty", propertyInfo, messageError);

						nctsBillDocAddress.OrganisationPK = ZGuid.Empty;
						var nctsHeaderDocAddress = nctsHeader.DocAddresses.FindOrCreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
						nctsHeaderDocAddress.OrganisationPK = organisation.PK;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("nctsHeader consignee is not empty", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Australia;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Not EU and Not CTP BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}

				ruleTestContext.EnableRule(c => c.IsRuleB1823Active);
				using (TemporarilySetTransitionPeriod(true))
				{
					CombineAssertions("When TP on", () =>
					{
						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}
			}
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);
	}
}
