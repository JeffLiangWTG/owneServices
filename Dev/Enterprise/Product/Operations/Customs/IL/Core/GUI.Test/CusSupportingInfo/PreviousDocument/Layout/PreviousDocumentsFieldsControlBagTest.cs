using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(PreviousDocumentsFieldsControlBag))]
	sealed class PreviousDocumentsFieldsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PreviousDocumentsFieldsControlBag.CodeCodeFindBox);
				yield return nameof(PreviousDocumentsFieldsControlBag.ReferenceTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PreviousDocumentsFieldsControlBag.Instance;
	}
}
