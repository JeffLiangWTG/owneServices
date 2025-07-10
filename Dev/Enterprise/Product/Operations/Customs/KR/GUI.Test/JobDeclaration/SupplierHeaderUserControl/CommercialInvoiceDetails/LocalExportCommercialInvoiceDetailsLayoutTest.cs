using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(LocalExportCommercialInvoiceDetailsLayout))]
	sealed class LocalExportCommercialInvoiceDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceNumberTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.IncoTermCodeDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.CommercialInvoiceDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

				yield return (CommercialInvoiceDetailsControlBag.Instance.DRWApplicantDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.SupportingDocumentTypeDropEdit, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.SupportingDocumentNoTextBox, ControlWidthClass.Auto);
				yield return (CommercialInvoiceDetailsControlBag.Instance.InboundDateEdit, ControlWidthClass.Auto);
			}
		}
	}
}
