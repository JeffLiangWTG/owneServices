using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class OperationalActionsTest : TestCaseWithFactory
	{
		public void TestOperationalActionCanInitializeJobParent()
		{
			var creator = new TestObjectCreator(Factory);

			//create GlbStaff
			var staff = creator.Staff;

			//create operational action
			var action = Factory.New<OperationalAction>();
			action.Context = new OperationalActionContext(new ForwardingShipmentSupporter(), "Shipments", "SHP");
			var descriptor = action.FieldDescriptors.AddNew();
			descriptor.FieldName = "Job+JH_GS_NKRepSales";
			descriptor.Order = 1;
			descriptor.EmptyBehaviour = EmptyBehaviourList.Codes.Skip;

			Factory.Save();

			//create shipments
			var s1 = creator.CreateShipment("S0001", false);
			var s2 = creator.CreateShipment("S0002", false);
			var s3 = creator.CreateShipment("S0003", false);
			var s4 = creator.CreateShipment("S0004", false);

			//initialize jobs
			var s2Job = new Job.Loader(s2).TryLoadOrCreateWithoutMutexForTestOnly();
			var s3Job = new Job.Loader(s3).TryLoadOrCreateWithoutMutexForTestOnly();

			Factory.Save();

			//precondition
			AssertEquals("", s2Job.JH_GS_NKRepSales);
			AssertEquals("", s3Job.JH_GS_NKRepSales);
			AssertNoErrors(s2Job.JH_GS_NKRepSalesInfo);
			AssertNoErrors(s3Job.JH_GS_NKRepSalesInfo);

			//run action making sure Job.Parent is correctly initialized
			var factoryForChanges = new BusinessObjectFactory();
			var dummyLog = new DummyOperationalActionLog();
			var selectedRecords = new SelectedRecords() { PrimaryKeys = new[] { s1.PK, s2.PK, s3.PK, s4.PK } };
			var runner = new OperationalActionRunner(action, typeof(ForwardingShipment), selectedRecords);
			runner.Printer = ZGuid.NewZGuid();
			runner.Fields[0][RunnerTextField.Schema.Property] = staff.GS_Code;
			runner.Run(dummyLog, factoryForChanges);
			factoryForChanges.Save();

			//assert assigned with no errors
			var s2JobReloaded = factoryForChanges.Load<Job>(s2Job.PK);
			var s3JobReloaded = factoryForChanges.Load<Job>(s3Job.PK);

			AssertEquals("TST", s2JobReloaded.JH_GS_NKRepSales);
			AssertEquals("TST", s3JobReloaded.JH_GS_NKRepSales);

			AssertNoErrors(s2JobReloaded.JH_GS_NKRepSalesInfo);
			AssertNoErrors(s3JobReloaded.JH_GS_NKRepSalesInfo);
		}
	}
}
