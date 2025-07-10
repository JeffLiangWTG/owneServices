using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.APAutomation.Testing.APReconciliation
{
	public class InvoicingJobBasedAPReconciliationAccrualSourceTest : TestCaseWithFactory
	{
		public void TestJobReconciliationSourceThrowExceptionWhenInvalidJobParentPKs()
		{
			foreach (var invalidJobParentsInfo in new List<List<ZGuid>> { null, new() })
			{
				var argumentException = AssertExceptionThrown<ArgumentException>(() =>
				{
					var jobParentInfo = invalidJobParentsInfo?.Select(jobParentPk => (jobParentPk, new ZString("JK")))
										?? Enumerable.Empty<(ZGuid, ZString)>();
					new InvoicingJobBasedAPReconciliationAccrualSource(jobParentInfo, SampleCompanyPK);
				});

				AssertNotNull(argumentException);
				AssertContains("jobParentInfo cannot be null or empty.", argumentException.Message);
				AssertEquals("jobParentInfo", argumentException.ParamName);
			}
		}

		public void TestJobReconciliationSourceThrowExceptionWhenCompanyPKIsEmpty()
		{
			var argumentException = AssertExceptionThrown<ArgumentException>(() =>
				new InvoicingJobBasedAPReconciliationAccrualSource(SampleJobParentPKsList
				.Select(jobParentPk => (jobParentPk, new ZString("JK"))), Guid.Empty));
			AssertNotNull(argumentException);
			AssertContains("companyPK cannot be empty.", argumentException.Message);
			AssertEquals("companyPK", argumentException.ParamName);
		}

		public void TestGetJobParentPKsShouldReturnJobParentPKs()
		{
			var source = new InvoicingJobBasedAPReconciliationAccrualSource(
				SampleJobParentPKsList.Select(jobParentPk => (jobParentPk, new ZString("JS"))), SampleCompanyPK);
			var result = source.GetJobParentPKs();
			Assert(SampleJobParentPKsList.SequenceEqual(result));
		}

		public void TestGetConsolJobParentPKsShouldReturnJobParentPKs()
		{
			var source = new InvoicingJobBasedAPReconciliationAccrualSource(
				SampleJobParentPKsList.Select(jobParentPk => (jobParentPk, new ZString("JK"))), SampleCompanyPK);
			var result = source.GetConsolPKs();
			Assert(SampleJobParentPKsList.SequenceEqual(result));
		}

		public void TestFactoryShouldReturnBusinessObjectFactoryInstance()
		{
			var source = new InvoicingJobBasedAPReconciliationAccrualSource(
				SampleJobParentPKsList.Select(jobParentPk => (jobParentPk, new ZString("JK"))), SampleCompanyPK);
			var factory = source.Factory;
			AssertNotNull(factory);
			AssertType<BusinessObjectFactory>(factory);
		}

		public void TestGetChargeAccrualsShouldReturnsExpectedAccruals()
		{
			var shipment1 = ObjectCreator.CreateShipment("S0001");
			var job1 = ObjectCreator.CreateJob(shipment1);
			var charge1 = ObjectCreator.CreateCharge(job1, ObjectCreator.CC1, "charge 1", costCurrency: ObjectCreator.AUD, osCostAmt: 100m, creditor: ObjectCreator.Creditor1);

			var shipment2 = ObjectCreator.CreateShipment("S0002");
			var job2 = ObjectCreator.CreateJob(shipment2);
			var charge2 = ObjectCreator.CreateCharge(job2, ObjectCreator.CC1, "charge 2", costCurrency: ObjectCreator.AUD, osCostAmt: 200m, creditor: ObjectCreator.Creditor1);

			var consol = ObjectCreator.CreateConsol(consolNum: "C0001");
			consol.CostSupporter.Shipments.Add(shipment2);

			Factory.Save();

			var source = new InvoicingJobBasedAPReconciliationAccrualSource(
				new[] { (shipment1.PK, new ZString("JS")), (consol.PK, new ZString("JK")) }, ObjectCreator.DefaultCompanyPK
			);

			var result = source.GetChargeAccruals();
			AssertEquals(2, result.Count());

			var job1Result = result.FirstOrDefault(x => x.JobNumber == "S0001");
			var shipment1Accrual = job1Result.Accruals.FirstOrDefault();
			AssertNotNull(shipment1Accrual);
			AssertEquals("JS", job1Result.JobParentTableCode);
			AssertEquals(AccrualSourceTypes.Job, job1Result.AccrualType);
			Assert(!job1Result.IsRelatedJob);

			var job2Result = result.FirstOrDefault(x => x.JobNumber == "S0002");
			var shipment2Accrual = job2Result.Accruals.FirstOrDefault();
			AssertNotNull(shipment2Accrual);
			AssertEquals("JS", job2Result.JobParentTableCode);
			AssertEquals(AccrualSourceTypes.Job, job2Result.AccrualType);
			Assert(job2Result.IsRelatedJob);
		}

		public void TestGetConsolCostAccrualsShouldReturnsExpectedConsolCosts()
		{
			var consol = ObjectCreator.CreateConsol(consolNum: "C0001");
			var consolCost = ObjectCreator.CreateConsolCost(consol, ObjectCreator.CC1, ObjectCreator.Creditor1);

			Factory.Save();

			var source = new InvoicingJobBasedAPReconciliationAccrualSource(
				new[] { (consol.PK, new ZString("JK")) }, ObjectCreator.DefaultCompanyPK
			);

			var result = source.GetConsolCostAccruals();
			AssertEquals(1, result.Count());

			var consolCostAccrual = result.FirstOrDefault().Accruals.FirstOrDefault();
			AssertNotNull(consolCostAccrual);
		}

		readonly List<ZGuid> SampleJobParentPKsList = [Guid.NewGuid(), Guid.NewGuid()];
		readonly ZGuid SampleCompanyPK = Guid.NewGuid();

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
