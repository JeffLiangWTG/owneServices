using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DeltaIEApplicationExtender))]
	class DeltaIEApplicationExtenderTest : ApplicationExtenderAbstractTest
	{
		public override void TestGetJobDeclarationValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeltaIEJobDeclarationValueSetStrategy>(applicationExtender.GetJobDeclarationValueSetStrategy(declaration));
		}

		public override void TestGetCusEntryInstructionValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<DeltaIECusEntryInstructionValueSetStrategy>(applicationExtender.GetCusEntryInstructionValueSetStrategy(entryInstruction));
		}

		public override void TestGetCusAuthorizationUsageValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var usage = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertType<DeltaIECusAuthorizationUsageValueSetStrategy>(applicationExtender.GetCusAuthorizationUsageValueSetStrategy(usage));
		}

		public override void TestGetVATDeferStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeltaIEVATDeferStrategy>(applicationExtender.GetVATDeferStrategy(declaration));
		}

		public override void TestIsUCC6()
		{
			AssertEquals(true, applicationExtender.IsUCC6);
		}

		public override void TestAmendmentSnapshotMessageType()
		{
			AssertEquals(DeclarationApplicationCodeList.Codes.DeltaIE, applicationExtender.AmendmentSnapshotMessageType);
		}

		public override void TestGetEffectiveCountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.ZG_CountryOfSupply = "DE";
			AssertEquals("When it is UCC6 Import, we use ZG_CountryOfSupply as country of origin.", "DE", applicationExtender.GetEffectiveCountryOfOrigin(invoiceLine));

			declaration.JE_MessageType = "EXP";
			AssertEquals("When it is UCC6 Export, we don't have a specific country of origin, and we will use logic from base.", "", applicationExtender.GetEffectiveCountryOfOrigin(invoiceLine));

			declaration.JE_MessageType = "IMP";
			invoiceLine.ZG_CountryOfSupply = "";
			AssertEquals("When it is UCC6 Import, we use ZG_CountryOfSupply as country of origin, here it is empty, and we will use logic from base.", "", applicationExtender.GetEffectiveCountryOfOrigin(invoiceLine));
		}

		public override void TestGetDataGroupingForCusProcedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var countryCode = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaIE).GetDataGroupingForCusProcedure(declaration);
			AssertEquals("For DeltaIE, DataGroupingForCusProcedure should be DIE.", Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, countryCode);
		}

		public override void TestGetDataGroupingForAdditionalDocumentCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var countryCode = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaIE).GetDataGroupingForAdditionalDocumentCodes(declaration);
			AssertEquals("For DeltaIE, DataGroupingForAdditionalDocumentCodes should be DIE.", Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, countryCode);
		}

		public override void TestGetDefinedDeclarationTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = "EXP";
				var list = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaIE).GetDefinedDeclarationTypeList(declaration);
				AssertEquals("EXP", new DeltaIEExportDeclarationTypeList().CodesAsString, list.CodesAsString);

				declaration.JE_MessageType = "IMP";
				list = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaIE).GetDefinedDeclarationTypeList(declaration);
				AssertEquals("IMP", new DeltaIEImportDeclarationTypeList().CodesAsString, list.CodesAsString);
			});
		}

		public override void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEJobDeclarationValidation>(declaration.Validation);
		}

		public override void TestGetNewLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEJobDeclarationLookups>(declaration.Lookups);
		}

		public override void TestGetAdditionalInfoValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var additionInfo = declaration.AdditionalInfos.AddNew();
			AssertType<DeltaIEAdditionalInfoValidation>(additionInfo.Validation);
		}

		public override void TestGetVATNumberSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIEVATNumberSupporter>(declaration.VATNumberSupporter);
		}

		public override void TestGetCustomsProfileRelatedAccount()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_CustomsProfile = ZString.Empty;

			AssertNull("No related account should found when the profile is not set.", declaration.JE_CustomsProfileRelatedAccount);
			declaration.JE_CustomsProfile = "TESTACC";
			AssertNull("No related account should found when selected profile is not linked to any org", declaration.JE_CustomsProfileRelatedAccount);
			orgCusAccount.CZ_OH = importer.PK;
			AssertEquals("The related acount should be the Customs profile account of type DCN tied to the profile organisation.", orgCusAccount, declaration.JE_CustomsProfileRelatedAccount);
		}

		public override void TestGetJobComInvoiceHeaderValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaIEJobComInvoiceHeaderValidation>(applicationExtender.GetJobComInvoiceHeaderValidation(invoiceHeader));
		}

		public override void TestGetJobComInvoiceHeaderLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaIEJobComInvoiceHeaderLookups>(applicationExtender.GetJobComInvoiceHeaderLookups(invoiceHeader));
		}

		public override void TestGetJobComInvoiceLineValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<DeltaIEJobComInvoiceLineValidation>(applicationExtender.GetJobComInvoiceLineValidation(invoiceLine));
		}

		public override void TestGetJobComInvoiceLineLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<DeltaIEJobComInvoiceLineLookups>(applicationExtender.GetJobComInvoiceLineLookups(invoiceLine));
		}

		public override void TestGetJobComInvoiceLineValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<DeltaIEJobComInvoiceLineValueSetStrategy>(applicationExtender.GetJobComInvoiceLineValueSetStrategy(invoiceLine));
		}

		public override void TestGetCusEntryInstructionValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<CusEntryInstructionValidation>(applicationExtender.GetCusEntryInstructionValidation(entryInstruction));
		}

		public override void TestGetCusEntryInstructionLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<DeltaIECusEntryInstructionLookups>(applicationExtender.GetCusEntryInstructionLookups(entryInstruction));
		}

		public override void TestGetCusAuthorizationUsageLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertType<DeltaIECusAuthorizationUsageLookups>(applicationExtender.GetCusAuthorizationUsageLookups(cusAuthorizationUsage));
		}

		public override void TestGetJobComInvoiceHeaderValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaIEJobComInvoiceHeaderValueSetStrategy>(applicationExtender.GetJobComInvoiceHeaderValueSetStrategy(invoiceHeader));
		}

		public override void TestGetAddInfoCusEntryInstructionValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var addInfoCusEntryInstruction = entryInstruction.AddInfo;
			AssertType<AddInfoCusEntryInstructionValidation>(applicationExtender.GetAddInfoCusEntryInstructionValidation(addInfoCusEntryInstruction));
		}

		public override void TestGetJobComInvoiceHeaderValuePostProcessingStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaIEJobComInvoiceHeaderValuePostProcessingStrategy>(applicationExtender.GetJobComInvoiceHeaderValuePostProcessingStrategy(invoiceHeader));
		}

		public override void TestCanBeRevertedToLastBAE()
		{
			var entry = Factory.New<CusEntryHeader>();
			Assert(!applicationExtender.CanBeRevertedToLastBAE(entry));
		}

		public override void TestIsEntryInstructionOutOfInward()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("IsEntryInstructionOutOfInward is always false", false, applicationExtender.IsEntryInstructionOutOfInward(entryInstruction));
		}

		public override void TestGetCorrelationIDPrefix()
		{
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "WTL"))
			{
				Env.Registry.PhysicalServerID = "FRM";
				GlbCompany.CurrentCompany.GC_Code = "DFR";
				AssertEquals("There should be no CorrelationID prefix for DeltaG declarations.", "WTLDFRFRM", applicationExtender.GetCorrelationIDPrefix());
			}
		}

		public override void TestIsEntryStatusCleared()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("IsEntryStatusCleared should be false when EntryStatus is not Released.", false, applicationExtender.IsEntryStatusCleared(entry));

			entry.CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Released;
			AssertEquals("IsEntryStatusCleared should be true when EntryStatus is Released.", true, applicationExtender.IsEntryStatusCleared(entry));
		}

		public override void TestGetDeltaAccounts()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			var importerAccountDCC = declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			var declarantAccountDCC = declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DIE002", ZString.Empty, ZString.Empty, "B26F06FF");
			var representativeAccountDCC = declaration.SetupRepresentative().Header.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DIE003", ZString.Empty, ZString.Empty, "B26F06FF");
			var importerAccountDCN = declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCN, "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			var importerAccountHDN = declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.HDN, "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");

			AssertContainsExactElementsInExactOrder("For DeltaIE + Import, we should retrieve DEC accounts from Importer, Declarant and Representative.", [importerAccountDCC, importerAccountDCN, importerAccountHDN, declarantAccountDCC, representativeAccountDCC], applicationExtender.GetDeltaAccounts(declaration));
		}

		public override void TestGetCusEntryLineFeeLookups()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertType<DeltaIECusEntryLineFeeLookups>("CusEntryLineFeeLookups should be DeltaIECusEntryLineFeeLookups in DeltaIE declaration.", applicationExtender.GetCusEntryLineFeeLookups(entryLineFee));
		}
	}
}
