using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsControlBag))]
	sealed class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryInstructionDetailsControlBag.DateForDutyDateEdit);
				yield return nameof(EntryInstructionDetailsControlBag.FormattedProcedureDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.ToWarehouseAddressControl);
				yield return nameof(EntryInstructionDetailsControlBag.AutonomyRegionTypeDropEdit);
				yield return nameof(EntryInstructionDetailsControlBag.FromWarehouseAddressControl);
				yield return nameof(EntryInstructionDetailsControlBag.PackagesQtyCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
	}
}
