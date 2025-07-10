using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRAddInfoHeaderValidationTest : AUAddInfoHeaderValidationTest
	{
		public void TestEnsureThatEFDAndVANIsTheSameForAllInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_IsPackToBondForLine = true;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice1.AddInfo.ZA_EFD = "120613";
			var efdErrorMessage = "This value (120613) is not the same value as the value () on invoice (INV2); Inventory Management Integration requires that all invoices with goods for warehousing should have the same value.";
			AssertHasError(invoice1.AddInfo.ZA_EFDInfo, efdErrorMessage);
			invoice2.AddInfo.ZA_EFD = "120613";
			invoice1.AddInfo.Validation.ValidateZA_EFD();
			AssertNoError(invoice1.AddInfo.ZA_EFDInfo, efdErrorMessage);

			invoice1.AddInfo.ZA_VAN = "2342";
			var vanErrorMessage = "This value (2342) is not the same value as the value () on invoice (INV2); Inventory Management Integration requires that all invoices with goods for warehousing should have the same value.";
			AssertHasError(invoice1.AddInfo.ZA_VANInfo, vanErrorMessage);
			invoice2.AddInfo.ZA_VAN = "2342";
			invoice1.AddInfo.Validation.ValidateZA_VAN();
			AssertNoError(invoice1.AddInfo.ZA_VANInfo, vanErrorMessage);
		}

		public void TestHeaderN10InvoiceLineNoError()
		{
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_Nature10PackCount = 10;

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 900m;

			invoiceHeader.JZ_BondPackCount = 0;
			Assert("No Error expected", !invoiceHeader.JZ_BondPackCountInfo.HasNotifications());
		}

		public void TestHeaderN20InvoiceLineN20NoError()
		{
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_Nature10PackCount = 10;
			invoiceHeader.JZ_BondPackCount = 10;

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_IsPackToBondForLine = true;

			invoiceHeader.JZ_BondPackCount = 10;
			Assert("No Error expected", !invoiceHeader.JZ_BondPackCountInfo.HasNotifications());
		}

		public void TestHeaderN20InvoiceLineN20Error()
		{
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_BondPackCount = 10;

			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 900m;

			invoiceHeader.JZ_BondPackCount = 10;
			Assert("Error expected", invoiceHeader.JZ_BondPackCountInfo.HasNotifications());
		}

		public void TestZA_ORGForSAC()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsSACWithoutLines", true, testDec.IsSACWithoutLines);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = "";
			AssertNoMessageError(invoice.AddInfo.ZA_ORGInfo, "You have indicated that this entry is SAC with lines and Origin is mandatory.");

			invoice.AddInfo.ZA_ORG = "ZA";
			AssertNoMessageErrors("Origin is entered", invoice.AddInfo.ZA_ORGInfo);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("IsSACWithoutLines", true, testDec.IsSACWithLines);
			invoice.AddInfo.ZA_ORG = "";
			AssertHasMessageError("Empty origin is a message error for SAC with line", invoice.AddInfo.ZA_ORGInfo, "You have indicated that this entry is SAC with lines and Origin is mandatory.");

			invoice.AddInfo.ZA_ORG = "NZ";
			AssertNoMessageError(invoice.AddInfo.ZA_ORGInfo, "You have indicated that this entry is SAC with lines and Origin is mandatory.");

			invoice.AddInfo.ZA_ORG = "TAIW"; //Invalid Code
			AssertHasMessageErrors("Origin is Invalid", invoice.AddInfo.ZA_ORGInfo);
		}

		public void TestCheckZA_DCX()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DCX=AA";
			AssertEquals("DCX cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DCXInfo.HasMessageErrors());
		}

		public void TestCheckZA_DXT()
		{
			invoiceHeader.AddInfo.AddInfoLine = "DXT=AA";
			AssertEquals("DXT cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_DXTInfo.HasMessageErrors());
		}

		public void TestCheckZA_ELA()
		{
			invoiceHeader.AddInfo.AddInfoLine = "ELA=AA";
			AssertEquals("ELA cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_ELAInfo.HasMessageErrors());
		}

		public void TestCheckZA_FOD()
		{
			invoiceHeader.AddInfo.AddInfoLine = "FOD=AA";
			AssertEquals("FOD cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_FODInfo.HasMessageErrors());
		}

		public void TestCheckZA_ISC()
		{
			invoiceHeader.AddInfo.AddInfoLine = "ISC=AA";
			AssertEquals("ISC cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_ISCInfo.HasMessageErrors());
		}

		public void TestCheckZA_LCP()
		{
			invoiceHeader.AddInfo.AddInfoLine = "LCP=1";
			AssertEquals("LCP cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_LCPInfo.HasMessageErrors());
		}

		public void TestCheckZA_WMC()
		{
			invoiceHeader.AddInfo.AddInfoLine = "WMC=AA";
			AssertEquals("WMC cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_WMCInfo.HasMessageErrors());
		}

		public void TestCheckZA_TRN()
		{
			invoiceHeader.AddInfo.AddInfoLine = "TRN=AA";
			AssertEquals("TRN cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_TRNInfo.HasMessageErrors());
		}

		public void TestCheckZA_PRI()
		{
			invoiceHeader.AddInfo.AddInfoLine = "PRI=AA";
			AssertEquals("PRI cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_PRIInfo.HasMessageErrors());
		}

		public void TestCheckZA_TCI()
		{
			invoiceHeader.AddInfo.AddInfoLine = "TCI=AA";
			AssertEquals("TCI cannot be entered at header level", true, invoiceHeader.AddInfo.ZA_TCIInfo.HasMessageErrors());
		}

		public void TestCheckZA_HeaderREL_Hidden()
		{
			invoiceHeader.AddInfo.AddInfoLine = "HeaderREL_Hidden=A";
			AssertHasMessageErrors(invoiceHeader.AddInfo.ZA_HeaderREL_HiddenInfo);
			invoiceHeader.AddInfo.AddInfoLine = "HeaderREL_Hidden=Y";
			AssertNoMessageErrors(invoiceHeader.AddInfo.ZA_HeaderREL_HiddenInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}
	}
}
