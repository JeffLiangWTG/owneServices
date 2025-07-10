using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class GEIElectronicMessagingProcessingServiceTaskTest<T> : ElectronicMessagingProcessingServiceTaskTest<T> where T : ElectronicMessagingProcessingServiceTask
	{
		public abstract void TestSuccessfulCreatedEDIInterchangeBodyText();

		protected virtual string ExpectedServicePoint => "GLB_ELEC_INVOICING";

		protected override void AssertEDIMessage(EDIMessage message, ZGuid branchPK, ZGuid departmentPK)
		{
			EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(message, branchPK, departmentPK);
		}

		protected override void AssertEDIInterchange(IXmlEDIInterchange interchange, ZString servicePointSuffix)
		{
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(interchange, expectedTo: ExpectedServicePoint);
		}

		protected override ZGuid[] GetLinkedObjectIDs(params AccEInvoicingBatch[] batches) => batches.Select(b => b.PK).ToArray();

		protected void AssertBatchesAndPivotsForCompany_BeforeProcess(GlbCompany company, int batchCount, int pivotCount)
		{
			var invoiceBatchesForCompany = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals($"{batchCount} invoice batches in DB for company {company.GC_Code}", batchCount, invoiceBatchesForCompany.Length);

			var transactionPivot = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals($"{pivotCount} pivots ready for batching in company {company.GC_Code}", pivotCount, transactionPivot.Length);
		}

		protected void AssertBatchesAndEDIInterchangesForCompany_AfterProcess(GlbCompany company, IEnumerable<int> expectedPivotsPerBatchUnordered, TestServiceLogger logger)
		{
			var expectedBatchCount = expectedPivotsPerBatchUnordered.Count();

			CombineAssertions(logger.ToString(), new VoidParameterlessDelegate(() =>
			{
				var apListIdentifier = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(company.Country.Code)?.ApTransactionListRequestBatchId;
				var isValidApListIdentifier = !string.IsNullOrEmpty(apListIdentifier);
				var invoiceBatchesForCompany = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, Core.Constants.EInvoicingBatchState.Sent);
				AssertEquals($"Company {company.GC_Code} has {expectedBatchCount} batches", expectedBatchCount, invoiceBatchesForCompany.Length);

				if (expectedBatchCount > 0)
				{
					var actualPivotsPerBatch = new int[invoiceBatchesForCompany.Length];
					for (int i = 0; i < invoiceBatchesForCompany.Length; i++)
					{
						var batch = invoiceBatchesForCompany[i];
						var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, Core.Constants.EInvoicingPivotState.Sent, batch.PK);

						actualPivotsPerBatch[i] = transactionPivots.Length;

						if (isValidApListIdentifier && batch.AIB_GovernmentAllocatedNumber.EqualsIgnoringCase(apListIdentifier))
						{
							AssertEquals("AP List batch does not have pivot", 0, transactionPivots.Length);
						}
					}

					AssertContainsExactElementsInAnyOrder($"Pivots per batch matches sequence [{string.Join(",", expectedPivotsPerBatchUnordered.Select(bp => bp.ToString()))}] in any order", expectedPivotsPerBatchUnordered, actualPivotsPerBatch);
				}

				AssertEDIInterchanges(new ZQuery(EDIInterchangeSchema.EI_GB, company.FirstActiveBranch.PK), expectedBatchCount, (ei) => AssertEDIMessages(ei, company.FirstActiveBranch.PK, GlbDepartment.CurrentDepartment.PK, GetLinkedObjectIDs(invoiceBatchesForCompany)));
			}));
		}
	}
}
