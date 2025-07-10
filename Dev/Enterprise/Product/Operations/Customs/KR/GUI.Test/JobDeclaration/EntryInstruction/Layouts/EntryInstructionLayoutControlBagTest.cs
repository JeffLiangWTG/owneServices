using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EntryInstructionLayoutControlBag))]
	sealed class EntryInstructionLayoutControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryInstructionLayoutControlBag.TotalPackagesCalcEdit);
				yield return nameof(EntryInstructionLayoutControlBag.TotalPackagesUQDropEdit);
				yield return nameof(EntryInstructionLayoutControlBag.AgreedRateDropEdit);
				yield return nameof(EntryInstructionLayoutControlBag.UseTypeDropEdit);
				yield return nameof(EntryInstructionLayoutControlBag.UseDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryInstructionLayoutControlBag.Instance;
	}
}
