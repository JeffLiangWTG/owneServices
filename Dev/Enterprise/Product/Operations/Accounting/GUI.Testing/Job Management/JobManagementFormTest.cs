using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobManagement.Testing
{
	[TestedType(typeof(JobManagementForm))]
	public class JobManagementFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override void SetUp()
		{
			Env.Registry.FreightChargeCode.ToString(); // fixes test failure caused by this registry item being loaded during the test
			base.SetUp();
		}

		protected override Form GetFormToBashCore()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = Factory.New<CommonShipment>();
			consol.Shipments.Add(shipment);

			Factory.Save();

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";
			job.JH_JobNum = shipment.JS_UniqueConsignRef;
			job.JH_GB = Env.CurrentBranch.PK;
			job.JH_GE = Env.CurrentDepartment.PK;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = new TestObjectCreator(Factory).CC1.PK;
			charge.JR_LocalSellAmt = 1818m;

			Factory.Save();

			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { job.PK });

			return new JobManagementForm(profitLoss);
		}

		public void TestDisabledMenuItems()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.CreateShipment("S0001"), false);
			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { job.PK });

			using (JobManagementForm testForm = new JobManagementForm(profitLoss))
			{
				testForm.Show();
				AssertMenuItemIsDisabled(testForm, ZFormMenuStrategy.FileNewMenuItemName);
				AssertMenuItemIsDisabled(testForm, ZFormMenuStrategy.FileSaveMenuItemName);
				AssertMenuItemIsDisabled(testForm, ZFormMenuStrategy.FileSaveAndCloseMenuItemName);
				AssertMenuItemIsDisabled(testForm, ZFormMenuStrategy.FileDeleteMenuItemName);
				//BAssertEquals("Should be false", false, TestForm.PostingButtonsUserControl.SaveButton.Enabled);
			}
		}

		void AssertMenuItemIsDisabled(Form form, string menuItemName)
		{
			MenuItem menuItem = form.Menu.MenuItems.FindByName(menuItemName, true);
			Assert("Should be disabled", menuItem == null || !menuItem.Enabled);
		}

		public void TestTotalCostCalculateTimesWhenSwitchingTab()
		{
			AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			var securityCheckpoint = securityHelper.GetInvSecurity(SecurityCore.GlobalChargeDetails);
			securityCheckpoint.IsAllowed = true;

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = shipment.PK;
			var charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();

			var profitLoss = new DummyJobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });

			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				jobManForm.Show();
				Application.DoEvents();

				var tabControl = jobManForm.Controls.Find("TabControl", true)[0] as TabControl;

				AssertEquals(1, profitLoss.TotalCostCalculateTimes);

				profitLoss.TotalCostCalculateTimes = 0;
				tabControl.SelectTab("DetailsTabPage");
				AssertEquals(1, profitLoss.TotalCostCalculateTimes);

				profitLoss.TotalCostCalculateTimes = 0;
				tabControl.SelectTab("GlobalJobCostingTabPage");
				AssertEquals(0, profitLoss.TotalCostCalculateTimes);
			}
		}

		public void TestOpenShipment()
		{
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = shipment.PK;
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();

			bool originalMaintainShipmentSecurityValue = Env.Security.MaintainShipment.IsAllowed;
			bool originalMaintainShipmentEditSecurityValue = Env.Security.MaintainShipmentEdit.IsAllowed;

			try
			{
				Env.Security.MaintainShipment.IsAllowed = false;
				Env.Security.MaintainShipmentEdit.IsAllowed = false;
				Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.MaintainShipmentCRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
				Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AttemptToShowForm(testJob);
				ZString expectedMessage = Env.Security.MaintainShipment.ErrorMessageForNotAllowed;
				AssertEquals("Security Message should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.MaintainShipment.IsAllowed = true;
				Env.Security.MaintainShipmentEdit.IsAllowed = false;
				AssertFormTypeIsCorrect(testJob, ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingShipmentForm>(), ODisplayMode.ReadOnly);

				Env.Security.MaintainShipment.IsAllowed = true;
				Env.Security.MaintainShipmentEdit.IsAllowed = true;
				Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.MaintainShipmentCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
				Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;
				AssertFormTypeIsCorrect(testJob, ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingShipmentForm>(), ODisplayMode.Browse);
			}
			finally
			{
				Env.Security.MaintainShipment.IsAllowed = originalMaintainShipmentSecurityValue;
				Env.Security.MaintainShipmentEdit.IsAllowed = originalMaintainShipmentEditSecurityValue;
			}
		}

		public void TestOpenLocalCartage()
		{
			CommonCartage localCartage = Factory.NewWithValidTestData<CommonCartage>();
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			testJob.JH_ParentID = localCartage.PK;
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();

			bool originalTransportJobSecurityValue = Env.Security.TransportJob.IsAllowed;
			bool originalTransportJobEditSecurityValue = Env.Security.TransportJobEdit.IsAllowed;

			try
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.Cartage);
				Type formType;
				using (ZForm form = (ZForm)controller.ShowNewForm())
				{
					formType = form.GetType();
				}
				AssertNotNull(formType);

				Env.Security.TransportJob.IsAllowed = false;
				Env.Security.TransportJobEdit.IsAllowed = false;
				Env.Security.TransportJobCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
				Env.Security.TransportJobCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.IgnoreOSMG.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AttemptToShowForm(testJob);
				ZString expectedMessage = Env.Security.TransportJob.ErrorMessageForNotAllowed;
				AssertEquals("Security Message should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.TransportJob.IsAllowed = true;
				Env.Security.TransportJobEdit.IsAllowed = false;
				AssertFormTypeIsCorrect(testJob, formType, ODisplayMode.ReadOnly);

				Env.Security.TransportJob.IsAllowed = true;
				Env.Security.TransportJobEdit.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.IgnoreOSMG.IsAllowed = true;
				AssertFormTypeIsCorrect(testJob, formType, ODisplayMode.Browse);
			}
			finally
			{
				Env.Security.TransportJob.IsAllowed = originalTransportJobSecurityValue;
				Env.Security.TransportJobEdit.IsAllowed = originalTransportJobEditSecurityValue;
			}
		}

		public void TestOpenLocalCartage_Internal()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S12233344";
			Factory.Save();

			CommonCartage localCartage = Factory.NewWithValidTestData<CommonCartage>();

			localCartage.JJ_ParentID = shipment.PK;
			localCartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			localCartage.JJ_ConsignmentID = "S12233344/I";
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			testJob.JH_ParentID = localCartage.PK;
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();

			bool originalTransportJobSecurityValue = Env.Security.TransportJob.IsAllowed;
			bool originalTransportJobEditSecurityValue = Env.Security.TransportJobEdit.IsAllowed;

			try
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.Cartage);
				Type formType;
				using (ZForm form = (ZForm)controller.ShowNewForm())
				{
					formType = form.GetType();
				}
				AssertNotNull(formType);

				Env.Security.TransportJob.IsAllowed = false;
				Env.Security.TransportJobEdit.IsAllowed = false;
				Env.Security.TransportJobCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
				Env.Security.TransportJobCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.IgnoreOSMG.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AttemptToShowForm(testJob);
				ZString expectedMessage = Env.Security.TransportJob.ErrorMessageForNotAllowed;
				AssertEquals("Security Message should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.TransportJob.IsAllowed = true;
				Env.Security.TransportJobEdit.IsAllowed = false;
				AssertFormTypeIsCorrect(testJob, formType, ODisplayMode.ReadOnly);

				Env.Security.TransportJob.IsAllowed = true;
				Env.Security.TransportJobEdit.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.TransportJobCRMSecurity.IgnoreOSMG.IsAllowed = true;
				AssertFormTypeIsCorrect(testJob, formType, ODisplayMode.Browse);
			}
			finally
			{
				Env.Security.TransportJob.IsAllowed = originalTransportJobSecurityValue;
				Env.Security.TransportJobEdit.IsAllowed = originalTransportJobEditSecurityValue;
			}
		}

		public void TestCreateForm_WhenJobProfitLossIsNotValid()
		{
			ErrorReporter.Clear();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = ZGuid.NewZGuid();
			Factory.Save();

			// Scenario 1: Profit/Loss job PK is not set.
			var profitLossWithoutJobPk = new JobProfitLoss(new BusinessObjectFactory());

			using (var form = new JobManagementForm(profitLossWithoutJobPk))
			{
				AssertEquals(
					"Cannot find corresponding job for JobManagementForm. Factory is unable to find Job with PK '00000000-0000-0000-0000-000000000000'.",
					ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();

			// Scenario 2: Profit/Loss job PK is set but the job is no longer exist.
			var nonExistingJobPk = ZGuid.NewZGuid();
			var profitLossWithNonExistingJob = new JobProfitLoss(new BusinessObjectFactory());
			profitLossWithNonExistingJob.SetJobPKs(new[] { nonExistingJobPk });

			using (var form = new JobManagementForm(profitLossWithNonExistingJob))
			{
				AssertEquals(
					string.Format("Cannot find corresponding job for JobManagementForm. Factory is unable to find Job with PK '{0}'.", nonExistingJobPk),
					ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();

			// Scenario 3: Profit/Loss with valid and existing job.
			var existingJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			existingJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			existingJob.JH_ParentID = ZGuid.NewZGuid();
			Factory.Save();

			var profitLossWithExistingJob = new JobProfitLoss(new BusinessObjectFactory());
			profitLossWithExistingJob.SetJobPKs(new[] { existingJob.PK });

			using (var form = new JobManagementForm(profitLossWithExistingJob))
			{
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		public void TestOpenJobWithInvalidParent()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = ZGuid.NewZGuid();
			Factory.Save();

			var profitLoss = new JobProfitLoss(new BusinessObjectFactory());
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });
			ErrorReporter.Clear();

			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				try
				{
					AssertEquals("No error should be reported.", "", ErrorReporter.LastMessageReported);
					jobManForm.Show();

					AssertEquals(@"Cannot find corresponding operation job.
You can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("There should be error.", 0, ErrorReporter.TotalErrorCount);
					AssertEquals("There should be developer exception.", 0, ExceptionReporterTestListener.Instance.Count);
					ErrorReporter.Clear();
					UnitTestUserNotification.Instance.ClearMessages();

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					Application.DoEvents();
					jobManForm.ShowOperationsForm_ForTestOnly(testJob);
					AssertNull("Operations Form should be created", jobManForm.LastShownForm_ForTestOnly);
					AssertEquals(@"Cannot find corresponding operation job.
You can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("There should be error.", 0, ErrorReporter.TotalErrorCount);
					AssertEquals("There should be developer exception.", 0, ExceptionReporterTestListener.Instance.Count);
				}
				finally
				{
					if (jobManForm.LastShownForm_ForTestOnly != null)
					{
						jobManForm.LastShownForm_ForTestOnly.Dispose();
					}
				}
			}
			ErrorReporter.Clear();
		}

		public void TestOpenJobDeclaration()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			testJob.JH_ParentID = declaration.PK;
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();

			bool originalCustomsDeclarationEnquirySecurityValue = Env.Security.CustomsDeclarationEnquiry.IsAllowed;
			bool originalCustomsDeclarationEnquiryEditSecurityValue = Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed;

			try
			{
				Type formType = Type.GetType("Enterprise.Customs.GUI.BaseJobDeclarationForm, Enterprise.Customs.GUI");

				Env.Security.CustomsDeclarationEnquiry.IsAllowed = false;
				Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed = false;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.IgnoreOSMG.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AttemptToShowForm(testJob);
				ZString expectedMessage = Env.Security.CustomsDeclarationEnquiry.ErrorMessageForNotAllowed;
				AssertEquals("Security Message should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.CustomsDeclarationEnquiry.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed = false;
				AssertFormTypeIsCorrect(testJob, formType, ODisplayMode.ReadOnly);

				Env.Security.CustomsDeclarationEnquiry.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;
				Env.Security.CustomsDeclarationEnquiryCRMSecurity.IgnoreOSMG.IsAllowed = true;
				AssertFormTypeIsCorrect(testJob, formType, ODisplayMode.Browse);
			}
			finally
			{
				Env.Security.CustomsDeclarationEnquiry.IsAllowed = originalCustomsDeclarationEnquirySecurityValue;
				Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed = originalCustomsDeclarationEnquiryEditSecurityValue;
			}
		}

		public void TestOpenCancelledJob()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_IsCancelled = true;
			Job testJob = testObjectCreator.CreateJob(declaration);
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			Factory.Save();

			var profitLoss = new JobProfitLoss(new BusinessObjectFactory());
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });
			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				AssertEquals("No error should be reported.", "", ErrorReporter.LastMessageReported);

				jobManForm.Show();

				AssertNull("Popup form should not show any message for cancelled job", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("There should be no error.", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("There should be no developer exception.", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		public void TestNoCharges()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			testJob.JH_ParentID = declaration.PK;
			Factory.Save();

			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });

			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				try
				{
					jobManForm.Show();
					Application.DoEvents();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					jobManForm.OpenButton_Click_ForTestOnly(this, EventArgs.Empty);
					AssertNotNull("Operations Form should be created even with no charges", jobManForm.LastShownForm_ForTestOnly);
					AssertNull("Msg about no job should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					if (jobManForm != null && jobManForm.LastShownForm_ForTestOnly != null)
					{
						jobManForm.LastShownForm_ForTestOnly.Dispose();
					}
				}
			}
		}

		public void TestShowOperationsForm()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S0001");
			var testJob = testObjectCreator.CreateJob(shipment);
			AssertEquals(shipment.Job.PK, testJob.PK);
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = testObjectCreator.FRT.PK;
			Factory.Save();

			var profitLoss = new JobProfitLoss(new BusinessObjectFactory());
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });
			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				jobManForm.Show();
				Application.DoEvents();
				using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
				{
					shipment.IsCancelled = true;
					plugInToFreight.OnBusinessObjectIsCancelledChanged(shipment.IsCancelled);
					Factory.Save();
				}
				jobManForm.OpenButton_Click_ForTestOnly(this, EventArgs.Empty);
				if (jobManForm.LastShownForm_ForTestOnly != null)
				{
					jobManForm.LastShownForm_ForTestOnly.Dispose();
				}
			}
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowOperationsFormForDeactivateJob()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S0001");
			var testJob = testObjectCreator.CreateJob(shipment);
			AssertEquals(shipment.Job.PK, testJob.PK);
			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = testObjectCreator.FRT.PK;
			Factory.Save();

			var profitLoss = new JobProfitLoss(new BusinessObjectFactory());
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });
			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				jobManForm.Show();
				Application.DoEvents();
				using (InvoicingPluginToFreight plugInToFreight = new InvoicingPluginToFreight(shipment))
				{
					shipment.IsCancelled = true;
					plugInToFreight.OnBusinessObjectIsCancelledChanged(shipment.IsCancelled);
					Factory.Save();
				}
				jobManForm.OpenButton_Click_ForTestOnly(this, EventArgs.Empty);
				if (jobManForm.LastShownForm_ForTestOnly != null)
				{
					jobManForm.LastShownForm_ForTestOnly.Dispose();
				}
			}
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCloseAndOpenOperationalDetailsButtonsEnabledWithViewMode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.CreateShipment("S0001"), false);
			Factory.Save();

			var profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { job.PK });

			using (JobManagementForm testForm = new JobManagementForm(profitLoss))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				testForm.ControllerID = DummyControllerIDs.Dummy;
				var module = testForm.GetModule();
				testForm.Show();

				Assert(testForm.CloseButton_ForTestOnly.Enabled);
				Assert(testForm.OpenOperationalDetailsButton_ForTestOnly.Enabled);
			}
		}

		void AssertFormTypeIsCorrect(Job testJob, Type formType, ODisplayMode displayMode)
		{
			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });

			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				try
				{
					jobManForm.Show();
					Application.DoEvents();
					jobManForm.ShowOperationsForm_ForTestOnly(testJob);

					AssertNotNull("Operations Form should be created", jobManForm.LastShownForm_ForTestOnly);
					Assert("Operations Form should be " + formType.ToString(), formType.IsAssignableFrom(jobManForm.LastShownForm_ForTestOnly.GetType()));
					AssertEquals("Operations Form Display Mode", displayMode, jobManForm.LastShownForm_ForTestOnly.DisplayMode);
				}
				finally
				{
					if (jobManForm.LastShownForm_ForTestOnly != null)
					{
						jobManForm.LastShownForm_ForTestOnly.Dispose();
					}
				}
			}
		}

		void AttemptToShowForm(Job testJob)
		{
			JobProfitLoss profitLoss = new JobProfitLoss(Factory);
			profitLoss.SetJobPKs(new ZGuid[] { testJob.PK });

			using (JobManagementForm jobManForm = new JobManagementForm(profitLoss))
			{
				jobManForm.Show();
				Application.DoEvents();
				jobManForm.ShowOperationsForm_ForTestOnly(testJob);

				if (jobManForm.LastShownForm_ForTestOnly != null)
				{
					jobManForm.LastShownForm_ForTestOnly.Dispose();
				}
			}
		}

		public class DummyJobProfitLoss : JobProfitLoss
		{
			public DummyJobProfitLoss(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new ZDecimal TotalCost
			{
				get
				{
					TotalCostCalculateTimes++;
					return base.TotalCost;
				}
			}

			public int TotalCostCalculateTimes;
		}
	}
}
