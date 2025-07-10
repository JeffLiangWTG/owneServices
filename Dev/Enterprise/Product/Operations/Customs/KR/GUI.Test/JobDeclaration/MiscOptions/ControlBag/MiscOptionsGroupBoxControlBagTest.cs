using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsGroupBoxControlBag))]
	sealed class MiscOptionsGroupBoxControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscOptionsGroupBoxControlBag.MiscellaneousGroupBox);
				yield return nameof(MiscOptionsGroupBoxControlBag.ReturnGroupBox);
				yield return nameof(MiscOptionsGroupBoxControlBag.SouthNorthTradeGroupBox);
				yield return nameof(MiscOptionsGroupBoxControlBag.AdditionalCargoGroupBox);
				yield return nameof(MiscOptionsGroupBoxControlBag.PenaltyDeclarationGroupBox);
				yield return nameof(MiscOptionsGroupBoxControlBag.RefundRequestGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscOptionsGroupBoxControlBag.Instance;
	}
}
