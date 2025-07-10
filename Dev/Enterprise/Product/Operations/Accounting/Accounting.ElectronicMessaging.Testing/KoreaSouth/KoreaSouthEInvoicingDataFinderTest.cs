using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class KoreaSouthEInvoicingDataFinderTest : TestCaseWithFactory
	{
		public void TestFindSubmitBatch_WhenTheInputBatchIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when batch is null.", () => new KoreaSouthEInvoicingDataFinder().FindSubmitBatch(null));
		}

		public void TestFindStatusCheckBatch_WhenTheInputBatchIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when batch is null.", () => new KoreaSouthEInvoicingDataFinder().FindStatusCheckBatch(null));
		}

		public void TestFindBatch_WhenTheInputBatchHasNoTransactionPivots()
		{
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			AssertEquals("Precondition", 0, batch.TransactionPivots.Count);

			AssertNull(null, new KoreaSouthEInvoicingDataFinder().FindSubmitBatch(batch));
			AssertNull(null, new KoreaSouthEInvoicingDataFinder().FindStatusCheckBatch(batch));
		}

		public void TestFindBatch_WhenTransactionPivotsHasDifferentAIP_ActionType()
		{
			var pivot1 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot1.AIP_ActionType = EInvoicingPivotActionType.Cancel;

			var pivot2 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot2.AIP_ActionType = EInvoicingPivotActionType.Reject;

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.TransactionPivots.Add(pivot1);
			batch.TransactionPivots.Add(pivot2);
			AssertEquals("Precondition", 2, batch.TransactionPivots.Count);

			var finder = new KoreaSouthEInvoicingDataFinder();
			finder.FindSubmitBatch(batch);

			AssertEquals("InvalidActionTypes", ErrorReporter.LastKeyReported);
			AssertContains("The TransactionPivots under the batch should not have different action types. ", ErrorReporter.LastMessageReported);
			AssertContains($"Batch Info:\r\n\tPK = {batch.PK}", ErrorReporter.LastMessageReported);
			AssertContains($"Pivot #1:\r\n\tPK = {pivot1.PK}", ErrorReporter.LastMessageReported);
			AssertContains($"Pivot #2:\r\n\tPK = {pivot2.PK}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			finder.FindStatusCheckBatch(batch);
			AssertEquals("InvalidActionTypes", ErrorReporter.LastKeyReported);
			AssertContains("The TransactionPivots under the batch should not have different action types. ", ErrorReporter.LastMessageReported);
			AssertContains($"Batch Info:\r\n\tPK = {batch.PK}", ErrorReporter.LastMessageReported);
			AssertContains($"Pivot #1:\r\n\tPK = {pivot1.PK}", ErrorReporter.LastMessageReported);
			AssertContains($"Pivot #2:\r\n\tPK = {pivot2.PK}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestFindBatch_WhenMultipleTransactionPivotsExistOnStatusCheckBatch()
		{
			var (batchSUB, batchSTA) = KREInvoicingTestHelper.SetUpQueryBatch();

			var pivotSTA1 = batchSTA.TransactionPivots[0];
			var pivotSTA2 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivotSTA2.AIP_ActionType = EInvoicingPivotActionType.StatusCheck;
			pivotSTA2.AIP_ParentID = batchSUB.TransactionPivots.Select(x => x.AIP_ParentID).Except(batchSTA.TransactionPivots[0].AIP_ParentID).FirstOrDefault();

			batchSTA.TransactionPivots.Add(pivotSTA2);
			AssertEquals("Precondition", 2, batchSTA.TransactionPivots.Count);

			var finder = new KoreaSouthEInvoicingDataFinder();
			finder.FindStatusCheckBatch(batchSUB);

			AssertEquals("InvalidTransactionPivots", ErrorReporter.LastKeyReported);
			AssertContains("Only one AccEInvoicingTransactionPivot should be found. ", ErrorReporter.LastMessageReported);
			AssertContains($"Pivot #1:", ErrorReporter.LastMessageReported);
			AssertContains($"Pivot #2:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestFindBatch_WhenTheInputBatchIsSubmitType()
		{
			var batchSUB = KREInvoicingTestHelper.SetUpSubmitBatch();
			AssertEquals("Precondition", true, batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_ActionType == EInvoicingPivotActionType.Submit));

			var finder = new KoreaSouthEInvoicingDataFinder();
			AssertEquals(batchSUB, finder.FindSubmitBatch(batchSUB));
			AssertNull(finder.FindStatusCheckBatch(batchSUB));
		}

		public void TestFindBatch_WhenTheInputBatchIsQueryType()
		{
			var (batchSUB, batchSTA) = KREInvoicingTestHelper.SetUpQueryBatch();
			AssertEquals("Precondition", true, batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_ActionType == EInvoicingPivotActionType.Submit));
			AssertEquals("Precondition", true, batchSTA.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_ActionType == EInvoicingPivotActionType.StatusCheck));

			var finder = new KoreaSouthEInvoicingDataFinder();

			AssertEquals(batchSUB, finder.FindSubmitBatch(batchSUB));
			AssertEquals(batchSTA, finder.FindStatusCheckBatch(batchSUB));

			AssertEquals(batchSUB, finder.FindSubmitBatch(batchSTA));
			AssertEquals(batchSTA, finder.FindStatusCheckBatch(batchSTA));
		}

		public void TestGetSubmitId()
		{
			var (batchSUB, batchSTA) = KREInvoicingTestHelper.SetUpQueryBatch();
			var finder = new KoreaSouthEInvoicingDataFinder();

			var mockProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockProductRegistrationKey.Setup(x => x.EnterpriseCode).Returns("AAA");
			mockProductRegistrationKey.Setup(x => x.ServerCode).Returns("111");
			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.Setup(x => x.Key).Returns(mockProductRegistrationKey.Object);

			GlbCompany.CurrentCompany.GC_Code = "DKR";
			GlbCompany.CurrentCompany.Factory.Save();

			using (ObjectFactory.Substitute(mockProductRegistration.Object))
			{
				using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem))
				{
					var expectedSubmitId = $"{KoreaSouthRegistryNumber.Production}-{batchSUB.AIB_SystemCreateTimeUtc:yyyyMMdd}-414141444b5231313100000000010001";
					AssertEquals(expectedSubmitId, finder.GetSubmitId(batchSUB));
					AssertEquals(expectedSubmitId, finder.GetSubmitId(batchSTA));
				}

				using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem))
				{
					var expectedSubmitId = $"{KoreaSouthRegistryNumber.Testing}-{batchSUB.AIB_SystemCreateTimeUtc:yyyyMMdd}-414141444b5231313100000000010001";
					AssertEquals(expectedSubmitId, finder.GetSubmitId(batchSUB));
					AssertEquals(expectedSubmitId, finder.GetSubmitId(batchSTA));
				}
			}
		}

		KoreaSouthEInvoicingTestHelper KREInvoicingTestHelper => krEInvoicingTestHelper ?? (krEInvoicingTestHelper = new KoreaSouthEInvoicingTestHelper(TestObjectCreator));
		KoreaSouthEInvoicingTestHelper krEInvoicingTestHelper;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
