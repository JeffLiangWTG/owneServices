using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	internal class EDIOrgRelatedPartyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPR_FreightDirection()
		{
			var relatedParty = Factory.New<EDIOrgRelatedParty>();
			var ediPartyTypes = new ZString[] { EDIOrgRelatedPartyLookups.ContractingPartyCode, EDIOrgRelatedPartyLookups.WARPConstant, EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode };
			foreach (var partyType in ediPartyTypes)
			{
				relatedParty.PR_PartyType = partyType;
				relatedParty.PR_FreightDirection = "FWD";
				relatedParty.Validation.ValidatePR_FreightDirection();
				AssertNoErrors(relatedParty.PR_FreightDirectionInfo);

				relatedParty.PR_FreightDirection = "";
				relatedParty.Validation.ValidatePR_FreightDirection();
				AssertNoErrors(relatedParty.PR_FreightDirectionInfo);
			}
		}

		public void TestCheckCompanyLevelCOP()
		{
			EDIOrgRelatedParty relatedParty = Factory.New<EDIOrgRelatedParty>();
			relatedParty.PR_PartyType = EDIOrgRelatedPartyLookups.ContractingPartyCode;

			relatedParty.CompanyLevel = CompanyLevelList.Codes.COM;
			relatedParty.Validation.ValidateCompanyLevel();
			AssertHasErrors(relatedParty.CompanyLevelInfo);

			relatedParty.CompanyLevel = CompanyLevelList.Codes.ENT;
			relatedParty.Validation.ValidateCompanyLevel();
			AssertNoErrors(relatedParty.CompanyLevelInfo);
		}

		public void TestCheckCompanyLevelWARP()
		{
			EDIOrgRelatedParty relatedParty = Factory.New<EDIOrgRelatedParty>();
			relatedParty.PR_PartyType = EDIOrgRelatedPartyLookups.WARPConstant;

			relatedParty.CompanyLevel = CompanyLevelList.Codes.COM;
			relatedParty.Validation.ValidateCompanyLevel();
			AssertHasErrors(relatedParty.CompanyLevelInfo);

			relatedParty.CompanyLevel = CompanyLevelList.Codes.ENT;
			relatedParty.Validation.ValidateCompanyLevel();
			AssertNoErrors(relatedParty.CompanyLevelInfo);
		}

		public void TestCheckPR_OH_RelatedParty_IsNotSelfReferringWARPRelation()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			var relation1 = Factory.New<EDIOrgRelatedParty>();
			relation1.PR_OH_Parent = org1.PK;
			relation1.PR_OH_RelatedParty = org2.PK;
			relation1.PR_PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			AssertNoErrors(relation1.PR_OH_RelatedPartyInfo);

			relation1.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation1.PR_OH_RelatedPartyInfo, "You cannot have this organization be a party of itself for this type of party.");

			relation1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relation1.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation1.PR_OH_RelatedPartyInfo, "You cannot have this organization be a party of itself for this type of party.");

			relation1.PR_OH_RelatedParty = org2.PK;
			AssertNoErrors(relation1.PR_OH_RelatedPartyInfo);

			relation1.PR_GC = GlbCompany.CurrentCompany.PK;
			relation1.PR_OH_RelatedParty = org1.PK;
			AssertHasError(relation1.PR_OH_RelatedPartyInfo, "You cannot have this organization be a party of itself for this type of party.");

			relation1.PR_PartyType = RelatedPartyTypeList.Codes.ShipperBroker;
			relation1.Validation.ValidateAll();
			AssertNoErrors("Brokers can still refer to themselves", relation1.PR_OH_RelatedPartyInfo);
		}

		public void TestCheckPR_OH_RelatedParty_HasDifferentENTCode()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_FullName = "OrgParent";
			org1.OH_Code = "ORG1";
			org1.LicenceEnterpriseCode = "LE1";
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_FullName = "OrgRelatedParty";
			org2.OH_Code = "ORG2";
			org2.LicenceEnterpriseCode = "LE2";
			Factory.Save();

			var relation1 = Factory.New<EDIOrgRelatedParty>();
			relation1.PR_OH_Parent = org1.PK;
			relation1.PR_OH_RelatedParty = org2.PK;
			relation1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			AssertHasWarning("Should show warning when ENT.Code is different.", relation1.PR_OH_RelatedPartyInfo, "This organisation has a different enterprise code than OrgParent (ORG1). As this is a rare scenario, the relationship should be verified as being correct.");

			var relation2 = Factory.New<EDIOrgRelatedParty>();
			relation2.PR_OH_Parent = org1.PK;
			relation2.PR_OH_RelatedParty = org2.PK;
			relation2.PR_PartyType = RelatedPartyTypeList.Codes.AccountingVATGSTGroup;
			AssertNoWarnings("Shouldn't show warning when PartyType is not MNG.", relation2.PR_OH_RelatedPartyInfo);

			org1.LicenceEnterpriseCode = string.Empty;
			Factory.Save();
			var relation3 = Factory.New<EDIOrgRelatedParty>();
			relation3.PR_OH_Parent = org1.PK;
			relation3.PR_OH_RelatedParty = org2.PK;
			relation3.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			AssertNoWarnings("Shouldn't show warning when ENT.Code is empty.", relation3.PR_OH_RelatedPartyInfo);
		}

		public void TestAllowMultipleRelatedPartyTypeList_WARPConstant()
		{
			var orgParent = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgParent.OH_Code = "ParentOrg";

			var orgChild1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgChild1.OH_Code = "orgChild1";

			var orgChild2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgChild2.OH_Code = "orgChild2";

			var relatedParty1 = CreateRelatedParty(orgParent.PK, orgChild1.PK, EDIOrgRelatedPartyLookups.ContractingPartyCode);
			var relatedParty2 = CreateRelatedParty(orgParent.PK, orgChild2.PK, EDIOrgRelatedPartyLookups.ContractingPartyCode);

			AssertHasAlreadyInCollection(relatedParty1, true);
			AssertHasAlreadyInCollection(relatedParty2, true);

			relatedParty1.PR_PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			relatedParty1.PR_GC = GlbCompany.CurrentCompany.PK;
			relatedParty2.PR_PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			relatedParty2.PR_GC = GlbCompany.CurrentCompany.PK;

			AssertHasAlreadyInCollection(relatedParty1, false);
			AssertHasAlreadyInCollection(relatedParty2, false);
		}

		void AssertHasAlreadyInCollection(OrgRelatedParty relatedParty, bool expectedValidationError)
		{
			relatedParty.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				var expectedErrorMessage = "There is already a party with the same type, address, direction, mode, company level and UNLOCO. You can only specify a single related organization for this combination.";
				int propertyInfoWithErrorCount = 0;
				foreach (var propertyInfo in relatedParty.PropertiesWithNotifications)
				{
					if (propertyInfo.HasError(expectedErrorMessage))
					{
						propertyInfoWithErrorCount++;
					}
				}

				if (expectedValidationError)
				{
					Assert("Should have validation errors", propertyInfoWithErrorCount > 0);
					AssertHasError("PR_PartyType should have error", relatedParty.PR_PartyTypeInfo, expectedErrorMessage);
					AssertHasError("PR_Location should have error", relatedParty.PR_LocationInfo, expectedErrorMessage);
					AssertHasError("PR_FreightContainerMode should have error", relatedParty.PR_FreightContainerModeInfo, expectedErrorMessage);
					AssertHasError("PR_FreightTransportMode should have error", relatedParty.PR_FreightTransportModeInfo, expectedErrorMessage);
				}
				else
				{
					Assert("Should not have validation errors", propertyInfoWithErrorCount == 0);
					AssertNoErrors("PR_PartyType should have no error", relatedParty.PR_PartyTypeInfo);
					AssertNoErrors("PR_FreightContainerMode should have no error", relatedParty.PR_FreightContainerModeInfo);
					AssertNoErrors("PR_FreightTransportMode should have no error", relatedParty.PR_FreightTransportModeInfo);
					AssertNoErrors("PR_Location should have no error", relatedParty.PR_LocationInfo);
				}
			});
		}

		public void TestAllowMultipleRelatedPartyTypeList_Generic()
		{
			var orgParent = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgParent.OH_Code = "ParentOrg";

			var orgChild1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgChild1.OH_Code = "orgChild1";

			var orgChild2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgChild2.OH_Code = "orgChild2";

			var relatedParty1 = CreateRelatedParty(orgParent.PK, orgChild1.PK, RelatedPartyTypeList.Codes.ForwarderCoLoadWith);
			relatedParty1.PR_RN_NKImporterCountry = "AU";
			relatedParty1.PR_FreightTransportMode = ZString.Empty;
			relatedParty1.PR_FreightContainerMode = ZString.Empty;
			relatedParty1.PR_Service = ZString.Empty;
			relatedParty1.PR_Location = ZString.Empty;
			relatedParty1.PR_OA = ZGuid.Empty;
			var relatedParty2 = CreateRelatedParty(orgParent.PK, orgChild2.PK, RelatedPartyTypeList.Codes.ForwarderCoLoadWith);
			relatedParty2.PR_RN_NKImporterCountry = "NZ";
			relatedParty2.PR_FreightTransportMode = ZString.Empty;
			relatedParty2.PR_FreightContainerMode = ZString.Empty;
			relatedParty2.PR_Service = ZString.Empty;
			relatedParty2.PR_Location = ZString.Empty;
			relatedParty2.PR_OA = ZGuid.Empty;

			Factory.Save();

			Assert(relatedParty1.IsInDatabase);
			Assert(relatedParty2.IsInDatabase);
		}

		EDIOrgRelatedParty CreateRelatedParty(ZGuid parentGuid, ZGuid relatedPartyGuid, ZString partyType)
		{
			EDIOrgRelatedParty party = Factory.NewWithValidTestData<EDIOrgRelatedParty>();
			party.PR_OH_Parent = parentGuid;
			party.PR_OH_RelatedParty = relatedPartyGuid;
			party.PR_PartyType = partyType;
			party.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			party.PR_GC = GlbCompany.CurrentCompany.PK;
			party.PR_SystemCreateTimeUtc = ZDateTime.Today;
			return party;
		}
	}
}
