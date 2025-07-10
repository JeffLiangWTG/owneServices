using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DetailsControlBag))]
	sealed class DetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DetailsControlBag.InstanceForDeclaration.DutyReductionTypeDropEdit);
				yield return nameof(DetailsControlBag.InstanceForDeclaration.SpecificUseCheckBox);
				yield return nameof(DetailsControlBag.InstanceForDeclaration.DutyReductionCodeFindBox);
				yield return nameof(DetailsControlBag.InstanceForDeclaration.InstalmentCodeFindBox);
				yield return nameof(DetailsControlBag.InstanceForDeclaration.RemarkLongTextControl);
				yield return nameof(DetailsControlBag.InstanceForDeclaration.RemarkTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DetailsControlBag.InstanceForDeclaration;
	}
}
