using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESH7AsycudaBillControlBag))]
	sealed class ESH7AsycudaBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ESH7AsycudaBillControlBag.DocumentationRequiredTextBox);
				yield return nameof(ESH7AsycudaBillControlBag.G3LocalReferenceNumberTextBox);
				yield return nameof(ESH7AsycudaBillControlBag.G3MovementReferenceNumberTextBox);
				yield return nameof(ESH7AsycudaBillControlBag.H7MovementReferenceNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ESH7AsycudaBillControlBag.Instance;
	}
}
