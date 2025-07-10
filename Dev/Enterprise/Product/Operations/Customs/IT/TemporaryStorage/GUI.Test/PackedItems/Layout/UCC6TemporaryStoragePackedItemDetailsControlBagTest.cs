using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStoragePackedItemDetailsControlBag))]
sealed class UCC6TemporaryStoragePackedItemDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.RegistrationNoTextBox);
			yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.ReleaseDateEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStoragePackedItemDetailsControlBag.Instance;
}

