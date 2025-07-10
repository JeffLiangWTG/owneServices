using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaIEJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckJE_GoodsOrigin()
		{
			var message = "[R0012 & C0002] If Country of Dispatch is specified in one of Invoice lines then it must be specified for all invoice lines or in Declaration tab.";
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.ZG_CountryOfDispatch = "GB";

			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertHasMessageError(declaration.JE_GoodsOriginInfo, message);

			invoiceLine1.ZG_CountryOfDispatch = "GB";
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError(declaration.JE_GoodsOriginInfo, message);

			invoiceLine1.ZG_CountryOfDispatch = "";
			invoiceLine2.ZG_CountryOfDispatch = "";
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoMessageError(declaration.JE_GoodsOriginInfo, message);

			message = "Invoice lines without value in Country of Dispatch will be mapped from the Declaration tab.";
			invoiceLine2.ZG_CountryOfDispatch = "GB";
			declaration.JE_GoodsOrigin = "FR";
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertHasWarning(declaration.JE_GoodsOriginInfo, message);

			invoiceLine1.ZG_CountryOfDispatch = "GB";
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoWarning(declaration.JE_GoodsOriginInfo, message);

			invoiceLine1.ZG_CountryOfDispatch = "";
			invoiceLine2.ZG_CountryOfDispatch = "";
			declaration.Validation.ValidateJE_GoodsOrigin();
			AssertNoWarning(declaration.JE_GoodsOriginInfo, message);
		}

		public void TestCheckJE_GoodsDestination()
		{
			var message = "[R0012 & C0002] If Country of Destination is specified in one of Invoice lines then it must be specified for all invoice lines or in Declaration tab.";
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.ZG_CountryOfDestination = "GB";

			declaration.Validation.ValidateJE_GoodsDestination();
			AssertHasMessageError("Error when there is a combination of invoice lines with empty and non-empty ZG_CountryOfDestination fields.", declaration.JE_GoodsDestinationInfo, message);

			invoiceLine1.ZG_CountryOfDestination = "GB";
			declaration.Validation.ValidateJE_GoodsDestination();
			AssertNoMessageError("No error when all invoice lines have non-empty ZG_CountryOfDestination fields.", declaration.JE_GoodsDestinationInfo, message);

			invoiceLine1.ZG_CountryOfDestination = "";
			invoiceLine2.ZG_CountryOfDestination = "";
			declaration.Validation.ValidateJE_GoodsDestination();
			AssertNoMessageError("No error when all invoice lines have empty ZG_CountryOfDestination fields.", declaration.JE_GoodsDestinationInfo, message);

			invoiceLine2.ZG_CountryOfDestination = "GB";
			declaration.JE_GoodsDestination = "FR";
			declaration.Validation.ValidateJE_GoodsDestination();
			AssertNoMessageError("No error when declaration JE_GoodsDestination is populated regardless of invoice line ZG_CountryOfDestination field values.", declaration.JE_GoodsDestinationInfo, message);
		}

		public void TestJE_DeltaMode()
		{
			declaration.JE_DeltaMode = ZString.Empty;
			AssertNoMessageErrors("When JE_ApplicationCode is DI, JE_DeltaMode is allowed to be empty", declaration.JE_DeltaModeInfo);
			declaration.JE_DeltaMode = "ABC";
			AssertHasMessageErrorContaining(declaration.JE_DeltaModeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateSupplierDocumentaryAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.SetupSupplier(orgHeader);
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(declaration.SupplierDocumentaryAddress, orgHeader);
		}

		public void TestValidateImporterDocumentaryAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.SetupImporter(orgHeader);
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(declaration.ImporterDocumentaryAddress, orgHeader);
		}

		public void TestCheckJE_OA_DeclarantAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var address = declaration.DeclarantAddress;
			var info = declaration.JE_OA_DeclarantAddressInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, declaration.Validation.ValidateJE_OA_DeclarantAddress);
		}

		public void TestCheckJE_DefermentAccountNumber()
		{
			var error = "The value should be i) starting with two numbers, ii) followed by 'D', iii) followed by 'N' or 'A', iv) followed by four alphabets, v) ends with exactly nine digits.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			ValidateAccountNumber(ZString.Empty);

			CombineAssertions("Account Number should match the required characteristics.", () =>
			{
				ValidateAccountNumber("11DACCCC123456789", false);
				ValidateAccountNumber("11DNCCCC123456789", false);
				ValidateAccountNumber("1DACCCC123456789");
				ValidateAccountNumber("1ADACCCC123456789");
				ValidateAccountNumber("A1DACCCC123456789");
				ValidateAccountNumber("AADACCCC123456789");
				ValidateAccountNumber("11EACCCC123456789");
				ValidateAccountNumber("11DBCCCC123456789");
				ValidateAccountNumber("11DA1CCC123456789");
				ValidateAccountNumber("11DACCC123456789");
				ValidateAccountNumber("11DACCCCC123456789");
				ValidateAccountNumber("11DACCCC12345S789");
				ValidateAccountNumber("11DACCCC1234567890");
				ValidateAccountNumber("11DACCCC12345678");
			});

			void ValidateAccountNumber(ZString accountNumber, bool hasError = true)
			{
				declaration.JE_DefermentAccountNumber = accountNumber;
				if (hasError)
				{
					AssertHasMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, error);
				}
				else
				{
					AssertNoMessageErrorContaining(declaration.JE_DefermentAccountNumberInfo, error);
				}
			}

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoMessageErrorContaining("Does not validate if declaration is of export type.", declaration.JE_DefermentAccountNumberInfo, error);
		}

		public void TestCheckJE_OA_Representative()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_Representative = orgHeader.MainAddress.PK;
			var address = declaration.Representative;
			var info = declaration.JE_OA_RepresentativeInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, declaration.Validation.ValidateJE_OA_Representative);
		}

		public void TestCheckJE_OH_Buyer()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_Buyer = orgHeader.PK;
			var address = declaration.Buyer.MainAddress;
			var info = declaration.JE_OH_BuyerInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, declaration.Validation.ValidateJE_OH_Buyer);
		}

		public void TestCheckJE_OA_SellerAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OA_SellerAddress = orgHeader.MainAddress.PK;
			var address = declaration.SellerAddress;
			var info = declaration.JE_OA_SellerAddressInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, declaration.Validation.ValidateJE_OA_SellerAddress);
		}

		public void TestCheckDeclarantVsRepresentative()
		{
			var declarantOrgHeader = CreateOrganisationWithAddressAndEori("DECLARANT", "DeclarantName", "DeclarantAddress", "11111111111111");
			var representativeOrgHeader = CreateOrganisationWithAddressAndEori("REPRESENT", "RepresentativeName", "RepresentativeAddress", "22222222222222");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			declaration.RunPreSaveValidation();
			AssertNoMessageError("There shoud not be any message error when one of declarant or representative is empty.", declaration.JE_OA_DeclarantAddressInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");
			AssertNoMessageError("There shoud not be any message error when one of declarant or representative is empty.", declaration.JE_OA_RepresentativeInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");

			declaration.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;
			AssertNoMessageError("There shoud not be any message error when one of declarant or representative is empty.", declaration.JE_OA_DeclarantAddressInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");
			AssertNoMessageError("There shoud not be any message error when one of declarant or representative is empty.", declaration.JE_OA_RepresentativeInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");

			declaration.JE_OA_Representative = representativeOrgHeader.MainAddress.PK;
			AssertNoMessageError("There shoud not be any message error when declarant address EORI and representative address EORI differ.", declaration.JE_OA_DeclarantAddressInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");
			AssertNoMessageError("There shoud not be any message error when declarant address EORI and representative address EORI differ.", declaration.JE_OA_RepresentativeInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");

			declaration.JE_OA_DeclarantAddress = representativeOrgHeader.MainAddress.PK;
			AssertHasMessageError("There shoud be a message error when declarant address EORI and representative address EORI are the same and not empty.", declaration.JE_OA_DeclarantAddressInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");

			declaration.JE_OA_DeclarantAddress = declarantOrgHeader.MainAddress.PK;
			declaration.JE_OA_Representative = declarantOrgHeader.MainAddress.PK;
			AssertHasMessageError("There shoud be a message error when declarant address EORI and representative address EORI are the same and not empty.", declaration.JE_OA_RepresentativeInfo, "Declarant and Representative can’t be the same, please check their EORI numbers");
		}

		public void TestCheckJE_OA_DeclarantAddress_DeclarantNeed_A_NotEmpty_CZAccount()
		{
			var message = "Declarant/Representative ID is not configured. Config it in Organization > Config > France tab.";
			var orgHeaderWithAccount = Factory.New<OrgHeader>();
			var declarantWithoutAccount = Factory.New<OrgHeader>();
			var declarantWithoutSameReference = Factory.New<OrgHeader>();
			var declarantWithoutAccountValue = Factory.New<OrgHeader>();

			var orgCusAccount = orgHeaderWithAccount.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount.CZ_Account = "12345678";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = orgHeaderWithAccount.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount.CZ_RepresentativeID = "123456";

			var orgCusAccount2 = declarantWithoutAccount.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount2.CZ_Account = "12345678";
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_OH = declarantWithoutAccount.PK;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount2.CZ_RepresentativeID = ZString.Empty;

			var orgCusAccount3 = declarantWithoutSameReference.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount3.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount3.CZ_Account = "87654321";
			orgCusAccount3.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount3.CZ_OH = declarantWithoutSameReference.PK;
			orgCusAccount3.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount3.CZ_RepresentativeID = "123456";

			var orgCusAccount4 = declarantWithoutAccountValue.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount4.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount4.CZ_Account = "";
			orgCusAccount4.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount4.CZ_OH = declarantWithoutAccountValue.PK;
			orgCusAccount4.CZ_Type = OrgCusAccountDeltaIETypeList.Codes.DCN;
			orgCusAccount4.CZ_RepresentativeID = "";

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_CustomsProfile = "12345678";
			declaration.JE_DeltaMode = "";
			declaration.JE_OA_DeclarantAddress = orgHeaderWithAccount.MainAddress.PK;
			AssertNoMessageErrorContaining("JE_DeltaMode is empty && RepresentativeID of declarant is not empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_CustomsProfile = "12345678";
			AssertHasMessageErrorContaining("There is no declarant Address && JE_CustomsProfile is not empty  => error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.SetupImporter(orgHeaderWithAccount);
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining("Declaration is import, there is no declarant Address, representativeID of importer is not empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_CustomsProfile = "12345678";
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining("Declaration is export, there is no declarant Address, representativeID of supplier is empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.SetupSupplier(orgHeaderWithAccount);
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining("Declaration is export, there is no declarant Address, representativeID of supplier is not empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = declarantWithoutAccount.MainAddress.PK;
			declaration.JE_CustomsProfile = "12345678";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageErrorContaining("RepresentativeID of declarant is empty && JE_CustomsProfile is not empty => error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = declarantWithoutSameReference.MainAddress.PK;
			declaration.JE_CustomsProfile = "12345678";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageErrorContaining("CZ_account is not equal to customs profile => error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = declarantWithoutAccountValue.MainAddress.PK;
			declaration.JE_CustomsOffice = ZString.Empty;
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrorContaining("RepresentativeID of declarant is empty && JE_CustomsProfile is empty => no error", declaration.JE_OA_DeclarantAddressInfo, message);
		}

		public void TestCheckJE_CustomsGuaranteeNumber()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping("DIE");

			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "71";
			procedure.ZZ6_PreviousProcedureCode = "51";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = "DIE";
			procedure.ZZ6_Concession = "000";
			procedure.ZZ6_Description = "description";

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "NJG";
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "TestMatchAddress";
			declarantAddress.Address1 = "TestMatchAddress";

			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.AI2, "GUAB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAC", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAlternateAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAD", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Denmark);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = "EXP";
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "XX";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "7151000";

			var errormessage = "No guarantee was set, although one is required. Check that the Supplier organization or the Declarant owns a valid guarantee of type COD.";

			AssertEquals("prerequisite: is delta IE", true, declaration.IsDeltaIE);
			AssertEquals("HasGuaranteeConsumingProcedure = true", true, invoiceLine.HasGuaranteeConsumingProcedure);

			declaration.JE_CustomsGuaranteeNumber = "GUAA";
			AssertNoMessageError("Selected guarantee matches the declaration perfectly.", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAB";
			AssertHasMessageError("Selected guarantee is not a COD guarantee", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAC";
			AssertHasMessageError("Selected guarantee address doesn't match the declarant's or client's", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAD";
			AssertNoMessageError("Selected guarantee matches the declaration perfectly, we don't care if is french or not.", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "";
			AssertHasMessageError("No guarantee has been selected.", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);
		}

		public void TestCheckChargePaymentOrDestinationID()
		{
			var portCodeAdditionalReferenceMessageError = "You must include a 1CPT Additional Reference in Additional Documents Tab to declare the port code.";
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.ChargePaymentOrDestinationID = "230";
			AssertNull("Prerequisite: Declaration doesn't has Additional Reference matching ChargePaymentOrDestinationID.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));
			AssertNoMessageErrorContaining("There should be no port code Additional Reference message error in Export declaration.", declaration.ChargePaymentOrDestinationIDInfo, portCodeAdditionalReferenceMessageError);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.ChargePaymentOrDestinationID = "157";
			AssertNull("Prerequisite: Declaration doesn't has Additional Reference matching ChargePaymentOrDestinationID.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));
			AssertNoMessageErrorContaining("There should be no port code Additional Reference message error in DeltaG Import declaration.", declaration.ChargePaymentOrDestinationIDInfo, portCodeAdditionalReferenceMessageError);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.ChargePaymentOrDestinationID = "230";
			AssertNotNull("Prerequisite: Declaration has Additional Reference matching ChargePaymentOrDestinationID.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));
			AssertNoMessageErrorContaining("No message error is expected when the declaration has AdditionalReference matching ChargePaymentOrDestinationID.", declaration.ChargePaymentOrDestinationIDInfo, portCodeAdditionalReferenceMessageError);

			declaration.AdditionalInfos.RemoveAndDeleteAll();
			AssertNull("Prerequisite: Declaration doesn't has Additional Reference matching ChargePaymentOrDestinationID.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));
			declaration.Validation.ValidateChargePaymentOrDestinationID();
			AssertHasMessageErrorContaining("A message error is expected when declaration is UCC6 and has no AdditionalReference matching ChargePaymentOrDestinationID.", declaration.ChargePaymentOrDestinationIDInfo, portCodeAdditionalReferenceMessageError);

			declaration.ChargePaymentOrDestinationID = ZString.Empty;
			AssertNull("Prerequisite: Declaration doesn't has Additional Reference matching ChargePaymentOrDestinationID.", DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(declaration.AdditionalInfos));
			AssertNoMessageErrorContaining("No message error is expected when ChargePaymentOrDestinationID is empty.", declaration.ChargePaymentOrDestinationIDInfo, portCodeAdditionalReferenceMessageError);
		}

		public void TestCheckJE_CustomsProfile_IsEmpty_ButProfileListIsNotEmpty()
		{
			var expectedMessageError = "There is more than one convenient account found. Please select one manually.";
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import).WithDeltaIE();
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertNoMessageErrorContaining("There should be no message error when ProfileList is empty.", declaration.JE_CustomsProfileInfo, expectedMessageError);

			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DEC, OrgCusAccountDeltaIETypeList.Codes.DCC, "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.SetupRepresentative();
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, expectedMessageError);

			declaration.JE_CustomsProfile = "DIE001";
			AssertNoMessageErrorContaining("There should be no message error when ProfileList is selected.", declaration.JE_CustomsProfileInfo, expectedMessageError);
		}

		OrgHeader CreateOrganisationWithAddressAndEori(ZString orgCode, ZString fullName, ZString address, ZString eori)
		{
			var fr = Factory.Load<RefCountry>(Core.Constants.CountryGuids.France);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = fullName;
			orgHeader.MainAddress.OA_Address1 = address;
			orgHeader.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, fr, eori.Left(11));
			orgHeader.SetCustomsCode(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, fr, eori.Right(5));
			var representativeEORICustomsCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Core.Constants.CountryCodes.France);
			representativeEORICustomsCode.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			return orgHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
		}

		JobDeclaration declaration;
	}
}
