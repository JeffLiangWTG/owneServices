using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class APReconciliationClusterBuilderTest : TestCaseWithFactory
	{
		public void TestDependencyInjection()
		{
			AssertNotNull(APReconciliationClusterBuilder);
			AssertType<APReconciliationClusterBuilder>(APReconciliationClusterBuilder);
		}

		public void TestBuildWithAllJobClustersHaveNonZeroAmount()
		{
			jobCluster1.AIC_Amount = 40m;
			jobCluster2.AIC_Amount = 50m;
			jobCluster3.AIC_Amount = 20m;
			var result = APReconciliationClusterBuilder.Build(draftInvoice);
			AssertEquals("Expect 4 logical cluster created", 4, result.Count());
			AssertEquals("Expect 3 non-header cluster created", 3, result.Count(x => !x.IsHeaderCluster));
			AssertEquals("Expect 1 header cluster created", 1, result.Count(x => x.IsHeaderCluster));

			var nonHeaderCluster1 = result.FirstOrDefault(x => x.Key == jobCluster1.PK.ToString());
			AssertEquals(40m, nonHeaderCluster1.TotalAmount);
			AssertEquals(2, nonHeaderCluster1.Nodes.Count());
			Assert(nonHeaderCluster1.Nodes.Any(x => x.Name == "S0001"));
			Assert(nonHeaderCluster1.Nodes.Any(x => x.Name == "S0002"));
			Assert("Expect no node for S003 due to it has no job", !nonHeaderCluster1.Nodes.Any(x => x.Name == "S0003"));

			var nonHeaderCluster2 = result.FirstOrDefault(x => x.Key == jobCluster2.PK.ToString());
			AssertEquals(50m, nonHeaderCluster2.TotalAmount);
			AssertEquals(1, nonHeaderCluster2.Nodes.Count());
			Assert(nonHeaderCluster2.Nodes.Any(x => x.Name == "C0001"));

			var nonHeaderCluster3 = result.FirstOrDefault(x => x.Key == jobCluster3.PK.ToString());
			AssertEquals(20m, nonHeaderCluster3.TotalAmount);
			AssertEquals(1, nonHeaderCluster3.Nodes.Count());
			Assert(nonHeaderCluster3.Nodes.Any(x => x.Name == "C0002"));

			var headerCluster = result.Single(x => x.IsHeaderCluster);
			AssertEquals(0m, headerCluster.TotalAmount);
			AssertEquals("HEADER_CLUSTER", headerCluster.Key);
			AssertEquals(0, headerCluster.Nodes.Count());
		}

		public void TestBuildWithAllJobClustersHaveZeroAmount()
		{
			var result = APReconciliationClusterBuilder.Build(draftInvoice);
			AssertEquals("Expect only 1 logical cluster created", 1, result.Count());
			AssertEquals("Expect no non-header cluster created", 0, result.Count(x => !x.IsHeaderCluster));
			AssertEquals("Expect 1 header cluster created", 1, result.Count(x => x.IsHeaderCluster));

			AssertNull(result.FirstOrDefault(x => x.Key == jobCluster1.PK.ToString()));
			AssertNull(result.FirstOrDefault(x => x.Key == jobCluster2.PK.ToString()));

			var headerCluster = result.Single(x => x.IsHeaderCluster);
			AssertEquals("Expect header cluster amount same as draft invoice amount", 110m, headerCluster.TotalAmount);
			AssertEquals("HEADER_CLUSTER", headerCluster.Key);
			AssertEquals(4, headerCluster.Nodes.Count());
			Assert(headerCluster.Nodes.Any(x => x.Name == "S0001"));
			Assert(headerCluster.Nodes.Any(x => x.Name == "S0002"));
			Assert("Expect no node for S003 due to it has no job", !headerCluster.Nodes.Any(x => x.Name == "S0003"));
			Assert(headerCluster.Nodes.Any(x => x.Name == "C0001"));
			Assert(headerCluster.Nodes.Any(x => x.Name == "C0002"));
		}

		public void TestBuildWithSomeJobClusterHasNonZeroAmount()
		{
			jobCluster2.AIC_Amount = 50m;
			jobCluster3.AIC_Amount = 20m;
			Factory.Save();

			var result = APReconciliationClusterBuilder.Build(draftInvoice);
			AssertEquals("Expect 3 logical clusters created", 3, result.Count());
			AssertEquals("Expect 2 non-header cluster created", 2, result.Count(x => !x.IsHeaderCluster));
			AssertEquals("Expect 1 header cluster created", 1, result.Count(x => x.IsHeaderCluster));

			AssertNull(result.FirstOrDefault(x => x.Key == jobCluster1.PK.ToString()));

			var nonHeaderCluster1 = result.FirstOrDefault(x => x.Key == jobCluster2.PK.ToString());
			AssertEquals(50m, nonHeaderCluster1.TotalAmount);
			AssertEquals(1, nonHeaderCluster1.Nodes.Count());
			Assert(nonHeaderCluster1.Nodes.Any(x => x.Name == "C0001"));

			var nonHeaderCluster2 = result.FirstOrDefault(x => x.Key == jobCluster3.PK.ToString());
			AssertEquals(20m, nonHeaderCluster2.TotalAmount);
			AssertEquals(1, nonHeaderCluster2.Nodes.Count());
			Assert(nonHeaderCluster2.Nodes.Any(x => x.Name == "C0002"));

			var headerCluster = result.Single(x => x.IsHeaderCluster);
			AssertEquals("Expect header cluster amount equal to draft invoice amount - total of all the non-zero clusters amount", 40m, headerCluster.TotalAmount);
			AssertEquals("HEADER_CLUSTER", headerCluster.Key);
			AssertEquals(2, headerCluster.Nodes.Count());
			Assert(headerCluster.Nodes.Any(x => x.Name == "S0001"));
			Assert(headerCluster.Nodes.Any(x => x.Name == "S0002"));
			Assert("Expect no node for S003 due to it has no job", !headerCluster.Nodes.Any(x => x.Name == "S0003"));
		}

		public void TestAmountSignIsConsideredWhileBuildingClusters()
		{
			jobCluster2.AIC_Amount = 50m;
			jobCluster3.AIC_Amount = 20m;
			Factory.Save();

			foreach (var transactionType in new[] { TransactionTypes.CreditNote, TransactionTypes.Invoice })
			{
				var sign = transactionType == TransactionTypes.CreditNote ? -1 : 1;
				draftInvoice.AIH_TransactionType = transactionType;

				var result = APReconciliationClusterBuilder.Build(draftInvoice);
				AssertEquals("Expect 3 logical clusters created", 3, result.Count());
				AssertEquals("Expect 2 non-header cluster created", 2, result.Count(x => !x.IsHeaderCluster));
				AssertEquals("Expect 1 header cluster created", 1, result.Count(x => x.IsHeaderCluster));

				AssertNull(result.FirstOrDefault(x => x.Key == jobCluster1.PK.ToString()));

				var nonHeaderCluster1 = result.FirstOrDefault(x => x.Key == jobCluster2.PK.ToString());
				AssertEquals(sign * 50m, nonHeaderCluster1.TotalAmount);
				AssertEquals(1, nonHeaderCluster1.Nodes.Count());
				Assert(nonHeaderCluster1.Nodes.Any(x => x.Name == "C0001"));

				var nonHeaderCluster2 = result.FirstOrDefault(x => x.Key == jobCluster3.PK.ToString());
				AssertEquals(sign * 20m, nonHeaderCluster2.TotalAmount);
				AssertEquals(1, nonHeaderCluster2.Nodes.Count());
				Assert(nonHeaderCluster2.Nodes.Any(x => x.Name == "C0002"));

				var headerCluster = result.Single(x => x.IsHeaderCluster);
				AssertEquals("Expect header cluster amount equal to draft invoice amount - total of all the non-zero clusters amount", sign * 40m, headerCluster.TotalAmount);
				AssertEquals("HEADER_CLUSTER", headerCluster.Key);
				AssertEquals(2, headerCluster.Nodes.Count());
				Assert(headerCluster.Nodes.Any(x => x.Name == "S0001"));
				Assert(headerCluster.Nodes.Any(x => x.Name == "S0002"));
				Assert("Expect no node for S003 due to it has no job", !headerCluster.Nodes.Any(x => x.Name == "S0003"));
			}
		}

		AccDraftInvoiceHeader draftInvoice;
		AccDraftInvoiceJobCluster jobCluster1;
		AccDraftInvoiceJobCluster jobCluster2;
		AccDraftInvoiceJobCluster jobCluster3;

		protected override void SetUp()
		{
			base.SetUp();

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			TestObjectCreator.CreateJob(shipment2);
			var shipment3 = TestObjectCreator.CreateShipment("S0003"); // S0003 doesn't have job header in current company
			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C0001");
			var consol2 = TestObjectCreator.CreateConsol(consolNum: "C0002");

			draftInvoice = TestObjectCreator.CreateDraftInvoice("ADI 0001", "ADIR 0001", TestObjectCreator.Creditor1.PK, 110M, 0M, TestObjectCreator.AUD.Code);
			jobCluster1 = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0M);
			var job1_1 = TestObjectCreator.AddJobToTheCluster(jobCluster1, shipment1);
			var job1_2 = TestObjectCreator.AddJobToTheCluster(jobCluster1, shipment2);
			var job1_3 = TestObjectCreator.AddJobToTheCluster(jobCluster1, shipment3);

			jobCluster2 = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0M);
			var job2_1 = TestObjectCreator.AddJobToTheCluster(jobCluster2, consol1);

			jobCluster3 = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0M);
			var job3_1 = TestObjectCreator.AddJobToTheCluster(jobCluster3, consol2);

			Factory.Save();
		}

		IAPReconciliationClusterBuilder APReconciliationClusterBuilder => aPReconciliationClusterBuilder ??= ObjectFactory.Get<IAPReconciliationClusterBuilder>();
		IAPReconciliationClusterBuilder aPReconciliationClusterBuilder;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
