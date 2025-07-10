using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsControlBag))]
	class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryInstructionDetailsControlBag.StyleDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.SubStyleDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.DescriptionTextBox);
				yield return nameof(EntryInstructionDetailsControlBag.AdditionalInfoTextBox);
				yield return nameof(EntryInstructionDetailsControlBag.CPCDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.DateForDutyDateEdit);
				yield return nameof(EntryInstructionDetailsControlBag.ExitDateDateEdit);
				yield return nameof(EntryInstructionDetailsControlBag.AuthorisationNumberDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.PartyConstellationDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
	}
}
