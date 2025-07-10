using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Billing.Business.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Freight;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	public class VisualBoardFormNonTransactionedFreightTest : VisualBoardFormBaseNonTransactionedTest
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_ShouldOpenQuoteBookingFormsInMainThread()
		{
			var qb = QuotedBooking.New(0, Factory);
			var board = SetupBoardWithGridAndFilter(ModuleIDs.QuotedBookings);

			Factory.Save();

			AssertShowFormFromBoard_ShouldOpenInMainThread<QuotedBookingForm>(board, BookingNewButtonLabelList.Descriptions.BookingWithQuote + " (Default)");
			AssertShowFormFromBoard_ShouldOpenInMainThread<QuotedBookingForm>(board, BookingNewButtonLabelList.Descriptions.QuickBooking);
			AssertShowFormFromBoard_ShouldOpenInMainThread<PreAllocationForm>(board, BookingNewButtonLabelList.Descriptions.PreAllocations);
		}

		[RequiresSTA]
		public void TestBoard_ShouldOpenTransportBookingFormInMainThread()
		{
			var board = SetupBoardWithGridAndFilter(ModuleIDs.JobShipment);
			var shipment = FreightTestHelper.CreateShipment<ForwardingShipment>(Factory) as BusinessObject;
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = shipment.TablePrefix;
			consolidation.KB_JobDirection = "PIC";
			var booking = consolidation.Bookings.AddNew();

			Factory.Save();

			AssertShowFormFromBoard_ShouldOpenInMainThread<TransportBookingForm>(
				board,
				"View/Edit Pickup Transport Booking",
				(contextMenu) => contextMenu.MenuItems.FindByText("Transport Booking", findSubitems: true).OnPopup(new EventArgs()));
		}

		[TestDate(2023, 1, 13)]
		[RequiresSTA]
		public void TestDoDeferFromVisualBoard_WhenESDDefaultsFromSpecified()
		{
			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;

			var originalEarliestStartDate = new ZDateTime(2023, 1, 12);
			var originalAgreedDeliveryDate = new ZDateTime(2023, 1, 16);
			var shipmentDepartureDate = new ZDateTime(2023, 1, 20);
			var shipmentArrivalDate = new ZDateTime(2023, 1, 23);
			var agreedDeliveryDateDefaultHoursOffset = ZDateTime.DefaultNegatableDurationEpoch.AddHours(-1);
			var earliestStartDateDefaultHoursOffset = ZDateTime.DefaultNegatableDurationEpoch.AddHours(1);

			var newEarliestStartDate = new ZDateTime(2023, 1, 13, DateTimeKind.Local);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "SHP");
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			var staff = BMSTestHelper.CreateStaff(Factory);

			var shipment = FreightTestHelper.CreateShipment<ForwardingShipment>(Factory);
			shipment.JS_E_DEP = shipmentDepartureDate;
			shipment.JS_E_ARV = shipmentArrivalDate;

			var jobHeader = BMSTestHelper.CreateJobHeader(shipment);
			var workflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "work", currentComponent: config.Buffer);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			workflow.FH_DoNotStartBeforeDate = originalEarliestStartDate;
			workflow.FH_AgreedDeliveryDate = originalAgreedDeliveryDate;
			workflow.FH_AgreedDeliveryDateDefaultHoursOffset = agreedDeliveryDateDefaultHoursOffset;
			workflow.FH_AgreedDeliveryDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA;
			workflow.FH_EarliestStartDefaultHoursOffset = earliestStartDateDefaultHoursOffset;
			workflow.FH_EarliestStartDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD;

			var task = workflow.Tasks.First();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(board))
			{
				var taskCard = form.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNotNull(taskCard);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					var deferWorkForm = dialog as DeferWorkflowForm;
					var viewModel = deferWorkForm.DataSource;
					viewModel.DoNotStartBeforeDate = newEarliestStartDate;

					AssertEquals("The workflow that you are attempting to defer is currently set to automatically update its deferral date from the Shipment Loading ETD field. Click Yes to defer and stop future automatic updates. Click No to cancel the deferral and return to the visual board.",
						UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					deferWorkForm.DeferButton.PerformClick();
				});

				taskCard.ContextMenuStrip.Items.Find("DeferToolStripMenuItem", false)[0].PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Earliest Start Date should match the value specified on the Defer form not shipment estimated departure time", newEarliestStartDate, workflow.DoNotStartBeforeDateLocal);
					AssertEquals("Agreed Delivery Date should match the shipment estimated arrival time with offset applied", shipmentArrivalDate.AddHours(-1), workflow.FH_AgreedDeliveryDate);
					AssertEquals("ADD offset should remain unchanged post deferral", agreedDeliveryDateDefaultHoursOffset, workflow.FH_AgreedDeliveryDateDefaultHoursOffset);
					AssertEquals("ADD Defaults From should remain unchanged", ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA, workflow.FH_AgreedDeliveryDateDefaultsFrom);
					AssertEquals("ESD offset should be cleared post deferral", ZDateTime.Empty, workflow.FH_EarliestStartDefaultHoursOffset);
					AssertEquals("ESD Defaults From should be cleared", string.Empty, workflow.FH_EarliestStartDateDefaultsFrom);
				});

				var messages = new UsageCollectorTestHelper(Factory).LoadUsageMessages();
				AssertEquals("Expected 1 message to be present, but was: " + messages.Length, 1, messages.Length);

				var message = messages.First();
				var messageText = message.EM_MessageText;
				var currentBranch = Env.CurrentBranch.Code;

				CombineAssertions(() =>
				{
					AssertContains("Message must contain FeatureCode: DEA", "\"FeatureCode\": \"DEA\"", messageText);
					AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", messageText);
					AssertContains("Message must contain BranchCode: " + currentBranch, currentBranch, message.Branch.Code);
					AssertContains("Message must contain PK:" + workflow.PK, $"\"PK\": \"{workflow.PK}\"", messageText);
					AssertContains("Message must contain Name:" + workflow.FH_CompletionStatement, $"\"Name\": \"{workflow.FH_CompletionStatement}\"", messageText);
					AssertContains("Message must contain FromVisualBoard: YES", "\"FromVisualBoard\": \"YES\"", messageText);
					AssertContains("Message must contain OriginalESDDefaultsFrom: DEP", $"\"OriginalESDDefaultsFrom\": \"{ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD}\"", messageText);
				});
			}
		}

		[TestDate(2023, 1, 13)]
		[RequiresSTA]
		public void TestCancelDeferFromVisualBoard_WhenESDDefaultsFromSpecified()
		{
			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;

			var originalEarliestStartDate = new ZDateTime(2023, 1, 12);
			var originalAgreedDeliveryDate = new ZDateTime(2023, 1, 16);
			var shipmentDepartureDate = new ZDateTime(2023, 1, 20);
			var shipmentArrivalDate = new ZDateTime(2023, 1, 23);
			var agreedDeliveryDateDefaultHoursOffset = ZDateTime.DefaultNegatableDurationEpoch.AddHours(-1);
			var earliestStartDateDefaultHoursOffset = ZDateTime.DefaultNegatableDurationEpoch.AddHours(1);

			var newEarliestStartDate = new ZDateTime(2023, 1, 13, DateTimeKind.Local);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "SHP");
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			var staff = BMSTestHelper.CreateStaff(Factory);

			var shipment = FreightTestHelper.CreateShipment<ForwardingShipment>(Factory);
			shipment.JS_E_DEP = shipmentDepartureDate;
			shipment.JS_E_ARV = shipmentArrivalDate;

			var jobHeader = BMSTestHelper.CreateJobHeader(shipment);
			var workflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "work", currentComponent: config.Buffer);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			workflow.FH_DoNotStartBeforeDate = originalEarliestStartDate;
			workflow.FH_AgreedDeliveryDate = originalAgreedDeliveryDate;
			workflow.FH_AgreedDeliveryDateDefaultHoursOffset = agreedDeliveryDateDefaultHoursOffset;
			workflow.FH_AgreedDeliveryDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA;
			workflow.FH_EarliestStartDefaultHoursOffset = earliestStartDateDefaultHoursOffset;
			workflow.FH_EarliestStartDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD;

			var task = workflow.Tasks.First();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(board))
			{
				var taskCard = form.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNotNull(taskCard);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					var deferWorkForm = dialog as DeferWorkflowForm;
					var viewModel = deferWorkForm.DataSource;
					viewModel.DoNotStartBeforeDate = newEarliestStartDate;

					AssertEquals("The workflow that you are attempting to defer is currently set to automatically update its deferral date from the Shipment Loading ETD field. Click Yes to defer and stop future automatic updates. Click No to cancel the deferral and return to the visual board.",
						UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					deferWorkForm.DeferButton.PerformClick();
				});

				taskCard.ContextMenuStrip.Items.Find("DeferToolStripMenuItem", false)[0].PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Earliest Start Date should match the value specified on the shipment departure date field with offset applied", shipmentDepartureDate.AddHours(1), workflow.FH_DoNotStartBeforeDate);
					AssertEquals("Agreed Delivery Date should match the shipment estimated arrival time with offset applied", shipmentArrivalDate.AddHours(-1), workflow.FH_AgreedDeliveryDate);
					AssertEquals("ADD offset should remain unchanged post cancellation deferral", agreedDeliveryDateDefaultHoursOffset, workflow.FH_AgreedDeliveryDateDefaultHoursOffset);
					AssertEquals("ADD Defaults From should remain unchanged", ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA, workflow.FH_AgreedDeliveryDateDefaultsFrom);
					AssertEquals("ESD offset should NOT be cleared post cancellation of deferral", earliestStartDateDefaultHoursOffset, workflow.FH_EarliestStartDefaultHoursOffset);
					AssertEquals("ESD Defaults From should NOT be cleared", ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD, workflow.FH_EarliestStartDateDefaultsFrom);
				});

				var messages = new UsageCollectorTestHelper(Factory).LoadUsageMessages();
				AssertEquals(0, messages.Length);
			}
		}

		[TestDate(2023, 1, 10)]
		public void TestDeferFromWorkflowAndTracking_WhenESDDefaultsFromSpecified()
		{
			var originalEarliestStartDate = new ZDateTime(2023, 1, 12);
			var originalAgreedDeliveryDate = new ZDateTime(2023, 1, 16);
			var shipmentDepartureDate = new ZDateTime(2023, 1, 20);
			var shipmentArrivalDate = new ZDateTime(2023, 1, 23);
			var agreedDeliveryDateDefaultHoursOffset = new ZDateTime(2023, 1, 1).AddHours(-1);
			var earliestStartDateDefaultHoursOffset = new ZDateTime(2023, 1, 1).AddHours(1);

			var newEarliestStartDate = new ZDateTime(2023, 1, 13, DateTimeKind.Local);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "SHP");
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			var staff = BMSTestHelper.CreateStaff(Factory);

			var shipment = FreightTestHelper.CreateShipment<ForwardingShipment>(Factory);
			shipment.JS_E_DEP = shipmentDepartureDate;
			shipment.JS_E_ARV = shipmentArrivalDate;

			var jobHeader = BMSTestHelper.CreateJobHeader(shipment);
			var workflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "work", currentComponent: config.Buffer);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			workflow.FH_DoNotStartBeforeDate = originalEarliestStartDate;
			workflow.FH_AgreedDeliveryDate = originalAgreedDeliveryDate;

			Factory.Save();

			var usageCollector = new UsageCollectorTestHelper(Factory);
			var messages = usageCollector.LoadUsageMessages();
			AssertEquals("Expected 0 messages to be present due to initial save:", 0, messages.Length);

			workflow.FH_DoNotStartBeforeDate = newEarliestStartDate;
			workflow.FH_AgreedDeliveryDateDefaultHoursOffset = agreedDeliveryDateDefaultHoursOffset;
			workflow.FH_AgreedDeliveryDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA;
			workflow.FH_EarliestStartDefaultHoursOffset = earliestStartDateDefaultHoursOffset;
			workflow.FH_EarliestStartDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD;

			Factory.Save();

			messages = usageCollector.LoadUsageMessages();
			AssertEquals("Expected 1 message to be present, but was: " + messages.Length, 1, messages.Length);

			var message = messages.First();
			var messageText = message.EM_MessageText;
			var currentBranch = Env.CurrentBranch.Code;

			CombineAssertions(() =>
			{
				AssertContains("Message must contain FeatureCode: DEA", "\"FeatureCode\": \"DEA\"", messageText);
				AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", messageText);
				AssertContains("Message must contain BranchCode: " + currentBranch, currentBranch, message.Branch.Code);
				AssertContains("Message must contain PK:" + workflow.PK, $"\"PK\": \"{workflow.PK}\"", messageText);
				AssertContains("Message must contain Name:" + workflow.FH_CompletionStatement, $"\"Name\": \"{workflow.FH_CompletionStatement}\"", messageText);
				AssertContains("Message must contain FromVisualBoard: NO", "\"FromVisualBoard\": \"NO\"", messageText);
				AssertContains("Message must contain OriginalESDDefaultsFrom: DEP", $"\"OriginalESDDefaultsFrom\": \"{ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD}\"", messageText);
			});
		}
	}
}
