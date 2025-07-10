using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestRefundRelatedControlsVisiblity()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;
			AssertEquals("RefundReferenceNumbe visible", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RefundReferenceNumberTextBox, invoiceLine));
			AssertEquals("RefundGoodsItemNumber visible", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RefundGoodsItemNumberIntEdit, invoiceLine));
			AssertEquals("RefundReason visible", true, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RefundReasonTextBox, invoiceLine));

			invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.OtherRefunds;
			AssertEquals("RefundReferenceNumbe invisible", false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RefundReferenceNumberTextBox, invoiceLine));
			AssertEquals("RefundGoodsItemNumber invisible", false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RefundGoodsItemNumberIntEdit, invoiceLine));
			AssertEquals("RefundReason invisible", false, Layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RefundReasonTextBox, invoiceLine));
		});
	}

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

	PanelLayout Layout => layout ?? (layout = new ExportInvoiceLineDetailsLayout().Layout);
	PanelLayout layout;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.ProcedureCodeFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.NonCommercialGoodsCheckBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.GoodsReturnedCheckBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.UNDGCodesUserControl, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.CusCodeFindBox, ControlWidthClass.Long);
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
			yield return (InvoiceLineDetailsControlBag.Instance.RefundTypeDropEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.RefundReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.RefundGoodsItemNumberIntEdit, ControlWidthClass.Long);
			yield return (InvoiceLineDetailsControlBag.Instance.RefundReasonTextBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
			yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Medium);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
}
