using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStoragePremisesDetailsControlBag))]
	sealed class TempStoragePremisesDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TempStoragePremisesDetailsControlBag.CodeTextBox);
				yield return nameof(TempStoragePremisesDetailsControlBag.DescriptionTextBox);
				yield return nameof(TempStoragePremisesDetailsControlBag.TypeDropEdit);
				yield return nameof(TempStoragePremisesDetailsControlBag.AuthorizationNumberCodeFindBox);
				yield return nameof(TempStoragePremisesDetailsControlBag.AuthorizationOwnerGuidFindBox);
				yield return nameof(TempStoragePremisesDetailsControlBag.PremisesAddressAddressControl);
				yield return nameof(TempStoragePremisesDetailsControlBag.CustomsLocationCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TempStoragePremisesDetailsControlBag.Instance;
	}
}
