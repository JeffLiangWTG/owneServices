using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPTemporaryLandingControlBag))]
	sealed class JPTemporaryLandingControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(JPTemporaryLandingControlBag.TemporaryLandingReasonDropEdit);
				yield return nameof(JPTemporaryLandingControlBag.TemporaryLandingPeriodDaysCalcEdit);
				yield return nameof(JPTemporaryLandingControlBag.TemporaryLandingStartDateEdit);
				yield return nameof(JPTemporaryLandingControlBag.TemporaryLandingEndDateEdit);
				yield return nameof(JPTemporaryLandingControlBag.TemporaryLandingBondedTransportCodeDropEdit);
				yield return nameof(JPTemporaryLandingControlBag.GoodsLocationCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => JPTemporaryLandingControlBag.Instance;
	}
}
