using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CalculateFreightForm))]
class CalculateFreightFormTest : ZFormBasherTest
{
	public void TestDataSourceType()
	{
		using (var form = (CalculateFreightForm)GetFormToBashCore())
		{
			AssertEquals(typeof(CalculateFreightBizObj), form.DataSourceType);
		}
	}

	public void TestFieldVisibilityAndBindings()
	{
		using (var form = (CalculateFreightForm)GetFormToBashCore())
		{
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("TotalAmountCurrencyControl.Visible", true, form.TotalAmountCurrencyControl.Visible);
				AssertEquals("PercentageToCHBoarderCalcEdit.BindTo", "TotalAmount", form.TotalAmountCurrencyControl.BindToAmount);
				AssertEquals("PercentageToCHBoarderCalcEdit.BindTo", "Currency", form.TotalAmountCurrencyControl.BindToUnit);

				AssertEquals("PercentageToCHBoarderCalcEdit.Visible", true, form.PercentageToCHBoarderCalcEdit.Visible);
				AssertEquals("PercentageToCHBoarderCalcEdit.BindTo", "PercentageToCHBoarder", form.PercentageToCHBoarderCalcEdit.BindTo);

				AssertEquals("PercentageToFinalDestinationCalcEdit.Visible", true, form.PercentageToFinalDestinationCalcEdit.Visible);
				AssertEquals("PercentageToFinalDestinationCalcEdit.BindTo", "PercentageToFinalDestination", form.PercentageToFinalDestinationCalcEdit.BindTo);

				AssertEquals("AmountToCHBorderCalcEdit.Visible", true, form.AmountToCHBorderCalcEdit.Visible);
				AssertEquals("AmountToCHBorderCalcEdit.BindTo", "AmountToCHBorder", form.AmountToCHBorderCalcEdit.BindTo);

				AssertEquals("AmountToFinalDestinationCalcEdit.Visible", true, form.AmountToFinalDestinationCalcEdit.Visible);
				AssertEquals("AmountToFinalDestinationCalcEdit.BindTo", "AmountToFinalDestination", form.AmountToFinalDestinationCalcEdit.BindTo);

				AssertEquals("OK.Visible", true, form.OKButton.Visible);
				AssertEquals("Cancel.Visible", true, form.QuitButton.Visible);
			});
		}
	}

	protected override string CountryCode => Core.Constants.CountryCodes.Switzerland;

	protected override Form GetFormToBashCore() => new CalculateFreightForm(GetNewBusinessObject());

	protected CalculateFreightBizObj GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		return new CalculateFreightBizObj(invoice.Charges);
	}
}
