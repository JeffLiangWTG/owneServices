using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(SecurityAtDepartureControlBag))]
	sealed class SecurityAtDepartureControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SecurityAtDepartureUserControl.PlaceOfLoadingUserControl);
				yield return nameof(SecurityAtDepartureUserControl.PlaceOfUnloadingUserControl);
				yield return nameof(SecurityAtDepartureUserControl.PlaceOfUnloadingCodeFindBox);
				yield return nameof(SecurityAtDepartureUserControl.SpecificCircumstanceIndicatorDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SecurityAtDepartureControlBag.Instance;
	}
}
