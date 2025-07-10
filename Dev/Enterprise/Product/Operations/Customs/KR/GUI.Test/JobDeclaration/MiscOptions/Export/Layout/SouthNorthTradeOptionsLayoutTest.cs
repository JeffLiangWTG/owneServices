using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SouthNorthTradeOptionsLayout))]
	sealed class SouthNorthTradeOptionsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new MiscOptionsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (MiscOptionsControlBag.Instance.SouthNorthTradeDropEdit, ControlWidthClass.Auto);
				yield return (MiscOptionsControlBag.Instance.SouthNorthTradeAreaDropEdit, ControlWidthClass.Auto);
			}
		}
	}
}
