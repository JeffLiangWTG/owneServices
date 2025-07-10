using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingBatchCreator))]
	public abstract class KoreaSouthEInvoicingBatchCreatorTest : EInvoicingBatchCreatorBaseTest
	{
		public void TestPerformBatching()
		{
			var batchCreator = GetBatchProcessor(GlbCompany.CurrentCompany);
			var pivot = CreatePivot(ActionType);

			ResetBatchStatus();
			Factory.Save();
			batchCreator.PerformBatching(new DetailedLoggerForTest());
			AssertBatchedPivoits("send " + ActionType + " when it is QUE status.", pivot.PK);
		}

		public void TestPerformBatching_WhenParentTableCodeIsNotAH()
		{
			AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);

			var complianceDocumentHeader = ObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var pivotSUB = ObjectCreator.CreateEInvoicingTransactionPivot(complianceDocumentHeader, Core.Constants.EInvoicingPivotState.Queued);
			pivotSUB.AIP_ActionType = EInvoicingPivotActionType.Submit;
			Factory.Save();

			var batchCreator = GetBatchProcessor(GlbCompany.CurrentCompany);
			batchCreator.PerformBatching(new DetailedLoggerForTest());

			var batches = Factory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Expect no batch When AIP_ParentTableCode is not AH", 0, batches.Length);
		}

		public void TestPerformBatching_OnlySendQUE()
		{
			var batchCreator = GetBatchProcessor(GlbCompany.CurrentCompany);
			var pivot = CreatePivot(ActionType);

			foreach (var statuCode in new string[] {
				EInvoicingPivotState.AwaitingReview,
				EInvoicingPivotState.Batched,
				EInvoicingPivotState.BatchedWithError,
				EInvoicingPivotState.Delivered,
				EInvoicingPivotState.Discarded,
				EInvoicingPivotState.Failed,
				EInvoicingPivotState.Pending,
				EInvoicingPivotState.Sent,
				EInvoicingPivotState.Succeed })
			{
				ResetBatchStatus();
				pivot.AIP_Status = statuCode;
				Factory.Save();
				batchCreator.PerformBatching(new DetailedLoggerForTest());
				AssertBatchedPivoits("do not send pivot when status is not QUE.", Array.Empty<ZGuid>());
				Factory.ReloadAll<AccEInvoicingTransactionPivot>();
			}

			ResetBatchStatus();
			pivot.AIP_Status = EInvoicingPivotState.Queued;
			Factory.Save();
			batchCreator.PerformBatching(new DetailedLoggerForTest());
			AssertBatchedPivoits("send pivot when status is QUE", pivot.PK);
		}

		protected void AssertBatchedPivoits(string comment, params ZGuid[] pivotPKs)
		{
			var query = new ZDBOnlyQuery(typeof(AccEInvoicingTransactionPivot))
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_AIB, SQLComparisonOperator.NotEqual, null);
			var pivots = Factory.Load<AccEInvoicingTransactionPivot>(query);
			AssertContainsExactElementsInAnyOrder(comment, pivotPKs, pivots.Select(x => x.PK).ToArray());
		}

		protected AccEInvoicingTransactionPivot CreatePivot(string actionType)
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			return TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, actionType, Core.Constants.EInvoicingPivotState.Queued);
		}

		protected void ResetBatchStatus()
		{
			var query = new ZDBOnlyQuery(typeof(AccEInvoicingTransactionPivot))
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, GlbCompany.CurrentCompany.PK);
			foreach (var pivot in Factory.Load<AccEInvoicingTransactionPivot>(query))
			{
				pivot.AIP_AIB = ZGuid.Empty;
				pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Queued;
				pivot.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			}

			Factory.Save();
		}

		protected abstract string ActionType { get; }

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
