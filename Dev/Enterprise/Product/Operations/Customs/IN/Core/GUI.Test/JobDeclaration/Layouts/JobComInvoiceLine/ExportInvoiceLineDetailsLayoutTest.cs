using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
public class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestAddControlBehaviour()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		using var form = new ZForm(declaration);
		using var control = new ExportInvoiceLineUserControl();
		form.Controls.Add(control);
		form.Show();

		var quantityCalcDropEdit = (ZCalcDropEdit)control.FindSingle<Control>(CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit.ControlName);
		CombineAssertions(() =>
		{
			AssertEquals(99999999.999m, quantityCalcDropEdit.MaxValue);
			AssertEquals(3, quantityCalcDropEdit.Decimals);
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.UnitPriceCalcFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.PMVFieldsUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.AccessoryStatusDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.RewardItemDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.IGSTPaymentGroupBox, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.UnitQuantityCalcDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.TotalPMVCalcFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.TransitCountryDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.EndUseCodeFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.AccessoryDescriptionLongTextBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.StateOrRegionOfOriginDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
}
