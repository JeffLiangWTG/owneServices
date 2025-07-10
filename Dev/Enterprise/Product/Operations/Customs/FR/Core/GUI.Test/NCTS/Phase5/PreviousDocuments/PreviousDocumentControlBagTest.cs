using System.Collections.Generic;
using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(PreviousDocumentControlBag))]
	sealed class PreviousDocumentControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PreviousDocumentControlBag.ReferenceNumberCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PreviousDocumentControlBag.Instance;
	}
}
