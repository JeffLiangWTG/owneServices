using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobChargePostingQueue))]
	public class JobChargePostingQueueTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateNew()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			AssertNotNull(charge.Job);
			var queue = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostCost, charge, charge.Job.JH_ParentID, charge.Job.JH_ParentTableCode);

			AssertNotNull(queue);
			AssertEquals("JPQ_GroupID", 0, queue.JPQ_GroupID);
			AssertEquals("JPQ_PostingInstruction", JobChargePostingQueueLookups.PostCost, queue.JPQ_PostingInstruction);
			AssertEquals("JPQ_JR", charge.PK, queue.JPQ_JR);
			AssertEquals("JPQ_ParentID", charge.Job.JH_ParentID, queue.JPQ_ParentID);
			AssertEquals("JPQ_ParentTableCode", charge.Job.JH_ParentTableCode, queue.JPQ_ParentTableCode);

			AssertExceptionThrown("Incorrenct 'QQQ' charge posting instruction type.", typeof(ArgumentException),
				() => JobChargePostingQueue.CreateNew(Factory, "QQQ", charge, charge.Job.JH_ParentID, charge.Job.JH_ParentTableCode));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobChargePostingQueue>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<JobChargePostingQueue>();
		}

		public static void AssertJobChargePostingQueue(JobChargePostingQueue postingRecord, Charge charge, ZGuid jobParentPK, string prefix, int groupId, string postingInstruction)
		{
			AssertEquals("JPQ_GroupID inconsist", groupId, postingRecord.JPQ_GroupID);
			AssertEquals("JPQ_HashVersion inconsist", (byte)0, postingRecord.JPQ_HashVersion);
			AssertEquals("JPQ_JR inconsist", charge.PK, postingRecord.JPQ_JR);
			AssertEquals("JPQ_ParentID inconsist", jobParentPK, postingRecord.JPQ_ParentID);
			AssertEquals("JPQ_ParentTableCode inconsist", prefix, postingRecord.JPQ_ParentTableCode);
			AssertEquals("JPQ_PostingInstruction inconsist", postingInstruction, postingRecord.JPQ_PostingInstruction);
			AssertChargeHash(postingInstruction, charge, postingRecord.JPQ_ChargeValuesHash);
		}

		static void AssertChargeHash(string postingInstruction, Charge charge, ZBlob chargeHashValue)
		{
			byte[] buffer = null;
			switch (postingInstruction)
			{
				case JobChargePostingQueueLookups.PostCost:
					buffer = charge.CalculateCostPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion);
					break;
				case JobChargePostingQueueLookups.PostRevenue:
					buffer = charge.CalculateSellPartHash(AccountingConstants.ChargeHashCalculatorInfo.ChargeSellHashVersion);
					break;
				default:
					break;
			}

			AssertEquals("Charge hash value inconsist", chargeHashValue, buffer);
		}
	}
}
