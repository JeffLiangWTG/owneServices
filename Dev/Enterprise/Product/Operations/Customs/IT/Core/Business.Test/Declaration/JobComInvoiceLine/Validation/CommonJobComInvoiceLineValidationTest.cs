using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class CommonJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJI_Procedure_AgainstEntryInstructionProcedureCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;

		entryInstruction.CEI_Procedure = "12";
		invoiceLine1.JI_Procedure = "4000";
		AssertHasMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "CPC invalid against related entry instruction. Expected starting with '12'");

		entryInstruction.CEI_Procedure = "40";
		invoiceLine1.JI_Procedure = "4000";
		AssertNoMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "CPC invalid against related entry instruction. Expected starting with '40'");

		entryInstruction.CEI_Procedure = "40";
		invoiceLine1.JI_Procedure = "5100";
		AssertHasMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "CPC invalid against related entry instruction. Expected starting with '40'");

		entryInstruction.CEI_Procedure = "10";
		invoiceLine1.JI_Procedure = "4000";
		AssertHasMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "CPC invalid against related entry instruction. Expected starting with '10'");

		entryInstruction.CEI_Procedure = "10";
		invoiceLine1.JI_Procedure = "";
		AssertNoMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "CPC invalid against related entry instruction. Expected starting with '10'");

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_Procedure = "1010";
		AssertNoMessageErrorContaining(invoiceLine2.JI_ProcedureInfo, "CPC invalid against related entry instruction. Expected starting with '10'");
	}

	public void TestCheckJI_Procedure_ProcedureLimitDate()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invLine1 = invoice.InvoiceLines.AddNew();
		invLine1.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			Assert("ZG_TempProcLimitDate not set", entryInstruction.ZG_TempProcLimitDate.IsEmpty);

			testDataHelper.CreateNewRefCusProcedure(procedureCode: "1201002", shipmentType: "IMP");
			entryInstruction.CEI_Procedure = "12";
			invLine1.JI_Procedure = "1201002";
			invLine1.Validation.ValidateJI_Procedure();
			AssertNoMessageError("JI_Procedure = 1201002", invLine1.JI_ProcedureInfo, "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");

			testDataHelper.CreateNewRefCusProcedure(procedureCode: "2201002", shipmentType: "IMP", configurationAction: x => x.ZZ6_IntoTemporaryImport = "Y");
			entryInstruction.CEI_Procedure = "22";
			invLine1.JI_Procedure = "2201002";
			invLine1.Validation.ValidateJI_Procedure();
			AssertHasMessageError("JI_Procedure = 2201002", invLine1.JI_ProcedureInfo, "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");

			entryInstruction.ZG_TempProcLimitDate = ZDate.BrettsBirthday;
			invLine1.Validation.ValidateJI_Procedure();
			AssertNoMessageError("JI_Procedure = 2201002 and ZG is set", invLine1.JI_ProcedureInfo, "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");

			entryInstruction.ZG_TempProcLimitDate = ZDate.Empty;

			testDataHelper.CreateNewRefCusProcedure(procedureCode: "3201002", shipmentType: "IMP");
			entryInstruction.CEI_Procedure = "32";
			invLine1.JI_Procedure = "3201002";
			invLine1.Validation.ValidateJI_Procedure();
			AssertNoMessageError("JI_Procedure = 3201002", invLine1.JI_ProcedureInfo, "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");

			testDataHelper.CreateNewRefCusProcedure(procedureCode: "4201002", shipmentType: "IMP");
			entryInstruction.CEI_Procedure = "42";
			invLine1.JI_Procedure = "4201002";
			invLine1.Validation.ValidateJI_Procedure();
			AssertNoMessageError("JI_Procedure = 4201002", invLine1.JI_ProcedureInfo, "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");

			testDataHelper.CreateNewRefCusProcedure(procedureCode: "5201002", shipmentType: "IMP", configurationAction: x => x.ZZ6_IntoTemporaryImport = "Y");
			entryInstruction.CEI_Procedure = "52";
			invLine1.JI_Procedure = "5201002";
			invLine1.Validation.ValidateJI_Procedure();
			AssertHasMessageError("JI_Procedure = 5201002", invLine1.JI_ProcedureInfo, "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");

			entryInstruction.ZG_TempProcLimitDate = ZDate.BrettsBirthday;

			invLine1.Validation.ValidateJI_Procedure();
			AssertNoMessageError("JI_Procedure = 5201002 and ZG is set", invLine1.JI_ProcedureInfo, "Entry Instruction -> Temporary Procedure Limit Date is required when the CPC starts with '2' or '5'.");
		});
	}

	public void TestCheckJI_Procedure_4400WithSupportingDocument_N990() => AssertInvoiceLineWithProcedureCodeAndSupportingDocument(procedureCode: "4400", supportingDocuement: "N990");

	public void TestCheckJI_Procedure_4451WithSupportingDocument_N990() => AssertInvoiceLineWithProcedureCodeAndSupportingDocument(procedureCode: "4451", supportingDocuement: "N990");

	public void TestCheckJI_Procedure_4453WithSupportingDocument_N990() => AssertInvoiceLineWithProcedureCodeAndSupportingDocument(procedureCode: "4453", supportingDocuement: "N990");

	public void TestCheckJI_Procedure_4471WithSupportingDocument_N990() => AssertInvoiceLineWithProcedureCodeAndSupportingDocument(procedureCode: "4471", supportingDocuement: "N990");

	public void TestCheckJI_Procedure_6171F01WithSupportingDocument_32YY() => AssertInvoiceLineWithProcedureCodeAndSupportingDocument(procedureCode: "6171F01", supportingDocuement: "32YY");

	public void TestCheckJI_CustomsSecondUnitQty()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffTypeImp = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, TariffTypes.Import);
		var tariffTypeExp = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, TariffTypes.Export);
		Factory.Save();

		var tariffMultiUomImp = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffTypeImp.PK, "10000010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffMultiUomImp, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariffUOM(tariffMultiUomImp, UnitOfMeasureTypes.AdditionalUOMType, "XXX");
		var tariffSingleUomImp = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffTypeImp.PK, "1000020", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffSingleUomImp, UnitOfMeasureTypes.AdditionalUOMType, "SSS");
		var tariffNoneUomImp = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffTypeImp.PK, "10000030", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var tariffMultiUomExp = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffTypeExp.PK, "10000011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffMultiUomExp, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariffUOM(tariffMultiUomExp, UnitOfMeasureTypes.AdditionalUOMType, "XXX");
		var tariffSingleUomExp = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffTypeExp.PK, "10000021", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffSingleUomExp, UnitOfMeasureTypes.AdditionalUOMType, "SSS");
		var tariffNoneUomExp = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffTypeExp.PK, "10000031", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		var tariffRequiresSupplementaryUOM = "The selected tariff requires a supplementary quantity unit of measure";
		var uomIsNotInTheList = "The selected supplementary quantity unit of measure is different from those available for the selected tariff: NAR, XXX";
		var theSelectedTariffHasNoSupQuantity = "The selected tariff has no supplementary quantity unit of measure";
		var uomIsMandatoryExpectedError = "You have entered a Supplementary Quantity without a Unit of Measure";

		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions("JI_CustomsSecondUnitQty Message error", () =>
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			invoiceLine.JI_Tariff = "10000010";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			AssertHasMessageError("The tariff needs a supplementary quantity UOM", invoiceLine.JI_CustomsSecondUnitQtyInfo, tariffRequiresSupplementaryUOM);

			invoiceLine.JI_Tariff = "10000010";
			invoiceLine.JI_CustomsSecondUnitQty = "NAR";
			AssertNoMessageError("The supplementary quantity UOM is valid for the tariff", invoiceLine.JI_CustomsSecondUnitQtyInfo, tariffRequiresSupplementaryUOM);

			invoiceLine.JI_Tariff = "10000010";
			invoiceLine.JI_CustomsSecondUnitQty = "XXX";
			AssertNoMessageError("The supplementary quantity UOM is valid for the tariff", invoiceLine.JI_CustomsSecondUnitQtyInfo, tariffRequiresSupplementaryUOM);

			invoiceLine.JI_Tariff = "10000010";
			invoiceLine.JI_CustomsSecondUnitQty = "ZZZ";
			AssertHasMessageError("The supplementary quantity UOM is NOT valid for the tariff", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsNotInTheList);

			invoiceLine.JI_Tariff = "10000030";
			invoiceLine.JI_CustomsSecondUnitQty = "ZZZ";
			AssertHasMessageError("The tariff has no supplementary quantity UOMs", invoiceLine.JI_CustomsSecondUnitQtyInfo, theSelectedTariffHasNoSupQuantity);

			invoiceLine.JI_CustomsSecondQuantity = 10;
			invoiceLine.JI_CustomsSecondUnitQty = "";
			AssertHasError("When Supplementary Quantity is set and Supplementary Unit UOM is empty, an error is expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);

			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsSecondUnitQty = "";
			AssertNoError("When Supplementary Quantity is not set and Supplementary Unit UOM is empty, no error expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);

			invoiceLine.JI_CustomsSecondUnitQty = "NAR";
			AssertNoError("When Supplementary Quantity is not set and Supplementary Unit UOM is set, no error expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);

			invoiceLine.JI_CustomsSecondQuantity = 10;
			invoiceLine.JI_CustomsSecondUnitQty = "TJO";
			AssertNoError("When Supplementary Quantity is set and Supplementary Unit UOM is set, no error expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);

			declaration.JE_MessageType = "EXP";

			invoiceLine.JI_Tariff = "10000011";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			AssertHasMessageError("The tariff needs a supplementary quantity UOM", invoiceLine.JI_CustomsSecondUnitQtyInfo, tariffRequiresSupplementaryUOM);

			invoiceLine.JI_Tariff = "10000011";
			invoiceLine.JI_CustomsSecondUnitQty = "NAR";
			AssertNoMessageError("The supplementary quantity UOM is valid for the tariff", invoiceLine.JI_CustomsSecondUnitQtyInfo, tariffRequiresSupplementaryUOM);

			invoiceLine.JI_Tariff = "10000011";
			invoiceLine.JI_CustomsSecondUnitQty = "XXX";
			AssertNoMessageError("The supplementary quantity UOM is valid for the tariff", invoiceLine.JI_CustomsSecondUnitQtyInfo, tariffRequiresSupplementaryUOM);

			invoiceLine.JI_Tariff = "10000011";
			invoiceLine.JI_CustomsSecondUnitQty = "ZZZ";
			AssertHasMessageError("The supplementary quantity UOM is NOT valid for the tariff", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsNotInTheList);

			invoiceLine.JI_Tariff = "10000031";
			invoiceLine.JI_CustomsSecondUnitQty = "ZZZ";
			AssertHasMessageError("The tariff has no supplementary quantity UOMs", invoiceLine.JI_CustomsSecondUnitQtyInfo, theSelectedTariffHasNoSupQuantity);

			invoiceLine.JI_CustomsSecondQuantity = 10;
			invoiceLine.JI_CustomsSecondUnitQty = "";
			AssertHasError("When Supplementary Quantity is set and Supplementary Unit UOM is empty, an error is expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);

			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsSecondUnitQty = "";
			AssertNoError("When Supplementary Quantity is not set and Supplementary Unit UOM is empty, no error expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);

			invoiceLine.JI_CustomsSecondUnitQty = "NAR";
			AssertNoError("When Supplementary Quantity is not set and Supplementary Unit UOM is set, no error expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);

			invoiceLine.JI_CustomsSecondQuantity = 10;
			invoiceLine.JI_CustomsSecondUnitQty = "TJO";
			AssertNoError("When Supplementary Quantity is set and Supplementary Unit UOM is set, no error expected", invoiceLine.JI_CustomsSecondUnitQtyInfo, uomIsMandatoryExpectedError);
		});
	}

	public void TestCheckJI_CustomsThirdUnitQty()
	{
		var tariffType = UniversalReferenceHelperTest.SetupTariffForTestintQuantityUOMs(Factory, "EXP");
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
		invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Australia;

		var thirdUnitQtyIsRequired = "Insert a Unit of Measure for third quantity";
		var uomIsNotInTheList = "The selected third quantity unit of measure is different from those available for the selected tariff: DTNZ";

		invoiceLine.JI_Tariff = "1702907100";
		invoiceLine.JI_CustomsThirdQuantity = 100;

		void AssertCustomsThirdUnitQty()
		{
			CombineAssertions("CustomsThirdUnitQty Has Message error", () =>
			{
				invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
				AssertHasMessageErrorContaining("The tariff needs a third quantity UOM", invoiceLine.JI_CustomsThirdUnitQtyInfo, thirdUnitQtyIsRequired);

				invoiceLine.JI_CustomsThirdUnitQty = "DTNL";
				AssertNoMessageErrorContaining("The tariff needs a third quantity UOM", invoiceLine.JI_CustomsThirdUnitQtyInfo, thirdUnitQtyIsRequired);
				AssertHasMessageErrorContaining("The third quantity UOM is NOT valid for the tariff", invoiceLine.JI_CustomsThirdUnitQtyInfo, uomIsNotInTheList);

				invoiceLine.JI_CustomsThirdUnitQty = "DTNZ";
				AssertNoMessageErrorContaining("Third quantity UOM: [DTNZ] should be valid for the selected tariff", invoiceLine.JI_CustomsThirdUnitQtyInfo, uomIsNotInTheList);
			});
		}

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		AssertCustomsThirdUnitQty();

		Factory.ClearCachedValue<ZString>("UniversalTariff_SupplementaryQuantityUOMs_1702907100");
		Factory.ClearQueryCache();
		var testTariff = Factory.LoadTop1<TariffView>(new ZQuery(ZArchitecture.Schema.TariffViewSchema.ZZ1_TariffCode, "1702907100"));
		tariffType.ZZI_TariffType = TariffTypes.Import;
		Factory.Save();
		testTariff.ZZ1_ZZI_NKTariffType = TariffTypes.Import;
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertCustomsThirdUnitQty();
	}

	public void TestCheckJI_CustomsQuantity()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CheckInvoiceLineMandatoryField(invoiceLine.JI_CustomsQuantityInfo);
	}

	protected void CheckInvoiceLineMandatoryField(ZPropertyInfo mandatoryFieldPropertyInfo)
	{
		mandatoryFieldPropertyInfo.Value = ZDecimal.Zero;
		AssertHasMessageErrorContaining(mandatoryFieldPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		mandatoryFieldPropertyInfo.Value = new ZDecimal(1m);
		AssertNoMessageErrorContaining(mandatoryFieldPropertyInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJI_CustomsQuantity_EqualToNetWeightWarning()
	{
		var expectedWarningMessage = "Customs Quantity is usually equal to Net Weight";

		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_NetWeight = 1000;
		invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
		invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		invoiceLine.JI_CustomsQuantity = 1000;
		AssertEquals("NetWeightInKG", 1m, invoiceLine.NetWeightInKG);
		AssertEquals("CustomsWeightInKG", 1000m, invoiceLine.CustomsFirstQuantityInKG);
		AssertHasWarning("1000G != 1000KG", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);

		invoiceLine.JI_CustomsQuantity = 1m;
		AssertNoWarning("1000G == 1KG", invoiceLine.JI_CustomsQuantityInfo, expectedWarningMessage);
	}

	public void TestCheckAdditionalInfoDescription()
	{
		var expectedWarning = ValidationCaptions.InvoiceLine.TotalLengthOfRemarksExceedsTheMaximumAllowedLengthInTheMessage(SADConstants.CustomsFieldMaxLength.EntryLine.Notes);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package1 = declaration.Packages.AddNew();
		package1.CW_CR_HouseContainer = billPackingGroup.PK;
		var packageInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
		packageInvoiceLine1.CHC_CW = package1.PK;

		package1.CW_MarksAndNos = "";
		invoiceLine1.Remarks = "this is an additional info";
		invoiceLine2.Remarks = "this is another additional info";
		AssertLessThanOrEqualTo(entryLine.LineNotes.Length, 500);
		AssertNoWarningContaining(invoiceLine1.RemarksInfo, expectedWarning);
		AssertNoWarningContaining(invoiceLine2.RemarksInfo, expectedWarning);

		package1.CW_MarksAndNos = new string('0', 200);
		invoiceLine1.Remarks = new string('1', 254);
		invoiceLine2.Remarks = new string('1', 254);
		AssertLessThanOrEqualTo(entryLine.LineNotes.Length, 500);
		AssertNoWarningContaining(invoiceLine1.RemarksInfo, expectedWarning);
		AssertNoWarningContaining(invoiceLine2.RemarksInfo, expectedWarning);

		package1.CW_MarksAndNos = new string('0', 300);
		invoiceLine1.Remarks = new string('1', 201);
		invoiceLine2.Remarks = "";
		AssertGreaterThan(entryLine.LineNotes.Length, 500);
		AssertHasWarningContaining(invoiceLine1.RemarksInfo, expectedWarning);
		AssertHasWarningContaining(invoiceLine2.RemarksInfo, expectedWarning);

		invoiceLine1.JI_CL = ZGuid.Empty;
		invoiceLine2.JI_CL = ZGuid.Empty;
		AssertNull(invoiceLine1.CusEntryLine);
		AssertNull(invoiceLine2.CusEntryLine);
		invoiceLine1.Validation.ValidateRemarks();
		invoiceLine2.Validation.ValidateRemarks();
		AssertNoWarningContaining(invoiceLine1.RemarksInfo, expectedWarning);
		AssertNoWarningContaining(invoiceLine2.RemarksInfo, expectedWarning);
	}

	public void TestCheckJI_WeightUQ_ListValidation()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_WeightUQ = ZString.Empty;
		AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
		AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_WeightUQ = "XX";
		AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckJI_NetWeightUQ_ListValidation()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_NetWeightUQ = ZString.Empty;
		AssertNoMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
		AssertNoMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

		invoiceLine.JI_NetWeightUQ = "XX";
		AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestValidateCustomsDecisionsSupportingDocument()
	{
		var expectedMessageError = "For Inward Processing procedure add document of type C601";

		var (_, importer, authorisation, _) = InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.SetupAndGetDataForDefaultingSupportingDocument(Factory, "AUTH_NUMBER", "IPO");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		declaration.JE_OH_Importer = importer.PK;
		entryInstruction.CEI_Procedure = "51";
		invoiceLine.JI_CEI = entryInstruction.PK;

		invoiceLine.JI_Procedure = "5100";
		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();

		invoiceLine.Validation.ValidateAll();
		AssertEquals($"Invoice Line should have following message error: {expectedMessageError}", true, invoiceLine.RowMessageErrors.Any(x => x.Message == expectedMessageError));

		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "C601";
		invoiceLine.Validation.ValidateAll();
		AssertEquals($"Invoice Line should not have the message error: {expectedMessageError}", false, invoiceLine.RowMessageErrors.Any(x => x.Message == expectedMessageError));
	}

	public void TestCheckJI_PartNo_ToWarehouseBond_Import() => SetupAndAssertPartNoWithWarehouseBondAndProcedure(messageType: "IMP", isToWarehouse: true);

	public void TestCheckJI_PartNo_FromWarehouseBond_Import() => SetupAndAssertPartNoWithWarehouseBondAndProcedure(messageType: "IMP", isToWarehouse: false);

	public void TestCheckJI_PartNo_ToWarehouseBond_Export() => SetupAndAssertPartNoWithWarehouseBondAndProcedure(messageType: "EXP", isToWarehouse: true);

	public void TestCheckJI_PartNo_FromWarehouseBond_Export() => SetupAndAssertPartNoWithWarehouseBondAndProcedure(messageType: "EXP", isToWarehouse: false);

	public void TestValidatePreviousDocumentsCount()
	{
		const string expectedMessageError = "The total number of previous documents in the Entry Line No. 2 which this Invoice Line is linked  exceeds the maximum allowed of 99.";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		var entryHeader1 = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 2;
		invoiceLine1.JI_CL = entryLine1.PK;

		CombineAssertions("When Import", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			AddPreviousDocuments(invoiceLine1, numberOfDocuments: 99, packageType: "CT", referenceNumber: "IT-2024-01", documentType: "IP2", unitOfQuantity: "KG", lineNo: null);
			invoiceLine1.Validation.ValidateAll();

			AssertNoRowMessageError("Only 99 distinct Previous Documents", invoiceLine1, expectedMessageError);

			AddPreviousDocuments(invoiceLine1, numberOfDocuments: 2, packageType: "CT", referenceNumber: "IT-2024-01", documentType: "IP2", unitOfQuantity: "KG", lineNo: 2);

			invoiceLine1.Validation.ValidateAll();
			AssertNoRowMessageError("After adding two new previous documents with the OLD aggregate key, Only 99 distinct Previous Documents", invoiceLine1, expectedMessageError);

			AddPreviousDocuments(invoiceLine1, numberOfDocuments: 1, packageType: "CT", referenceNumber: "IT-2024-02", documentType: "IP2", unitOfQuantity: "KG", lineNo: 2);
			invoiceLine1.Validation.ValidateAll();

			AssertHasRowMessageError("After adding new previous documents with a NEW aggregate key, the count of distinct Previous Documents exceeds 99", invoiceLine1, expectedMessageError);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		invoiceLine1.Validation.ValidateAll();

		AssertNoRowMessageError("When export, and count of distinct Previous Documents exceeds 99", invoiceLine1, expectedMessageError);
	}

	#region Implementation

	void AddPreviousDocuments(IPreviousDocumentsProvider invoiceLine, int numberOfDocuments, string packageType, string referenceNumber, string documentType, string unitOfQuantity, int? lineNo)
	{
		for (var i = 0; i < numberOfDocuments; i++)
		{
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_LineNo = lineNo ?? i;
			previousDocument.CSI_PackType = packageType;
			previousDocument.CSI_ReferenceNumber = referenceNumber;
			previousDocument.CSI_Code = documentType;
			previousDocument.CSI_UnitOfQuantity3 = unitOfQuantity;
			previousDocument.CSI_Quantity3 = 1;
		}
	}

	void AssertInvoiceLineWithProcedureCodeAndSupportingDocument(string procedureCode, string supportingDocuement)
	{
		var expectedMessage = $"A Supporting document with code '{supportingDocuement}' must be present for the Procedure Code inserted";
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = procedureCode;

		AssertNoMessageErrorContaining("Before merge without supporting document", invoiceLine.JI_ProcedureInfo, expectedMessage);

		declaration.DoMerge();
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertHasMessageErrorContaining("After merge without supporting document", invoiceLine.JI_ProcedureInfo, expectedMessage);

		invoiceLine.SupportingDocuments.AddNew(supportingDocuement, "1234");
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageErrorContaining("After merge with supporting document", invoiceLine.JI_ProcedureInfo, expectedMessage);
	}

	void SetupAndAssertPartNoWithWarehouseBondAndProcedure(string messageType, bool isToWarehouse)
	{
		const string expectedMessage = "You have not entered a Product Code, inventory cannot be updated.";
		var (entryInstruction, invoiceLine, procedure, orgAddressBond, orgAddress) = SetupDataForPartNoBondWarehouseAndProcedure(messageType);

		var setEntryInstructionWarehouse = isToWarehouse
			? new Action<ZGuid>(pk => entryInstruction.CEI_OA_Warehouse2 = pk)
			: new Action<ZGuid>(pk => entryInstruction.CEI_OA_Warehouse = pk);

		var setProcedureDirection = isToWarehouse
			? new Action<string>(dir => procedure.ZZ6_IntoWarehouse = dir)
			: new Action<string>(dir => procedure.ZZ6_OutOfWarehouse = dir);

		var assertionMessageWarehouse = isToWarehouse
			? "ToWarehouse"
			: "FromWarehouse";

		var assertionMessageDirection = isToWarehouse
			? "Into"
			: "Out of";

		CombineAssertions(() =>
		{
			setProcedureDirection("Y");
			setEntryInstructionWarehouse(orgAddressBond.PK);
			invoiceLine.JI_FormattedProcedure = procedure.FullCodeCurrentPlusPreviousPlusConcession;

			invoiceLine.Validation.ValidateJI_PartNo();
			AssertHasMessageError($"When PartNo empty, {assertionMessageWarehouse} WhsArea Type = BON and {assertionMessageDirection} warehouse = Y", invoiceLine.JI_PartNoInfo, expectedMessage);

			invoiceLine.JI_PartNo = "ABC";
			AssertNoMessageError($"When PartNo filled, {assertionMessageWarehouse} WhsArea Type = BON and {assertionMessageDirection} warehouse = Y", invoiceLine.JI_PartNoInfo, expectedMessage);

			invoiceLine.JI_PartNo = "";
			setEntryInstructionWarehouse(orgAddress.PK);
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoMessageError($"When PartNo empty, {assertionMessageWarehouse} WhsArea Type not BON and {assertionMessageDirection} warehouse = Y", invoiceLine.JI_PartNoInfo, expectedMessage);

			setEntryInstructionWarehouse(orgAddressBond.PK);
			setProcedureDirection("N");
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoMessageError($"When PartNo empty, {assertionMessageWarehouse} WhsArea Type = BON and {assertionMessageDirection} warehouse = N", invoiceLine.JI_PartNoInfo, expectedMessage);
		});
	}

	(CusEntryInstruction, JobComInvoiceLine, RefCusProcedure, OrgAddress orgAddressBond, OrgAddress orgAddress) SetupDataForPartNoBondWarehouseAndProcedure(string messageType)
	{
		var orgHeaderBond = Factory.New<OrgHeader>();
		var orgAddressBond = orgHeaderBond.MainAddress;

		var helper = new WhsDataTestHelper(Factory);
		helper.GetNewWhsWarehouse(orgAddressBond.PK, true, "B01", "PRW");

		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.MainAddress;

		var refDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		var procedure = refDataHelper.CreateRefCusProcedure("IT", "A", "18", "00", "", "Test", messageType);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew()
			.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		return (entryInstruction, invoiceLine, procedure, orgAddressBond, orgAddress);
	}

	#endregion
}
