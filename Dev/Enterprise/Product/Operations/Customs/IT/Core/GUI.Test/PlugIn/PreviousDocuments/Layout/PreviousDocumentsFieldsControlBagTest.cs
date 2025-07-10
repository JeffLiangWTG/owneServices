using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(PreviousDocumentsFieldsControlBag))]
sealed class PreviousDocumentsFieldsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(PreviousDocumentsFieldsControlBag.ItemNumberCalcEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => PreviousDocumentsFieldsControlBag.Instance;
}
