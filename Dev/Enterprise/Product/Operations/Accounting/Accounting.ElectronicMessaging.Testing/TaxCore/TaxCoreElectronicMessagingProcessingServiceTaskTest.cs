using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public abstract class TaxCoreElectronicMessagingProcessingServiceTaskTest<T> : GEIElectronicMessagingProcessingServiceTaskTest<T> where T : TaxCoreElectronicMessagingProcessingServiceTask
	{
		[TestDate(2018, 5, 10)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			var company1 = Helper.CreateCompanyAndBranch("MN1", "BR1", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch("MN2", "BR2", CountryCode, true);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 2);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1 }, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
			}
		}

		[TestDate(2018, 5, 10)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);

			//Create first set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 2);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1 }, logger);

			//Create another set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 2);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1, 1, 1 }, logger);
		}

		[TestDate(2018, 5, 10)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "BR2", CountryCode, true);

			//Create first set of transactions for 2 companies and run the service task.

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 2);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1 }, logger);

			//Create another set of transactions for 2 companies and run the service task again.
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 2);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1, 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1, 1, 1 }, logger);
		}

		[TestDate(2020, 4, 29)]
		public void TestEDIInterchangeIsNotCreatedForCreditNoteWhenOriginalInvoiceIsNotSent()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			Helper.AddCustomsCodeForCountryIfMissing(company1.FirstActiveBranch.OrgProxy, CountryCode, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(TestObjectCreator.AALSHI, CountryCode, "VAT");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company1.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
				arInvoice.Lines[0].AL_AT = taxRate.PK;
				Factory.Save();

				AssertTransactionPivot(Factory, arInvoice.PK, EInvoicingPivotState.Queued, EInvoicingPivotActionType.Submit);

				var reverser = new ARInvoiceReversing(arInvoice as ARInvoice);
				reverser.Reverse();
				Factory.Save();

				var reversedInvoice = arInvoice.ReverseTransaction;
				var creditNote = Factory.Load<ARCreditNote>(reversedInvoice.PK);
				AssertEquals("Reversed AR Transaction type", TransactionTypes.CreditNote, creditNote.AH_TransactionType);

				AssertTransactionPivot(Factory, creditNote.PK, EInvoicingPivotState.Queued, EInvoicingPivotActionType.Cancel);

				Factory.Save();

				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				CombineAssertions(logger.ToString(), () =>
				{
					var newFactory = new BusinessObjectFactory();
					AssertTransactionPivot(newFactory, arInvoice.PK, EInvoicingPivotState.Discarded, EInvoicingPivotActionType.Submit);
					AssertTransactionPivot(newFactory, creditNote.PK, EInvoicingPivotState.Discarded, EInvoicingPivotActionType.Cancel);

					var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company1.PK, EInvoicingBatchState.Discarded);
					AssertEDIInterchanges(new ZQuery(), 0, (interchangePK) => AssertEDIMessages(interchangePK, company1.FirstActiveBranch.PK, GlbDepartment.CurrentDepartment.PK, GetLinkedObjectIDs(batches)));
				});
			}

			void AssertTransactionPivot(BusinessObjectFactory factory, ZGuid transactionPK, string expectedPivotStatus, string expectedPivotActionType)
			{
				var arInvoicePivots = factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPK));
				AssertEquals("Invoice Pivot got created", 1, arInvoicePivots?.Length ?? 0);
				var arInvoicePivot = arInvoicePivots[0];
				AssertEquals("Invoice Pivot Status", expectedPivotStatus, arInvoicePivot.AIP_Status);
				AssertEquals("Invoice Pivot Action Type", expectedPivotActionType, arInvoicePivot.AIP_ActionType);
			}
		}

		protected abstract void AssertLogTextWhenThereIsNoTransactionBatch(string log);

		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			//Please implement this test method in WI00255308
			Assert(true);
		}
	}
}
