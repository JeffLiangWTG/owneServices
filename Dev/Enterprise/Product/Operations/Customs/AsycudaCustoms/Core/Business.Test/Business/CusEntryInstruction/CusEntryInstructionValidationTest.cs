using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRemainingCustomsValue_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				CombineAssertions(() =>
				{
					instruction.Validation.ValidateRemainingCustomsValue();
					AssertNoErrorContaining("Valid", instruction.RemainingCustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);
					instruction.RiskManagements.AddNew().CSI_Value = 0.03m;
					instruction.Validation.ValidateRemainingCustomsValue();
					AssertHasErrorContaining("Negative", instruction.RemainingCustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);
				});
			}
		}

		public void TestCheckRemainingNetWeightKilograms_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				CombineAssertions(() =>
				{
					instruction.Validation.ValidateRemainingNetWeightKilograms();
					AssertNoErrorContaining("Valid", instruction.RemainingNetWeightKilogramsInfo, MandatoryValidation.ValueCannotBeNegative);
					instruction.RiskManagements.AddNew().CSI_Quantity = 0.03m;
					instruction.Validation.ValidateRemainingNetWeightKilograms();
					AssertHasErrorContaining("Negative", instruction.RemainingNetWeightKilogramsInfo, MandatoryValidation.ValueCannotBeNegative);
				});
			}
		}

		public void TestCheckRemainingCustomsQuantity_IsRiskManagementEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				CombineAssertions(() =>
				{
					instruction.Validation.ValidateRemainingCustomsQuantity();
					AssertNoErrorContaining("Valid", instruction.RemainingCustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
					instruction.RiskManagements.AddNew().CSI_Quantity2 = 0.03m;
					instruction.Validation.ValidateRemainingCustomsQuantity();
					AssertHasErrorContaining("Negative", instruction.RemainingCustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
				});
			}
		}

		public void TestCheckRemainingCustomsValue_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				instruction.RiskManagements.AddNew().CSI_Value = 0.03m;
				instruction.Validation.ValidateRemainingCustomsValue();
				AssertEquals(false, instruction.RemainingCustomsValueInfo.HasNotifications());
			}
		}

		public void TestCheckRemainingNetWeightKilograms_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				instruction.RiskManagements.AddNew().CSI_Quantity = 0.03m;
				instruction.Validation.ValidateRemainingNetWeightKilograms();
				AssertEquals(false, instruction.RemainingNetWeightKilogramsInfo.HasNotifications());
			}
		}

		public void TestCheckRemainingCustomsQuantity_IsRiskManagementDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				instruction.RiskManagements.AddNew().CSI_Quantity2 = 0.03m;
				instruction.Validation.ValidateRemainingCustomsQuantity();
				AssertEquals(false, instruction.RemainingCustomsQuantityInfo.HasNotifications());
			}
		}

		public void TestValidateASY_LocalReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B002000";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_LocalReferenceNumber = "number2";
			AssertNoErrors(instruction.ASY_LocalReferenceNumberInfo);
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.ASY_LocalReferenceNumber = "number2";
			AssertHasError("CusEntryInstruction.ASY_LocalReferenceNumber", instruction2.ASY_LocalReferenceNumberInfo, "This Local Reference Number found on other Entry Instruction of this declaration.");
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.CH_BGMReference = "entry1";
			instruction2.ASY_LocalReferenceNumber = "entry1";
			AssertHasError("CusEntryInstruction.ASY_LocalReferenceNumber", instruction2.ASY_LocalReferenceNumberInfo, "This Local Reference Number is duplicate to the entry reference of the entry header linked to other Entry Instruction of this declaration.");
			instruction2.ASY_LocalReferenceNumber = "number3";
			AssertNoErrors(instruction2.ASY_LocalReferenceNumberInfo);
		}

		public void TestValidateASY_LocalReferenceNumber_DuplicateWithOtherDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B001000";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_LocalReferenceNumber = "number1";
			Factory.Save();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B002000";
			declaration2.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			instruction2.ASY_LocalReferenceNumber = "number1";
			AssertHasError("CusEntryInstruction.ASY_LocalReferenceNumber", instruction2.ASY_LocalReferenceNumberInfo, "This Local Reference Number found on Declaration Job: B001000.");
			declaration2.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			instruction2.ASY_LocalReferenceNumber = "number1";
			AssertNoErrors(instruction2.ASY_LocalReferenceNumberInfo);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_DeclarationReference = "B003000";
				declaration3.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
				instruction3.ASY_LocalReferenceNumber = "number1";
				AssertNoErrors(instruction2.ASY_LocalReferenceNumberInfo);
			}
		}

		public void TestValidateASY_LocalReferenceNumber_DuplicateWithOtherEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B001000";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.CH_BGMReference = "entry1";
			Factory.Save();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B002000";
			declaration2.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			instruction2.ASY_LocalReferenceNumber = "entry1";
			AssertHasError("CusEntryInstruction.ASY_LocalReferenceNumber", instruction2.ASY_LocalReferenceNumberInfo, "This Local Reference Number found on an Entry for Declaration Job: B001000.");
			declaration2.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			instruction2.ASY_LocalReferenceNumber = "entry1";
			AssertNoErrors(instruction2.ASY_LocalReferenceNumberInfo);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_DeclarationReference = "B003000";
				declaration3.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
				instruction3.ASY_LocalReferenceNumber = "entry1";
				AssertNoErrors(instruction2.ASY_LocalReferenceNumberInfo);
			}
		}

		public void TestValidateASY_PortOfExit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, "Botswana", null);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsUQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Botswana, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ARIA", "Ariamsvlei", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.ASY_PortOfExit = "ARIA";
			AssertNoMessageErrors(instruction.ASY_PortOfExitInfo);
			instruction.ASY_PortOfExit = "ZZ";
			AssertHasMessageError(instruction.ASY_PortOfExitInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCEI_Style_EnsureThatAllInvoiceLinesHaveSameProcedureDetails()
		{
			var impDeclaration = Factory.New<JobDeclaration>();
			impDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = impDeclaration.Invoices.AddNew();
			var instruction = impDeclaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.JobComInvoiceLines.AddNew();
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "AA";
			procedure.ZZ6_ShipmentType = SharedJobMessageTypeList.Codes.Import;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_IntoTemporaryImport = "Y";
			procedure.ZZ6_OutOfTemporaryImport = "Y";
			procedure.ZZ6_IntoTemporaryExport = "Y";
			procedure.ZZ6_OutOfTemporaryExport = "Y";
			procedure.ZZ6_IntoInwardProcessing = "Y";
			procedure.ZZ6_OutOfInwardProcessing = "Y";
			procedure.ZZ6_IntoOutwardProcessing = "Y";
			procedure.ZZ6_OutofOutwardProcessing = "Y";
			procedure.ZZ6_IsGuaranteeConsumed = "Y";
			procedure.ZZ6_IsGuaranteeReleased = "Y";
			procedure.ZZ6_IsTransit = "Y";
			line.JI_CEI = instruction.PK;
			line.JI_Procedure = procedure.ZZ6_ProcedureCode;
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "BB";
			procedure2.ZZ6_ShipmentType = SharedJobMessageTypeList.Codes.Import;
			procedure2.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure2.ZZ6_IntoWarehouse = "N";
			procedure2.ZZ6_OutOfWarehouse = "N";
			procedure2.ZZ6_IntoTemporaryImport = "N";
			procedure2.ZZ6_OutOfTemporaryImport = "N";
			procedure2.ZZ6_IntoTemporaryExport = "N";
			procedure2.ZZ6_OutOfTemporaryExport = "N";
			procedure2.ZZ6_IntoInwardProcessing = "N";
			procedure2.ZZ6_OutOfInwardProcessing = "N";
			procedure2.ZZ6_IntoOutwardProcessing = "N";
			procedure2.ZZ6_OutofOutwardProcessing = "N";
			procedure2.ZZ6_IsGuaranteeConsumed = "N";
			procedure2.ZZ6_IsGuaranteeReleased = "N";
			procedure2.ZZ6_IsTransit = "N";
			var line3 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_CEI = instruction.PK;
			line2.JI_Procedure = procedure2.ZZ6_ProcedureCode;
			instruction.Validation.ValidateCEI_Style();
			CombineAssertions(() =>
			{
				var errMessage = @"All invoice lines should have matching procedure attributes:
Into Warehouse is Y on Invoice Line 1, but is N on Invoice Line 2
Out Of Warehouse is Y on Invoice Line 1, but is N on Invoice Line 2
Into Temporary Import is Y on Invoice Line 1, but is N on Invoice Line 2
Out Of Temporary Import is Y on Invoice Line 1, but is N on Invoice Line 2
Into Temporary Export is Y on Invoice Line 1, but is N on Invoice Line 2
Out Of Temporary Export is Y on Invoice Line 1, but is N on Invoice Line 2
Into Inward Processing is Y on Invoice Line 1, but is N on Invoice Line 2
Out Of Inward Processing is Y on Invoice Line 1, but is N on Invoice Line 2
Into Outward Processing is Y on Invoice Line 1, but is N on Invoice Line 2
Outof Outward Processing is Y on Invoice Line 1, but is N on Invoice Line 2
Is Guarantee Consumed is Y on Invoice Line 1, but is N on Invoice Line 2
Is Guarantee Released is Y on Invoice Line 1, but is N on Invoice Line 2
Is Transit is Y on Invoice Line 1, but is N on Invoice Line 2
";
				AssertHasMessageError("not matched", instruction.CEI_StyleInfo, errMessage);
				procedure2.ZZ6_IntoWarehouse = "Y";
				procedure2.ZZ6_OutOfWarehouse = "Y";
				procedure2.ZZ6_IntoTemporaryImport = "Y";
				procedure2.ZZ6_OutOfTemporaryImport = "Y";
				procedure2.ZZ6_IntoTemporaryExport = "Y";
				procedure2.ZZ6_OutOfTemporaryExport = "Y";
				procedure2.ZZ6_IntoInwardProcessing = "Y";
				procedure2.ZZ6_OutOfInwardProcessing = "Y";
				procedure2.ZZ6_IntoOutwardProcessing = "Y";
				procedure2.ZZ6_OutofOutwardProcessing = "Y";
				procedure2.ZZ6_IsGuaranteeConsumed = "Y";
				procedure2.ZZ6_IsGuaranteeReleased = "Y";
				procedure2.ZZ6_IsTransit = "Y";
				instruction.Validation.ValidateCEI_Style();
				AssertNoMessageError("matched", instruction.CEI_StyleInfo, errMessage);
				Assert("no error message for procedure match", !instruction.CEI_StyleInfo.GetMessageErrors().Any(x => x.Message.Contains("All invoice lines should have matching procedure attributes")));
			}

			);
		}

		public void TestCheckCEI_Style_EnsureThatAllInvoiceLinesHaveSameProcedureDetails_OnlyOneInvoiceLine()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "CAT", "11", "10", "000", "Eleven", JobMessageTypeList.Codes.Import, "");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Procedure = "1110000";
			instruction.Validation.ValidateCEI_Style();
			AssertEquals(false, instruction.CEI_StyleInfo.HasMessageError(AllInvoiceLinesShouldHaveSameProcedureDetails));
		}

		const string AllInvoiceLinesShouldHaveSameProcedureDetails = " All invoice lines should have matching procedure attributes.";
		public void TestCheckCEI_OA_Warehouse()
		{
			const string errorMsg = "The From Warehouse, must be entered and match a Warehouse record.";
			var helper = new WhsDataTestHelper(Factory);
			helper.Supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "AB";
			procedure.ZZ6_PreviousProcedureCode = "10";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Botswana;
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			var whsWarehouse = helper.WhsWarehouse;
			whsWarehouse.WW_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = helper.Supplier.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			CombineAssertions(() =>
			{
				entryInstruction.Validation.ValidateCEI_OA_Warehouse();
				AssertNoMessageError("No validation before invoice line is entered.", entryInstruction.CEI_OA_WarehouseInfo, errorMsg);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = "AB10";
				entryInstruction.Validation.ValidateCEI_OA_Warehouse();
				AssertHasMessageError("From Warehouse should have message error message.", entryInstruction.CEI_OA_WarehouseInfo, errorMsg);
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				AssertNoMessageError("From Warehouse should not have message error message.", entryInstruction.CEI_OA_WarehouseInfo, errorMsg);
				entryInstruction.CEI_OA_Warehouse = helper.Supplier.MainAddress.PK;
				AssertHasMessageError("From Warehouse should have message error message.", entryInstruction.CEI_OA_WarehouseInfo, errorMsg);
				invoiceLine.JI_CEI = ZGuid.Empty;
				entryInstruction.Validation.ValidateCEI_OA_Warehouse();
				AssertNoMessageError("From Warehouse should not have message error message.", entryInstruction.CEI_OA_WarehouseInfo, errorMsg);
			});
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			const string errorMsg = "The To Warehouse must be entered and match a Warehouse record.";
			var helper = new WhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "AB";
			procedure.ZZ6_PreviousProcedureCode = "10";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Botswana;
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			var whsWarehouse = helper.WhsWarehouse;
			whsWarehouse.WW_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			CombineAssertions(() =>
			{
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertNoMessageError("No validation before invoice line is entered.", entryInstruction.CEI_OA_Warehouse2Info, errorMsg);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = "AB10";
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageError("To Warehouse should have message error message.", entryInstruction.CEI_OA_Warehouse2Info, errorMsg);
				entryInstruction.CEI_OA_Warehouse2 = helper.Warehouse.MainAddress.PK;
				AssertNoMessageError("To Warehouse should not have message error message.", entryInstruction.CEI_OA_Warehouse2Info, errorMsg);
				entryInstruction.CEI_OA_Warehouse2 = helper.Supplier.MainAddress.PK;
				AssertHasMessageError("To Warehouse should have message error message.", entryInstruction.CEI_OA_Warehouse2Info, errorMsg);
				invoiceLine.JI_CEI = ZGuid.Empty;
				entryInstruction.Validation.ValidateCEI_OA_Warehouse2();
				AssertNoMessageError("To Warehouse should not have message error message.", entryInstruction.CEI_OA_Warehouse2Info, errorMsg);
			});
		}
	}
}
