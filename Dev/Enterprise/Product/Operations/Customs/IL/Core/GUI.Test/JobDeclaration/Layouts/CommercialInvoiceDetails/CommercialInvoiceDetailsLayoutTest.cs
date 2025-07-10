using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceDetailsLayout))]
	sealed class CommercialInvoiceDetailsLayoutTest : LayoutsAbstractTest
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
				yield return (InvoiceControlBag.Instance.SequenceTextBox, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceNumberTextBox, ControlWidthClass.Auto);
				yield return (InvoiceControlBag.Instance.InvoiceDateDateEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
				yield return (InvoiceControlBag.Instance.IncoTermsWithCountryCodeUserControl, ControlWidthClass.Auto);
				yield return (InvoiceControlBag.Instance.SupplierCodeFindBox, ControlWidthClass.Auto);
				yield return (InvoiceControlBag.Instance.PreferenceAgreementDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceControlBag.Instance.PaymentTermsDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
	}
}
