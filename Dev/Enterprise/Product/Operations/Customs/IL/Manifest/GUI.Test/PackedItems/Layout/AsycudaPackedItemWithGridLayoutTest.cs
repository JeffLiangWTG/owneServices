using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(AsycudaPackedItemWithGridLayout))]
	public class AsycudaPackedItemWithGridLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumn;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
		{
			get
			{
				yield return (AsycudaPackedItemDetailsControlBag.Instance.SeqTextBox, ControlWidthClass.Medium);
				yield return (AsycudaPackedItemDetailsControlBag.Instance.TariffCodeFindBox, ControlWidthClass.Long);
				yield return (AsycudaPackedItemDetailsControlBag.Instance.GoodDescriptionTextBox, ControlWidthClass.Long);
				yield return (AsycudaPackedItemDetailsControlBag.Instance.GrossWeightCalcEdit, ControlWidthClass.Long);
				yield return (AsycudaPackedItemDetailsControlBag.Instance.UQDropCodeBox, ControlWidthClass.Long);
				yield return (AsycudaPackedItemDetailsControlBag.Instance.PackStatusDropCodeBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AsycudaPackedItemDetailsBuilder();
		protected override Type ExpectedGridUserControlType => typeof(AsycudaPackedItemGridControl);
	}
}
