using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5TransportMeansLayout))]
	sealed class Phase5TransportMeansLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new Phase5TransportMeansLayoutBuilder<CusInBondEvent>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Phase5TransportMeansControlBag.Instance.TransportAtDepartureTypeDropEdit, ControlWidthClass.Long);
				yield return (Phase5TransportMeansControlBag.Instance.TransportAtDepartureIDTextBox, ControlWidthClass.Long);
				yield return (Phase5TransportMeansControlBag.Instance.TransportAtDepartureNationalityCodeFindBox, ControlWidthClass.Long);
			}
		}
	}
}
