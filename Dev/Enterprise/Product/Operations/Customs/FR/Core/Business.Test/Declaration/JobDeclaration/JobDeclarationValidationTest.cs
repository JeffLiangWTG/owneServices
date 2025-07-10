using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_DeclarantType()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerMainAddress = importer.MainAddress;
			importerMainAddress.OA_RN_NKCountryCode = "AU";
			var importerAddress = importer.Addresses.AddNew(OrgAddressType.Delivery, false);
			importerAddress.OA_RN_NKCountryCode = "AU";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierMainAddress = supplier.MainAddress;
			supplierMainAddress.OA_RN_NKCountryCode = "FR";
			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Pickup, false);
			supplierAddress.OA_RN_NKCountryCode = "AU";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError(declaration.JE_DeclarantTypeInfo, "For non-EU office of Importer it is legally forbidden to use Direct Representation.");

			importerMainAddress.OA_RN_NKCountryCode = "DE";
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError(declaration.JE_DeclarantTypeInfo, "For non-EU office of Importer it is legally forbidden to use Direct Representation.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError(declaration.JE_DeclarantTypeInfo, "For non-EU office of Supplier it is legally forbidden to use Direct Representation.");

			supplierMainAddress.OA_RN_NKCountryCode = "CN";
			declaration.Validation.ValidateJE_DeclarantType();
			AssertHasMessageError(declaration.JE_DeclarantTypeInfo, "For non-EU office of Supplier it is legally forbidden to use Direct Representation.");

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			importerMainAddress.OA_RN_NKCountryCode = "AU";
			supplierMainAddress.OA_RN_NKCountryCode = "CN";
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError(declaration.JE_DeclarantTypeInfo, "For non-EU office of Supplier it is legally forbidden to use Direct Representation.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			declaration.Validation.ValidateJE_DeclarantType();
			AssertNoMessageError(declaration.JE_DeclarantTypeInfo, "For non-EU office of Importer it is legally forbidden to use Direct Representation.");
		}

		public void TestCheckJE_ApplicationCodeRuleNat_145Bis()
		{
			var message = "[NAT_145BIS] A0010 Additional Information is mandatory for Delta Import declarations.";
			var declaration = Factory.New<JobDeclaration>();

			using var context = new Business.Testing.ConfigurationProviders.DeclarationValidationDeciderTestContext(declaration);
			context.EnableRule(x => x.IsRuleNat_145BisActive);
			AssertNoMessageErrorContaining("No MessageError is expected, when RuleNat_145Bis is enabled as Default Additional Information is auto-populated for DeltaIE Import type.", declaration.JE_ApplicationCodeInfo, message);

			declaration.AdditionalInfos.RemoveAndDeleteAll();
			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining("MessageError is expected, When RuleNat_145Bis is enabled as Default Additional Information is not populated for DeltaIE Import type.", declaration.JE_ApplicationCodeInfo, message);

			context.DisableRule(x => x.IsRuleNat_145BisActive);
			declaration.AdditionalInfos.RemoveAndDeleteAll();
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining("Regardless of whether the declaration has A0010 Additional Information or not, there should be no RuleNAT_145Bis message error, When RuleNat_145Bis is disabled.", declaration.JE_ApplicationCodeInfo, message);
		}

		public void TestCheckJE_CustomsOfficeRuleNat_021()
		{
			var messageError = "You have entered a concession F48, the declaration office must be located in metropolitan France.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using var context = new Business.Testing.ConfigurationProviders.DeclarationValidationDeciderTestContext(declaration);
			context.EnableRule(x => x.IsRuleNAT_021Active);
			invoiceLine.JI_Procedure = "1234F48";
			declaration.JE_CustomsOffice = "FR123";
			AssertNoMessageError("No MessageError is expected when declaration office is in France.", declaration.JE_CustomsOfficeInfo, messageError);

			declaration.JE_CustomsOffice = "AU123";
			AssertHasMessageError("MessageError is expected when declaration office is outside France.", declaration.JE_CustomsOfficeInfo, messageError);

			invoiceLine.JI_Procedure = "1234F49";
			declaration.JE_CustomsOffice = "FR123";
			AssertNoMessageError("No MessageError is expected when declaration office is in France.", declaration.JE_CustomsOfficeInfo, messageError);

			context.DisableRule(x => x.IsRuleNAT_021Active);
			invoiceLine.JI_Procedure = "1234F48";
			declaration.JE_CustomsOffice = "FR123";
			AssertNoMessageError("No MessageError is Expected When rule NAT_021 is disabled, regardless of procedure or declaration office.", declaration.JE_CustomsOfficeInfo, messageError);

			declaration.JE_CustomsOffice = "AU123";
			AssertNoMessageError("No MessageError is Expected When rule NAT_021 is disabled, regardless of procedure or declaration office.", declaration.JE_CustomsOfficeInfo, messageError);
		}

		public void TestCheckJE_OH_ControllingCustomer()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OH_ControllingCustomer = orgHeader.PK;
			var address = jobDeclaration.ControllingCustomer.MainAddress;
			var info = jobDeclaration.JE_OH_ControllingCustomerInfo;
			string error = "[NAT_020] The selected organization does not have an EORI.";
			DeclarationValidationTestHelper.AssertEORI(jobDeclaration, address, info, jobDeclaration.Validation.ValidateJE_OH_ControllingCustomer, error);
		}

		public void TestCheckJE_OA_Representative_EORI()
		{
			var message = "[NAT_020] The selected organization does not have an EORI.";

			var representative = Factory.NewWithValidTestData<OrgAddress>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using var context = new Business.Testing.ConfigurationProviders.DeclarationValidationDeciderTestContext(declaration);
			context.EnableRule(x => x.IsRuleNAT_020Active);
			declaration.JE_OA_Representative = representative.PK;
			AssertHasMessageError("MessageError is expected when representative has no EORI", declaration.JE_OA_RepresentativeInfo, message);

			var eori = declaration.Representative.Header.CustomsCodes.AddNew();
			eori.OK_CodeType = "EOR";
			eori.OK_RN_NKCodeCountry = "FR";
			eori.OK_CustomsRegNo = "regno";
			declaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError("No MessageError is expected when representative has a valid EORI", declaration.JE_OA_RepresentativeInfo, message);
		}

		public void TestCheckChargePaymentOrDestinationID()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[40LCL] * 10 + [20LCL] * 5 + [45LCL] * 2", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX BASSENS 2", "FRBAS", "FR000100");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "395", "BORDEAUX BASSENS 3", "FRBAS", "FR000111");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			AssertNoNotifications("No message errors when list is empty and value is empty.", declaration.ChargePaymentOrDestinationIDInfo);

			declaration.ChargePaymentOrDestinationID = "010";

			AssertHasMessageErrorContaining(declaration.ChargePaymentOrDestinationIDInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR000100";
			declaration.JE_RL_NKPortOfArrival = "FRBAS";

			declaration.ChargePaymentOrDestinationID = ZString.Empty;
			AssertNoNotifications("No message error when all no THI code in the list has harbour rate and no value is selected.", declaration.ChargePaymentOrDestinationIDInfo);

			declaration.ChargePaymentOrDestinationID = "999";
			AssertHasMessageErrorContaining(declaration.ChargePaymentOrDestinationIDInfo, ListValidation.InvalidCodeMessageError);

			declaration.ChargePaymentOrDestinationID = "010";
			AssertNoNotifications(declaration.ChargePaymentOrDestinationIDInfo);

			declaration.ChargePaymentOrDestinationID = "555";
			AssertHasMessageErrorContaining(declaration.ChargePaymentOrDestinationIDInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsOffice = "FR000111";
			declaration.ChargePaymentOrDestinationID = ZString.Empty;
			AssertHasMessageErrorContaining("Message error should be added when one of the THI codes in the list is has harbour rate but no value is selected.", declaration.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPackagesActualPackageCountWhenDeclarationIsUCC6()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "BX";
			package1.CW_PackQty = 10;
			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackType = "BX";
			package2.CW_PackQty = 10;
			var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackType = "BG";
			package3.CW_PackQty = 10;

			var linkPackageCollection1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var linkPackageCollection2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly;

			linkPackageCollection1[0].IsLinked = true;
			linkPackageCollection1[0].PackQty = 5;
			linkPackageCollection1[1].IsLinked = true;
			linkPackageCollection1[1].PackQty = 4;
			linkPackageCollection1[2].IsLinked = true;
			linkPackageCollection1[2].PackQty = 3;
			linkPackageCollection2[0].IsLinked = true;
			linkPackageCollection2[0].PackQty = 5;
			linkPackageCollection2[1].IsLinked = true;
			linkPackageCollection2[1].PackQty = 6;
			linkPackageCollection2[2].IsLinked = true;
			linkPackageCollection2[2].PackQty = 5;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining("No message error is expected when lines have not already merged.", declaration.PackagesActualPackageCountInfo, "Not all packs of type");

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.PackagesActualPackageCountInfo, "Not all packs of type");

			linkPackageCollection2[2].PackQty = 7;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.PackagesActualPackageCountInfo, "Not all packs of type");
		}

		public void TestCheckPackagesActualPackageCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var package = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package.CW_PackType = "CT";
			package.CW_PackQty = 10;
			var linkPackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining("No message error is expected when lines have not already merged.", declaration.PackagesActualPackageCountInfo, "Not all packs of type");

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.PackagesActualPackageCountInfo, "Not all packs of type");

			linkPackageCollection[0].IsLinked = true;
			linkPackageCollection[0].PackQty = 5;
			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.PackagesActualPackageCountInfo, "Not all packs of type");

			linkPackageCollection[0].PackQty = 10;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.PackagesActualPackageCountInfo, "Not all packs of type");
		}

		public void TestCheckJE_ShipmentIncoTerm()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ShipmentIncoTerm = declaration.Lookups.IncoTermList[0].Code;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertNoMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ShipmentIncoTerm = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertHasMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ShipmentIncoTerm = declaration.Lookups.IncoTermList[0].Code;
			AssertNoMessageErrorContaining(declaration.JE_ShipmentIncoTermInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_TransportModeInland()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			AssertNoMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportModeInland = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_TransportModeInlandInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAuthorizationOwnerEORI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "85D92E80");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ZString.Empty, "6CB38AF6");
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			declaration.JE_CustomsProfile = "DGI001";
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, "EORI code not found");
		}

		public void TestCheckAuthorizationOwnerNotNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ZGuid.Invalid;
			declaration.JE_OA_DeclarantAddress = ZGuid.Invalid;
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Invalid;
			declaration.JE_CustomsProfile = "DGI001";
			AssertEquals(null, declaration.Declarant);
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, "No authorization owner could be inferred from declaration actual data. Please fix this or the declaration will be rejected by Customs.");
		}

		public void TestCheckJE_LocationOfGoods()
		{
			OrgHeader organisationTested;
			JobDeclaration jobDeclarationWithList;
			CreationOfOrganisation(out organisationTested, out jobDeclarationWithList);
			jobDeclarationWithList.JE_LocationOfGoods = "BAD VALUE";
			AssertNoMessageErrorContaining(jobDeclarationWithList.JE_LocationOfGoodsInfo, "The code you have selected is not in the list");

			ConfigurationOfCusAuthorisationHeader(organisationTested, jobDeclarationWithList);
			jobDeclarationWithList.JE_LocationOfGoods = "BAD VALUE";
			AssertHasMessageErrorContaining(jobDeclarationWithList.JE_LocationOfGoodsInfo, "The code you have selected is not in the list");

			jobDeclarationWithList.JE_LocationOfGoods = "GOOD VALUE";
			AssertNoMessageErrorContaining(jobDeclarationWithList.JE_LocationOfGoodsInfo, "The code you have selected is not in the list");
		}

		public void TestCheckJE_LocationOfGoods_ValuesAreAvailable()
		{
			var message = MandatoryValidation.YouHaveNotEntered + " an Authorization when there is at least one available for this Profile.";

			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.Declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL000000000000000000000000001");
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", "DIE002", ZString.Empty, ZString.Empty, "B26F06FF");

			declaration.JE_CustomsProfile = "DIE002";
			AssertNoWarning("No warning expected when selected Customs profile has no linked AUL authorization.", declaration.JE_LocationOfGoodsInfo, message);

			declaration.JE_CustomsProfile = "DIE001";
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertHasWarning("Warning expected when selected Customs profile has one linked AUL authorization but location of goods is empty.", declaration.JE_LocationOfGoodsInfo, message);

			declaration.JE_LocationOfGoods = "AUL000000000000000000000000001";
			AssertNoWarning("No Warning expected when selected Customs profile has one linked AUL authorization and location of goods is not empty.", declaration.JE_LocationOfGoodsInfo, message);

			declaration.JE_LocationOfGoods = ZString.Empty;
			declaration.JE_CustomsProfile = "DIE002";
			declaration.Declarant.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL000000000000000000000000003");
			declaration.JE_CustomsProfile = "DIE001";
			AssertHasWarning("A warning is expected when selected Customs profile has several linked AUL authorizations and JE_LocationOfGoods is empty.", declaration.JE_LocationOfGoodsInfo, message);

			declaration.JE_LocationOfGoods = "AUL000000000000000000000000003";
			AssertNoWarning("No warning expected when JE_LocationOfGoods is set.", declaration.JE_LocationOfGoodsInfo, message);
		}

		void ConfigurationOfCusAuthorisationHeader(OrgHeader organisationTested, JobDeclaration jobDeclarationWithList)
		{
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation;
			cusAuthorisationHeader.CPH_Number = "GOOD VALUE";
			cusAuthorisationHeader.CPH_OH_PermitHolder = organisationTested.PK;
			cusAuthorisationHeader.CPH_OA_AppliesTo = organisationTested.Addresses.MainAddress.PK;

			var locCusAuthorisationRule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			locCusAuthorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			locCusAuthorisationRule.CPR_ValueFrom = "G1";

			var subCusAuthorisationRule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
			subCusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.SUB;
			subCusAuthorisationRule.CPR_ValueFrom = "G2";
			Factory.Save();

			organisationTested.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", "DIE002", ZString.Empty, ZString.Empty, "B26F06FF");
			jobDeclarationWithList.JE_OH_Importer = organisationTested.PK;
			jobDeclarationWithList.JE_MessageType = "IMP";
			AssertEquals("Pre req, there should be an item in the list", 1, jobDeclarationWithList.Lookups.GoodsLocations.Count);
		}

		void CreationOfOrganisation(out OrgHeader organisationTested, out JobDeclaration jobDeclarationWithList)
		{
			//Data set up : create an organisation with an empty JE_LocationOfGoods
			organisationTested = Factory.NewWithValidTestData<OrgHeader>();
			organisationTested.OH_IsWarehouseClient = true;
			jobDeclarationWithList = Factory.NewWithValidTestData<JobDeclaration>();
		}

		public void TestJE_SubLocationOfGoods()
		{
			OrgHeader organisationTested;
			JobDeclaration jobDeclarationWithList;
			CreationOfOrganisation(out organisationTested, out jobDeclarationWithList);
			jobDeclarationWithList.JE_SubLocationOfGoods = "BAD VALUE";
			AssertNoMessageErrorContaining(jobDeclarationWithList.JE_SubLocationOfGoodsInfo, "The code you have selected is not in the list");

			ConfigurationOfCusAuthorisationHeader(organisationTested, jobDeclarationWithList);

			jobDeclarationWithList.JE_SubLocationOfGoods = "BAD VALUE";
			AssertHasMessageErrorContaining(jobDeclarationWithList.JE_SubLocationOfGoodsInfo, "The code you have selected is not in the list");

			jobDeclarationWithList.JE_SubLocationOfGoods = "G2";
			AssertNoMessageErrorContaining(jobDeclarationWithList.JE_SubLocationOfGoodsInfo, "The code you have selected is not in the list");
		}

		public void TestCheckJE_CustomsProfile_Mandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_CustomsProfileInfo);
		}

		public void TestCheckJE_CustomsProfile_List()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "AUTHDEC", ZString.Empty, ZString.Empty, "6AE244EF");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_CustomsProfileInfo, "ZZZZ", "AUTHDEC");
		}

		public void TestCheckJE_CustomsProfile_Mismatch()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "85D92E80");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ZString.Empty, "6CB38AF6");
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = ZString.Empty;
				declaration.JE_DeltaMode = ZString.Empty;
				AssertNoMessageErrorContaining("Matched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match");

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertHasMessageErrorContaining("Matched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match");
				AssertHasMessageError("Types mismatched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match. To resolve this, please select a profile that supports Delta mode G2");

				declaration.JE_CustomsProfile = "DGI001";
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertHasMessageError("Types mismatched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match. To resolve this, please select Delta mode G1 or select a profile that supports Delta mode G2");

				declaration.JE_CustomsProfile = "DGI001";
				declaration.JE_DeltaMode = "X";
				AssertHasMessageError("Types mismatched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match. To resolve this, please select Delta mode G1");

				declaration.JE_CustomsProfile = "DGI001";
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertNoMessageErrorContaining("Matched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match");

				declaration.JE_CustomsProfile = "X";
				declaration.JE_DeltaMode = "G1";
				AssertHasMessageError("Types mismatched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match. To resolve this, please select a profile that supports Delta mode G1");

				declaration.JE_CustomsProfile = ZString.Empty;
				declaration.JE_DeltaMode = "G1";
				AssertHasMessageError("Types mismatched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match. To resolve this, please select a profile that supports Delta mode G1");

				declaration.JE_CustomsProfile = "X";
				declaration.JE_DeltaMode = "X";
				AssertHasMessageError("Types mismatched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match. To resolve this, please select a valid profile and a Delta mode matching the profile.");

				declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI001", ZString.Empty, ZString.Empty, "85D92E80");
				declaration.JE_CustomsProfile = "DGI001";
				declaration.JE_DeltaMode = "X";
				AssertHasMessageError("Types mismatched", declaration.JE_CustomsProfileInfo, "Delta mode and profile don't match. To resolve this, please select a Delta mode matching the profile");
			});
		}

		public void TestJE_ExportExitTypePreviousDocumentValidation()
		{
			CombineAssertions("Message Error Expected", () =>
			{
				SetupAndAssertExportExitTypePreviousDocumentValidation("EMC", "AAD");
				SetupAndAssertExportExitTypePreviousDocumentValidation("TRA", "820");
				SetupAndAssertExportExitTypePreviousDocumentValidation("TRA", "821");
				SetupAndAssertExportExitTypePreviousDocumentValidation("TRA", "822");
				SetupAndAssertExportExitTypePreviousDocumentValidation("TRA", "823");
				SetupAndAssertExportExitTypePreviousDocumentValidation("TRA", "952");
				SetupAndAssertExportExitTypePreviousDocumentValidation("TRA", "T2F");
				SetupAndAssertExportExitTypePreviousDocumentValidation("TRA", "T2M");
			});
		}

		void SetupAndAssertExportExitTypePreviousDocumentValidation(ZString exportExitType, ZString producedDocumentType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ExportExitType = "XXX";
			var pd1 = declaration.PreviousDocuments.AddNew();
			pd1.CSI_Code = "999";
			pd1.CSI_ReferenceNumber = "Reference 1";
			declaration.Validation.ValidateJE_ExportExitType();
			declaration.JE_ExportExitType = exportExitType;
			declaration.Validation.ValidateJE_ExportExitType();
			AssertHasMessageErrorContaining(declaration.JE_ExportExitTypeInfo, "Export Exit Type " + exportExitType + " requires a previous document but none was found");
			var pd = declaration.PreviousDocuments.AddNew();
			pd.CSI_Code = producedDocumentType;
			pd.CSI_ReferenceNumber = "Reference 2";
			declaration.Validation.ValidateJE_ExportExitType();
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, "Export Exit Type " + exportExitType + " requires a previous document but none was found");
		}

		public void TestJE_ExportExitTypeCTStatusValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.EMC;
			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.X;
			declaration.JE_CustomsProfile = "NOT EMPTY";
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, JobDeclarationValidation.ExportExitTypeCTStatusError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.TRA;
			declaration.Validation.ValidateJE_ExportExitType();
			AssertHasMessageErrorContaining(declaration.JE_ExportExitTypeInfo, JobDeclarationValidation.ExportExitTypeCTStatusError);
			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.T1;
			declaration.Validation.ValidateJE_ExportExitType();
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, JobDeclarationValidation.ExportExitTypeCTStatusError);
			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.T2;
			declaration.Validation.ValidateJE_ExportExitType();
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, JobDeclarationValidation.ExportExitTypeCTStatusError);
			declaration.ZG_CTStatusID = "T-";
			declaration.Validation.ValidateJE_ExportExitType();
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, JobDeclarationValidation.ExportExitTypeCTStatusError);
		}

		public void TestJE_ExportExitTypeListValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ExportExitType = "XXX";
			AssertHasMessageErrorContaining(declaration.JE_ExportExitTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.ECS;
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.TRA;
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.STC;
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.EMC;
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.OTH;
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestJE_ExportExitTypeOfficeValidation()
		{
			SetupCustomsOfficesOfExportAndExit();

			CombineAssertions(() =>
			{
				SetupAndAssertExportExitTypeOfficeValidation("IMP", "", "", "", "", false, "import - no error");
				SetupAndAssertExportExitTypeOfficeValidation("IMP", "", "", "FRCQF100", "", false, "import - no error");
				SetupAndAssertExportExitTypeOfficeValidation("IMP", "", "", "", "FRCQF100", false, "import - no error");
				SetupAndAssertExportExitTypeOfficeValidation("IMP", "", "", "FRCQF100", "FRCQF100", false, "import - no error");
				SetupAndAssertExportExitTypeOfficeValidation("IMP", "", "", "FRCQF100", "NLRTM200", false, "import - no error");
				SetupAndAssertExportExitTypeOfficeValidation("IMP", ExportExitTypeList.Codes.STC, "", "FRCQF100", "FRCQF100", false, "import - no error");

				SetupAndAssertExportExitTypeOfficeValidation("EXP", "", "", "", "", false, "export, no offices - no error");
				SetupAndAssertExportExitTypeOfficeValidation("EXP", "", "", "FRCQF100", "", false, "export, export office - no error");
				SetupAndAssertExportExitTypeOfficeValidation("EXP", "", "", "", "FRCQF100", false, "export, exit office - no error");
				SetupAndAssertExportExitTypeOfficeValidation("EXP", "", "", "FRCQF100", "NLRTM200", false, "export, different offices - no error");
				SetupAndAssertExportExitTypeOfficeValidation("EXP", "", "", "FRCQF100", "FRCQF100", false, "export, same offices but office of lodgement - no error");
				SetupAndAssertExportExitTypeOfficeValidation("EXP", "", "FRCQF100", "FRCQF100", "FRCQF100", true, "export, same offices - error");
				SetupAndAssertExportExitTypeOfficeValidation("EXP", ExportExitTypeList.Codes.STC, "", "FRCQF100", "FRCQF100", false, "export, same offices, exit type set - no error");
			});
		}

		void SetupAndAssertExportExitTypeOfficeValidation(ZString decType, ZString exportExitType, ZString officeOfLodgement, ZString officeOfExitCode, ZString officeOfExportCode, bool assertTrue, ZString assertionMessage)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = decType;
			declaration.JE_CustomsOffice = officeOfLodgement;
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.OTH;

			if (!officeOfExportCode.IsEmpty)
			{
				declaration.OfficeOfDeclaration = officeOfExportCode;
			}

			if (!officeOfExitCode.IsEmpty)
			{
				var officeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit)
					?? declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
				officeOfExit.CY_Data = officeOfExitCode;
			}

			declaration.JE_ExportExitType = exportExitType;
			declaration.Validation.ValidateAll();

			if (assertTrue)
			{
				AssertHasMessageErrorContaining(assertionMessage, declaration.JE_ExportExitTypeInfo, JobDeclarationValidation.ExportExitTypeRequiredError);
			}
			else
			{
				AssertNoMessageErrorContaining(assertionMessage, declaration.JE_ExportExitTypeInfo, JobDeclarationValidation.ExportExitTypeRequiredError);
			}
		}

		void SetupCustomsOfficesOfExportAndExit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var officeGB000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Descriptions.OfficeOfExport);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Descriptions.OfficeOfExit);

			var officeFRCQF100 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FRCQF100", "CALAIS PORT OFFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeFRCQF100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Descriptions.OfficeOfExport);

			var officeNLRTM200 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NLRTM200", "ROTTERDAM PORT OFFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeNLRTM200.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Descriptions.OfficeOfExit);
			Factory.Save();
		}

		public void TestJE_ExportExitTypeReasonValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.JE_ExportExitTypeReasonInfo.ReadOnly);
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
			declaration.JE_ExportExitType = "XXX";
			AssertEquals(true, declaration.JE_ExportExitTypeReasonInfo.ReadOnly);
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.ECS;
			AssertEquals(true, declaration.JE_ExportExitTypeReasonInfo.ReadOnly);
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.TRA;
			AssertEquals(true, declaration.JE_ExportExitTypeReasonInfo.ReadOnly);
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.STC;
			AssertEquals(true, declaration.JE_ExportExitTypeReasonInfo.ReadOnly);
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.EMC;
			AssertEquals(true, declaration.JE_ExportExitTypeReasonInfo.ReadOnly);
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.OTH;
			AssertEquals(false, declaration.JE_ExportExitTypeReasonInfo.ReadOnly);
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
			declaration.JE_ExportExitTypeReason = "smuggling dope";
			AssertNoMessageErrorContaining(declaration.JE_ExportExitTypeReasonInfo, JobDeclarationValidation.ExportExitTypeReasonRequiredError);
		}

		public void TestJE_RL_NKOriginValidation()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.JE_RL_NKOriginInfo, "This port code is invalid. Please check against the transport mode and shipment type.");

			declaration.JE_RL_NKOrigin = "XXXXX";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_RL_NKOriginInfo, "This port code is invalid. Please check against the transport mode and shipment type.");
		}

		public void TestJE_RL_NKFinalDestinationValidation()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, "This port code is invalid. Please check against the transport mode and shipment type.");

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "XXXXX";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.JE_RL_NKFinalDestinationInfo, "This port code is invalid. Please check against the transport mode and shipment type.");
		}

		#region JE_PaymentMethod
		public void TestJE_PaymentMethod_Mandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_PaymentMethodInfo);
		}

		public void TestJE_PaymentMethod_List()
		{
			var declaration = Factory.New<JobDeclaration>();
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_PaymentMethodInfo, "X", MethodOfPaymentList.Codes.A);
		}

		public void TestJE_PaymentMethod_GuaranteeBalanced()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "Ye";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = "IMP";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";

			var entry = SetupForTotalD48Amount();

			var importerOrSupplier = Factory.New<OrgHeader>();
			importerOrSupplier.OH_Code = "TEST001";
			importerOrSupplier.MainAddress.Address1 = "TestMatchAddress";
			importerOrSupplier.MainAddress.OA_Code = "TestMatchAddress";
			importerOrSupplier.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "0771DEC5");

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "TEST002";
			declarant.MainAddress.Address1 = "TestMatchAddress21";
			declarant.MainAddress.OA_Code = "TestMatchAddress21";
			var declarantAddressMatch2 = declarant.Addresses.AddNew();
			declarantAddressMatch2.Address1 = "TestMatchAddress22";
			declarantAddressMatch2.OA_Code = "TestMatchAddress22";
			var declarantAddressNoMatch = declarant.Addresses.AddNew();
			declarantAddressNoMatch.Address1 = "TestNoMatchAddress";
			declarantAddressNoMatch.OA_Code = "TestNoMatchAddress";

			var declaration = entry.Declaration;
			declaration.SetupImporter(importerOrSupplier);
			declaration.SetupDeclarant(declarant.MainAddress);

			var guarantee1 = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GHOO", importerOrSupplier.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.Germany);
			Factory.Save();

			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			AssertEquals("Guarantees from other countries can be loaded as well.", guarantee1, declaration.CustomsGuarantee);

			var oblTran1 = guarantee1.CusGuaranteeLineTransactions.AddNew();
			oblTran1.CPL_TranValue = 100;
			oblTran1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			oblTran1.CPL_Reference = "REF00001";

			var guaranteeAdjustment = guarantee1.CusGuaranteeLineTransactions.AddNew();
			guaranteeAdjustment.CPL_TranValue = 0;
			guaranteeAdjustment.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			guaranteeAdjustment.CPL_Reference = "REF00002";
			guaranteeAdjustment.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;

			guarantee1.CPH_Balance = 100;
			var rule1 = guarantee1.CusGuaranteeRules.AddNew();
			rule1.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule1.CPR_ValueFrom = "TestMatchAddress2";

			declaration.JE_PaymentMethod = "A";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			Factory.Save();

			AssertNotNull(declaration.CustomsGuarantee);

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.FillWithValidTestData();
			entryInstruction.CEI_Style = "XX";

			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			entry1.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entry1.AllEntryLines.AddNew();
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_ChargeAmount = 10;
			fee1.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			var fee2 = entryLine1.Fees.AddNew();
			fee2.CF_ChargeAmount = 20;
			fee2.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			var entryLine2 = entry1.AllEntryLines.AddNew();
			var fee3 = entryLine2.Fees.AddNew();
			fee3.CF_ChargeAmount = 30;
			fee3.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_EntryStatus = Common.EU.EntryStatusList.Codes.Cancelled;
			entry2.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine3 = entry2.AllEntryLines.AddNew();
			var fee4 = entryLine3.Fees.AddNew();
			fee4.CF_ChargeAmount = 40;
			fee4.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;

			CombineAssertions(() =>
			{
				guaranteeAdjustment.CPL_TranValue = -80;
				Factory.Save();

				declaration.Validation.ValidateJE_PaymentMethod();
				AssertHasMessageError("Unbalanced", declaration.JE_PaymentMethodInfo, "The remaining balance of this guarantee is 20,00 EUR but the open entries on this declaration require 40,00 EUR");

				guaranteeAdjustment.CPL_TranValue = -50;
				Factory.Save();

				declaration.Validation.ValidateJE_PaymentMethod();
				AssertNoMessageError("Balanced", declaration.JE_PaymentMethodInfo, "The remaining balance of this guarantee is 20,00 EUR but the open entries on this declaration require 40,00 EUR");
				AssertNoMessageErrors(declaration.JE_PaymentMethodInfo);
			});
		}

		CusEntryHeader SetupForTotalD48Amount()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0003", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "GHI", "Anser fabalis/Bean goose", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA, "DEF", "DDezful", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.FillWithValidTestData();
			entryInstruction.CEI_Style = "Ye12367";

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc3 = invoiceLine1.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "0001";
			suppDoc3.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc3.CSI_Status = "AN";
			suppDoc3.CSI_Quantity3 = 30;
			suppDoc3.CSI_Value = 20;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc4 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "DEF";
			suppDoc4.CSI_Status = "AN";
			suppDoc4.CSI_Quantity3 = 0;
			suppDoc4.CSI_Value = 10;
			var suppDoc5 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc5.CSI_Code = "GHI";
			suppDoc5.CSI_Status = "AN";
			suppDoc5.CSI_Quantity3 = 0;
			suppDoc5.CSI_Value = 30;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc6 = invoiceLine3.SupportingDocuments.AddNew();
			suppDoc6.CSI_Code = "0003";
			suppDoc6.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc6.CSI_Quantity3 = 5;
			suppDoc6.CSI_Value = 10;

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var suppDoc7 = invoiceLine4.SupportingDocuments.AddNew();
			suppDoc7.CSI_Code = "0003";
			suppDoc7.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc7.CSI_Quantity3 = 5;
			suppDoc7.CSI_Value = 10;

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(CusEntryHeader));

			entry.CH_CEI_Instruction = entryInstruction.PK;

			// After merge CSI_Code = "0001" so IsD48 = true and CSI_Value = 10
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "DEF" | "GHI" so IsD48 = false and CSI_Value = 40 so value will not be counted
			var entryLine2 = entry.AllEntryLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10
			var entryLine3 = entry.AllEntryLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = "Ye12367";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10, however CSI_Status = 'Y' so therefore IsCompletee = false so value will not be counted
			var entryLine4 = entry.AllEntryLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Procedure = "Ye12367";
			return entry;
		}
		#endregion

		public void TestJE_DefermentAccountNumber_Mandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_DefermentAccountNumberInfo);
		}

		public void TestJE_DefermentAccountNumber_Length()
		{
			const string errorMessage = "An account number must be 4 letters long";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			CombineAssertions(() =>
			{
				declaration.JE_DefermentAccountNumber = "XXX";
				AssertHasWarning("Too Short", declaration.JE_DefermentAccountNumberInfo, errorMessage);
				declaration.JE_DefermentAccountNumber = "XXXXX";
				AssertHasWarning("Too Long", declaration.JE_DefermentAccountNumberInfo, errorMessage);
				declaration.JE_DefermentAccountNumber = "AB#1";
				AssertHasWarning("More then just Alpha's", declaration.JE_DefermentAccountNumberInfo, errorMessage);
				declaration.JE_DefermentAccountNumber = "ABCD";
				AssertNoWarning("Valid", declaration.JE_DefermentAccountNumberInfo, errorMessage);
			});
		}

		public void TestJE_DeltaMode_List()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "FR33159700", ZString.Empty, ZString.Empty, "8C61AD4E");
			declaration.JE_OH_Importer = testImporter.PK;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_DeltaModeInfo, OrgCusAccountDeltaGTypeList.Codes.G2, OrgCusAccountDeltaGTypeList.Codes.G1);
		}

		public void TestCheckJE_LandedPieces()
		{
			var messageErrorNotAllPiecesAreReceived = "Not all pieces are received yet.";
			var declaration = Factory.New<JobDeclaration>();
			var targetInfo = declaration.JE_LandedPiecesInfo;
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_LandedPieces = 0;
			AssertNoMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
			declaration.JE_LandedPieces = 10;
			AssertNoMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
			declaration.JE_LandedPieces = 5;
			AssertHasMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_LandedPieces = 5;
			AssertHasMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertNoMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_LandedPieces = 5;
			AssertHasMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
			declaration.JE_TotalNoOfPacks = 4;
			AssertNoMessageError(targetInfo, messageErrorNotAllPiecesAreReceived);
		}

		public void TestCheckJE_OA_DeclarantAddress()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "ABC";
			var declarantAddress = declarant.Addresses.AddNewMainAddress();

			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "REP";
			var representativeAddress = representative.Addresses.AddNewMainAddress();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.DeclarantIsRequired);

			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._3Indirect;
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.DeclarantIsRequired);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertHasMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.DeclarantIsRequired);

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			AssertNoMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.DeclarantIsRequired);
			AssertHasMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.CBRNotConfigured);

			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "A", Core.Constants.CountryCodes.France);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.CBRNotConfigured);

			declaration.JE_OA_Representative = representativeAddress.PK;
			declarant.CustomsCodes.RemoveAndDeleteAll();
			representative.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "A", Core.Constants.CountryCodes.France);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.CBRNotConfigured);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;

				var messageError = "Phone and email are mandatory in customs message, please enter both the data for Broker staff or have it configured in current Branch information.";
				CombineAssertions("A MessageError is expected when Phone and Email are not available for both CusAgent and Branch.", () =>
				{
					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Export, "", brokerEmailExists: false, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					void setUpDeclarationForTest(string messageType, string brokerPhone, bool brokerEmailExists, string branchPhone, string branchEmail)
					{
						declaration.JE_MessageType = messageType;
						declaration.CusAgent.GS_WorkPhone = brokerPhone;

						if (brokerEmailExists)
						{
							var email = declaration.CusAgent.EmailAddresses.AddNew();
							email.GSE_EmailAddress = "emailAddress";
						}
						else
						{
							declaration.CusAgent.EmailAddresses.DeleteAll();
						}

						declaration.Branch.GB_Phone_Formatted = branchPhone;
						declaration.Branch.GB_Email = branchEmail;
					}
				});
			}
		}

		public void TestValidateWarehouseDocAddressForeignKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Validation.ValidateWarehouseDocAddressForeignKey();
			AssertNoMessageErrors(declaration.WarehouseDocAddress.E2_OA_AddressInfo);
		}

		public void TestCheckJE_DateOfFirstArrival()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DateOfFirstArrival = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_DateOfFirstArrival = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.JE_DateOfFirstArrivalInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_GS_NKCusAgent()
		{
			var messageError = "You have not entered a Broker.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertHasMessageErrorContaining("A MessageError is expected when JE_GS_NKCusAgent is empty.", declaration.JE_GS_NKCusAgentInfo, messageError);

				declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertNoMessageErrorContaining("No MessageError is expected when JE_GS_NKCusAgent has a value.", declaration.JE_GS_NKCusAgentInfo, messageError);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				declaration.JE_GS_NKCusAgent = ZString.Empty;
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertNoMessageErrorContaining("A MessageError is expected when JE_GS_NKCusAgent is empty.", declaration.JE_GS_NKCusAgentInfo, messageError);

				messageError = "Phone and email are mandatory in customs message, please enter both the data for Broker staff or have it configured in current Branch information.";
				declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;

				CombineAssertions("A MessageError is expected when Phone and Email are not available for both CusAgent and Branch.", () =>
				{
					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "branchPhone", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: true, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "brokerPhone", brokerEmailExists: false, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: false, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Import, "", brokerEmailExists: true, "branchPhone", "branchEmail");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					setUpDeclarationForTest(EUJobMessageTypeList.Codes.Export, "", brokerEmailExists: false, "", "");
					declaration.Validation.ValidateJE_GS_NKCusAgent();
					AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, messageError);

					void setUpDeclarationForTest(string messageType, string brokerPhone, bool brokerEmailExists, string branchPhone, string branchEmail)
					{
						declaration.JE_MessageType = messageType;
						declaration.CusAgent.GS_WorkPhone = brokerPhone;

						if (brokerEmailExists)
						{
							var email = declaration.CusAgent.EmailAddresses.AddNew();
							email.GSE_EmailAddress = "emailaddress";
						}
						else
						{
							declaration.CusAgent.EmailAddresses.DeleteAll();
						}

						declaration.Branch.GB_Phone_Formatted = branchPhone;
						declaration.Branch.GB_Email = branchEmail;
					}
				});
			}
		}

		public void TestCheckJE_ExportDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_RN_NKTransportNationality()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RN_NKTransportNationality = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_RN_NKTransportNationality = "FR";
			AssertNoMessageErrorContaining(declaration.JE_RN_NKTransportNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_ApplicationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				Assert("Precondition : declaration is not saved", !declaration.IsInDatabase);
				ValidationTestHelper.AssertErrorIfNotEntered(declaration.JE_ApplicationCodeInfo);
				var validCodes = declaration.Lookups.ApplicationCodeList.GetAllCodesZString();

				foreach (var validCode in validCodes)
				{
					ValidationTestHelper.AssertErrorIfInvalidCode(declaration.JE_ApplicationCodeInfo, new ZString("XX"), validCode);
				}
			});

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Precondition : declaration is saved", declaration.IsInDatabase);

				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_ApplicationCodeInfo);
				var validCodes = declaration.Lookups.ApplicationCodeList.GetAllCodesZString();

				foreach (var validCode in validCodes)
				{
					ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_ApplicationCodeInfo, new ZString("XX"), validCode);
				}
			});

			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				declaration.CustomsEntryHeaders.AddNew();
				Factory.Save();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertNoErrorContaining(declaration.JE_ApplicationCodeInfo, "You may not change the message because messages have been sent.");

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var message = entryHeader.Messages.AddNew();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Factory.Save();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				AssertHasErrorContaining(declaration.JE_ApplicationCodeInfo, "You may not change the message because messages have been sent.");
			});
		}

		public void TestCheckJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateCusCodeList(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VALID", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var message = "Entered office code is not a valid office.";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				declaration.JE_CustomsOffice = "VALID";
				AssertNoMessageErrorContaining("No error", declaration.JE_CustomsOfficeInfo, message);
				declaration.JE_CustomsOffice = "INVALID";
				AssertHasMessageErrorContaining("Error", declaration.JE_CustomsOfficeInfo, message);
			});
		}

		public void TestJE_UCR_LengthWithODSDocuments()
		{
			SetupODSDocuments();

			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			CombineAssertions("DUCR should not exceed 22 chars when declaration has ODS supporting document at any level.", () =>
			{
				AssertJE_UCRMaxLengthMessageError(declaration, true, false, false, false, false);
				AssertJE_UCRMaxLengthMessageError(declaration, false, true, false, false, false);
				AssertJE_UCRMaxLengthMessageError(declaration, true, true, false, false, true);
				AssertJE_UCRMaxLengthMessageError(declaration, false, false, true, false, false);
				AssertJE_UCRMaxLengthMessageError(declaration, true, false, true, false, true);
				AssertJE_UCRMaxLengthMessageError(declaration, false, false, false, true, false);
				AssertJE_UCRMaxLengthMessageError(declaration, true, false, false, true, true);
			});
		}

		void AssertJE_UCRMaxLengthMessageError(JobDeclaration declaration, bool ducrExceedsMaxLength, bool declarationHasODSDocument, bool invoiceHasODSDocument, bool invoiceLineHasODSDocument, bool ducrShouldHaveMessageError)
		{
			declaration.SupportingDocuments.RemoveAndDeleteAll();
			var declarationSuppDocument1 = declaration.SupportingDocuments.AddNew();
			declarationSuppDocument1.CSI_Code = "0001";

			var invoice = declaration.Invoices.Cast<JobComInvoiceHeader>().First();
			invoice.SupportingDocuments.RemoveAndDeleteAll();
			var invoiceSuppDocument1 = invoice.SupportingDocuments.AddNew();
			invoiceSuppDocument1.CSI_Code = "0001";

			var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().First();
			invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
			var invoiceLineSuppDocument1 = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineSuppDocument1.CSI_Code = "0001";

			if (declarationHasODSDocument)
			{
				var declarationSuppDocument2 = declaration.SupportingDocuments.AddNew();
				declarationSuppDocument2.CSI_Code = "L100";
				var declarationSuppDocument3 = declaration.SupportingDocuments.AddNew();
				declarationSuppDocument3.CSI_Code = "E013";
			}

			if (invoiceHasODSDocument)
			{
				var invoiceSuppDocument2 = invoice.SupportingDocuments.AddNew();
				invoiceSuppDocument2.CSI_Code = "L100";
				var invoiceSuppDocument3 = invoice.SupportingDocuments.AddNew();
				invoiceSuppDocument3.CSI_Code = "E013";
			}

			if (invoiceLineHasODSDocument)
			{
				var invoiceLineSuppDocument2 = invoiceLine.SupportingDocuments.AddNew();
				invoiceLineSuppDocument2.CSI_Code = "L100";
				var invoiceLineSuppDocument3 = invoiceLine.SupportingDocuments.AddNew();
				invoiceLineSuppDocument3.CSI_Code = "E013";
			}

			declaration.JE_UCR = ducrExceedsMaxLength ? "12345678901234567890123456" : "123456789012345678901";

			declaration.Validation.ValidateJE_UCR();

			if (ducrShouldHaveMessageError)
			{
				AssertHasMessageErrorContaining(declaration.JE_UCRInfo, "limits the local reference to 22 chars max.");
				AssertHasMessageErrorContaining(declaration.JE_UCRInfo, "L100, E013");
			}
			else
			{
				AssertNoMessageErrorContaining(declaration.JE_UCRInfo, "limits the local reference to 22 chars max.");
				AssertNoMessageErrorContaining(declaration.JE_UCRInfo, "L100, E013");
			}
		}

		void SetupODSDocuments()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "L100", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "E013", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "L100", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "E013", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			Factory.Save();
		}

		public void TestCheckJE_AirRouteType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_AirRouteType = "1";
			AssertNoMessageError(declaration.JE_AirRouteTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_AirRouteType = "A";
			AssertHasMessageErrorContaining(declaration.JE_AirRouteTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_AirRouteType = "8";
			AssertNoMessageError(declaration.JE_AirRouteTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_AirRouteType = "11";
			AssertHasMessageErrorContaining(declaration.JE_AirRouteTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_RegionOrTerritoryOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			var info = declaration.JE_RegionOrTerritoryOfDestinationInfo;
			declaration.JE_RegionOrTerritoryOfDestination = ZString.Empty;
			AssertNoMessageErrors(info);
			declaration.JE_RegionOrTerritoryOfDestination = "XXXXX";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			declaration.JE_RegionOrTerritoryOfDestination = "MAYOT";
			AssertNoMessageErrors(info);
		}

		public void TestCheckZG_ExportExitType_WhenTRA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.T1;
			declaration.JE_ExportExitType = ExportExitTypeList.Codes.TRA;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			AssertHasMessageError(declaration.JE_ExportExitTypeInfo, $"Export Exit Type {declaration.JE_ExportExitType} requires a previous document but none was found.");

			var decPrevDoc = declaration.PreviousDocuments.AddNew();
			decPrevDoc.CSI_ReferenceNumber = "ZZZ";
			decPrevDoc.CSI_Code = declaration.AllowedPreviousDocsForTRAExportExitType[0];
			declaration.Validation.ValidateJE_ExportExitType();

			AssertNoMessageErrors(declaration.JE_ExportExitTypeInfo);

			declaration.PreviousDocuments.RemoveAndDeleteAll();

			declaration.Validation.ValidateJE_ExportExitType();
			AssertHasMessageError(declaration.JE_ExportExitTypeInfo, $"Export Exit Type {declaration.JE_ExportExitType} requires a previous document but none was found.");

			var invoicePrevDoc = invoice.PreviousDocuments.AddNew();
			invoicePrevDoc.CSI_ReferenceNumber = "ZZZ";
			invoicePrevDoc.CSI_Code = declaration.AllowedPreviousDocsForTRAExportExitType[0];

			declaration.Validation.ValidateJE_ExportExitType();
			AssertNoMessageErrors(declaration.JE_ExportExitTypeInfo);

			invoice.PreviousDocuments.RemoveAndDeleteAll();

			declaration.Validation.ValidateJE_ExportExitType();
			AssertHasMessageError(declaration.JE_ExportExitTypeInfo, $"Export Exit Type {declaration.JE_ExportExitType} requires a previous document but none was found.");

			var invLinePrevDoc = invoiceLine.PreviousDocuments.AddNew();
			invLinePrevDoc.CSI_ReferenceNumber = "ZZZ";
			invLinePrevDoc.CSI_Code = declaration.AllowedPreviousDocsForTRAExportExitType[0];

			declaration.Validation.ValidateJE_ExportExitType();
			AssertNoMessageErrors(declaration.JE_ExportExitTypeInfo);
		}

		public void TestCheckJE_PaymentMethod()
		{
			const string messageError = "[NAT_130_Bis] Cash is the only suitable Method of Payment when the declaration has E0001 Additional Information.";

			var declaration = Factory.New<JobDeclaration>();

			using (var context = new Business.Testing.ConfigurationProviders.DeclarationValidationDeciderTestContext(declaration, true))
			{
				var testCase = new MultiFactorTestCase<JobDeclaration>(() =>
				{
					declaration.AdditionalInfos.AddNew();
					declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

					return declaration;
				});

				var isStandardDeclaration = new FieldPreq<JobDeclaration>(declaration => declaration.AdditionalInfos.First().CSI_CodeInfo)
					.Values(UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration)
					.NotValues(UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FretCargo);

				var paymentMethodIsNotA = new FieldPreq<JobDeclaration>(declaration => declaration.JE_PaymentMethodInfo)
					.Values(MethodOfPaymentList.Codes.M)
					.NotValues(MethodOfPaymentList.Codes.A);

				testCase.SetUpCondition(isStandardDeclaration && paymentMethodIsNotA);

				context.EnableRule(x => x.IsRuleNAT_130BisActive);
				testCase.RunAssertion(declaration =>
				{
					declaration.Validation.ValidateJE_PaymentMethod();
					AssertHasMessageError("When the declaration has E0001 Additional Information, PaymentMethod must be value A.", declaration.JE_PaymentMethodInfo, messageError);
				}, declaration =>
				{
					declaration.Validation.ValidateJE_PaymentMethod();
					AssertNoMessageError(declaration.JE_PaymentMethodInfo, messageError);
				});

				context.DisableRule(x => x.IsRuleNAT_130BisActive);
				testCase.RunAssertion(declaration =>
				{
					declaration.Validation.ValidateJE_PaymentMethod();
					AssertNoMessageError("When RuleNAT_130Bis is not activated, there should be no validation.", declaration.JE_PaymentMethodInfo, messageError);
				}, declaration =>
				{
					declaration.Validation.ValidateJE_PaymentMethod();
					AssertNoMessageError(declaration.JE_PaymentMethodInfo, messageError);
				});
			}
		}

		public void TestCheckJE_DefermentAccountNumber()
		{
			const string originMessageErrorOfDeltaIEImport = "The value should be i) starting with two numbers, ii) followed by 'D', iii) followed by 'N' or 'A', iv) followed by four alphabets, v) ends with exactly nine digits.";
			const string messageErrorOfRuleNAT_130Bis = "[NAT_130_Bis] The deferred payment number should be empty when the declaration has E0001 Additional Information.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var context = new Business.Testing.ConfigurationProviders.DeclarationValidationDeciderTestContext(declaration, true))
			{
				var additionalInfo = declaration.AdditionalInfos.AddNew();

				context.EnableRule(x => x.IsRuleNAT_130BisActive);

				CombineAssertions("When RuleNAT_130Bis is enabled:", () =>
				{
					additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					declaration.JE_DefermentAccountNumber = "11DACCCC123456789";
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertHasMessageError("When the declaration has E0001 Additional Information, DefermentAccountNumber should be empty.", declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);
					declaration.JE_DefermentAccountNumber = ZString.Empty;
					AssertNoMessageError("When the declaration has E0001 Additional Information, the old validation rule of DefermentAccountNumber should be disabled.", declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);

					additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FretCargo;
					declaration.JE_DefermentAccountNumber = "11DACCCC123456789";
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertNoMessageError("When the declaration does not have E0001 Additional Information, there should be no RuleNAT_130Bis message error.", declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);
					declaration.JE_DefermentAccountNumber = ZString.Empty;
					AssertHasMessageError("When the declaration does not have E0001 Additional Information, the old validation rule of DefermentAccountNumber should be enabled.", declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);
				});

				context.DisableRule(x => x.IsRuleNAT_130BisActive);

				CombineAssertions("When RuleNAT_130Bis is disabled:", () =>
				{
					additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.StandardDeclaration;
					declaration.JE_DefermentAccountNumber = "11DACCCC123456789";
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertNoMessageError("Regardless of whether the declaration has E0001 Additional Information or not, there should be no RuleNAT_130Bis message error.", declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);
					declaration.JE_DefermentAccountNumber = ZString.Empty;
					AssertHasMessageError("Regardless of whether the declaration has E0001 Additional Information or not, the old validation rule of DefermentAccountNumber should be enabled.", declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);

					additionalInfo.CSI_Code = UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FretCargo;
					declaration.JE_DefermentAccountNumber = "11DACCCC123456789";
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertNoMessageError("Regardless of whether the declaration has E0001 Additional Information or not, there should be no RuleNAT_130Bis message error.", declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);
					declaration.JE_DefermentAccountNumber = ZString.Empty;
					AssertHasMessageError("Regardless of whether the declaration has E0001 Additional Information or not, the old validation rule of DefermentAccountNumber should be enabled.", declaration.JE_DefermentAccountNumberInfo, originMessageErrorOfDeltaIEImport);
					AssertNoMessageError(declaration.JE_DefermentAccountNumberInfo, messageErrorOfRuleNAT_130Bis);
				});
			}
		}

		public void TestCheckJE_DefermentAccountNumberWhenPaymentMethodIsA()
		{
			const string messageErrorWhenPaymentMethodIsA = "The deferred payment number should be empty when the Method of Payment is Cash.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.A;

			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoMessageError("When PaymentMethod is A, DefermentAccountNumber should be empty.", declaration.JE_DefermentAccountNumberInfo, messageErrorWhenPaymentMethodIsA);
			declaration.JE_DefermentAccountNumber = "11DACCCC123456789";
			AssertHasMessageError("When PaymentMethod is A, DefermentAccountNumber should be empty.", declaration.JE_DefermentAccountNumberInfo, messageErrorWhenPaymentMethodIsA);
		}
	}
}
