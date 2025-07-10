using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class FiscalRepresentativeDefaulterTest : TestCaseWithFactory
	{
		public void TestFRDefaultingOfFiscalReferences()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

				var cpvParty = Factory.NewWithValidTestData<OrgHeader>();
				cpvParty.OH_Code = "CPV PARTY";
				cpvParty.MainAddress.Address1 = "CPV ADDRESS";
				var eori = cpvParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "11111111111", Core.Constants.CountryCodes.France);

				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_Code = "IMPORTER";
				declaration.JE_OH_Importer = importer.PK;

				OrgRelatedParty relation = Factory.New<OrgRelatedParty>();
				relation.PR_OH_RelatedParty = cpvParty.PK;
				relation.PR_OH_Parent = importer.PK;
				relation.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
				relation.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;

				var euAddInfo = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
				euAddInfo.ZO_UseFr3FiscalRepresentation = false;
				Factory.Save();

				var cei1 = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("When a party of type CPV relates to the importer, but importer's UseFr3FiscalRepresentation is unticked, the FR entry instruction should not contain any FR3 type of fiscal reference", false, cei1.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));

				euAddInfo.ZO_UseFr3FiscalRepresentation = true;
				relation.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryAgent;
				Factory.Save();
				var cei2 = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("When no party of type CPV relates to the importer, the FR entry instruction should not contain any FR3 type of fiscal reference", false, cei2.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));

				relation.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;
				Factory.Save();
				var cei3 = declaration.CustomsEntryInstructions.AddNew();

				AssertEquals("When a party of type CPV relates to the importer, and importer's UseFr3FiscalRepresentation is ticked, the FR entry instruction should contain an FR3 type of fiscal reference", true, cei3.FiscalReferences.Cast<CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative));
				var fr3TypeOfFiscalReference = cei3.FiscalReferences.Cast<CusFiscalReference>().First(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
				AssertEquals("FR11111111111", fr3TypeOfFiscalReference.CFR_Reference);
				AssertEquals(cpvParty.MainAddress.EntityPK, fr3TypeOfFiscalReference.CFR_OA_Owner);
				AssertEquals("CPV ADDRESS", fr3TypeOfFiscalReference.CFR_OA_Owner_ZAddress.AddressFull);
			}
		}

		public void TestDefaultingOfFiscalReferences_InvoiceLine()
		{
			var declaration = GetJobDeclarationForTest();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			const string countryCode = Core.Constants.CountryCodes.Belgium;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					FiscalRepresentativeDefaulter.DefaultFiscalReferences(invoiceLine, FiscalReferenceCodeList.Codes.FR2_Customer);
					var fiscalReference = GetFR2FiscalReference(invoiceLine);
					AssertNotNull("The invoice line should contain a FR2 fiscal reference.", fiscalReference);
					AssertNullOrEmpty("No reference vat number should be given", fiscalReference.CFR_Reference);

					var buyer = Factory.NewWithValidTestData<OrgHeader>();
					buyer.OH_Code = "BUYER";
					buyer.MainAddress.Address1 = "BUYER ADDRESS";
					buyer.MainAddress.ClosestPort = countryCode + "PRT";

					declaration.JE_OA_ConsigneeAddress = buyer.MainAddress.PK;

					var importer = Factory.NewWithValidTestData<OrgHeader>();
					importer.OH_Code = "IMPORTER";
					importer.MainAddress.Address1 = "IMPORTER ADDRESS";
					importer.MainAddress.ClosestPort = countryCode + "PRT";

					declaration.JE_OH_Importer = importer.PK;

					declaration.Importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("BTW", "BTWIMPORTER", countryCode);

					var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
					FiscalRepresentativeDefaulter.DefaultFiscalReferences(invoiceLine2, FiscalReferenceCodeList.Codes.FR2_Customer);
					fiscalReference = GetFR2FiscalReference(invoiceLine2);
					AssertEquals("Fiscal reference should be 'BTWIMPORTER'", "BTWIMPORTER", fiscalReference.CFR_Reference);
					AssertEquals("IMPORTER ADDRESS BEPRT", "IMPORTER ADDRESS BEPRT", fiscalReference.CFR_OA_Owner_ZAddress.AddressFull);

					declaration.ConsigneeOrgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("BTW", "BTWBUYER", countryCode);

					var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
					FiscalRepresentativeDefaulter.DefaultFiscalReferences(invoiceLine3, FiscalReferenceCodeList.Codes.FR2_Customer);
					fiscalReference = GetFR2FiscalReference(invoiceLine3);
					AssertEquals("Fiscal reference should be 'BTWBUYER'", "BTWBUYER", fiscalReference.CFR_Reference);
					AssertEquals("BUYER ADDRESS BEPRT", "BUYER ADDRESS BEPRT", fiscalReference.CFR_OA_Owner_ZAddress.AddressFull);
				});
			}
		}

		public void TestDefaultingOfFiscalReferences_InvoiceLine_NoUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				FiscalRepresentativeDefaulter.DefaultFiscalReferences(invoiceLine, FiscalReferenceCodeList.Codes.FR2_Customer);
				var fiscalReference = GetFR2FiscalReference(invoiceLine);
				AssertNull("The invoice line should not contain a FR2 fiscal reference.", fiscalReference);
			}
		}

		public void TestRemoveFiscalReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var fiscalReference = cei.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalReference.CFR_Reference = "123";

			CombineAssertions(() =>
			{
				AssertEquals("A fiscal reference should exist for this entry instruction", 1, cei.FiscalReferences.Count);
				FiscalRepresentativeDefaulter.RemoveFiscalReference(cei, FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
				AssertEquals("No fiscal reference expected after removal of FR3 type", 0, cei.FiscalReferences.Count);
			});
		}

		public void TestGetFiscalReferenceRelatedParty_WhenNullOrNoImporter()
		{
			var cei = Factory.New<CusEntryInstruction>();
			var (owner, ownerRef) = cei.FiscalRepresentativeDefaulter.GetFiscalReferenceRelatedParty(cei);
			AssertEquals(null, owner);
			AssertEquals(ZString.Empty, ownerRef);

			var declaration = GetJobDeclarationForTest();
			cei = declaration.CustomsEntryInstructions.AddNew();
			(owner, ownerRef) = cei.FiscalRepresentativeDefaulter.GetFiscalReferenceRelatedParty(cei);
			AssertEquals(null, owner);
			AssertEquals(ZString.Empty, ownerRef);

			declaration = GetJobDeclarationForTest();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TSTI");
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "987654321", declaration.Company.Country);
			declaration.JE_OH_Importer = importer.PK;
			cei = declaration.CustomsEntryInstructions.AddNew();
			(owner, ownerRef) = cei.FiscalRepresentativeDefaulter.GetFiscalReferenceRelatedParty(cei);
			AssertEquals(null, owner);
			AssertEquals(ZString.Empty, ownerRef);
		}

		public void TestGetFiscalReferenceRelatedParty_WhenNoCpvRelatedOrg()
		{
			var declaration = Factory.New<JobDeclaration>();

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
			AssertEquals(null, owner);
			AssertEquals(ZString.Empty, ownerRef);
		}

		public void TestGetFiscalReferenceRelatedParty()
		{
			var declaration = Factory.New<JobDeclaration>();

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
			AssertEquals(declaration.Company.Country.Code + "TSTR", ownerRef);
		}

		static CusFiscalReference GetFR2FiscalReference(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Factory.Load<CusFiscalReference>(new ZQuery(CusReferenceSchema.CFR_ParentID, invoiceLine.PK)).FirstOrDefault(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer);
		}

		protected virtual JobDeclaration GetJobDeclarationForTest() => Factory.New<JobDeclaration>();
	}
}
