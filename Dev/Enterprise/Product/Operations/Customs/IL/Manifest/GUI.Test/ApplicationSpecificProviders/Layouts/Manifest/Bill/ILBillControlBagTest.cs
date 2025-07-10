using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(ILBillControlBag))]
	sealed class ILBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ILBillControlBag.DischargePortCodeFindBox);
				yield return nameof(ILBillControlBag.ConditionDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ILBillControlBag.Instance;
	}
}
