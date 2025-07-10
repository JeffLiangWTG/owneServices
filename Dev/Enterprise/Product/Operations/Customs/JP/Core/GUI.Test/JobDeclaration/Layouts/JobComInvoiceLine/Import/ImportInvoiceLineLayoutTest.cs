using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineLayouts))]
	sealed class ImportInvoiceLineLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Auto);
				yield return (ImportInvoiceLineControlBag.Instance.TariffFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineControlBag.Instance.NACCSCodeDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (ImportInvoiceLineControlBag.Instance.CertificateOfOriginPanel, ControlWidthClass.Auto);
				yield return (ImportInvoiceLineControlBag.Instance.DutyRateTextBox, ControlWidthClass.Auto);
				yield return (ImportInvoiceLineControlBag.Instance.ProcedureTextBox, ControlWidthClass.Auto);
				yield return (ImportInvoiceLineControlBag.Instance.StorageTypeDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Auto);
				yield return (InvoiceLineControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineControlBag.Instance.UnitPriceCalcEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 3;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ImportInvoiceLineLayoutBuilder();
	}
}
