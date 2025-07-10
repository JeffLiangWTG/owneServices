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
	sealed class ConsigneeJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestConstructor()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var jobDocAddress = Factory.New<JobDocAddress>();

			CombineAssertions(() =>
			{
#if NETFRAMEWORK
				AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: nctsHeader",
					() => new ConsigneeJobDocAddressValidationTestHelper(jobDocAddress, null));
#else
				AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null. (Parameter 'nctsHeader')",
					() => new ConsigneeJobDocAddressValidationTestHelper(jobDocAddress, null));
#endif

				AssertNoExceptionThrown("Not Null NctsHeader", () => new ConsigneeJobDocAddressValidationTestHelper(jobDocAddress, nctsHeader));
			});
		}

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
				var movementHeader = nctsHeader.MovementHeader;

				var jobDocAddress = Factory.New<JobDocAddress>();
				var validation = new ConsigneeJobDocAddressValidationTestHelper(jobDocAddress, nctsHeader);
				var organisation = Factory.New<OrgHeader>();
				var propertyInfo = jobDocAddress.OrganisationPKInfo;

				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When TP off", () =>
					{
						validation.ValidateOrganisationPK();
						AssertNoMessageError("Empty BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						validation.ValidateOrganisationPK();
						AssertHasMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						ruleTestContext.DisableRule(c => c.IsRuleC0001Active);
						validation.ValidateOrganisationPK();
						AssertNoMessageError("Rule C0001 disabled", propertyInfo, messageError);
						ruleTestContext.EnableRule(c => c.IsRuleC0001Active);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						validation.ValidateOrganisationPK();
						AssertHasMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);

						jobDocAddress.OrganisationPK = organisation.PK;
						AssertNoMessageError("nctsHeader consignee is not empty", propertyInfo, messageError);

						jobDocAddress.OrganisationPK = ZGuid.Empty;
						validation.IsRelevantConsigneeEmptyExposed = false;
						AssertNoMessageError("IsRelevantConsigneeEmptyExposed is false", propertyInfo, messageError);

						validation.IsRelevantConsigneeEmptyExposed = true;
						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Australia;
						validation.ValidateOrganisationPK();
						AssertNoMessageError("Not EU and Not CTP BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}

				ruleTestContext.EnableRule(c => c.IsRuleB1823Active);
				using (TemporarilySetTransitionPeriod(true))
				{
					CombineAssertions("When TP on", () =>
					{
						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
						validation.ValidateOrganisationPK();
						AssertNoMessageError("EU Member BM_RL_NKDestinationPort", propertyInfo, messageError);

						movementHeader.BM_RL_NKDestinationPort = CountryCodes.Macedonia;
						validation.ValidateOrganisationPK();
						AssertNoMessageError("Common Transit Procedure BM_RL_NKDestinationPort", propertyInfo, messageError);
					});
				}
			}
		}

		class ConsigneeJobDocAddressValidationTestHelper : ConsigneeJobDocAddressValidation
		{
			public ConsigneeJobDocAddressValidationTestHelper(AutoJobDocAddress parent, NctsHeader nctsHeader) : base(parent, nctsHeader)
			{
			}

			public bool IsRelevantConsigneeEmptyExposed = true;

			protected override bool IsRelevantConsigneeEmpty() => IsRelevantConsigneeEmptyExposed;
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);
	}
}
