using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobChargePostingQueueCreatorTest : TestCaseWithFactory
	{
		public void TestDuplicateJobChargePostingQueuesNotCreated()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var queueCreatorFactory = new BusinessObjectFactory();
				var queueCreator = new JobChargePostingQueueCreator(queueCreatorFactory, shipment.PK, JobShipmentSchema.Constants.Prefix);
				queueCreator.AddChargePostingInstruction(chargeLine1.ImportMetaData?.PostingInstruction, charge1);
				queueCreator.CreateJobChargePostingQueueRecords();
				queueCreatorFactory.Save();

				var queuesInCreatorFactory = queueCreatorFactory.Load<JobChargePostingQueue>(new ZQuery());
				AssertEquals("JobChargePostingQueue for charge1 should exist in database.", 1, queuesInCreatorFactory.Count(q => q.JPQ_JR == charge1.PK && q.IsInDatabase));

				queueCreator.AddChargePostingInstruction(chargeLine1.ImportMetaData?.PostingInstruction, charge1);
				queueCreator.AddChargePostingInstruction(chargeLine2.ImportMetaData?.PostingInstruction, charge2);
				queueCreator.AddChargePostingInstruction(chargeLine2.ImportMetaData?.PostingInstruction, charge2);
				queueCreator.AddChargePostingInstruction(chargeLine3.ImportMetaData?.PostingInstruction, charge3);
				queueCreator.AddChargePostingInstruction(chargeLine3.ImportMetaData?.PostingInstruction, charge3);
				queueCreator.CreateJobChargePostingQueueRecords();

				queuesInCreatorFactory = queueCreatorFactory.Load<JobChargePostingQueue>(new ZQuery());
				AssertEquals("Old JobChargePostingQueue for charge1 should exist in database.", 1, queuesInCreatorFactory.Count(q => q.JPQ_JR == charge1.PK && q.IsInDatabase));
				AssertEquals("New JobChargePostingQueue for charge1 should exist in factory.", 1, queuesInCreatorFactory.Count(q => q.JPQ_JR == charge1.PK && !q.IsInDatabase));
				AssertEquals("New JobChargePostingQueue for charge2 should exist in factory.", 1, queuesInCreatorFactory.Count(q => q.JPQ_JR == charge2.PK && !q.IsInDatabase));
				AssertEquals("New JobChargePostingQueues for charge3 should exist in factory.", 2, queuesInCreatorFactory.Count(q => q.JPQ_JR == charge3.PK && !q.IsInDatabase));
			}
		}

		public void TestAddChargePostingInstruction()
		{
			var recordCreator = new JobChargePostingQueueCreator(Factory, shipment.PK, JobShipmentSchema.Constants.Prefix);

			recordCreator.AddChargePostingInstruction(chargeLine1.ImportMetaData?.PostingInstruction, charge1);

			AssertEquals("Should have 1 cost info", 1, recordCreator.CostCollection_ForTestOnly.Count);
			AssertEquals(charge1.PK, recordCreator.CostCollection_ForTestOnly.First().PK);
			AssertEquals("Should have no revenue info", 0, recordCreator.RevenueCollection_ForTestOnly.Count);

			recordCreator.AddChargePostingInstruction(chargeLine2.ImportMetaData?.PostingInstruction, charge2);

			AssertEquals("Should have 1 cost info", 1, recordCreator.CostCollection_ForTestOnly.Count);
			AssertEquals(charge1.PK, recordCreator.CostCollection_ForTestOnly.First().PK);
			AssertEquals("Should have 1 revenue info", 1, recordCreator.RevenueCollection_ForTestOnly.Count);
			AssertEquals(charge2.PK, recordCreator.RevenueCollection_ForTestOnly.First().PK);

			recordCreator.AddChargePostingInstruction(chargeLine3.ImportMetaData?.PostingInstruction, charge3);

			AssertEquals("Should have 2 cost info", 2, recordCreator.CostCollection_ForTestOnly.Count);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { charge1.PK, charge3.PK }, recordCreator.CostCollection_ForTestOnly.Select(x => x.PK));
			AssertEquals("Should have 2 revenue info", 2, recordCreator.RevenueCollection_ForTestOnly.Count);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { charge2.PK, charge3.PK }, recordCreator.RevenueCollection_ForTestOnly.Select(x => x.PK));
		}

		public void TestGroupIdShouldNotIncreaseWhenHandleEmptyInstruction()
		{
			var recordCreator = new JobChargePostingQueueCreator(Factory, shipment.PK, JobShipmentSchema.Constants.Prefix);

			AssertEquals("Precondition", 0, recordCreator.CostCollection_ForTestOnly.Count);
			AssertEquals(0, recordCreator.RevenueCollection_ForTestOnly.Count);

			recordCreator.CreateJobChargePostingQueueRecords();

			var records = Factory.Load<JobChargePostingQueue>(new ZQuery());
			AssertEquals("No JobChargePostingQueue created", 0, records.Length);

			recordCreator.CostCollection_ForTestOnly.Add(charge1);
			recordCreator.CreateJobChargePostingQueueRecords();

			records = Factory.Load<JobChargePostingQueue>(new ZQuery());
			AssertEquals("JobChargePostingQueue should be created with Id 1", 1, records.Length);
		}

		public void TestHandlePostingInstructionWithEmptyAccount()
		{
			var recordCreator = new JobChargePostingQueueCreator(Factory, shipment.PK, JobShipmentSchema.Constants.Prefix);

			AssertEquals("Precondition", null, charge1.CostAccount);

			recordCreator.CostCollection_ForTestOnly.Add(charge1);
			recordCreator.CreateJobChargePostingQueueRecords();

			var records = Factory.Load<JobChargePostingQueue>(new ZQuery());
			AssertEquals("JobChargePostingQueue should be created", 1, records.Length);
		}

		public void TestHandlePostingInstruction()
		{
			charge1.JR_OH_CostAccount = testObjectCreator.Creditor1.PK;
			charge2.JR_OH_SellAccount = testObjectCreator.Debtor1.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var recordCreator = new JobChargePostingQueueCreator(newFactory, shipment.PK, JobShipmentSchema.Constants.Prefix);
			recordCreator.CostCollection_ForTestOnly.Add(charge1);

			var assertHit = false;
			var mock = new Mock<IServiceTaskNudger>();
			mock.Setup(m => m.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>())).Callback(() => { assertHit = true; });

			using (ObjectFactory.Substitute(mock.Object))
			{
				newFactory.Save();
				Assert(!assertHit);

				recordCreator.CreateJobChargePostingQueueRecords();
				newFactory.Save();
				Assert(assertHit);
			}

			var records = Factory.Load<JobChargePostingQueue>(new ZQuery());
			AssertEquals("1 JobChargePostingQueue created", 1, records.Length);
			JobChargePostingQueueTest.AssertJobChargePostingQueue(records[0], charge1, shipment.PK, JobShipmentSchema.Constants.Prefix, 1, JobChargePostingQueueLookups.PostCost);

			records[0].Delete();
			Factory.Save();
			AssertEquals(0, Factory.Load<JobChargePostingQueue>(new ZQuery()).Length);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var guid = new ZGuid();
				newFactory = new BusinessObjectFactory();
				recordCreator = new JobChargePostingQueueCreator(newFactory, guid, JobDeclarationSchema.Constants.Prefix);
				recordCreator.CostCollection_ForTestOnly.Add(charge1);
				recordCreator.RevenueCollection_ForTestOnly.Add(charge2);
				recordCreator.CreateJobChargePostingQueueRecords();
				newFactory.Save();

				records = Factory.Load<JobChargePostingQueue>(new ZQuery());
				AssertEquals("2 JobChargePostingQueue created", 2, records.Length);
				var costRecords = records.Where(x => x.JPQ_PostingInstruction == JobChargePostingQueueLookups.PostCost);
				var revenueRecords = records.Where(x => x.JPQ_PostingInstruction == JobChargePostingQueueLookups.PostRevenue);
				AssertEquals(1, costRecords.Count());
				AssertEquals(1, revenueRecords.Count());
				JobChargePostingQueueTest.AssertJobChargePostingQueue(costRecords.First(), charge1, guid, JobDeclarationSchema.Constants.Prefix, 2, JobChargePostingQueueLookups.PostCost);
				JobChargePostingQueueTest.AssertJobChargePostingQueue(revenueRecords.First(), charge2, guid, JobDeclarationSchema.Constants.Prefix, 2, JobChargePostingQueueLookups.PostRevenue);
			}
		}

		public void TestUnhandledPostingInstruction()
		{
			var handledPostingInstructions = new PostingInstruction[] { PostingInstruction.PostCost,
																		PostingInstruction.PostRevenue,
																		PostingInstruction.PostRevenueAndCost };

			var values = Enum.GetValues(typeof(PostingInstruction));
			AssertContainsExactElementsInAnyOrder(@"All posting instruction values should be handled.
If the UT fails, please handle the new posting instruction value in JobChargePostingInstructionHandler.AddChargePostingInstruction method,
and add the new value into handledPostingInstructions array.", handledPostingInstructions, values);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);

			shipment = testObjectCreator.CreateShipment("S0001", true);
			job = testObjectCreator.CreateJob(shipment);
			charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, 100m, 100m);
			charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, 150m, 150m);
			charge3 = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, 200m, 200m);
			Factory.Save();

			chargeLine1 = CreateCharge(PostingInstruction.PostCost);
			chargeLine2 = CreateCharge(PostingInstruction.PostRevenue);
			chargeLine3 = CreateCharge(PostingInstruction.PostRevenueAndCost);
		}

		ChargeLine CreateCharge(PostingInstruction postingInstruction)
		{
			var chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
			chargeLine.ImportMetaData.PostingInstruction = postingInstruction;
			return chargeLine;
		}

		TestObjectCreator testObjectCreator;
		Charge charge1, charge2, charge3;
		ChargeLine chargeLine1, chargeLine2, chargeLine3;
		ForwardingShipment shipment;
		Job job;
	}
}
