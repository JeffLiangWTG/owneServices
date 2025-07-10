using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaGJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckJE_LocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			declaration.JE_LocationOfGoods = string.Empty;
			AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, "An authorized location is mandatory when doing G2.");

			declaration.JE_LocationOfGoods = "ABC";
			AssertNoMessageErrors(declaration.JE_LocationOfGoodsInfo);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertNoMessageErrors(declaration.JE_LocationOfGoodsInfo);

			declaration.JE_DeltaMode = string.Empty;
			AssertNoMessageErrors(declaration.JE_LocationOfGoodsInfo);
		}

		public void TestCheckJE_CustomsGuaranteeNumber()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "71";
			procedure.ZZ6_PreviousProcedureCode = "51";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			procedure.ZZ6_Concession = "000";
			procedure.ZZ6_Description = "description";

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "NJG";
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "TestMatchAddress";
			declarantAddress.Address1 = "TestMatchAddress";

			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.AI2, "GUAB", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAC", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAlternateAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAD", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G2, "REFA", Core.Constants.CountryCodes.France);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAE", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "REFA", Core.Constants.CountryCodes.Denmark);
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "GUAF", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "TestMatchAddress", OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "XX";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "7151000";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			Factory.Save();

			var errormessage = "No guarantee was set, although one is required. Check that the Supplier organization or the Declarant owns a valid guarantee of type COD, matching Delta mode G1 for use at the selected address.";

			AssertEquals("IsGuaranteeConsumed = true", true, entryHeader.IsGuaranteeConsumed);

			declaration.JE_CustomsGuaranteeNumber = "GUAA";
			AssertNoMessageError("Selected guarantee matches the declaration perfectly.", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAB";
			AssertHasMessageError("Selected guarantee is not a COD guarantee", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAC";
			AssertHasMessageError("Selected guarantee address doesn't match the declarant's or client's", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAD";
			AssertHasMessageError("Selected guarantee doesn't feature the declaration Delta mode", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAE";
			AssertNoMessageError("Selected guarantee matches the declaration perfectly, we don't care if is french or not.", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "GUAF";
			AssertNoMessageError("Selected guarantee code is empty", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);

			declaration.JE_CustomsGuaranteeNumber = "";
			AssertHasMessageError("No guarantee has been selected.", declaration.JE_CustomsGuaranteeNumberInfo, errormessage);
		}

		public void TestJE_DeltaMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SetupImporter().SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", ZString.Empty, ZString.Empty, ZString.Empty, "5DD39475");
			declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "B", ZString.Empty, ZString.Empty, ZString.Empty, "3E93054B");
			declaration.WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("The DeltaG mode list for import should contain account types of Importer and Declarant DGI configuration.", "A, B", declaration.Lookups.DeltaModeList.CodesAsString);

			declaration.JE_DeltaMode = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_DeltaModeInfo);
			declaration.JE_DeltaMode = "ABC";
			AssertHasMessageErrorContaining(declaration.JE_DeltaModeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_DeltaMode = "B";
			AssertNoMessageErrors(declaration.JE_DeltaModeInfo);
		}

		public void TestJE_DeltaMode_DeltaAccountOrgHeaderWithoutZO_DeltaG1SubProcedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var declarant = Factory.New<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var orgCusAccount1 = declarant.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount1.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount1.CZ_Account = "12345678";
			orgCusAccount1.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount1.CZ_OH = declarant.PK;
			orgCusAccount1.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount1.CZ_RepresentativeID = "123456";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			CombineAssertions(() =>
			{
				AssertEquals(declaration.DeltaAccountOrgHeader.PK, declarant.PK);
				var orgImpAddInfo = FROrgImpAddInfo.Get(declarant);
				orgImpAddInfo.ZO_DeltaG1SubProcedure = ZString.Empty;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertHasMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertNoMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
				orgImpAddInfo.ZO_DeltaG1SubProcedure = "C";
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertNoMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
			});

			var importer = declaration.SetupImporter();
			var orgCusAccount2 = importer.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount2.CZ_Account = "TESTIMP";
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_OH = importer.PK;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount2.CZ_RepresentativeID = "123456";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "TESTIMP";

			CombineAssertions(() =>
			{
				AssertEquals(declaration.DeltaAccountOrgHeader.PK, importer.PK);
				var orgImpAddInfo = FROrgImpAddInfo.Get(importer);
				orgImpAddInfo.ZO_DeltaG1SubProcedure = ZString.Empty;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertHasMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertNoMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
				orgImpAddInfo.ZO_DeltaG1SubProcedure = "C";
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertNoMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
			});

			var exporter = declaration.SetupSupplier();
			var orgCusAccount3 = exporter.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount3.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount3.CZ_Account = "TESTEXP";
			orgCusAccount3.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount3.CZ_OH = exporter.PK;
			orgCusAccount3.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount3.CZ_RepresentativeID = "123456";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_OH_Exporter = exporter.PK;
			declaration.JE_CustomsProfile = "TESTEXP";

			CombineAssertions(() =>
			{
				AssertEquals(declaration.DeltaAccountOrgHeader.PK, exporter.PK);
				var orgImpAddInfo = FROrgImpAddInfo.Get(exporter);
				orgImpAddInfo.ZO_DeltaG1SubProcedure = ZString.Empty;
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertHasMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				AssertNoMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
				orgImpAddInfo.ZO_DeltaG1SubProcedure = "C";
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				AssertNoMessageErrorContaining(declaration.JE_DeltaModeInfo, "The agreement owner Delta G1 sub procedure has not been set yet.");
			});
		}

		public void TestCheckJE_CustomsProfileWithoutDeltaAgreement()
		{
			var declarant = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, "Importer or Declarant must have an agreement number.");

			var orgCusAccount = declarant.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount.CZ_Account = "12345678";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = declarant.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_RepresentativeID = "123456";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, "Importer or Declarant must have an agreement number.");

			declarant.DeltaAgreementNumberCollection.RemoveAndDeleteAll();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, "Importer or Declarant must have an agreement number.");

			declaration.SetupImporter();
			orgCusAccount = declaration.Importer.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount.CZ_Account = "12345678";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = declarant.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_RepresentativeID = "123456";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, "Importer or Declarant must have an agreement number.");
		}

		public void TestCheckJE_OA_DeclarantAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var orgAddress = orgHeader.Addresses.AddNewMainAddress();
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;

			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.DeclarantEoriNotConfigured);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A", Core.Constants.CountryCodes.France);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.DeclarantEoriNotConfigured);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "B", Core.Constants.CountryCodes.Denmark);
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("No EOR missing error as long as one EOR of EUN-countries is provided.", declaration.JE_OA_DeclarantAddressInfo, ErrorCollectorHelper.DeclarantEoriNotConfigured);
		}

		public void TestCheckJE_OA_DeclarantAddress_DeclarantNeed_A_NotEmpty_CZAccount()
		{
			var message = "Declarant/Representative ID is not configured. Config it in Organization > Config > France tab.";
			var orgHeaderWithAccount = Factory.New<OrgHeader>();
			var declarantWithoutAccount = Factory.New<OrgHeader>();
			var declarantWithoutSameReference = Factory.New<OrgHeader>();
			var declarantWithoutAccountValue = Factory.New<OrgHeader>();

			var orgCusAccount = orgHeaderWithAccount.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount.CZ_Account = "12345678";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = orgHeaderWithAccount.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount.CZ_RepresentativeID = "123456";

			var orgCusAccount2 = declarantWithoutAccount.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount2.CZ_Account = "12345678";
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_OH = declarantWithoutAccount.PK;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount2.CZ_RepresentativeID = ZString.Empty;

			var orgCusAccount3 = declarantWithoutSameReference.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount3.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount3.CZ_Account = "87654321";
			orgCusAccount3.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount3.CZ_OH = declarantWithoutSameReference.PK;
			orgCusAccount3.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount3.CZ_RepresentativeID = "123456";

			var orgCusAccount4 = declarantWithoutAccountValue.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount4.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount4.CZ_Account = "";
			orgCusAccount4.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount4.CZ_OH = declarantWithoutAccountValue.PK;
			orgCusAccount4.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			orgCusAccount4.CZ_RepresentativeID = "";

			var orgCusAccount5 = declarantWithoutAccount.DeltaAgreementNumberCollection.AddNew();
			orgCusAccount5.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount5.CZ_Account = "12345678";
			orgCusAccount5.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount5.CZ_OH = declarantWithoutAccount.PK;
			orgCusAccount5.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount5.CZ_RepresentativeID = "34657";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			declaration.JE_OA_DeclarantAddress = orgHeaderWithAccount.MainAddress.PK;
			declaration.JE_CustomsProfile = "12345678";
			AssertNoMessageErrorContaining("RepresentativeID of declarant is not empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_CustomsProfile = "12345678";
			AssertHasMessageErrorContaining("There is no declarant Address && JE_CustomsProfile is not empty  => error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.SetupImporter(orgHeaderWithAccount);
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_CustomsProfile = "12345678";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertNoMessageErrorContaining("Declaration is import, there is no declarant Address, representativeID of importer is not empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_CustomsProfile = "12345678";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertHasMessageErrorContaining("Declaration is export, there is no declarant Address, representativeID of supplier is empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.SetupSupplier(orgHeaderWithAccount);
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_CustomsProfile = "12345678";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertNoMessageErrorContaining("Declaration is export, there is no declarant Address, representativeID of supplier is not empty && JE_CustomsProfile is not empty  => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = declarantWithoutAccount.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertHasMessageErrorContaining("RepresentativeID of declarant is empty && JE_CustomsProfile is not empty => error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = declarantWithoutSameReference.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_CustomsProfile = "12345678";
			AssertHasMessageErrorContaining("CZ_account is not equal to customs profile => error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = declarantWithoutAccountValue.MainAddress.PK;
			declaration.JE_CustomsOffice = ZString.Empty;
			AssertNoMessageErrorContaining("RepresentativeID of declarant is empty && JE_CustomsProfile is empty => no error", declaration.JE_OA_DeclarantAddressInfo, message);

			declaration.JE_OA_DeclarantAddress = declarantWithoutAccount.MainAddress.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			AssertNoMessageErrorContaining("RepresentativeID of declarant is not empty nor G2 && JE_CustomsProfile is not empty => no error", declaration.JE_OA_DeclarantAddressInfo, message);
		}
	}
}
