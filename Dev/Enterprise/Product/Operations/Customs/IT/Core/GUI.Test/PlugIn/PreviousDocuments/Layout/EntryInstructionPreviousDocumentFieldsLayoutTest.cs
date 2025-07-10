using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryInstructionPreviousDocumentFieldsLayout))]
sealed class EntryInstructionPreviousDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestCaptions()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		var layout = new EntryInstructionPreviousDocumentFieldsLayout().Layout;

		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, "20CA76C7-65C8-47B7-AB85-E68C45E411B1:\r\nDocument");
		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, "FE28A01A-1830-4350-916A-1C77AC72A4D6:\r\nNumber");
		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.Reference2TextBox, "4F8F483D-CA75-43A4-A0EE-D9055133B23D:\r\nMRN");
		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.QuantityCalcDropEdit, "DBDB6603-36F7-42E9-8E68-AAE8FB776F8B:\r\nMass");
		AssertCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, "42C211EA-01D6-445C-894D-FA291D1F0A9D:\r\nPkg Qty\r\nPkg. Qty\r\nPackage Qty");

		void AssertCaption(ControlReference controlReference, string expectedCaption)
		{
			layout.TryGetCaption(controlReference, previousDocument, out var resourceStringData);
			AssertNotNull($"ResourceData for {controlReference.Name}", resourceStringData);
			AssertEquals($"Caption for {controlReference.Name}", expectedCaption, resourceStringData.ToString());
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
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);
		}
	}
}
