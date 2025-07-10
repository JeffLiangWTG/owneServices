using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ImportInvoiceDetailsLayoutProvider))]
sealed class ImportInvoiceDetailsLayoutProviderTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var commonBag = builder.CommonBag;
			yield return (commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			yield return (commonBag.GroupInvoiceDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			yield return (commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			yield return (commonBag.IncoTermDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			yield return (commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			yield return (commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
		}
	}
}
