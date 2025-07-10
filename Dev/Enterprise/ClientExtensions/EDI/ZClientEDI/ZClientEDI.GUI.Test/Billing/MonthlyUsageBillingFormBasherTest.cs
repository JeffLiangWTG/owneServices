using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(MonthlyUsageBillingForm))]
	internal sealed class MonthlyUsageBillingFormBasherTest : ZFormBasherTest
	{
		public void TestRowNumberFromColourDecidingEventArgs()
		{
			System.Reflection.FieldInfo rowNumberField = typeof(ColourDecidingEventArgs).GetField("RowNumber", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			AssertNotNull(rowNumberField);
		}

		public void TestViewSummaryPopupMenuExist()
		{
			MonthlyUsageBilling monthlyUsageBilling = new MonthlyUsageBilling(Factory);
			using (MonthlyUsageBillingForm form = new MonthlyUsageBillingForm(monthlyUsageBilling))
			{
				form.Show();
				AssertNotNull("Context menu contains View Summary", form.OrganisationBillsGrid.ContextMenu.MenuItems.FindByText("View Summary"));
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new MonthlyUsageBillingForm(new MonthlyUsageBilling(Factory));
		}

		#endregion
	}
}
