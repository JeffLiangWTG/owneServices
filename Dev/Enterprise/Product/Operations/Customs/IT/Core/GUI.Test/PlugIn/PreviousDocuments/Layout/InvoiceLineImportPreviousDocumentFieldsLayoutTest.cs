using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(InvoiceLineImportPreviousDocumentFieldsLayout))]
sealed class InvoiceLineImportPreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ProcedureDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.Reference2TextBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.IssueDateEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.QuantityCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);
		}
	}
}
