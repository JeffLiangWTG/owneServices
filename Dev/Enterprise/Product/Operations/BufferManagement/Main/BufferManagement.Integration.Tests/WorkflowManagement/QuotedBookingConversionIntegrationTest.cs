using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.Freight.QuotedBookings.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Freight;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class QuotedBookingConversionIntegrationTest : BMSTestCaseWithFactory
	{
		[GuiTest]
		[RequiresSTA]
		public void TestConvertQuotedBookingToShipment_ShouldRetainSeparatePaveDataForBothIncarnations()
		{
			var config = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var bookingTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode);
			var bookingTemplateJobHeader = bookingTemplate.GetJobHeader();
			bookingTemplateJobHeader.FH_CompletionStatement = "Alastria";

			var bookingTemplateWorkflow1 = BMSTestHelper.CreateWorkflow(bookingTemplate, "Booking Workflow 1");
			var bookingTemplateWorkflow2 = BMSTestHelper.CreateWorkflow(bookingTemplate, "Booking Workflow 2");

			BMSTestHelper.CreateTask(bookingTemplate, bookingTemplateWorkflow1, description: "Booking Task 1");
			BMSTestHelper.CreateTask(bookingTemplate, bookingTemplateWorkflow2, description: "Booking Task 2");

			var shipmentTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var shipmentTemplateJobHeader = shipmentTemplate.GetJobHeader();
			shipmentTemplateJobHeader.FH_CompletionStatement = "Sikaris";

			var shipmentTemplateWorkflow1 = BMSTestHelper.CreateWorkflow(shipmentTemplate, "Shipment Workflow 1");
			var shipmentTemplateWorkflow2 = BMSTestHelper.CreateWorkflow(shipmentTemplate, "Shipment Workflow 2");

			BMSTestHelper.CreateTask(shipmentTemplate, shipmentTemplateWorkflow1, description: "Shipment Task 1");
			BMSTestHelper.CreateTask(shipmentTemplate, shipmentTemplateWorkflow2, description: "Shipment Task 2");

			Factory.Save();

			QuotedBooking booking;

			using (var module = new QuotedBookingModule())
			using (var form = module.ShowPopup())
			{
				Application.DoEvents();

				module.NewMenuItem.MenuItems.FindByText(BookingNewButtonLabelList.Descriptions.QuickBooking).PerformClick();

				using (var bookingForm = Application.OpenForms.OfType<QuotedBookingForm>().Single())
				{
					booking = bookingForm.QuotedBooking;
					booking.FillWithValidTestData(); // This does nothing??

					booking.Factory.Save(); // Bypassing validation completely seems to be the go...

					((IFileMenuItemsProvider)bookingForm).ActionsMenuItem.MenuItems.FindByText("Consolidate").PerformClick();

					using (var consolForm = Application.OpenForms.OfType<ZForm>().Single(f => f.ControllerID == ControllerIDs.JobConsol))
					{
						var consol = (BusinessObject)consolForm.BusinessEntity;
						consol.FillWithValidTestData();

						consol.Factory.Save(); // Bypassing validation completely seems to be the go...
					}
				}
			}

			var newFactory = Factory.CreateNewFactory();
			booking = newFactory.Load<QuotedBooking>(booking.PK);

			var bookingJobHeader = ProcessJobHeader.GetForParentWithoutCreation(booking, newFactory);

			AssertNotNull("Should have created a job-level workflow for the booking", bookingJobHeader);
			AssertEquals("FH_ParentTableCode should match the ViewQuotedBooking table prefix", "VB", bookingJobHeader.FH_ParentTableCode);
			AssertEquals("Alastria", bookingJobHeader.FH_CompletionStatement);

			AssertContainsExactElementsInAnyOrder("Workflows applied to job should come from QBK template", new[] { "Booking Workflow 1", "Booking Workflow 2" }, bookingJobHeader.ProcessHeaders.Select(w => w.FH_CompletionStatement));
			AssertContainsExactElementsInAnyOrder("Tasks applied to job should come from QBK template", new[] { "Booking Task 1", "Booking Task 2" }, bookingJobHeader.Tasks.Select(t => t.P9_Description));

			var bookingAsShipment = newFactory.Load<ForwardingShipment>(booking.PK);

			AssertEquals("Booking should have an attached consol if it really was converted", 1, bookingAsShipment.Consols.Count);

			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(bookingAsShipment, newFactory);

			AssertNotNull("Should have created a separate job-level workflow for the shipment incarnation", shipmentJobHeader);
			AssertNotEquals(bookingJobHeader.PK, shipmentJobHeader.PK);
			AssertEquals("FH_ParentTableCode should match the JobShipment table prefix", "JS", shipmentJobHeader.FH_ParentTableCode);
			AssertEquals("FH_ParentId should be the same as the booking incarnation", bookingJobHeader.FH_ParentId, shipmentJobHeader.FH_ParentId);

			AssertContainsExactElementsInAnyOrder("Workflows applied to job should come from SHP template", new[] { "Shipment Workflow 1", "Shipment Workflow 2" }, shipmentJobHeader.ProcessHeaders.Select(w => w.FH_CompletionStatement));
			AssertContainsExactElementsInAnyOrder("Tasks applied to job should come from SHP template", new[] { "Shipment Task 1", "Shipment Task 2" }, shipmentJobHeader.Tasks.Select(t => t.P9_Description));
		}

		protected override void SetUp()
		{
			base.SetUp();

			MasterFilesTestHelper.ClearWorkflowTables();
		}
	}
}
