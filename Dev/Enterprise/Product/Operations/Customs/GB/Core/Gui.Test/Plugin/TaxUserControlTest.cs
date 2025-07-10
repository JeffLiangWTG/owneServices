using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin.Testing
{
	class TaxUserControlTest : TestCaseWithFactory
	{
		public void TestCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = "CDS";
			using (var form = new ZForm(declaration))
			{
				using (var control = new TaxUserControl())
				{
					control.JobDeclaration = declaration;

					form.Controls.Add(control);
					form.Show();

					AssertEquals("Taxes", control.FindSingle<ZGroupBox>(x => x.Name == "TaxGroupBox").Text);
					AssertEquals("Type", control.FindSingle<ZDropEdit>(x => x.Name == "TaxTypeDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Method of Payment", control.FindSingle<ZDropEdit>(x => x.Name == "TaxMethodOfPaymentDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Base Amount", control.FindSingle<ZCalcEdit>(x => x.Name == "TaxBaseAmountCalcEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Base Quantity", control.FindSingle<ZCalcEdit>(x => x.Name == "TaxBaseQtyCalcEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Amount", control.FindSingle<ZTextBox>(x => x.Name == "TaxAmountCalcEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Rate", control.FindSingle<ZDropEdit>(x => x.Name == "TaxRateDutyDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
		}
	}
}
