using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	public class TaxUserControlTest : TestCaseWithFactory
	{
		public void TestPaymentMethodShouldBeRemoved()
		{
			using (var form = new ZForm())
			using (var control = new TaxUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var taxGrid = (ZGrid)control.Controls.Find("TaxGrid", true).Single();
				AssertNull("", taxGrid.GetColumnStyle("Data+G4_MethodOfPayment"));

				var methodOfPaymentDropEdit = control.Controls.Find("TaxMethodOfPaymentDropEdit", true).Single();
				Assert(!methodOfPaymentDropEdit.Visible);
			}
		}

		public void TestMethodOfcalculationColumn()
		{
			using (var form = new ZForm())
			using (var control = new TaxUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var taxGrid = (ZGrid)control.Controls.Find("TaxGrid", true).Single();
				var zDropEditColumnStyleInfo = taxGrid.GetColumnStyle(JobComInvoiceLineTax.Schema.JLT_MethodOfCalculation);
				AssertNotNull("Tax grid misses a Method Of Calculation column.", zDropEditColumnStyleInfo);
				Assert("Tax grid Method Of Calculation column should be visible.", zDropEditColumnStyleInfo.IsVisible);
				AssertEquals("Tax grid Method Of Calculation column should be editable.", zDropEditColumnStyleInfo.IsReadOnly, false);
			}
		}

		public void TestTariffBypassColumn()
		{
			using (var form = new ZForm())
			using (var control = new TaxUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var taxGrid = (ZGrid)control.Controls.Find("TaxGrid", true).Single();
				var zDropEditColumnStyleInfo = taxGrid.GetColumnStyle("TariffBypassCode");
				AssertNotNull("Tax grid misses a Tariff Bypass column.", zDropEditColumnStyleInfo);
				Assert("Tax grid Tariff Bypass column should be visible.", zDropEditColumnStyleInfo.IsVisible);
				Assert("Tax grid Tariff Bypass column should be read only.", zDropEditColumnStyleInfo.IsReadOnly);
			}
		}
	}
}
