using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemPackagesAndContainersControlBag))]
	internal class Phase5GoodsItemPackagesAndContainersControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Phase5GoodsItemPackagesAndContainersUserControl.DynamicPackagesUserControl);
				yield return nameof(Phase5GoodsItemPackagesAndContainersUserControl.ContainersUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Phase5GoodsItemPackagesAndContainersControlBag.Instance;
	}
}
