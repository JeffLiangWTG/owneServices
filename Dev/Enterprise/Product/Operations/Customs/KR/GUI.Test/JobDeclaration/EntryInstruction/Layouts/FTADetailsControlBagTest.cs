using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTADetailsControlBag))]
	sealed class FTADetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(FTADetailsControlBag.LawCodeDropEdit);
				yield return nameof(FTADetailsControlBag.CustomsDisbursementBillDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => FTADetailsControlBag.Instance;
	}
}
