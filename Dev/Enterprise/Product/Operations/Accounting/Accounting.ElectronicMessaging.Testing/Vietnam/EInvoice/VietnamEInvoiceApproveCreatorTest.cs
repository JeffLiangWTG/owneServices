using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice.Testing
{
	public class VietnamEInvoiceApproveCreatorTest : TestCaseWithFactory
	{
		public void TestVietnamEInvoiceApprove()
		{
			var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
			var overrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			var overrideContact = TestObjectCreator.CreateContact(TestObjectCreator.ABIGAS);

			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_Code = "AAA";
			sequence.XD_SequenceClass = "TXI";
			sequence.XD_Prefix = prefix;
			sequence.XD_NumberFormat = AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
			sequence.XD_IsActive = true;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.VND, 0.2m, 20m, 2m, 100.00m, 10.00m, TestObjectCreator.Debtor, TestObjectCreator.CC1.PK);
			invoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			invoice.AH_OC_InvoiceContactOverride = overrideContact.PK;
			invoice.AH_TransactionReference = "PREFIX/AP/19E1";
			invoice.AH_XD_ComplianceBook = sequence.PK;
			invoice.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			invoice.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			invoice.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice.AH_ComplianceDocumentDate = ZDate.Today;
			TestObjectCreator.Debtor.CompanyData.OB_ARExternalDebtorCode = "ExternalDebtorCode";
			invoice.AH_ChequeOrReference = "SellReference";

			var invoiceLine = invoice.Lines.Cast<ARInvoiceLine>().First();
			invoiceLine.AL_AT = taxRate.PK;
			invoiceLine.AL_Desc = "Line1\r\nLine2\r\nLine3";
			invoiceLine.AL_OSTaxAmount = 2m;
			invoiceLine.AL_OverseasTotal = 22m;
			invoiceLine.AL_LocalTaxAmount = 10m;
			invoiceLine.AL_LocalExTaxAmount = 100m;

			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Incorrect amount", null, TestObjectCreator.CC1, 60m, ZDateTime.UtcNow, false);
			creditNote.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			creditNote.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			creditNote.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			creditNote.AH_OC_InvoiceContactOverride = overrideContact.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_XD_ComplianceBook = sequence.PK;
			creditNote.AH_TransactionReference = "ARCRD0001";

			var creditNoteLine = creditNote.Lines.Cast<ARCreditNoteLine>().First();
			creditNoteLine.AL_AT = taxRate.PK;

			TestObjectCreator.SetCustomsCodeForOrgHeader(invoice.Company.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "0100233488");

			var eInvoiceBatch = CreateEInvoicingBatch(creditNote);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "en");

			var (eInvoice, notifications) = CreateVietnamEInvoice(eInvoiceBatch, null, () => new AdditionalTransactionInfoForVietnamEInvoice()
			{
				TransactionPK = creditNote.PK,
				VATRegistrationNum = "0104128565-999",
				ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo
				{
					SeriesPrefix = creditNote.ComplianceBook?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = true,
				},
				CompanyPK = creditNote.Company.PK,
				BranchPK = creditNote.Branch.PK,
				TransactionReference = creditNote.AH_TransactionReference
			});

			AssertNotNull("EInvoice", eInvoice);
			AssertEInvoice(eInvoice, creditNote, "0100233488");
		}

		void AssertEInvoice(VietnamEInvoiceApprove eInvoice, InvoicingBase arTransaction, string vietnamProxyVatNumber)
		{
			CombineAssertions(() =>
			{
				// User
				AssertEquals("user", string.Empty, eInvoice.User.Username);
				AssertEquals("password", string.Empty, eInvoice.User.Password);
				AssertEquals("lang", AccountingConfigurationRegistry.Instance.VietnamEInvoicingErrorMessageLanguage.GetFallBackValueAtAllLevels(arTransaction.Company.PK.ToGuid(), arTransaction.Branch.PK.ToGuid(), Guid.Empty), eInvoice.User.Lang);
				// Inv
				AssertEquals("sid", arTransaction.PK.ToString(), eInvoice.Inv.Sid);
				AssertEquals("form", arTransaction.Branch.PK == TestObjectCreator.NonCurrentBranch.PK ? form : string.Empty, eInvoice.Inv.Form);
				AssertEquals("serial", arTransaction.ComplianceBook?.XD_Prefix ?? ZString.Empty, eInvoice.Inv.Serial);
				AssertEquals("seq", arTransaction.AH_TransactionReference, eInvoice.Inv.Seq);
				AssertEquals("stax", vietnamProxyVatNumber, eInvoice.Inv.Stax);
				AssertEquals("sendfile", 1, eInvoice.Inv.SendFile);
			});
		}

		(VietnamEInvoiceApprove EInvoice, INotifications ValidationErrors) CreateVietnamEInvoice(AccEInvoicingBatch eInvoiceBatch, Action<UniversalTransactionInfo> modifyTransactionInfo = null, Func<AdditionalTransactionInfoForVietnamEInvoice> createAdditionalTransactionInfoForVietnamEInvoice = null)
		{
			var universalTransactionBatch = CreateUniversalTransactionBatch(eInvoiceBatch);
			var universalTransaction = universalTransactionBatch.TransactionCollection[0];
			modifyTransactionInfo?.Invoke(universalTransaction);
			var notifications = new Logger();
			var eInvoiceCreator = new VietnamEInvoiceApproveCreator(createAdditionalTransactionInfoForVietnamEInvoice?.Invoke());
			var eInvoice = eInvoiceCreator.Create();
			return (eInvoice, notifications);
		}

		AccEInvoicingBatch CreateEInvoicingBatch(InvoicingBase invoice)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Batched, Core.Constants.EInvoicingPivotActionType.Approve);
			return batch;
		}

		UniversalTransactionBatch CreateUniversalTransactionBatch(AccEInvoicingBatch batch)
		{
			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)TestConnection).ADOConnection, ((IDbConnectionInternals)TestConnection).ADOTransaction);
			var exportor = new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exportor.CreateTransactionBatch(batch);
			return transactionBatch;
		}

		readonly string form = "FormNumber";
		readonly string prefix = "PREFIX/";

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetupControlAccounts();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123456");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingFormNumber.SetTemporaryValue(Guid.Empty, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), Guid.Empty, form);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator));
		EInvoicingTestHelper helper;
	}
}
