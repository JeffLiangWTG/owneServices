using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument.Testing
{
	public class APComplianceDocumentHeaderTaiwanValidationTest : APComplianceDocumentHeaderValidationTest
	{
		public new void TestCheckADH_DocumentNumber()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);
			var errorMsg = "Document Number must contains two alphabet prefix followed by eight numeric values. E.g. TX00001001";
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			apComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsPayable;
			apComplianceDocumentHeader.ADH_DocumentNumber = "";
			apComplianceDocumentHeader.ADH_ComplianceSubType = "NTI";
			apComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			Factory.Save();

			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA0000001";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_ComplianceSubType = "TXI";

			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA0000001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_DocumentNumber = "A000000001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_DocumentNumber = "0000100001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_DocumentNumber = "00001000001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_DocumentNumber = "00001000001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_DocumentNumber = "TX000010011";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_DocumentNumber = "TX0000101";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_DocumentNumber = "TX00010001";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			var apInv = testObjectCreator.CreateAPInvoice<APInvoice>("INV001", testObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, testObjectCreator.Debtor);
			var apInvLine = (APInvoiceLine)apInv.Lines.AddNew();
			apInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_ReportingPeriod = 201802;

			Factory.Save();

			var apInv1 = testObjectCreator.CreateAPInvoice<APInvoice>("INV002", testObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, testObjectCreator.Debtor);
			var apInvLine1 = (APInvoiceLine)apInv1.Lines.AddNew();
			apInvLine1.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader1 = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine1);
			invoiceDocumentHeader1.ADH_OH_Organisation = testObjectCreator.Debtor.PK;

			Factory.Save();

			var apInvoiceDocumentHeader = Factory.Load<APComplianceDocumentHeader>(invoiceDocumentHeader1.PK);

			invoiceDocumentHeader.ADH_DocumentStatus = "SET";
			apInvoiceDocumentHeader.ADH_DocumentNumber = "TX00090001";
			Assert(apInvoiceDocumentHeader.ADH_DocumentNumberInfo.HasError("This Compliance Document Number is already in use. Please enter another number."));

			invoiceDocumentHeader.ADH_DocumentStatus = "VOD";
			apInvoiceDocumentHeader.ADH_DocumentNumber = "TX00090001";
			Assert(!apInvoiceDocumentHeader.ADH_DocumentNumberInfo.HasError("This Compliance Document Number is already in use. Please enter another number."));
		}

		public void TestCheckRuleOfADH_DocumentNumber()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);
			var errorMsg = "Document Number must contains two alphabet prefix followed by eight numeric values. E.g. TX00001001";
			var errorMsg1 = @"For compliance sub type TXE and TCE, the document number must be in one of the following formats:
1. 2 characters prefix followed by 8 numeric digits. E.g. TX00001001.
2. 2 characters 'BB' prefix followed by 8 alpha-numeric characters. E.g. BB12345678, BBTXIC2535.";
			var errorMsg2 = @"For compliance sub type TDC and TCD, the document number must be in one of the following formats:
1. 2 characters prefix followed by 8 numeric digits. E.g. TX00001001.
2. 10 alpha-numeric. E.g. A1G2345678, BDTXIC2535, 1234567890.";
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			apComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsPayable;
			apComplianceDocumentHeader.ADH_DocumentNumber = "";
			apComplianceDocumentHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			apComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			Factory.Save();

			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA0000001";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA0000001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg));

			apComplianceDocumentHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA0000001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg1));

			apComplianceDocumentHeader.ADH_DocumentNumber = "AA12345678";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg1));

			apComplianceDocumentHeader.ADH_DocumentNumber = "BBABCD1234";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg1));

			apComplianceDocumentHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA0000001";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg1));

			apComplianceDocumentHeader.ADH_DocumentNumber = "AA12345678";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg1));

			apComplianceDocumentHeader.ADH_DocumentNumber = "BBABCD1234";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg1));

			apComplianceDocumentHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC;
			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA00000011";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg2));

			apComplianceDocumentHeader.ADH_DocumentNumber = "A1B2C3D4E5";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg2));

			apComplianceDocumentHeader.ADH_DocumentNumber = "AA12345678";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg2));

			apComplianceDocumentHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
			apComplianceDocumentHeader.ADH_DocumentNumber = "AAA00000011";
			Assert(apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg2));

			apComplianceDocumentHeader.ADH_DocumentNumber = "A1B2C3D4E5";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg2));

			apComplianceDocumentHeader.ADH_DocumentNumber = "AA12345678";
			Assert(!apComplianceDocumentHeader.ADH_DocumentNumberInfo.HasError(errorMsg2));
		}
	}
}
