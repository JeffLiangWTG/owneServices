using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ExampleAdditionalControlBag))]
	sealed class ExampleAdditionalControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ExampleAdditionalControlBag.ManifestTypeDropEdit);
				yield return nameof(ExampleAdditionalControlBag.MasterBOLTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExampleAdditionalControlBag.Instance;
	}
}
