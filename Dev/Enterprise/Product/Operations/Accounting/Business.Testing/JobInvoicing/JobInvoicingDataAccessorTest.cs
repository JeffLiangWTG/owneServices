using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobInvoicingDataAccessorTest : TestCaseWithFactory
	{
		public void TestSearchIsDoneUsingCompanyAndNotBranch()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			ForwardingShipment newShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			newShipment1.JS_UniqueConsignRef = "S00001001";
			Job newJob1 = Job.CreateWithMutex(Factory, newShipment1);
			newJob1.JH_JobNum = "S00001001";
			newJob1.JH_ParentID = newShipment1.PK;
			newJob1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			newJob1.JH_GB = objectCreator.NonCurrentCompanyBranch.PK;
			newJob1.JH_GC = GlbCompany.CurrentCompany.PK;
			newJob1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			ForwardingShipment newShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			newShipment2.JS_UniqueConsignRef = "S00001002";
			Job newJob2 = Job.CreateWithMutex(Factory, newShipment2);
			newJob2.JH_JobNum = "S00001002";
			newJob2.JH_ParentID = newShipment2.PK;
			newJob2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			newJob2.JH_GB = GlbBranch.CurrentBranch.PK;
			newJob2.JH_GC = GlbCompany.CurrentCompany.PK;
			newJob2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			IJobInvoicingPlugIn[] shipments = new IJobInvoicingPlugIn[] { newShipment1 };
			AssertEquals(1, new JobInvoicingDataAccessor().GetJobsFromShipment(shipments).Length);

			shipments = new IJobInvoicingPlugIn[] { newShipment2 };
			AssertEquals(1, new JobInvoicingDataAccessor().GetJobsFromShipment(shipments).Length);
		}

		public void TestGetNoShipmentsReturnsNothing()
		{
			IJobInvoicingPlugIn[] shipments = System.Array.Empty<IJobInvoicingPlugIn>();
			AssertEquals(0, new JobInvoicingDataAccessor().GetJobsFromShipment(shipments).Length);
		}

		public void TestGetJobsFromShipmentAndGetJobFromForeignKeyWithRelatedJobs()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			ForwardingShipment newMainShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			newMainShipment.JS_UniqueConsignRef = "S00001001";
			newMainShipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			Job newMainJob = Job.CreateWithMutex(Factory, newMainShipment);
			newMainJob.JH_JobNum = "S00001001";
			newMainJob.JH_ParentID = newMainShipment.PK;
			newMainJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			newMainJob.JH_GB = GlbBranch.CurrentBranch.PK;
			newMainJob.JH_GC = GlbCompany.CurrentCompany.PK;
			newMainJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			newMainJob.JH_OA_LocalChargesAddr = objectCreator.AALSHI.Addresses[0].PK;
			AssertEquals("Pre-codition: Consol Invoicing Style should be Master", Core.Constants.ConsolInvoicingStyles.Master, newMainJob.LocalCharges.CompanyData.EffectiveBuyersConsolInvoicingStyle);

			ForwardingShipment newShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			newShipment.JS_UniqueConsignRef = "S00001002";
			Job newJob = Job.CreateWithMutex(Factory, newShipment);
			newJob.JH_JobNum = "S00001002";
			newJob.JH_ParentID = newShipment.PK;
			newJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			newJob.JH_GB = GlbBranch.CurrentBranch.PK;
			newJob.JH_GC = GlbCompany.CurrentCompany.PK;
			newJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			newMainShipment.CoLoadShipments.Add(newShipment);

			ForwardingShipment childShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			childShipment.JS_UniqueConsignRef = "S00001001/A";
			Job childJob = Job.CreateWithMutex(Factory, childShipment);
			childJob.JH_JH_ParentJob = newMainJob.PK;
			childJob.JH_JobNum = "S00001001/A";
			childJob.JH_ParentID = childShipment.PK;
			childJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			childJob.JH_GB = GlbBranch.CurrentBranch.PK;
			childJob.JH_GC = GlbCompany.CurrentCompany.PK;
			childJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			IJobInvoicingPlugIn[] shipments = new IJobInvoicingPlugIn[] { newMainShipment };
			AssertEquals("Jobs from main shipment", 3, new JobInvoicingDataAccessor().GetJobsFromShipment(shipments).Length);

			shipments = new IJobInvoicingPlugIn[] { newShipment };
			AssertEquals("Jobs from shipment", 1, new JobInvoicingDataAccessor().GetJobsFromShipment(shipments).Length);

			shipments = new IJobInvoicingPlugIn[] { childShipment };
			AssertEquals("Jobs from child shipment", 1, new JobInvoicingDataAccessor().GetJobsFromShipment(shipments).Length);

			AssertEquals("GetJobFromForeignKey from main shipment", newMainJob.PK, new JobInvoicingDataAccessor().GetJobFromForeignKey(newMainShipment));
			AssertEquals("GetJobFromForeignKey from shipment", newJob.PK, new JobInvoicingDataAccessor().GetJobFromForeignKey(newShipment));
			AssertEquals("GetJobFromForeignKey from child shipment", childJob.PK, new JobInvoicingDataAccessor().GetJobFromForeignKey(childShipment));
		}
	}
}