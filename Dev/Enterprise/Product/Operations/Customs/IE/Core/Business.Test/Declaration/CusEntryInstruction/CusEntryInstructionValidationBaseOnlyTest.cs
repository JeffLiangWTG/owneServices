using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class CusEntryInstructionValidationBaseOnlyTest : CusEntryInstructionValidationAbstractTest<CusEntryInstructionValidation>
	{
		public void TestCheckCEI_Style()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Ireland, "A", "11", "11", "111", "One", MessageTypeList.Codes.MiscellaneousCustoms, group: ImportDeclarationTypeList.Codes.H1);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_StyleInfo, "XX", ImportDeclarationTypeList.Codes.H1);
		}

		public void TestCheckCEI_OA_Warehouse_AuthorizationUsageCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var validation = entryInstruction.Validation;
			validation.ValidateCEI_OA_Warehouse();
			AssertHasMessageErrorContaining("When no authorization usage", entryInstruction.CEI_OA_WarehouseInfo, "Please enter an Authorization for the Warehouse From.");

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			cusAuthorizationUsage.AGC_Number = "001";
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			entryInstruction.Factory.ClearCachedValue<CusAuthorisationHeader[]>($"GetAuthorisationHeaders|IE|{ZDateTime.Today.ToISO8601ShortDateString()}|{authorizationHeader.CPH_OA_AppliesTo.ToStringKey()}|CW1_CW2_CWP_TST");

			validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageErrorContaining("When just one authorization usage", entryInstruction.CEI_OA_WarehouseInfo, "Please enter an Authorization for the Warehouse From.");
			AssertNoMessageErrorContaining("When just one authorization usage", entryInstruction.CEI_OA_WarehouseInfo, "More than one Authorizations exist for the Warehouse From.");

			cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			cusAuthorizationUsage.AGC_Number = "001";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;
			validation.ValidateCEI_OA_Warehouse();
			AssertHasMessageErrorContaining("When more than one authorization usages", entryInstruction.CEI_OA_WarehouseInfo, "More than one Authorizations exist for the Warehouse From.");
		}

		public void TestCheckCEI_OA_Warehouse_Mandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Ireland, "", "76", "00", "E71", "", MessageTypeList.Codes.Export, "B3");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.JI_Procedure = "7600E71";

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_OA_WarehouseInfo, "This field is mandatory when Procedure/CPC is an out of warehouse procedure.");
		}

		public void TestCheckCEI_OA_Warehouse2_AuthorizationUsageCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var validation = entryInstruction.Validation;
			validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageErrorContaining("When no authorization usage", entryInstruction.CEI_OA_Warehouse2Info, "Please enter an Authorization for the Warehouse To. Authorizations can be entered under Maintain > Customs > Customs Files > Authorizations");

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = "0700";
			validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageErrorContaining("When no authorization usage and procedure code is 0700", entryInstruction.CEI_OA_Warehouse2Info, "Please enter an Authorization for the Warehouse To. Authorizations can be entered under Maintain > Customs > Customs Files > Authorizations");

			invoiceLine1.JI_Procedure = ZString.Empty;

			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			cusAuthorizationUsage.AGC_Number = "001";
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OA_AppliesTo = warehouse.MainAddress.PK;

			entryInstruction.Factory.ClearCachedValue<CusAuthorisationHeader[]>($"GetAuthorisationHeaders|IE|{ZDateTime.Today.ToISO8601ShortDateString()}|{authorizationHeader.CPH_OA_AppliesTo.ToStringKey()}|CW1_CW2_CWP_TST");

			validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageErrorContaining("When just one authorization usage", entryInstruction.CEI_OA_Warehouse2Info, "Please enter an Authorization for the Warehouse To. Authorizations can be entered under Maintain > Customs > Customs Files > Authorizations");
			AssertNoMessageErrorContaining("When just one authorization usage", entryInstruction.CEI_OA_Warehouse2Info, "More than one Authorizations exist for the Warehouse To.");

			cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			cusAuthorizationUsage.AGC_Number = "001";
			cusAuthorizationUsage.AGC_OH_Owner = owner.PK;
			validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageErrorContaining("When more than one authorization usages", entryInstruction.CEI_OA_Warehouse2Info, "More than one Authorizations exist for the Warehouse To.");
		}

		public void TestCheckCEI_OA_Warehouse2_Mandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Ireland, "", "76", "00", "E71", "", MessageTypeList.Codes.Export, "B3");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.JI_Procedure = "7600E71";

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_OA_Warehouse2Info, "This field is mandatory when Procedure/CPC is an into warehouse procedure.");
		}

		public void TestValidateGoodsLocationDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			var entryInstructionAsProvider = entryInstruction as ICusGoodsLocationProvider;
			CombineAssertions(() =>
			{
				entryInstructionAsProvider.ValidateGoodsLocationDescription();
				AssertHasMessageError("Import Declaration Goods Location empty Unlocode and contains notifications", entryInstructionAsProvider.GoodsLocationDescriptionInfo, "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.");

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryInstructionAsProvider.ValidateGoodsLocationDescription();
				AssertNoNotifications("Export Declaration Goods Location empty Unlocode has no notifications", entryInstructionAsProvider.GoodsLocationDescriptionInfo);
			});
		}

		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override CusEntryInstructionValidation GetValidation() => new CusEntryInstructionValidation(instruction);

		protected override void SetUp()
		{
			base.SetUp();
			var entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
		}
		JobComInvoiceLine invoiceLine;
	}
}
