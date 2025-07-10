using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument.Testing
{
	public class ARComplianceDocumentHeaderTaiwanValidationTest : ARComplianceDocumentHeaderValidationTest
	{
		public new void TestCheckADH_DocumentDate()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);
			arComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			arComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			arComplianceDocumentHeader.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			arComplianceDocumentHeader.ADH_DocumentDate = ZDateTime.Today.AddDays(-1);
			Assert(arComplianceDocumentHeader.ADH_DocumentDateInfo.HasError("This date cannot be changed for TXE compliance sub type. Please set to today date."));

			AssertEquals("CRD", arComplianceDocumentHeader.ADH_TransactionType);
			arComplianceDocumentHeader.ADH_DocumentDate = ZDateTime.Today.AddDays(-1);
			Assert(arComplianceDocumentHeader.ADH_DocumentDateInfo.HasError("This date cannot be changed for CRD compliance document created in Taiwan Login Company. Please set to today date."));

			arComplianceDocumentHeader.ADH_DocumentDate = ZDateTime.Today;
			Assert(arComplianceDocumentHeader.ADH_DocumentDateInfo.HasChanges);
			Assert(!arComplianceDocumentHeader.ADH_DocumentDateInfo.HasError("This date cannot be changed for TXE compliance sub type. Please set to today date."));
			Assert(!arComplianceDocumentHeader.ADH_DocumentDateInfo.HasError("This date cannot be changed for CRD compliance document created in Taiwan Login Company. Please set to today date."));
		}

		public new void TestCheckADH_DocumentNumber()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);
			var errorMsg = "Document Number must contains two alphabet prefix followed by eight numeric values. E.g. TX00001001";

			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var arInv = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1.0m, testObjectCreator.Debtor);
				var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
				arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
				var arInvoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "T0001002", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
				arInvoiceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
				arInvoiceDocumentHeader.ADH_ComplianceSubType = "TXI";
				arInvoiceDocumentHeader.ADH_ReportingPeriod = 201802;
				Factory.Save();

				var arCrd = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.Debtor, testObjectCreator.TWD, 1m, "");
				var arCrdLine = (ARCreditNoteLine)arCrd.Lines.AddNew();
				arCrdLine.AL_AG = testObjectCreator.GLHeader1.PK;
				var complianceDocumentHeader3 = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE, "desc", arCrdLine);
				complianceDocumentHeader3.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
				complianceDocumentHeader3.ADH_ReportingPeriod = 201802;
				Factory.Save();

				var arComplianceDocumentHeader1 = Factory.Load<ARComplianceDocumentHeader>(complianceDocumentHeader3.PK);
				arComplianceDocumentHeader1.ADH_DocumentStatus = "ADD";
				arComplianceDocumentHeader1.ADH_DocumentNumber = "T0001002";
				Assert(arComplianceDocumentHeader1.ADH_DocumentNumberInfo.HasError(errorMsg));
			}
		}
	}
}