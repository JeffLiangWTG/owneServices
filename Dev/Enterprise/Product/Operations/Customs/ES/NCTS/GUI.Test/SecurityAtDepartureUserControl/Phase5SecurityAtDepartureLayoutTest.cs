using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5SecurityAtDepartureLayout))]
	sealed class Phase5SecurityAtDepartureLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.SecurityAtDepartureLayoutBuilder<Business.NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SecurityAtDepartureControlBag.Instance.PlaceOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (SecurityAtDepartureControlBag.Instance.PlaceOfUnloadingCodeFindBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.SecurityAtDepartureControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
