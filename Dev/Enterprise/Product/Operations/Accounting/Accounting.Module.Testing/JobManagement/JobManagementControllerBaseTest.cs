using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementControllerBase))]
	public class JobManagementControllerBaseTest : ZControllerBasherTest
	{
		public void TestGetFormUsesGenericJob()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			charge.JR_LocalCostAmt = 100m;
			charge.JR_LocalSellAmt = 200m;
			Factory.Save();

			var jobManagement = Factory.Load<JobManagement>(job.PK);
			JobManagementControllerBase controller = (JobManagementControllerBase)ZControllerFactory.Create(GetControllerID());
			using (JobManagementForm form = (JobManagementForm)controller.ShowViewForm(jobManagement))
			{
				Assert(form.BusinessEntity is JobProfitLoss);
				JobProfitLoss profitLoss = (JobProfitLoss)form.BusinessEntity;

				AssertEquals(200m, profitLoss.TotalWIP);
				AssertEquals(-100m, profitLoss.TotalAccrual);

				AssertEquals(1, profitLoss.ProfitLossSummaryDetails.Count);
				AssertEquals(2, profitLoss.ProfitLossDetails.Count);
			}
		}

		public void TestGetFormForInactiveJob()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			job.MarkAsInactive();
			Factory.Save();

			Assert(job.IsCancelled);

			UnitTestUserNotification.Instance.ClearMessages();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var jobManagement = Factory.Load<JobManagement>(job.PK);
			var controller = (JobManagementControllerBase)ZControllerFactory.Create(GetControllerID());
			using (var form = (JobManagementForm)controller.ShowViewForm(jobManagement))
			{
				AssertNotNull(form);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			using (var form = (JobManagementForm)controller.ShowEditForm(jobManagement))
			{
				AssertNotNull(form);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetFormWithNewJobManagement()
		{
			var controller = new JobManagementControllerBaseForTest();
			var newJobManagement = Factory.New<JobManagement>() as IBusiness;
			AssertNull("Get Form With New JobManagement will retrun null", controller.TestGetFrom(newJobManagement));
		}

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			bizObjWithoutAccess.JH_GS_NKRepSales = "U00";
			CRMSecurityProviderTest<JobHeader>.AssertController(new JobManagementControllerBase(), bizObjWithoutAccess, Env.Security.JobManagementCRMSecurity);
		}

		#endregion

		class JobManagementControllerBaseForTest : JobManagementControllerBase
		{
			public IZForm TestGetFrom(IBusiness businessEntity)
			{
				return GetForm(businessEntity);
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			return Factory.Load<JobManagement>(job.PK);
		}

		public override Type ControllerToBashType
		{
			get { return typeof(JobManagementControllerBase); }
		}

		public override void TestDeleteForm()
		{
			Assert("JobManagement does not have a delete form.", true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobHeader;
		}
	}
}
