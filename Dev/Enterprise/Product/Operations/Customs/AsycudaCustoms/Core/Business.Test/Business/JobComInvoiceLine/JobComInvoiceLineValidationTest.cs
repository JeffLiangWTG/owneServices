using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidVehicleVIN()
		{
			const string warningMsg = "The VIN captured is invalid.";
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.VehicleVIN = "12";
			AssertHasWarning("vin has no 17 digits", invoiceLine.VehicleVINInfo, warningMsg);
			invoiceLine.VehicleVIN = "123456789ABCDEFGI";
			AssertHasWarning("vin has 17 digits but contain invalid char 'I'", invoiceLine.VehicleVINInfo, warningMsg);
			invoiceLine.VehicleVIN = "0123456789ABCDEFG";
			AssertHasWarning("vin has 17 digits and no invalid char but failed through the ninth digit check'", invoiceLine.VehicleVINInfo, warningMsg);
			invoiceLine.VehicleVIN = "UU6JA69691D713820";
			AssertNoWarnings("valid vin", invoiceLine.VehicleVINInfo);
			invoiceLine.VehicleVIN = "";
			AssertNoWarnings("vin is empty", invoiceLine.VehicleVINInfo);
		}

		public void TestCheckJI_PreviousEntryNumber()
		{
			SetUpTestDataForValidation();
			CombineAssertions(() =>
			{
				inWhsInvoiceLine.JI_PreviousEntryNumber = ZString.Empty;
				AssertNoMessageErrors(inWhsInvoiceLine.JI_PreviousEntryNumberInfo);
				outWhsInvoiceLine.JI_PreviousEntryNumber = ZString.Empty;
				AssertHasMessageErrorContaining(outWhsInvoiceLine.JI_PreviousEntryNumberInfo, MandatoryValidation.YouHaveNotEntered);
				outWhsInvoiceLine.JI_PreviousEntryNumber = "123";
				AssertNoMessageErrors(outWhsInvoiceLine.JI_PreviousEntryNumberInfo);
			});
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			SetUpTestDataForValidation();
			CombineAssertions(() =>
			{
				inWhsInvoiceLine.JI_PreviousEntryLineNumber = 0;
				AssertNoMessageErrorContaining(inWhsInvoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
				outWhsInvoiceLine.JI_PreviousEntryLineNumber = 0;
				AssertHasMessageErrorContaining(outWhsInvoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
				outWhsInvoiceLine.JI_PreviousEntryLineNumber = 11;
				AssertNoMessageErrorContaining(outWhsInvoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(outWhsInvoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.ValueCannotBeNegative);
				outWhsInvoiceLine.JI_PreviousEntryLineNumber = -123;
				AssertNoMessageErrorContaining(outWhsInvoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(outWhsInvoiceLine.JI_PreviousEntryLineNumberInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestParent()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckJI_PartNo()
		{
			SetUpTestDataForValidation();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.RelatedOrganisations.AddOrganisationIfNotExist(helper.Importer.PK, "OWN");
			product.RelatedOrganisations.AddOrganisationIfNotExist(helper.Supplier.PK, "SUP");
			const string errorMsg = "Please enter a valid Product.";
			inWhsInvoiceLine.Validation.ValidateJI_PartNo();
			AssertHasMessageError("Error for empty JI_PartNO for into Whs invoice line.", inWhsInvoiceLine.JI_PartNoInfo, errorMsg);
			inWhsInvoiceLine.JI_PartNo = "123";
			AssertHasMessageError("Error for invalid JI_PartNO for into Whs invoice line.", inWhsInvoiceLine.JI_PartNoInfo, errorMsg);
			inWhsInvoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNoMessageError("No error for valid JI_PartNO for into Whs invoice line.", inWhsInvoiceLine.JI_PartNoInfo, errorMsg);
			outWhsInvoiceLine.Validation.ValidateJI_PartNo();
			AssertHasMessageError("Error for empty JI_PartNO for out of Whs invoice line.", outWhsInvoiceLine.JI_PartNoInfo, errorMsg);
			outWhsInvoiceLine.JI_PartNo = "123";
			AssertHasMessageError("Error for invalid JI_PartNO for out of Whs invoice line.", outWhsInvoiceLine.JI_PartNoInfo, errorMsg);
			outWhsInvoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNoMessageError("No error for valid JI_PartNO for out of Whs invoice line.", outWhsInvoiceLine.JI_PartNoInfo, errorMsg);
		}

		public void TestCheckJI_BondedWhsQuantity()
		{
			SetUpTestDataForValidation();
			inWhsInvoiceLine.Validation.ValidateJI_BondedWhsQuantity();
			AssertHasMessageErrorContaining("Error for empty Bonded Whs. Qty for into Whs invoice line", inWhsInvoiceLine.JI_BondedWhsQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			inWhsInvoiceLine.JI_BondedWhsQuantity = 100m;
			AssertNoMessageErrorContaining("No error for valid Bonded Whs. Qty for into Whs invoice line", inWhsInvoiceLine.JI_BondedWhsQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			outWhsInvoiceLine.Validation.ValidateJI_BondedWhsQuantity();
			AssertHasMessageErrorContaining("Error for empty Bonded Whs. Qty for out of Whs invoice line", outWhsInvoiceLine.JI_BondedWhsQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			outWhsInvoiceLine.JI_BondedWhsQuantity = 100m;
			AssertNoMessageErrorContaining("No error for Bonded Whs. Qty for out of Whs invoice line", outWhsInvoiceLine.JI_BondedWhsQuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_BondedWhsUnitQty()
		{
			SetUpTestDataForValidation();
			inWhsInvoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
			AssertHasMessageErrorContaining("Error for empty Bonded Whs. Qty unit for into Whs invoice line", inWhsInvoiceLine.JI_BondedWhsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			inWhsInvoiceLine.JI_BondedWhsUnitQty = Core.Constants.PkgUnit.Bag;
			AssertNoMessageErrorContaining("No error for valid Bonded Whs. Qty unit for into Whs invoice line", inWhsInvoiceLine.JI_BondedWhsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			outWhsInvoiceLine.Validation.ValidateJI_BondedWhsUnitQty();
			AssertHasMessageErrorContaining("Error for empty Bonded Whs. Qty unit for out of Whs invoice line", outWhsInvoiceLine.JI_BondedWhsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
			outWhsInvoiceLine.JI_BondedWhsUnitQty = Core.Constants.PkgUnit.Bag;
			AssertNoMessageErrorContaining("No error for Bonded Whs. Qty unit for out of Whs invoice line", outWhsInvoiceLine.JI_BondedWhsUnitQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Procedure_JE_MessageType()
		{
			var impDeclaration = Factory.New<JobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			var invoiceHeader = impDeclaration.Invoices.AddNew();
			var line = invoiceHeader.JobComInvoiceLines.AddNew();
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "XX";
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_ShipmentType = "EXP";
			line.JI_Procedure = procedure.ZZ6_ProcedureCode;
			CombineAssertions(() =>
			{
				var shipmentTypeDesc = impDeclaration.Lookups.MessageTypeList.GetDescriptionFromCode(impDeclaration.JE_MessageType);
				var errMessage = $"{shipmentTypeDesc} Procedure Code should be selected when Shipment Type is {shipmentTypeDesc}";
				AssertHasMessageError("not matched", line.JI_ProcedureInfo, errMessage);
				procedure.ZZ6_ShipmentType = "IMP";
				line.Validation.ValidateJI_Procedure();
				AssertNoMessageError("ShipmentType matched IMP", line.JI_ProcedureInfo, errMessage);
				procedure.ZZ6_ShipmentType = "IMP,EXP";
				line.Validation.ValidateJI_Procedure();
				AssertNoMessageError("ShipmentType matched IMP,EXP", line.JI_ProcedureInfo, errMessage);
				line.JI_Procedure = null;
				line.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Procedure is null", line.JI_ProcedureInfo, errMessage);
			});
		}

		public void TestCheckJI_Procedure_CEI_Style()
		{
			var impDeclaration = Factory.New<JobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			var invoiceHeader = impDeclaration.Invoices.AddNew();
			var instruction = impDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "AAA";
			var line = invoiceHeader.JobComInvoiceLines.AddNew();
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "XX";
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_ShipmentType = "IMP";
			procedure.ZZ6_Group = "BBB";
			line.JI_CEI = instruction.PK;
			line.JI_Procedure = procedure.ZZ6_ProcedureCode;
			CombineAssertions(() =>
			{
				var errMessage = "AAA Procedure Code should be selected when Declaration Type is AAA";
				AssertHasMessageError("mot mached", line.JI_ProcedureInfo, errMessage);
				procedure.ZZ6_Group = "AAA";
				line.Validation.ValidateJI_Procedure();
				AssertNoMessageError("declarationType matched AAA", line.JI_ProcedureInfo, errMessage);
				procedure.ZZ6_Group = "AAA,CCC";
				line.Validation.ValidateJI_Procedure();
				AssertNoMessageError("declarationType matched AAA,CCC", line.JI_ProcedureInfo, errMessage);
				line.JI_Procedure = null;
				line.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Procedure is null", line.JI_ProcedureInfo, errMessage);
			});
		}

		void SetUpTestDataForValidation()
		{
			var inProcedure = Factory.New<RefCusProcedure>();
			inProcedure.ZZ6_ProcedureCode = "AB";
			inProcedure.ZZ6_PreviousProcedureCode = "10";
			inProcedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Botswana;
			inProcedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			inProcedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			inProcedure.ZZ6_Description = "in procedure";
			var outProcedure = Factory.New<RefCusProcedure>();
			outProcedure.ZZ6_ProcedureCode = "CD";
			outProcedure.ZZ6_PreviousProcedureCode = "11";
			outProcedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Botswana;
			outProcedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			outProcedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			outProcedure.ZZ6_Description = "out procedure";
			helper = new WhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var impDeclaration = Factory.New<JobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			impDeclaration.JE_OH_Importer = helper.Importer.PK;
			var inWhsInvoice = impDeclaration.Invoices.AddNew();
			inWhsInvoiceLine = inWhsInvoice.JobComInvoiceLines.AddNew();
			inWhsInvoiceLine.JI_Procedure = "AB10";
			var expDeclaration = Factory.New<JobDeclaration>();
			expDeclaration.JE_MessageType = "EXP";
			expDeclaration.JE_OH_Supplier = helper.Supplier.PK;
			var outWhsInvoice = expDeclaration.Invoices.AddNew();
			outWhsInvoiceLine = outWhsInvoice.JobComInvoiceLines.AddNew();
			outWhsInvoiceLine.JI_Procedure = "CD11";
		}

		WhsDataTestHelper helper;
		JobComInvoiceLine inWhsInvoiceLine, outWhsInvoiceLine;
	}
}
