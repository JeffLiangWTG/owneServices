using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(MiscellaneousOptionsControlBag))]
	sealed class MiscellaneousOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscellaneousOptionsControlBag.BranchCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscellaneousOptionsControlBag.Instance;
	}
}
