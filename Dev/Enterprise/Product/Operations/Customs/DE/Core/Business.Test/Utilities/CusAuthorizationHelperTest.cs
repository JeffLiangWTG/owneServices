using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;
using LinkedCusAuthorisationRuleTypeList = Enterprise.Customs.Business.LinkedCusAuthorisationRuleTypeList;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusAuthorizationHelperTest : TestCaseWithFactory
	{
		public void TestDeclarantRequiresCustomsWarehousingAuthorizationMessage()
		{
			AssertEquals("Declarant must have an Authorization of Type 'CWP' or 'CW1' for this Declaration Type.", CusAuthorizationHelper.DeclarantRequiresCustomsWarehousingAuthorizationMessage);
		}

		public void TestDeclarantRequiresEIRAuthorisationForBondedWarehouseMessage()
		{
			AssertEquals("For this Declaration Type the Declarant must have an EIR Authorization for Bonded Warehouse.", CusAuthorizationHelper.DeclarantRequiresEIRAuthorisationForBondedWarehouseMessage);
		}

		public void TestDeclarantRequiresSDEAuthorisationForBondedWarehouseMessage()
		{
			AssertEquals("For this Declaration Type the Declarant must have an SDE Authorization for Bonded Warehouse.", CusAuthorizationHelper.DeclarantRequiresSDEAuthorisationForBondedWarehouseMessage);
		}

		public void TestDeclarantRequiresEndUserAuthorization()
		{
			AssertEquals("The Declarant must have an End Use Authorization (EUS) for this declaration.", CusAuthorizationHelper.DeclarantRequiresEndUserAuthorization);
		}

		public void TestDeclarantRequiresEIRAuthorizationForInwardProcessingMessage()
		{
			AssertEquals("For this Declaration Type the Declarant must have an EIR Authorization for Inward Processing.", CusAuthorizationHelper.DeclarantRequiresEIRAuthorizationForInwardProcessingMessage);
		}

		public void TestDeclarantRequiresSDEAuthorizationForInwardProcessingMessage()
		{
			AssertEquals("For this Declaration Type the Declarant must have an SDE Authorization for Inward Processing.", CusAuthorizationHelper.DeclarantRequiresSDEAuthorizationForInwardProcessingMessage);
		}

		public void TestDeclarantAZRequiresEIRAuthorizationMessage()
		{
			AssertEquals("For Declaration Type 'AZ' an Authorization record of Type ‘EIR’ is mandatory.", CusAuthorizationHelper.DeclarantAZRequiresEIRAuthorization);
		}

		public void TestDeclarantVZARequiresSDEAuthorizationMessage()
		{
			AssertEquals("For Declaration Type 'VZA' an Authorization record of Type ‘SDE’ is mandatory.", CusAuthorizationHelper.DeclarantVZARequiresSDEAuthorization);
		}

		public void TestSupportingDocumentTypesRequiringEndOfUseAuthorisation()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.C990, UniversalReferenceConstants.SupportingDocumentTypes.D019, UniversalReferenceConstants.SupportingDocumentTypes.N990 }, CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation);
		}

		public void TestValidateCusAuthorisationUsageOwner_AZ_SEL_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZ, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = SEL, AGC_Code = EIR, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = SEL, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZA_SEL_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZA, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = SEL, AGC_Code = SDE, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = SEL, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AZ_EUS()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZ, ZString.Empty, CusAuthorizationHeaderTypeList.Codes.EndUse);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, AGC_Code = EUS, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, AGC_Code = EUS, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZA_EUS()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZA, ZString.Empty, CusAuthorizationHeaderTypeList.Codes.EndUse);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, AGC_Code = EUS, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, AGC_Code = EUS, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AZ_DIR_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZ, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = DIR, AGC_Code = EIR, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = DIR, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = DIR, AGC_Code = EIR, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZA_DIR_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZA, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = DIR, AGC_Code = SDE, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = DIR, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = DIR, AGC_Code = SDE, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AZ_IND_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZ, RepresentationTypeList.Codes._3Indirect, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = IND, AGC_Code = EIR, RepresentedParty = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = IND, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZ, JE_DeclarantType = IND, AGC_Code = EIR, (Declarant, RepresentedParty) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentedPartyToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZA_IND_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZA, RepresentationTypeList.Codes._3Indirect, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = IND, AGC_Code = SDE, RepresentedParty = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = IND, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZA, JE_DeclarantType = IND, AGC_Code = SDE, (Declarant, RepresentedParty) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentedPartyToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AAV_SEL_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AAV, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = SEL, AGC_Code = EIR, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = SEL, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AAV_SEL_IPO()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AAV, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = SEL, AGC_Code = IPO, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = SEL, AGC_Code = IPO, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AAV_DIR_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AAV, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = DIR, AGC_Code = EIR, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = DIR, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = DIR, AGC_Code = EIR, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AAV_DIR_IPO()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AAV, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = DIR, AGC_Code = IPO, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = DIR, AGC_Code = IPO, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AAV_IND_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AAV, RepresentationTypeList.Codes._3Indirect, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = IND, AGC_Code = EIR, RepresentedParty = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = IND, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = IND, AGC_Code = EIR, (Declarant, RepresentedParty) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentedPartyToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AAV_IND_IPO()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AAV, RepresentationTypeList.Codes._3Indirect, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = IND, AGC_Code = IPO, RepresentedParty = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = IND, AGC_Code = IPO, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AAV, JE_DeclarantType = IND, AGC_Code = IPO, (Declarant, RepresentedParty) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentedPartyToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VAV_SEL_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VAV, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = SEL, AGC_Code = SDE, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = SEL, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VAV_SEL_IPO()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VAV, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = SEL, AGC_Code = IPO, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = SEL, AGC_Code = IPO, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VAV_DIR_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VAV, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = DIR, AGC_Code = SDE, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = DIR, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = DIR, AGC_Code = SDE, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VAV_DIR_IPO()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VAV, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = DIR, AGC_Code = IPO, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = DIR, AGC_Code = IPO, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VAV_IND_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VAV, RepresentationTypeList.Codes._3Indirect, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = IND, AGC_Code = SDE, RepresentedParty = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = IND, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = IND, AGC_Code = SDE, (Declarant, RepresentedParty) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentedPartyToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VAV_IND_IPO()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VAV, RepresentationTypeList.Codes._3Indirect, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = IND, AGC_Code = IPO, RepresentedParty = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = IND, AGC_Code = IPO, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VAV, JE_DeclarantType = IND, AGC_Code = IPO, (Declarant, RepresentedParty) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentedPartyToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AZL_CWP()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZL, string.Empty, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, AGC_Code = CWP, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, AGC_Code = CWP, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZL_CWP()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZL, string.Empty, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, AGC_Code = CWP, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, AGC_Code = CWP, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AZL_SEL_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZL, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = SEL, AGC_Code = EIR, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = SEL, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZL_SEL_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZL, RepresentationTypeList.Codes._1Self, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = SEL, AGC_Code = SDE, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = SEL, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AZL_DIR_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZL, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = DIR, AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = DIR, AGC_Code = EIR, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = DIR, AGC_Code = EIR, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZL_DIR_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZL, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = DIR, AGC_Code = SDE, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = DIR, AGC_Code = SDE, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = DIR, AGC_Code = SDE, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_AZL_DIR_CW1()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.AZL, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = DIR, AGC_Code = CW1, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = DIR, AGC_Code = CW1, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = AZL, JE_DeclarantType = DIR, AGC_Code = CW1, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_VZL_DIR_CW1()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZL, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = DIR, AGC_Code = CW1, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = DIR, AGC_Code = CW1, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = VZL, JE_DeclarantType = DIR, AGC_Code = CW1, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_EZL_CWP()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.EZL, string.Empty, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = EZL, AGC_Code = CWP, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = EZL, AGC_Code = CWP, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_NoConditionMet()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ImportDeclarationTypeList.Codes.VZA, RepresentationTypeList.Codes._2Direct, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
			AssertEquals("For Style = VZA, JE_DeclarantType = DIR, AGC_Code = EIR, (Declarant, Representative) <> Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
		}

		public void TestValidateCusAuthorisationUsageOwner_EIR()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(string.Empty, string.Empty, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("AGC_Code = EIR, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("AGC_Code = EIR, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_X01XXX_X00X_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._001310, string.Empty, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0001, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0001, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_X01XXX_X10X_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._001310, string.Empty, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0100, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0100, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_X01XXX_X01X_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._001310, string.Empty, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0010, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0010, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0010, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_X01XXX_X11X_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._001310, string.Empty, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0110;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0110, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0110, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = 001310, AGC_Code = SDE, PartyConstellation = 0110, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_111XXX_XX00_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._111300, string.Empty, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0100;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = 111300, AGC_Code = SDE, PartyConstellation = 0100, Declarant <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = 111300, AGC_Code = SDE, PartyConstellation = 0100, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_111XXX_XX10_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._111300, string.Empty, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0110;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For Style = 111300, AGC_Code = SDE, PartyConstellation = 0110, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = 111300, AGC_Code = SDE, PartyConstellation = 0110, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = 111300, AGC_Code = SDE, PartyConstellation = 0110, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_11XXXX_OPO()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._111300, string.Empty, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For Style = 111300, AGC_Code = OPO, Declarant <> Authorization Holder ", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For Style = 111300, AGC_Code = OPO, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_XX0X_CCL()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(string.Empty, string.Empty, CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For PartyConstellation = 0101, AGC_Code = CCL, Declarant <> Authorization Holder ", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For PartyConstellation = 0101, AGC_Code = CCL, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_XX1X_CCL()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(string.Empty, string.Empty, CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			cusAuthorizationUsage.Instruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0110;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.BuyingAgentAddress.OA_OH;
				AssertEquals("For PartyConstellation = 0110, AGC_Code = CCL, (Declarant, Representative) <> Authorization Holder", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("For PartyConstellation = 0110, AGC_Code = CCL, Declarant = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("For PartyConstellation = 0110, AGC_Code = CCL, Representative = Authorization Holder", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_CCL()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._001410, string.Empty, CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("No SDE authorization", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				var cusAuthorizationUsage2 = cusAuthorizationUsage.Instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				cusAuthorizationUsage2.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				cusAuthorizationUsage.Validation.ValidateAGC_OH_Owner();
				AssertEquals("Has SDE authorization with different owner", CusAuthorizationHelper.AuthHolderSdeAndCclToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("Has SDE authorization with same owner", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TestValidateCusAuthorisationUsageOwner_SDE()
		{
			var cusAuthorizationUsage = GetCusAuthorizationUsageForTest(ExportDeclarationTypeProcedureList.Codes._001410, string.Empty, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
			cusAuthorizationUsage.Instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertEquals("No CCL authorization", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				var cusAuthorizationUsage2 = cusAuthorizationUsage.Instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				cusAuthorizationUsage2.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				cusAuthorizationUsage.Validation.ValidateAGC_OH_Owner();
				AssertEquals("Has CCL authorization with different owner", CusAuthorizationHelper.AuthHolderSdeAndCclToBeSame, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));

				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertEquals("Has CCL authorization with same owner", ZString.Empty, CusAuthorizationHelper.ValidateCusAuthorisationUsageOwner(cusAuthorizationUsage));
			});
		}

		public void TesAuthHolderAndDeclarantToBeSame()
		{
			AssertEquals("Authorization Holder and Declarant must be equal.", CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame);
		}

		public void TestAuthHolderAndDeclarantOrRepresentativeToBeSame()
		{
			AssertEquals("Authorization Holder and Declarant or Representative must be equal.", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentativeToBeSame);
		}

		public void TestAuthHolderAndDeclarantOrRepresentedPartyToBeSame()
		{
			AssertEquals("Authorization Holder and Declarant or Represented Party must be equal.", CusAuthorizationHelper.AuthHolderAndDeclarantOrRepresentedPartyToBeSame);
		}

		public void TestAuthHolderSdeAndCclToBeSame()
		{
			AssertEquals("Authorization Holder of Type ‘SDE‘ and ‘CCL‘ must be equal.", CusAuthorizationHelper.AuthHolderSdeAndCclToBeSame);
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_AZ()
		{
			declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;

				AssertContainsExactElementsInAnyOrder("For style AZ without EIR authorisation", new ZString[] { CusAuthorizationHelper.DeclarantAZRequiresEIRAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				AssertEquals("For style AZ with EIR authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_VZA()
		{
			declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
				AssertContainsExactElementsInAnyOrder("For style VZA without SDE authorisation", new ZString[] { CusAuthorizationHelper.DeclarantVZARequiresSDEAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertEquals("For style VZA with SDE authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_AAV()
		{
			declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertContainsExactElementsInAnyOrder("For style AAV without EIR or IPO authorisation", new ZString[] { CusAuthorizationHelper.DeclarantAAVRequiresEIRAuthorization, CusAuthorizationHelper.DeclarantAAVRequiresIPOAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				AssertContainsExactElementsInAnyOrder("For style AAV with EIR authorisation", new ZString[] { CusAuthorizationHelper.DeclarantAAVRequiresIPOAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage2 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				AssertEquals("For style AAV with EIR and IPO authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_VAV()
		{
			declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				AssertContainsExactElementsInAnyOrder("For style VAV without SDE or IPO authorisation", new ZString[] { CusAuthorizationHelper.DeclarantVAVRequiresSDEAuthorization, CusAuthorizationHelper.DeclarantVAVRequiresIPOAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertContainsExactElementsInAnyOrder("For style VAV with SDE authorisation", new ZString[] { CusAuthorizationHelper.DeclarantVAVRequiresIPOAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage2 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				AssertEquals("For style VAV with EIR and IPO authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_AZL()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = ZString.Empty;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("For style AZL without EIR authorisation", new ZString[] { CusAuthorizationHelper.DeclarantAZLRequiresEIRAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				AssertEquals("For style AZL with EIR authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertContainsExactElementsInAnyOrder("For style AZL, JE_DeclarantType SEL, without CWP or CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantAZLRequiresCWPOrCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage2 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				AssertEquals("For style AZL, JE_DeclarantType SEL, with CWP authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("For style AZL, JE_DeclarantType SEL, with CW1 authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertContainsExactElementsInAnyOrder("For style AZL, JE_DeclarantType DIR, without CWP or CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantAZLRequiresCWPOrCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				AssertEquals("For style AZL, JE_DeclarantType DIR, with CWP authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("For style AZL, JE_DeclarantType DIR, with CW1 authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertContainsExactElementsInAnyOrder("For style AZL, JE_DeclarantType IND, without CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantAZLRequiresCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("For style AZL, JE_DeclarantType IND, with CW1 authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				var cusAuthorizationUsage3 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage3.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				AssertContainsExactElementsInAnyOrder("For style AZL, JE_DeclarantType IND, with CWP authorisation", new ZString[] { CusAuthorizationHelper.DeclarantINDMustNotHaveCWPAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_VZL()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = ZString.Empty;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("For style VZL without SDE authorisation", new ZString[] { CusAuthorizationHelper.DeclarantVZLRequiresSDEAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertEquals("For style VZL with SDE authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertContainsExactElementsInAnyOrder("For style VZL, JE_DeclarantType SEL, without CWP or CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantVZLRequiresCWPOrCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage2 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				AssertEquals("For style VZL, JE_DeclarantType SEL, with CWP authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("For style VZL, JE_DeclarantType SEL, with CW1 authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertContainsExactElementsInAnyOrder("For style VZL, JE_DeclarantType DIR, without CWP or CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantVZLRequiresCWPOrCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				AssertEquals("For style VZL, JE_DeclarantType DIR, with CWP authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("For style VZL, JE_DeclarantType DIR, with CW1 authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertContainsExactElementsInAnyOrder("For style VZL, JE_DeclarantType IND, without CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantVZLRequiresCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("For style VZL, JE_DeclarantType IND, with CW1 authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				var cusAuthorizationUsage3 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage3.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				AssertContainsExactElementsInAnyOrder("For style VZL, JE_DeclarantType IND, with CWP authorisation", new ZString[] { CusAuthorizationHelper.DeclarantINDMustNotHaveCWPAuthorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_EZL()
		{
			declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("For style EZL without CWP or CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantEZLRequiresCWPOrCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				AssertEquals("For style EZL with CWP authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("For style EZL with CW1 authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction).Any());

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument;
				AssertContainsExactElementsInAnyOrder("For style EZL without CWP or CW1 authorisation", new ZString[] { CusAuthorizationHelper.DeclarantEZLRequiresCWPOrCW1Authorization }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleImport(instruction));
			});
		}

		public void TestHasAuthorisationForBondedWarehouse()
		{
			CombineAssertions(() =>
			{
				OrgHeader org = null;
				AssertEquals("OrgHeader null", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				org = Factory.NewWithValidTestData<OrgHeader>();
				AssertEquals("No authorisation", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				var authorisation = org.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DESDE123");
				AssertEquals("No rules", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				var authorisationRule = authorisation.CreateAuthorisationRule(UsageRuleCode, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
				AssertEquals("Valid authorisation with rule CWP", true, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				authorisationRule.CPR_ValueFrom = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("Valid authorisation with rule CW1", true, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				authorisationRule.CPR_ValueFrom = "ZZZ";
				AssertEquals("Invalid rule value", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				authorisationRule.CPR_ValueFrom = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				authorisationRule.CPR_RuleCode = LocationRuleCode;
				AssertEquals("Invalid rule code", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				authorisationRule.CPR_RuleCode = UsageRuleCode;
				authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
				AssertEquals("Invalid country", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				AssertEquals("Invalid authorisation type", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				authorisation.CPH_StartDate = ZDate.Today.AddDays(1);
				AssertEquals("Invalid start date", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
				authorisation.CPH_EndDate = ZDate.Today.AddDays(-1);
				AssertEquals("Invalid end date", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				authorisation.CPH_OH_PermitHolder = org2.PK;
				AssertEquals("Invalid permit holder", false, org.HasAuthorisationForBondedWarehouse(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration));
			});
		}

		public void TestValidateRequiredAuthorisationForStyle_Declarant()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
				AssertEquals("No authorisation for EZL", CusAuthorizationHelper.DeclarantRequiresCustomsWarehousingAuthorizationMessage, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				AssertEquals("No authorisation for AZL", CusAuthorizationHelper.DeclarantRequiresEIRAuthorisationForBondedWarehouseMessage, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
				AssertEquals("No authorisation for VZL", CusAuthorizationHelper.DeclarantRequiresSDEAuthorisationForBondedWarehouseMessage, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("No authorisation for AAV", CusAuthorizationHelper.DeclarantRequiresEIRAuthorizationForInwardProcessingMessage, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				AssertEquals("No authorisation for VAV", CusAuthorizationHelper.DeclarantRequiresSDEAuthorizationForInwardProcessingMessage, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEAuth123");
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
				AssertEquals("Has CWP authorisation for EZL", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				AssertEquals("Has CW1 authorisation for EZL", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				authorisation.CreateAuthorisationRule(UsageRuleCode, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
				AssertEquals("Has SDE authorisation with USE-CWP rule for VZL", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				AssertEquals("Has SDE authorisation with USE-IPO rule for VAV", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				AssertEquals("Has EIR authorisation for AZL", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Has EIR authorisation with USE-IPO rule for AAV", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
			});
		}

		public void TestValidateRequiredAuthorisationForStyle_Declarant_AZ()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			instruction.CEI_LocalClearanceDate = ZDateTime.Today.AddDays(-2);
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("JE_DeclarantType = 'SEL', no authorisation for AZ", CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', no authorisation for AZ", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("JE_DeclarantType = 'IND', no authorisation for AZ", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEAuth123");
				AssertEquals("JE_DeclarantType = 'IND', has EIR authorisation without USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', has EIR authorisation without USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("JE_DeclarantType = 'SEL', has EIR authorisation without USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				AssertEquals("JE_DeclarantType = 'SEL' and CEI_LocalClearanceDate is invalid, has EIR authorisation with USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR' and CEI_LocalClearanceDate is invalid, has EIR authorisation with USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("JE_DeclarantType = 'IND' and CEI_LocalClearanceDate is invalid, has EIR authorisation with USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				instruction.CEI_LocalClearanceDate = ZDateTime.Today;
				AssertEquals("JE_DeclarantType = 'IND' and CEI_LocalClearanceDate is valid, has EIR authorisation with USE-IMP rule for AZ", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR' and CEI_LocalClearanceDate is valid, has EIR authorisation with USE-IMP rule for AZ", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("JE_DeclarantType = 'SEL' and CEI_LocalClearanceDate is valid, has EIR authorisation with USE-IMP rule for AZ", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
			});
		}

		public void TestValidateRequiredAuthorisationForStyle_Declarant_VZA()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("JE_DeclarantType = 'SEL', no authorisation for VZA", CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', no authorisation for VZA", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("JE_DeclarantType = 'IND', no authorisation for VZA", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DEAuth123");
				AssertEquals("JE_DeclarantType = 'IND', has SDE authorisation without USE-IMP rule for VZA", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', has SDE authorisation without USE-IMP rule for VZA", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("JE_DeclarantType = 'SEL', has SDE authorisation without USE-IMP rule for VZA", CusAuthorizationHelper.DeclarantRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));

				authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				AssertEquals("JE_DeclarantType = 'SEL', has SDE authorisation with USE-IMP rule for VZA", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', has SDE authorisation with USE-IMP rule for VZA", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("JE_DeclarantType = 'IND', has SDE authorisation with USE-IMP rule for VZA", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, true, false, false));
			});
		}

		public void TestValidateRequiredAuthorisationForStyle_Representative_AZ()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_Representative = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', no authorisation for AZ", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, true, false));

				var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEAuth123");
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', has EIR authorisation without USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, true, false));

				authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				AssertEquals("JE_DeclarantType = 'DIR' and CEI_LocalClearanceDate is empty, has EIR authorisation with USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, true, false));

				instruction.CEI_LocalClearanceDate = ZDateTime.Today;
				AssertEquals("JE_DeclarantType = 'DIR' and CEI_LocalClearanceDate is valid, has EIR authorisation with USE-IMP rule for AZ", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, true, false));
			});
		}

		public void TestValidateRequiredAuthorisationForStyle_Representative_VZA()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_Representative = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', no authorisation for VZA", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, true, false));

				var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DEAuth123");
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType = 'DIR', has SDE authorisation without USE-IMP rule for VZA", CusAuthorizationHelper.DeclarantOrRepresentativeRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, true, false));

				authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				AssertEquals("JE_DeclarantType = 'DIR', has SDE authorisation with USE-IMP rule for VZA", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, true, false));
			});
		}

		public void TestValidateRequiredAuthorisationForStyle_BuyingAgent_AZ()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_BuyingAgentAddress = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			instruction.CEI_LocalClearanceDate = ZDateTime.Today.AddDays(2);
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("JE_DeclarantType = 'IND', no authorisation for AZ", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, false, true));

				var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEAuth123");
				AssertEquals("JE_DeclarantType = 'IND', has EIR authorisation without USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, false, true));

				authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				AssertEquals("JE_DeclarantType = 'IND' and CEI_LocalClearanceDate is invalid, has EIR authorisation with USE-IMP rule for AZ", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, false, true));

				instruction.CEI_LocalClearanceDate = ZDateTime.Today;
				AssertEquals("JE_DeclarantType = 'IND' and CEI_LocalClearanceDate is valid, has EIR authorisation with USE-IMP rule for AZ", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, false, true));
			});
		}

		public void TestValidateRequiredAuthorisationForStyle_BuyingAgent_VZA()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_BuyingAgentAddress = orgHeader.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("JE_DeclarantType = 'IND', no authorisation for VZA", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, false, true));

				var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DEAuth123");
				AssertEquals("JE_DeclarantType = 'IND', has SDE authorisation without USE-IMP rule for VZA", CusAuthorizationHelper.DeclarantOrRepresentedPartyRequiresAuthorizationTypeSDE, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, false, true));

				authorisation.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
				AssertEquals("JE_DeclarantType = 'IND', has SDE authorisation with USE-IMP rule for VZA", ZString.Empty, CusAuthorizationHelper.ValidateRequiredAuthorisationForStyle(declaration, instruction, false, false, true));
			});
		}

		public void TestHasInwardProcessingAuthorization()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Org is null", false, CusAuthorizationHelper.HasInwardProcessingAuthorization(null));
				AssertHasSpecificAuthorisation(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, () => orgHeader.HasInwardProcessingAuthorization());
			});
		}

		public void TestHasOutwardProcessingAuthorization()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Org is null", false, CusAuthorizationHelper.HasOutwardProcessingAuthorization(null));
				AssertHasSpecificAuthorisation(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, () => orgHeader.HasOutwardProcessingAuthorization());
			});
		}

		public void TestHasConsignorTransitAuthorization()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Org is null", false, CusAuthorizationHelper.HasConsignorTransitAuthorization(null));
				AssertHasSpecificAuthorisation(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, () => orgHeader.HasConsignorTransitAuthorization());
			});
		}

		public void TestHasAuthorization()
		{
			CombineAssertions(() =>
			{
				var authorizationTypes = new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir };
				AssertEquals("NULL OrgHeader", false, ((OrgHeader)null).HasAuthorization(authorizationTypes));
				AssertEquals("No Authorization", false, orgHeader.HasAuthorization(authorizationTypes));

				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, ZString.Empty);
				AssertEquals("CWP exists", true, orgHeader.HasAuthorization(authorizationTypes));

				authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertEquals("ACT exists", true, orgHeader.HasAuthorization(authorizationTypes));
			});
		}

		public void TestHasAuthorization_OrgHeaders()
		{
			var authorizationTypes = new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir };
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaders = new[] { orgHeader, orgHeader2 };
			CombineAssertions(() =>
			{
				AssertEquals("NULL OrgHeaders", false, ((OrgHeader[])null).HasAuthorization(authorizationTypes));
				AssertEquals("No Authorization", false, orgHeaders.HasAuthorization(authorizationTypes));

				var authorization = orgHeader2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, ZString.Empty);
				AssertEquals("CWP exists", true, orgHeaders.HasAuthorization(authorizationTypes));

				authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertEquals("ACT exists", true, orgHeaders.HasAuthorization(authorizationTypes));
			});
		}

		public void TestHasAuthorizationOrgAddress()
		{
			CombineAssertions(() =>
			{
				var orgAddress = orgHeader.Addresses.AddNew();
				var authorizationTypes = new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir };
				AssertEquals("NULL OrgAddress", false, ((OrgAddress)null).HasAuthorization(authorizationTypes));
				AssertEquals("No Authorization", false, orgAddress.HasAuthorization(authorizationTypes));

				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, ZString.Empty);
				AssertEquals("CWP exists", true, orgAddress.HasAuthorization(authorizationTypes));

				authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertEquals("ACT exists", true, orgAddress.HasAuthorization(authorizationTypes));
			});
		}

		public void TestGetAuthorisationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NULL OrgHeader", ZString.Empty, ((OrgHeader)null).GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));
				AssertEquals("No Authorization", ZString.Empty, orgHeader.GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));

				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "TESTNUMBER");
				AssertEquals("ACR doesn't exist", ZString.Empty, orgHeader.GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));

				authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				AssertEquals("ACR exists", "TESTNUMBER", orgHeader.GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));
			});
		}

		public void TestGetAuthorisationNumberOrgAddress()
		{
			CombineAssertions(() =>
			{
				OrgAddress orgAddress = null;
				AssertEquals("orgAddress is null", ZString.Empty, orgAddress.GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));

				orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress.OA_OH = orgHeader.PK;
				AssertEquals("No Authorization", ZString.Empty, orgAddress.GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));

				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "TESTNUMBER");
				AssertEquals("ACR doesn't exist", ZString.Empty, orgAddress.GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));

				authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				AssertEquals("ACR exists", "TESTNUMBER", orgAddress.GetAuthorizationNumber(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));
			});
		}

		public void TestGetAuthorization()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL OrgHeader", ((OrgHeader)null).GetAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));
				AssertNull("No Authorization", orgHeader.GetAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));

				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "TESTNUMBER");

				AssertEquals("ACR exists", "TESTNUMBER", orgHeader.GetAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit).CPH_Number);

				authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertNull("ACR NOT exists", orgHeader.GetAuthorization(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));
			});
		}

		public void TestGetCachedAuthorizationNumbers()
		{
			var list = CusAuthorizationHelper.GetCachedAuthorizationNumbers(Factory, Array.Empty<ZString>(), new[] { ZGuid.Empty }, ZDate.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("List values", ZString.Empty, list.CodesAsString);
			});
		}

		public void TestGetCachedAuthorizationNumbers_MultipleTypes()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "EDIBRNWIS";
			organisation.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			organisation.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			organisation.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER3");
			Factory.Save();

			var list = CusAuthorizationHelper.GetCachedAuthorizationNumbers(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1 }, new[] { organisation.PK }, ZDate.Today);

			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1, NUMBER2, NUMBER3", list.CodesAsString);
				AssertSame("Cached-Type Order has no effect", list, CusAuthorizationHelper.GetCachedAuthorizationNumbers(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorizationHeaderTypeList.Codes.InwardProcessing }, new[] { organisation.PK }, ZDate.Today));
			});
		}

		public void TestGetCachedAuthorizationNumbers_MultipleAuthorizationHolders()
		{
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			orgHeader2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER3");

			var authorizationHolderPks = new[] { orgHeader.PK, orgHeader2.PK, ZGuid.Empty };
			var list = GetAuthorizations();

			CombineAssertions(() =>
			{
				AssertEquals("ZGuid.Empty in authorizationHolders is ignored", "NUMBER1, NUMBER2, NUMBER3", list.CodesAsString);

				authorizationHolderPks = new[] { orgHeader2.PK, orgHeader.PK };
				AssertSame("Reorder authorizationHolders, still cached", list, GetAuthorizations());

				authorizationHolderPks = new[] { ZGuid.Empty };
				AssertEquals("authorizationHolders with only ZGuid.Empty returns empty set", 0, GetAuthorizations().Count);
			});

			CodeDescriptionPairList GetAuthorizations() => CusAuthorizationHelper.GetCachedAuthorizationNumbers(Factory,
				new ZString[] { CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1 }
				, authorizationHolderPks
				, ZDate.Today);
		}

		public void TestGetCachedAuthorizationNumbersForAddresses()
		{
			var list = CusAuthorizationHelper.GetCachedAuthorizationNumbersForAddresses(Factory, Array.Empty<ZString>(), new[] { ZGuid.Empty }, ZDate.Empty);
			CombineAssertions(() =>
			{
				AssertEquals("List values", ZString.Empty, list.CodesAsString);
			});
		}

		public void TestGetCachedAuthorizationNumbersForAddresses_MultipleTypes()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "EDIBRNWIS";
			var address = organisation.Addresses.AddNew();
			address.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			address.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			address.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER3");

			var list = CusAuthorizationHelper.GetCachedAuthorizationNumbersForAddresses(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1 }, new[] { address.PK }, ZDate.Today);

			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1, NUMBER2, NUMBER3", list.CodesAsString);
				AssertSame("Cached-Type Order has no effect", list, CusAuthorizationHelper.GetCachedAuthorizationNumbersForAddresses(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorizationHeaderTypeList.Codes.InwardProcessing }, new[] { address.PK }, ZDate.Today));
			});
		}

		public void TestGetCachedAuthorizationNumbersForAddresses_MultipleAuthorizationAddresses()
		{
			var address = orgHeader.Addresses.AddNew();
			address.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			address.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = orgHeader2.Addresses.AddNew();
			address2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");
			address2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER3");

			var authorizationAddressPks = new[] { address.PK, address2.PK, ZGuid.Empty };
			var list = GetAuthorizations();

			CombineAssertions(() =>
			{
				AssertEquals("ZGuid.Empty in authorizationAddresses is ignored", "NUMBER1, NUMBER2, NUMBER3", list.CodesAsString);

				authorizationAddressPks = new[] { address2.PK, address.PK };
				AssertSame("Reorder authorizationAddresses, still cached", list, GetAuthorizations());

				authorizationAddressPks = new[] { ZGuid.Empty };
				AssertEquals("authorizationAddresses with only ZGuid.Empty returns empty set", 0, GetAuthorizations().Count);
			});

			CodeDescriptionPairList GetAuthorizations() => CusAuthorizationHelper.GetCachedAuthorizationNumbersForAddresses(Factory,
				new ZString[] { CusAuthorizationHeaderTypeList.Codes.InwardProcessing, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1 }
				, authorizationAddressPks
				, ZDate.Today);
		}

		public void TestGetLocationOfGoodsListForTemporaryStorage()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "01", "01 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "02", "02 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var authorisation = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "NUMBER1");
			var rule1 = authorisation.CreateAuthorisationRule(LocationRuleCode, "01");
			rule1.CreateLinkedAuthorisationRule(CustomsOfficeLinkedRuleCode, "DE000001");
			var authorisation2 = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "NUMBER2");
			var rule2 = authorisation2.CreateAuthorisationRule(LocationRuleCode, "02");
			rule2.CreateLinkedAuthorisationRule(CustomsOfficeLinkedRuleCode, "DE000001");
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = orgHeader.GetLocationOfGoodsListForTemporaryStorage("DE000001");
				AssertEquals("Matched CustomsOffice", "01, 02", list.CodesAsString);
				AssertSame("Cached", orgHeader.GetLocationOfGoodsListForTemporaryStorage("DE000001"), list);

				AssertEquals("CustomsOffice mismatch", ZString.Empty, orgHeader.GetLocationOfGoodsListForTemporaryStorage("DE000002").CodesAsString);

				AssertEquals("No Organisation", ZString.Empty, CusAuthorizationHelper.GetLocationOfGoodsListForTemporaryStorage(null, "DE000001").CodesAsString);
			});
		}

		public void TestGetCachedRuleValues()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "123", "123 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "456", "456 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE003202");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "789", "789 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE111111");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Expired", "Expired", ZDateTime.MinSmallDateTimeValue, ZDate.Today.AddDays(-1), RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE111111");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "000", "000 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE111111");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Latvia", "Latvia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "DE111111");
			Factory.Save();

			var expiredAuthorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "EXPIRED");
			expiredAuthorization.CPH_EndDate = ZDate.Today.AddDays(-1);
			expiredAuthorization.CreateAuthorisationRule(LocationRuleCode, "000");

			var nonGermanyAuthorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "LVACR1", Core.Constants.CountryCodes.Latvia);
			nonGermanyAuthorization.CreateAuthorisationRule(LocationRuleCode, "Latvia");

			var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "DEACR1");
			authorization.CreateAuthorisationRule(LocationRuleCode, "123");
			authorization.CreateAuthorisationRule(LocationRuleCode, "456");
			authorization.CreateAuthorisationRule(LocationRuleCode, "789");
			authorization.CreateAuthorisationRule(LocationRuleCode, "Expired");

			var authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			var authorizationHolderPk = orgHeader.PK;
			var ruleCode = LocationRuleCode;
			var ruleValueType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			var attributeFilterName = RefCusCodeListAttributeTypes.Codes.CustomsOffice;
			var attributeFilterValue = "DE003202";

			AssertEquals("Valid params, attributeFilterValue 'DE003202'", "123, 456", GetAuthorizationRuleValuesAsString());

			authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			AssertEquals("Incorrect authorizationType", string.Empty, GetAuthorizationRuleValuesAsString());

			authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			authorizationHolderPk = ZGuid.Empty;
			AssertEquals("Incorrect authorizationHolderPk", string.Empty, GetAuthorizationRuleValuesAsString());

			authorizationHolderPk = orgHeader.PK;
			ruleCode = "XYZ";
			AssertEquals("Incorrect ruleCode", string.Empty, GetAuthorizationRuleValuesAsString());

			ruleCode = LocationRuleCode;
			ruleValueType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes;
			AssertEquals("Incorrect ruleValueType", string.Empty, GetAuthorizationRuleValuesAsString());

			ruleValueType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			attributeFilterName = RefCusCodeListAttributeTypes.Codes.DisplayCode;
			AssertEquals("Incorrect attributeFilterName", string.Empty, GetAuthorizationRuleValuesAsString());

			attributeFilterName = RefCusCodeListAttributeTypes.Codes.CustomsOffice;
			attributeFilterValue = ZString.Empty;
			AssertEquals("Incorrect attributeFilterValue", string.Empty, GetAuthorizationRuleValuesAsString());

			attributeFilterValue = "DE111111";
			AssertEquals("Valid params, attributeFilterValue 'DE111111'", "789", GetAuthorizationRuleValuesAsString());

			string GetAuthorizationRuleValuesAsString()
			{
				var ruleValues = CusAuthorizationHelper.GetCachedRuleValues(Factory, authorizationType, authorizationHolderPk, ruleCode, ruleValueType, attributeFilterName, attributeFilterValue);
				return ruleValues.CodesAsString;
			}
		}

		public void TestGetCachedRuleValues_Cached()
		{
			const string authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			var authorizationHolderPk = orgHeader.PK;
			const string ruleValueType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			const string attributeFilterName = RefCusCodeListAttributeTypes.Codes.CustomsOffice;
			const string attributeFilterValue = "DE003202";
			var ruleValues = CusAuthorizationHelper.GetCachedRuleValues(Factory, authorizationType, authorizationHolderPk, LocationRuleCode, ruleValueType, attributeFilterName, attributeFilterValue);
			AssertSame(ruleValues, CusAuthorizationHelper.GetCachedRuleValues(Factory, authorizationType, authorizationHolderPk, LocationRuleCode, ruleValueType, attributeFilterName, attributeFilterValue));
		}

		public void TestGetCachedAuthorizationRules()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "LOC01", "LOC01 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "LOC02", "LOC02 Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Latvia", "Latvia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			var authorizationHolderPk = orgHeader.PK;
			var ruleCode = LocationRuleCode;
			var linkedRuleCode = CustomsOfficeLinkedRuleCode;
			var linkedRuleValue = "DE003202";
			var transactionDate = ZDate.Today;

			var nonGermanyAuthorization = orgHeader.CreateAuthorisationRecord(authorizationType, "LATVIA");
			nonGermanyAuthorization.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var nonGermanyAuthorizationRule = nonGermanyAuthorization.CreateAuthorisationRule(ruleCode, "Latvia");
			nonGermanyAuthorizationRule.CreateLinkedAuthorisationRule(linkedRuleCode, linkedRuleValue);

			var authorization = orgHeader.CreateAuthorisationRecord(authorizationType, "ACR1");
			var rule1 = authorization.CreateAuthorisationRule(ruleCode, "LOC01");
			rule1.CreateLinkedAuthorisationRule(linkedRuleCode, linkedRuleValue);
			var rule2 = authorization.CreateAuthorisationRule(ruleCode, "LOC02");
			rule2.CreateLinkedAuthorisationRule("XYZ", "ABC");
			rule2.CreateLinkedAuthorisationRule(linkedRuleCode, linkedRuleValue);

			CombineAssertions(() =>
			{
				AssertEquals("Valid params", $"LOC01 - LOC01 Desc.{System.Environment.NewLine}LOC02 - LOC02 Desc.", GetAuthorizationRuleValuesAsString());

				authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				AssertEquals("Incorrect authorizationType", string.Empty, GetAuthorizationRuleValuesAsString());

				authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				authorizationHolderPk = ZGuid.Empty;
				AssertEquals("Incorrect authorizationHolderPk", string.Empty, GetAuthorizationRuleValuesAsString());

				authorizationHolderPk = orgHeader.PK;
				ruleCode = UsageRuleCode;
				AssertEquals("Incorrect ruleCode", string.Empty, GetAuthorizationRuleValuesAsString());

				ruleCode = LocationRuleCode;
				linkedRuleCode = LocationRuleCode;
				AssertEquals("Incorrect linkedRuleCode", string.Empty, GetAuthorizationRuleValuesAsString());

				linkedRuleCode = CustomsOfficeLinkedRuleCode;
				linkedRuleValue = ZString.Empty;
				AssertEquals("Incorrect linkedRuleValue", string.Empty, GetAuthorizationRuleValuesAsString());

				linkedRuleValue = "DE003202";
				transactionDate = ZDate.Today.AddYears(-1);
				AssertEquals("transactionDate before startDate", string.Empty, GetAuthorizationRuleValuesAsString());

				transactionDate = ZDate.Today.AddYears(1);
				AssertEquals("transactionDate after endDate", string.Empty, GetAuthorizationRuleValuesAsString());
			});

			string GetAuthorizationRuleValuesAsString() => CusAuthorizationHelper.GetCachedAuthorizationRules(Factory, authorizationType, authorizationHolderPk, ruleCode, linkedRuleCode, linkedRuleValue, transactionDate).ElementsAsString;
		}

		public void TestGetCachedAuthorizationRules_Cached()
		{
			AssertSame(GetCachedAuthorizationRulesFunc(), GetCachedAuthorizationRulesFunc());
			CodeDescriptionPairList GetCachedAuthorizationRulesFunc() => CusAuthorizationHelper.GetCachedAuthorizationRules(Factory, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, orgHeader.PK, LocationRuleCode, CustomsOfficeLinkedRuleCode, "DE003202", ZDate.Today);
		}

		public void TestGetCachedAuthorizationRules_AuthorizationNumber()
		{
			var authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			var authorizationHolderPk = orgHeader.PK;
			var ruleCode = LocationRuleCode;
			var linkedRuleCode = CustomsOfficeLinkedRuleCode;
			var linkedRuleValue = "DE003202";
			var transactionDate = ZDate.Today;

			var authorization = orgHeader.CreateAuthorisationRecord(authorizationType, "ACR1");
			var rule1 = authorization.CreateAuthorisationRule(ruleCode, "LOC01");
			rule1.CreateLinkedAuthorisationRule(linkedRuleCode, linkedRuleValue);

			CombineAssertions(() =>
			{
				AssertEquals("Valid authorizationNumber", "LOC01", CusAuthorizationHelper.GetCachedAuthorizationRules(Factory, authorizationType, authorizationHolderPk, ruleCode, linkedRuleCode, linkedRuleValue, transactionDate, "ACR1").CodesAsString);
				AssertEquals("Invalid authorizationNumber", string.Empty, CusAuthorizationHelper.GetCachedAuthorizationRules(Factory, authorizationType, authorizationHolderPk, ruleCode, linkedRuleCode, linkedRuleValue, transactionDate, "ACR2").CodesAsString);
			});
		}

		public void TestGetAuthorisationWithRule()
		{
			CombineAssertions(() =>
			{
				AssertNull("No EIR Authorization", orgHeader.GetAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", Core.Constants.CountryCodes.Germany);
				AssertNull("EIR Authorization Without USE = AEX", orgHeader.GetAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
				var rule = authorization.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				AssertEquals("EIR Authorization With USE = AEX", "TEST", orgHeader.GetAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }).CPH_Number);
			});
		}

		public void TestGetAuthorisationWithRule_TransactionDate()
		{
			CombineAssertions(() =>
			{
				orgHeader.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
				AssertEquals("No Parameter transactionDate", "TEST", orgHeader.GetAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }).CPH_Number);
				AssertEquals("Valid Parameter TransactionDate", "TEST", orgHeader.GetAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }).CPH_Number);
				AssertNull("Invalid Parameter TransactionDate", orgHeader.GetAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today.AddDays(-2), new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
			});
		}

		public void TestHasAuthorizationWithRule()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No EIR Authorization", ZBool.False, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", Core.Constants.CountryCodes.Germany);
				AssertEquals("EIR Authorization Without USE = AEX", ZBool.False, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
				var rule = authorization.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				AssertEquals("EIR Authorization With USE = AEX", ZBool.True, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
			});
		}

		public void TestHasAuthorizationWithRule_OrgHeaders()
		{
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaders = new[] { orgHeader, orgHeader2 };
			CombineAssertions(() =>
			{
				AssertEquals("No EIR Authorization", ZBool.False, orgHeaders.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
				var authorization = orgHeader2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", Core.Constants.CountryCodes.Germany);
				AssertEquals("EIR Authorization Without USE = AEX", ZBool.False, orgHeaders.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
				authorization.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
				AssertEquals("EIR Authorization With USE = AEX", ZBool.True, orgHeaders.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter }));
			});
		}

		public void TestHasAuthorizationWithRule_MultipleValues_MatchAny()
		{
			CombineAssertions(() =>
			{
				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", Core.Constants.CountryCodes.Germany);
				var rule1 = authorization.CusAuthorisationRules.AddNew();
				rule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule1.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				var rule2 = authorization.CusAuthorisationRules.AddNew();
				rule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule2.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1;
				AssertEquals("All rule Values exist", ZBool.True, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter, CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1 }));
				AssertEquals("Only one rule value exists", ZBool.True, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing }));
				AssertEquals("No rule value exists", ZBool.False, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.FreeCirculation, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing }));
			});
		}

		public void TestHasAuthorizationWithRule_MultipleValues_MatchAll()
		{
			CombineAssertions(() =>
			{
				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", Core.Constants.CountryCodes.Germany);
				var rule1 = authorization.CusAuthorisationRules.AddNew();
				rule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule1.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				var rule2 = authorization.CusAuthorisationRules.AddNew();
				rule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule2.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1;
				AssertEquals("All rule Values exist", ZBool.True, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter, CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1 }, true));
				AssertEquals("Only one rule value exists", ZBool.False, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.AccreditedExporter, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing }, true));
				AssertEquals("No rule value exists", ZBool.False, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today, new ZString[] { CusAuthorisationUsageRuleList.Codes.FreeCirculation, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing }, true));
			});
		}

		public void TestHasAuthorizationWithRule_NoValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Authorization doesn't exist", ZBool.False, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today));
				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", Core.Constants.CountryCodes.Germany);
				AssertEquals("Authorization exists but without rules", ZBool.False, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today));
				var rule1 = authorization.CusAuthorisationRules.AddNew();
				rule1.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule1.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				AssertEquals("Rule exists", ZBool.True, orgHeader.HasAuthorizationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, ZDate.Today));
			});
		}

		public void TestGetCachedRuleValuesFilteredByLinkedRules()
		{
			var authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			var authorizationHolderPk = orgHeader.PK;
			var targetRuleCode = LocationRuleCode;
			var filterRuleCode = UsageRuleCode;
			var filterRuleValue = CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure;
			var linkedRuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
			var linkedRuleValue = "DE003202";

			var expiredAuthorization = CreateTestAuthorization("EXPIRED", "InvalidAuthorization");
			expiredAuthorization.CPH_EndDate = ZDate.Today.AddDays(-1);

			var futureAuthorization = CreateTestAuthorization("FUTURE", "InvalidAuthorization");
			futureAuthorization.CPH_StartDate = ZDate.Today.AddDays(1);

			var nonGermanyAuthorization = CreateTestAuthorization("NONGERMANY", "InvalidAuthorization");
			nonGermanyAuthorization.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			var validAuthorization = CreateTestAuthorization("DEACR1", "LOC02");
			var validRule = validAuthorization.CreateAuthorisationRule(LocationRuleCode, "LOC01", "LOC01 description");
			validRule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");

			CombineAssertions(() =>
			{
				AssertEquals("Valid params", $"LOC01 - LOC01 description{System.Environment.NewLine}LOC02 - LOC02 description", GetAuthorizationRuleValuesAsString());

				authorizationType = ZString.Empty;
				AssertEquals("Invalid authorizationType", string.Empty, GetAuthorizationRuleValuesAsString());

				authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				authorizationHolderPk = ZGuid.Empty;
				AssertEquals("Invalid authorizationHolderPk", string.Empty, GetAuthorizationRuleValuesAsString());

				authorizationHolderPk = orgHeader.PK;
				targetRuleCode = "XYZ";
				AssertEquals("Invalid targetRuleCode", string.Empty, GetAuthorizationRuleValuesAsString());

				targetRuleCode = LocationRuleCode;
				filterRuleCode = "XYZ";
				AssertEquals("Invalid filterRuleCode", string.Empty, GetAuthorizationRuleValuesAsString());

				filterRuleCode = UsageRuleCode;
				filterRuleValue = "XYZ";
				AssertEquals("Invalid filterRuleValue", string.Empty, GetAuthorizationRuleValuesAsString());

				filterRuleValue = CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure;
				linkedRuleCode = "XYZ";
				AssertEquals("Invalid linkedRuleCode", string.Empty, GetAuthorizationRuleValuesAsString());

				linkedRuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				linkedRuleValue = ZString.Empty;
				AssertEquals("Invalid linkedRuleCode", string.Empty, GetAuthorizationRuleValuesAsString());
			});

			CusAuthorisationHeader CreateTestAuthorization(ZString authorizationNumber, ZString ruleValue)
			{
				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, authorizationNumber);
				authorization.CreateAuthorisationRule(UsageRuleCode, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
				var targetRule = authorization.CreateAuthorisationRule(LocationRuleCode, ruleValue, $"{ruleValue} description");
				targetRule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");
				return authorization;
			}

			string GetAuthorizationRuleValuesAsString()
			{
				var ruleValues = CusAuthorizationHelper.GetCachedRuleValuesFilteredByLinkedRules(Factory, authorizationType, new[] { authorizationHolderPk }, targetRuleCode, filterRuleCode, filterRuleValue, linkedRuleCode, linkedRuleValue);
				return ruleValues.ElementsAsString;
			}
		}

		public void TestGetCachedRuleValuesFilteredByLinkedRules_MultipleAuthorizationHolders()
		{
			var authorization1 = CreateValidAuthorization(orgHeader, "DEACR1");
			CreateValidRule(authorization1, "LOC02");
			CreateValidRule(authorization1, "LOC01");

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var authorization2 = CreateValidAuthorization(orgHeader, "DEACR2");
			CreateValidRule(authorization2, "LOC03");
			CreateValidRule(authorization2, "LOC02");

			var authorizationHolderPks = new[] { orgHeader.PK, orgHeader2.PK, ZGuid.Empty };
			var authorizationRuleValues = GetAuthorizationRuleValues();

			CombineAssertions(() =>
			{
				AssertEquals("ZGuid.Empty in authorizationHolderPks is ignored", $"LOC01 - LOC01 description{System.Environment.NewLine}LOC02 - LOC02 description{System.Environment.NewLine}LOC03 - LOC03 description", authorizationRuleValues.ElementsAsString);

				authorizationHolderPks = new[] { orgHeader2.PK, orgHeader.PK };
				AssertSame("Reorder authorizationHolderPks, still cached", authorizationRuleValues, GetAuthorizationRuleValues());

				authorizationHolderPks = new[] { ZGuid.Empty };
				AssertEquals("authorizationHolderPks with only ZGuid.Empty returns empty set", 0, GetAuthorizationRuleValues().Count);
			});

			CodeDescriptionPairList GetAuthorizationRuleValues() => CusAuthorizationHelper.GetCachedRuleValuesFilteredByLinkedRules(Factory
					, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit
					, authorizationHolderPks
					, LocationRuleCode
					, UsageRuleCode
					, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure
					, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice
					, "DE003202");

			CusAuthorisationHeader CreateValidAuthorization(OrgHeader authorizationHolder, ZString authorizationNumber)
			{
				var authorization = authorizationHolder.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, authorizationNumber);
				authorization.CreateAuthorisationRule(UsageRuleCode, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
				return authorization;
			}

			void CreateValidRule(CusAuthorisationHeader authorization, ZString ruleValue)
			{
				var rule = authorization.CreateAuthorisationRule(LocationRuleCode, ruleValue, $"{ruleValue} description");
				rule.CreateLinkedAuthorisationRule(LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice, "DE003202");
			}
		}

		public void TestGetCachedRuleValuesFilteredByLinkedRules_Cached()
		{
			AssertSame(Func(), Func());
			CodeDescriptionPairList Func() => CusAuthorizationHelper.GetCachedRuleValuesFilteredByLinkedRules(Factory
				, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit
				, new[] { orgHeader.PK }
				, LocationRuleCode
				, CusAuthorisationRuleTypeList.Codes.Usage
				, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure
				, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice
				, "DE003202");
		}

		public void TestGetDeclarationAuthorizationNumberList_Import_ATAV()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			SetupDeclaration(declaration, entryInstruction);

			AssertEquals("NUMBER30, NUMBER34", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATAV).CodesAsString);
		}

		public void TestGetDeclarationAuthorizationNumberList_Import_ATAV_DeclarantTypeDIR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			SetupDeclaration(declaration, entryInstruction);

			CombineAssertions(() =>
			{
				AssertEquals("Has Representative, has Declarant, has FromWarehouse", "NUMBER30, NUMBER31, NUMBER34", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATAV).CodesAsString);

				declaration.JE_OA_Representative = ZGuid.Empty;
				AssertEquals("No Representative, has Declarant, has FromWarehouse", "NUMBER30, NUMBER34", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATAV).CodesAsString);

				entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				AssertEquals("No Representative, has Declarant, no FromWarehouse", "NUMBER30", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATAV).CodesAsString);
			});
		}

		public void TestAuthorizationNumberList_Import_ATZL()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			SetupDeclaration(declaration, entryInstruction);

			AssertEquals("NUMBER1, NUMBER13, NUMBER14, NUMBER15, NUMBER2, NUMBER3", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATZL).CodesAsString);
		}

		public void TestAuthorizationNumberList_Import_ATZL_DeclarantTypeDIR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			SetupDeclaration(declaration, entryInstruction);

			CombineAssertions(() =>
			{
				AssertEquals("Has Representative, has Declarant, has FromWarehouse", "NUMBER1, NUMBER13, NUMBER14, NUMBER15, NUMBER2, NUMBER3, NUMBER4, NUMBER5, NUMBER6",
					CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATZL).CodesAsString);

				declaration.JE_OA_Representative = ZGuid.Empty;
				AssertEquals("No Representative, has Declarant, has FromWarehouse", "NUMBER1, NUMBER13, NUMBER14, NUMBER15, NUMBER2, NUMBER3", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATZL).CodesAsString);

				entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				AssertEquals("No Representative, has Declarant, no FromWarehouse", "NUMBER1, NUMBER2, NUMBER3", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, entryInstruction, ZDate.Today, PreviousProcedureList.Codes._ATZL).CodesAsString);
			});
		}

		public void TestAuthorizationNumberList_Export_ATAV()
		{
			var declaration = Factory.New<JobDeclaration>();
			SetupDeclaration(declaration, null);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("List values", "NUMBER30, NUMBER31, NUMBER32, NUMBER33", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, null, ZDate.Today, PreviousProcedureList.Codes._ATAV).CodesAsString);
		}

		public void TestAuthorizationNumberList_Export_ATZL()
		{
			var declaration = Factory.New<JobDeclaration>();
			SetupDeclaration(declaration, null);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("List values", "NUMBER1, NUMBER10, NUMBER11, NUMBER12, NUMBER2, NUMBER3, NUMBER4, NUMBER5, NUMBER6, NUMBER7, NUMBER8, NUMBER9", CusAuthorizationHelper.GetDeclarationAuthorizationNumberList(declaration, null, ZDate.Today, PreviousProcedureList.Codes._ATZL).CodesAsString);
		}

		public void TestGetAuthorizationNumbersForPrimaryAndSecondaryHolders_ResultsFromPrimary()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER2");

			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "EDIBRNFRA";
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER3");
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER4");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = declarant.Addresses.AddNew().PK;
			declaration.JE_OA_Representative = representative.Addresses.AddNew().PK;

			var list = CusAuthorizationHelper.GetAuthorizationNumbersForPrimaryAndSecondaryHolders(Factory, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, ZDate.Today, declarant.PK, representative.PK);
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER1, NUMBER2", list.CodesAsString);
				AssertSame("Cached", list, CusAuthorizationHelper.GetAuthorizationNumbersForPrimaryAndSecondaryHolders(Factory, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, ZDate.Today, declarant.PK, representative.PK));
			});
		}

		public void TestGetAuthorizationNumbersForPrimaryAndSecondaryHolders_ResultsFromSecondary()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "NUMBER1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "NUMBER2");

			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "EDIBRNFRA";
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER3");
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER4");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = declarant.Addresses.AddNew().PK;
			declaration.JE_OA_Representative = representative.Addresses.AddNew().PK;

			var list = CusAuthorizationHelper.GetAuthorizationNumbersForPrimaryAndSecondaryHolders(Factory, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, ZDate.Today, declarant.PK, representative.PK);
			CombineAssertions(() =>
			{
				AssertEquals("List values", "NUMBER3, NUMBER4", list.CodesAsString);
				AssertSame("Cached", list, CusAuthorizationHelper.GetAuthorizationNumbersForPrimaryAndSecondaryHolders(Factory, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, ZDate.Today, declarant.PK, representative.PK));
			});
		}

		public void TestRequiresAuthorizationRuleRelease()
		{
			var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "EIR123");
			var rule = authorization.CusAuthorisationRules.AddNew();

			CombineAssertions(() =>
			{
				foreach (var useValue in new[]
				{
					CusAuthorisationUsageRuleList.Codes.FreeCirculation,
					CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure,
					CusAuthorisationUsageRuleList.Codes.CustomsWarehousing,
					CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1
				})
				{
					rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
					rule.CPR_ValueFrom = useValue;
					AssertEquals($"USE value '{useValue}'", true, authorization.RequiresAuthorizationRuleRelease());

					rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.BusinessReference;
					AssertEquals($"USE value '{useValue}', CPR_RuleCode <> 'USE'", false, authorization.RequiresAuthorizationRuleRelease());
				}

				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				AssertEquals("USE value 'AEX'", false, authorization.RequiresAuthorizationRuleRelease());
			});
		}

		public void TestHasCusAuthorizationUsageWithRule()
		{
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			cusAuthorizationUsage.AGC_Number = "EIR123";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertHasCusAuthorizationUsageWithRule("No CusPermitHeader", ZBool.False);

				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "EIR123");
				var rule = authorization.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Release;
				rule.CPR_ValueFrom = CusAuthorisationReleaseRuleList.Codes._1;
				Factory.Save();
				AssertHasCusAuthorizationUsageWithRule("Has CusAuthorizationUsage and CusPermitHeader", ZBool.True);

				AssertHasCusAuthorizationUsageWithRule("Incorrect authorizationType", ZBool.False, authorizationType: CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir);

				AssertHasCusAuthorizationUsageWithRule("Incorrect ruleCode", ZBool.False, ruleCode: CusAuthorisationRuleTypeList.Codes.Usage);

				AssertHasCusAuthorizationUsageWithRule("Incorrect valueFrom", ZBool.False, valueFrom: CusAuthorisationReleaseRuleList.Codes._2);

				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				AssertHasCusAuthorizationUsageWithRule("No CusAuthorizationUsages", ZBool.False);
			});

			void AssertHasCusAuthorizationUsageWithRule(string message, ZBool expectedResult, string authorizationType = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, string ruleCode = CusAuthorisationRuleTypeList.Codes.Release, string valueFrom = CusAuthorisationReleaseRuleList.Codes._1)
			{
				AssertEquals(message, expectedResult, instruction.HasCusAuthorizationUsageWithRule(authorizationType, ruleCode, valueFrom));
			}
		}

		public void TestHasCusAuthorizationUsage()
		{
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			cusAuthorizationUsage.AGC_Number = "EIR123";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertHasCusAuthorizationUsage("Has CusAuthorizationUsage", ZBool.True);

				AssertHasCusAuthorizationUsage("Incorrect authorizationType", ZBool.False, authorizationType: CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir);

				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				AssertHasCusAuthorizationUsage("No CusAuthorizationUsages", ZBool.False);
			});

			void AssertHasCusAuthorizationUsage(string message, ZBool expectedResult, string authorizationType = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords)
			{
				AssertEquals(message, expectedResult, instruction.HasCusAuthorizationUsage(authorizationType));
			}
		}

		public void TestValidateRequiredCusAuthorisationUsageForSubStyle_20()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				AssertContainsExactElementsInAnyOrder("For subStyle 20 without EIR authorisation", new ZString[] { CusAuthorizationHelper.DeclarantSubStyle20RequiresEIRAuthorization(instruction.CEI_SubStyle, instruction.CEI_Style) }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				AssertEquals("For subStyle 20 with EIR authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_X01XXX()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._12;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
				AssertContainsExactElementsInAnyOrder("For Style '*01***' without SDE authorisation", new ZString[] { CusAuthorizationHelper.DeclarantRequiresSDEAuthorization(instruction.CEI_SubStyle, instruction.CEI_Style) }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				AssertEquals("For Style '*01***' with SDE authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_111XXX()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._12;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
				AssertContainsExactElementsInAnyOrder("For Style '111***' without SDE(Outward Processing) authorisation and For Style '11****' without OPO authorisation",
					new ZString[] { CusAuthorizationHelper.DeclarantRequiresSDEOutwardProcessingAuthorization(instruction.CEI_SubStyle, instruction.CEI_Style),
						CusAuthorizationHelper.DeclarantRequiresOPOAuthorization(instruction.CEI_SubStyle, instruction.CEI_Style) }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				var cusAuthorizationUsage2 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
				AssertEquals("For Style '111***' with SDE(Outward Processing) authorisation and For Style '11****' with OPO authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_X0X4XX()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._12;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
				AssertContainsExactElementsInAnyOrder("For Style '*0*4**' without CCL authorisation", new ZString[] { CusAuthorizationHelper.DeclarantRequiresCCLAuthorization(instruction.CEI_SubStyle, instruction.CEI_Style) }, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				AssertEquals("For Style '*0*4**' with CCL authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction).Any());
			});
		}

		public void TestValidateRequiredCusAuthorisationUsageForStyle_11X4XXAnd11XXXX()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._12;
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110400;
				AssertContainsExactElementsInAnyOrder("For Style '11*4**' without CCL authorisation and For Style '11****' without OPO authorisation",
					new ZString[] { CusAuthorizationHelper.DeclarantRequiresCCLAuthorization(instruction.CEI_SubStyle, instruction.CEI_Style),
						CusAuthorizationHelper.DeclarantRequiresOPOAuthorization(instruction.CEI_SubStyle, instruction.CEI_Style) },
					CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction));

				var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				var cusAuthorizationUsage2 = instruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
				AssertEquals("For Style '11*4**' with CCL authorisation and For For Style '11****' with OPO authorisation", false, CusAuthorizationHelper.ValidateRequiredCusAuthorisationUsageForStyleExport(instruction).Any());
			});
		}

		public void UpdateAuthorizationNumberOnEntryInstructionsIfNecessary_HasOneAuthorizationNumber()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var master1 = instruction1.PreviousDocumentMaster;
			master1.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var master2 = instruction2.PreviousDocumentMaster;
			master2.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			var declarantOrgHeader = Factory.New<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DEFAULT1");
			declarantOrgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DEFAULT2");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;
			master1.AuthorizationNumber = ZString.Empty;
			master2.AuthorizationNumber = ZString.Empty;

			CombineAssertions(() =>
			{
				declaration.UpdateAuthorizationNumberOnEntryInstructionsIfNecessary();
				AssertEquals("EntryInstruction1", "DEFAULT1", master1.AuthorizationNumber);
				AssertEquals("EntryInstruction2", "DEFAULT2", master2.AuthorizationNumber);
			});
		}

		public void UpdateAuthorizationNumberOnEntryInstructionsIfNecessary_HasMoreThanOneAuthorizationNumber()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var master1 = instruction1.PreviousDocumentMaster;
			master1.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var master2 = instruction2.PreviousDocumentMaster;
			master2.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			var declarantOrgHeader = Factory.New<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "Number1");
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "Number2");
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "Number3");
			var representativeOrgHeader = Factory.New<OrgHeader>();
			var representativeOrgAddress = representativeOrgHeader.MainAddress;
			representativeOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "Number4");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;
			declaration.JE_OA_Representative = representativeOrgAddress.PK;
			master1.AuthorizationNumber = ZString.Empty;
			master2.AuthorizationNumber = ZString.Empty;

			CombineAssertions(() =>
			{
				declaration.UpdateAuthorizationNumberOnEntryInstructionsIfNecessary();
				AssertEquals("EntryInstruction1", ZString.Empty, master1.AuthorizationNumber);
				AssertEquals("EntryInstruction2", ZString.Empty, master2.AuthorizationNumber);
			});
		}

		public void UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessary_HasOneAuthorizationNumber()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var master = instruction.PreviousDocumentMaster;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var declarantOrgHeader = Factory.New<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "Number1");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;
			master.AuthorizationNumber = ZString.Empty;

			instruction.UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessary();
			AssertEquals("Number1", master.AuthorizationNumber);
		}

		public void UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessary_HasMoreThanOneAuthorizationNumber()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var master = instruction.PreviousDocumentMaster;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var declarantOrgHeader = Factory.New<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "Number1");
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "Number2");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;
			master.AuthorizationNumber = ZString.Empty;

			instruction.UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessary();
			AssertEquals(ZString.Empty, master.AuthorizationNumber);
		}

		public void TestGetAuthorizationNumberIfOnlyOneExists_HasOneAuthorizationNumber()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var master = instruction.PreviousDocumentMaster;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var declarantOrgHeader = Factory.New<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "Number1");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;

			AssertEquals("Number1", master.GetAuthorizationNumberIfOnlyOneExists());
		}

		public void TestGetAuthorizationNumberIfOnlyOneExists_HasMoreThanOneAuthorizationNumber()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var master = instruction.PreviousDocumentMaster;
			master.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			var declarantOrgHeader = Factory.New<OrgHeader>();
			var declarantOrgAddress = declarantOrgHeader.MainAddress;
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "Number1");
			declarantOrgAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "Number2");

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;

			AssertEquals(ZString.Empty, master.GetAuthorizationNumberIfOnlyOneExists());
		}

		void SetupDeclaration(JobDeclaration declaration, CusEntryInstruction entryInstruction)
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "EDIBRNWIS";

			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "EDIBRNFRA";

			var subcontractor = Factory.New<OrgHeader>();
			subcontractor.OH_Code = "EDIBRNFXX";

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "EDIBRNFYY";

			var fromWarehouse = Factory.New<OrgHeader>();
			fromWarehouse.OH_Code = "EDIBRNMAZ";

			var declarantAddress = declarant.MainAddress;
			var representativeAddress = representative.MainAddress;
			var subcontractorAddress = subcontractor.MainAddress;
			var supplierAddress = supplier.MainAddress;

			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER2");
			declarantAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER3");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER4");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER5");
			representativeAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER6");
			subcontractorAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER7");
			subcontractorAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER8");
			subcontractorAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER9");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER10");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER11");
			supplierAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER12");

			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER30");
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER31");
			subcontractor.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER32");
			supplier.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER33");
			fromWarehouse.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "NUMBER34");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;
			declaration.JE_OA_SellerAddress = subcontractorAddress.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;

			if (entryInstruction != null)
			{
				var fromWarehouseAddress = fromWarehouse.MainAddress;
				fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER13");
				fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, "NUMBER14");
				fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER15");
				entryInstruction.CEI_OA_Warehouse = fromWarehouseAddress.PK;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		}
		OrgHeader orgHeader;
		JobDeclaration declaration;

		const string LocationRuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		const string UsageRuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
		const string CustomsOfficeLinkedRuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;

		void AssertHasSpecificAuthorisation(string authorisationType, Func<bool> hasAuthorisation)
		{
			AssertEquals("No Authorisation for Org", false, hasAuthorisation.Invoke());
			var authorization = orgHeader.CreateAuthorisationRecord(authorisationType, "DEFAULT1", Core.Constants.CountryCodes.Italy);
			AssertEquals($"Italian {authorisationType} Authorisation for Org", false, hasAuthorisation.Invoke());
			authorization.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			AssertEquals($"German {authorisationType} Authorisation for Org", true, hasAuthorisation.Invoke());
			authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
			AssertEquals("German CCO Authorisation for Org", false, hasAuthorisation.Invoke());
		}

		CusAuthorizationUsage GetCusAuthorizationUsageForTest(string style, string declarantType, string code)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var orgHeader1 = Factory.New<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = orgHeader1.MainAddress.PK;

			var orgHeader2 = Factory.New<OrgHeader>();
			declaration.JE_OA_Representative = orgHeader2.MainAddress.PK;

			var orgHeader3 = Factory.New<OrgHeader>();
			declaration.JE_OA_BuyingAgentAddress = orgHeader3.MainAddress.PK;

			declaration.JE_DeclarantType = declarantType;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = style;

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = code;

			return cusAuthorizationUsage;
		}
	}
}
