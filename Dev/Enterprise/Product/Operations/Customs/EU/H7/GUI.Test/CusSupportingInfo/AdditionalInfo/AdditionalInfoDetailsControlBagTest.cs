using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(AdditionalInfoDetailsControlBag))]
	sealed class AdditionalInfoDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(AdditionalInfoDetailsControlBag.AddInfoTypeCodeDropEdit);
				yield return nameof(AdditionalInfoDetailsControlBag.AddInfoDescriptionTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => AdditionalInfoDetailsControlBag.Instance;
	}
}
