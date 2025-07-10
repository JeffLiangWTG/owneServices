using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DeltaGApplicationExtender))]
	class DeltaGApplicationExtenderTest : ApplicationExtenderAbstractTest
	{
		public override void TestGetJobDeclarationValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeltaGJobDeclarationValueSetStrategy>(applicationExtender.GetJobDeclarationValueSetStrategy(declaration));
		}

		public override void TestGetCusEntryInstructionValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<CusEntryInstructionValueSetStrategy>(applicationExtender.GetCusEntryInstructionValueSetStrategy(entryInstruction));
		}

		public override void TestGetCusAuthorizationUsageValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var usage = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertType<CusAuthorizationUsageValueSetStrategy>(applicationExtender.GetCusAuthorizationUsageValueSetStrategy(usage));
		}

		public override void TestGetVATDeferStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeltaGVATDeferStrategy>(applicationExtender.GetVATDeferStrategy(declaration));
		}

		public override void TestIsUCC6()
		{
			AssertEquals(false, applicationExtender.IsUCC6);
		}

		public override void TestAmendmentSnapshotMessageType()
		{
			AssertEquals(DeclarationApplicationCodeList.Codes.DeltaG, applicationExtender.AmendmentSnapshotMessageType);
		}

		public override void TestGetEffectiveCountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.ZG_CountryOfSupply = "DE";
			AssertEquals("There is no FR specific country of origin for Delta G, and we will use logic from base.", "", applicationExtender.GetEffectiveCountryOfOrigin(invoiceLine));
		}

		public override void TestGetDataGroupingForCusProcedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var countryCode = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaG).GetDataGroupingForCusProcedure(declaration);
			AssertEquals("For DeltaG, DataGroupingForCusProcedure from ApplicationExtender should be empty.", ZString.Empty, countryCode);
		}

		public override void TestGetDataGroupingForAdditionalDocumentCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var countryCode = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaG).GetDataGroupingForAdditionalDocumentCodes(declaration);
			AssertEquals("For DeltaG, DataGroupingForAdditionalDocumentCodes from ApplicationExtender should be empty.", ZString.Empty, countryCode);
		}

		public override void TestGetDefinedDeclarationTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = "EXP";
				var list = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaG).GetDefinedDeclarationTypeList(declaration);
				AssertEquals("EXP", new DeltaGExportDeclarationTypeList().CodesAsString, list.CodesAsString);

				declaration.JE_MessageType = "IMP";
				list = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.DeltaG).GetDefinedDeclarationTypeList(declaration);
				AssertEquals("IMP", new DeltaGImportDeclarationTypeList().CodesAsString, list.CodesAsString);
			});
		}

		public override void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeltaGJobDeclarationValidation>(declaration.Validation);
		}

		public override void TestGetNewLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertType<DeltaGJobDeclarationLookups>(declaration.Lookups);
		}

		public override void TestGetAdditionalInfoValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var additionInfo = declaration.AdditionalInfos.AddNew();
			AssertType<AdditionalInfoValidation>(additionInfo.Validation);
		}

		public override void TestGetVATNumberSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeltaGVATNumberSupporter>(declaration.VATNumberSupporter);
		}

		public override void TestGetCustomsProfileRelatedAccount()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			CombineAssertions(() =>
			{
				AssertNull("JE_CustomsProfile Empty.", declaration.JE_CustomsProfileRelatedAccount);
				declaration.JE_CustomsProfile = "TESTACC";
				AssertNull("No Account was found for the Org.", declaration.JE_CustomsProfileRelatedAccount);
				orgCusAccount.CZ_OH = importer.PK;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertNull("No Account of correct type was found.", declaration.JE_CustomsProfileRelatedAccount);
				orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertEquals("Account found.", orgCusAccount, declaration.JE_CustomsProfileRelatedAccount);
			});
		}

		public override void TestGetJobComInvoiceHeaderValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaGJobComInvoiceHeaderValidation>(applicationExtender.GetJobComInvoiceHeaderValidation(invoiceHeader));
		}

		public override void TestGetJobComInvoiceHeaderLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaGJobComInvoiceHeaderLookups>(applicationExtender.GetJobComInvoiceHeaderLookups(invoiceHeader));
		}

		public override void TestGetJobComInvoiceLineValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<DeltaGJobComInvoiceLineValidation>(applicationExtender.GetJobComInvoiceLineValidation(invoiceLine));
		}

		public override void TestGetJobComInvoiceLineLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<JobComInvoiceLineLookups>(applicationExtender.GetJobComInvoiceLineLookups(invoiceLine));
		}

		public override void TestGetJobComInvoiceLineValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<JobComInvoiceLineValueSetStrategy>(applicationExtender.GetJobComInvoiceLineValueSetStrategy(invoiceLine));
		}

		public override void TestGetCusEntryInstructionValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<DeltaGCusEntryInstructionValidation>(applicationExtender.GetCusEntryInstructionValidation(entryInstruction));
		}

		public override void TestGetCusEntryInstructionLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertType<CusEntryInstructionLookups>(applicationExtender.GetCusEntryInstructionLookups(entryInstruction));
		}

		public override void TestGetCusAuthorizationUsageLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertType<DeltaGCusAuthorizationUsageLookups>(applicationExtender.GetCusAuthorizationUsageLookups(cusAuthorizationUsage));
		}

		public override void TestGetJobComInvoiceHeaderValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaGJobComInvoiceHeaderValueSetStrategy>(applicationExtender.GetJobComInvoiceHeaderValueSetStrategy(invoiceHeader));
		}

		public override void TestGetAddInfoCusEntryInstructionValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var addInfoCusEntryInstruction = entryInstruction.AddInfo;
			AssertType<DeltaGAddInfoCusEntryInstructionValidation>(applicationExtender.GetAddInfoCusEntryInstructionValidation(addInfoCusEntryInstruction));
		}

		public override void TestGetJobComInvoiceHeaderValuePostProcessingStrategy()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertType<DeltaGJobComInvoiceHeaderValuePostProcessingStrategy>(applicationExtender.GetJobComInvoiceHeaderValuePostProcessingStrategy(invoiceHeader));
		}

		public override void TestCanBeRevertedToLastBAE()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			declaration.ActiveEntryHeaders.Add(entry);
			Assert(!applicationExtender.CanBeRevertedToLastBAE(entry));
			var outgoingMessage = AddOutgoingMessage(entry);
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.REC;
			var incomingMessage = AddIncomingMessage(entry, "Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");

			Assert("Prerequisite: entry is in status of RectificationError", new DeltaGStatusResolver(entry, incomingMessage).CheckIsRectificationError());
			Assert(applicationExtender.CanBeRevertedToLastBAE(entry));

			FREDIMessage AddOutgoingMessage(CusEntryHeader entry)
			{
				var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				outgoingInterchange.EI_InterchangeNum = "123";
				var outgoingMessage = Factory.NewWithValidTestData<FREDIMessage>();
				outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
				entry.Messages.Add(outgoingMessage);
				outgoingMessage.EM_EI = outgoingInterchange.PK;
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				Factory.Save();
				return outgoingMessage;
			}

			FREDIMessage AddIncomingMessage(CusEntryHeader entry, string messageFile)
			{
				var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				incomingInterchange.EI_InterchangeNum = "123.";
				var incomingMessage = Factory.NewWithValidTestData<DeltaCImportFREDIMessage>();
				entry.Messages.Add(incomingMessage);
				incomingMessage.EM_EI = incomingInterchange.PK;
				incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				incomingMessage.EM_MessageText = resourceRetriever.Value.GetString(messageFile);
				return incomingMessage;
			}
		}
		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		public override void TestIsEntryInstructionOutOfInward()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.PermanentExportWithEI;
			AssertEquals("IsEntryInstructionOutOfInward is false when CEI_Style is not 31P", false, applicationExtender.IsEntryInstructionOutOfInward(entryInstruction));

			entryInstruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ReExportOfNonUnionGoodsWithEI;
			AssertEquals("IsEntryInstructionOutOfInward is true when CEI_Style is 31P", true, applicationExtender.IsEntryInstructionOutOfInward(entryInstruction));
		}

		public override void TestGetCorrelationIDPrefix()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("There should be no CorrelationID prefix for DeltaG declarations.", ZString.Empty, applicationExtender.GetCorrelationIDPrefix());
		}

		public override void TestIsEntryStatusCleared()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("IsEntryStatusCleared should always be false in DeltaG.", false, applicationExtender.IsEntryStatusCleared(entry));
		}

		public override void TestGetDeltaAccounts()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			var importerAccountDGI = declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, ZString.Empty, "DGI001", ZString.Empty, ZString.Empty, "B26F06FF");
			var declarantAccountDGI = declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, ZString.Empty, "DGI002", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.SetupRepresentative().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, ZString.Empty, "DGI003", ZString.Empty, ZString.Empty, "B26F06FF");

			AssertContainsExactElementsInExactOrder("For DeltaG + Import, we should only retrieve DGI accounts from Importer and Declarant.", [importerAccountDGI, declarantAccountDGI], applicationExtender.GetDeltaAccounts(declaration));

			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Export);
			var supplierAccountDGE = declaration.SetupSupplier().SetupAccount(OrgCusAccountCodeList.Codes.DGE, ZString.Empty, "DGE004", ZString.Empty, ZString.Empty, "B26F06FF");
			var declarantAccountDGE = declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGE, ZString.Empty, "DGE005", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.SetupRepresentative().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGE, ZString.Empty, "DGE006", ZString.Empty, ZString.Empty, "B26F06FF");

			AssertContainsExactElementsInExactOrder("For DeltaG + Export, we should only retrieve DGE accounts from Supplier and Declarant.", [supplierAccountDGE, declarantAccountDGE], applicationExtender.GetDeltaAccounts(declaration));
		}

		public override void TestGetCusEntryLineFeeLookups()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertType<DeltaGCusEntryLineFeeLookups>("CusEntryLineFeeLookups should be DeltaGCusEntryLineFeeLookups in DeltaG declaration.", applicationExtender.GetCusEntryLineFeeLookups(entryLineFee));
		}
	}
}
