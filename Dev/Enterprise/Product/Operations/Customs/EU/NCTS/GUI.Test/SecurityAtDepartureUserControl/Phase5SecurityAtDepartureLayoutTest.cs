using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5SecurityAtDepartureLayout))]
	sealed class Phase5SecurityAtDepartureLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SecurityAtDepartureLayoutBuilder<Business.NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SecurityAtDepartureControlBag.Instance.PlaceOfLoadingUserControl, ControlWidthClass.Long);
				yield return (SecurityAtDepartureControlBag.Instance.PlaceOfUnloadingUserControl, ControlWidthClass.Long);
				yield return (SecurityAtDepartureControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
