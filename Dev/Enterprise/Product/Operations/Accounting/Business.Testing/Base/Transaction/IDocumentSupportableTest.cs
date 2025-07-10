using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class IDocumentSupportableTest : TestCaseWithFactory
	{
		public void TestGetDocBusinessObject()
		{
			AssertNull(ARTransaction.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Shipment, null));
			AssertNotNull(ARTransaction.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null));
			AssertNotNull(ARTransaction.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Cheques, null));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				var transactionTypes = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
				foreach (CodeDescriptionPair transactionType in transactionTypes)
				{
					ARTransaction.AH_TransactionType = transactionType.Code;
					if (VoucherProviderFactory.SupportList.Contains(ARTransaction.AH_TransactionType))
					{
						AssertNotNull(ARTransaction.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.AccountingVoucher, null));
					}
					else
					{
						AssertNull(ARTransaction.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.AccountingVoucher, null));
					}
				}
			}
		}

		public void TestIsAllowedReturnsFalseWhenCashBookAllowFuturePostingOfCashBookTransactionsIsNullForCashBookLedger()
		{
			TestObjectCreator.ResetSecurityCore();
			AssertEquals(false, Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint());
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert(AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value);

			ARTransaction.AH_Ledger = LedgerTypes.CashBook;
			ARTransaction.AH_TransactionType = TransactionTypes.DirectPayment;
			Assert("CashBookAllowFuturePostingOfTransactions.IsAllowed should be false for CashBook", !ARTransaction.AllowFuturePostingForReceiptPaymentOnInvoice);
		}

		public void TestIsAllowedReturnsFalseWhenCashBookAllowFuturePostingOfCashBookTransactionsIsNullForAccountsReceivableLedger()
		{
			TestObjectCreator.ResetSecurityCore();
			AssertEquals(false, Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint());
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert(AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value);

			ARTransaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			Assert("CashBookAllowFuturePostingOfTransactions.IsAllowed should be false for AccountsReceivable", !ARTransaction.AllowFuturePostingForReceiptPaymentOnInvoice);
		}
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.ReceivablesCustomiseDocuments, ARTransaction.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("Constants.DataContext.TransactionHeader is Supported", true, ARTransaction.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.TransactionHeader)));
			AssertEquals("Constants.DataContext.Cheques is Supported", true, ARTransaction.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Cheques)));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				var transactionTypes = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
				foreach (CodeDescriptionPair transactionType in transactionTypes)
				{
					ARTransaction.AH_TransactionType = transactionType.Code;
					if (VoucherProviderFactory.SupportList.Contains(ARTransaction.AH_TransactionType))
					{
						AssertEquals("Constants.DataContext.AccountingVoucher is Supported", true, ARTransaction.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.AccountingVoucher)));
					}
					else
					{
						AssertEquals("Constants.DataContext.AccountingVoucher is not Supported", false, ARTransaction.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.AccountingVoucher)));
					}
				}
			}
		}

		public void TestBusinessObject()
		{
			AssertEquals(CargoWise.Definitions.BusinessContext.ARTransaction, ARTransaction.DocumentSupporter.BusinessContext);
		}

		protected ARReceipt ARTransaction;
		protected override void SetUp()
		{
			ARTransaction = Factory.New<ARReceipt>();
			base.SetUp();
		}

		public void TestOperationsJob()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			ForwardingShipment shipment = testObjectCreator.CreateShipment("S00001234");

			Job job = Job.CreateWithMutex(Factory, shipment);
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_JobNum = shipment.JS_UniqueConsignRef;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			ARTransaction.AH_JH = job.PK;
			Factory.Save();

			var genericJob = job.GenericJobView;
			AssertNotNull("Precondition: Checking that GenericJob works", genericJob);

			AssertNotNull("OperationsJob should not be null", ARTransaction.OperationsJob);
			AssertEquals("should be the correct job", job.JH_ParentID, ARTransaction.OperationsJob.PK);
		}

		public void TestEInvoicingDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				batch.AIB_BatchNumber = 1;
				batch.AIB_EHubAllocatedNumber = "123";
				batch.AIB_GovernmentAllocatedNumber = "13579";
				var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
				pivot.AIP_Status = "QUE";
				pivot.AIP_ErrorDescription = "The transaction is invalid";
				pivot.AIP_LastResponseReceivedUtc = new ZDateTime(2018, 1, 31, 16, 45, 33);
				pivot.AIP_LastSentTimeUtc = new ZDateTime(2017, 10, 22, 18, 12, 44);
				pivot.AIP_ParentTableCode = "AH";
				pivot.AIP_AIB = batch.PK;
				pivot.AIP_ParentID = invoice.PK;
				Factory.Save();

				Assert("EInvoicing details only available for countries supporting EInvoicing (eg, Turkey)", ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().DoesCountrySupportElectronicInvoicing(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				AssertEquals("EInvoicing Status should match", pivot.AIP_Status, invoice.EInvoicingStatus);
				AssertEquals("EInvoicing Error should match", pivot.AIP_ErrorDescription, invoice.EInvoicingError);
				AssertEquals("EInvoicing Last Response Received should match", pivot.AIP_LastResponseReceivedUtc, invoice.EInvoicingLastResponseReceivedUtc);
				AssertEquals("EInvoicing Last Sent Time UTC should match", pivot.AIP_LastSentTimeUtc, invoice.EInvoicingLastSentTimeUtc);
				AssertEquals("E-Reporting Batch should match", batch.AIB_BatchNumber.ToString(), invoice.EInvoicingBatchNumber);
				AssertEquals("E-Reporting Govt # should match", batch.AIB_EHubAllocatedNumber, invoice.EInvoicingeHubAllocatedNumber);
				AssertEquals("E-Reporting eHub # should match", batch.AIB_GovernmentAllocatedNumber, invoice.EInvoicingGovernmentAllocatedNumber);
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
