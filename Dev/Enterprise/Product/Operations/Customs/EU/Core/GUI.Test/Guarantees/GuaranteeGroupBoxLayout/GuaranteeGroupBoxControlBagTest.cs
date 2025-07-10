using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GuaranteeGroupBoxControlBag))]
	sealed class GuaranteeGroupBoxControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GuaranteeGroupBoxUserControl.BondNumberCodeFindBox);
				yield return nameof(GuaranteeGroupBoxUserControl.AmountCalcDropEdit);
				yield return nameof(GuaranteeGroupBoxUserControl.OverrideCheckBox);
			}
		}
		protected override ControlBag GetControlBagForTesting() => GuaranteeGroupBoxControlBag.Instance;
	}
}
