using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	internal sealed class BillingInvoicingControlTestCase : TestCaseWithFactory
	{
		public void TestRecalculateBalanceMenu()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var ctrl = new BillingInvoicingControl())
			{
				var grid = ctrl.Controls.Find("depositGrid", true).First() as ZGrid;
				AssertEquals(true, grid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().Any(x => x.ColumnName == "IsValid"));
				var menu = grid.ContextMenu.MenuItems.FindByText("Recalculate Balance");
				menu.PerformClick();
				AssertEquals("Recalculation succeeded.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}