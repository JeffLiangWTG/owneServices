using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(SecurityAtDepartureControlBag))]
	class SecurityAtDepartureControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SecurityAtDepartureUserControl.PlaceOfLoadingCodeFindBox);
				yield return nameof(SecurityAtDepartureUserControl.PlaceOfUnloadingCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SecurityAtDepartureControlBag.Instance;
	}
}
