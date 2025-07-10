using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test
{
	[TestClass]
	[TestedType(typeof(SelectChangeLogFilterDatesForm))]
	sealed class SelectChangeLogFilterDatesFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new SelectChangeLogFilterDatesForm();
		}

		[RequiresSTA]
		public void TestFormCloseOnCancelClick()
		{
			using (var testForm = new SelectChangeLogFilterDatesForm())
			{
				testForm.Show();
				AssertNoExceptionThrown(testForm.cancelButton.PerformClick);
				AssertNull("No message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, testForm.IsAccessible);
			}
		}

		public void TestFromDateBound()
		{
			using (var testForm = new SelectChangeLogFilterDatesForm())
			{
				testForm.Show();
				AssertEquals("From date should be correctly bound to non-persistent business object.", "FromDate", testForm.fromDate.BindTo);
			}
		}

		public void TestToDateBound()
		{
			using (var testForm = new SelectChangeLogFilterDatesForm())
			{
				testForm.Show();
				AssertEquals("To date should be correctly bound to non-persistent business object.", "ToDate", testForm.toDate.BindTo);
			}
		}
	}
}
