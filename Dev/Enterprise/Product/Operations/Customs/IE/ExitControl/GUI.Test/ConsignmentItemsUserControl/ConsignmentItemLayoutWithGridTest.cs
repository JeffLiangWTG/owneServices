using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	[TestedType(typeof(ConsignmentItemLayoutWithGrid))]
	sealed class ConsignmentItemLayoutWithGridTest : LayoutsAbstractTest
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
				yield return (ConsignmentItemControlBag.Instance.MainUserControl, ControlWidthClass.Auto);
			}
		}
	}
}
