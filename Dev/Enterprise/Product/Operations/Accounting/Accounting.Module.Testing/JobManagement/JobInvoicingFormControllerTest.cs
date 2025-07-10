using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobInvoicingFormController))]
	public class JobInvoicingFormControllerTest : ZControllerBasherTest
	{
		public void TestGetFormUsesJob()
		{
			var (job, controller) = CreateJobInvoicingFormControllerForTest();
			using (var form = (JobInvoicingForm)controller.ShowEditForm(job))
			{
				AssertNotNull(form);
			}
		}

		(Job job, JobInvoicingFormController controller) CreateJobInvoicingFormControllerForTest()
		{
			var factory = new BusinessObjectFactory();
			var job = factory.NewJobForTesting<Job>();
			var consumerType = new DummyConsumerTypeForJobBilling();

			var invoicingSupporterMock = new Mock<IJobInvoicingSupporter>();
			invoicingSupporterMock.Setup(x => x.Job).Returns(job);
			invoicingSupporterMock.Setup(x => x.ConsumerType).Returns(consumerType);

			var plugInMock = new Mock<IJobInvoicingPlugIn>();
			plugInMock.Setup(x => x.InvoicingSupporter).Returns(invoicingSupporterMock.Object);
			plugInMock.As<IBusiness>().Setup(x => x.Factory).Returns(factory);
			job.PlugInData = plugInMock.Object;

			var controller = (JobInvoicingFormController)ZControllerFactory.Create(GetControllerID());

			return (job, controller);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			return Factory.Load<Job>(job.PK);
		}

		public override Type ControllerToBashType
		{
			get { return typeof(JobInvoicingFormController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobInvoicingForm;
		}

		public override void TestDeleteForm()
		{
			Assert("Delete form is not supported.", true);
		}

		#region Security Checkpoints

		public void TestSecurityCheckPoint()
		{
			var (job, controller) = CreateJobInvoicingFormControllerForTest();
			using ((JobInvoicingForm)controller.ShowEditForm(job))
			{
				CombineAssertions(() =>
				{
					AssertEquals("For View", Env.Security.SystemFeatureTest, controller.CheckPointForViewExposedForTest);
					AssertEquals("For New", Env.Security.SystemFeatureTest, controller.CheckPointForNewExposedForTest);
					AssertEquals("For Edit", Env.Security.SystemFeatureTest, controller.CheckPointForEditExposedForTest);
					AssertEquals("For Delete", Env.Security.SystemFeatureTest, controller.CheckPointForDeleteExposedForTest);

					AssertEquals("For CRM View", Env.Security.SystemFeatureTest, controller.GetCheckPointForView(job));
					AssertEquals("For CRM Edit", Env.Security.SystemFeatureTest, controller.GetCheckPointForEdit(job));
					AssertEquals("For CRM Delete", Env.Security.SystemFeatureTest, controller.GetCheckPointForDelete(job));
				});
			}
		}

		#endregion

		class DummyConsumerTypeForJobBilling : JobInvoicingConsumerType
		{
			public DummyConsumerTypeForJobBilling() : base("DUM", (NoResString)"Dummy")
			{
			}

			public override Type BizoType
			{
				get { return typeof(DummyZZBizo); }
			}

			public override ControllerID ControllerID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			public override SecurityCheckpoint DistanceCalculationCheckpoint
			{
				get { return Env.Security.None; }
			}

			public override bool SupportGlowBilling => true;

			public override SecurityCheckpoint JobInvoicingCheckPoint => Env.Security.SystemFeatureTest;
		}
	}
}
