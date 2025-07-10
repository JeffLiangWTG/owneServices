using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ImportPreviousDocumentFieldsLayout))]
sealed class ImportPreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestCaptions()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		var layout = new ImportPreviousDocumentFieldsLayout().Layout;

		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, "Document");
		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, "Number");
		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.QuantityCalcDropEdit, "Net Mass");

		void AssertCaption(ControlReference controlReference, string expectedCaption)
		{
			layout.TryGetCaption(controlReference, previousDocument, out var resourceStringData);
			AssertNotNull($"ResourceData for {controlReference.Name}", resourceStringData);
			AssertEquals($"Caption for {controlReference.Name}", expectedCaption, resourceStringData.Caption);
		}
	}

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
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.Quantity3CalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);
		}
	}
}
