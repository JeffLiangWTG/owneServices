using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan.Testing
{
	public class GlobalElectronicInvoiceBuilderForTaiwanTest : TestCaseWithFactory
	{
		[TestDate(2019, 6, 3, 13, 50, 23, 111)]
		public void TestGlobalElectrnoiceInvoiceMessageForINVComplianceDocumentBatchIsCreatedCorrectly()
		{
			var arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arInvoice.Lines[0].AL_AT = ObjectCreator.GST1.PK;
			var complianceDocument = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], ObjectCreator.Debtor);

			AssertGlobalElectronicInvoice(complianceDocument, "52889317-InvoiceMD-12345675-Paper-20190603-135023111.txt");
		}

		[TestDate(2019, 6, 3, 13, 50, 23, 111)]
		public void TestGlobalElectrnoiceInvoiceMessageForCRDComplianceDocumentBatchIsCreatedCorrectly()
		{
			var arCreditNote = ObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arCreditNote.Lines[0].AL_AT = ObjectCreator.GST1.PK;
			var complianceDocument = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arCreditNote.Lines[0], ObjectCreator.Debtor);

			AssertGlobalElectronicInvoice(complianceDocument, "52889317-AllowanceMD-12345675-Paper-20190603-135023111.txt");
		}

		[TestDate(2019, 6, 3, 13, 50, 23, 111)]
		public void TestGlobalElectrnoiceInvoiceMessageForSpecialVoidingINVComplianceDocumentBatchIsCreatedCorrectly()
		{
			var arInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arInvoice.Lines[0].AL_AT = ObjectCreator.GST1.PK;
			var complianceDocument = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], ObjectCreator.Debtor);
			complianceDocument.ADH_ApprovalNumber = "ApprovalNumber1\nTest";
			complianceDocument.ADH_VoidingReason = "VoidingReason1\nTest";
			
			AssertGlobalElectronicInvoice(complianceDocument, "52889317-Invoice-PV-MD-12345675-Paper-20190603-135023111.txt");
		}

		[TestDate(2019, 6, 3, 13, 50, 23, 111)]
		public void TestGlobalElectrnoiceInvoiceMessageForSpecialVoidingCRDComplianceDocumentBatchIsCreatedCorrectly()
		{
			var arCreditNote = ObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arCreditNote.Lines[0].AL_AT = ObjectCreator.GST1.PK;
			var complianceDocument = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arCreditNote.Lines[0], ObjectCreator.Debtor);
			complianceDocument.ADH_ApprovalNumber = "ApprovalNumber\nTest";
			complianceDocument.ADH_VoidingReason = "VoidingReason\nTest";

			AssertGlobalElectronicInvoice(complianceDocument, "52889317-Allowance-PV-MD-12345675-Paper-20190603-135023111.txt");
		}
		
		void AssertGlobalElectronicInvoice(AccComplianceDocumentHeader complianceDocument, string fileName)
		{
			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot1 = ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);

			var cusCode1 = batch.Company.OrgProxy.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "VAT";
			cusCode1.OK_RN_NKCodeCountry = batch.Company.Country.RN_Code;
			cusCode1.OK_CustomsRegNo = "12345675";
			Factory.Save();

			var exportor = new ComplianceDocumentBatchExporter();
			var complianceDocumentBatch = exportor.CreateComplianceDocumentBatch(batch);
			var (einvoice, validationErrors, validationWarnings) = new GlobalElectronicInvoiceBuilderForTaiwan(batch.AIB_BatchNumber.ToString(), complianceDocumentBatch).Create();

			AssertEquals("MessagingSystem", "Taiwan electronic invoicing system", einvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("MessageType", "REQ", einvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString(), einvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("FileName", fileName, einvoice.Header.ElectronicInvoiceBatchRequest.FileName);
			AssertNotEquals("Payload is not Empty", string.Empty, einvoice.Payload);
		}

		public void TestGEIMessageIsProductionSystem()
		{
			// Arrange
			var defaultRegistryValue = AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.Value;
			AssertEquals("Precondition", AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, defaultRegistryValue);

			var arCreditNote = ObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "001", ObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arCreditNote.Lines[0].AL_AT = ObjectCreator.GST1.PK;

			var complianceDocument = ObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arCreditNote.Lines[0], ObjectCreator.Debtor);
			complianceDocument.ADH_ApprovalNumber = "ApprovalNumber\nTest";
			complianceDocument.ADH_VoidingReason = "VoidingReason\nTest";
			complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK.ToGuid();
			complianceDocument.ADH_BarCode = "12345675";

			var batch = ObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot1 = ObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Batched);

			var cusCode1 = batch.Company.OrgProxy.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "VAT";
			cusCode1.OK_RN_NKCodeCountry = batch.Company.Country.RN_Code;
			cusCode1.OK_CustomsRegNo = "12345675";
			Factory.Save();

			var exportor = new ComplianceDocumentBatchExporter();
			var complianceDocumentBatch = exportor.CreateComplianceDocumentBatch(batch);
			var globalBuilder = new GlobalElectronicInvoiceBuilderForTaiwan("12345", complianceDocumentBatch);

			// Assert
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, DatabaseTypes.Codes.Training, true, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, DatabaseTypes.Codes.Production, false, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Production, true, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Test, false, globalBuilder);

			void ExecuteWithTemporaryRegistryValue(Guid companyPk, string registryCode, string licenceType, bool expectedValue, IGlobalElectronicInvoiceBuilder globalBuilder)
			{
				LicenceTypeChanger.SetSystemLicence(licenceType);
				using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, registryCode))
				{
					var (eInvoice, _, _) = globalBuilder.Create();
					AssertEquals(expectedValue, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);
				}
			}
		}

		TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
