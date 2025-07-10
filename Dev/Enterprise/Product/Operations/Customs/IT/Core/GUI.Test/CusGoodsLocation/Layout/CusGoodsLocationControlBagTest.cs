using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(CusGoodsLocationControlBag))]
sealed class CusGoodsLocationControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(CusGoodsLocationControlBag.AdditionalIdentifierDropEdit);
			yield return nameof(CusGoodsLocationControlBag.OrganizationAddressControl);
			yield return nameof(CusGoodsLocationControlBag.OverrideCheckBox);
		}
	}
	protected override ControlBag GetControlBagForTesting() => CusGoodsLocationControlBag.Instance;
}
