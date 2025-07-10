using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public static class ValidationCaptions
{
	#region Shared

	public static class Shared
	{
		public static string InvoicesLinkedHaveDifferentIncoPlaces => Res.GetString("1CF97694-4694-47F0-906A-027E243359B7", "Invoices linked to the same Entry Instruction have different [20.2] Agreed Place");
		public static string InvoicesLinkedHaveDifferentAgreedPlaceCode => Res.GetString("CF38D500-A710-41F4-940F-40E18DE629B5", "Invoices linked to the same Entry Instruction have different [20.3] Agreed Place Code");
		public static string InvoicesLinkedHaveDifferentDeliveryTerms => Res.GetString("072BB9D7-970D-4AA6-B3E3-0AE195960718", "Invoices linked to the same Entry Instruction have different Delivery Terms");
		public static string GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(ZPropertyInfo propertyInfo, int customsMaxLength) => GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(propertyInfo?.HumanReadableName ?? ZString.Empty, customsMaxLength);
		public static string GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(string fieldName, int customsMaxLength) => Res.GetString("8E413A5C-379D-42AF-9E47-68FD1EB1AD1B", "{0} exceeds the maximum allowed length in the declaration message ({1} characters). Excess characters will be truncated.", fieldName, customsMaxLength);
		public static string GetFieldExceedsCustomsMaxLengthCaption(int customsMaxLength) => Res.GetString("A0FF71BB-4842-4D64-8D18-D9CB12864BE3", "Field exceeds the maximum allowed length in the declaration message ({0} characters).", customsMaxLength);
		public static string IncotermInvalid => Res.GetString("8EA207E0-B817-498B-8DCD-2FA7D428D47F", "This code is not valid for Italian Customs.");
		public static string SelectedNodeHasNoMAUCertificate => Res.GetString("136A63B8-EE99-4DFD-B8C8-25CEA1CA2620", "The Node you selected has no MAU Certificate. Please add it in Company>Brokerage for your company.");
		public static string DeclarationDispatchAndInvoiceLinesCountryOfExportCannotBeEnteredAtTheSameTime => Res.GetString("204E3D47-3346-4386-8388-527CBB921858", "Declaration > Dispatch and Invoice Lines > Country of Export cannot be entered at the same time.");
		public static string FieldIsLongerThanMaximumLength(string traderName, string fieldName, int maximumLength)
		{
			return Res.GetString("9AB2D33B-B2FC-42CA-965C-622E26C5ECC6", "{0} {1} is longer than {2} characters, it will be truncated in the message.", traderName, fieldName, maximumLength);
		}

		public static string RepresentativeCaption => Res.GetString("67D6D3D4-6ADE-419D-8AE7-B179EC82E68A", "Representative");
		public static string ConsignorCaption => Res.GetString("EE621204-E75A-4184-AB7E-8D6CB8A1A207", "Consignor");
		public static string ConsigneeCaption => Res.GetString("91DC4B08-71B8-4D5F-820D-049B3BF7ED17", "Consignee");
	}

	#endregion

	#region Entry Instruction

	public static class EntryInstruction
	{
		public static string TemporaryProcedureLimitDateIsNotRequiredForEmptyProcedure => Res.GetString("35ED942A-9877-4691-9EF6-B666B268D8F1", "Temporary Procedure Limit Date is not required for empty Procedure Code");
		public static string TemporaryProcedureLimitDateIsNotRequiredForProcedureCode(ZString procedureCode) => Res.GetString("A9B260C8-6900-4BB5-829F-69B492914473", "Temporary Procedure Limit Date is not required for Procedure Code {0}", procedureCode);
		public static string TemporaryProcedureLimitDateShouldBeGreaterThanToday => Res.GetString("42600078-603E-40E1-8BDB-9AA58A95E60C", "Temporary Procedure Limit Date should be greater than today.");
		public static string AcceptanceDateShouldBe(ZDate expectedDate) => Res.GetString("148917A3-76C2-4193-B009-4A8E472A0266", "The Acceptance Date should be {0}", expectedDate.ToShortDateString());
		public static string DeclarationTypeMustBeTheSame => Res.GetString("3BD9A54A-7D2D-4474-9EDE-D23920F1DB32", "The Declaration Type must be the same for every Entry Instruction");
		public static string NoIncotermFoundInInvoicesLinkedToThisEntryInstruction => Res.GetString("99455E72-469B-CB83-4D25-86FBD9BC5027", "No Incoterm [20] found in Invoices linked to this Entry Instruction");
		public static string InvoicesLinkedToThiEntryInstructionHaveDifferentIncoTerm => Res.GetString("1C7BD97A-6FC1-248A-426C-569578E0F08F", "Invoices linked to this Entry Instruction have different Incoterms [20]");
		public static string NoTransactionNatureFoundInInvoicesLinkedToThisEntryInstruction => Res.GetString("9EB9EBD7-0B73-48DA-BCE7-E013492CEE76", "No Transaction Nature [24] found in Invoices linked to this Entry Instruction");
		public static string NoCurrencyFoundInInvoicesLinkedToThisEntryInstruction => Res.GetString("6C243A06-C774-4654-8135-09EA3456C86A", "No Currency [22] found in Invoices linked to this Entry Instruction");
		public static string InvoicesLinkedToThiEntryInstructionHaveDifferentCurrency => Res.GetString("1A14C868-4FA6-4703-8006-F02DBEA51574", "Invoices linked to this Entry Instruction have different Currency [22]");
		public static string FromWarehouseShouldNotBeEmptyIfHasInvoiceLinesThatRequireIt => Res.GetString("11D2495A-A4BA-4A63-9775-93D220D9C25E", "At least one invoice line uses a CPC that requires that [49] From Warehouse is set. Please select a value");
		public static string ToWarehouseShouldNotBeEmptyIfHasInvoiceLinesThatRequireIt => Res.GetString("4FD9ED7E-A429-4F22-9E39-1E420D1AF2B8", "At least one invoice line uses a CPC that requires that [49] To Warehouse is set. Please select a value");
		public static string RequireAuthorizationTypeOPOWhenDeclarationTypeIsB2AndProcedureCodeIs21Or22 => Res.GetString("875C7ED0-3880-11EF-8372-292AE0CF9ADE", "For this Declaration Type and Procedure code, an Authorization of type 'OPO' must be present");
		public static string FallbackDeclarationTypeCanOnlyBeSubmittedInPaperForm => Res.GetString("AD9F42B9-87CD-4797-AE6B-98769D3A981B", "Declaration type DSE cannot be submitted to Customs with a file interchange, but only in paper form");
		public static string FinancialAndBankingDataLineExceedMaxLength(int customsMaxLength) => Res.GetString("D75B9562-6485-4CE2-B86B-E0C00F31B26A", "You have entered more than {0} characters, data could be truncated in printing", customsMaxLength);
		public static string CannotFindAuthorisationForWarehouse => Res.GetString("1CED2F95-1180-4F3F-B997-136F5E3F4F5F", "Cannot find the Authorization for this Warehouse. Consider adding this warehouse code to an Authorization");
		public static string PreliminaryDeclarationMustHaveStyleCod => Res.GetString("6811333C-2FED-494C-BAFD-EA492D4083BA", "A declaration with Sub-Style = D must have Type = COD");
		public static string AcceptanceDateMustBeEmptyForPreliminaryDeclaration => Res.GetString("752ADFD0-069F-490B-A638-E73D8039B1ED", "For Sub-Style = D the Acceptance Date must be empty");
		public static string TemporaryProcedureLimitDateRequired => Res.GetString("B7EAD0DF-4B24-4762-9032-BE6FAF54470F", "Temporary procedure Limit Date is required if Procedure Code is temporary - CN4");
		public static string FieldMustBeEmptyForThisKindOfDeclaration => Res.GetString("21CAAC61-3247-4969-A137-C87DAB009E98", "Field must be empty for this kind of declaration");
		public static string GetEntryStyleNotAllowedForADeclarationType(string entryStyle) => Res.GetString("A2888838-C598-4ACB-B128-A564BBEA6E50", "The Entry Style you selected {0} is not valid for this Declaration Type", entryStyle);
		public static string InvoicesLinkedToThiEntryInstructionMustHaveSameDeliveryTerm => Res.GetString("524B4216-FC7A-43B8-9E62-5ECDBFAD2763", "All Invoices on an Entry Instruction must have the same Delivery Terms");
		public static string MustHaveFRxFiscalReference(params ZString[] fiscalCodes)
		{
			var formattedFiscalCodes = fiscalCodes.Select(x => new ZString(FormattableString.Invariant($"'{x}'")))
				.ToArray();

			return Res.GetString("F870EC65-FFC1-4A05-A482-0BD0C7305706"
				, "When CPC is 42 or 63, {0} code must exist in 'Fiscal References' Tab in Entry Instruction or in all linked 'Invoice Lines' Tab.",
				ZString.Join((NoResString)" or ", formattedFiscalCodes));
		}
		public static string MustNotHaveSameReferenceForFiscalCode(string fiscalCode) => Res.GetString("0D6D778E-C9E8-4D94-B4DD-8654291E8E91", "When CPC is 42 or 63, all linked Invoice Lines must not have the same 'Reference' for '{0}' code in their 'Fiscal References' Tab.", fiscalCode);
		public static string MultipleAuthorisationFoundMessage => Res.GetString("968ECC3C-DFB7-4C87-8C06-8FC73CF18281", "More than one Authorization of type CW1, CW2 or CWP exists for Warehouse");
		public static string NoAuthorisationConfiguredMessage => Res.GetString("6796ADA1-CCFF-4C6A-A1D6-BDE7D036E19A", "No Authorization of type CW1, CW2 or CWP exists for Warehouse");
		public static string SDECodeMustBePresentInEntryInstructionAuthorizationsType => Res.GetString("792A7B56-650E-4231-927E-DCC8C444FB0C", "[R0677] A 'SDE' code must be present in Entry instruction > authorizations > type");
		public static string SupportingDocumentsNotAllowedAtHeaderLevelInTransitionPeriod => Res.GetString("071f5b13-f3ce-41dc-9d4c-54e121dd0da2", "[E1301] During the transition period, which is active now, Supporting documents at header level must not be used");
		public static string SupportingDocumentCountForExportMustNotExceed99 => Res.GetString("89325b00-c426-4375-9b82-ac126359527d", "You have entered more than 99 Supporting Documents");
		public static string PreviousDocumentMustBeFilledInEntryInstructionsForSubStyleXorYRuleB1905 => Res.GetString("041DE339-296E-46E7-95EE-E36AC35514C0", "[B1905] For Sub Style X or Y a Previous Document must be filled in Entry Instructions.");
		public static string PreviousDocumentMustNotPresentInEntryInstructionsForSubStyleNotXorYRuleB1905 => Res.GetString("4F9498B4-5DC1-41B4-B1A2-72761532CB95", "[B1905] For Sub Style different from X and Y no Previous Documents must be present in Entry Instructions.");
		public static string WarehouseShouldHaveVATFiscalArea => Res.GetString("CC68A2FE-6951-46F7-9801-A87DF5CB5003", "There is no Warehouse of type VAT Fiscal for the selected Organization and Address");
	}

	#endregion

	#region GuaranteeForEntryInstruction

	public static class GuaranteeForEntryInstruction
	{
		public static string GuaranteeNumberShouldBeEnteredInReference2 => Res.GetString("99E7044E-1B04-499B-A855-3E0616A61315", "For H3 or H4 message the Guarantee Number should be entered in Reference 2");
	}

	#endregion

	#region Account

	public static class Account
	{
		public static string EnterAccountNumber => Res.GetString("E22D4005-DE52-465E-B771-101624A59761", "Enter an Account Number");
		public static string AccountLengthMustBe15Or20 => Res.GetString("9EE402D2-5151-47C7-96D7-98B7B40F2401", "Account Numbers must be 15 or 20 characters in length");
		public static string AccountFormat => Res.GetString("C0C0DB9C-7B15-4C8A-B760-EF8231214ED0", "Account Numbers must be in the format XXXXXXXXXXX-XXX for VAT Code or XXXXXXXXXXXXXXXX-XXX for Fiscal Code");
		public static string AccountNumberMustBeUnique => Res.GetString("30A5A9C4-50B7-4E89-819B-DB040CF572C5", "Account Number must be unique");
		public static string AccountNodeMustBeUnique => Res.GetString("FE567F57-F298-4D9E-8D2F-918CA697EBE0", "Account Node must be unique");
		public static string AccountNumberAlreadyExistsInAnotherCompany(ZString companyName) => Res.GetString("994F663A-18AC-4E92-9C8E-4F2F9BE4D318", "This Account Number already exists in company {0}, it is not possible to have duplicated Account Numbers", companyName);
		public static string AccountNodeAlreadyExistsInAnotherCompany(ZString companyName) => Res.GetString("960360EE-2600-4395-A621-D290106FE625", "This Account Node already exists in company {0}, it is not possible to have duplicated Account Nodes", companyName);
		public static string EnterPassword => Res.GetString("59F27697-0A88-4E16-8C0E-DF4A8BCCBADE", "Enter a password.");
		public static string PasswordLengthMustBeBetween8and15 => Res.GetString("7BD802AD-1AB7-47DC-A257-903A7F861682", "Password must be between 8 and 15 characters.");
		public static string PasswordFormat => Res.GetString("02FC8047-69BA-4C6D-A2FC-9280DC389813", "Only letters and numbers can be used for the password.");
		public static string AccountMustHaveAtleastOneNode => Res.GetString("A084B507-4C92-418E-BD53-3F3FC16EF5CE", "An account must have at least one node.");
		public static string CertificateOrPasswordInvalid => Res.GetString("43801F14-6DC7-45C8-8335-CF6C0912BA37", "The Certificate or accompanying password is invalid.");
		public static string PasswordRequiredForCertificate => Res.GetString("93C0E47E-5B47-4DD1-899D-A0348F8EB293", "A Certificate Password is required if a Certificate is loaded.");
		public static string AccountNodeMandatoryAndLengthMustBe4 => Res.GetString("2664D012-6A3B-44D7-B6C2-8D5CD8F68C5A", "Node field must be filled with the customs user code consisting of 4 characters.");
		public static string AccountRangeInvalidFormat => Res.GetString("B9650C6F-7FBE-49FC-94CB-B2D0A4EA7D43", "Account Range must be in the format [0-9][A-Z][a-z]");
		public static string AccountRangeStartMustBeLessThanRangeEnd => Res.GetString("FD22A6C0-52D6-4E9E-A8C1-C507377777BF", "Account Range Start must be less than Account Range End");
		public static string AccountRangeEndMustBeGreaterThanRangeStart => Res.GetString("1E3D8A36-2199-4B5A-BD2F-8DBDCBF273B6", "Account Range End must be greater than Account Range Start");
		public static string AccountMustHaveAtLeastOneExciseNumber => Res.GetString("8BE7D056-1217-4AFE-B83F-7FA02D635422", "An account must have at least one excise number.");
		public static string AccountMustHaveAtLeastOneAccountDetail => Res.GetString("12E4616D-5EC8-4C5B-A8B1-BBF018119657", "At least one row of Account Details must be filled");
		public static string AccountDetailInternalCodeMustBeUnique => Res.GetString("FC3D2F69-2D86-4980-8567-337B92078188", "Internal Code must be unique");
		public static string AccountDetailInternalCodeAlreadyExistsInAnotherCompany(ZString companyName) => Res.GetString("B9D4EFBD-678C-4B4D-8ED6-70B103E16503", "This Account Internal Code already exists in company {0}, it is not possible to have duplicated Account Internal Codes", companyName);
		public static string AccountDetailAuthorizedUserCodeFormat => Res.GetString("48427FDB-C5D0-45DC-BEC7-21CC9940E022", "The value should be in the form X-NNN, EG: 11111111111-001");
		public static string AccountDetailDeclarantCodeAndAuthorizedUserMustBeUnique => Res.GetString("B8F47C95-1D57-474E-8E07-FFF141E2F284", "A row with the same Declarant and Authorized User already exists for this Account");
	}

	#endregion

	#region Internal Code

	public static class InternalCode
	{
		public static string InternalCodeFormat => Res.GetString("74223728-55FC-467A-BFFC-B00CC8580F09", "Allow only chars [A-Z][0-9][-_]");
	}

	#endregion

	#region ExciseNumber

	public static class ExciseNumber
	{
		public static string ExciseNumberInvalidFormat => Res.GetString("591F86DC-C8E0-4BB9-BFAD-D4B9CB56F172", "Excise Number is in an invalid format.");
		public static string ExciseNumberMustBeUniqueInRegistry => Res.GetString("E00BC555-FDC7-49F1-ADED-C8250E90A5A7", "Excise Number must be unique in the registry.");
	}

	#endregion

	#region InvoiceHeader

	public static class InvoiceHeader
	{
		public static string AgreedPlaceCodeCannotBe1 => Res.GetString("AD45C7B2-F429-498C-924A-BD9D5A6D8B2E", "For the chosen INCO term Agreed Place Code cannot be 1, only 2 and 3 are allowed");
		public static string InvoiceFreightChargesRequired => Res.GetString("E9C26306-CACC-4568-BA01-AD6D45D73B4A", "Invoice Freight Charges are required for the used INCO Term Place Code. Add needed charges to the invoice or the invoice line");
		public static string CannotHaveDifferentSuppliers => Res.GetString("18C2C01E-FE8D-463A-8DC8-8AE1A89FE66D", "Supplier Country (Country Code) must be the same as Declaration [15] Origin.");
		public static string TransportChargesMethodPaymentMustBeEmpty => Res.GetString("D1282E69-81D2-42B8-8F02-C66EEB73B874", "If Safety and Security is not selected, the Transport Charges Method of Payment must be empty.");
		public static string IncotermPlaceCodeCannotBeIt => Res.GetString("0D57E86D-591D-44CF-BADA-4DAB7138942C", "For the chosen INCO Term, INCO Term Place Code cannot be IT.");
		public static string AllInvoicesMustHaveTheSameTransactionNature => Res.GetString("52261130-9D2C-4889-889F-056013CF0B61", "[E1301] During the transition period, which is active now, all invoices linked to the same entry must have the same [24] Transaction Nature.");
	}

	#endregion

	#region JobComInvoiceLine

	public static class InvoiceLine
	{
		public static string TheSelectedSupplementaryQuantityUOMIsNotInTheList(IEnumerable<ZString> unitOfMeasureForTariffList) => Res.GetString("90BE0440-481B-4ACB-AD2C-4ADA5F875229", "The selected supplementary quantity unit of measure is different from those available for the selected tariff: {0}", string.Join(", ", unitOfMeasureForTariffList));
		public static string TheSelectedTariffRequiresSupplementaryQuantityUOM => Res.GetString("A677EFAA-363B-4A7F-B317-DA7E6D7C2F33", "The selected tariff requires a supplementary quantity unit of measure");
		public static string TheSelectedTariffHasNoSupplementaryQuantityUOM => Res.GetString("1EF6A92E-74E6-481D-9F40-815DCE422471", "The selected tariff has no supplementary quantity unit of measure");
		public static string YouHaveEnteredSupplementaryQuantityWithoutUOM => Res.GetString("7CD882D0-FB3E-47BB-A4FF-7098E4720857", "You have entered a Supplementary Quantity without a Unit of Measure");
		public static string SteelTypeIsNormallyFrom1To4 => Res.GetString("77fc876e-4e41-4761-aa76-1a0c420bc7e2", "Steel Type is normally 1-4.");
		public static string SteelTypeMustBeNumeric => Res.GetString("9ae2e1e3-0a5e-437e-b9ce-67af50cc3328", "Steel Type must be numeric.");
		public static string InvalidSteelType => Res.GetString("24fed80b-9ac4-4516-b996-f3a8d10a5ecb", "Invalid Steel Type.");
		public static string YouHaveSelectedMoreThanOnePackageType => Res.GetString("0B40F385-5B6C-421E-B75B-61285EFDAC5D", "You have selected more than one package type. Only one will be sent to Customs.");
		public static string YouHaveSelectedMoreThan99PackageType => Res.GetString("D1924EF0-98F6-48C3-B298-B991DB58FEF8", "You have selected more than 99 different package types in all the invoice lines linked to the entry line.");
		public static string CpcInvalidAgainstRelatedEntryInstructionExpectedStartingWith(ZString procedureCode) => Res.GetString("BD510C86-D5CF-4B3A-8400-08307F7DE48C", "CPC invalid against related entry instruction. Expected starting with '{0}'.", procedureCode);
		public static string TemporaryProcedureLimitDateIsRequiredWhenCpcStartsWith2Or5 => Res.GetString("E5DC9A10-9C1D-415B-86A2-38A37D432852", "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");
		public static string ThirdQuantityUOMIsRequire => Res.GetString("B31A8F28-2AA6-4EB5-8B46-6B405FBBC9F5", "Insert a Unit of Measure for third quantity");
		public static string TheSelectedThirdQuantityUOMIsNotInTheList(ZString suggestedUOM) => Res.GetString("381FC18A-19B0-4B1A-8D73-DDD77809CED9", "The selected third quantity unit of measure is different from those available for the selected tariff: {0}", suggestedUOM);
		public static string TheSelectedTariffRequiresThirdQuantity => Res.GetString("CC1C3C7A-3C0F-4FA1-A2CA-0D7A363AD69C", "The selected tariff requires a third quantity");
		public static string TheSelectedTariffHasNoThirdQuantity => Res.GetString("3A42CD12-7E71-4C36-B98C-10F9DD9BB14D", "The selected tariff has no third quantity");
		public static string TotalLengthOfRemarksExceedsTheMaximumAllowedLengthInTheMessage(ZInt maximumLength) => Res.GetString("C08EB403-62F0-4659-A4B7-5E80CBCD0465", "Total length of remarks (including exceeding marks and number of packages) for the entry line related to this invoice line exceeds the maximum allowed length in the message ({0} characters), data will be truncated.", maximumLength);
		public static string CustomsQtyIsUsuallyEqualNetWeight => Res.GetString("DB9D63A7-1BCB-40B0-80C0-FC76616BF25C", "Customs Quantity is usually equal to Net Weight");
		public static string GetVatRateDescription(ZDecimal vatRate) => Res.GetString("B697520C-2066-41B4-97C0-27C8F23625BE", "VAT Rate {0}", vatRate.ToString(FormattableString.Invariant($"P{((ZDecimal)(vatRate * 100)).DecimalPlaces}")));
		public static string GetPreferenceRequiresSupportingDocumentC100orU166Caption(ZString preferenceCode) => Res.GetString("CBA97750-DBB4-426B-B52F-6144663EF3B6", "Preference {0} requires a Supporting Document of type 'C100' or 'U166'. Consider adding the REX code to the Supplier>Config>Registration Numbers in order to add automatically C100 document.", preferenceCode);
		public static string SupportingDocumentC100andU166CannotBeUsedTogheterCaption => Res.GetString("5DFF8115-8039-4E4F-8A4F-C0C13B924E6C", "U166 and C100 cannot be used together: use U166 if the supplier is not a registered one and the total value of the imported goods is less than {0}€", UniversalReferenceConstants.CustomsValueInEuroTresholdForOriginDeclaration);
		public static string GetPreferenceRequiresSupportingDocumentC164orC165WhenItemPriceIsLessThanThresholdCaption(ZString preferenceCode) => Res.GetString("A91EF562-DCF4-42AB-9D02-1A4480F9FB19", "Preference {0} requires a Supporting Document of type U164 when Customs value is less than {1}€.", preferenceCode, UniversalReferenceConstants.CustomsValueInEuroTresholdForOriginDeclaration);
		public static string GetPreferenceRequiresSupportingDocumentC164orC165WhenItemPriceIsGreaterOrEqualThanThresholdCaption(ZString preferenceCode) => Res.GetString("4A06B1C6-F07D-454D-90C9-DB1F57C17571", "Preference {0} requires a Supporting Document of type U165 when Customs value exceeds {1}€.", preferenceCode, UniversalReferenceConstants.CustomsValueInEuroTresholdForOriginDeclaration);
		public static string GetInvoiceLineRequiresOnlyOneCertificateCaption => Res.GetString("C0875745-1DC7-4C5A-985B-6F0EAE36D948", "One and only one Certificate of Origin is required: use document U164 when Customs value is less than 6000€, U165 otherwise.");
		public static string ProvinceMandatory => Res.GetString("48DE7440-7950-45DC-8F0F-BC99728BC701", "You must enter a province.");
		public static string GetCustomsDecisionsIntoWarehouseMissingDocumentCaption(ZString documentType) => Res.GetString("1CBBB7FE-D9A8-4043-AFB5-6683CBAB6602", "For Into Warehouse procedure add document of type {0}", documentType.IsEmpty ? (ZString)(NoResString)"C517, C518 or C519" : documentType);
		public static string GetCustomsDecisionsInwardProcessingMissingDocumentCaption(ZString documentType) => Res.GetString("E849111E-E614-482D-B94A-A3427E3F8ACE", "For Inward Processing procedure add document of type {0}", documentType.IsEmpty ? (ZString)"C601" : documentType);
		public static string GetCustomsDecisionsOutwardProcessingMissingDocumentCaption(ZString documentType) => Res.GetString("6EBE119C-1AD7-4407-B303-D8DA61A77AFC", "For Outward Processing procedure add document of type {0}", documentType.IsEmpty ? (ZString)"C019" : documentType);
		public static string InvoiceMustHaveOnlyOneParticipantType => Res.GetString("7C14F40D-4A97-4628-B00B-D4132BB0FE31", "It is not possible to link Invoice Lines of the same Invoice to Entry Instructions with different Participants");
		public static string ProvinceMustBeEmptyWhenCountryOfDispatchIsNotItaly => Res.GetString("AEA25DFC-8473-4ADC-BBE1-96DAAD90BAC7", "Province must be empty when [15] Country of Dispatch is not Italy");
		public static string GetSupportingDocumentMustPresentForProcedureCodeCaption(ZString supportingDocument) => Res.GetString("BA254494-FF45-400E-8438-B5AFBC96B62D", "A Supporting document with code '{0}' must be present for the Procedure Code inserted", supportingDocument);
		public static string ProcedureMustBeEnteredForAdditionalProceduresSelection => Res.GetString("C9053F7B-5C74-4EF6-B32A-F5A291CB9028", "To select additional procedure codes the procedure/CPC must be filled in.");
		public static string GrossWeightMustBeGreaterThanZeroR02221 => Res.GetString("DBE40414-683C-41CA-A6CE-418195E63C02", "[R0221] Gross Weight must be greater than zero");
		public static string GrossWeightMustBeZeroWhenPackIsZeroR0222 => Res.GetString("E5C51302-CDAC-4EED-9B66-006D2CD7B657", "[R0222] Gross Weight must be zero when Pack Quantity is zero");
		public static string TotalGrossWeightMustBeGreaterThanCustomsQty => Res.GetString("12A16A65-E054-416B-B599-442241C5EB71", "[R0224] Total Gross Weight for this entry must be greater or equal than the total Customs Quantity");
		public static string MaximumTwoAdditionalCodesAreAllowedDuringTransitionPeriod => Res.GetString("F212A4E4-BCA5-44AD-A01C-DD65223DA1F6", "[E1404] During the transition period, which is active now, only 2 additional codes are allowed");
		public static string MaximumOneUndgIsAllowedDuringTransitionPeriod => Res.GetString("A750BC26-177B-439B-8015-0D1ADB0E2308", "[E1406] During the transition period, which is active now, only 1 Dangerous Goods code is allowed");
		public static string GrossWeightInKgCannotHaveMoreThan3Decimals => Res.GetString("26655837-520A-4190-B0BD-41FC83124E08", "[E1109] During the transition period, which is active now, [35] GWT converted in KG cannot have more than 3 decimals");
		public static string Ucc6ExportMergedLineMissingPreviousDocuments => Res.GetString("4EDFE12A-D697-44F5-ADCA-58C156D7B256", "This invoice line's merged entry line has no previous documents. Without a previous document the entry may be rejected. Add one to any of the entry line's invoice line or to its entry instruction");
		public static string GetSupportingDocumentMustPresentWithType39YYAndReferenceCaption(ZString expectedRefNumber) => Res.GetString("859DDB2D-EC0D-426F-B07C-5E96A27561AE", "A Supporting Document of Type 39YY and Reference {0} is required for Port Taxes.", expectedRefNumber);
		public static string Document60YYRequiredBasedOnDeclarationTypeAndProcedureCode => Res.GetString("90F46060-386D-11EF-99F4-77F6947630B8", "For this Declaration Type and Procedure code, at least one Supporting document of the Entry Line, linked to this Invoice Line, must be of type 60YY");
		public static string GetPortTaxRateCannotBeDeterminedCaption(ZString barrierPort) => Res.GetString("A621E5C8-16A1-4DB6-A0AB-24A63199C605", "A Port Tax Rate cannot be determined for the Port you entered ({0}).", barrierPort);
		public static string YouHaveNotEnteredProductCodeInventoryCannotBeUpdated => Res.GetString("8C7A8ACD-FFB3-4107-B392-60099FE5657A", "You have not entered a Product Code, inventory cannot be updated.");

		public static string GetPreference400MissingForTurkeyCustomsDutyExemptionCaption(ZString tariff)
		{
			return Res.GetString("F55CFE48-B8F1-45E5-95F2-6C52CE962098",
				"There is no applicable Duty rate for the Tariff '{0}' and Country Of Dispatch 'TR' in combination with other data entered on the form.\r\nValid Duty rates exist for Preference = '400'",
				tariff);
		}

		public static string PreferenceCannotBe400WithoutN018 => Res.GetString("7DEEB762-D694-4D3E-BD51-57E815972DD5", "Preference cannot be '400' if Goods Origin is not 'TR' and there is no Supporting Document with code 'N018'");
		public static string TheTotalNumberOfPreviousDocumentsForThisInvoiceLineExceedsTheMaximumAllowed(int entryLineNumber, int maximumTotalDistinctPreviousDocuments) => Res.GetString("008C59A2-7F4E-446F-902F-CF56D5320B2F", "The total number of previous documents in the Entry Line No. {0} which this Invoice Line is linked  exceeds the maximum allowed of {1}.", entryLineNumber, maximumTotalDistinctPreviousDocuments);
	}

	#endregion

	#region GlbExternalPassword

	public static class GlbBrokerExternalPassword
	{
		public static string NodeMustHaveValue => Res.GetString("8CC382F0-6DCE-4E6C-B6AB-6B78F0397EB2", "Node must have a value.");
		public static string TheNodeMustBelongToValidAccountInRegistry => Res.GetString("286DE82D-C66A-49CD-8A22-3376583C6B87", "The node must belong to a valid account in the registry.");
		public static string NodeMustBeUnique => Res.GetString("64F9310E-BBCB-49CE-A9C3-616F91C80E1E", "Node must be unique.");
		public static string WhenCertificateIsLoadedPasswordIsMandatory => Res.GetString("FB078565-2FA8-4B22-BEA2-7648A086F9FB", "When a Certificate is loaded, Certificate Password is mandatory");
		public static string PleaseEnterCertificateOrXadesCertificate => Res.GetString("4053DDA2-2FBB-4986-8822-D8DF97ACA859", "Please enter a Certificate or a XADES Certificate.");
	}

	public static class GlbMauExternalPassword
	{
		public static string InternalCodeMustBeUnique => Res.GetString("F250780D-C54C-4E7A-BF8A-BE39EE658441", "Internal Code must be unique for this Company.");
		public static string InternalCodeMustAtLeastFiveCharsLength => Res.GetString("8763D14F-E39D-4097-9E2A-9CF26340DE12", "Internal Code must be at least 5 chars length.");
		public static string AuthorizedUserFormatMustBeValid => Res.GetString("CD71EC4F-0F3C-484F-A53D-0170EA5F7C92", "Authorized User should be a valid VAT or fiscal code.");
	}

	public static class CryptokiExternalPassword
	{
		public static string YouMustSpecifyChipset => Res.GetString("80657705-0846-4CC9-B799-8F6FE00D34F4", "You must specify chipset.");
		public static string OnlyOneXadesCertificateIsAllowed => Res.GetString("8C455FD4-0166-465D-B6C4-82C42C796554", "Only one XADES certificate is allowed.");
		public static string PleaseEnterCertificateAuthority => Res.GetString("11EFAD2B-9098-47B5-84EE-C358F1454B53", "Please enter a Certificate Authority or click 'Clear Certificate'.");
		public static string PleaseEnterChipset => Res.GetString("CB38DD2F-7889-4789-BB33-DE581EDCBFC9", "Please enter a Chipset or click 'Clear Certificate'.");
		public static string PleaseEnterSerialNumber => Res.GetString("9F1F9863-560F-4A52-A6D6-6765C41DAEA8", "Please enter a Serial Number or click 'Clear Certificate'.");
	}

	public static class AutomaticSignatureExternalPassword
	{
		public static string OnlyOneAutomaticSignatureEntryIsAllowed => Res.GetString("2621F4C7-0F70-4D3F-A744-F161FA1358E2", "Only one Automatic Signature entry is allowed.");
	}

	#endregion

	#region ITCustomsNumberViewStmNumsWrapper

	public static class ITCustomsNumberViewStmNumsWrapper
	{
		public static string YearOfApplicabilityShouldNotBeEarlierThanCurrentYear => Res.GetString("{8471D679-AF34-48BD-B550-56F9904217D1}", "Year of applicability should not be earlier than current year.");
	}

	#endregion

	#region JobDeclaration

	public static class JobDeclaration
	{
		public static string YouHaveNotEnteredAValueForGoodsLocationAddress => Res.GetString("2B97AE96-CD34-47A5-972C-C48699298843", "You have not entered a value");
		public static string GoodsLocationAddressLongerThan70 => Res.GetString("8F41175D-90D4-4A5E-B8C3-62449C64100E", "Goods Location Address is longer than 70 characters, it will be truncated in the message");
		public static string GoodsLocationAddressPostcodeLongerThan9 => Res.GetString("CB5519BA-1EF3-4F6A-9C6A-DA9A6AAF915C", "Goods Location Address Postcode is longer than 9 characters, it will be truncated in the message");
		public static string GoodsLocationAddressCityLongerThan35 => Res.GetString("912D1FD7-C9E4-4BAD-975C-8AF963B990AA", "Goods Location Address City is longer than 35 characters, it will be truncated in the message");
		public static string GoodsLocationCustomsOfficeDiffersFromOfficeOfPresentation => Res.GetString("5F8D12F7-FF69-4708-8708-809EEC0BB889", "Goods Location Customs Office is different from Customs Office of Presentation");
		public static string CustomsOfficeShouldBe8AlphanumericCharacters => Res.GetString("388b6ed7-4c01-4cae-b831-2e297904250e", "Customs Office should be 8 alphanumeric characters.");
		public static string AtLeastOneContainerRecordMustBeEntered => Res.GetString("8D56109A-0515-46CD-A999-8B4B01984959", "Container mode contains a containerized type, at least one Container record must be entered.");
		public static string NoCoherenceBetweenRepresentativeTypeAndDeclarant => Res.GetString("84FC1EC9-3C48-4322-A29C-79791428D435", "There is no coherence between Representative Type and selected Declarant");
		public static string YouHaveNoEnteredDestination => Res.GetString("EC7A3073-3878-4F7F-B310-5D8A32AC9C3D", "You have not entered a Destination.");
		public static string DestinationDoesNotHaveProvince => Res.GetString("9EB37BE6-80BE-4246-8610-04E59ED32B26", "Destination doesn't have a province (State).");
		public static string VesselPlusVoyageExceedsCustomsMaxLengthExceesCaption(int customsMaxLength) => Res.GetString("9685C519-BF23-41FA-8F5B-94D46C6610C5", "Fields (Vessel + Voyage) exceeds the maximum allowed length in the declaration message ({0} characters).", customsMaxLength);
		public static string AuthorisationIsRequiredForEntryInstructionsAtPlace => Res.GetString("8693EECD-F045-4371-BAC6-B5D6116C02B6", "Authorization is required for Entry Instructions of type 'COL'.");
		public static string AuthorisationMustBeEmptyForEntryInstructionsAtCustoms => Res.GetString("38A69BC8-913D-4045-B8F2-668EEAF51CB8", "Authorization must be empty for Entry Instructions of type 'COD'.");
		public static string ProvidePortCodeToDeterminePortTaxRate => Res.GetString("5F81B620-55DF-400C-B5A0-C542AC64FD12", "Provide a port code to determine a port tax rate");
		public static string CannotHaveDifferentSuppliers => Res.GetString("63F3C365-C6FB-4F03-A32F-C47A7C81ABBE", "[15] Declaration Origin must be the same as Invoice Supplier Country");
		public static string LocationQualifierMustBeEmpty => Res.GetString("320019C7-2D84-45B4-9B50-3409F218401E", "Goods Location qualifier must be empty if Authorization is filled");
		public static string LocationQualifierRequired => Res.GetString("9AC68852-77C4-4566-A9BA-3535A4DAEE98", "You have not entered a Goods Location qualifier");
		public static string GetSelectedAuthorizationRequiresLocationQualifierOfTypeCaption(string locationQualifier) => Res.GetString("DD290688-F710-4D2E-9801-AE720FE2A2E2", "The selected Authorization requires a Location qualifier of type {0}", locationQualifier);
		public static string DeclarationRequiresTransitOffice => Res.GetString("7411CEC2-739A-4CAB-9844-91A711C92462", "The declaration requires an office of type Office of Transit with Purpose {0}.", EuOfficeCodesTypes.Codes.OfficeOfTransit);
		public static string AllSuppliersMustHaveValidAEOofTypeAEOForAEOS => Res.GetString("2AA0CC3E-0695-418E-A68D-356201F1104B", "For Circumstance 'E' the Declarant and all Suppliers must have a valid AEO of type AEOF or AEOS. Consider adding the appropriate AEO in Organization>Config>Registration numbers/codes");
		public static string CircumstanceAndTransportAreInCongruent => Res.GetString("46964ADA-1C07-4259-9EFC-9F358F86C639", "Circumstance and [25] Transport are in-congruent");
		public static string NeedItalianRegistrationNumberForSubcriber => Res.GetString("DEFA4B6A-8BB9-47B1-9460-B8E7570CF946", "An Italian Registration Number is not set for this subscriber. Go to Maintain -> User Admin -> Staff and resources -> Human Resources -> Certificates, ID and training. Add a Certificate with Type = COD and Certificate Number = Italian Registration Number");
		public static string HowToConfiguraStaffItalianRegistrationNumber => Res.GetString("C7973C8D-0758-4698-BCC7-B3E3FAC84323", "Go to Maintain -> User Admin -> Staff and resources -> Human Resources -> Certificates, ID and training");
		public static string EnterNationalityOfMeansOfTransportAtDeparture => Res.GetString("979C31F9-E86F-4D18-833C-181314AB8F66", "You must enter a nationality of means of transport at departure.");
		public static string GetEoriCodeIsRequiredCaption(string fieldName) => Res.GetString("BC96778A-35D3-498E-BE67-70DA35DB7905", "EORI code for {0} is mandatory: please press F3, go to details -> config and fill a 'Registration Number / code' with Type ='EOR'", fieldName);
		public static string DeclarantAuthorizationMissingForParty1 => GetAuthorizationMissingForPartyCaption("1", Res.GetString("31318386-E4E7-4D13-B049-FB8D4DAE7372", "Declarant"));
		public static string ImporterAuthorizationMissingForParty2 => GetAuthorizationMissingForPartyCaption("2", Res.GetString("929952A1-A07E-4F45-8E5D-B046D23FC3DF", "Importer"));
		public static string ForwarderAuthorizationMissingForParty3 => GetAuthorizationMissingForPartyCaption("3", Res.GetString("AD103AAE-2313-48FB-BFAB-0C8D15F79391", "Forwarder"));
		public static string RepresentativeAuthorizationMissingForParty4 => GetAuthorizationMissingForPartyCaption("4", Res.GetString("967519DF-FF0D-4884-ABBF-11C733D8A737", "Representative"));
		public static string SupplierAuthorizationMissingForParty4 => GetAuthorizationMissingForPartyCaption("4", Res.GetString("C9890454-9737-41F6-AB2A-CA516B9A21BD", "Supplier"));
		public static string ExporterAuthorizationMissingForParty5 => GetAuthorizationMissingForPartyCaption("5", Res.GetString("B6CF11D7-1303-45F6-A067-4B6EDFFC1069", "Exporter"));
		public static string DefermentAccountNumberMustBeEmptyIfMethodOfPaymentIsNotDEorG => Res.GetString("9DEA05BB-368F-4AF3-8262-3874E5E36004", "This field must be empty if no 'Method of Payment' is [D], [E] or [G]");
		public static string YouHaveNotEnteredAValueForInlandTransportCode => Res.GetString("8C532D9D-E0E0-433B-A791-18C36A1D05E1", "You have not entered a [18] Code.");
		static string GetAuthorizationMissingForPartyCaption(string paymentParty, string ownerType) => Res.GetString("3F7BDC30-C0F0-402D-87D8-8DCA5D4E5FDD", "For Payment Party = {0} an Entry Instruction > Authorization must be present, with Code = DPO and Owner = {1}", paymentParty, ownerType);
		public static string YouHaveNotEnteredLocationOfGoodsC0392 => Res.GetString("4E749CD3-97D3-49F8-84C8-C9B4DE349C54", "You have not entered a Location of Goods (C0392)");
		public static string RepresentativeMustBeDifferentFromDeclarant => Res.GetString("3CA03C18-9C35-453A-A743-5E9A927DE24A", "Representative must be different from Declarant");
		public static string DeclarationRequiresPresentationOfficeForCentralizedClearanceWithPurposePRE_R0675 => Res.GetString("CA0B2CEA-DAEF-46E6-9A31-2CA99E674916", "[R0675] The declaration requires an office of type 'Office of Presentation for Centralized Clearance' with purpose PRE");
		public static string OnlyOneMeansOfTransportMustBeFilledInTransitionPeriodB1884 => Res.GetString("62AE1F7E-DF7C-4242-9EE9-869BB753F893", "[B1884] During the transitory period, which is active now, only one means of transport must be filled in");
		public static string OnlyOneMeansOfTransportMustBeFilledR0855 => Res.GetString("60058EC2-115C-4561-8792-45B3DB69A48B", "[R0855] Only one means of transport must be filled in");
		public static string FieldMustBeFilledB2101 => Res.GetString("1100FA1B-0316-4A8D-A652-89E9ED2D5C36", "[B2101] This field must be filled");
		public static string LowercaseLettersNotAllowedR0473 => Res.GetString("67DED530-F025-46B7-A513-19B963170D9E", "[R0473] Lowercase letters are not allowed");
		public static string AtLeastOneMeansOfTransportMustBeFilledC0834 => Res.GetString("34E2D677-6FD5-4BBD-90E1-26AEB65088A6", "[C0834] At least one means of transport must be filled in");
		public static string MeansOfTransportMustBeFilledC0834 => Res.GetString("6E954E48-C179-4DE7-A0B1-E8C26A3AAC4D", "[C0834] Means of transport must be filled in");
		public static string ActiveBorderGroupFieldsMustAllBeFilledOrMustAllBeEmpty => Res.GetString("abea0a62-1cba-4fd5-9a9e-0fb69c3918fc", "Fields of group [21] must be all filled or all empty");
		public static string DeclarantHasNoEORIorTCUcode => Res.GetString("D5565EB0-893F-4689-AE43-A4F128BDC46E", "Declarant has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers/Codes.");
		public static string RepresentativeHasNoEORI => Res.GetString("CCA3A5C9-9E63-4346-833B-E2B8BE997ED3", "Representative has no EORI code. Please consider adding the 'EOR' code in Organization > Config > Registration Numbers/Codes.");
		public static string TheSelectedMethodOfPaymentInTaxOrFeeRequiresToEnterApprovalDeferNoCN0558 => Res.GetString("E93509D6-8BAE-4620-9CC9-E49E69DC2236", "[CN0558] {0}", TheSelectedMethodOfPaymentInTaxOrFeeRequiresToEnterApprovalDeferNo);
		public static string TheSelectedMethodOfPaymentInTaxOrFeeRequiresToEnterApprovalDeferNo => Res.GetString("807284B8-918C-41F8-A4D6-8CEDBCAC79F8", "The selected method of payment in Entries > Entry Lines > Tax or Fee requires to enter an Approval Defer No.");
		public static string ItineraryCountriesRequired => Res.GetString("856AA14B-FA53-42BA-A58A-52CFEE24C764", "[C0211] Please insert Itinerary Countries in Misc tab");
	}

	#endregion

	#region NctsHeader

	public static class NctsHeader
	{
		public static string ForTheSelectedRepTypeThisFieldIsMandatory => Res.GetString("{66FE10CC-489F-422C-85F8-9E7CA3804427}", "For the selected Rep. Type this field is mandatory.");
		public static string CountryOfDispatchIsequired => Res.GetString("4DEBA5A8-75F2-461D-8C11-5C3034091D71", "Country of dispatch is required");
		public static string YouHaveNotEnteredADispatchCountry => Res.GetString("77BB65E3-D639-4473-8AA9-BBD44BEAF7EF", "You have not entered a Dispatch Country");
		public static string CountryDeclarationDifferentDispatch => Res.GetString("CCA1492C-FF0F-40D1-A11F-E59B07BB62F0", "Dispatch Country in the Declaration is different from Dispatch Country in Goods Items");
		public static string ConsignorAndConsigneeAtHeaderLevelMustBeEmpty => Res.GetString("06A9A288-F271-4273-B95A-4F408536ADCC", "To declare Consignor and Consignee at Goods Items level (Participants = GRP), Consignor and Consignee must be empty in Departure Declaration Header TAB");
		public static string CountryOfRoutingRuleB1848 => Res.GetString("8112E6C7-54F6-45A0-8FAD-B26C348B2AAD", "[B1848] In transition period, which is now, no Country/Region of Routing rows must be entered.");
	}

	#endregion

	#region NctsBill

	public static class NctsBill
	{
		public static string AllDepartureTransportMeansAreSame => Res.GetString("DAF938C8-7DFC-48DD-B21E-34A20D5244A4", "This section contains the same data for all the House consignments and will be written at Header level");
	}

	#endregion

	#region NctsHeaderContainer

	public static class NctsHeaderContainer
	{
		public static string NoGoodsItemIsLinkedToThisContainer => Res.GetString("4163DD61-A246-4A94-8572-7BE46DF24EB3", "No Goods Item is linked to this container.");
	}

	#endregion

	#region NctsDepartureMovementHeader

	public static class NctsDepartureMovementHeader
	{
		public static string NotEnteredDestinationCountry => Res.GetString("CDC8D626-92A2-409E-9ACE-41CE651F96B9", "You have not entered a Destinations Country.");
		public static string NotEnteredGoodsLocationCode => Res.GetString("B065B72C-4AF2-439F-B31F-9842C85257A1", "You have not entered a [30] Goods Location.");
		public static string DestinationCountryDeclarationDifferentGoodItems => Res.GetString("0D2A4741-C3F6-4294-A288-13C309AFDFDA", "Destinations Country in the Declaration is different from Destinations Country in Goods Items.");
		public static string Missing50Representative => Res.GetString("4233D739-D892-460D-805B-A45B948C7595", "You have not entered [50] Representative");
		public static string ApprovalDeferNoMustBeFilled => Res.GetString("C24CF7B6-247D-4646-B8DC-356345C2EDD9", "C558: If field 'Goods Items>Tax or Fee>Method of Payment' is equal to E, F or G, this field must be filled");
		public static string ApprovalDeferNoMustBeEmpty => Res.GetString("9FA6F0E2-4B07-4262-AFFC-53B2733349E2", "C558: If field 'Goods Items>Tax or Fee>Method of Payment' is NOT equal to E, F or G, this field must be empty");
		public static string DateLimitCannotBeInThePast => Res.GetString("CEE8C91A-E2D1-4705-8D1B-08D19DBFAEBA", "Date Limit cannot be in the past.");
		public static string DateLimitIsLessThan8DaysFromNow => Res.GetString("E6534240-A283-4D51-91F9-A8BFDCFEF1C2", "Date Limit is less than 8 days from now.");
		public static string CountryOrUNLOCOForPlaceOfLoading => Res.GetString("B317DA32-019E-4743-9644-2BC33918F7DC", "Country/Region Code or an UNLOCO for Place of Loading.");
		public static string DateOfPresentationCanNotBeInPast => Res.GetString("92123639-540D-4C84-B437-456DDF6FF233", "Date of presentation can not be in the past.");
		public static string HeaderLevelWillBeIgnoredHousesHaveSameValue => Res.GetString("12EA9A96-562A-45E1-B180-156C8303A064", "The Transport MoP declared in the header will be ignored because all Houses have the same value, which differs from the value declared in the header.");
		public static string HeaderLevelWillBeIgnoredHousesHaveDifferentValue => Res.GetString("9EAE1106-85B5-49E7-BBC3-98006C5E0AEA", "The Transport MoP declared in the header will be ignored because all Houses have values different from the header one.");
	}

	#endregion

	#region NctsCargoDesc

	public static class NctsCargoDesc
	{
		public static string NetWeightIsRequired => Res.GetString("BCB0BBEF-4308-48A9-87C0-65D5093EFA17", "For a declaration that unloads a temporary storage (A3) or an Into Warehouse (7), Net weight is mandatory.");
		public static string CommodityCodeIsRequired => Res.GetString("3515EE84-82B0-4B13-AF3E-6651BDEB91A1", "For a declaration that unloads a temporary storage (A3) or an Into Warehouse (7), Commodity code is mandatory.");
		public static string AllOrNoneDispatchCountry => Res.GetString("26D8FD00-5EE8-4476-B3E1-50EE30ED7D3D", "All or none Goods Items Dispatch Country must be filled.");
		public static string AllOrNoneDestinationCountry => Res.GetString("80D08A04-6630-48D7-AF24-1C19992A06C1", "All or none Goods Items Destinations Country must be filled.");
		public static string FieldMustBeEmptyIfCountryOfDispatchIsIt => Res.GetString("9C0BD02D-83CB-4B2A-B50C-2526573AC831", "The field must be empty if [15] Country of Dispatch is not IT (CN34)");
		public static string MustHaveMoreThanOneGoodsItemToDeclareTradersAtGoodsItemLevel => Res.GetString("861A0DDD-09D9-4142-9422-F2825811B14B", "To declare Consignor and Consignee at Goods Items level (Participants = GRP), you must have more than one Goods Item");
		public static string MustHaveDifferentOrganizationsAtGoodsItemLevel => Res.GetString("6ABAE12D-841E-4162-A69F-69FC2A7", "To declare Consignor and Consignee at Goods Items level (Participants = GRP), they have to be different organizations, not the same in every item");
		public static string ConsignorIsEmptyAndParticipantTypeShouldBeGroupage => Res.GetString("133F56E2-FB8F-49EC-9484-7A0D8D7D3297", "Participants is STD and the Consignor is empty in Departure Declaration Header TAB, even if there are Consignors entered at a goods item level. Change the participants to GRP if you want to declare more than one Consignor or enter Consignor at header level and remove it from goods items for STD participants");
		public static string ConsigneeIsEmptyAndParticipantTypeShouldBeGroupage => Res.GetString("8928AFAD-E661-415E-995F-62D1369FE002", "Participants is STD and the Consignee is empty in Departure Declaration Header TAB, even if there are Consignees entered at a goods item level. Change the participants to GRP if you want to declare more than one Consignee or enter Consignee at header level and remove it from goods items for STD participants");
	}

	#endregion

	#region NctsPackage

	public static class NctsPackage
	{
		public static string MarksAndNumbersIsRequired => Res.GetString("B2A2A5AF-28EC-4C90-85AC-B6069FB351F3", "For the selected unit type, Marks and Numbers is mandatory.");
		public static string UnitCountMustBeEmpty => Res.GetString("E572E966-C8B8-4BB9-8FCE-45BC9756BD63", "For the selected unit type, Unit count must be zero.");
		public static string GoodsItemMustHaveAtLeastOnePackage => Res.GetString("8CAF890E-9FC7-47B4-8502-E4C7CFD16073", "A goods item must have at least one package.");
		public static string OnlyOnePackageIsAllowedInEt => Res.GetString("B4745DB0-F635-40B4-B972-37AEB927A05A", "Only 1 line of Packages is allowed in the ET message");
	}

	#endregion

	#region Package

	public static class Package
	{
		public static string QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame => Res.GetString("5238AAF2-EC03-4E22-BD87-CF8368E47F48", "Quantity can be zero only if 'Type' and 'Marks and Numbers' are the same as the previous Entry line.");
		public static string QuantityCanBeZeroOnlyWhenOtherEntryLineHasSamePackTypeMarksAndQtyDifferentFromZero => Res.GetString("877C96AF-267D-4EEC-97AF-2D01404419D5", "Quantity can be zero for this kind of 'Pack Type' only if another 'Entry Line' is entered with the same Pack Type and Marks and a quantity different from zero");
		public static string PackQtyCannotBeZeroForTheSelectedPackType => Res.GetString("F2DE061E-CB7B-4CF3-AF31-102EAA42532A", "Pack Qty cannot be zero for the selected Pack Type.");
		public static string PackQtyMustBeZeroForTheSelectedPackType => Res.GetString("ACECE428-DE62-4D67-B5E2-2D8A8DD6A5ED", "Pack Qty must be zero for the selected Pack Type.");
		public static string NoOtherPackageHavePackageNumberGreaterThanZero => Res.GetString("8FA5E8EC-CA11-4FE9-9602-C6C012B507D1", "[R0219] If in an Entry Line there is a Packaging Line with Package Number equal to zero, no other Packaging Lines in the same Entry Line may have Package Number greater than zero.");
		public static string QuantityCanBeZeroOnlyWhenOtherLineWithSameMarksAndBulkPackType => Res.GetString("EB4CECD6-9E28-41EA-9CCC-9939A2594837", "[R0364] Quantity of packages can be zero only if there is an 'Entry Line' containing a packaging line with the same Marks and with Quantity greater than zero, not in type Bulk or Break-bulk.");
	}

	#endregion

	#region NctsGuarantee
	public static class NctsGuarantee
	{
		public static string IsRequiredForSelectedType => Res.GetString("4FCF9B2B-0CBD-49FC-948E-6548127D49AD", "This field must be set for Type<> ('0','1','2','4','9').");
		public static string ShouldBeEmptyForApplicableType => Res.GetString("2E09EBEF-E10A-4171-AE92-D36CC0600172", "This field should be empty for Type = ('0','1','2','4','9').");
		public static string ShouldBeEmptyForNonApplicableType => Res.GetString("AEFD79B3-015E-4BBC-AE5D-D4374B91C625", "This field should be empty for Type <> ('0','1','2','4','9').");
		public static string YouNeedToSupplyValidGuarantee => Res.GetString("B8D7AA00-53BC-4251-8C3B-CD6FB015DBA1", "You need to supply a valid Guarantee.");
	}
	#endregion

	#region NCTS Previous Document

	public static class NctsPreviousDocument
	{
		public static string MoreThanOnePaAndOneOrMoreRpDocuments => Res.GetString("2FE40138-D091-42A9-BC9B-4FD86FF42F6F", "You have entered more than one PA and one or more RP documents, only the first PA will be included in the NB message! To close additional PA documents, please create one goods item per each PA related to one or more RP.");
		public static string PreviousDocQtyDoesNotMatchGoodsItemQty => Res.GetString("27AB5454-B234-47FE-AF50-AA00EC1264C5", "The quantities of the previous documents do not match with the quantities of the Goods Item. Verify that the Goods Item and previous documents have the correct quantities.");
		public static string InvalidDateOfIssue => Res.GetString("5981789c-d3e9-4251-9e2d-ba472d13a090", "Enter a valid date of issue");
		public static string RpWithLineNoAndTariffEmpty => Res.GetString("E93AC4DA-950F-4E88-8344-3E927C5EAB97", "Both Item No and Tariff are empty, that means you are considering to close the whole previous document");
	}

	#endregion

	#region SupportingDocument

	public static class SupportingDocument
	{
		public static string Document10YYShouldNotBeUsed => Res.GetString("7B6BB63C-DE9A-4B32-8033-24A9CB090E1E", "Instead of document 10YY use field Third Quantity, 10YY will be added automatically in the Message to Customs");

		public static string GetMissingAEOCertificateMessage(ZString organisation) => Res.GetString("CF11AD64-8998-4325-B1F1-4D534856E3E4", "This document references the AEO number of the {0}, but its Organization has no AEO code. Please consider adding the AEO in Organization > Config > Registration Numbers/Codes.", organisation);

		public static string GetUnmatchedAEOCertificateMessage(ZString organisation) => Res.GetString("2ADE2251-CD87-41BC-963F-324E36CFCFC5", "This document Reference does not match the AEO Registration Number of the {0}.", organisation);

		public static string DocumentReferenceIsDifferentToSupplierREX(ZString supplierRex) => Res.GetString("0678949C-CE77-45FC-98BC-70166EE6022F", "This document Reference is different from the REX code of the Supplier ({0})", supplierRex);

		public static string SupplierHasNoRexNumber => Res.GetString("2D3FC367-7F81-4B38-8A45-61EECF1107C6", "Cannot validate this document Reference because the Supplier has no REX code. Consider adding the REX code to the Supplier>Config>Registration Numbers");

		public static string DocumentU165WasUsedButU164WouldBeBetter => Res.GetString("268C9E58-0800-4C15-ADC4-B8DD00838647", "Document U165 was used but U164 would be better: please consider changing the document type.");

		public static string DocumentU164WasUsedButU165WouldBeBetter => Res.GetString("08DA30F7-05C3-4CA6-8E00-6DAD6A69C2DA", "Document U164 was used but U165 would be better: please consider changing the document type.");

		public static string DontUseTheseDocumentsWhenPrefernceDoesNotStartWith2 => Res.GetString("B8DB4E61-7992-445F-9332-EE081FAEB52E", "Consider removing this document or checking for the used preference");

		public static string DeclarationOfIntentNumberFormatNotValid => Res.GetString("004B0824-6266-43D0-B63A-6FF77519B3E1", "Document number format is not valid.");

		public static string ThereShouldBeOnlyOneDistinct01DIDocumentPerEntry => Res.GetString("FB74631E-C671-44D2-A40D-894434903181", "There should be only one distinct 01DI document per Entry.");

		public static string GetHasNoAcrCertificate(ZString docType) => Res.GetString("40151235-0103-42DB-8924-7D4351FCE5C0", "For Transit Authorized Consignor add document type [Authorization> Rule Code [DOC]> {0}]", docType);

		public static string IndicativeDocExceedsCustomsMaxLengthCaption => Res.GetString("F8AFDEB7-155F-4805-A979-D6B0A8BA0D89", "Reference + Year of Issue + Country Code exceeds the maximum allowed length in the declaration message.");

		public static string IfQuantityIsPresentThenUnitOfQuantityMustBeFilled => Res.GetString("4EABDF85-FC7F-4171-9EF9-78D51C191366", "Rule C0298: If Quantity is present then Unit of Quantity must be filled");

		public static string IfQuantityIsEmptyThenAlsoUnitOfQuantityMustBeEmpty => Res.GetString("102D9AE1-FB1F-42D9-8B23-66A2090D5002", "Rule C0298: If Quantity is empty then also Unit of Quantity must be empty");

		public static string DocumentY04XShouldNotBeUsedAtDeclarationOrInvoiceLevel => Res.GetString("58652305-A6C9-44B6-B31F-05A3CE77BD56", "When at least one Entry Instruction with Procedure Code is '42' or '63' and Declaration Type is 'H1' or 'H5', the document types 'Y040', 'Y041' and 'Y042' must not be used");
		public static string DocumentY04XShouldNotBeUsedAtInvoiceLineLevel => Res.GetString("51DC1AEC-AAA4-47BE-84A2-3CEE261577AD", "When related Entry Instruction with Procedure Code is '42' or '63' and Declaration Type is 'H1' or 'H5', the document types 'Y040', 'Y041' and 'Y042' must not be used");
		public static string PortCodeFormatForType39YY => Res.GetString("280B7DAF-5E92-4A1C-806B-B9C53BC6AC48", "Port Code for document 39YY must start with \"--\" (e.g. \"--ITTRS\")");
	}

	#endregion

	#region Previous Document

	public static class PreviousDocument
	{
		public static string ReferenceNumberEnteredIsInvalidFormat => Res.GetString("126C1C25-0C46-4DC8-8B1F-C0BB3EF4E876", "Number entered is in an invalid format.");
		public static string CheckDigitIsIncorrectErrorPrefix => Res.GetString("9D71FD80-FFC0-4E4F-AFEA-792C3626C1D6", "The check digit entered is incorrect.");
		public static string CheckDigitIsIncorrect(ZString actualCheckDigit, ZString expectedCheckDigit) => CheckDigitIsIncorrectErrorPrefix + " " + Res.GetString("6CC9B15F-64B9-4AA2-9800-B72D399A1A00", "Current value is: \'{0}\', expected \'{1}\'.", actualCheckDigit, expectedCheckDigit);
		public static string ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerJob => Res.GetString("8696AE87-9300-4B17-A301-04D8541D0A3F", "For Reimport procedures you can have just 1 PA document per job. Remove additional documents.");
		public static string ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerInvoice => Res.GetString("F320EB97-863B-44DA-AD6E-A961F551AF67", "For Reimport procedures you can have just 1 PA document per invoice. Remove additional documents.");
		public static string ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine => Res.GetString("D0946110-6149-4398-8A7B-7B5994F13F00", "For Reimport procedures you can have just 1 PA document per invoice line. Remove additional documents.");
		public static string ForReimportProceduresYouCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine => Res.GetString("F593FF72-B347-43A2-8649-8D8A7660230A", "For Reimport procedures you can have just 1 PA document at job/invoice/invoice line level at the same time. Remove one or more of them.");
		public static string TheQuantitiesOfDocumentsDoNotMatchWithQuantitiesOfEntryLine => Res.GetString("93A8866B-E9FE-4341-83E6-74DE770F661F", "The quantities of the previous documents do not match with the quantities of the entry line. Verify that the entry line and previous documents have the correct quantities.");
		public static string GetQuantityCouldBeRoundedMessage(ZString uomDescription, ZInt truncatedDecimals, ZDecimal originalValue, ZString valueInKg) => Res.GetString("15559C2C-D80C-493F-99CF-F265A62AE30B", "Be aware that quantity in {0} exceeding {1} decimal could be rounded in Customs declaration message. For instance {2} {0} will be: {3} Kilograms.", uomDescription, truncatedDecimals, originalValue, valueInKg);
		public static string GetQuantityCouldBeRoundedMessageForUCC6Import(ZString uomDescription, ZDecimal originalValue, ZString valueInKg) => Res.GetString("460A493F-42AC-4581-8E82-A9A3362CA802", "Be aware that quantity in Kilograms exceeding 6 decimals could be rounded in Customs declaration message. For instance {1} {0} will be: {2} Kilograms.", uomDescription, originalValue, valueInKg);
		public static string ApportionmentIsPendingPleaseGenerateEntriesAndSaveOrValidateAll => Res.GetString("FC17E0DE-8814-43B2-8FF0-5A2ABCA143DB", "Previous Documents apportionment is pending. Selecting Brokerage > Generate Entries (Merge) will run this feature. Then Save or select File > Validate All to refresh the validation.");
		public static string GrossMassMustBeGreaterThanOrEqualToNetMass => Res.GetString("AE9C7FD5-854C-43CA-A41B-EEF859C105E1", "Gross Mass must be greater than or equal to Net Mass.");
		public static string MoreThanOnePaAndOneOrMoreRpDocuments => Res.GetString("5B52751C-FEB7-42AF-B4B8-E0281A47ED1B", "You have entered more than one PA and one or more RP documents, only the first PA will be included in the NB message! To close additional PA documents, please create one goods item per each PA related to one or more RP.");
		public static string ForRpDocumentsLineNoAndTariffAreEmpty => Res.GetString("BDA9529D-3C2B-48F9-8777-4B8584E41EBD", "If Line No.and Tariff are empty, the whole previous declaration will be considered by Customs.");
		public static string NumberExceedsTheMaximumLengthInTheMessage => Res.GetString("23A325DB-8B95-4BFA-87D3-2258ECD50B1C", "The number exceeds the maximum length in the message.");
		public static string PreviousDocumentsQuantityAllowedNumberOfDigits(int maximumAllowedNumberOfDigits) => Res.GetString("EFCE70E4-990D-4035-B91D-5F8211AF5A9A", "The maximum allowed number of digits is {0}.", maximumAllowedNumberOfDigits);
		public static string CustomsOfficeCodeIsNotValid => Res.GetString("41E6A651-0E79-4672-8D1D-92114A185629", "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the fourth part must contain a valid Italian Customs Office");
		public static string ReferenceNumber2LengthMustBe18 => Res.GetString("7FFE8344-2F28-4061-B086-FC5FB7035CB6", "The MRN field must be entered with 18 characters.");
	}

	#endregion

	#region TraderJobDocAddressValidation

	public static class TraderJobDocAddressValidation
	{
		public static string EoriCodeOrFiscalCodeIsRequired => Res.GetString("7BDE0B70-DF3E-4F0B-A891-001BBAFD50D3", "EORI Code or Fiscal Code is required for the EU Natural Person. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter one.");
		public static string EoriCodeOrVatCodeIsRequired => Res.GetString("8227CA2F-A05B-4B47-939F-B281B0549531", "EORI Code or VAT (IVA) Code is required for the EU Organization. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter one.");
		public static string VatCodeOrFiscalCodeIsRequired => Res.GetString("10B1A2CA-5F51-4B7A-8F93-FCBB17DC287C", "VAT (IVA) Code or Fiscal Code (Italian Registration Number) is required for the EU Organization. Please press F3, go to Detail -> Config -> Registration Numbers/Codes and enter a valid one.");
	}

	#endregion

	#region UCC6TraderJobDocAddressValidation

	public static class UCC6TraderJobDocAddressValidation
	{
		public static string EoriCodeForImporterIsRequired => JobDeclaration.GetEoriCodeIsRequiredCaption(Res.GetString("036760D3-5C96-413F-BA4E-2469FDE118C8", "Importer"));
		public static string SupplierOrConsignorsMustBeFilled => Res.GetString("85AF1838-503E-4015-958A-A8132E5D25FF", "Declaration -> Supplier or Invoice Line -> Organizations -> Consignor must be filled");
		public static string SupplierAtDeclarationLevelWillBeIgnored => Res.GetString("D43A5B7A-537D-4564-BAD4-B66C752360D5", "The Supplier declared in the header will be ignored because all Invoice Line -> Consignor have values different from the Supplier");
		public static string GetEoriOrTCUCodesForTraderAreRequiredCaption(string traderName) => Res.GetString("DD276104-B481-4EE2-B035-DB22585920BE", "{0} has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers/Codes", traderName);
	}

	#endregion

	#region ImportJobDeclarationValidation

	public static class ImportJobDeclarationValidation
	{
		public static string EoriCodeForDeclarantIsRequired => JobDeclaration.GetEoriCodeIsRequiredCaption(Res.GetString("7CD6381F-AFE6-4E52-98EE-7267360F14D9", "Declarant"));
		public static string EoriCodeForRepresentativeIsRequired => JobDeclaration.GetEoriCodeIsRequiredCaption(Shared.RepresentativeCaption);
	}

	#endregion

	#region PackageValidation

	public static class PackageValidation
	{
		public static string MarksExceedsMaxLength42 => Res.GetString("1C9D626F-B36E-4BDF-80DE-3B4E67BB1FF1", "Marks field exceeds the maximum allowed length in the declaration message (42 characters).");
	}

	#endregion

	#region MessageSendingObject

	public static class MessageSendingObject
	{
		public static string NonSendableStatusWarning(ZString entryReferenceNumber) => Res.GetString("891BDB52-6B27-47C0-9C74-DF69210A4659", "The Entry {0} was already sent and is already registered or waiting for a message from Customs. Resending this entry could result in a duplicated declaration. ", entryReferenceNumber);
		public static string SelectedDeclarationTypeCanBeSentOnlyInFallback => Res.GetString("DEB2AAD7-009C-45F8-A281-37FB9D1FD82A", "The selected declaration types can be sent only in Fallback");
		public static string SelectedEntryDoesNotHaveAnAssociatedEntryInstruction => Res.GetString("986D179D-5E12-43E4-A4A4-000D0D7D6D25", "You can't select this entry because it has no Entry Instruction associated to it. Please create an Entry Instruction and associate it to the desired Invoice Lines.");
	}

	#endregion

	#region OrgHeader

	public static class OrgHeader
	{
		public static string AddressExceedsCustomsMaxLengthExceesWillBeTruncateCaption(int customsMaxLength) => Res.GetString("EA257B9D-3182-4F18-8E81-6696A402BA84", "Address (Line 1 + Line 2) exceeds the maximum allowed length in the declaration message ({0} characters). Excess characters will be truncated.", customsMaxLength);
		public static string CcpDoesNotHaveValidLocAuthorisation => Res.GetString("36052DC1-BE89-4523-874D-01AD3CCE15B7", "This CCP code does not have an Authorization. Please consider adding this code as LOC in Authorizations for this Organization");
	}

	#endregion

	#region OrgCusCode

	public static class OrgCusCode
	{
		public static string EachDanAndDatMustBeUniqueMessage => Res.GetString("B82122C3-2BCA-40C9-B9BE-7BBD72C858D8", "Each 'DAN' and 'DAT' must have an unique code.");
	}

	#endregion

	#region CusEntryHeader

	public static class CusEntryHeader
	{
		public static string TheMaximumNumberOfEntryLinesHasBeenExceeded(int linesCountLimit) => Res.GetString("123A2FC5-CE28-41D6-A228-2A00EA1482CF", "This Entry has more than {0} Lines, consider adding a new Entry Instruction.", linesCountLimit);
	}

	#endregion

	#region CusEntryLine

	public static class CusEntryLine
	{
		public static string TheMaximumCumulativeNumberOfSupportingTransportAndAdditionalReferenceExceeded => Res.GetString("69DD9517-4D03-430D-82FB-8C781FFC78CA", "[E1407] The maximum cumulative number of Supporting Documents, Transport Document and Additional Reference must not exceed 99.\r\nPlease check, in both Invoice lines and Invoice headers bound to this Entry line, the Codes indicated in Supporting Documents and Additional Documents (of Kind 'TRA' and 'REF')");
	}

	#endregion

	#region CusEntryLineFee

	public static class CusEntryLineFee
	{
		public static string DoNotUse405Code => Res.GetString("E138835A-3145-42AD-9FA3-B8AEE2C7B758", "Do not use the code 405 for VAT. Instead, use the code B00, which is the standard VAT code for the EU.");
		public static string PortTaxCanOnlyBeEnteredOnce => Res.GetString("1C802064-7749-47A9-A296-517DC44F78FC", "Port Tax can only be entered once");
		public static string MopShouldBeRWhenChargeTypeIsA35orA45 => Res.GetString("0E1A160E-53B1-41A1-8240-062305CD4F88", "A35 and A45 are provisional duties and should be guaranteed with Method of Payment R");
		public static string YouHaveNotEnteredApprovalDeferNoForThisMop => Res.GetString("C2C80FEC-1CE1-4BBA-BDDA-0CF1F3DD37F4", "You have not entered an Approval Defer No. for this method of payment");
		public static string DefermentAccountNumberHasBeenSetButNotUsed => Res.GetString("3F3AA4D3-651A-4F0D-86E9-91AAA8129A8D", "Approval Defer No. has been set but not used, consider to change the Method of Payment");
		public static string MethodOfPaymentForExportUCC6DecDutiesWhenApprovalDeferNoIsSetShouldBeEorD => Res.GetString("277AAEA3-1A9D-464F-BA4C-A585F155D13E", "Method of payment for duties, when Approval Defer No. is set, should be E or D");
		public static string MethodOfPaymentForDutiesWhenApprovalDeferNoIsSetShouldBeForT => Res.GetString("A597637B-DE83-4313-96D6-A401E761A122", "Method of Payment for duties when Approval Defer No. is set should be F or T");
		public static string PortTaxShouldNotBeEntered => Res.GetString("28919115-29F9-438F-A344-05A077033B8E", "Port Tax should not be used for the selected transport mode.");
		public static string VatExemptionFeeRequiresDeclarationOfIntentSupportingDocument => Res.GetString("CD633E61-C0C9-419D-9C2C-37ACED8E1477", "Fee '406' requires a '01DI' supporting document.");
		public static string DeclarationOfIntentSupportingDocumentRequiresVatExemptionFee => Res.GetString("95384376-37AA-4644-A6C1-08227BD93714", "A supporting document '01DI' is present and requires a fee of type '406' (VAT exemption).");
		public static string MopShouldBe(ZString mop) => Res.GetString("D9CA06C0-8DAC-46C8-9FDC-3AC72DB3BEA0", "Method of Payment should be {0}.", mop);
		public static string TotalAmountIsDifferentFromSystemCalculatedAmount(ZDecimal systemCalculatedAmount) => Res.GetString("4E770CBA-FC74-417E-80C0-E6F9E45C5176", "The Total Amount is different from what is calculated by the system: {0}.", systemCalculatedAmount);
		public static string TaxRateShouldBeZero(ZString procedureCode) => Res.GetString("31AABC9E-F3D3-4FB6-880C-4F06ACE4B521", "For Procedure {0}, Tax Rate should be = '0' or not present.", procedureCode);
	}

	#endregion

	#region CusFiscalReference

	public static class CusFiscalReference
	{
		public static string ReferenceMustNotStartWithIT(ZString procedureCode) => Res.GetString("D04B9C8F-7A04-4C69-A5BA-25BCC9AC4E88", "For Procedure {0}, Reference field must not start with 'IT'.", procedureCode);
	}

	#endregion

	#region SupplementaryCode

	public static class SupplementaryCode
	{
		public static string OnlyOneVatAdditionalCodeCanBeUsedAtTime => Res.GetString("0C97B08F-E539-4797-8754-FD02A2546F20", "Only one VAT Additional Code can be used at a time");
		public static string SetVatAdditionalCodeIsNotValidForThisTaricCode => Res.GetString("1ADC106A-AD6A-4DD8-A2CF-02C8FC5930C2", "The set VAT additional code is not valid for this Taric code");

		public static string FirstCharacterOfTheSelectedCodeIsNotValid => Res.GetString("26066DC5-CF72-4A0C-8729-7C9B7A73844A", "The first character of the code you have selected is not valid. Please make sure the first character is ‘2, 3, 4, 6, 8, A, B, C, D or P’ for ‘Taric Additional Code’, or ‘Q, R, S, T, U or Z’ for ‘National Additional Code’.");
	}

	#endregion

	#region CusAuthorisations

	public static class CusAuthorisations
	{
		public static string ValueMustStartWithOneOrMoreDigitsAndEndWithOneUpperAlphabeticalCharacter => Res.GetString("C1E10EE6-7089-4750-8938-E4E0C30CEB0A", "The Value must start with one or more digits and end with one upper alphabetical character");

		public static string InvalidApprovedLocationAuthorisationNumber => Res.GetString("7BC0ADB2-CC16-4177-AE29-DB962E5DBB14", "The Authorization Number must start with one or more digits and end with one upper alphabetical character.");

		public static class Header
		{
			public static string DeclarationOfIntentNotValid => Res.GetString("C052B7BF-21EE-499B-A764-7AF8E27DFFF3", "DOI format not valid, enter 'X' in the number as a placeholder if you want to enter the real number afterwards in the declaration.");
		}
	}

	#endregion

	#region ProgressiveAnnualNumber

	public static class ProgressiveAnnualNumber
	{
		public static string CannotRetrievePanNumberRangeForSelectedNode => Res.GetString("3521B94D-0773-4739-9967-973D0FAFDD5C", "System cannot retrieve the PAN number range for the selected node. Please enter a valid range in Company > Number Ranges.");
		public static string NumberRangeHasReachedTheLimit(ZString rangeType, ZString account, ZLong availableNumbers) => Res.GetString("37FC915F-50BB-40F2-BE0F-950B1899B16F", "The available Number Range for range type {0}, account {1} is less than {2}. Create a new Number Range in Companies or expand the existing one.", rangeType, account, availableNumbers);
	}

	#endregion

	#region MessageSending

	public static class MessageSending
	{
		public static string GetFilenameRangeRunOutForTodayDialogErrorMessage(Registry.Account account) => Res.GetString("8B74F942-B7F6-45E1-9010-90B8B11BE9E2", "The filename range for the account {0}:{1} has run out for today, it is not possible to generate a new filename.\r\nPlease check the Range Start and Range End in Registry>Customs>Italy>Account Management>Company for the account or consider using another account if available.", account.AccountNumber, account.AccountNode);
		public static string SendMessageErrorDialogCaption => Res.GetString("80C0ED9E-6C1A-483D-920F-A24AC52815A1", "Error - cannot send message");
		public static string AccountNotFoundDialogErrorMessage => Res.GetString("112F9C02-4F44-4E04-95F1-9E34C8E85D50", "No Account could be found for the selected Node.Please check that the selected Node in Misc.Tab is valid.");
		public static string SubscriberNotFoundDialogErrorMessage => Res.GetString("B546C497-A916-4420-9515-FC08AE6EB2A7", "Please select a Subscriber in Misc. tab");
		public static string SubscriberHasNoFiscalCode => Res.GetString("D028F9DF-21DB-4108-97A8-896AE0268EA1", "The Current User (Subscriber) has no Fiscal Code. Go to Staff and Resources > Human Resources > Certificates, ID and Training and add a 'COD' code in Type field and Fiscal code in Certificate Number.");
	}

	#endregion

	#region AdditionalInfo

	public static class AdditionalInfo
	{
		public static string OnlyOneLineOfAdditionalInfoIsAllowedPerEntryLine => Res.GetString("6D670324-17B6-431A-A894-D329B259780F", "Only 1 line of Additional Info is allowed per Entry Line.");
		public static string OnlyOneLineOfAdditionalInfoIsAllowed => Res.GetString("38D16888-714F-77A5-4271-DF978D55A725", "Only 1 line of Additional Info is allowed.");
		public static string Only99LinesOfAdditionalInfoAreAllowed => Res.GetString("2E850B3C-1250-4A3B-8157-84326ED59BBF", "Only 99 lines of Additional Info are allowed.");
		public static string Only99LinesOfAdditionalInfoAreAllowedForAnEntryLine => Res.GetString("35A5FBF5-C19F-493E-9280-6DDD32A79C60", "Only 99 lines of Additional Info are allowed for an Entry Line.");
		public static string ExportFromECOrExportFromCountryIsRequired => Res.GetString("1A356DDA-88F5-4997-9C78-43B6A9591C68", "'Export from EC' or 'Export from Country' (Box 44) is required (C075)");
		public static string ExportFromECOrExportFromCountryCannotBeUsed => Res.GetString("F39BAF32-4EE5-47BE-A78D-F981343AB566", "'Export from EC' or 'Export from Country' (Box 44) cannot be used (C075)");
		public static string CodeOrDescriptionMustBeFilled => Res.GetString("C0160146-DC09-4EEC-A901-987D7D68FEF6", "Code or Description must be filled.");
		public static string GetFieldCannotBeLongerThanCustomsMaxLengthInTransitionPeriodCaption(string ruleNumber, string fieldLocation, int maxLength) => Res.GetString("AA382C47-0F9C-438F-89BC-4C0AB400AB34", "[{0}] during the transition period, which is active now, {1} cannot be longer than {2} characters.", ruleNumber, fieldLocation, maxLength);
		public static string AddInfoNotAllowedInTransitionPeriodRuleE1301 => Res.GetString("a54d821b-5705-4e6b-9b78-9abe96bb8b03", "[E1301] during the transition period, which is active now, Additional Documents at message header level must not be used");
		public static string GetUcc6ExportCusEntryInstructionMoreThan99DocumentsAreNotAllowedMessage(ZString subStyle) => Res.GetString("3c59925f-d887-4694-b08c-b822c8e00e6b", "You have entered more than 99 Additional Documents with Type {0}", subStyle);
	}

	#endregion

	#region Ucc6ExportInvoiceLineAdditionalInfo

	public static class Ucc6ExportInvoiceLineAdditionalInfo
	{
		public static string DescriptionCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1106 => AdditionalInfo.GetFieldCannotBeLongerThanCustomsMaxLengthInTransitionPeriodCaption("E1106", Res.GetString("70CD237D-4FC1-48EC-8BB4-D9DC6CDCD1FA", "Invoice Line>Additional Documents>Description"), Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceLineAdditionalInfo.Description);
		public static string ReferenceCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1104 => AdditionalInfo.GetFieldCannotBeLongerThanCustomsMaxLengthInTransitionPeriodCaption("E1104", Res.GetString("571E2CFD-E9DF-4A51-8B7F-5FDD80320F64", "Invoice Line>Additional Documents>Reference"), Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceLineAdditionalInfo.ReferenceNumber);
	}

	#endregion

	#region Ucc6ExportInvoiceHeaderAdditionalInfo

	public static class Ucc6ExportInvoiceHeaderAdditionalInfo
	{
		public static string DescriptionCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1106 => AdditionalInfo.GetFieldCannotBeLongerThanCustomsMaxLengthInTransitionPeriodCaption("E1106", Res.GetString("CC7C4ED2-FE08-495E-B624-999455B53E85", "Invoice Header>Additional Documents>Description"), Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceHeaderAdditionalInfo.Description);
		public static string ReferenceCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1104 => AdditionalInfo.GetFieldCannotBeLongerThanCustomsMaxLengthInTransitionPeriodCaption("E1104", Res.GetString("0A123A4B-86CE-4F65-A49C-30BE7DC52A18", "Invoice Header>Additional Documents>Reference"), Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceHeaderAdditionalInfo.ReferenceNumber);
	}

	#endregion

	#region OfficeCode

	public static class OfficeCode
	{
		public static string OfficeOfTransitMustBeAD => Res.GetString("67D3CF0A-ADE1-4795-8B39-D2E8E5D6175F", "When Country of Office of Destination is {0} then Country of Office of Transit must be {0}", Core.Constants.CountryCodes.Andorra);
		public static string OfficeOfTransitMustBePartOfEU => Res.GetString("E2269F0B-1570-4AE2-982F-493F143A611D", "When Country of Office of Destination is {0} then Country of Office of Transit must be an {1} country", Core.Constants.CountryCodes.SanMarino, Core.Constants.CountryCodes.EuropeanUnion);
		public static string GetSelectedOfficeNotInTheListForPurpose(string purpose) => Res.GetString("CF7A802A-ABAA-4D43-B299-B362E83DEA03", "{0}: The value you selected is not in the list.", purpose);
		public static string R0676_CCLCodeMustBePresentInEntryInstructionAuthorizationsType => Res.GetString("973314D1-1190-4176-9502-C94818996DB1", "[R0676] A 'CCL' code must be present in Entry instruction > authorizations > type");
	}

	#endregion

	#region CommercialReferenceNumber

	public static class CommercialReferenceNumber
	{
		public static string CommercialReferenceCannotBeSetHeaderAndGoodsItems => Res.GetString("FC21E587-AA95-4C4B-98B0-ECED59458F2B", "The field Commercial Reference Number cannot be set both at Header and Goods Items level");
		public static string CommercialReferenceCannotBeSetHeaderAndInvoices => Res.GetString("EFE3F797-A8B8-4C7E-B973-B2E7C7A6E1CA", "This field cannot be set in the header and in the details at the same time");
	}

	#endregion

	#region CusAuthorizationUsage

	public static class CusAuthorizationUsage
	{
		public static string EoriCodeIsRequired => Res.GetString("778653DE-950C-4EB7-B86D-7B9DDE8F1817", "EORI Code is required. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter one.");
		public static string YouHaveNotEnteredAnOwner(string rulePrefix) => Res.GetString("F348CAB5-6DB9-4D09-B546-47E68C5D26BA", "{0} You have not entered an Owner", rulePrefix);
		public static string YouHaveNotEnteredAnOwnerC0848 => YouHaveNotEnteredAnOwner("[C0848]");
		public static string YouHaveNotEnteredAnOwnerG0089 => YouHaveNotEnteredAnOwner("[G0089]");
	}

	#endregion

	#region CusSupplyChainActorReference

	public static class CusSupplyChainActorReference
	{
		public static string SelectedOwnerDoesNotContainEORCode => Res.GetString("0A75B75B-7BAC-487D-B4ED-B81B59B2FA7D", "The selected Owner does not contain an EOR code");
	}

	#endregion

	#region EntryManualRelease

	public static class EntryManualReleaseCaptions
	{
		public static string InvalidDate => Res.GetString("35F56810-624A-4827-A076-1C2F7C577608", "Invalid date");
	}

	#endregion

	#region Transport

	public static class TransportCaptions
	{
		public static string ConsecutiveLinesAreEnteredWithSameCountry => Res.GetString("B383144A-DF42-41D7-90DE-FF17237ED81A", "Several consecutive lines are entered with the same country");
		public static string LinesAreEnteredWithSameLegOrder => Res.GetString("24526B00-EF80-4AD8-BD86-4BDF78A7CB7D", "Several lines are entered with the same number");
		public static string LegOrderShouldBeSequential => Res.GetString("C4100DCD-3571-4DB7-8E0F-988A79462D9E", "Numbering is non sequential");
	}

	#endregion

	#region AdditionalProcedureCode

	public static class AdditionalProcedureCode
	{
		public static string DoesNotMatchMainProcedure => Res.GetString("94C3449E-C8A9-4127-A6E9-F930F4314C5F", "The first 4 characters of Additional Procedure code should be same as the main Procedure's code.");
		public static string DuplicatedProcedre => Res.GetString("EACFA5FE-E9C9-407F-B71F-1250D9F16E11", "Additional Procedure code already specified. Additional Procedure codes should not be duplicated.");
		public static string MustBeEmptyInTransitionPeriod => Res.GetString("D8EA8907-8BE6-4145-A5B5-8AC378AE5087", "During the transition period, which is active now, Additional Procedures may not be provided.");
	}

	#endregion

	#region PreviousDocuments

	public static class InvoiceLinePreviousDocuments
	{
		public static string PackageTypeRequiredWhenNumberOfItemsArePresent => Res.GetString("18A03634-6A01-4B62-AC79-F1BC45097A8A", "When Number of Package is provided at Invoice Line's Previous Document, then Package Type is mandatory.");
		public static string PackageTypeRequiredForNonBulkPkgTypeAndForNonZeroQuantity => Res.GetString("8B9BC831-E88F-450A-9C81-DBAF78730D68", "Package Type (non BULK) is required at Invoice Lines only if Number of Package is provided.");
		public static string UnitOfMeasureRequiredOnlyIfQuantityIsProvided => Res.GetString("8CA3A35B-4B03-446B-A925-A501BBC2BD48", "Previous Document's Measurement Unit is required at Invoice Lines only if quantity is provided.");
		public static string UnitOfQuantityIsRequireForNonZeroQuantity => Res.GetString("C08C1921-C2A1-4A3D-99DE-C6D34A227415", "When Quantity is provided at Invoice Line's Previous Document, then Measurement Unit & Qualifier is mandatory.");
		public static string ItemNumberIsRequiredForC651OrC658Codes => Res.GetString("649410BD-14FB-442D-8E9F-51C5173A69C9", "When Previous Document is in either C651 or C658, then Previous Document Item No. is required.");
		public static string ReferenceNumberCanHaveOnly35Characters => Res.GetString("02EC6057-A46C-41B8-A542-3AA288B93243", "Reference Number of Previous Document can have up to 35 alpha numeric characters.");
		public static string Max9DocumentsAreAllowedInTransitionPeriod => Res.GetString("50667ADD-9D01-459E-8B01-18C252631DDC", "During the transition period, which is active now, the maximum number of previous documents allowed is 9.");
		public static string Max99DocumentsAreAllowed => Res.GetString("BA18B70A-991B-4595-A736-FC7046BC9687", "The maximum number of previous documents allowed is 99");
		public static string MrnReferenceNumberMustBeOfLength18 => Res.GetString("D48587DD-E6BF-4221-8847-C4A1B79F91D4", "The MRN Number in Previous Documents Reference field must be 18 characters");
		public static string MrnReferenceNumberMustHaveOnlyAlphanumericCharacters => Res.GetString("EF5F66DC-D618-4F50-8333-F205B559C30F", "Reference field in Previous Documents can only contain the MRN Number for type NMRN. Only Alphanumeric Characters are allowed");
		public static string N337ReferenceNumberHasLessParts => Res.GetString("BA952378-5A2B-4F57-B598-B7A81CF5B639", "{0}. You didn't enter all the fields required", N377ReferenceNumberMessageErrorCommonPart);
		public static string N337ReferenceNumberHasMoreParts => Res.GetString("0E262255-3A8B-428E-A856-46936B3653B2", "{0}. You entered more fields than required", N377ReferenceNumberMessageErrorCommonPart);
		public static string N337InvalidProcedureCode(string procedureCodes) => Res.GetString("2E5A8DBE-CA80-4EC4-8C7A-8A41E5E23834", "Reference fields must start with a valid Procedure (accepted codes are {0})", procedureCodes);
		public static string N337ReferenceNumberSecondPartMustBeOf8Digits => Res.GetString("01FD3412-CE53-4FB8-9075-D2F94672AD6F", "{0}, the second part must contain a number of registration between 1 to 8 digits", N377ReferenceNumberMessageErrorCommonPart);
		public static string N337ReferenceNumberSecondPartMustNotContainWhiteSpaces => Res.GetString("FC96FDC0-F2E5-462C-A09A-0D7620D78642", "{0}, spaces are not admitted in between", N377ReferenceNumberMessageErrorCommonPart);
		public static string N337ReferenceNumberSecondPartMustContainAllNumbers => Res.GetString("BBA9CFB9-B36D-4939-8CA7-9671E35D0F95", "{0}, the second part must contain a number of registration", N377ReferenceNumberMessageErrorCommonPart);
		public static string N337ReferenceNumberYearOfIssuingMustBeSpecified => Res.GetString("989EE733-C12C-48F0-8B1C-D64E6B55DEBE", "{0}, the third part must contain the year of issuing", N377ReferenceNumberMessageErrorCommonPart);
		public static string N337ReferenceNumberItalianCustomsOfficeRequired => Res.GetString("28CD8D33-FB1E-46ED-BFCA-70E72AC39C84", "{0}, the fourth part must contain a valid Italian Customs Office", N377ReferenceNumberMessageErrorCommonPart);

		static string N377ReferenceNumberMessageErrorCommonPart => Res.GetString("F06CE151-7206-4EEB-9534-E67E0CB9CC6B", "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office without country)");
	}

	#endregion

	#region TemporaryStorageBill

	public static class TemporaryStorageBill
	{
		public static string GoodsDescriptionOrTariffMustBeThere => Res.GetString("360DDD7F-B5B5-4503-AFB7-D5EACF527CA2", "Enter a Goods Description or a valid Tariff in at least one Item.");

		public static string PacksLineMustBeThere => Res.GetString("0B803EF4-F36B-11EF-8934-5600051F1387", "No Packing Lines against Bill - Please enter some Packing Lines for the Bill");

		public static string DuplicatedBills => Res.GetString("0129EE0A-29FC-4AD1-9780-B73D1B8C5DC0", "Bill Number should be unique.");
	}

	public static class TemporaryStoragePackedItem
	{
		public static string PreviousDocumentsRequired => Res.GetString("38771AED-8FFF-48BB-A8E0-9777A1416D9E", "In Previous Documents, you have to enter at least one row.");
		public static string TariffCodeOrGoodsDescriptionRequired => Res.GetString("5A1C2D4E-7F8B-4A9E-ABCD-12345EF67890", "Enter the Tariff Code or enter the Description of Goods in the Bill Details tab.");
		public static string HaveToEnterAtleastOneRow => Res.GetString("682E1792-76F0-4489-8FAA-FFDF676215E1", "In Supporting Documents, you have to enter at least one row.");
	}

	#endregion

	#region TemporaryStorageHeader

	public static class TemporaryStorageHeader
	{
		public static string PlaceIDMustBeNumeric => Res.GetString("E0A7210A-EECE-43C0-A10D-A63FD8408EE4", "Only numeric characters are allowed.");
	}

	#endregion

	#region TemporaryStoragePack

	public static class TemporaryStoragePack
	{
		public static string MarksAndNumbersExceedMaxLength(int maxLength) => Res.GetString("2EF9B87E-7D8D-49C6-A9A7-10C3DDE43057", "You have entered more than {0} digits in the field Marks", maxLength);

		public static string PackQtyExceedMaxLength => Res.GetString("9AB54D26-BFF3-4CE0-B09B-10725013A0AA", "You have entered more than 8 digits in the field Quantity");
	}

	#endregion

	#region TemporaryStorageAdditionalInfo

	public static class TemporaryStorageAdditionalInfo
	{
		public static string TypeOrDescriptionRequired => Res.GetString("3701097E-3746-407C-9447-3A009F27734D", "You have not entered a Type or a Description");
	}

	public static class TemporaryStoragePreviousDocument
	{
		public static string ShouldHaveSameType => Res.GetString("fb969e0b-124a-45fd-8c38-b81b8d0e91e5", "All lines should have the same Type for Previous Document.");
		public static string ShouldHaveSameReferenceNumberMessage => Res.GetString("fb969e0b-124a-45fd-8c38-b81b7d0e91e5", "All lines should have the same Reference Number for Previous Document.");
		public static string ShouldHaveDiffLineNumber => Res.GetString("fb872e0b-124a-45fd-8c38-b81b7d0e91e5", "All lines should have a different Goods Item Identifier for Previous Document.");
	}

	#endregion

	#region InventorySelectionLine

	public static class InventorySelectionLine
	{
		public static string CustomsEntryKeyIsPendingForResponse => Res.GetString("D88AF8F3-282B-4633-AE38-421074D2D1F0", "The Customs Entry Key is pending for a response from the customs system, it cannot be selected.");
		public static string CustomsEntryKeyIsNotValid => Res.GetString("7E6D7F39-460A-4DF4-8067-564FA97CF3E8", "The Customs Entry Key is not an MRN and does not start with '7', it cannot be selected.");
	}

	#endregion

	#region TemporaryStorage

	public static class ArrivalTransportMeans
	{
		public static string YouHaveNotEnteredAnArrivalTransportMeans => Res.GetString("A6762D90-9BE4-4341-B3C0-076B6186E90D", "You have not entered an Arrival Transport Means.");
		public static string YouHaveNotEnteredAnIMONumber => Res.GetString("C9514231-5A44-43B4-AE18-D0B70DD0043B", "You have not entered an IMO Number.");
		public static string YouHaveNotEnteredAVesselName => Res.GetString("F0CA909A-6E2C-4DDE-B01E-1DD62197A1EE", "You have not entered a Vessel Name.");
		public static string YouHaveNotEnteredAWagonNumber => Res.GetString("E9ECE5E8-D00C-40F1-A8BF-9E58DB19A98E", "You have not entered a Wagon Number.");
		public static string YouHaveNotEnteredARoadVehicleRegNo => Res.GetString("EF53EF12-6780-4A6E-92A9-E5171593DEC5", "You have not entered a Road Vehicle Reg. No.");
		public static string YouHaveNotEnteredAnIATAFlightNumber => Res.GetString("700CF427-0BA7-456D-A278-06DC415A1BC8", "You have not entered an IATA Flight Number.");
		public static string YouHaveNotEnteredAnAircraftRegNo => Res.GetString("91B291DC-E181-467D-995D-5560500298D1", "You have not entered an Aircraft Reg. No.");
		public static string YouHaveNotEnteredAnENICode => Res.GetString("71CCC207-15ED-4AA0-B463-F0FA20DA99C5", "You have not entered an ENI Code.");
	}

	public static class TemporaryStorageContainer
	{
		public static string NoSealAvailableOrBulkGoods => Res.GetString("46570EE3-C570-4B1C-B3A2-181065A5C93C", "No seal available or bulk goods. In the message will be written 0 (zero)");
	}

	#endregion
}
