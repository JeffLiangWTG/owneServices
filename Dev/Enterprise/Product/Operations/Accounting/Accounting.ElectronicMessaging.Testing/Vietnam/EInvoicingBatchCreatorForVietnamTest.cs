using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForVietnam))]
	public class EInvoicingBatchCreatorForVietnamTest : EInvoicingBatchCreatorBaseTest
	{
		[UseSnapshotProtection]
		[TestDate(2020, 2, 27)]
		public void TestCompanySpecificBatchNumbers()
		{
			var company1 = Helper.CreateCompanyAndBranch("VN1", "BR1",Constants.CountryCodes.VietNam, true);
			Helper.AddCustomsCodeForCountryIfMissing(company1.FirstActiveBranch.OrgProxy,Constants.CountryCodes.VietNam, "VAT");
			var company2 = Helper.CreateCompanyAndBranch("VN2", "BR2",Constants.CountryCodes.VietNam, true);
			Helper.AddCustomsCodeForCountryIfMissing(company2.FirstActiveBranch.OrgProxy,Constants.CountryCodes.VietNam, "VAT");

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			AssertCompanySpecificBatchNumbers(company1, 1);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			AssertCompanySpecificBatchNumbers(company1, 2);
			AssertCompanySpecificBatchNumbers(company2, 1);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			AssertCompanySpecificBatchNumbers(company1, 3);
			AssertCompanySpecificBatchNumbers(company2, 2);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			AssertCompanySpecificBatchNumbers(company1, 4);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			AssertCompanySpecificBatchNumbers(company2, 3);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS, "AA12345");
			AssertCompanySpecificBatchNumbers(company2, 4);
		}

		void AssertCompanySpecificBatchNumbers(GlbCompany company, int batchNumber)
		{
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK,Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("Number of transactions ready for batching.", 1, transactionPivots.Length);

			var processor = GetBatchProcessor(company);
			processor.PerformBatching(new DetailedLoggerForTest());

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK,Constants.EInvoicingBatchState.Ready);
			AssertEquals("Batch number starts as expected.", batchNumber, invoiceBatches[invoiceBatches.Length - 1].AIB_BatchNumber);
		}

		[TestDate(2020, 01, 29)]
		public void TestBatchCreatedForPivotsWithCancelActionType()
		{
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.Batched, Constants.EInvoicingPivotState.Discarded, expectedSUBBatchStatus: Constants.EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.Queued, Constants.EInvoicingPivotState.Discarded, expectedSUBBatchStatus: Constants.EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.Discarded, Constants.EInvoicingPivotState.Discarded);
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.BatchedWithError, Constants.EInvoicingPivotState.Discarded, Constants.EInvoicingPivotState.Discarded, Constants.EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.Failed, Constants.EInvoicingPivotState.Discarded,  Constants.EInvoicingPivotState.Failed);
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.Delivered, Constants.EInvoicingPivotState.Batched,  Constants.EInvoicingPivotState.Delivered);
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.Succeed, Constants.EInvoicingPivotState.Batched,  Constants.EInvoicingPivotState.Succeed);
			AssertTransactionPivotCreation(Constants.EInvoicingPivotState.Sent, Constants.EInvoicingPivotState.Queued,  Constants.EInvoicingPivotState.Sent);
		}

		void AssertTransactionPivotCreation(ZString subPivotStatus, ZString expectedCANPivotStatus, string expectedSUBPivotStatus = Constants.EInvoicingPivotState.Discarded, string expectedSUBBatchStatus = Constants.EInvoicingBatchState.Sent)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(GlbCompany.CurrentCompany.OrgProxy, Constants.CountryCodes.VietNam, OrgCusCode.CodeTypes.VATCode, "0100233488");

				GlbCompany.CurrentCompany.Factory.Save();

				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.VND, 1m, Helper.ObjectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "12345";
				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivot);

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				processor.PerformBatching(new DetailedLoggerForTest());

				var pivotAfterProcess = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivotAfterProcess);

				var batch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterProcess.AIP_AIB));
				AssertNotNull(batch);

				var expectedOriginalTransactionPivotMessage = $"[{subPivotStatus}] Sometimes, original transaction pivots have error messages.";
				pivotAfterProcess.AIP_ErrorDescription = expectedOriginalTransactionPivotMessage;
				pivotAfterProcess.AIP_Status = subPivotStatus;
				pivotAfterProcess.Factory.Save();
				batch.AIB_Status = Constants.EInvoicingBatchState.Sent;
				batch.Factory.Save();

				var arCreditNote = Helper.ObjectCreator.CreateARCreditNote("CRD001", Helper.ObjectCreator.AALSHI);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, Constants.EInvoicingPivotActionType.Cancel);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				arCreditNote.Factory.Save();

				processor.PerformBatching(new DetailedLoggerForTest());

				var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivotAfterCreditNoteProcessed);
				AssertEquals(expectedSUBPivotStatus, pivotAfterCreditNoteProcessed.AIP_Status);
				AssertEquals(expectedOriginalTransactionPivotMessage, pivotAfterCreditNoteProcessed.AIP_ErrorDescription);

				var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
				AssertNotNull(creditPivot);
				AssertEquals(Constants.EInvoicingPivotActionType.Cancel, creditPivot.AIP_ActionType);
				AssertEquals(expectedCANPivotStatus, creditPivot.AIP_Status);

				var batchAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterCreditNoteProcessed.AIP_AIB));
				AssertNotNull(batchAfterCreditNoteProcessed);
				AssertEquals(expectedSUBBatchStatus, batchAfterCreditNoteProcessed.AIB_Status);
			}
		}

		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestCancelInvoiceWhenPivotIsQueued()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(GlbCompany.CurrentCompany.OrgProxy, Constants.CountryCodes.VietNam, OrgCusCode.CodeTypes.VATCode, "0100233488");
				GlbCompany.CurrentCompany.Factory.Save();

				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.TRY, 1m, Helper.ObjectCreator.AALSHI);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
				Factory.Save();

				var arCreditNote = Helper.ObjectCreator.CreateARCreditNote("CRD001", Helper.ObjectCreator.AALSHI);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, Constants.EInvoicingPivotActionType.Cancel);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);

				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing cancellation pivots."));
				AssertEquals(0, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == "Processing all queued pivots."));
				var postFactory = new BusinessObjectFactory();
				var invPivot = LoadPivotForTransaction(postFactory, arInvoice.PK);
				AssertNotNull(invPivot);
				var crdPivot = LoadPivotForTransaction(postFactory, arCreditNote.PK);
				AssertNotNull(crdPivot);
				AssertEquals(Constants.EInvoicingPivotState.Discarded, invPivot.AIP_Status);
				AssertEquals(Constants.EInvoicingPivotState.Discarded, crdPivot.AIP_Status);
			}
		}

		AccEInvoicingTransactionPivot LoadPivotForTransaction(BusinessObjectFactory factory, ZGuid transactionPk)
		{
			return factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPk));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var configurations = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value;
			var configuration = configurations.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;

			configuration = configurations.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations);
		}

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForVietnam(company);
		}
	}
}
