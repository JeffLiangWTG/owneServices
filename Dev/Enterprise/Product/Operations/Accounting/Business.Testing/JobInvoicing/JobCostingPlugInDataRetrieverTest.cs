using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobCostingPlugInDataRetrieverTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			JobCostingPlugInDataRetriever retriever = new JobCostingPlugInDataRetriever(Consol1, Factory);
			Assert("Should be the same HostPlugIn", Object.ReferenceEquals(Consol1, retriever.HostPlugIn_ForTestOnly));
			Assert("Should be the same Factory", Object.ReferenceEquals(Factory, retriever.Factory_ForTestOnly));
		}

		public void TestGetApportionments()
		{
			JobCostingPlugInDataRetriever retriever = new JobCostingPlugInDataRetriever(Consol1, Factory);
			AssertNotNull(retriever.GetApportionments());
		}

		public void TestJobsCollection()
		{
			Job job1A = ObjectCreator.CreateJob(Shipment1); // Correct Consol
			Job job3A = ObjectCreator.CreateJob(Shipment3); // Wrong Consol
			Job job5A = ObjectCreator.CreateJob(Shipment5); // No Consol
			Factory.Save();

			// All of these jobs belong to the 'other' company 
			// and shouldn't be included in the Jobs collection
			using (Environment.Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), ObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Job job1B = ObjectCreator.CreateJob(Shipment1);
				Job job2B = ObjectCreator.CreateJob(Shipment2);
				Job job3B = ObjectCreator.CreateJob(Shipment3);
				Job job4B = ObjectCreator.CreateJob(Shipment4);
				Job job5B = ObjectCreator.CreateJob(Shipment5);
				Job job6B = ObjectCreator.CreateJob(Shipment6);
				Factory.Save();
			}

			JobCostingPlugInDataRetriever retriever = new JobCostingPlugInDataRetriever(Consol1, Factory);
			AssertEquals("Jobs.Count", 1, retriever.Jobs.Count());
			Assert("Should contain Job 1", retriever.Jobs.Any(x => x.PK == job1A.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);

			Consol1 = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001001");
			Shipment1 = ObjectCreator.CreateShipment("S00001001", Consol1);
			Shipment2 = ObjectCreator.CreateShipment("S00001002", Consol1);

			Consol2 = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001002");
			Shipment3 = ObjectCreator.CreateShipment("S00001003", Consol2);
			Shipment4 = ObjectCreator.CreateShipment("S00001004", Consol2);

			Shipment5 = ObjectCreator.CreateShipment("S00001005");
			Shipment6 = ObjectCreator.CreateShipment("S00001006");
		}

		ForwardingConsol Consol1;
		ForwardingConsol Consol2;

		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		ForwardingShipment Shipment3;
		ForwardingShipment Shipment4;
		ForwardingShipment Shipment5;
		ForwardingShipment Shipment6;

		TestObjectCreator ObjectCreator;
	}
}
