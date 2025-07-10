using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class ItemDetailsUserControlTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestIsOriginVisible()
	{
		using (var form = new ZForm())
		using (var control = new ItemDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var countryOfOrigin = (ZDropEdit)control.Controls.Find("CountryOfOriginDropEdit", true).FirstOrDefault();
			var originState = (ZDropEdit)control.Controls.Find("OriginStateDropEdit", true).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertNotNull("Country of origin drop down", countryOfOrigin);
				AssertNotNull("Origin state drop down", originState);
			});

			CombineAssertions(() =>
			{
				AssertEquals("Country of origin visible", true, countryOfOrigin.Visible);
				AssertEquals("Origin state visible", true, originState.Visible);
			});
		}
	}

	public void TestCommodityCodeTariffFindBox()
	{
		using (var control = new ItemDetailsUserControl())
		{
			var commodityCodeTariffFindBox = control.FindSingle<TariffFindBox>("CommodityCodeTariffFindBox");
			AssertEquals(nameof(commodityCodeTariffFindBox.BindTo), "BY_FormattedHarmonisedTariff", commodityCodeTariffFindBox.BindTo);
		}
	}

	public void TestPortTaxRateDropEdit()
	{
		using (var control = new ItemDetailsUserControl())
		{
			var portTaxRateDropEdit = control.FindSingle<ZDropEdit>("PortTaxRateDropEdit");
			AssertNotNull("PortTaxRateDropEdit", portTaxRateDropEdit);
			CombineAssertions("PortTaxRateDropEdit", () =>
			{
				AssertEquals("Visible", true, portTaxRateDropEdit.Visible);
				AssertEquals("BindTo", "BY_CommodityCode", portTaxRateDropEdit.BindTo);
			});
		}
	}

	[RequiresSTA]
	public void TestCustomsThirdQtyDropEdit_Visibility()
	{
		using (var control = new ItemDetailsUserControl())
		{
			var customsThirdQtyDropEdit = control.FindSingle<ZCalcDropEdit>("CustomsThirdQtyDropEdit");
			AssertEquals("Should be visible for IT", true, customsThirdQtyDropEdit.Visible);
		}
	}

	[RequiresSTA]
	public void TestCustomsFirstQtyDropEditDecimalPlaces()
	{
		using (var control = new ItemDetailsUserControl())
		{
			var customsFirstQtyDropEdit = control.FindSingle<ZCalcDropEdit>("CustomsFirstQtyDropEdit");
			AssertEquals("CustomsFirstQtyDropEdit Decimal places for IT NCTS", 5, customsFirstQtyDropEdit.Decimals);
		}
	}
}
