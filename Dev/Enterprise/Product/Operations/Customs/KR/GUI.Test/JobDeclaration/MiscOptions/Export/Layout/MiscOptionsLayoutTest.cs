using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayout))]
	sealed class MiscOptionsLayoutTest : LayoutsAbstractTest
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
				yield return (MiscOptionsGroupBoxControlBag.Instance.MiscellaneousGroupBox, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsGroupBoxControlBag.Instance.ReturnGroupBox, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsGroupBoxControlBag.Instance.SouthNorthTradeGroupBox, ControlWidthClass.LongNoCaption);
				yield return (MiscOptionsGroupBoxControlBag.Instance.AdditionalCargoGroupBox, ControlWidthClass.LongNoCaption);
			}
		}
	}
}
