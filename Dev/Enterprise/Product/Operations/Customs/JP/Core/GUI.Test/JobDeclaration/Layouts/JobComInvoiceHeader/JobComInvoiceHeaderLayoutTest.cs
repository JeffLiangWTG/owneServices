using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderLayouts))]
	sealed class JobComInvoiceHeaderLayoutsTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceNumberTextBox, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.GroupInvoiceDropEdit, ControlWidthClass.Auto);
				yield return (JobComInvoiceHeaderControlBag.Instance.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
				yield return (JobComInvoiceHeaderControlBag.Instance.IncoTermsUserControl, ControlWidthClass.LongNoCaption);
				yield return (JobComInvoiceHeaderControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (JobComInvoiceHeaderControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			}
		}
		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new JobComInvoiceHeaderLayoutBuilder();
	}
}
