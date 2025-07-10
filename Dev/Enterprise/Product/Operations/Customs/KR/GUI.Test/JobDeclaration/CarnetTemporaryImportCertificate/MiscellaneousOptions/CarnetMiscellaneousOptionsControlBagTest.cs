using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CarnetMiscellaneousOptionsControlBag))]
	sealed class CarnetMiscellaneousOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CarnetMiscellaneousOptionsControlBag.Instance.BranchGuidFindBox);
				yield return nameof(CarnetMiscellaneousOptionsControlBag.Instance.BrokerCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CarnetMiscellaneousOptionsControlBag.Instance;
	}
}
