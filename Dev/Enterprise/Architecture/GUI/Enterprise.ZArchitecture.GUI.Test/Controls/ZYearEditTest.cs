using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZYearEditTest : TestCaseWithDummy
	{
		/// <summary>
		/// Ensures that non-positive values cannot be entered.
		/// </summary>
		public void TestNoNonPositives()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZYearEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			using (var form = new ZYearEditTestForm(dummy))
			{
				form.Show();

				var currentValue = form.TestYearEdit.Text;

				KeySender.PostKeyDown(form.TestYearEdit, Keys.Subtract);
				Application.DoEvents();
				AssertEquals("Negative values should be disallowed.", currentValue, form.TestYearEdit.Text);

				form.TestYearEdit.Text = "0";
				AssertEquals("Year 0 should be disallowed.", string.Empty, form.TestYearEdit.Text);
			}
		}

		/// <summary>
		/// Ensures that the control can be emptied.
		/// </summary>
		public void TestEraseContent()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZYearEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			using (var form = new ZYearEditTestForm(dummy))
			{
				form.Show();

				form.TestYearEdit.Text = "2999";
				AssertEquals("Could not set the value correctly.", "2999", form.TestYearEdit.Text);

				form.TestYearEdit.Text = "";
				AssertEquals("Could not delete the value.", string.Empty, form.TestYearEdit.Text);
			}
		}

		/// <summary>
		/// Ensure that 1- and 2-digit years are expanded to a 4-digit figure according to common sense.
		/// </summary>
		public void TestAutoCompleteYears()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZYearEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			using (var form = new ZYearEditTestForm(dummy))
			{
				form.Show();

				form.TestYearEdit.Text = "02";
				AssertEquals("Year 2 should be interpreted as 2002.", "2002", form.TestYearEdit.Text);

				form.TestYearEdit.Text = "99";
				AssertEquals("Year 99 should be interpreted as 1999.", "1999", form.TestYearEdit.Text);
			}
		}

		/// <summary>
		/// Ensures that years 4 characters long are not allowed
		/// </summary>
		public void TestMaxLength()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZYearEditTestForm.BindTo = DummyBusinessObject.Schema.Z0_Decimal;
			using (var form = new ZYearEditTestForm(dummy))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				// year 99999
				for (var i = 0; i < 5; ++i)
				{
					KeySender.PostKeyDown(form.TestYearEdit, Keys.D9);
				}

				Application.DoEvents();
				AssertEquals("The maximum year length should be 4.", "9999", form.TestYearEdit.Text);
			}
		}
	}
}
