using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class StlRetrieverFormTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			using (var form = new StlRetrieverFormForTest())
			{//
				form.Show();
				Application.DoEvents();

				var fromDate = form.Controls.Find("FromDate", true)[0] as KDateTimePicker;
				var toDate = form.Controls.Find("ToDate", true)[0] as KDateTimePicker;
				var collect = form.Controls.Find("collectButton", true)[0] as ZButton;
				var textBox = form.Controls.Find("outputTextBox", true)[0] as ZTextBox;

				fromDate.Value = form.BillingDateTimeMinusOneMonth.AddDays(1);
				Application.DoEvents();
				toDate.Value = form.BillingDateTimeMinusOneMonth;
				Application.DoEvents();
				Assert("should not be able to collect data", !collect.Enabled);
			}
		}

		public void TestDateTimeShowing()
		{
			using (var form = new StlRetrieverFormForTest())
			{
				form.UtcMinusOneMonth = new DateTime(2020, 08, 04);
				form.Show();
				Application.DoEvents();

				AssertEquals("Should showing correct format", "From/To: 03/08/2020 00:00:00/04/08/2020 23:59:59\r\n", form.OutPutTimeToTextBox());
			}
		}

		[TestDate(2014, 8, 15, 0, 59, 59)]
		public void TestScriptList()
		{
			using (var form = new StlRetrieverFormForTest())
			{
				form.UtcMinusOneMonth = new DateTime(2020, 08, 04);
				form.Show();
				Application.DoEvents();

				var expectedScripts = new ScriptLoader().Load(Factory).Select(t => t.Script).Where(s => s.StlGrain != StlDataGrain.MonthlyCurrentDataOnly);
				var actualScripts = form.Scripts;
				AssertEquals("Wrong number of scripts in form", expectedScripts.Count() + 1, actualScripts.Count());
				Assert("All items entry missing from scripts", actualScripts.Contains(new StlRetrieverForm.ScriptViewer("All Items")));
				foreach (var scriptViewer in expectedScripts.Select(s => new StlRetrieverForm.ScriptViewer(s.Code, s.Feature)))
				{
					Assert(string.Format(CultureInfo.InvariantCulture, "Item ({0}) missing from scripts", scriptViewer), actualScripts.Contains(scriptViewer));
				}
			}
		}

		public void TestScriptViewer_Operator_WorksAsExpected()
		{
			var script1 = new StlRetrieverForm.ScriptViewer("A123", "FeatureX");
			var script2 = new StlRetrieverForm.ScriptViewer("A123", "FeatureX");
			StlRetrieverForm.ScriptViewer nullViewer = null;
			Assert("Expected a != null", !(script1 == nullViewer));
			Assert("Expected null != a", !(nullViewer == script1));
			Assert("Expected null == null", nullViewer == null);
			Assert("Expected a == b", script1 == script2);
		}

		class StlRetrieverFormForTest : StlRetrieverForm
		{
			public StlRetrieverFormForTest() : base()
			{
			}

			public DateTime UtcMinusOneMonth
			{
				get { return utcMinusOneMonth; }
				set { utcMinusOneMonth = value; }
			}

			public DateTime BillingDateTimeMinusOneMonth
			{
				get { return TimeZoneInfo.ConvertTimeFromUtc(UtcMinusOneMonth, BaseDateTimeRange.BillingTimeZoneInfo); }
			}

			public IEnumerable<ScriptViewer> Scripts => (List<ScriptViewer>)script.DataSource;
		}
	}
}
