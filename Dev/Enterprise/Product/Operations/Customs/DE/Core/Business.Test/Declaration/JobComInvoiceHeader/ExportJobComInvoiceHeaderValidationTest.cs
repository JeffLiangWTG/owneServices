using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_RX_NKInvoice_CurrencyNotMandatory()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_InvoiceAmount = 0m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
				AssertNoMessageErrorContaining("Empty currency, empty amount", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.MustBeEntered);

				invoiceHeader.JZ_InvoiceAmount = 100m;
				invoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
				AssertHasMessageErrorContaining("Empty currency, has amount", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.MustBeEntered);

				invoiceHeader.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
				AssertNoMessageErrorContaining("Has currency, has amount", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.MustBeEntered);

				invoiceHeader.JZ_InvoiceAmount = 0m;
				invoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
				AssertNoMessageErrorContaining("Has currency, empty amount", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckShouldValidateNeedAtLeastOneInvoiceSupportingDocumentFlag()
		{
			var exportJobComInvHeaderValidator = new ExportJobComInvoiceHeaderValidationForTest(invoiceHeader);
			AssertEquals("ShouldValidateNeedAtLeastOneInvoiceSupportingDocument flag should be false", false, exportJobComInvHeaderValidator.ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentExposed);
		}

		public void TestCheckJZ_Weight_MaxValue()
		{
			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilotonnes;
			invoiceHeader.JZ_Weight = 99999;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_WeightInfo, "Invoice Gross Weight must not exceed 99999999999 KG.");

			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilotonnes;
			invoiceHeader.JZ_Weight = 100000;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_WeightInfo, "Invoice Gross Weight must not exceed 99999999999 KG.");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_WeightInfo, "Invoice Gross Weight must not exceed 99999999999 KG.");
		}

		public void TestCheckJZ_Weight_InvoiceLineGrossWeightSum()
		{
			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_Weight = 4m;
			invLine1.JI_WeightUQ = Core.Constants.Weight.Pounds;

			var invLine2 = invoiceHeader.InvoiceLines.AddNew();
			invLine2.JI_Weight = 2m;
			invLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			var invLine3 = invoiceHeader.InvoiceLines.AddNew();
			invLine3.JI_Weight = 3m;
			invLine3.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			invoiceHeader.JZ_Weight = 6m;
			AssertHasMessageError(invoiceHeader.JZ_WeightInfo, "Invoice Gross Weight must not be less than the sum of all related invoice line's [35] GWT. (6.814369 KG)");

			invoiceHeader.JZ_Weight = 6.815m;
			AssertNoMessageError(invoiceHeader.JZ_WeightInfo, "Invoice Gross Weight must not be less than the sum of all related invoice line's [35] GWT. (6.814369 KG)");
		}

		public void TestCheckJZ_Weight_InvoiceLineGrossWeightSum_InvalidUQ()
		{
			var invLine = invoiceHeader.InvoiceLines.AddNew();
			invLine.JI_Weight = 10m;
			invLine.JI_WeightUQ = "$";

			invoiceHeader.JZ_Weight = 5m;
			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertNoMessageErrors("Should not validate the gross weight against the invoice lines gross weight total if an invoice line has an invalid UQ", invoiceHeader.JZ_WeightInfo);

			invLine.JI_Weight = 10m;
			invLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			invoiceHeader.JZ_Weight = 5m;
			invoiceHeader.JZ_WeightUQ = "$";

			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertNoMessageErrors("Should not validate the gross weight against the invoice lines gross weight total if the gross weight has an invalid UQ", invoiceHeader.JZ_WeightInfo);
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			const string message = "This invoice has no previous documents and not all of its invoice lines have one. Without a previous document the entry may be rejected.";
			invoiceHeader.JZ_InvoiceNumber = "1";
			AssertHasWarning("Export declaration requires previous documents", invoiceHeader.JZ_InvoiceNumberInfo, message);
		}

		public void TestCheckJZ_ValuationCode_Style4thDigitIs4()
		{
			CreateValuationRefCusCodeList();

			const string message = "For the selected Type (Procedure) the Transaction Nature code must be from code list C0091.";
			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
				invoiceHeader.JZ_ValuationCode = "02";
				AssertNoMessageError("Invoice isn't linked to an Entry Instruction", invoiceHeader.JZ_ValuationCodeInfo, message);

				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceHeader.Validation.ValidateJZ_ValuationCode();
				AssertHasMessageError("Invoice is linked to an Entry Instruction", invoiceHeader.JZ_ValuationCodeInfo, message);

				invoiceHeader.JZ_ValuationCode = "01";
				AssertNoMessageError("Has attribute C0091", invoiceHeader.JZ_ValuationCodeInfo, message);
			});
		}

		public void TestCheckJZ_ValuationCode_Style4thDigitIsNot4()
		{
			CreateValuationRefCusCodeList();

			const string message = "For the selected Type (Procedure) the Transaction Nature code must be from code list A1150.";
			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
				invoiceHeader.JZ_ValuationCode = "01";
				AssertNoMessageError("Invoice isn't linked to an Entry Instruction", invoiceHeader.JZ_ValuationCodeInfo, message);

				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceHeader.Validation.ValidateJZ_ValuationCode();
				AssertHasMessageError("Invoice is linked to an Entry Instruction", invoiceHeader.JZ_ValuationCodeInfo, message);

				invoiceHeader.JZ_ValuationCode = "02";
				AssertNoMessageError("Has attribute A1150", invoiceHeader.JZ_ValuationCodeInfo, message);
			});
		}

		public void TestCheckJZ_ValuationCode_MultipleInstructions()
		{
			var helper = CreateValuationRefCusCodeList();
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "03", "03 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codeList3.PK, RefCusCodeListAttributes.Name.C0091, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListAttribute(codeList3.PK, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			const string message1 = "For the selected Type (Procedure) the Transaction Nature code must be from code list C0091.";
			const string message2 = "For the selected Type (Procedure) the Transaction Nature code must be from code list A1150.";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;

			CombineAssertions(() =>
			{
				invoiceHeader.JZ_ValuationCode = "01";
				AssertHasMessageError("Only has attribute C0091", invoiceHeader.JZ_ValuationCodeInfo, message2);

				invoiceHeader.JZ_ValuationCode = "02";
				AssertHasMessageError("Only has attribute A1150", invoiceHeader.JZ_ValuationCodeInfo, message1);

				invoiceHeader.JZ_ValuationCode = "03";
				AssertNoMessageError("Has attributes C0091 and A1150, no message1", invoiceHeader.JZ_ValuationCodeInfo, message1);
				AssertNoMessageError("Has attributes C0091 and A1150, no message2", invoiceHeader.JZ_ValuationCodeInfo, message2);
			});
		}

		public void TestCheckJZ_ValuationCode_Mandatory()
		{
			const string message = "You have not entered a Transaction Nature.";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_ValuationCodeInfo, message, "Mandatory");

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000210;
				ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_ValuationCodeInfo, message, "Not mandatory");
			});
		}

		public void TestShouldValidateWaterIncoTermAndTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			CombineAssertions(() =>
			{
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				AssertEquals("FOB", expected: false, invoiceHeader.Validation.ShouldValidateWaterIncoTermAndTransportMode);

				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
				AssertEquals("CFR", expected: false, invoiceHeader.Validation.ShouldValidateWaterIncoTermAndTransportMode);

				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				AssertEquals("CIF", expected: false, invoiceHeader.Validation.ShouldValidateWaterIncoTermAndTransportMode);

				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeAlongsideShip;
				AssertEquals("FAS", expected: true, invoiceHeader.Validation.ShouldValidateWaterIncoTermAndTransportMode);
			});
		}

		public override void TestValidateBalance()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_InvoiceAmount = 100.121m;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_InvoiceQuantity = 1;
				invoiceLine.JI_InvoiceUQ = "NO";
				invoiceLine.JI_LinePrice = 100.124m;
				declaration.ResumeApportionment();
				AssertNoNotifications(invoiceHeader.JZ_Calc_BalanceInfo);
				invoiceLine.JI_LinePrice = 100.128m;
				declaration.ResumeApportionment();
				AssertHasWarning(invoiceHeader.JZ_Calc_BalanceInfo, "The total of all invoice lines does not equal the invoice total.");
			});
		}

		protected override Type GetTypeForTest() => typeof(ExportJobComInvoiceHeaderValidation);

		protected override string MessageType => MessageTypeList.Codes.Export;

		UniversalReferenceTestDataHelper CreateValuationRefCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TranNature");

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"01", "01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.C0091, RefCusCodeListAttributes.Value.Yes);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature,
				"02", "02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Name.A1150, RefCusCodeListAttributes.Value.Yes);
			Factory.Save();

			return helper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var procedure = Factory.NewWithValidTestData<RefCusProcedure>();
			isOutOfWarehouseWarehousingProcedureCode = "4071";
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			procedure.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_ProcedureCode = isOutOfWarehouseWarehousingProcedureCode.Left(2);
			procedure.ZZ6_Concession = isOutOfWarehouseWarehousingProcedureCode.PadRight(7).Right(3);
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_PreviousProcedureCode = "71";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
		ZString isOutOfWarehouseWarehousingProcedureCode;

		class ExportJobComInvoiceHeaderValidationForTest : ExportJobComInvoiceHeaderValidation
		{
			public ExportJobComInvoiceHeaderValidationForTest(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
			{
			}

			public bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentExposed => ShouldValidateNeedAtLeastOneInvoiceSupportingDocument;
		}
	}
}
