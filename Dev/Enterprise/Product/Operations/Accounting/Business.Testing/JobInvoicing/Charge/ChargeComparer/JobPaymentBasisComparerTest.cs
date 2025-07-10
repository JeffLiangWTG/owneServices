using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class JobPaymentBasisComparerTest : TestCaseWithFactory
	{
		public void TestEquals_Cost()
		{
			AssertEquals(true);
		}

		public void TestEquals_Sell()
		{
			AssertEquals(false);
		}

		void AssertEquals(bool isCost)
		{
			var shipment = TestObjectCreator.CreateShipment("S10052018");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var currency = TestObjectCreator.AUD;

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 88, null, currency, 88, null);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 20, null, currency, 20, null);
				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, job.JH_JobNum, currency, 10, null, currency, 10, null);

				charge1.AddPaymentBases(new[] { PaymentBasis }, isCost);
				charge2.AddPaymentBases(new[] { PaymentBasis }, isCost);

				var jobPaymentBases1 = isCost ? charge1.CostPaymentBases : charge1.SellPaymentBases;
				var jobPaymentBases2 = isCost ? charge2.CostPaymentBases : charge2.SellPaymentBases;

				AssertContainsExactElementsInAnyOrder(Comparer, jobPaymentBases1, jobPaymentBases2);

				charge3.AddPaymentBases(new[] { DifferentPaymentBasis }, isCost);
				var jobPaymentBasis3 = isCost ? charge3.CostPaymentBases.First() : charge3.SellPaymentBases.First();

				AssertEquals("New job payment basis should not be found in first collection", false, jobPaymentBases1.Any(x => Comparer.Equals(x, jobPaymentBasis3)));
				AssertEquals("New job payment basis should not be found in second collection", false, jobPaymentBases2.Any(x => Comparer.Equals(x, jobPaymentBasis3)));
			}
		}

		protected PaymentBasis PaymentBasis
		{
			get
			{
				var quantity = new Quantity(1, QuantityUnit.HB);
				var rateInfo = RateInfo.CreateFLT(20, Constants.CurrencyCodes.Australia);

				return new PaymentBasis(quantity, rateInfo, AdapterType.Shipment, "S00001234");
			}
		}

		protected virtual PaymentBasis DifferentPaymentBasis
		{
			get
			{
				var quantity = new Quantity(1, QuantityUnit.HB);
				var rateInfo = RateInfo.CreateFLT(20, Constants.CurrencyCodes.Australia);

				return new PaymentBasis(quantity, rateInfo, AdapterType.Shipment, "S00009876");
			}
		}

		protected virtual IEqualityComparer<JobPaymentBasis> Comparer => null;

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}