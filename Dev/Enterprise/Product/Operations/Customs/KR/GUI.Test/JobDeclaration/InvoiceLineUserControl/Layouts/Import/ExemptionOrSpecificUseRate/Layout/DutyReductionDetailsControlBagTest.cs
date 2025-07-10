using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DutyReductionDetailsControlBag))]
	sealed class DutyReductionDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DutyReductionDetailsControlBag.InstanceForDeclaration.GroupNumberDropEdit);
				yield return nameof(DutyReductionDetailsControlBag.InstanceForDeclaration.SeqNumberTextBox);
				yield return nameof(DutyReductionDetailsControlBag.InstanceForDeclaration.ItemNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DutyReductionDetailsControlBag.InstanceForDeclaration;
	}
}
