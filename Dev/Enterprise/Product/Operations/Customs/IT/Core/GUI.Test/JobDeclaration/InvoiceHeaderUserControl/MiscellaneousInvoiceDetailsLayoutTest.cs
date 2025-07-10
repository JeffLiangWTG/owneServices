using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(MiscellaneousInvoiceDetailsLayout))]
sealed class MiscellaneousInvoiceDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => new[]
	{
		new[]
		{
			(customsBag.InvoiceNumberTextBox, ControlWidthClass.Auto),
			(customsBag.InvoiceDateEdit, ControlWidthClass.Auto),
			(customsBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto),
			(customsBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto),
			(customsBag.IncoTermsUserControl, ControlWidthClass.Auto),
			(customsBag.IncoTermPlaceTextBox, ControlWidthClass.Auto),
			(itBag.AgreedPlaceCodeDropEdit, ControlWidthClass.Auto),
			(customsBag.ValuationCodeDropEdit, ControlWidthClass.Auto),
			(customsBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto),
			(customsBag.NetWeightCalcDropEdit, ControlWidthClass.Auto),
			(customsBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto),
			(customsBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto),
		}
	};

	protected override int ControlBagCount => 2;

	CommercialInvoiceDetailsControlBag customsBag => CommercialInvoiceDetailsControlBag.Instance;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();

	readonly InvoiceDetailsControlBag itBag = InvoiceDetailsControlBag.Instance;
}
