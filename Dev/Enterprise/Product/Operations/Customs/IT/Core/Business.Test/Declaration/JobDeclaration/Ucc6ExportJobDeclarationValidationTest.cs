using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportJobDeclarationValidationTest : TestCaseWithFactory
{
	public void TestCheckJE_CustomsOffice_ValidateRuleR0675_OfficeOfPresentation_Export()
	{
		declaration.JE_MessageType = "EXP";
		var validation = declaration.Validation;
		var expectedErrorMessage = "[R0675] The declaration requires an office of type 'Office of Presentation for Centralized Clearance' with purpose PRE";
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		var customsOffice = GetNewCustomsOfficeWithCode(declaration, "ZZZ");

		authorizationUsage.AGC_Code = "CCL";
		validation.ValidateJE_CustomsOffice();
		AssertNoMessageErrorContaining("In non UCC6 Export declaration, even if no Customs Office of Type PRE has been provided, no error message is expected", declaration.JE_CustomsOfficeInfo, expectedErrorMessage);

		declaration.JE_MessageType = "IMP";
		customsOffice = GetNewCustomsOfficeWithCode(declaration, "XXX");
		validation = declaration.Validation;
		validation.ValidateJE_CustomsOffice();
		AssertNoMessageErrorContaining("In Import declaration, even if no Customs Office of Type PRE has been provided, no error message is expected", declaration.JE_CustomsOfficeInfo, expectedErrorMessage);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = "EXP";
				customsOffice = GetNewCustomsOfficeWithCode(declaration, "YYY");
				validation = declaration.Validation;
				validation.ValidateJE_CustomsOffice();
				AssertHasMessageErrorContaining("When no Customs Office of Type PRE has been provided, error message is expected", declaration.JE_CustomsOfficeInfo, expectedErrorMessage);

				customsOffice.CY_Code = "PRE";
				validation.ValidateJE_CustomsOffice();
				AssertNoMessageErrorContaining("No error message expected when a Customs Office of Type PRE has been provided", declaration.JE_CustomsOfficeInfo, expectedErrorMessage);
			});
		}
	}

	public void TestCheckJE_OA_DeclarantAddress_UCC6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var validation = declaration.Validation;
		var declarantInfo = declaration.JE_OA_DeclarantAddressInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining("When declaration is UCC6, error message is expected for empty Declarant", declarantInfo, MandatoryValidation.YouHaveNotEntered);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			AssertNoMessageErrorContaining("No error message is expected for filled Declarant", declarantInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestExporterDocAddressRequirement_ValidateOrganisationPK_SingleInstruction()
	{
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			var validateExporterDocAddress = new Action(() => declaration.ExporterDocAddress.Validation.ValidateAll());
			var info = declaration.ExporterDocAddress.OrganisationPKInfo;

			AssertEquals("CE_Style is empty", string.Empty, entryInstruction.CEI_Style);
			AssertEquals("Exporter is empty", ZGuid.Empty, declaration.ExporterDocAddress.OrganisationPK);

			validateExporterDocAddress();
			AssertHasMessageErrorContaining("Exporter and CE_Style are empty", info, "You have not entered");

			entryInstruction.CEI_Style = "NOTC2";
			validateExporterDocAddress();
			AssertHasMessageErrorContaining("Exporter is empty and CE_Style is NOT C2", info, "You have not entered");

			entryInstruction.CEI_Style = "C2";
			validateExporterDocAddress();
			AssertNoMessageErrorContaining("Exporter is empty and CE_Style is C2", info, "You have not entered");

			entryInstruction.CEI_Style = string.Empty;
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ExporterDocAddress.OrganisationPK = exporter.PK;
			validateExporterDocAddress();
			AssertNoMessageErrorContaining("Exporter is filled, CE_Style is empty ", info, "You have not entered");
		}
	}

	public void TestExporterDocAddressRequirement_ValidateOrganisationPK_MultipleInstructions()
	{
		declaration.JE_MessageType = "EXP";
		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction1.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction2.PK;

		var validateExporterDocAddress = new Action(() => declaration.ExporterDocAddress.Validation.ValidateAll());
		var info = declaration.ExporterDocAddress.OrganisationPKInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: false))
		{
			AssertEquals("CE_Style is empty in instruction 1", string.Empty, entryInstruction1.CEI_Style);
			AssertEquals("CE_Style is empty in instruction 2", string.Empty, entryInstruction2.CEI_Style);
			AssertEquals("Exporter is empty", ZGuid.Empty, declaration.ExporterDocAddress.OrganisationPK);

			validateExporterDocAddress();
			AssertNoMessageErrorContaining("No validation when declaration is not UCC6", info, "You have not entered");
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			AssertEquals("Exporter is empty", ZGuid.Empty, declaration.ExporterDocAddress.OrganisationPK);

			entryInstruction1.CEI_Style = "C2";
			entryInstruction2.CEI_Style = "NOTC2";
			validateExporterDocAddress();
			AssertHasMessageErrorContaining("Instruction 1 is C2, instruction 2 is not C2 ", info, "You have not entered");

			entryInstruction1.CEI_Style = "C2";
			entryInstruction2.CEI_Style = string.Empty;
			validateExporterDocAddress();
			AssertHasMessageErrorContaining("Instruction 1 is C2, instruction 2 is empty ", info, "You have not entered");

			entryInstruction1.CEI_Style = "C2";
			entryInstruction2.CEI_Style = "C2";
			validateExporterDocAddress();
			AssertNoMessageErrorContaining("Both instructions are C2 ", info, "You have not entered");

			entryInstruction1.CEI_Style = string.Empty;
			entryInstruction2.CEI_Style = string.Empty;
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			declaration.ExporterDocAddress.OrganisationPK = exporter.PK;
			validateExporterDocAddress();
			AssertNoMessageErrorContaining("both instructions with empty style, Exporter filled", info, "You have not entered");
		}
	}

	public void TestExporterDocAddressRequirement_ValidateOrganisationPK_NoInstruction()
	{
		declaration.JE_MessageType = "EXP";
		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var validateExporterDocAddress = new Action(() => declaration.ExporterDocAddress.Validation.ValidateAll());
		var info = declaration.ExporterDocAddress.OrganisationPKInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			AssertEquals("Exporter is empty", ZGuid.Empty, declaration.ExporterDocAddress.OrganisationPK);

			validateExporterDocAddress();
			AssertNoMessageErrorContaining("When no instruction is available, message is not expected", info, "You have not entered");

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			validateExporterDocAddress();
			AssertHasMessageErrorContaining("When instruction is available, message is expected", info, "You have not entered");
		}
	}

	public void TestCheckGoodsLocationDescription_IncludeValidationFromAddressAndGoodsLocation()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var goodsLocation = declaration.GoodsLocation;
		var address = goodsLocation.Address;
		var validation = new ExportJobDeclarationValidation(declaration);

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			goodsLocation.CGL_Qualifier = "Z";
			address.E2_AddressOverride = true;
			address.E2_Contact = "Some Person";
			validation.ValidateGoodsLocationDescription();
			const string expectedMessageError = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";
			AssertHasErrorContaining(declaration.GoodsLocationDescriptionInfo, expectedMessageError);
		}
	}

	public void TestCheckJE_OA_DeclarantAddressAgainstTransitionPeriod()
	{
		var expectedNameWarningForTransitionPeriod = "Declarant Company Name is longer than 35 characters, it will be truncated in the message.";
		var expectedNameWarningForNonTransitionPeriod = "Declarant Company Name is longer than 70 characters, it will be truncated in the message.";

		var declarant = Factory.New<OrgHeader>();
		var declarantAddress = declarant.MainAddress;
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		var validation = declaration.Validation;
		var declarantAddressInfo = declaration.JE_OA_DeclarantAddressInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				CombineAssertions("When is UCC6 and InTransition Period", () =>
				{
					declarant.OH_FullName = "".PadRight(36, 'D');
					validation.ValidateJE_OA_DeclarantAddress();
					AssertHasWarningContaining(declarantAddressInfo, expectedNameWarningForTransitionPeriod);

					declarant.OH_FullName = "".PadRight(30, 'D');
					validation.ValidateJE_OA_DeclarantAddress();
					AssertNoWarningContaining(declarantAddressInfo, expectedNameWarningForTransitionPeriod);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				CombineAssertions("When is UCC6 but not In transition period", () =>
				{
					declarant.OH_FullName = "".PadRight(71, 'D');
					validation.ValidateJE_OA_DeclarantAddress();
					AssertHasWarningContaining(declarantAddressInfo, expectedNameWarningForNonTransitionPeriod);

					declarant.OH_FullName = "".PadRight(30, 'D');
					validation.ValidateJE_OA_DeclarantAddress();
					AssertNoWarningContaining(declarantAddressInfo, expectedNameWarningForNonTransitionPeriod);
				});
			}
		}
	}

	public void TestCheckJE_DefermentAccountNumberForPaymentParty1()
		=> AssertJE_DefermentAccountNumberForPaymentParty(paymentParty: "1", ownerNameForMessages: "Declarant", owner => declaration.JE_OA_DeclarantAddress = owner.MainAddress.PK, () => declaration.JE_OA_DeclarantAddress = ZGuid.Empty);

	public void TestCheckJE_DefermentAccountNumberForPaymentParty2()
		=> AssertJE_DefermentAccountNumberForPaymentParty(paymentParty: "2", ownerNameForMessages: "Importer", owner => declaration.JE_OH_Importer = owner.PK, () => declaration.JE_OH_Importer = ZGuid.Empty);

	public void TestCheckJE_DefermentAccountNumberForPaymentParty3()
		=> AssertJE_DefermentAccountNumberForPaymentParty(paymentParty: "3", ownerNameForMessages: "Forwarder", owner => declaration.JE_OH_Forwarder = owner.PK, () => declaration.JE_OH_Forwarder = ZGuid.Empty);

	public void TestCheckJE_DefermentAccountNumberForPaymentParty4()
		=> AssertJE_DefermentAccountNumberForPaymentParty(paymentParty: "4", ownerNameForMessages: "Supplier", owner => declaration.JE_OH_Supplier = owner.PK, () => declaration.JE_OH_Supplier = ZGuid.Empty);

	public void TestCheckJE_DefermentAccountNumberForPaymentParty5()
		=> AssertJE_DefermentAccountNumberForPaymentParty(paymentParty: "5", ownerNameForMessages: "Exporter", owner => declaration.ExporterDocAddress.OrganisationPK = owner.PK, () => declaration.ExporterDocAddress.OrganisationPK = ZGuid.Empty);

	public void TestRequireJE_ShipmentIncoTermPlaceMandatory()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var validation = new Ucc6ExportJobDeclarationValidationForTest(declaration);
			AssertEquals("[PRE-CONDITION] JE_ShipmentIncoTerm is Empty", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);

			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("JE_ShipmentIncoTerm=FOB", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);

			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.Other;
			AssertEquals("JE_ShipmentIncoTerm=XXX", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);

			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriagePaidTo;
			AssertEquals("JE_ShipmentIncoTerm=CPT", false, validation.RequireJE_ShipmentIncoTermPlaceMandatory_Exposed);
		}
	}

	public void TestCheckJE_ShipmentIncoTermPlace_RequiredForUCC6_XXX()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_ShipmentIncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_ShipmentIncoTermPlaceInfo);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_ShipmentIncoTerm = Enterprise.Core.Constants.IncoTerms.Other;
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(declaration.JE_ShipmentIncoTermPlaceInfo);
		}
	}

	public void TestCheckJE_OH_ShippingLineWithCusCodesAndDeclarant()
	{
		var expectedMessage = "Carrier has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers/Codes";
		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_ShippingLine = carrier.PK;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			CombineAssertions("Carrier different from Declarant", () =>
			{
				AssertNotEquals("Carrier and Declarant", declaration.JE_OH_ShippingLine, declaration.Declarant.OA_OH);

				carrier.CustomsCodes.AddNew("EOR", "123", "IT");
				declaration.Validation.ValidateJE_OH_ShippingLine();
				AssertNoMessageErrorContaining("When EOR added", declaration.JE_OH_ShippingLineInfo, expectedMessage);

				carrier.CustomsCodes.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_OH_ShippingLine();
				AssertHasMessageErrorContaining("When EOR and TCU codes missing", declaration.JE_OH_ShippingLineInfo, expectedMessage);

				carrier.CustomsCodes.AddNew("TCU", "123", "IT");
				declaration.Validation.ValidateJE_OH_ShippingLine();
				AssertNoMessageErrorContaining("When TCU added", declaration.JE_OH_ShippingLineInfo, expectedMessage);
			});

			CombineAssertions("Carrier same as Declarant", () =>
			{
				declaration.JE_OA_DeclarantAddress = carrier.MainAddress.PK;
				AssertEquals("Carrier and Declarant", declaration.JE_OH_ShippingLine, declaration.Declarant.OA_OH);

				carrier.CustomsCodes.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_OH_ShippingLine();
				AssertNoMessageErrorContaining("When EOR and TCU codes missing", declaration.JE_OH_ShippingLineInfo, expectedMessage);
			});

			CombineAssertions("Carrier and no Declarant", () =>
			{
				declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				AssertNull(declaration.Declarant);

				carrier.CustomsCodes.AddNew("EOR", "123", "IT");
				declaration.Validation.ValidateJE_OH_ShippingLine();
				AssertNoMessageErrorContaining("When EOR added", declaration.JE_OH_ShippingLineInfo, expectedMessage);

				carrier.CustomsCodes.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_OH_ShippingLine();
				AssertHasMessageErrorContaining("When EOR and TCU codes missing", declaration.JE_OH_ShippingLineInfo, expectedMessage);

				carrier.CustomsCodes.AddNew("TCU", "123", "IT");
				declaration.Validation.ValidateJE_OH_ShippingLine();
				AssertNoMessageErrorContaining("When TCU added", declaration.JE_OH_ShippingLineInfo, expectedMessage);
			});
		}
	}

	public void TestCheckJE_OA_DeclarantAddress_EORIAndTCUNeeded()
	{
		const string expectedMessage = "Declarant has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers/Codes.";
		var declarantAddress = Factory.NewWithValidTestData<OrgHeader>();
		var declarantAddressInfo = declaration.JE_OA_DeclarantAddressInfo;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			var ucc6ExportJobDeclarationValidation = declaration.Validation;
			declaration.JE_OA_DeclarantAddress = declarantAddress.MainAddress.PK;
			declarantAddress.CustomsCodes.AddNew("IVA", "456", "IT");
			AssertHasMessageErrorContaining("When Declarant and both EOR and TCU are missing", declarantAddressInfo, expectedMessage);

			declarantAddress.CustomsCodes.AddNew("EOR", "123", "IT");
			ucc6ExportJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrorContaining("When Declarant has only EOR", declarantAddressInfo, expectedMessage);

			declarantAddress.CustomsCodes.RemoveAndDeleteAll();
			declarantAddress.CustomsCodes.AddNew("TCU", "456", "IT");
			ucc6ExportJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrorContaining("When Declarant has only TCU", declarantAddressInfo, expectedMessage);

			declarantAddress.CustomsCodes.RemoveAndDeleteAll();
			declarantAddress.CustomsCodes.AddNew("EOR", "123", "IT");
			declarantAddress.CustomsCodes.AddNew("TCU", "456", "IT");
			ucc6ExportJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrorContaining("When Declarant has EOR + TCU", declarantAddressInfo, expectedMessage);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			ucc6ExportJobDeclarationValidation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageErrorContaining("When Declarant Address is Empty", declarantAddressInfo, expectedMessage);
		}
	}

	public void TestCheckJE_OA_Representative_EORINeeded()
	{
		const string expectedMessage = "Representative has no EORI code. Please consider adding the 'EOR' code in Organization > Config > Registration Numbers/Codes.";
		var representative = Factory.NewWithValidTestData<OrgHeader>();
		var representativeAddressInfo = declaration.JE_OA_RepresentativeInfo;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			var ucc6ExportJobDeclarationValidation = declaration.Validation;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			representative.CustomsCodes.AddNew("IVA", "456", "IT");
			ucc6ExportJobDeclarationValidation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining("When Representative has no EOR", representativeAddressInfo, expectedMessage);

			representative.CustomsCodes.AddNew("TCU", "456", "IT");
			ucc6ExportJobDeclarationValidation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining("When Representative has even TCU", representativeAddressInfo, expectedMessage);

			representative.CustomsCodes.AddNew("EOR", "123", "IT");
			ucc6ExportJobDeclarationValidation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When Representative has EOR", representativeAddressInfo, expectedMessage);

			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNoMessageErrorContaining("When Representative Address is Empty", representativeAddressInfo, expectedMessage);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isActive)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isActive);

	void AssertJE_DefermentAccountNumberForPaymentParty(string paymentParty
		, string ownerNameForMessages
		, Action<OrgHeader> setOwnerAction
		, Action clearOwnerAction)
	{
		var expectedMessage = $"For Payment Party = {paymentParty} an Entry Instruction > Authorization must be present, with Code = DPO and Owner = {ownerNameForMessages}";
		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

		setOwnerAction(orgHeader1);
		declaration.JE_PaymentMethod = paymentParty;
		declaration.JE_DefermentAccountNumber = "12345678A";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: false))
		{
			CombineAssertions($"For Non Ucc6 Export and Payment Party = {paymentParty}", () =>
			{
				entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertNoMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} - entry missing", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader2.PK;
				cusAuthorizationUsage.AGC_Code = "DPO";
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertNoMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} and owner different", declaration.JE_DefermentAccountNumberInfo, expectedMessage);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			CombineAssertions($"For Ucc6 Export and Payment Party = {paymentParty}", () =>
			{
				entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertHasMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} - entry missing", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader2.PK;
				cusAuthorizationUsage.AGC_Code = "DPO";
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertHasMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} and owner different", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

				cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader1.PK;
				cusAuthorizationUsage.AGC_Code = "DPO";
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertNoMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} present", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

				clearOwnerAction();
				declaration.JE_DefermentAccountNumber = "12345678A";
				entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage.AGC_Code = "DPO";
				cusAuthorizationUsage.AGC_OH_Owner = ZGuid.Empty;
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertHasMessageErrorContaining($"When DPO Authorization with {ownerNameForMessages} and owner are empty", declaration.JE_DefermentAccountNumberInfo, expectedMessage);
			});
		}
	}

	OfficeCode GetNewCustomsOfficeWithCode(JobDeclaration declaration, string code)
	{
		var customsOffice = declaration.CustomsOffices.AddNew();
		customsOffice.CY_Data = "IT0001";
		customsOffice.CY_Code = code;

		return customsOffice;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
}

sealed class Ucc6ExportJobDeclarationValidationForTest : Ucc6ExportJobDeclarationValidation
{
	public Ucc6ExportJobDeclarationValidationForTest(JobDeclaration parent) : base(parent)
	{
	}

	public ZBool RequireJE_ShipmentIncoTermPlaceMandatory_Exposed => RequireJE_ShipmentIncoTermPlaceMandatory;
}
