using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
	sealed class ExportInvoiceLineDetailsLayoutTest : BaseInvoiceLineDetailsLayoutTest<ExportInvoiceLineDetailsLayout>
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.FullGoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ComplementaryDescriptionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.BRNFENumberTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.BRNFEItemNumberTextBox, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.CargoPriorityDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.CountryDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.CPCGroupBox, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.NFeLinePriceCalcEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
			}
		}
	}
}
