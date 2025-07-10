using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TemporaryStorageUserControlBag))]
sealed class TemporaryStorageUserControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TemporaryStorageUserControlBag.AccountNameDropEdit);
			yield return nameof(TemporaryStorageUserControlBag.RepresentativeQualificationDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => TemporaryStorageUserControlBag.Instance;
}
