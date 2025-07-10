using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaGJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestCheckJI_CountryOfOrigin_Mandatory()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoice.JZ_RN_NKDefaultOrigin = ZString.Empty;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageErrors("Not Mandatory", invoiceLine.JI_CountryOfOriginInfo);
		}

		public void TestInventoryManagementSettingMatchesProcedure()
		{
			AssertInventoryManagementSettingMatchesProcedure(true, "40", WarehouseMoveStatus.Codes.Yes, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, true);
			AssertInventoryManagementSettingMatchesProcedure(false, "41", WarehouseMoveStatus.Codes.Yes, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, false);
			AssertInventoryManagementSettingMatchesProcedure(true, "42", WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.Yes, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, true);
			AssertInventoryManagementSettingMatchesProcedure(true, "43", WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.Yes, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, true);
			AssertInventoryManagementSettingMatchesProcedure(true, "44", WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.Yes, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, true);
			AssertInventoryManagementSettingMatchesProcedure(true, "45", WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.Yes, WarehouseMoveStatus.Codes.No, true);
			AssertInventoryManagementSettingMatchesProcedure(true, "46", WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.Yes, true);
			AssertInventoryManagementSettingMatchesProcedure(true, "47", WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, WarehouseMoveStatus.Codes.No, false);

			void AssertInventoryManagementSettingMatchesProcedure(bool supportBondedWarehouse, string procedureCode, string intoWarehouse, string intoInwardProcessing, string intoOutwardProcessing, string outOfInwardProcessing, string outofOutwardProcessing, string outOfWarehouse, bool shouldBeTrue)
			{
				using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
				{
					var helper = new UniversalReferenceTestDataHelper(Factory);
					var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", procedureCode, "71", "F61", "", "EXP", "40P");

					procedure.ZZ6_IntoWarehouse = intoWarehouse;
					procedure.ZZ6_IntoInwardProcessing = intoInwardProcessing;
					procedure.ZZ6_IntoOutwardProcessing = intoOutwardProcessing;
					procedure.ZZ6_OutOfInwardProcessing = outOfInwardProcessing;
					procedure.ZZ6_OutofOutwardProcessing = outofOutwardProcessing;
					procedure.ZZ6_OutOfWarehouse = outOfWarehouse;

					Factory.Save();

					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
					declaration.SetSupportsBondedWarehousingForTesting(supportBondedWarehouse);
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Procedure = procedureCode + "71F61";

					var instruction = declaration.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = instruction.PK;
					var invoiceLineValidation = new JobComInvLineValidationForTest(invoiceLine);

					AssertEquals("Prerequisite: SupportsBondedWarehousing has the expected value.", expected: supportBondedWarehouse, invoiceLine.SupportsBondedWarehousing);
					AssertEquals(shouldBeTrue, invoiceLineValidation.InventoryManagementSettingMatchesProcedureExposed);
				}
			}
		}

		class JobComInvLineValidationForTest : DeltaGJobComInvoiceLineValidation
		{
			public JobComInvLineValidationForTest(JobComInvoiceLine invL) : base(invL)
			{
			}

			protected override IEnumerable<ZString> PreviousDocumentsCheckExceptedDeclarationTypes => new ZString[] { "LIN" };

			public ZBool AdditionalProcedureCodesCheckFirst4Characters_Exposed => AdditionalProcedureCodesCheckFirst4Characters;
			public ZBool IsJIDescriptionMandatory_Exposed => IsJIDescriptionMandatory;

			public ZBool InventoryManagementSettingMatchesProcedureExposed => InventoryManagementSettingMatchesProcedure;
		}
	}
}
