using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	public class ProcessHeaderWithIntegrationTest : TestCaseWithFactory
	{
		public void TestParentType_TransactionHeaderParentsAreSupported_APInvoice()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			AssertBizoIsIWorkflowProviderAndCorrectlyCreatesChildWorkflow(apInvoice, "AH");
		}

		public void TestParentType_TransactionHeaderParentsAreSupported_ARInvoice()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();

			AssertBizoIsIWorkflowProviderAndCorrectlyCreatesChildWorkflow(arInvoice, "AH");
		}

		public void TestParentType_HeaderParentsAreSupported_QuotedBooking()
		{
			var quotedBooking = QuotedBooking.New(0, Factory);

			AssertBizoIsIWorkflowProviderAndCorrectlyCreatesChildWorkflow(quotedBooking, "VB");
		}

		public void AssertBizoIsIWorkflowProviderAndCorrectlyCreatesChildWorkflow(BusinessObject bizo, string parentTableCode)
		{
			var workflowProvider = bizo as IWorkflowProvider;
			AssertNotNull("PRE: Our bizo of type " + bizo.GetType().ToString() + " should be an IWorkflowProvider, and yet...", workflowProvider);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(workflowProvider, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_ParentTableCode = parentTableCode;
			workflow.FH_ParentId = bizo.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var workflowReloaded = newFactory.Load<IProcessHeader>(workflow.PK);

			AssertEquals("Our workflow should be the baby of a TransactionHeader, and yet...", bizo.TablePrefix, workflow.FH_ParentTableCode);
			AssertEquals("Our spawn of TransactionHeader should have a properly found ParentType, but instead...", bizo.PK, workflowReloaded.Parent.PK);
		}

		public void TestCustomisation_WhenAppliedToQuotedBooking_WithComplexPropertyName_ShouldNotThrowException()
		{
			var quotedBooking = QuotedBooking.New(0, Factory);
			quotedBooking.Origin = "AUALX";

			var buffer = BMSTestHelper.CreateBuffer(system);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader(quotedBooking, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", buffer);
			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code);

			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_JobType = "QBK";
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.Job;
			line.PropertyName = "<Booking.Origin.Code>";

			var taskCardContent = new TaskCardContent(task, null);
			string bindingPath = null;

			AssertNoExceptionThrown(() => bindingPath = line.GetBindingPath(taskCardContent));
			AssertEquals("Should bind to the correct path, including the bizo, and yet...", "Parent.Booking.Origin.Code", bindingPath);
		}

		[GuiTest]
		[RequiresSTA]
		public void TestShipmentLinkedToConsol_WithDifferentWorkflowCategoryOptions_WhenValidateAllCalledOnOneJob_ShouldNotValidateIncorrectly()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "SH", "Test category 1");
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.JobConsolWorkflowDescriptorCode, "CN", "Test category 2");

			var consolWorkflow = BMSTestHelper.CreateWorkflowAndParents<ForwardingConsol>(Factory, "Consol Workflow");
			consolWorkflow.FH_Category = "CN";
			var consol = (CommonConsol)consolWorkflow.Parent;

			var shipment = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
			shipment.FillWithValidTestData();

			var shipmentJobHeader = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(shipment, shipment.Factory, addDefaultProcessHeaderIfNone: false);
			var shipmentWorkflow = BMSTestHelper.CreateWorkflow(shipmentJobHeader, "Shipment Workflow");
			shipmentWorkflow.FH_Category = "SH";

			Factory.Save();

			var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var consolForm = (ZForm)consolController.ShowEditForm(consol))
			{
				Application.DoEvents();

				var shipmentGrid = consolForm.FindSingle<ZModuleButtonGrid>("ShipmentModuleButtonGrid");
				shipmentGrid.InnerGrid.Select(0);
				var toolStrip = (ZToolStrip)shipmentGrid.Controls.Find("toolStrip", true)[0];
				var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];
				editButton.PerformClick();
				Application.DoEvents();

				using (var shipmentForm = (ZForm)shipmentGrid.LastShownZForm)
				{
					AssertNotNull(shipmentForm);
					AssertEquals(shipment.PK, ((BusinessObject)shipmentForm.BusinessEntity).PK);

					consolForm.FireValidateAllForTest();
					Application.DoEvents();

					AssertNoErrors("Workflow categories were properly set, so as long as the correct lookup lists are used, there should be no errors. SAD!", consolWorkflow.FH_CategoryInfo);
					AssertNoErrors("Workflow categories were properly set, so as long as the correct lookup lists are used, there should be no errors. SAD!", shipmentWorkflow.FH_CategoryInfo);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();

			system = BMSTestHelper.CreateSystem(Factory, ObjectFactory.Get<IWorkflowDescriptorList>().Cast<CodeDescriptionPair>()
												.Select(pair => WorkflowDescriptors.Instance.TryGetValueSafe(pair.Code))
												.OrderBy(d => d.GetType().FullName)
												.Where(d => d.SupportsBufferManagement)
												.Select(de => de.Code).ToArray());
		}

		BMSystem system;
	}
}
