using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(MonthYearPeriodUserControl))]
	sealed class MonthYearPeriodUserControlTest : RuntimeOptionUserControlBaseTest<MonthYearPeriodUserControl>
	{
		public void TestMonthYearFields()
		{
			using (var form = new ZForm())
			using (var control = new MonthYearPeriodUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var monthEdit = form.Controls.Find("PeriodMonthEdit", true).Single();
				AssertEquals("", monthEdit.Text);
				monthEdit.Text = "1";
				AssertEquals("01", monthEdit.Text);
				monthEdit.Text = "10";
				AssertEquals("10", monthEdit.Text);
				monthEdit.Text = "0";
				AssertEquals("", monthEdit.Text);

				var yearEdit = form.Controls.Find("PeriodYearEdit", true).Single();
				AssertEquals("", yearEdit.Text);
				yearEdit.Text = "1";
				AssertEquals("0001", yearEdit.Text);
				yearEdit.Text = "10";
				AssertEquals("0010", yearEdit.Text);
				yearEdit.Text = "0";
				AssertEquals("", yearEdit.Text);
			}
		}
	}
}
