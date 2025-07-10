using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseFineCalculatorTest : TestCaseWithFactory
	{
		public void TestUpdateImportLicenseFineOnEntryLine()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			var date = ZDateTime.Now;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ExportDate = date.AddDays(-5);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 1500m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.ImportLicenseNumber = "123";
			invoiceLine1.ImportLicenseAuthorizationDate = date;
			invoiceLine1.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
			invoiceLine1.ImportLicenseFeeType = "F1D5";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_NetWeight = 50m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.ImportLicenseNumber = "123";
			invoiceLine2.ImportLicenseAuthorizationDate = date;
			invoiceLine2.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
			invoiceLine2.ImportLicenseFeeType = "F1D5";

			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			entryLine.CL_CustomsValue = 2000m;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			declaration.ResumeApportionment();
			new ImportLicenseFineCalculator(entryLine).UpdateImportLicenseFineOnEntryLine();
			var fee = entryLine.Fees.GetElementWithThisCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeAmount for F1D5 should be equal to 300", 300m, fee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue for F1D5 should be equal to 2000", 2000m, fee.CF_BaseValue);
				AssertEquals("CF_Rate for F1D5 should be equal to 15", 15m, fee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation for F1D5 should be equal to 50%", "50%", fee.CF_MethodOfCalculation);
			});

			invoiceLine1.ImportLicenseFeeType = "F1ND";
			invoiceLine2.ImportLicenseFeeType = "F1ND";

			new ImportLicenseFineCalculator(entryLine).UpdateImportLicenseFineOnEntryLine();
			fee = entryLine.Fees.GetElementWithThisCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeAmount for F1ND should be equal to 600", 600m, fee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue for F1ND should be equal to 2000", 2000m, fee.CF_BaseValue);
				AssertEquals("CF_Rate for F1ND should be equal to 30", 30m, fee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation for F1ND should be equal to %", "%", fee.CF_MethodOfCalculation);
			});
		}

		public void TestUpdateImportLicenseFineOnEntryLineLowerThanMinimum()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			var date = ZDateTime.Now;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ExportDate = date.AddDays(-5);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 80m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.ImportLicenseNumber = "123";
			invoiceLine1.ImportLicenseAuthorizationDate = date;
			invoiceLine1.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
			invoiceLine1.ImportLicenseFeeType = "F1D5";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 20m;
			invoiceLine2.JI_NetWeight = 50m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.ImportLicenseNumber = "123";
			invoiceLine2.ImportLicenseAuthorizationDate = date;
			invoiceLine2.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
			invoiceLine2.ImportLicenseFeeType = "F1D5";

			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100m;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			declaration.ResumeApportionment();
			new ImportLicenseFineCalculator(entryLine).UpdateImportLicenseFineOnEntryLine();

			var fee = entryLine.Fees.GetElementWithThisCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode);
			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeAmount for F1D5 should be equal to 250", 250m, fee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue for F1D5 should be equal to 100", 100m, fee.CF_BaseValue);
				AssertEquals("CF_Rate for F1D5 should be equal to 15", 15m, fee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation for F1D5 should be equal to 50%", "50%", fee.CF_MethodOfCalculation);
			});

			invoiceLine1.ImportLicenseFeeType = "F1ND";
			invoiceLine2.ImportLicenseFeeType = "F1ND";

			new ImportLicenseFineCalculator(entryLine).UpdateImportLicenseFineOnEntryLine();

			fee = entryLine.Fees.GetElementWithThisCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode);
			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeAmount for F1ND should be equal to 500", 500m, fee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue for F1ND should be equal to 100", 100m, fee.CF_BaseValue);
				AssertEquals("CF_Rate for F1ND should be equal to 30", 30m, fee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation for F1ND should be equal to %", "%", fee.CF_MethodOfCalculation);
			});
		}

		public void TestUpdateImportLicenseFineOnEntryLineHigherThanMaximum()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			var date = ZDateTime.Now;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_ExportDate = date.AddDays(-5);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 12000m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.ImportLicenseNumber = "123";
			invoiceLine1.ImportLicenseAuthorizationDate = date;
			invoiceLine1.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
			invoiceLine1.ImportLicenseFeeType = "F1D5";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 8000m;
			invoiceLine2.JI_NetWeight = 50m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.ImportLicenseNumber = "123";
			invoiceLine2.ImportLicenseAuthorizationDate = date;
			invoiceLine2.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
			invoiceLine2.ImportLicenseFeeType = "F1D5";

			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			entryLine.CL_CustomsValue = 20000m;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			declaration.ResumeApportionment();
			new ImportLicenseFineCalculator(entryLine).UpdateImportLicenseFineOnEntryLine();

			var fee = entryLine.Fees.GetElementWithThisCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode);
			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeAmount for F1D5 should be equal to 2500", 2500m, fee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue for F1D5 should be equal to 20000", 20000m, fee.CF_BaseValue);
				AssertEquals("CF_Rate for F1D5 should be equal to 15", 15m, fee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation for F1D5 should be equal to 50%", "50%", fee.CF_MethodOfCalculation);
			});

			invoiceLine1.ImportLicenseFeeType = "F1ND";
			invoiceLine2.ImportLicenseFeeType = "F1ND";

			new ImportLicenseFineCalculator(entryLine).UpdateImportLicenseFineOnEntryLine();

			fee = entryLine.Fees.GetElementWithThisCode(Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.NoDiscountCode);
			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeAmount for F1ND should be equal to 5000", 5000m, fee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue for F1ND should be equal to 20000", 20000m, fee.CF_BaseValue);
				AssertEquals("CF_Rate for F1ND should be equal to 30", 30m, fee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation for F1ND should be equal to %", "%", fee.CF_MethodOfCalculation);
			});
		}
	}
}
