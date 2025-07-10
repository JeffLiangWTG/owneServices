using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayoutControlBag))]
	sealed class MiscOptionsLayoutControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscOptionsLayoutControlBag.VATAccountNumberDropEdit);
				yield return nameof(MiscOptionsLayoutControlBag.VatPaymentPartyDropEdit);
				yield return nameof(MiscOptionsLayoutControlBag.DutyAccountNumberDropEdit);
				yield return nameof(MiscOptionsLayoutControlBag.VATClaimBackDropEdit);
				yield return nameof(MiscOptionsLayoutControlBag.StatisticStatusDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscOptionsLayoutControlBag.Instance;
	}
}
