using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
sealed class ImportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
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
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.ProcedureCodeFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.NonCommercialGoodsCheckBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionPrimaryPreferenceDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.PermitObligationDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.NonCustomsLawObligationDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.StorageTypeDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.VATCodeUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.RateFormulaDescriptionTextBox, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.RateOverrideCheckBox, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.OverriddenRateCalcEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.DutyRateUserControl, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.NetDutyCheckBox, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.CustomNetWeightCalcDropEdit, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.TareSupplementUserControl, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.CalculatedGrossMassCalcDropEdit, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.ConfirmationCodesSeparatorUserControl, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.GrossMassConfirmationCheckBox, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.NetMassConfirmationCheckBox, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.AdditionalUnitConfirmationCheckBox, ControlWidthClass.Auto);
			yield return (InvoiceLineDetailsControlBag.Instance.StatisticalValueConfirmationCheckBox, ControlWidthClass.Medium);
			yield return (InvoiceLineDetailsControlBag.Instance.VATValueConfirmationCheckBox, ControlWidthClass.Medium);
		}
	}

	public void TestCEI_OverridenRateVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		invoiceLine.JI_RateOverride = false;
		AssertEquals("CEI_RateOverride false - CEI_OverriddenRate not visible", false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.OverriddenRateCalcEdit, invoiceLine));

		invoiceLine.JI_RateOverride = true;
		AssertEquals("CEI_RateOverride true - CEI_OverriddenRate visible", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.OverriddenRateCalcEdit, invoiceLine));
	}

	public PanelLayout Layout => layout ?? (layout = new ImportInvoiceLineDetailsLayout().Layout);
	PanelLayout layout;
}
