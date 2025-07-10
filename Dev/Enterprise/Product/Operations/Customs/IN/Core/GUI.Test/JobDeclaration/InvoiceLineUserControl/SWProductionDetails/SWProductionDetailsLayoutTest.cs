using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SWProductionDetailsLayout))]
sealed class SWProductionDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (commonBag.BatchIDTextBox, ControlWidthClass.Long);
			yield return (commonBag.ManufacturingDateEdit, ControlWidthClass.Auto);
			yield return (commonBag.ExpiryDateEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (commonBag.BatchQuantityDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.BestBeforeDateTimeOffsetEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new SWProductionDetailsLayoutBuilder<SWProduction>();

	SWProductionDetailsControlBag commonBag => SWProductionDetailsControlBag.Instance;

	protected override Type ExpectedGridUserControlType => typeof(SWProductionDetailsUserControl);
}
