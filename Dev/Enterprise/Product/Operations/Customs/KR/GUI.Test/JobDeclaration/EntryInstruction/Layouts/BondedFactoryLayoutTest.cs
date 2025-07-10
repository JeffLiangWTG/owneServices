using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(BondedFactoryLayout))]
	sealed class BondedFactoryLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (BondedFactoryControlBag.UseTypeDropEdit, ControlWidthClass.Auto);
				yield return (BondedFactoryControlBag.UseDateEdit, ControlWidthClass.Auto);
			}
		}

		EntryInstructionLayoutControlBag BondedFactoryControlBag => EntryInstructionLayoutControlBag.Instance;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BondedFactoryLayoutBuilder();
	}
}
