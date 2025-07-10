using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

[TestedType(typeof(ConsignmentItemUcc6LayoutWithGrid))]
sealed class ConsignmentItemUcc6LayoutWithGridTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override Type ExpectedGridUserControlType => typeof(ConsignmentItemsGridUserControl);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ConsignmentItemLayoutBuilder<Business.CusExitConsignmentItem>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (ConsignmentItemControlBag.Instance.ConsignmentItemPackingDetailsUserControl, ControlWidthClass.LongNoCaption);
			yield return (ConsignmentItemControlBag.Instance.AdditionalDocumentsLabel, ControlWidthClass.LongNoCaption);
			yield return (ConsignmentItemControlBag.Instance.ReportAdditionalDocumentsGridUserControl, ControlWidthClass.LongNoCaption);
		}
	}
}
