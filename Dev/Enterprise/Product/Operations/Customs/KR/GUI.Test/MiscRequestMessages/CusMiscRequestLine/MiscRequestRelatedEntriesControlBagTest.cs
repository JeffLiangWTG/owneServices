using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscRequestRelatedEntriesControlBag))]
	sealed class MiscRequestRelatedEntriesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscRequestRelatedEntriesControlBag.EntryTypeDropEdit);
				yield return nameof(MiscRequestRelatedEntriesControlBag.EntryNumberTextBox);
				yield return nameof(MiscRequestRelatedEntriesControlBag.EntryDetailsTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscRequestRelatedEntriesControlBag.Instance;
	}
}
