using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(InAndOutwardProcessingFieldsLayout))]
sealed class InAndOutwardProcessingFieldsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new InAndOutwardProcessingFieldsLayoutBuilder();

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
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
			yield return (InAndOutwardProcessingFieldsControlBag.Instance.SubTypeDropEdit, ControlWidthClass.Auto);
			yield return (InAndOutwardProcessingFieldsControlBag.Instance.CodeDropEdit, ControlWidthClass.Auto);
			yield return (InAndOutwardProcessingFieldsControlBag.Instance.ProcedureDropEdit, ControlWidthClass.Auto);
			yield return (InAndOutwardProcessingFieldsControlBag.Instance.IssuerTypeDropEdit, ControlWidthClass.Auto);
			yield return (InAndOutwardProcessingFieldsControlBag.Instance.StatusCheckBox, ControlWidthClass.Auto);
			yield return (InAndOutwardProcessingFieldsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Auto);
			yield return (InAndOutwardProcessingFieldsControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);
		}
	}

	public void TestControlsVisibility_Import() => AssertConstrolsVisibility(CHJobMessageTypeList.Codes.Import, true, false);

	public void TestControlsVisibility_Export() => AssertConstrolsVisibility(CHJobMessageTypeList.Codes.Export, false, true);

	public void TestControlsVisibility_ExportDeclarationActivation() => AssertConstrolsVisibility(CHJobMessageTypeList.Codes.ExportDeclarationActivation, false, true);

	void AssertConstrolsVisibility(string messageType, bool subTypeVisibility, bool customsOfficeCodeVisibility) => CombineAssertions($"MessageType={messageType}", () =>
	{
		InvoiceLine.Declaration.JE_MessageType = messageType;
		AssertEquals("SubTypeDropEdit", subTypeVisibility, Layout.IsVisible(InAndOutwardProcessingFieldsControlBag.Instance.SubTypeDropEdit, InvoiceLine));
		AssertEquals("CustomsOfficeCodeFindBox", customsOfficeCodeVisibility, Layout.IsVisible(InAndOutwardProcessingFieldsControlBag.Instance.CustomsOfficeCodeFindBox, InvoiceLine));
	});

	public void TestNoDeclaration() => CombineAssertions(() =>
	{
		AssertNoExceptionThrown("SubTypeDropEdit", () => Layout.GetVisibilityDependencies(InAndOutwardProcessingFieldsControlBag.Instance.SubTypeDropEdit, Factory.New<JobComInvoiceLine>()));
		AssertNoExceptionThrown("CustomsOfficeCodeFindBox", () => Layout.GetVisibilityDependencies(InAndOutwardProcessingFieldsControlBag.Instance.CustomsOfficeCodeFindBox, Factory.New<JobComInvoiceLine>()));
	});

	PanelLayout Layout => layout ??= new InAndOutwardProcessingFieldsLayout().Layout;
	PanelLayout layout;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
	JobComInvoiceLine invoiceLine;
}
