using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	class CDSFiscalRepresentativeDefaulterTest : EU.Business.Declaration.Testing.FiscalRepresentativeDefaulterTest
	{
		public new void TestGetFiscalReferenceRelatedParty_WhenNoCpvRelatedOrg()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTR");
			relatedParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789", declaration.Company.Country);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTI");
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "987654321", declaration.Company.Country);

			var euObj = EUOrgImpAddInfo.Get(importer, declaration.Company.Country.Code);
			euObj.ZO_UseFr3FiscalRepresentation = true;
			var importerRelatedParty = importer.AllRelatedParties.AddNew();
			importerRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			importerRelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			importerRelatedParty.PR_OH_RelatedParty = relatedParty.PK;
			declaration.JE_OH_Importer = importer.PK;

			var cei = declaration.CustomsEntryInstructions.AddNew();

			var (owner, ownerRef) = cei.FiscalRepresentativeDefaulter.GetFiscalReferenceRelatedParty(cei);
			AssertEquals(importer.PK, owner.PK);
			AssertEquals(declaration.Company.Country.Code + "987654321", ownerRef);
		}

		public new void TestGetFiscalReferenceRelatedParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTR");
			relatedParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789", declaration.Company.Country);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTI");
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "987654321", declaration.Company.Country);

			var euObj = EUOrgImpAddInfo.Get(importer, declaration.Company.Country.Code);
			euObj.ZO_UseFr3FiscalRepresentation = true;
			var importerRelatedParty = importer.AllRelatedParties.AddNew();
			importerRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;
			importerRelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			importerRelatedParty.PR_OH_RelatedParty = relatedParty.PK;
			declaration.JE_OH_Importer = importer.PK;

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var (owner, ownerRef) = cei.FiscalRepresentativeDefaulter.GetFiscalReferenceRelatedParty(cei);
			AssertEquals(relatedParty.PK, owner.PK);
			AssertEquals(declaration.Company.Country.Code + "123456789", ownerRef);
		}

		protected override JobDeclaration GetJobDeclarationForTest()
		{
			var declaration =  base.GetJobDeclarationForTest();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}
	}
}
