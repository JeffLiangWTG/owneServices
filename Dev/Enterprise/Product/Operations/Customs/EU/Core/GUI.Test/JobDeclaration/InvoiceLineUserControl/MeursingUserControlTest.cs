using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Meursing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class MeursingUserControlTest : TestCaseWithFactory
	{
		public void TestCalculationNoException()
		{
			var lines = new List<JobComInvoiceLine>();
			lines.Add(Factory.NewWithValidTestData<JobComInvoiceLine>());
			var meursingTableManager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			using (var form = new MeursingUserControl(meursingTableManager))
			{
				form.Show();

				var button = form.Controls.Find("CalculateButton", true).FirstOrDefault() as ZButton;
				AssertNoExceptionThrown(() => button.PerformClick());
			}
		}

		public void TestControls()
		{
			using (var control = new MeursingUserControl())
			{
				CombineAssertions("Controls of MeursingUserControl", () =>
				{
					AssertNotNull("StarchGlucoseUpDown should exist.", control.FindSingleOrDefault<ZNumericUpDown>("StarchGlucoseUpDown"));
					AssertNotNull("SucroseUpDown should exist.", control.FindSingleOrDefault<ZNumericUpDown>("SucroseUpDown"));
					AssertNotNull("MilkFatUpDown should exist.", control.FindSingleOrDefault<ZNumericUpDown>("MilkFatUpDown"));
					AssertNotNull("MilkProteinsUpDown should exist.", control.FindSingleOrDefault<ZNumericUpDown>("MilkProteinsUpDown"));
					AssertNotNull("CalculateButton should exist.", control.FindSingleOrDefault<ZButton>("CalculateButton"));
				});
			}
		}

		public void TestIsDialogDisposeAfterCalculateButtonClicked()
		{
			AssertCalculateButtonClicked(false);
			AssertCalculateButtonClicked(true);
		}

		void AssertCalculateButtonClicked(bool isShouldDispose)
		{
			var lines = new List<JobComInvoiceLine>();
			lines.Add(Factory.NewWithValidTestData<JobComInvoiceLine>());
			var meursingTableManager = new MeursingTableManager(new InvoiceLinesMeursingTarget(lines));
			using (var form = new ZForm())
			using (var control = new MeursingUserControl(meursingTableManager, isShouldDispose))
			{
				var button = control.FindSingleOrDefault<ZButton>("CalculateButton");
				form.Controls.Add(control);
				form.Show();

				button.PerformClick();
				System.Windows.Forms.Application.DoEvents();
				AssertEquals(isShouldDispose ? DialogResult.OK : DialogResult.None, form.DialogResult);
			}
		}
	}
}
