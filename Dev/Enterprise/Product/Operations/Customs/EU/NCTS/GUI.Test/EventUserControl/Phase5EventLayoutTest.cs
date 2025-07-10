using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5EventLayout))]
	sealed class Phase5EventLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EventLayoutBuilder<Business.NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EventControlBag.Instance.Phase5EventTabUserControl, ControlWidthClass.Auto);
			}
		}
	}
}
