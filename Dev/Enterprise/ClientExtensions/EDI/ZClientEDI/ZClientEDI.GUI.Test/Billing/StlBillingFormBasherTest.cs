using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.GUI.Testing
{
	[TestedType(typeof(StlBillingForm))]
	internal sealed class StlBillingFormBasherTest : ZFormBasherTest
	{
		public void TestRowNumberFromColourDecidingEventArgs()
		{
			System.Reflection.FieldInfo rowNumberField = typeof(ColourDecidingEventArgs).GetField("RowNumber", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			AssertNotNull(rowNumberField);
		}

		public void TestBillingTestingEnvironmentSyncMenuItem()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddUserResponse("I CONFIRM THAT I WANT TO SYNC.");
			UnitTestUserNotification.Instance.AddOKAnswer();

			var bizObj = new StlBilling(Factory);
			bizObj.DateTo = new ZDateTime(2015, 7, 31);

			using (var form = new StlBillingForm(bizObj))
			{
				form.Show();
				var menu = form.Menu.MenuItems.FindByText("Billing Testing Environment Synchronization...", true);
				menu.PerformClick();

				AssertEquals(@"Please check the registry items under category: WiseTech Global Client Extensions/Licence Billing/External Servers/Billing Testing.

This operation will synchronize the Billing Testing Environment with the latest billing data, replacing any existing information.
Please note that this action cannot be undone.
Are you sure you want to proceed?
", string.Join("\r\n", UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text)));
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
		}

		public void TestBillingExternalServerCheckerMenuItem()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			var bizObj = new StlBilling(Factory);
			bizObj.DateTo = new ZDateTime(2015, 7, 31);

			using (var form = new StlBillingForm(bizObj))
			{
				form.Show();
				var menu = form.Menu.MenuItems.FindByText("Check External Servers...", true);
				menu.PerformClick();

				AssertEquals(@"Please enter the connection string for the new external server, or leave it blank to test the current ones."
					, UnitTestUserNotification.Instance.PreviousMessages.First().Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
		}

		public void TestBillingMilestoneSetterMenuItem()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			var bizObj = new StlBilling(Factory);
			bizObj.DateTo = new ZDateTime(2015, 7, 31);

			using (var form = new StlBillingForm(bizObj))
			{
				form.Show();
				var menu = form.Menu.MenuItems.FindByText("STL Billing Milestone Reset...", true);
				menu.PerformClick();

				using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(StlBillingMilestoneSetterForm), lastShownForm);
					AssertNotNull(lastShownForm);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
		}

		public void TestStlBillingAdminFunction()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();

			var bizObj = new StlBilling(Factory);
			bizObj.DateTo = new ZDateTime(2015, 7, 31);

			EDISecurityCheckpoints.STLBillingAdminFunction.IsAllowed = false;
			using (var form = new StlBillingForm(bizObj))
			{
				form.Show();
				var menuItems = form.AdminMenuItem_Exposed.MenuItems.Cast<MenuItem>().ToList();
				AssertEquals(6, menuItems.Count);
				foreach (var item in menuItems)
				{
					Assert("item should be disabled when STLBillingAdminFunction is false", !item.Enabled);
				}
			}

			EDISecurityCheckpoints.STLBillingAdminFunction.IsAllowed = true;
			using (var form = new StlBillingForm(bizObj))
			{
				form.Show();
				var menuItems = form.AdminMenuItem_Exposed.MenuItems.Cast<MenuItem>().ToList();
				AssertEquals(6, menuItems.Count);
				foreach (var item in menuItems)
				{
					Assert("item should be enabled when STLBillingAdminFunction is true", item.Enabled);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new StlBillingForm(new StlBilling(Factory));
		}

		#endregion
	}
}
