using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.Testing
{
	public class DeleteApportionmentChargesWhenSaveJobConsolCostMonitorTest : TestCaseWithFactory
	{
		public void TestCollectApportionmentChargesInfo()
		{
			var consolCost1 = Factory.NewWithValidTestData<JobConsolCost>();
			var charge1 = consolCost1.ApportionmentCharges.AddNew();
			var charge2 = consolCost1.ApportionmentCharges.AddNew();

			var consolCost2 = Factory.NewWithValidTestData<JobConsolCost>();
			var charge3 = consolCost2.ApportionmentCharges.AddNew();

			var charge4 = Factory.NewWithValidTestData<Charge>();

			var monitor = new DeleteApportionmentChargesWhenSaveJobConsolCostMonitor();
			monitor.CollectApportionmentChargesInfo(consolCost1);
			monitor.CollectApportionmentChargesInfo(consolCost2);

			AssertEquals(3, monitor.Mapping_ForTestOnly.Count);
			AssertEquals(consolCost1.PK, monitor.Mapping_ForTestOnly[consolCost1.ApportionmentCharges[0].PK]);
			AssertEquals(consolCost1.PK, monitor.Mapping_ForTestOnly[consolCost1.ApportionmentCharges[1].PK]);
			AssertEquals(consolCost2.PK, monitor.Mapping_ForTestOnly[consolCost2.ApportionmentCharges[0].PK]);
			AssertEquals(false, monitor.Mapping_ForTestOnly.ContainsKey(charge4.PK));
		}

		public void TestCollectApportionmentChargesInfo_WithSavedConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);

			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC11, 100);
			var charge = consolCost.ApportionmentCharges.AddNew();
			charge.JR_JH = job.PK;
			consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.USD.Code;
			consolCost.E6_ExchangeRate = 5;
			consolCost.E6_OSCostAmount = 100;

			Factory.Save();
			AssertEquals(true, consolCost.IsInDatabase);

			var monitor = new DeleteApportionmentChargesWhenSaveJobConsolCostMonitor();
			monitor.CollectApportionmentChargesInfo(consolCost);

			AssertEquals(0, monitor.Mapping_ForTestOnly.Count);
		}

		public void TestTryGetRelativeJobConsolCostPK()
		{
			var consolCost1 = Factory.NewWithValidTestData<JobConsolCost>();
			var charge1 = consolCost1.ApportionmentCharges.AddNew();
			var charge2 = consolCost1.ApportionmentCharges.AddNew();

			var consolCost2 = Factory.NewWithValidTestData<JobConsolCost>();
			var charge3 = consolCost2.ApportionmentCharges.AddNew();

			var charge4 = Factory.NewWithValidTestData<Charge>();

			var monitor = new DeleteApportionmentChargesWhenSaveJobConsolCostMonitor();
			monitor.Mapping_ForTestOnly.Add(charge1.PK, consolCost1.PK);
			monitor.Mapping_ForTestOnly.Add(charge2.PK, consolCost1.PK);
			monitor.Mapping_ForTestOnly.Add(charge3.PK, consolCost2.PK);

			var result = monitor.TryGetRelativeJobConsolCostPK(consolCost1.ApportionmentCharges[0], out ZGuid consolCostPK);
			AssertEquals(true, result);
			AssertEquals(consolCost1.PK, consolCostPK);

			result = monitor.TryGetRelativeJobConsolCostPK(consolCost1.ApportionmentCharges[1], out consolCostPK);
			AssertEquals(true, result);
			AssertEquals(consolCost1.PK, consolCostPK);

			result = monitor.TryGetRelativeJobConsolCostPK(consolCost2.ApportionmentCharges[0], out consolCostPK);
			AssertEquals(true, result);
			AssertEquals(consolCost2.PK, consolCostPK);

			result = monitor.TryGetRelativeJobConsolCostPK(charge4, out consolCostPK);
			AssertEquals(false, result);
			AssertEquals(ZGuid.Empty, consolCostPK);
		}

		public void TestAddTempService()
		{
			DeleteApportionmentChargesWhenSaveJobConsolCostMonitor monitor = null;

			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Factory))
			{
				monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
				AssertNotNull(monitor);
			}

			monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
			AssertNull(monitor);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
