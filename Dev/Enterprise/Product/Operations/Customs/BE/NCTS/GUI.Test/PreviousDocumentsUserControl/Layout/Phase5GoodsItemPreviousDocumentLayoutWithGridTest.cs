using System;
using System.Collections.Generic;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

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

	protected override Type ExpectedGridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentLayoutBuilder<NctsPreviousDocument>();

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (PreviousDocumentControlBag.Instance.ReferenceNumberN785UserControl, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.NumOfPackagesDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.QuantityDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Long);
		}
	}
}
