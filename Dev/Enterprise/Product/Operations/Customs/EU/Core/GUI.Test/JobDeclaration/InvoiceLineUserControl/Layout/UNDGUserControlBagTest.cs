using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(UNDGUserControlBag))]
	sealed class UNDGUserControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UNDGUserControlBag.DGGuidFindBox);
				yield return nameof(UNDGUserControlBag.DGLinkLabel);
				yield return nameof(UNDGUserControlBag.FlashpointUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UNDGUserControlBag.Instance;
	}
}
