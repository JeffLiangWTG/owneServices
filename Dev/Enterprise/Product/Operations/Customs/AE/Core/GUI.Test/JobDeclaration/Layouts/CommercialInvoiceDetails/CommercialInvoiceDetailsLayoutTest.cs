using System.Collections.Generic;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(CommercialInvoiceDetailsLayout))]
public sealed class CommercialInvoiceDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceNumberTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.GroupInvoiceDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.IncoTermsUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.PaymentMethodDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.TotNoOfInvPagesCalcEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceTypeDropEdit, ControlWidthClass.Auto);
			yield return (CommercialInvoiceDetailsControlBag.Instance.AttestationNoTextBox, ControlWidthClass.Auto);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
}
