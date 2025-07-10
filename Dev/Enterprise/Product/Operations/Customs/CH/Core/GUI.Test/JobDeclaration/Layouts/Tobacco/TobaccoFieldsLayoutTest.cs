using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Test;

[TestedType(typeof(TobaccoFieldsLayout))]
class TobaccoFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestControlVisibility_Export()
	{
		Tobacco.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			AssertEquals("MainGroupDropEdit should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.MainGroupDropEdit, Tobacco));
			AssertEquals("SubGroupDropEdit should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.SubGroupDropEdit, Tobacco));
			AssertEquals("DesignationTextBox should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.DesignationTextBox, Tobacco));
			AssertEquals("SequentialNumberIntEdit should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.SequentialNumberIntEdit, Tobacco));
			AssertEquals("RetailPriceCalcEdit should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.RetailPriceCalcEdit, Tobacco));
			AssertEquals("ReverseNumberTextBox should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.ReverseNumberTextBox, Tobacco));
			AssertEquals("SpecialUnitOfMeasureDropEdit should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.SpecialUnitOfMeasureDropEdit, Tobacco));
			AssertEquals("TobaccoBrandDropEdit should be visible on Export", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.TobaccoBrandDropEdit, Tobacco));
		});
	}

	public void TestControlVisibility_Import()
	{
		Tobacco.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertEquals("MainGroupDropEdit should be visible on Import", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.MainGroupDropEdit, Tobacco));
			AssertEquals("SubGroupDropEdit should be visible on Import", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.SubGroupDropEdit, Tobacco));
			AssertEquals("DesignationTextBox should be visible on Import", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.DesignationTextBox, Tobacco));
			AssertEquals("SequentialNumberIntEdit should be visible on Import", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.SequentialNumberIntEdit, Tobacco));
			AssertEquals("RetailPriceCalcEdit should be visible on Import", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.RetailPriceCalcEdit, Tobacco));
			AssertEquals("ReverseNumberTextBox should be not visible on Import", false, Layout.IsVisible(TobaccoFieldsControlBag.Instance.ReverseNumberTextBox, Tobacco));
			AssertEquals("SpecialUnitOfMesureDropEdit should be not visible on Import", false, Layout.IsVisible(TobaccoFieldsControlBag.Instance.SpecialUnitOfMeasureDropEdit, Tobacco));
			AssertEquals("TobaccoBrandDropEdit should be visible on Import", true, Layout.IsVisible(TobaccoFieldsControlBag.Instance.TobaccoBrandDropEdit, Tobacco));
		});
	}

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
			yield return (TobaccoFieldsControlBag.Instance.MainGroupDropEdit, ControlWidthClass.Auto);
			yield return (TobaccoFieldsControlBag.Instance.SubGroupDropEdit, ControlWidthClass.Auto);
			yield return (TobaccoFieldsControlBag.Instance.DesignationTextBox, ControlWidthClass.Long);
			yield return (TobaccoFieldsControlBag.Instance.SequentialNumberIntEdit, ControlWidthClass.Auto);
			yield return (TobaccoFieldsControlBag.Instance.RetailPriceCalcEdit, ControlWidthClass.Auto);
			yield return (TobaccoFieldsControlBag.Instance.ReverseNumberTextBox, ControlWidthClass.Auto);
			yield return (TobaccoFieldsControlBag.Instance.SpecialUnitOfMeasureDropEdit, ControlWidthClass.Auto);
			yield return (TobaccoFieldsControlBag.Instance.TobaccoBrandDropEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TobaccoFieldsLayoutBuilder<Tobacco>();

	Tobacco GetNewTobacco(BusinessObjectFactory factory)
	{
		return factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().Tobaccos.AddNew();
	}

	Tobacco Tobacco => tobacco ?? (tobacco = GetNewTobacco(Factory));
	Tobacco tobacco;

	public PanelLayout Layout => layout ?? (layout = new TobaccoFieldsLayout().Layout);
	PanelLayout layout;
}
