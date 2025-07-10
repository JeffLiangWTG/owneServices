using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	[TestedType(typeof(DataPurgeUserForm))]
	sealed class DataPurgeUserFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestQuotationsCheckBox()
		{
			using (var form = (DataPurgeUserForm)GetFormToBashCore())
			{
				form.Show();

				AssertEquals("Rating and Quotations", form.RatingInfoCheckBox.Text);

				AssertEquals("Quotations", form.QuotationsCheckBox.Text);
				AssertEquals(form.EnterpriseWideGroupBox, form.QuotationsCheckBox.Parent);
				AssertEquals("HasQuotationsPurgeScript", form.QuotationsCheckBox.BindTo);

				form.Purger.HasQuotationsPurgeScript = true;
				AssertEquals(true, form.QuotationsCheckBox.Checked);
				form.Purger.HasQuotationsPurgeScript = false;
				AssertEquals(false, form.QuotationsCheckBox.Checked);
			}
		}

		[RequiresSTA]
		public void TestRunSystemWideWithoutTransactionButtonVisibility()
		{
			using (var form = new DataPurgeUserForm())
			{
				form.Show();
				AssertEquals("[Non-Production System] RunSystemWideWithoutTransactionButton.Visible", true, form.RunSystemWideWithoutTransactionButton.Visible);
			}

			using (DataPurgerNonTransactionedTest.MockProductionEnvironment())
			using (var form = new DataPurgeUserForm())
			{
				form.Show();
				AssertEquals("[Production System] RunSystemWideWithoutTransactionButton.Visible", false, form.RunSystemWideWithoutTransactionButton.Visible);
			}
		}

		[RequiresSTA]
		public void TestShowConfirmationBeforeRunningPurge()
		{
			void AddAnswers()
			{
				if (((IActiveUserQuery)new ActiveUserQuery()).GetActiveUsers(false).Length > 0)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			}

			using (var form = new DataPurgeUserForm())
			{
				form.Show();
				AddAnswers();
				form.RunSystemWideButton.PerformClick();
				AssertEquals("Message after clicking run was not expected", "You have selected to purge data. The data will be permanently deleted and it cannot be reverted.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("No purge", string.IsNullOrEmpty(form.Purger.Output));

				AddAnswers();
				form.RunSystemWideWithoutTransactionButton.PerformClick();
				AssertEquals("Message after clicking run without transaction was not expected", "You have selected to purge data. The data will be permanently deleted and it cannot be reverted.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("No purge", string.IsNullOrEmpty(form.Purger.Output));

				AddAnswers();
				form.MainTabControl.SelectNextTabPage();
				form.RunCompanySpecificButton.PerformClick();
				AssertEquals("Message after clicking company specific run was not expected", "You have selected to purge data. The data will be permanently deleted and it cannot be reverted.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("No purge", string.IsNullOrEmpty(form.Purger.Output));
				form.DialogResult = DialogResult.Cancel;
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DataPurgeUserForm();
		}
	}
}
