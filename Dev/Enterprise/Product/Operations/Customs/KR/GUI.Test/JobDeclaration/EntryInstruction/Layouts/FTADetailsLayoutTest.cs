using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTADetailsLayout))]
	sealed class FTADetailsLayoutTest : LayoutsAbstractTest
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
				yield return (ftaDetailsControlBag.LawCodeDropEdit, ControlWidthClass.Auto);
				yield return (ftaDetailsControlBag.CustomsDisbursementBillDropEdit, ControlWidthClass.Auto);
			}
		}

		FTADetailsControlBag ftaDetailsControlBag => FTADetailsControlBag.Instance;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => new FTADetailsLayoutBuilder();
	}
}
