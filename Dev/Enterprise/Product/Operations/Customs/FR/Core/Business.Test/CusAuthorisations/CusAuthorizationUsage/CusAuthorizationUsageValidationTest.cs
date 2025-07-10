using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	class CusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAGC_CodeWhenAuthorizationIsAdHoc()
		{
			var message = "You can't request an Ad Hoc Authorization for authorization types other than IPO, EUS, OPO, or TEA.";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTTST";
			var header1 = Factory.New<CusAuthorisationHeader>();
			header1.CPH_OH_PermitHolder = orgHeader.PK;
			header1.CPH_Number = "110001";
			header1.CPH_Type = "TST";
			header1.CPH_IsAdHoc = true;
			var header2 = Factory.New<CusAuthorisationHeader>();
			header2.CPH_OH_PermitHolder = orgHeader.PK;
			header2.CPH_Number = "110002";
			header2.CPH_Type = "TST";
			header2.CPH_IsAdHoc = false;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_CPH_Authorization = header1.PK;

			CombineAssertions("When the selected Authorization is Ad Hoc, then AGC_Code must be one of the following: IPO, EUS, OPO, or TEA.", () =>
			{
				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertHasMessageError(cusAuthorizationUsage.AGC_CodeInfo, message);

				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;
				AssertNoMessageError(cusAuthorizationUsage.AGC_CodeInfo, message);

				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				AssertNoMessageError(cusAuthorizationUsage.AGC_CodeInfo, message);

				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
				AssertNoMessageError(cusAuthorizationUsage.AGC_CodeInfo, message);

				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
				AssertNoMessageError(cusAuthorizationUsage.AGC_CodeInfo, message);

				cusAuthorizationUsage.AGC_CPH_Authorization = header2.PK;
				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertNoMessageError(cusAuthorizationUsage.AGC_CodeInfo, message);
			});
		}

		public void TestCheckAGC_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "40", "71", "F61", "", "EXP", "40P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;

			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "40", "71", "F62", "", "EXP", "40P");
			procedure2.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure2.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "4071F61";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EndUse;
			AssertHasMessageError(authorizationUsage.AGC_CodeInfo, "It is legally forbidden to use Indirect representation for End-Use procedure, you should consider not using EUS authorization or changing Representation Type on Misc Tab.");

			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			AssertNoMessageError(authorizationUsage.AGC_CodeInfo, "It is legally forbidden to use Indirect representation for End-Use procedure, you should consider not using EUS authorization or changing Representation Type on Misc Tab.");

			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			AssertHasMessageError(authorizationUsage.AGC_CodeInfo, "It is legally forbidden to use Indirect representation for private customs warehouses, you should consider using a public customs warehouse (authorization codes CW1 and CW2) or changing Representation Type on Misc Tab.");

			invoiceLine.JI_Procedure = "4071F62";
			authorizationUsage.Validation.ValidateAGC_Code();
			AssertNoMessageError(authorizationUsage.AGC_CodeInfo, "It is legally forbidden to use Indirect representation for private customs warehouses, you should consider using a public customs warehouse (authorization codes CW1 and CW2) or changing Representation Type on Misc Tab.");

			invoiceLine.JI_Procedure = "4071F61";
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			AssertNoMessageError(authorizationUsage.AGC_CodeInfo, "It is legally forbidden to use Indirect representation for private customs warehouses, you should consider using a public customs warehouse (authorization codes CW1 and CW2) or changing Representation Type on Misc Tab.");
		}

		public void TestCheckAGC_CPH_Authorization_Rules()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var instruction = Factory.New<Declaration.CusEntryInstruction>();
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;

			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authorisationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authorisationHeader.CPH_OH_PermitHolder = importer.PK;
			authorisationHeader.CPH_Number = "TST_ATH_001";
			authorisationHeader.CPH_StartDate = new ZDate(2023, 08, 07);
			authorisationHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authorisationHeader.CPH_IsActive = true;

			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = importer.PK;

			authorisationHeader.Validation.ValidateCPH_Number();
			usage.Validation.ValidateAGC_CPH_Authorization();

			var authorisationsMessageErrors = authorisationHeader.CPH_NumberInfo.Notifications.Where(x => x.Message.Contains("You are required to have at least")).Select(x => x.Message);
			var usageMessageErrors = usage.AGC_CPH_AuthorizationInfo.Notifications.Where(x => x.Message.Contains("You are required to have at least")).Select(x => x.Message);
			AssertContainsExactElementsInAnyOrder("All authorisations message errors related to rules should also show on usage.", usageMessageErrors.Select(x => ((ZString)x).SubstringSafe(x.IndexOf(":") + 2)), authorisationsMessageErrors);
		}

		public void TestCheckAGC_OH_Owner()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount1 = Factory.New<OrgCusAccount>();
			orgCusAccount1.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount1.CZ_Account = "TESTACC";
			orgCusAccount1.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount1.CZ_OH = importer.PK;
			orgCusAccount1.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			var export = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount2 = Factory.New<OrgCusAccount>();
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount2.CZ_Account = "TESTACC";
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_OH = export.PK;
			orgCusAccount2.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Exporter = export.PK;
			declaration.JE_CustomsProfile = "TESTACC";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

			var propertyInfo = authorizationUsage.AGC_OH_OwnerInfo;
			AssertNoMessageError(propertyInfo, "The owner should be either Consignor or Declarant.");

			authorizationUsage.AGC_OH_Owner = declaration.Declarant.Header.PK;
			AssertNoMessageError(propertyInfo, "The owner should be either Consignor or Declarant.");

			authorizationUsage.AGC_OH_Owner = testHeader.PK;
			AssertHasMessageError(propertyInfo, "The owner should be either Consignor or Declarant.");

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			AssertNoMessageError(propertyInfo, "The owner should be either Consignee or Declarant.");

			authorizationUsage.AGC_OH_Owner = declaration.ActualClient.PK;
			AssertNoMessageError(propertyInfo, "The owner should be either Consignee or Declarant.");

			authorizationUsage.AGC_OH_Owner = testHeader.PK;
			AssertHasMessageError(propertyInfo, "The owner should be either Consignee or Declarant.");
		}

		public void TestCheckAGC_CPH_Authorization()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_TriggeringPointForValidation = "VAQ";
			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;

			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Number = "123";
			authorizationHeader.CPH_Type = "abc";
			authorizationHeader.CPH_IsAdHoc = true;
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			authorizationHeader.CPH_IsActive = false;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_CPH_Authorization = authorizationHeader.PK;

			AssertNoMessageError("Inactive authorizations should not be used for pre BAE statuses", authorizationUsage.AGC_CPH_AuthorizationInfo, "The authorization you chose is inactive, please choose another one");

			cusEntryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES085;
			authorizationUsage.Validation.ValidateAGC_CPH_Authorization();
			AssertHasMessageError("Inactive authorizations should not be used for pre BAE statuses", authorizationUsage.AGC_CPH_AuthorizationInfo, "The authorization you chose is inactive, please choose another one");
		}

		public void TestAGC_CPH_AuthorizationValidationWhenAuthorizationHasBlueMessage()
		{
			var authorisationWithError = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationWithError.CPH_Type = UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge;
			var ruleOfauthorisationWithError = authorisationWithError.CusAuthorisationRules.AddNew();
			ruleOfauthorisationWithError.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.STO;
			ruleOfauthorisationWithError.CPR_ValueFrom = "101";
			AssertHasMessageErrorContaining("prerequisite :", ruleOfauthorisationWithError.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotLessThan100(CusAuthorisationRuleTypeList.Codes.STO));

			var authorisationWithOutError = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			var ruleOfauthorisationWithOutError = authorisationWithOutError.CusAuthorisationRules.AddNew();
			ruleOfauthorisationWithOutError.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.STO;
			ruleOfauthorisationWithOutError.CPR_ValueFrom = "51";
			AssertNoMessageErrorContaining("prerequisite :", ruleOfauthorisationWithOutError.CPR_ValueFromInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotLessThan100(CusAuthorisationRuleTypeList.Codes.STO));

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_CPH_Authorization = authorisationWithError.PK;
			AssertHasMessageErrorContaining("There is an error in authorisation header => error in authorisation usage", authorizationUsage.AGC_CPH_AuthorizationInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotLessThan100(CusAuthorisationRuleTypeList.Codes.STO));

			authorizationUsage.AGC_CPH_Authorization = authorisationWithOutError.PK;
			AssertNoMessageErrorContaining("There is no error in authorisation header => no error in authorisation usage", authorizationUsage.AGC_CPH_AuthorizationInfo, CusAuthorisationRuleRequirementHelper.ErrorMessageIfNotLessThan100(CusAuthorisationRuleTypeList.Codes.STO));
		}

		public void TestUseEffectiveReferenceNumberValidation()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsage>();
			var validation = new CusAuthorizationUsageValidationForTest(cusAuthorizationUsage);
			AssertEquals("EffectiveReferenceNumber is not in use for GUI", false, validation.UseEffectiveReferenceNumberValidationExposed);
		}

		class CusAuthorizationUsageValidationForTest : CusAuthorizationUsageValidation
		{
			public CusAuthorizationUsageValidationForTest(CusAuthorizationUsage parent) : base(parent)
			{
			}

			public bool UseEffectiveReferenceNumberValidationExposed => UseEffectiveReferenceNumberValidation;
		}
	}
}
