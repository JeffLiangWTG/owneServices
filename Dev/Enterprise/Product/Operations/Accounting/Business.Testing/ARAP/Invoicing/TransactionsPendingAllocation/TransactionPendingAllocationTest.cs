using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocation))]
	public class TransactionPendingAllocationTest : InvoicingBaseTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestfetchStrategy()
		{
			var transaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var fetchStrategy = transaction.FetchStrategy;
			var errorMessage = "Expected type TransactionPendingAllocationFetchStrategy";

			Assert(errorMessage, fetchStrategy != null);
			AssertEquals(errorMessage, fetchStrategy.GetType(), typeof(TransactionPendingAllocationFetchStrategy));
		}

		public override void TestCopiedFromPK()
		{
			Assert("Tax Transactions is not calcualted for this transaction type.", true);
		}

		public override void TestIsTaxReportable()
		{
			Assert(!Header.IsTaxReportable);
		}

		public override void TestReleaseAllMutexOnInvoiceShouldDisposeLineJobsWithMutex()
		{
			Assert("Cannot test LineJobsWithMutex, because this transaction has no lines.", true);
		}

		public void TestDocManagerInfoReturnsCorrectObject()
		{
			var transaction = (TransactionPendingAllocation)GetNewBusinessObject();
			AssertEquals("IDocManagerSupport should return the same object as used during saving eDocs with transaction.", ((IDocManagerSupport)transaction).DocManagerInfo, transaction.DocManagerInfo);
		}

		public void TestPostDateIsNotReadonlyForBackDating()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.PayablesAllowMatchDateToBeBackDated.IsAllowed = false;
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			transaction.AH_PostDate = ZDateTime.Today.AddDays(-10);

			Assert("Transaction can be edited without posting, so user is allowed to modify date to fix back dating validation error.", !transaction.AH_PostDateInfo.ReadOnly);
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			Assert("This is just to check that in different configuration Post Date will be readonly for this transaction, otherwise above check doesn't make scene.", transaction.AH_PostDateInfo.ReadOnly);
		}

		public void TestHasUniversalTransaction()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			AssertNull("Precondition: TransactionApprovalRequest", transaction.TransactionApprovalRequest);
			Assert(!transaction.IsImportedFromUniversalXML);

			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			AssertNotNull("Precondition: TransactionApprovalRequest", transaction.TransactionApprovalRequest);
			Assert(!transaction.IsImportedFromUniversalXML);

			request.Initialize(transaction, "some xml", false);
			Assert(transaction.IsImportedFromUniversalXML);
		}

		public void TestCreateMultipleTransactionPendingAllocationWithSameTransactionNumber()
		{
			var transaction1 = TestObjectCreator.CreateTransactionPendingAllocation("TPA0001", TestObjectCreator.Creditor1, 100);
			Factory.Save();

			var transaction2 = TestObjectCreator.CreateTransactionPendingAllocation("TPA0001", TestObjectCreator.Creditor2, 100);
			AssertNoExceptionThrown(() => Factory.Save());

			var transaction3 = TestObjectCreator.CreateTransactionPendingAllocation("TPA0001", TestObjectCreator.Creditor2, 100);
			var expectedExceptionMessage = string.Format("Cannot insert duplicate key row in object 'dbo.AccTransactionHeader' with unique index 'NR_UX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_TransactionNum_AH_TransactionCount'. The duplicate key value is ({0}, PA, {1}, IPA, TPA0001, 1).",
				transaction3.AH_GC, TestObjectCreator.Creditor2.PK);
			AssertExceptionThrown(expectedExceptionMessage, typeof(ZSaveException), () => Factory.Save());
		}

		public void TestNoDBHitsForGettingApprovalRequestOfNewPendingAllocation()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);

			Factory.ClearQueryCache();
			int hitCountBeforeApprovalLoaded = Factory.GetTableHitCount(GenApprovalRequestSchema.Constants.TableName);
			AssertEquals("Precondition: transaction is not yet saved", false, transaction.IsInDatabase);
			AssertEquals("Should found the approval request", request.PK, transaction.TransactionApprovalRequest.PK);
			int hitCountAfterApprovalLoaded = Factory.GetTableHitCount(GenApprovalRequestSchema.Constants.TableName);
			AssertEquals("No DB Hits", 0, hitCountAfterApprovalLoaded - hitCountBeforeApprovalLoaded);
		}

		public void TestCanDeleteForTransactionWithApprovalRequest()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();

			Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);
			Assert("CanDelete", transaction.CanDelete);

			transaction.AH_Desc = "this transaction has just been updated !!";
			Factory.Save();
			Assert("CanDelete", !transaction.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "This transaction cannot be deleted as it has been edited, you can Cancel the corresponding approval request.", transaction.ReasonForNotAbleToDelete);
		}

		public override void TestGetWritableProperties()
		{
			var transactionPendingAllocation = (TransactionPendingAllocation)GetNewBusinessObject();

			AssertEquals("GetWritableProperties().Count", 6, transactionPendingAllocation.GetWritableProperties_ForTestOnly().Count);
			AssertEquals("GetWritableProperties()[0]", "OSPartialPaymentAmount", transactionPendingAllocation.GetWritableProperties_ForTestOnly()[0]);
			AssertEquals("GetWritableProperties()[1]", "MatchStatus", transactionPendingAllocation.GetWritableProperties_ForTestOnly()[1]);
			AssertEquals("GetWritableProperties()[2]", "MatchStatusReasonCode", transactionPendingAllocation.GetWritableProperties_ForTestOnly()[2]);
			AssertEquals("GetWritableProperties()[3]", "IncludeInTheBatch", transactionPendingAllocation.GetWritableProperties_ForTestOnly()[3]);
			AssertEquals("GetWritableProperties()[4]", "IncludeInThePeriodicInvoice", transactionPendingAllocation.GetWritableProperties_ForTestOnly()[4]);
			AssertEquals("GetWritableProperties()[3]", "AH_ComplianceSubType", transactionPendingAllocation.GetWritableProperties_ForTestOnly()[5]);
		}

		public override void TestApprovalRequestStatus()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);

			var request1 = new BusinessObjectFactory().New<TransactionPendingAllocationApprovalRequest>();
			request1.Initialize(transaction);
			var expectedCreateTime1 = ZDateTime.Today.AddDays(-2);
			request1.XP_SystemCreateTimeUtc = expectedCreateTime1;
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			AssertEquals("Show status only for saved requests", "", transaction.ApprovalRequestStatus);
			AssertNull("ApprovalRequest", transaction.TransactionRelatedApprovalRequest);
			Assert("HasApprovalRequest", !transaction.HasApprovalRequest);
			request1.Factory.Save();
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, transaction.ApprovalRequestStatus);
			AssertEquals("ApprovalRequest", request1.PK, transaction.TransactionRelatedApprovalRequest.PK);
			Assert("HasApprovalRequest", transaction.HasApprovalRequest);

			var request2 = new BusinessObjectFactory().New<TransactionPendingAllocationApprovalRequest>();
			request2.Initialize(transaction);
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
			var expectedCreateTime2 = ZDateTime.Today.AddDays(-5);
			request2.XP_SystemCreateTimeUtc = expectedCreateTime2;
			request2.Factory.Save();
			AssertEquals("Precondition: request1.XP_SystemCreateTimeUtc", expectedCreateTime1, request1.XP_SystemCreateTimeUtc);
			AssertEquals("Precondition: request2.XP_SystemCreateTimeUtc", expectedCreateTime2, request2.XP_SystemCreateTimeUtc);
			AssertEquals("Show status only for saved requests", Constants.GenApprovalRequestApprovalStatus.Cancelled, transaction.ApprovalRequestStatus);
			AssertEquals("ApprovalRequest", request1.PK, transaction.TransactionRelatedApprovalRequest.PK);

			expectedCreateTime2 = ZDateTime.Today.AddDays(-1);
			request2.XP_SystemCreateTimeUtc = expectedCreateTime2;
			request2.Factory.Save();
			AssertEquals("Precondition: request1.XP_SystemCreateTimeUtc", expectedCreateTime1, request1.XP_SystemCreateTimeUtc);
			AssertEquals("Precondition: request2.XP_SystemCreateTimeUtc", expectedCreateTime2, request2.XP_SystemCreateTimeUtc);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Rejected, transaction.ApprovalRequestStatus);
			AssertEquals("ApprovalRequest", request2.PK, transaction.TransactionRelatedApprovalRequest.PK);
		}

		public void TestAmountsAreNotResetToZeroAsNoLinesAdded()
		{
			var transaction = (TransactionPendingAllocation)GetNewBusinessObject();
			transaction.FillWithValidTestData();
			transaction.AH_OSExTaxAmount = 100;
			transaction.AH_OSTaxAmount = 10;

			transaction.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			Factory.Save();

			AssertEquals("AH_OSExTaxAmount", 100m, transaction.AH_OSExTaxAmount);
			AssertEquals("AH_OSTaxAmount", 10m, transaction.AH_OSTaxAmount);
		}

		public void TestFullyPaidDateIsNotSet()
		{
			var transaction = (TransactionPendingAllocation)GetNewBusinessObject();
			transaction.FillWithValidTestData();
			Factory.Save();

			AssertEquals("Precondition: AH_OSExTaxAmount", 0m, transaction.AH_OSExTaxAmount);
			AssertEquals("Precondition: AH_OSTaxAmount", 0m, transaction.AH_OSTaxAmount);
			AssertEquals("AH_FullyPaidDate", ZDateTime.Empty, transaction.AH_FullyPaidDate);
		}

		public override void TestWorkflowType()
		{
			var transaction = (TransactionPendingAllocation)GetNewBusinessObject();
			var workflowProvider = transaction as IWorkflowProvider;
			AssertNotNull("IWorkflowProvider", workflowProvider);
			AssertEquals("WorkflowType", WorkflowDescriptors.APInvoiceCode, workflowProvider.WorkflowType);
		}

		public void TestNoLinesCanBeAdded()
		{
			var transaction = (TransactionPendingAllocation)GetNewBusinessObject();
			Assert("Lines.AllowNew", !transaction.Lines.AllowNew);
			Assert("Lines collection filter shouldn't allow to load or match any lines to not be updated by data refresh bus.", transaction.Lines.CompleteFilter.IsNoResultQuery);
		}

		public void TestExchangeRateRecalculatedOnSaving()
		{
			TransactionPendingAllocation transaction = new BusinessObjectFactory().New<TransactionPendingAllocation>();
			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OH = transaction.Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.AH_TransactionNum = "Tran # 1";
			transaction.ExchangeRate.Currency = "USD";
			transaction.ExchangeRate.Rate = 1.6352;
			transaction.AH_OSExTaxAmount = 50m;
			transaction.AH_OSTaxAmount = 30m;
			transaction.AH_OutstandingAmount = -48.93;
			transaction.Factory.Save();

			AssertEquals(1.634989m, transaction.AH_ExchangeRate);
		}

		public void TestGSTReadOnly_Ness()
		{
			TransactionPendingAllocation transaction = new BusinessObjectFactory().New<TransactionPendingAllocation>();
			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OH = transaction.Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.Header.CompanyData.SetAPTaxApplicable(true);

			Assert(!transaction.AH_OSTaxAmountInfo.ReadOnly);
			transaction.AH_OSTaxAmount = 10m;

			OrgHeader nonTaxOrg = transaction.Factory.NewWithValidTestData<OrgHeader>();
			nonTaxOrg.CompanyData.SetAPTaxApplicable(false);

			transaction.AH_OH = nonTaxOrg.PK;
			Assert(transaction.AH_OSTaxAmountInfo.ReadOnly);
			AssertEquals(0m, transaction.AH_OSTaxAmount);
		}

		public void TestDueDateCalculatedWhenSettingOrg()
		{
			TransactionPendingAllocation transaction = new BusinessObjectFactory().New<TransactionPendingAllocation>();

			OrgHeader org = transaction.Factory.New<OrgHeader>();
			org.CompanyData.OB_APPaymentTerms = "INV";
			org.CompanyData.OB_APPaymentTermDays = 10;

			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OH = org.PK;

			AssertEquals(transaction.AH_InvoiceDate.AddDays(10), transaction.AH_DueDate);
		}

		public void TestDescription()
		{
			TransactionPendingAllocation transaction = new BusinessObjectFactory().New<TransactionPendingAllocation>();

			OrgHeader org = transaction.Factory.New<OrgHeader>();
			org.CompanyData.OB_APPaymentTerms = "INV";
			org.CompanyData.OB_APPaymentTermDays = 10;

			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OH = org.PK;

			AssertEquals("Description should be 'UNAPPROVED INVOICE'", "UNAPPROVED INVOICE", transaction.AH_Desc);
			AssertEquals("Number Of Supporting Documents should be 1", 1, transaction.AH_NumberOfSupportingDocuments.ToZInt());
		}

		public void TestSetTransactionTypeOnSaving()
		{
			TransactionPendingAllocation transaction = new BusinessObjectFactory().New<TransactionPendingAllocation>();
			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OH = transaction.Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.AH_OSExTaxAmount = 100m;
			transaction.AH_OutstandingAmount = -100m;
			transaction.Factory.Save();
			AssertEquals(TransactionTypes.InvoicePendingAllocation, transaction.AH_TransactionType);
			transaction.AH_OSExTaxAmount = -100m;
			transaction.AH_OutstandingAmount = 100m;
			transaction.Factory.Save();
			AssertEquals(TransactionTypes.CreditNotePendingAllocation, transaction.AH_TransactionType);
		}

		public void TestTransactionType()
		{
			TransactionPendingAllocation transaction = new BusinessObjectFactory().New<TransactionPendingAllocation>();
			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OH = transaction.Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.AH_OSExTaxAmount = 100m;

			AssertEquals(TransactionTypes.InvoicePendingAllocation, transaction.AH_TransactionType);
			transaction.AH_OSExTaxAmount = -100m;
			AssertEquals(TransactionTypes.CreditNotePendingAllocation, transaction.AH_TransactionType);
		}

		public override void TestAH_RX_NKTransactionCurrency()
		{
			Assert(Header.AH_LocalExTaxAmountInfo.ReadOnly);
			Assert(Header.AH_LocalTaxAmountInfo.ReadOnly);
		}

		public void TestTransactionsPendingApprovalInDatabaseAreNotReadOnly()
		{
			TransactionPendingAllocation transactionPendingAllocation = (TransactionPendingAllocation)GetNewBusinessObject();
			transactionPendingAllocation.FillWithValidTestData();
			Factory.Save();
			AssertEquals(false, transactionPendingAllocation.IsTransactionInDatabaseReadOnly);
		}

		public void TestCreateLogAdded()
		{
			var header = Factory.New<TransactionPendingAllocation>();
			header.AH_TransactionNum = "00001010";
			header.AH_OSExTaxAmount = 100m;
			header.AH_OH = TestObjectCreator.Creditor1.PK;
			header.AH_PostDate = ZDateTime.Today;

			Factory.Save();

			Assert("IPA: log added", header.Logs.HasLogWith(StmALogSchema.SL_Reference, "PA|IPA|Created"));

			var header1 = Factory.New<TransactionPendingAllocation>();
			header1.AH_TransactionNum = "00001010";
			header1.AH_OSExTaxAmount = -100m;
			header1.AH_OH = TestObjectCreator.Creditor1.PK;
			header1.AH_PostDate = ZDateTime.Today;

			Factory.Save();

			Assert("CPA: log added", header1.Logs.HasLogWith(StmALogSchema.SL_Reference, "PA|CPA|Created"));
		}

		protected override void CancelTransactionHeaderToBeAbleToSave(TransactionHeader header)
		{
			var cancellable = header as ICancellable;
			cancellable.IsCancelled = true;
			Assert("IsCancelled", cancellable.IsCancelled);
		}

		public override void TestDeleteTransactionInDBCausesException()
		{
			SetupForSave();
			Header.Factory.Save();
			Header.Delete();
			Assert(Header.IsDeleted);
		}

		protected override void SetupForSave()
		{
			base.SetupForSave();
			Header.AH_TransactionNum = "00001010";
			Header.AH_OSExTaxAmount = 100m;
			Header.AH_OH = TestObjectCreator.Creditor1.PK;
			Header.AH_PostDate = ZDateTime.Today;
			SetupLinesForSaving();

			Header.RunPreSaveValidation();
			AssertNoErrors(Header);
		}

		public void TestCanDelete()
		{
			SetupForSave();
			Header.Factory.Save();
			Assert("Can Delete Should Return True Becuase No Export Batch Exist Against Transaction", Header.CanDelete);

			GenExportBatchSequence batch = Factory.New<GenExportBatchSequence>();
			batch.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport;
			batch.XB_BatchNumber = 100;
			batch.XB_ParentID = Header.PK;
			batch.XB_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			batch.XB_Sequence = 1;

			batch.Factory.Save();

			Assert("Can Delete Should Return False Becuase Export Batch Exist Against Transaction", !Header.CanDelete);
		}

		public void TestCanDeleteForAllocatedTransaction()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();

			APInvoice invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;
			InvoicingLineBase line = (InvoicingLineBase)invoiceTransaction.Lines.AddNew();
			line.AL_OSExTaxAmount = invoiceTransaction.AH_OSExTaxAmount;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceTransaction.Factory.Save();

			Assert("Precondition: IsAllocated", transaction.IsAllocated);
			Assert("CanDelete", !transaction.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "This transaction has been allocated and cannot be deleted.", transaction.ReasonForNotAbleToDelete);
		}

		public void TestTemplateCopy()
		{
			ZGuid expectedOrg = TestObjectCreator.AALSHI.PK;
			ZString expectedDesc = "test desc";
			ZGuid expectedBrn = TestObjectCreator.NonCurrentBranch.PK;
			ZGuid expectedDept = TestObjectCreator.NonCurrentDepartment.PK;
			ZGuid expectedAddress = TestObjectCreator.CreateAddress(TestObjectCreator.AALSHI, "123 test st").PK;
			ZGuid expectedContact = TestObjectCreator.CreateContact(TestObjectCreator.AALSHI, "Peter").PK;

			TransactionPendingAllocation transactionPendingAllocation = (TransactionPendingAllocation)GetNewBusinessObject();
			transactionPendingAllocation.AH_OH = expectedOrg;
			transactionPendingAllocation.AH_Desc = expectedDesc;
			transactionPendingAllocation.AH_GB = expectedBrn;
			transactionPendingAllocation.AH_GE = expectedDept;
			transactionPendingAllocation.AH_OA_InvoiceAddressOverride = expectedAddress;
			transactionPendingAllocation.AH_OC_InvoiceContactOverride = expectedContact;

			TransactionPendingAllocation copy = (TransactionPendingAllocation)transactionPendingAllocation.TemplateCopy();
			AssertEquals("Organisation", expectedOrg, copy.AH_OH);
			AssertEquals("Description", expectedDesc, copy.AH_Desc);
			AssertEquals("Branch", expectedBrn, copy.AH_GB);
			AssertEquals("Department", expectedDept, copy.AH_GE);
			AssertEquals("Address", expectedAddress, copy.AH_OA_InvoiceAddressOverride);
			AssertEquals("Contact", expectedContact, copy.AH_OC_InvoiceContactOverride);
		}

		public void TestIDocManagerSupport()
		{
			IDocManagerSupport support = Header;
			AssertNotNull("Should support IDocManagerSupport", support);

			AssertNotNull("DocManagerInfo", support.DocManagerInfo);
			AssertEquals("DocManagerCode should match", Constants.DocManagerCodes.PayableTransactionPendingAllocation, support.DocManagerInfo.DocManagerCode);
			AssertEquals("BusinessEntity should match", Header.PK, support.DocManagerInfo.BusinessEntity.PK);
		}

		public void TestExchangeRateUnchangedOnUpdatingOSTaxAmountForLocalCurrency()
		{
			TransactionPendingAllocation transactionPendingAllocation = (TransactionPendingAllocation)GetNewBusinessObject();
			transactionPendingAllocation.AH_OH = TestObjectCreator.AALSHI.PK;
			transactionPendingAllocation.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("AH_ExchangeRate", 1m, transactionPendingAllocation.AH_ExchangeRate);

			transactionPendingAllocation.AH_OSExTaxAmount = 100m;
			AssertEquals("AH_LocalExTaxAmount", 100m, transactionPendingAllocation.AH_LocalExTaxAmount);
			transactionPendingAllocation.AH_OSTaxAmount = 10m;
			AssertEquals("AH_LocalTaxAmount", 10m, transactionPendingAllocation.AH_LocalTaxAmount);

			transactionPendingAllocation.AH_OSTaxAmount = 20m;
			AssertEquals("AH_ExchangeRate should not be changed", 1m, transactionPendingAllocation.AH_ExchangeRate);
			AssertEquals("AH_LocalTaxAmount should be as expected", 20m, transactionPendingAllocation.AH_LocalTaxAmount);
			AssertEquals("AH_LocalExTaxAmount should not be changed", 100m, transactionPendingAllocation.AH_LocalExTaxAmount);
			AssertEquals("AH_OSExTaxAmount should not be changed", 100m, transactionPendingAllocation.AH_OSExTaxAmount);
		}

		public void TestExchangeRateUnchangedOnUpdatingOSTaxAmountForForeignCurrency()
		{
			TransactionPendingAllocation transactionPendingAllocation = (TransactionPendingAllocation)GetNewBusinessObject();
			transactionPendingAllocation.AH_OH = TestObjectCreator.AALSHI.PK;
			transactionPendingAllocation.AH_RX_NKTransactionCurrency = "USD";
			transactionPendingAllocation.AH_ExchangeRate = 0.5;

			transactionPendingAllocation.AH_OSExTaxAmount = 100m;
			AssertEquals("AH_LocalExTaxAmount", 200m, transactionPendingAllocation.AH_LocalExTaxAmount);
			transactionPendingAllocation.AH_OSTaxAmount = 10m;
			AssertEquals("AH_LocalTaxAmount", 20m, transactionPendingAllocation.AH_LocalTaxAmount);

			transactionPendingAllocation.AH_OSTaxAmount = 20m;
			AssertEquals("AH_ExchangeRate should not be changed", 0.5m, transactionPendingAllocation.AH_ExchangeRate);
			AssertEquals("AH_LocalTaxAmount should be as expected", 40m, transactionPendingAllocation.AH_LocalTaxAmount);
			AssertEquals("AH_LocalExTaxAmount should not be changed", 200m, transactionPendingAllocation.AH_LocalExTaxAmount);
			AssertEquals("AH_OSExTaxAmount should not be changed", 100m, transactionPendingAllocation.AH_OSExTaxAmount);
		}

		public void TestCreditLimitEmailIsNotSent()
		{
			SetupCreditControlGroup();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARCreditLimit = 10m;
			org.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			Factory.Save();
			InvoicingBase invoice = (InvoicingBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			invoice.AH_OH = org.PK;
			invoice.AH_OSExTaxAmount = 20m;
			invoice.AH_InvoiceTerm = "INV";
			invoice.SubmittedFromInvoicingForm = true;
			Factory.Save();
			AssertEquals("No Email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public override void TestBusinessContext()
		{
			AssertEquals(CargoWise.Definitions.BusinessContext.INVALID, InvoicingBase.DocumentSupporter.BusinessContext);
		}

		public override void TestDeleteTransationHeaderInDatabase()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();
			AssertEquals(true, transaction.CanDelete);

			transaction.TransactionApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
			Factory.Save();
			AssertEquals(true, transaction.CanDelete);

			transaction.AH_Desc = "this transaction has just been updated !!";
			transaction.TransactionApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Factory.Save();
			AssertEquals(false, transaction.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "This transaction cannot be deleted as it has been edited, you can Cancel the corresponding approval request.", transaction.ReasonForNotAbleToDelete);

			transaction.TransactionApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();
			AssertEquals(false, transaction.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "This transaction cannot be deleted as it is linked to approval request that has the status of APP, REJ or CAN.", transaction.ReasonForNotAbleToDelete);

			transaction.TransactionApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			transaction.TransactionApprovalRequest.SetContext(BusinessContext.CancelApprovalRequestByUser);
			Factory.Save();
			AssertEquals(false, transaction.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "This transaction cannot be deleted as it is linked to approval request that has the status of APP, REJ or CAN.", transaction.ReasonForNotAbleToDelete);
		}

		public void TestDeleteTransactionRelatedApprovalRequest()
		{
			var transactionPendingAllocation1 = TestObjectCreator.CreateTransactionPendingAllocation("INV11", TestObjectCreator.Creditor1, 100);
			var transactionPendingAllocation2 = TestObjectCreator.CreateTransactionPendingAllocation("INV22", TestObjectCreator.Creditor2, 100);
			Factory.Save();

			AssertNotNull("Precondition: transaction1 has related approval request", transactionPendingAllocation1.TransactionRelatedApprovalRequest);
			AssertNotNull("Precondition: transaction2 has related approval request", transactionPendingAllocation2.TransactionRelatedApprovalRequest);
			AssertEquals("Precondition", Constants.GenApprovalRequestApprovalStatus.Requested, transactionPendingAllocation1.TransactionApprovalRequest.XP_ApprovalStatus);
			AssertEquals("Precondition", Constants.GenApprovalRequestApprovalStatus.Requested, transactionPendingAllocation2.TransactionApprovalRequest.XP_ApprovalStatus);

			transactionPendingAllocation1.Delete();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			Assert("transaction1 is deleted", transactionPendingAllocation1.IsDeleted);
			AssertNull("transaction1 Related Approval Request is deleted", transactionPendingAllocation1.TransactionRelatedApprovalRequest);

			transactionPendingAllocation2.AH_Desc = "This transaction has just been updated!";
			transactionPendingAllocation2.TransactionApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			transactionPendingAllocation2.TransactionApprovalRequest.SetContext(BusinessContext.CancelApprovalRequestByUser);
			var approvalRequest2 = Factory.New<TransactionPendingAllocationApprovalRequest>();
			approvalRequest2.Initialize(transactionPendingAllocation2);
			approvalRequest2.XP_ReasonDescription = "Desc";

			Factory.Save();

			AssertEquals("Precondition", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest2.XP_ApprovalStatus);

			transactionPendingAllocation2.Delete();

			AssertEquals("TransactionPendingAllocation_ApprovalRequest_OnDelete", ErrorReporter.LastKeyReported);
			AssertEquals("Developer Exception:", "System allowed to delete the Transaction Pending Allocation which has more than one Approval Request. TPA should be allowed to delete when it has only one approval request.", ErrorReporter.LastMessageReported);
			Assert("transaction2 is deleted", transactionPendingAllocation2.IsDeleted);
			Assert("transaction2 Related Approval Request with Requested Status is deleted", approvalRequest2.IsDeleted);
			ErrorReporter.Clear();
		}

		public void TestDeleteTransactionOnErrorStatus()
		{
			var transactionPendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV11", TestObjectCreator.Creditor1, 100);
			transactionPendingAllocation.AH_OH = Guid.Empty;
			Factory.Save();

			AssertNotNull("Precondition: transaction1 has related approval request", transactionPendingAllocation.TransactionRelatedApprovalRequest);
			AssertEquals("Precondition", Constants.GenApprovalRequestApprovalStatus.Error, transactionPendingAllocation.TransactionApprovalRequest.XP_ApprovalStatus);

			transactionPendingAllocation.Delete();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			Assert("Transaction is deleted", transactionPendingAllocation.IsDeleted);
			AssertNull("Transaction on Error status is deleted", transactionPendingAllocation.TransactionRelatedApprovalRequest);
		}

		public override void TestPostedBy()
		{
			AssertNullOrEmpty(Header.PostedBy);
		}

		public void TestIsUseJobExchangeRateApplicable()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var bizO = GetNewBusinessObject() as TransactionPendingAllocation;
			Assert(!bizO.IsUseJobExchangeRateApplicable_ForTestOnly);
			AssertEquals(Constants.CurrencyCodes.Australia, bizO.AH_RX_NKTransactionCurrency);
			AssertEquals(true, bizO.ExchangeRate.IsRateReadOnly);
			AssertEquals(false, bizO.UseJobExchangeRate);
			AssertEquals(false, bizO.AH_PostedToEFT);

			bizO.AH_RX_NKTransactionCurrency = "USD";
			Assert(!bizO.IsUseJobExchangeRateApplicable_ForTestOnly);
			AssertEquals(Constants.CurrencyCodes.UnitedStates, bizO.AH_RX_NKTransactionCurrency);
			AssertEquals(false, bizO.ExchangeRate.IsRateReadOnly);
			AssertEquals(false, bizO.UseJobExchangeRate);
			AssertEquals(false, bizO.AH_PostedToEFT);
		}

		public void TestAutoCreateComplianceDocumentWhenAllocateTransaction()
		{
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			TestObjectCreator.AALSHI.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			Factory.Save();

			var unallocatedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("TEST001", TestObjectCreator.AALSHI, 100m);
			Factory.Save();

			AssertEquals("Pre-condition", true, unallocatedTransaction.IsInDatabase);
			AssertEquals("Auto-create compliance document feature does NOT apply to unallocated transactions", false, unallocatedTransaction.CanCreateComplianceDocument);

			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(unallocatedTransaction).Invoice;
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 100m);
			line.AL_AC = TestObjectCreator.OverheadChargeCode.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_LocalTaxAmount = line.AL_LocalExTaxAmount / 10m;
			line.AL_Desc = Guid.NewGuid().ToString();

			AssertEquals("Pre-condition", true, invoice.IsInDatabase);
			var query = invoice.GetComplianceDocumentByTransactionQuery_ForTestOnly(invoice.PK);
			var complianceDocuments = invoice.Factory.Load<AccComplianceDocumentHeader>(query);
			AssertEquals(0, complianceDocuments.Length);
			AssertEquals("Auto-create compliance document feature applied when post an allocated transaction", true, invoice.CanCreateComplianceDocument);

			invoice.Factory.Save();

			complianceDocuments = invoice.Factory.Load<AccComplianceDocumentHeader>(query);
			AssertEquals("Compliance document created once posted", 1, complianceDocuments.Length);
		}

		public override void TestComplianceWarningIndia()
		{
			Assert("Not applicable", true);
		}

		public override void TestComplianceWarningAustrailia()
		{
			Assert("Not applicable", true);
		}

		public override void TestComplianceWarningPortugal()
		{
			Assert("Not applicable", true);
		}

		public void TestNoteTypes()
		{
			var transaction = (TransactionPendingAllocation)GetNewBusinessObject();
			AssertEquals(2, transaction.NoteTypes.Count);
			AssertCollectionContains(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote, transaction.NoteTypes);
			AssertCollectionContains(PredefinedNoteTypes.Instance.DataImportLogNote, transaction.NoteTypes);
		}

		protected override void CreateAndAssertInvoiceTaxDate()
		{
			var transaction = PrepareTransactionHeaderForTest() as TransactionPendingAllocation;
			AssertEquals(ZDateTime.Empty, transaction.InvoiceTaxDate);
		}

		[TestDate(2025, 3, 26)]
		[ExpectException(typeof(ZSaveException))]
		public void TestSaveDuplicateTransactionNumberSameYear_Standard()
		{
			AssertSaveDuplicateTransactionNumber(
				AllowDuplicateInvoiceNumberRule.STD,
				invoiceDate1: ZDateTime.Now.AddMonths(-2),
				invoiceDate2: ZDateTime.Today,
				expectValidationError: true);
		}

		[TestDate(2025, 3, 26)]
		[ExpectException(typeof(ZSaveException))]
		public void TestSaveDuplicateTransactionNumberSameYear_Calendar()
		{
			AssertSaveDuplicateTransactionNumber(
				AllowDuplicateInvoiceNumberRule.CAL,
				invoiceDate1: ZDateTime.Now.AddMonths(-2),
				invoiceDate2: ZDateTime.Today,
				expectValidationError: true);
		}

		[TestDate(2025, 3, 26)]
		[ExpectException(typeof(ZSaveException))]
		public void TestSaveDuplicateTransactionNumberPreviousYearButLess12Months_Standard()
		{
			AssertSaveDuplicateTransactionNumber(
				AllowDuplicateInvoiceNumberRule.STD,
				invoiceDate1: ZDateTime.Now.AddMonths(-5),
				invoiceDate2: ZDateTime.Today,
				expectValidationError: true);
		}

		[TestDate(2025, 3, 26)]
		[ExpectNoExceptions()]
		public void TestSaveDuplicateTransactionNumberPreviousYearButLess12Months_Calendar()
		{
			AssertSaveDuplicateTransactionNumber(
				AllowDuplicateInvoiceNumberRule.CAL,
				invoiceDate1: ZDateTime.Now.AddMonths(-5),
				invoiceDate2: ZDateTime.Today,
				expectValidationError: false);
		}

		[TestDate(2025, 3, 26)]
		[ExpectNoExceptions()]
		public void TestSaveDuplicateTransactionNumberOver12Months_Standard()
		{
			AssertSaveDuplicateTransactionNumber(
				AllowDuplicateInvoiceNumberRule.STD,
				invoiceDate1: ZDateTime.Now.AddMonths(-13),
				invoiceDate2: ZDateTime.Today,
				expectValidationError: false);
		}

		[TestDate(2025, 3, 26)]
		[ExpectNoExceptions()]
		public void TestSaveDuplicateTransactionNumberOver12Months_Calendar()
		{
			AssertSaveDuplicateTransactionNumber(
				AllowDuplicateInvoiceNumberRule.CAL,
				invoiceDate1: ZDateTime.Now.AddMonths(-13),
				invoiceDate2: ZDateTime.Today,
				expectValidationError: false);
		}

		void AssertSaveDuplicateTransactionNumber(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate1, ZDateTime invoiceDate2, bool expectValidationError)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("ABC", org, -200);
				transaction.AH_InvoiceDate = invoiceDate1;
				transaction.Validation.ValidateAH_TransactionNum();
				transaction.AH_TransactionType = "IPA";
				AssertNoErrors(transaction.AH_TransactionNumInfo);
				Factory.Save();

				var transaction2 = TestObjectCreator.CreateTransactionPendingAllocation("ABC", org, 500);
				transaction2.AH_InvoiceDate = invoiceDate2;
				transaction2.AH_TransactionType = "IPA";
				transaction2.Validation.ValidateAH_TransactionNum();

				if (expectValidationError)
				{
					AssertHasErrorContaining(transaction2.AH_TransactionNumInfo, "The transaction number is already in use by Transaction Pending Allocation.");
					AssertNull(transaction2.GetPreviousSameNumberTransactionDetails());
				}
				else
				{
					AssertNoErrors(transaction2.AH_TransactionNumInfo);
					AssertNotNull(transaction2.GetPreviousSameNumberTransactionDetails());
				}

				Factory.Save();
			}
		}

		#region Not Applicable Tests

		public override void TestInvoicingBaseTaxRecordParent_RunOnSavingOperations_IsCalledOnSaving()
		{
			Assert("Not Applicable as no lines here", true);
		}

		protected override bool CanSetOtherTaxes => false;

		protected override void AssertRelatedFieldsCountOnOSAndLocalTaxAmountOtherTaxes(bool isEnableNewOSOutstandingAmountFeature)
		{
			Assert("Not Applicable as no lines here", true);
		}

		public override void TestUpdateAH_OSTotalAmountCountOnOtherTaxesAmount()
		{
			Assert("Not Applicable as no lines here", true);
		}

		public override void TestRevenueRecognitionTypeNotEmptyWhenJobUpdatedByDataRefreshAfterSetOnLine()
		{
			Assert("Not Job related transaction", true);
		}

		protected override bool IsJobRelatedTransaction => false;

		public override void TestRemoveJobsCreatedButNotRequiredForPostingDisposeJobProperly()
		{
			Assert("Not Applicable", true);
		}

		public new void TestTransactionNumberGenerator()
		{
			Assert("Not applicable", true);
		}

		public override void TestGenerateReverseTransaction()
		{
			Assert("Reversing is not supported", true);
		}

		public override void TestReverseTransaction()
		{
			Assert("Reversing is not supported", true);
		}

		public override void TestValidationTypeWhenReversing()
		{
			Assert("Reversing is not supported", true);
		}

		public override void TestTransactionNumberOnSave()
		{
			Assert("transaction number set by user", true);
		}

		public override void TestImportSingleCostPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public override void TestImportAllApportionmentsFromCostingPreserveIndexOfImportedUniversalTransactionLineValue()
		{
			Assert("Not applicable", true);
		}

		public override void TestAH_GB_TaxBranch()
		{
			Assert("Not applicable", true);
		}

		public new void TestAccrualDepartmentMaintainedAfterImportAccrualsIntoInvoice()
		{
			Assert("Not Applicable", true);
		}

		public new void TestAccrualTaxDateMaintainedAfterImportAccrualsIntoInvoice()
		{
			Assert("Not Applicable", true);
		}

		public new void TestAccrualDepartmentMaintainedAfterImportJobChargesIntoInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAH_ExchangeRateIsSetCorrectlyWhenAH_PostedToEFT()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAH_OSTaxAmountUsesTransactionCompanyNotCurrentCompany()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAllJobsOneJob()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAllJobsThreeJobs()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAllLinesHaveSameBranch()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAllLinesHaveSameDepartment()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAllLinesHaveSameJob()
		{
			Assert("Not Applicable", true);
		}
		public new void TestAPInvoicingForChinaNolongerGeneratesTransactionReferenceInReversing()
		{
			Assert("Not Applicable", true);
		}
		public new void TestApportionedLineModifiedNotRaised()
		{
			Assert("Not Applicable", true);
		}
		public new void TestBalancingLIneTaxAMountWhenCurrentCountryAndCurrencyIsIcelandic()
		{
			Assert("Not Applicable", true);
		}
		public new void TestBalancingLineTaxAmountWhenTaxCalculatedAtHeaderLevel()
		{
			Assert("Not Applicable", true);
		}
		public new void TestBalancingLineTaxAmountWhenTaxCalculatedAtLineLevel()
		{
			Assert("Not Applicable", true);
		}
		public override void TestCalculatePaidOutstandingAmountInSpecificCurrencyOnly()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCalculatingExtraTaxDoesntSetHasChanges()
		{
			Assert("Not Applicable", true);
		}
		public new void TestChangeOfComplianceSubtypeReQueueComplianceGeneratedReport()
		{
			Assert("Not Applicable", true);
		}
		public new void TestChangeOfComplianceSubtypeReQueueComplianceNotGeneratedReport()
		{
			Assert("Not Applicable", true);
		}
		public new void TestChangeOfComplianceSubtypeWouldNotReQueueIfThereIsFinalisedComplianceReport()
		{
			Assert("Not Applicable", true);
		}
		public new void TestChangingAH_ExchangeRateReapportionCharges()
		{
			Assert("Not Applicable", true);
		}
		public new void TestConsolInvoiceType()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCopyFromOriginalReference()
		{
			Assert("Not Applicable", true);
		}
		public new void TestTaxBranchCopyFromOriginalReference()
		{
			Assert("Not Applicable", true);
		}
		public new void TestPlaceOfSupplyCopyFromOriginalReference()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCreateTasksAndMilestonesFromTemplateWhenReversing()
		{
			Assert("Not Applicable", true);
		}
		public new void TestApplyWorkFlowTemplateAfterBOFieldsAreSetWhenReversing()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCreditLimitEmailIsNotSentWhenInInvoiceBatchingContext()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCreditLimitEmailIsNotSentWhenInMatchingContext()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCreditLimitEmailIsNotSentWhenInReversing()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCreditLimitEmailIsNotSentWhenInvoiceNotSubmittedFromForm()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCreditLimitExceededEmailWillBeSentOnlyOnce()
		{
			Assert("Not Applicable", true);
		}
		public new void TestCustomsInvoiceType()
		{
			Assert("Not Applicable", true);
		}
		public new void TestEDUAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}

		public new void TestExpectedInvoiceTotal()
		{
			Assert("Not Applicable", true);
		}
		public new void TestFreightInvoiceType()
		{
			Assert("Not Applicable", true);
		}
		public new void TestFullyPayForTransLinePay()
		{
			Assert("Not Applicable", true);
		}

		public new void TestFullyPayForTransLinePay_EnableNewOSOutstandingAmountFeature()
		{
			Assert("Not Applicable", true);
		}

		public new void TestFullyPayInvoiceAndCreateCashBasisVATWhenTotalIsZero()
		{
			Assert("Not Applicable", true);
		}
		public new void TestFullyPayInvoiceWhenTotalIsNonZero()
		{
			Assert("Not Applicable", true);
		}
		public new void TestFullyPayInvoiceWhenTotalIsZero()
		{
			Assert("Not Applicable", true);
		}
		public new void TestFullyPayInvoiceWhenTotalIsZeroFullyPaidDateIsNotEmpty()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGeneratePaymentApprovalItems()
		{
			Assert("Not Applicable", true);
		}

		public new void TestGeneratePaymentApprovalItems_EnableNewOSOutstandingAmountFeature()
		{
			Assert("Not Applicable", true);
		}

		public new void TestGenerateReverseTransaction_CopyCharges()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGenerateReverseTransactionWithDifferentCurrencyLines()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGenerateTransLinePayRecords()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGenerateTransLinePayRecords_EnableNewOSOutstandingAmountFeature()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGetSumOfLines()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGetSumOfLinesTwoFields()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGSTAndQSTBasedOnQCTAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGSTInclusiveAmountAfterImportJobChargesIntoInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestGSTInclusiveAmountAfterImportSingleCost()
		{
			Assert("Not Applicable", true);
		}
		public new void TestHasCustomsDisbursementCharges()
		{
			Assert("Not Applicable", true);
		}
		public new void TestHasDisbursementCharges()
		{
			Assert("Not Applicable", true);
		}
		public new void TestHeaderAmountsUpdateSuspenderDoesNotUpdateAllAmountsOnDispose()
		{
			Assert("Not Applicable", true);
		}
		public new void TestImportJobChargesIntoInvoicePreservesOSAmountWhenHeaderCurrencyIsLocal()
		{
			Assert("Not Applicable", true);
		}
		public new void TestImportJobChargesIntoInvoicePreservesOSAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestImportJobChargesIntoInvoiceRecalculatesGSTAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInitializeDuplicateLinesSequenceLookup()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInvoiceJobReferenceConsolWithMultipleShipmentInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInvoiceJobReferenceConsolWithOneLine()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInvoiceJobReferenceConsolWithSingleShipmentInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInvoiceJobReferenceLoadListConsolInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInvoiceJobReferenceSingleDeclarationInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInvoiceJobReferenceSingleShipmentInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestInvoiceJobRevenueRecognitionDate_FactoryIndependent()
		{
			Assert("Not Applicable", true);
		}

		public new void TestInvoiceJobRevenueRecognitionDate_WithCurrentDateRegistryOn()
		{
			Assert("Not Applicable", true);
		}

		public new void TestIsAllPaidInTheSameCurrency()
		{
			Assert("Not Applicable", true);
		}
		public new void TestIsARAP()
		{
			Assert("Not Applicable", true);
		}
		public new void TestIsBelongToMultipleJobs()
		{
			Assert("Not Applicable", true);
		}
		public new void TestIsInDBNotRecalculateAH_JH()
		{
			Assert("Not Applicable", true);
		}
		public new void TestIsJobRelatedInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestIsSelfBillingInvoice()
		{
			Assert("Not Applicable", true);
		}
		public override void TestLevelAuthorization()
		{
			Assert("Not Applicable", true);
		}
		public new void TestLines()
		{
			Assert("Not Applicable", true);
		}
		public new void TestLinesAreReadOnlyAfterSaving()
		{
			Assert("Not Applicable", true);
		}
		public new void TestLineSequence()
		{
			Assert("Not Applicable", true);
		}
		public new void TestLoadedLinesOrderedBySequence()
		{
			Assert("Not Applicable", true);
		}
		public new void TestLoadLines()
		{
			Assert("Not Applicable", true);
		}
		public new void TestLocalPartialPaymentAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestMiscellaneousInvoiceTypeForNoForeignTableLinkFromJob()
		{
			Assert("Not Applicable", true);
		}
		public new void TestMiscellaneousInvoiceTypeWithDifferentForeignTable()
		{
			Assert("Not Applicable", true);
		}
		public new void TestMiscellaneousLinesHaveAL_ReverseDateSet()
		{
			Assert("Not Applicable", true);
		}
		public new void TestOSPartialPaymentAmountComesFromLinesAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestOSTaxAmountIsAlwaysZeroWhenLineTaxIsZero()
		{
			Assert("Not Applicable", true);
		}
		public new void TestOSTaxAmountNotReadOnlyAfterImportChargesIntoInvoice()
		{
			Assert("Not Applicable", true);
		}
		public new void TestOTO6AmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}
		public override void TestPostDateOnLinesSameAsHeader()
		{
			Assert("Not Applicable", true);
		}
		public new void TestProxyRaiseMutexErrorFromLinesCollection()
		{
			Assert("Not Applicable", true);
		}
		public new void TestQSTAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}
		public new void TestQueueForComplianceReport()
		{
			Assert("Not Applicable", true);
		}
		public new void TestQueueForComplianceReportWithExemption()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRaiseApportionedLineModified()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRecalculateAH_JH()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRecalculateHeaderAmounts()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRelatedJobsForReversing()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRemoveNotRequiredJobsAndChargesFromCostsAndLinesDoesNotDeleteChargesUsedForApportionment()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRemoveNotRequiredJobsAndChargesFromCostsAndLinesDeletesNotUsedInvoiceLineLevelJobs()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRemoveNotRequiredJobsAndChargesFromCostsAndLines_ActivateJobs()
		{
			Assert("Not Applicable", true);
		}
		public new void TestResetAmounts()
		{
			Assert("Not Applicable", true);
		}
		public new void TestRETAmountUpdatedOnLoad()
		{
			Assert("Not Applicable", true);
		}
		public new void TestReverseFullyPayInvoiceAndCreateCashBasisVATWhenTotalIsZero()
		{
			Assert("Not Applicable", true);
		}
		public new void TestReverseTransactionHasLinesCorrectlyOrdered()
		{
			Assert("Not Applicable", true);
		}

		public new void TestReverseTransactionHasCorrectPlaceOfSupply()
		{
			Assert("Not Applicable", true);
		}
		public new void TestReversingErroneousTransaction()
		{
			Assert("Not Applicable", true);
		}
		public override void TestReversingErroneousTransactionLine()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSetLinesAL_GSTVATBasis()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSettingAH_RXWithZeroExchangeRateDoesNotClearOSAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSettingCurrencyRoundsOSExTax()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSettingFullyPaidDateForZeroValueTransactions()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSetTransactionLinesCurrency()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSetTransactionLinesExchangeRate()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSetWHTReadOnlyState()
		{
			Assert("Not Applicable", true);
		}
		public new void TestSingleJobInvoicePK()
		{
			Assert("Not Applicable", true);
		}
		public new void TestTransportInvoiceType()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_LocalExTaxAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_LocalExTaxAmountWithDefaultNewAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_LocalExtraTaxAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_LocalTaxAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_LocalTaxAmountWithDefaultNewAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_LocalWHTAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_LocalWHTAmountWithDefaultNewAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSExTaxAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSExTaxAmountWithDefaultNewAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSExtraTaxAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSTaxAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSTaxAmountWithDefaultNewAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSTotalAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSTotalAmountWithDefaultNewAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSWHTAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateAH_OSWHTAmountWithDefaultNewAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateComplianceSubTypeAndSequenceNumber_SubTypeNotOK()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateComplianceSubTypeAndSequenceNumber_SubTypeOK_GenerateNumberNotOK()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateComplianceSubTypeAndSequenceNumber_SubTypeOK_GenerateNumberOK()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUpdateTotalsCallAmount()
		{
			Assert("Not Applicable", true);
		}
		public new void TestUseJobExchangeRateDefault()
		{
			Assert("Not Applicable", true);
		}
		public new void TestValidateAH_OSTotalAmountAfterImportJobChargesIntoInvoice()
		{
			Assert("Not Applicable", true);
		}

		public override void TestOSGST()
		{
			Assert("Not Applicable", true);
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			Assert("Not Applicable", true);
		}

		public override void TestReverseTransactionDoeNotSetComplianceSubTypeForNonPeru()
		{
			Assert("Not Applicable", true);
		}

		public override void TestReverseTransactionSetComplianceSubTypeForPeru()
		{
			Assert("Not Applicable", true);
		}

		public new void TestReversingReason()
		{
			Assert("Not Applicable", true);
		}

		public new void TestSetDescription()
		{
			Assert("Not Applicable", true);
		}

		#region SetExchangeRateForInvoicePostingExchangeRateOption

		public new void TestSetExchangeRateForInvoicePostingExchangeRateOption_EIT_NoProperExRateOption()
		{
			Assert("Not Applicable", true);
		}

		public new void TestSetExchangeRateForInvoicePostingExchangeRateOption_EIT_ForeignCurrency()
		{
			Assert("Not Applicable", true);
		}

		public new void TestSetExchangeRateForInvoicePostingExchangeRateOption_EIT_LocalCurrency()
		{
			Assert("Not Applicable", true);
		}

		public new void TestSetExchangeRateForInvoicePostingExchangeRateOption_EIT_NoProperExRateOption_ForceToUpdateLineExRate()
		{
			Assert("Not Applicable", true);
		}

		public new void TestSetExchangeRateForInvoicePostingExchangeRateOption_EIT_ForeignCurrency_ForceToUpdateLineExRate()
		{
			Assert("Not Applicable", true);
		}

		public new void TestSetExchangeRateForInvoicePostingExchangeRateOption_EIT_LocalCurrency_ForceToUpdateLineExRate()
		{
			Assert("Not Applicable", true);
		}

		#endregion

		#region TestExchangeRateRecalculatedFromOtherTaxesAmounts

		public override void TestExchangeRateRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsDEF()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsDEF()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromOSTaxAmountOtherTaxes_WhenAPInvoiceUseJobExchangeRateFlagIsTicked()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPInvoiceUseJobExchangeRateFlagIsTicked()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsTOD()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsTOD()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsINV()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsINV()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsPST()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsPST()
		{
			Assert("Not Applicable", true);
		}

		#endregion

		#endregion

		#region Setup

		protected override Type TypeOfValidation
		{
			get { return typeof(TransactionPendingAllocationValidation); }
		}

		protected override bool ShouldSupportCalculatingTaxAtHeaderLevel
		{
			get { return false; }
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(InvoicingLineBase);
		}

		protected override ZString ExpectedTransactionTypeForIncomplete
		{
			get { throw new NotSupportedException(); }
		}

		protected override BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForTransactionRuleRegistry => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions;

		protected override void CreateHeaderLines()
		{
			//lines can't be created for TransactionPendingAllocation
		}

		#endregion
	}
}
