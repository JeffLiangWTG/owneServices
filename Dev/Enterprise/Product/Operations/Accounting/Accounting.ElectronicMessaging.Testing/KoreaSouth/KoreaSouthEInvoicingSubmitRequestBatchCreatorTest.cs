using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingSubmitRequestBatchCreator))]
	public class KoreaSouthEInvoicingSubmitRequestBatchCreatorTest : KoreaSouthEInvoicingBatchCreatorTest
	{
		protected override string ActionType => EInvoicingPivotActionType.Submit;

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new KoreaSouthEInvoicingSubmitRequestBatchCreator(company);
		}

		public void TestPerformBatching_SubmitRequestOnly()
		{
			var batchCreatorSUB = GetBatchProcessor(GlbCompany.CurrentCompany);
			var pivotSTA = CreatePivot(EInvoicingPivotActionType.StatusCheck);
			var pivotSUB = CreatePivot(EInvoicingPivotActionType.Submit);
			ResetBatchStatus();
			Factory.Save();

			batchCreatorSUB.PerformBatching(new DetailedLoggerForTest());
			AssertBatchedPivoits("KoreaSouthEInvoicingSubmitRequestBatchCreator shoul only get 'Submit' pivots.", pivotSUB.PK);
		}

		public void TestPerformBatching_MultipleBranch()
		{
			AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);

			var pivot1_NonCurrentBranch = CreatePivot(ActionType);
			pivot1_NonCurrentBranch.ParentTransactionHeader.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			var pivot2 = CreatePivot(ActionType);
			var pivot3 = CreatePivot(ActionType);

			Factory.Save();

			var batchCreator = GetBatchProcessor(GlbCompany.CurrentCompany);
			batchCreator.PerformBatching(new DetailedLoggerForTest());

			var batches = Factory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals(2, batches.Length);
			batches.ToList().ForEach(x => x.TransactionPivots.Reload(true));
			batches.Single(x => x.TransactionPivots.Count == 1 && x.TransactionPivots[0] == pivot1_NonCurrentBranch);
			batches.Single(x => x.TransactionPivots.Count == 2 && x.TransactionPivots.Contains(pivot2) && x.TransactionPivots.Contains(pivot3));
		}

		public void TestTransactionGrouping_CreateDifferentBatches_TransactionsWithTheSameUniqueKeys_KoreaSouth()
		{
			AssertTransactionGroupingWithTheSameUniqueKeys(CountryCodes.KoreaSouth, 1);
		}
	}
}
