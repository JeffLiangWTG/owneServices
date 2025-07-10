using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.GUI.Options;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	public class VisualBoardFormNonTransactionedWarehouseTest : VisualBoardFormBaseNonTransactionedTest
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBoard_WithWhsReceiveGrid_WithFilter_ShouldNotCauseThreadSentryException_WhenInvokingCartageAdvice()
		{
			Factory.NewWithValidTestData<WhsReceive>();
			var board = SetupBoardWithGridAndFilter(ModuleIDs.WhsReceive);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var visualBoardForm = VisualBoardFormDisplayer.ShowBoard(board))
			{
				visualBoardForm.AwaitAll();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					var transportBookingDocumentForm = (TransportBookingDocumentForm)form;
					transportBookingDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
				});

				var grid = visualBoardForm.FindSingle<ZGrid>();
				grid.SelectAllElements();

				grid.ContextMenu.SetSourceControl(grid);
				grid.ContextMenu.DoPopup();
				var documentsMenuItem = grid.ContextMenu.MenuItems.FindByText("Documents");
				documentsMenuItem.OnPopup(EventArgs.Empty);
				var documentsSubMenuItem = documentsMenuItem.MenuItems.FindByText("Cartage Advice");
				documentsSubMenuItem.PerformClick();

				Application.DoEvents();

				var transportBookingForm = ZApplication.GetOpenForms().OfType<TransportBookingForm>().Single();
				AssertNoExceptionThrown("TransportBookingForm should be created on main thread, so closing the form on main thread should not cause thread sentry violations.", () => transportBookingForm.Close());
			}
		}
	}
}
