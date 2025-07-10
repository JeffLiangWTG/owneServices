using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class WhsPopulateStrategyTest : TestCaseWithFactory
	{
		#region Test Construction

		public void TestConstructorWithNullObject()
		{
			WhsPopulateOrderStrategy mockStrategy;
			try
			{
				mockStrategy = new WhsPopulateOrderStrategy(null);
				Assert("Expected null object exception", false);
			}
			catch
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateEmptyJobStrategyConstructor()
		{
			var strategy = new WhsPopulateEmptyJobStrategy();
		}

		#region TestGetNewPopulationStrategy

		public void TestGetNewPopulationStrategy()
		{
			var strategy1 = WhsPopulateStrategy.NewPopulateStrategy(Factory.New<WhsAdjustment>());
			AssertEquals("WhsAdjustment bizo wrapped by WhsPopulateAdjustmentStrategy", typeof(WhsPopulateAdjustmentStrategy), strategy1.GetType());

			var strategy2 = WhsPopulateStrategy.NewPopulateStrategy(Factory.New<WhsOrder>());
			AssertEquals("WhsOrder bizo wrapped by WhsPopulateOrderStrategy", typeof(WhsPopulateOrderStrategy), strategy2.GetType());

			var strategy3 = WhsPopulateStrategy.NewPopulateStrategy(Factory.New<WhsWorkOrder>());
			AssertEquals("WhsWorkOrder bizo wrapped by WhsPopulateComponentOrderStrategy", typeof(WhsPopulateComponentOrderStrategy), strategy3.GetType());

			var strategy4 = WhsPopulateStrategy.NewPopulateStrategy(Factory.New<WhsDynamicWorkOrder>());
			AssertEquals("WhsWorkOrder bizo wrapped by WhsPopulateComponentOrderStrategy", typeof(WhsPopulateComponentOrderStrategy), strategy4.GetType());

			var strategy5 = WhsPopulateStrategy.NewPopulateStrategy(Factory.New<WhsPickableDocket>());
			AssertEquals("WhsPickableDocket bizo wrapped by WhsPopulatePickableDocketStrategy", typeof(WhsPopulatePickableDocketStrategy), strategy5.GetType().BaseType);

			var strategy6 = WhsPopulateStrategy.NewPopulateStrategy(Factory.New<OrgHeader>());
			AssertEquals("Unknown bizo wrapped by WhsPopulateEmptyJobStrategy", typeof(WhsPopulateEmptyJobStrategy), strategy6.GetType());

			var strategy7 = WhsPopulateStrategy.NewPopulateStrategy(Factory.New<WhsPick>());
			AssertEquals("WhsPick bizo wrapped by WhsPopulatePickStrategy", typeof(WhsPopulatePickStrategy), strategy7.GetType());
		}

		#endregion

		#endregion

		#region Test Properties (default strategy is an empty strategy)

		public void TestDefaultStrategy()
		{
			AssertEquals("ConsolidatedInvoiceRef", "", MockOrEmptyStrategy.ConsolidatedInvoiceRef);
			AssertNull("Consignee", MockOrEmptyStrategy.Consignee);
			AssertNull("ConsigneeAddress", MockOrEmptyStrategy.ConsigneeAddress);
			AssertNull("Destination", MockOrEmptyStrategy.Destination);
			AssertEquals("HandlingInstructions", LabelValuePairWrapper.Empty, MockOrEmptyStrategy.HandlingInstructions);
			AssertNull("IncoTerm", MockOrEmptyStrategy.IncoTerm);
			AssertEquals("InvoiceNumber", "", MockOrEmptyStrategy.InvoiceNumber);
			AssertEquals("JobNumber", "", MockOrEmptyStrategy.JobNumber);
			AssertEquals("JobNumberHeading", "", MockOrEmptyStrategy.JobNumberHeading);
			AssertEquals("SecondaryHeading", "", MockOrEmptyStrategy.SecondaryHeading);
			AssertEquals("SecondaryNumber", "", MockOrEmptyStrategy.SecondaryNumber);
			AssertNull("ServiceLevel", MockOrEmptyStrategy.ServiceLevel);
			AssertEquals("SecondaryReference", LabelValuePairWrapper.Empty, MockOrEmptyStrategy.SecondaryReference);
			AssertEquals("TotalNumberOfPackageLabels", ZInt.Zero, MockOrEmptyStrategy.TotalNumberOfPackageLabels);
			AssertNull("TransportCompany", MockOrEmptyStrategy.TransportCompany);
			AssertEquals("TransportReference", LabelValuePairWrapper.Empty, MockOrEmptyStrategy.TransportReference);
		}

		#endregion

		#region Implementation

		WhsPopulateEmptyJobStrategy MockOrEmptyStrategy
		{
			get { return mockOrEmptyStrategy ?? (mockOrEmptyStrategy = new WhsPopulateEmptyJobStrategy()); }
		}
		WhsPopulateEmptyJobStrategy mockOrEmptyStrategy;

		#endregion
	}
}
