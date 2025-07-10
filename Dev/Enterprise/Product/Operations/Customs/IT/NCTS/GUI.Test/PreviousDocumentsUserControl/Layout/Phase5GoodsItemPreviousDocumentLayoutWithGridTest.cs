using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(Phase5GoodsItemPreviousDocumentLayoutWithGrid))]
sealed class Phase5GoodsItemPreviousDocumentLayoutWithGridTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override Type ExpectedGridUserControlType => typeof(EU.NCTS.GUI.GoodsItemPreviousDocumentsGridUserControl);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, ControlWidthClass.CustomWidth);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, ControlWidthClass.CustomWidth);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, ControlWidthClass.CustomWidth);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.QuantityCalcDropEdit, ControlWidthClass.CustomWidth);
			yield return (IT.GUI.PreviousDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.CustomWidth);
			yield return (EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.Reference2TextBox, ControlWidthClass.CustomWidth);
		}
	}
}
