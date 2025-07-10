using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(UnloadingDetailsControlBag))]
	sealed class UnloadingDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UnloadingDetailsControlBag.UnloadingDateDateEdit);
				yield return nameof(UnloadingDetailsControlBag.UnloadingConformCheckBox);
				yield return nameof(UnloadingDetailsControlBag.StateOfSealsCheckBox);
				yield return nameof(UnloadingDetailsControlBag.UnloadingCompletedCheckBox);
				yield return nameof(UnloadingDetailsControlBag.UnloadingRemarksTextBox);
				yield return nameof(UnloadingDetailsControlBag.OtherThingsToReportTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UnloadingDetailsControlBag.Instance;
	}
}
