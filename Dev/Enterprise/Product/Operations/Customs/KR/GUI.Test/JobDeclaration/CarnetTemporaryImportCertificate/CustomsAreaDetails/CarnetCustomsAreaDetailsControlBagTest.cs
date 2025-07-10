using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CarnetCustomsAreaDetailsControlBag))]
	sealed class CarnetCustomsAreaDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CarnetCustomsAreaDetailsControlBag.Instance.CustomsOfficeCodeFindBox);
				yield return nameof(CarnetCustomsAreaDetailsControlBag.Instance.DepartmentCodeFindBox);
				yield return nameof(CarnetCustomsAreaDetailsControlBag.Instance.BondedAreaCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CarnetCustomsAreaDetailsControlBag.Instance;
	}
}
