using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsDepartureMovementHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsNctsDepartureMovementHeaderPhase5ValidationType()
		{
			AssertEquals(true, typeof(NctsDepartureMovementHeaderPhase5Validation).IsAssignableFrom(departureMovement.Validation.GetType()));
		}

		public void TestCheckBM_BTAIndicator_Mandatory()
		{
			const string messageError = "'E - Authorized economic operators' is only allowed if Principal and Security Consignor are Authorized Economic Operators with AEO-S or AEO-F Certificate.";
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_FTZMove = true;

			var principal = Factory.New<OrgHeader>();
			header.Principal.OrganisationPK = principal.PK;

			var consignor = Factory.New<OrgHeader>();
			header.SecurityConsignor.OrganisationPK = consignor.PK;

			departureMovement.BM_BTAIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("No AEO Codes", departureMovement.BM_BTAIndicatorInfo, messageError);
				principal.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOS00001", Core.Constants.CountryCodes.Germany);
				departureMovement.Validation.ValidateBM_BTAIndicator();
				AssertHasMessageErrorContaining("Principal has valid AEO code", departureMovement.BM_BTAIndicatorInfo, messageError);
				var principalAeo = consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "00002", Core.Constants.CountryCodes.Germany);
				departureMovement.Validation.ValidateBM_BTAIndicator();
				AssertHasMessageErrorContaining("Consignor has invalid AEO code", departureMovement.BM_BTAIndicatorInfo, messageError);
				principalAeo.OK_CustomsRegNo = "AEOF00002";
				departureMovement.Validation.ValidateBM_BTAIndicator();
				AssertNoMessageErrorContaining("Consignor has valid AEO code", departureMovement.BM_BTAIndicatorInfo, messageError);
			});
		}

		public void TestCheckBM_PaperlessInbondNum()
		{
			var targetInfo = departureMovement.BM_PaperlessInbondNumInfo;
			var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(currentCompanyPk, Guid.Empty, Guid.Empty, true))
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			}

			using (EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.SetTemporaryValue(currentCompanyPk, Guid.Empty, Guid.Empty, false))
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			}
		}

		public void TestCheckBM_InBondEntryType()
		{
			const string message = "Country/Region of Destination San Marino requires Declaration Type 'T2' or 'T2F'.";
			CombineAssertions(() =>
			{
				departureMovement.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Germany;
				departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				AssertNoMessageError("Country of Destination not San Marino", departureMovement.BM_InBondEntryTypeInfo, message);

				departureMovement.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.SanMarino;
				departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				AssertHasMessageError("BM_InBondEntryType must be T2(F) for SM", departureMovement.BM_InBondEntryTypeInfo, message);
				departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
				AssertNoMessageError("BM_InBondEntryType T2 for SM", departureMovement.BM_InBondEntryTypeInfo, message);
				departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories;
				AssertNoMessageError("BM_InBondEntryType T2F for SM", departureMovement.BM_InBondEntryTypeInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = header.MovementHeader;
		}
		NctsHeader header;
		NctsDepartureMovementHeader departureMovement;
	}
}
