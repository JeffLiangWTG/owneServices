using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GuaranteeGroupBoxWithOverrideLayout))]
	sealed class GuaranteeGroupBoxWithOverrideLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (ControlBagInstance.BondNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (ControlBagInstance.AmountCalcDropEdit, ControlWidthClass.Auto);
				yield return (ControlBagInstance.OverrideCheckBox, ControlWidthClass.Auto);
			}
		}

		GuaranteeGroupBoxControlBag ControlBagInstance => GuaranteeGroupBoxControlBag.Instance;

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new GuaranteeGroupBoxLayoutBuilder<CommonGuarantee>();
	}
}
