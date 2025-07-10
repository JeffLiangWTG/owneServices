using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterPremisesControlBag))]
	public class TempStorageRegisterPremisesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TempStorageRegisterPremisesControlBag.PremisesCodeTextBox);
				yield return nameof(TempStorageRegisterPremisesControlBag.PremisesDescriptionTextBox);
				yield return nameof(TempStorageRegisterPremisesControlBag.LocationDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TempStorageRegisterPremisesControlBag.Instance;
	}
}
