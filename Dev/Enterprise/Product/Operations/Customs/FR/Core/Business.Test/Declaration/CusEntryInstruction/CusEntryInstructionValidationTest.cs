using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class CusEntryInstructionValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckRuleR0933_N03()
		{
			var message = "R0933_N03 A Simplified authorization must be selected (SDE).";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			entryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.I1;
			AssertHasMessageError("CEI_Style is I1, a Simplified authorization must be selected", entryInstruction.CEI_StyleInfo, message);

			cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			entryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.I1;
			AssertNoMessageError(entryInstruction.CEI_StyleInfo, message);
		}

		public void TestCheckRuleC0810_N01()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = "A";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = "1122F15";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "3344F15";

			var message = "[C0810_N01] Location Of Goods must be empty when all invoice lines Customs procedure end with F15 (Trading With Special Fiscal Territories).";

			entryInstruction.GoodsLocation.CGL_Qualifier = "V";
			entryInstruction.Validation.ValidateAll();
			AssertHasMessageError("Rule C0810_N01 should apply when all instruction invoice lines have procedure ending with F15 and instruction location of goods is not empty.", entryInstruction.GoodsLocationDescriptionInfo, message);

			invoiceLine2.JI_Procedure = "3344000";
			entryInstruction.Validation.ValidateAll();
			AssertNoMessageError("Rule C0810_N01 should not apply when not all instruction invoice lines have procedure ending with F15 even if instruction location of goods is not empty.", entryInstruction.GoodsLocationDescriptionInfo, message);

			invoiceLine2.JI_Procedure = "3344F15";
			entryInstruction.GoodsLocation.CGL_Qualifier = ZString.Empty;
			entryInstruction.Validation.ValidateAll();
			AssertNoMessageError("Rule C0810_N01 should not apply when all instruction invoice lines have procedure ending with F15 but instruction Location of goods is empty. ", entryInstruction.GoodsLocationDescriptionInfo, message);
		}

		public void TestCheckRuleC0834_N02()
		{
			var message = "[C0834_N02] For Temporary Admission under CPC 53 a fiscal reference of any type but FR5 must be served.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using var context = new EntryInstructionValidationDeciderTestContext(entryInstruction, true);
			context.EnableRule(x => x.IsRuleC0834_N02Active);
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);

			entryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedure.Procedure._53;
			AssertHasMessageError(entryInstruction.CEI_ProcedureInfo, message);

			context.DisableRule(x => x.IsRuleC0834_N02Active);
			entryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedure.Procedure._53;
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);

			context.EnableRule(x => x.IsRuleC0834_N02Active);
			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageError(entryInstruction.CEI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageError(entryInstruction.CEI_ProcedureInfo, message);

			context.DisableRule(x => x.IsRuleC0834_N02Active);
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);

			context.EnableRule(x => x.IsRuleC0834_N02Active);
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);

			context.DisableRule(x => x.IsRuleC0834_N02Active);
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			AssertNoMessageError(entryInstruction.CEI_ProcedureInfo, message);
		}

		public void TestRuleC0628ModeratedByRuleC0810()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = "A";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = "1122F15";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "3344D01";

			var message = "[C0628] Location Of Goods is mandatory for this declaration sub style.";

			AssertEquals(true, entryInstruction.Validation.ValidationDecider.IsRuleC0628ActiveForGoodsLocationDescription);
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertHasMessageError("Rule C0628 should apply when at least one instruction invoice lines has not procedure ending with F15.", entryInstruction.GoodsLocationDescriptionInfo, message);

			invoiceLine2.JI_Procedure = "3344F15";
			AssertEquals(false, entryInstruction.Validation.ValidationDecider.IsRuleC0628ActiveForGoodsLocationDescription);
			entryInstruction.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("Rule C0628 should not apply when all instruction invoice lines have procedure ending with F15.", entryInstruction.GoodsLocationDescriptionInfo, message);
		}

		public void TestValidationDecider()
		{
			var deltaGDeclaration = Factory.New<JobDeclaration>();
			var deltaGEntryInstruction = deltaGDeclaration.CustomsEntryInstructions.AddNew();

			deltaGDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertNull(deltaGEntryInstruction.Validation.ValidationDecider);

			var exportDeltaIEDeclaration = Factory.New<JobDeclaration>();
			var exportDeltaIEEntryInstruction = deltaGDeclaration.CustomsEntryInstructions.AddNew();
			exportDeltaIEDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			exportDeltaIEDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertNull(deltaGEntryInstruction.Validation.ValidationDecider);

			var importDeltaIEDeclaration = Factory.New<JobDeclaration>();
			var importDeltaIEEntryInstruction = importDeltaIEDeclaration.CustomsEntryInstructions.AddNew();
			importDeltaIEDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			importDeltaIEDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<UCC6ImportEntryInstructionValidationDecider>(importDeltaIEEntryInstruction.Validation.ValidationDecider);
		}

		public void TestCheckCEI_DateForDuty_NAT_004Bis()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			using (var context = new EntryInstructionValidationDeciderTestContext(entryInstruction, true))
			{
				context.EnableRule(x => x.IsRuleNAT_004BisActive);
				entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(-1);
				AssertHasMessageError("MessageError is expected when EntrySubStyleCode is 'D' and Assessment Date is in the past.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInThePastMessage);

				entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.F;
				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(-2);
				AssertHasMessageError("MessageError is expected when EntrySubStyleCode is 'F' and Assessment Date is in the past.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInThePastMessage);

				entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.A;
				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(-3);
				AssertNoMessageError("No MessageError is Expected as the Assessment Date range does not have restrictions when EntrySubStyleCode other than 'D' or 'F'.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInThePastMessage);

				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(31);
				AssertNoMessageError("No MessageError is Expected as the Assessment Date range does not have restrictions when EntrySubStyleCode other than 'D' or 'F'.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInTheFutureMessage);

				entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(32);
				AssertHasMessageError("MessageError is expected because EntrySubStyleCode is 'D', and the Assessment Date exceeds 30 days from the declaration date.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInTheFutureMessage);

				entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.F;
				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(33);
				AssertHasMessageError("MessageError is expected because EntrySubStyleCode is 'F', and the Assessment Date exceeds 30 days from the declaration date.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInTheFutureMessage);

				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(10);
				AssertNoMessageError("No MessageError is Expected when the Assessment Date is within the allowed range (within 30 days).", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInTheFutureMessage);

				context.DisableRule(x => x.IsRuleNAT_004BisActive);
				entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(-1);
				AssertNoMessageError("No MessageError is Expected When rule NAT_004Bis is disabled.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInThePastMessage);

				entryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.F;
				entryInstruction.CEI_DateForDuty = DateTime.Now.AddDays(-2);
				AssertNoMessageError("No MessageError is Expected When rule NAT_004Bis is disabled.", entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.AssessmentDateInThePastMessage);
			}
		}

		public void TestCheckCEI_Style()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = ZString.Empty;
			entryInstruction.Validation.ValidateCEI_Style();
			AssertHasMessageErrorContaining("CEI_Style should have a blue message error as the value is empty.", entryInstruction.CEI_StyleInfo, "You have not entered");

			string authorizationMandatoryMessage(IEnumerable<string> authorizationTypes)
			{
				var readableAuthorizationTypes = string.Join(",", authorizationTypes.ToArray());
				return $"One of {readableAuthorizationTypes} authorization is mandatory for Declaration Type {entryInstruction.CEI_Style}, please configure it by clicking menu item Brokerage > Data > Auto Populate Authorizations, or selecting one in the Authorization grid manually.";
			}

			var declarationCheckList = new[]
			{
				DeltaGExportDeclarationTypeList.Codes.TemporaryExportOtherThanUnderCode21,
				DeltaGExportDeclarationTypeList.Codes.TemporaryExportWithEI,
				DeltaGExportDeclarationTypeList.Codes.PlacingGoodsUnderBW,
				DeltaGExportDeclarationTypeList.Codes.ManufacturingOfGoodsUnderSupervision,
				DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing,
				DeltaGImportDeclarationTypeList.Codes.TemporaryImportation,
				DeltaGImportDeclarationTypeList.Codes.EntryForEndUse,
				DeltaGImportDeclarationTypeList.Codes.EntryForEndUseWithEI,
				DeltaGImportDeclarationTypeList.Codes.EntryOfGoodsForAFreeZone,
				DeltaGImportDeclarationTypeList.Codes.EntryOfGoodsForAFreeZoneWithEI
			};
			foreach (var declarationType in declarationCheckList)
			{
				AssertAuthorizationRequiredMessage(declarationType);
			}

			void AssertAuthorizationRequiredMessage(string declarationType)
			{
				entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				var requiredAuthorizationTypes = FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes(declarationType);
				if (!requiredAuthorizationTypes.IsNullOrEmpty())
				{
					entryInstruction.CEI_Style = declarationType;
					var authorizationRequiredMessage = authorizationMandatoryMessage(requiredAuthorizationTypes);
					AssertHasMessageError("No usage.", entryInstruction.CEI_StyleInfo, authorizationRequiredMessage);
					AssertNoMessageErrorContaining("CEI_Style should not have a blue message error as the value is not empty.", entryInstruction.CEI_StyleInfo, "You have not entered");

					var usage = entryInstruction.CusAuthorizationUsages.AddNew();
					usage.AGC_Code = "XXX";
					AssertHasMessageError("Have an usage with incorrect type.", entryInstruction.CEI_StyleInfo, authorizationRequiredMessage);

					usage.AGC_Code = requiredAuthorizationTypes.FirstOrDefault();
					AssertNoMessageError("With expected usage.", entryInstruction.CEI_StyleInfo, authorizationRequiredMessage);
				}
			}
		}

		public void TestCheckCEI_StyleWithProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "40", "71", "F61", "", "EXP", "40P");
			procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;

			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "40", "71", "F62", "", "EXP", "40P");
			procedure2.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.Yes;

			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "40", "71", "F63", "", "EXP", "40P");
			procedure3.ZZ6_IntoTemporaryImport = WarehouseMoveStatus.Codes.Yes;

			var procedure4 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "40", "71", "F64", "", "EXP", "40P");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "4071F61";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			instruction.Validation.ValidateCEI_Style();
			AssertHasMessageError(instruction.CEI_StyleInfo, "It is legally forbidden to use Indirect representation for this procedure, you should consider not using Special Procedure or changing Representation Type on Misc Tab.");

			invoiceLine.JI_Procedure = "4071F62";
			instruction.Validation.ValidateCEI_Style();
			AssertHasMessageError(instruction.CEI_StyleInfo, "It is legally forbidden to use Indirect representation for this procedure, you should consider not using Special Procedure or changing Representation Type on Misc Tab.");

			invoiceLine.JI_Procedure = "4071F63";
			instruction.Validation.ValidateCEI_Style();
			AssertHasMessageError(instruction.CEI_StyleInfo, "It is legally forbidden to use Indirect representation for this procedure, you should consider not using Special Procedure or changing Representation Type on Misc Tab.");

			invoiceLine.JI_Procedure = "4071F64";
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError(instruction.CEI_StyleInfo, "It is legally forbidden to use Indirect representation for this procedure, you should consider not using Special Procedure or changing Representation Type on Misc Tab.");

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			invoiceLine.JI_Procedure = "4071F61";
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError(instruction.CEI_StyleInfo, "It is legally forbidden to use Indirect representation for this procedure, you should consider not using Special Procedure or changing Representation Type on Misc Tab.");
		}

		public void TestCheckCEI_DateForDuty()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			AssertNoMessageError(entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.EmptyAssessmentDateMessage);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertHasMessageError(entryInstruction.CEI_DateForDutyInfo, CusEntryInstructionValidation.EmptyAssessmentDateMessage);
		}

		public void TestCheckCEI_OA_Warehouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "desc", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			Factory.Save();

			var warehouseAddress = Factory.New<OrgAddress>();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			var declaration = Factory.New<JobDeclaration>();
			entryInstruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageError(entryInstruction.CEI_OA_WarehouseInfo, ErrorCollectorHelper.WarehouseAuthorisationNotFoundMessage);

			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, ErrorCollectorHelper.WarehouseAuthorisationNotFoundMessage);

			var aulAuthorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL Auth");
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertHasMessageError(entryInstruction.CEI_OA_WarehouseInfo, ZString.Format(ErrorCollectorHelper.AlternativePurposeAuthorisationFound, CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation));

			var cwpAuthorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("CWP Auth");
			entryInstruction.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageErrors(entryInstruction.CEI_OA_WarehouseInfo);
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "desc", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();

			var warehouseAddress = Factory.New<OrgAddress>();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			var declaration = Factory.New<JobDeclaration>();
			entryInstruction.CEI_JE = declaration.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError(entryInstruction.CEI_OA_Warehouse2Info, ErrorCollectorHelper.WarehouseAuthorisationNotFoundMessage);

			entryInstruction.CEI_OA_Warehouse2 = warehouseAddress.PK;
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, ErrorCollectorHelper.WarehouseAuthorisationNotFoundMessage);

			var aulAuthorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUL Auth");
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError(entryInstruction.CEI_OA_Warehouse2Info, ZString.Format(ErrorCollectorHelper.AlternativePurposeAuthorisationFound, CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation));

			var cwpAuthorisation = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("CWP Auth");
			entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageErrors(entryInstruction.CEI_OA_Warehouse2Info);
		}

		public void TestValuationBypassValidations()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.ZG_BypassCode = "X";

			AssertHasMessageErrorContaining(entryInstruction.ZG_BypassCodeInfo, ListValidation.InvalidCodeMessageError);
			entryInstruction.ZG_BypassCode = TariffBypassCodeList.Codes.TariffBypass_I;
			AssertNoMessageErrorContaining(entryInstruction.ZG_BypassCodeInfo, ListValidation.InvalidCodeMessageError);

			AssertHasMessageErrorContaining(entryInstruction.ZG_BypassReasonInfo, "Valuation bypass reason is required when valuation bypass code is 'I'");

			entryInstruction.ZG_BypassReason = "Unit Test";
			AssertNoMessageErrorContaining(entryInstruction.ZG_BypassReasonInfo, "Valuation bypass reason is required when valuation bypass code is 'I'");
		}

		public void TestCheckCEI_SubStyle()
		{
			const string messageError = "[NAT_130_Bis] Entry Instruction Sub Style must be A when the declaration has E0001 Additional Information.";

			var declaration = Factory.New<JobDeclaration>();

			using (var context = new Business.Testing.EntryInstructionValidationDeciderTestContext(declaration, true))
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

				var subStyleIsNotA = new FieldPreq<JobDeclaration>(declaration => declaration.CustomsEntryInstructions[0].CEI_SubStyleInfo)
					.Values(EntrySubstyleCodePairList.Codes.C)
					.NotValues(EntrySubstyleCodePairList.Codes.A);

				testCase.SetUpCondition(isStandardDeclaration && subStyleIsNotA);

				context.EnableRule(x => x.IsRuleNAT_130BisActive);
				testCase.RunAssertion(declaration =>
				{
					var entryInstruction = declaration.CustomsEntryInstructions[0];
					entryInstruction.Validation.ValidateCEI_SubStyle();
					AssertHasMessageError("When the declaration has E0001 Additional Information, CEI_SubStyle must be value A.", entryInstruction.CEI_SubStyleInfo, messageError);
				}, declaration =>
				{
					var entryInstruction = declaration.CustomsEntryInstructions[0];
					entryInstruction.Validation.ValidateCEI_SubStyle();
					AssertNoMessageError(entryInstruction.CEI_SubStyleInfo, messageError);
				});

				context.DisableRule(x => x.IsRuleNAT_130BisActive);
				testCase.RunAssertion(declaration =>
				{
					var entryInstruction = declaration.CustomsEntryInstructions[0];
					entryInstruction.Validation.ValidateCEI_SubStyle();
					AssertNoMessageError("When RuleNAT_130Bis is not activated, there should be no validation.", entryInstruction.CEI_SubStyleInfo, messageError);
				}, declaration =>
				{
					var entryInstruction = declaration.CustomsEntryInstructions[0];
					entryInstruction.Validation.ValidateCEI_SubStyle();
					AssertNoMessageError(entryInstruction.CEI_SubStyleInfo, messageError);
				});
			}
		}
	}
}
