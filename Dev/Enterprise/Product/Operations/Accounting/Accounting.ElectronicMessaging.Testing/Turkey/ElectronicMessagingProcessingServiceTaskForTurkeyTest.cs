using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.TestHelpers.Xml;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForTurkey))]
	sealed class ElectronicMessagingProcessingServiceTaskForTurkeyTest : GEIElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForTurkey>
	{
		protected override string ExpectedServicePoint => "XHUB_TR_EINVOICING";

		public override void TestDataProviderType()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			AssertType<ElectronicMessagingProcessingServiceTaskDataProviderForTurkey>(serviceTask.DataProvider);
		}

		[TestDate(2020, 1, 29)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				var company1 = HelperTR.CommonHelper.CreateCompanyAndBranch("TR1", "BR1", CountryCodes.Turkey, true);
				var company2 = HelperTR.CommonHelper.CreateCompanyAndBranch("TR2", "BR2", CountryCodes.Turkey, true);
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(company1.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(company2.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "33334444");

				HelperTR.CreateARINVTransactions(company1.FirstActiveBranch, TestObjectCreator.KDV18);
				HelperTR.CreateARINVTransactions(company2.FirstActiveBranch, TestObjectCreator.KDV18);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 1);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					var serviceTask = GetCountrySpecificServiceTask();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);
					AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1 }, logger);
					AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
				}
			}
		}

		[TestDate(2020, 1, 29)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				var company1 = HelperTR.CommonHelper.CreateCompanyAndBranch("TR1", "BR1", CountryCodes.Turkey, true);
				var company2 = HelperTR.CommonHelper.CreateCompanyAndBranch("TR2", "BR2", CountryCodes.Turkey, true);
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(company1.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(company2.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "33334444");

				HelperTR.CreateARINVTransactions(company1.FirstActiveBranch, TestObjectCreator.KDV18);
				HelperTR.CreateARINVTransactions(company2.FirstActiveBranch, TestObjectCreator.KDV18);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 1);

				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1 }, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1 }, logger);

				HelperTR.CreateARINVTransactions(company1.FirstActiveBranch, TestObjectCreator.KDV18);
				HelperTR.CreateARINVTransactions(company2.FirstActiveBranch, TestObjectCreator.KDV18);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 1);

				serviceTask = GetCountrySpecificServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1 }, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1 }, logger);
			}
		}

		[TestDate(2020, 1, 29)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				var company = HelperTR.CommonHelper.CreateCompanyAndBranch("TR1", "BR1", CountryCodes.Turkey, true);
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");

				HelperTR.CreateARINVTransactions(company.FirstActiveBranch, TestObjectCreator.KDV18);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 1);

				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1 }, logger);

				HelperTR.CreateARINVTransactions(company.FirstActiveBranch, TestObjectCreator.KDV18);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 1);

				serviceTask = GetCountrySpecificServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1 }, logger);
			}
		}

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "sent");
			Factory.Save();
			using (HelperTR.EnableEInvoicingFunctionalityForCompany(HelperTR.TurkeyBranch.Company))
			using (HelperTR.SetEReportingComplianceDateForCompany(HelperTR.TurkeyBranch.Company, ZDateTime.Now.ToDateTime()))
			using (HelperTR.SetUpForTestingEInvoicingTurkeyWithControlAccounts())
			{
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(HelperTR.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				HelperTR.CreateCompanySignatureCredential(HelperTR.TurkeyBranch.Company);

				var arInvoice = HelperTR.CreateARINVTransactions(HelperTR.TurkeyBranch, TestObjectCreator.KDV18);
				AssertBatchesAndPivotsForCompany_BeforeProcess(HelperTR.TurkeyBranch.Company, 0, 1);
				var serviceTask = new ElectronicMessagingProcessingServiceTaskForTurkey();
				InitialiseAndRunTaskSchedule(serviceTask);
				TestEDIMessageBodyTest(TurkeyEInvoiceAPICommandList.Codes.SendReceivablesInvoice);

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				var previousBatch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivot.AIP_AIB));
				previousBatch.AIB_GovernmentAllocatedNumber = HelperTR.GovermentAllocatedNumberForTest;
				Factory.Save();

				HelperTR.CreateAccEInvoicingTransactionPivot(previousBatch, arInvoice, EInvoicingPivotState.Queued, EInvoicingPivotActionType.DocumentAction);
				InitialiseAndRunTaskSchedule(serviceTask);
				TestEDIMessageBodyTest(TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice);

				HelperTR.CreateAccEInvoicingTransactionPivot(previousBatch, arInvoice, EInvoicingPivotState.Queued, EInvoicingPivotActionType.StatusCheck);
				InitialiseAndRunTaskSchedule(serviceTask);
				TestEDIMessageBodyTest(TurkeyEInvoiceAPICommandList.Codes.StatusRequestForReceivablesInvoice);

				var arCreditNote = HelperTR.CreateReverseTransaction(arInvoice);
				HelperTR.CreateAccEInvoicingTransactionPivot(previousBatch, arCreditNote, EInvoicingPivotState.Queued, EInvoicingPivotActionType.Cancel);
				InitialiseAndRunTaskSchedule(serviceTask);
				TestEDIMessageBodyTest(TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice);

				var paInvoice = HelperTR.TestObjectCreator.Factory.NewWithValidTestData<TransactionPendingAllocation>();
				paInvoice.AH_TransactionNum = "TR001";
				paInvoice.AH_ComplianceSubType = "EIN";
				paInvoice.AH_TransactionReference = "EIN2022000001";
				paInvoice.AH_GovernmentAllocatedID = HelperTR.GovermentAllocatedNumberForTest;
				HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(paInvoice, EInvoicingPivotActionType.ConfirmTransactionReceived, EInvoicingPivotState.Queued);
				HelperTR.Factory.Save();
				InitialiseAndRunTaskSchedule(serviceTask);
				TestEDIMessageBodyTest(TurkeyEInvoiceAPICommandList.Codes.SetInvoiceTaken);
			}
		}

		void TestEDIMessageBodyTest(string interchangeType)
		{
			var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery(new ZQuery(EDIInterchangeSchema.EI_GB, HelperTR.TurkeyBranch.Company.FirstActiveBranch.PK), new ZQuery(EDIInterchangeSchema.EI_InterchangeType, interchangeType)));
			AssertEquals(1, interchanges.Length);

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
			AssertEquals(1, messages.Length);

			var message = messages[0];
			var expected = HelperTR.GetTurkeyEInvoiceGEIMessageXml(interchangeType);
			var expectedBytes = Encoding.UTF8.GetBytes(expected);
			XmlComparison.CompareAndAssertXml(expected, message.EM_MessageText);
			AssertEquals(expectedBytes.Length, message.EM_MessageData.Length);
		}

		[UseSnapshotProtection(true)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestTriggerStatusUpdateByServiceTaskForTurkey()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(HelperTR.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), ZDateTime.UtcToday.AddDays(-1).ToDateTime()))
			{
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(HelperTR.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				HelperTR.CreateCompanySignatureCredential(HelperTR.TurkeyBranch.Company);
				var serviceTask = new ElectronicMessagingProcessingServiceTaskForTurkey();

				var invoice1 = HelperTR.CreateARINVTransactions(HelperTR.TurkeyBranch, TestObjectCreator.KDV18);
				invoice1.AH_TransactionReference = "ABC2020000000001";
				var invoice2 = HelperTR.CreateARINVTransactions(HelperTR.TurkeyBranch, TestObjectCreator.KDV18);
				invoice2.AH_TransactionReference = "ABC2020000000002";
				HelperTR.Factory.Save();

				InitialiseAndRunTaskSchedule(serviceTask);

				var pivot1 = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice1.PK));
				var batch1 = Factory.Load<AccEInvoicingBatch>(pivot1.AIP_AIB);
				AssertEquals(EInvoicingPivotActionType.Submit, pivot1.AIP_ActionType);
				AssertEquals(EInvoicingPivotState.Sent, pivot1.AIP_Status);
				AssertEquals(EInvoicingBatchState.Sent, batch1.AIB_Status);

				pivot1.AIP_Status = EInvoicingPivotState.Delivered;
				batch1.AIB_GovernmentAllocatedNumber = ZGuid.NewZGuid().ToString();
				HelperTR.Factory.Save();

				var batch1ForStatus = HelperTR.CreateEInvoicingBatch(123, EInvoicingBatchState.Sent, invoice1, "123456", EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
				var pivot1ForStatus = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(batch1ForStatus, invoice1, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
				pivot1ForStatus.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-60);
				HelperTR.Factory.Save();

				var pivot2 = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice2.PK));
				var batch2 = Factory.Load<AccEInvoicingBatch>(pivot2.AIP_AIB);
				AssertEquals(EInvoicingPivotActionType.Submit, pivot2.AIP_ActionType);
				AssertEquals(EInvoicingPivotState.Sent, pivot2.AIP_Status);
				AssertEquals(EInvoicingBatchState.Sent, batch2.AIB_Status);

				pivot2.AIP_Status = EInvoicingPivotState.Succeed;
				batch2.AIB_GovernmentAllocatedNumber = ZGuid.NewZGuid().ToString();
				HelperTR.Factory.Save();

				var batch2ForStatus = HelperTR.CreateEInvoicingBatch(456, EInvoicingBatchState.Sent, invoice2, "456789", EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
				var pivot2ForStatus = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(batch2ForStatus, invoice2, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
				pivot2ForStatus.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-59);
				HelperTR.Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				processor.PerformBatching(serviceTask.ServiceLogger);

				var batch1AfterServiceTask = Factory.Load<AccEInvoicingBatch>(pivot1ForStatus.AIP_AIB);
				var batch2AfterServiceTask = Factory.Load<AccEInvoicingBatch>(pivot2ForStatus.AIP_AIB);
				var pivot1AfterServiceTask = Factory.Load<AccEInvoicingTransactionPivot>(pivot1ForStatus.PK);
				var pivot2AfterServiceTask = Factory.Load<AccEInvoicingTransactionPivot>(pivot2ForStatus.PK);

				AssertEquals(EInvoicingBatchState.Ready, batch1AfterServiceTask.AIB_Status);
				AssertEquals(EInvoicingBatchState.Sent, batch2AfterServiceTask.AIB_Status);
				AssertEquals(EInvoicingPivotState.Batched, pivot1AfterServiceTask.AIP_Status);
				AssertEquals(EInvoicingPivotState.Sent, pivot2AfterServiceTask.AIP_Status);
				AssertNotEquals(pivot1ForStatus.AIP_LastSentTimeUtc, pivot1AfterServiceTask.AIP_LastSentTimeUtc);
				AssertEquals(pivot2ForStatus.AIP_LastSentTimeUtc.ToString("HH:mm"), pivot2AfterServiceTask.AIP_LastSentTimeUtc.ToString("HH:mm"));
			}
		}

		public void TestTriggerStatusUpdateByServiceTaskWorksWithExistingChecks()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(HelperTR.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), ZDateTime.UtcToday.AddDays(-1).ToDateTime()))
			{
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(HelperTR.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				HelperTR.CreateCompanySignatureCredential(HelperTR.TurkeyBranch.Company);
				var serviceTask = new ElectronicMessagingProcessingServiceTaskForTurkey();

				var invoice = HelperTR.CreateARINVTransactions(HelperTR.TurkeyBranch, TestObjectCreator.KDV18);
				InitialiseAndRunTaskSchedule(serviceTask);

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				AssertNotNull(pivot);
				AssertEquals(EInvoicingPivotState.Sent, pivot.AIP_Status);
				var batch = Factory.Load<AccEInvoicingBatch>(pivot.AIP_AIB);
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				batch.AIB_GovernmentAllocatedNumber = ZGuid.NewZGuid().ToString();
				HelperTR.Factory.Save();

				var batchForStatus = HelperTR.CreateEInvoicingBatch(123, EInvoicingBatchState.Sent, invoice, "123456", EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
				var pivotForStatus = HelperTR.TestObjectCreator.CreateEInvoicingTransactionPivot(batchForStatus, invoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
				pivotForStatus.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-60);
				HelperTR.Factory.Save();

				InitialiseAndRunTaskSchedule(serviceTask);

				var pivotForStatusAfterServiceTask = Factory.Load<AccEInvoicingTransactionPivot>(pivotForStatus.PK);
				AssertNotEquals(pivotForStatus.AIP_LastSentTimeUtc, pivotForStatusAfterServiceTask.AIP_LastSentTimeUtc);
			}
		}

		protected override (int period, RunsEvery runningEvery) ExpectedServiceTaskRunFrequency => (1, RunsEvery.Hour);

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						null,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Turkey),
				};
			}
		}

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestTurkeyServiceTaskCreatesBatchesForAPInvoiceList()
		{
			HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(HelperTR.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
			HelperTR.CreateCompanySignatureCredential(HelperTR.TurkeyBranch.Company);
			var serviceTask = new ElectronicMessagingProcessingServiceTaskForTurkey();

			//Asserting AP functionalities are necessary for APList Batch Creation by showing APListRequestBAtchId is null when features are inactive.
			//Setting value inactive by setting date two days later.
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(HelperTR.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), ZDate.Today.AddDays(2).ToDateTime()))
			{
				var apTransactionListRequestBatchId = GetApTransactionListRequestBatchId(GlbCompany.CurrentCompany.Country.Code);
				AssertNull("Turkey AP Compliance Features and should be activated for this value to be not null.", apTransactionListRequestBatchId);

				var query = new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK);
				var batchForAPList = Factory.LoadTop1<AccEInvoicingBatch>(query);
				AssertNull(batchForAPList);

				InitialiseAndRunTaskSchedule(serviceTask);

				batchForAPList = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(query);
				AssertNull(batchForAPList);
			}

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_ARAP(HelperTR.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), ZDateTime.UtcToday.AddDays(-1).ToDateTime()))
			{
				var apTransactionListRequestBatchId = GetApTransactionListRequestBatchId(GlbCompany.CurrentCompany.Country.Code);
				var query = new ZQuery(AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber, apTransactionListRequestBatchId);
				var batchForAPList = Factory.LoadTop1<AccEInvoicingBatch>(query);
				AssertNull(batchForAPList);

				InitialiseAndRunTaskSchedule(serviceTask);

				batchForAPList = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(query);
				AssertNotNull(batchForAPList);
				AssertEquals(apTransactionListRequestBatchId, batchForAPList.AIB_GovernmentAllocatedNumber);
				AssertEquals(EInvoicingBatchState.Sent, batchForAPList.AIB_Status);
				AssertEquals(HelperTR.TurkeyBranch.Company.PK, batchForAPList.AIB_GC);
				AssertEquals(ZDateTime.UtcNow, batchForAPList.AIB_SystemCreateTimeUtc);
			}
		}

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestTurkeyServiceTask_CreatesAPBatchWhenNoPreviousBatchOrBatchExpired()
		{
			var apListAutomatedRequestScheduleValue = AccountingMasterFilesRegistry.Instance.APListAutomatedRequestSchedule.GetValueWithoutFallback(HelperTR.TurkeyBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var factory = new BusinessObjectFactory();

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(HelperTR.TurkeyBranch.Company.FirstActiveBranch.PK.ToGuid(), ZDateTime.UtcToday.AddDays(-1).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				HelperTR.CommonHelper.AddCustomsCodeForCountryIfMissing(HelperTR.TurkeyBranch.Company.OrgProxy, CountryCodes.Turkey, OrgCusCode.CodeTypes.VATCode, "11112222");
				HelperTR.CreateCompanySignatureCredential(HelperTR.TurkeyBranch.Company);
				var serviceTask = new ElectronicMessagingProcessingServiceTaskForTurkey();
				var apTransactionListRequestBatchId = GetApTransactionListRequestBatchId(GlbCompany.CurrentCompany.Country.Code);
				var logString = string.Empty;

				var initialBatchCount = Factory.Load<AccEInvoicingBatch>(getNewQuery()).Length;
				AssertEquals("No existing AP batches", 0, initialBatchCount);

				// Running service task to create a batch and taking count of batches
				InitialiseAndRunTaskSchedule(serviceTask);
				var firstBatchForAPList = getAndAssertActiveBatch(true, "The first batch should have been created", ZGuid.Empty);

				// Simulating scenario if active batch is not expired yet
				TestDateAttribute.AddMinutes(apListAutomatedRequestScheduleValue);
				InitialiseAndRunTaskSchedule(serviceTask);
				var existingBatch = getAndAssertActiveBatch(false, "The batch should be the same", firstBatchForAPList.PK);

				// Simulating scenario that disacarded batch does not affect batching process 
				existingBatch.AIB_Status = EInvoicingBatchState.Discarded;
				existingBatch.Factory.Save();
				TestDateAttribute.AddMinutes(apListAutomatedRequestScheduleValue);
				InitialiseAndRunTaskSchedule(serviceTask);
				existingBatch = getAndAssertActiveBatch(false, "The batch should be the same", firstBatchForAPList.PK, EInvoicingBatchState.Discarded);

				// Simulating scenario if active batch is expired
				TestDateAttribute.AddMinutes(1);
				InitialiseAndRunTaskSchedule(serviceTask);
				existingBatch = getAndAssertActiveBatch(true, "A new batch should have been created", existingBatch.PK);

				AccEInvoicingBatch getAndAssertActiveBatch(bool expectNewBatch, string message, ZGuid previousBatchPK, string activeBatchStatus = EInvoicingBatchState.Sent)
				{
					var activeBatch = factory.LoadTop1<AccEInvoicingBatch>(getNewQuery());
					AssertNotNull(activeBatch);
					logString = serviceTask.ServiceLogger.ToString();
					if (expectNewBatch)
					{
						AssertNotEquals(message, previousBatchPK, activeBatch.PK);
						AssertContains($"Start creating AP transaction list batch for company {HelperTR.TurkeyBranch.Company.GC_Code}.", logString);
						AssertContains("AP transaction list request batch has been created.", logString);
					}
					else
					{
						AssertEquals(message, previousBatchPK, activeBatch.PK);
						AssertNotContains($"Start creating AP transaction list batch for company {HelperTR.TurkeyBranch.Company.GC_Code}.", logString);
						AssertNotContains("No AP transaction list request batch was created.", logString);
					}
					AssertEquals(activeBatchStatus, activeBatch.AIB_Status);
					((TestServiceLogger)serviceTask.ServiceLogger).ClearLog();
					return activeBatch;
				}

				ZQuery getNewQuery() =>
					new ZQuery(AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber, apTransactionListRequestBatchId)
					{ OrderBy = AccEInvoicingBatchSchema.Constants.AIB_SystemLastEditTimeUtc + OrderByClause.Descending };
			}
		}

		string GetApTransactionListRequestBatchId(string countryCode) => GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode).ApTransactionListRequestBatchId;

		public void TestTurkeyServiceTask_CreatesValidSETInterchangeMessageWithValidPayload()
		{
			// TO DO: This test should be done when SET pivot action type is applied to DB
			Assert("Turkey Service Task Creates Valid SET Interchange Message with valid Payload for PE Invoices", true);
		}

		#region Inner Class

		public class ElectronicMessagingProcessingServiceTaskForTurkey_ForTest : ElectronicMessagingProcessingServiceTaskForTurkey
		{
			protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
			{
				var countryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode);
				return new EDIInterchangeCreatorForTurkeyTest.MockEDIInterchangeCreatorForTurkeyEInvoicingBatch(company, () => new MockAccEInvoiceBatchToGEIConverter(), countryFactory);
			}
		}

		#endregion

		#region Implementation

		protected override ZString CountryCode => CountryCodes.Turkey;

		protected override ElectronicMessagingProcessingServiceTaskForTurkey GetCountrySpecificServiceTask()
		{
			return new ElectronicMessagingProcessingServiceTaskForTurkey_ForTest();
		}

		TurkeyEInvoiceTestHelper HelperTR => helper ?? (helper = new TurkeyEInvoiceTestHelper());
		TurkeyEInvoiceTestHelper helper;

		EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForTurkey(company);

		#endregion
	}
}
