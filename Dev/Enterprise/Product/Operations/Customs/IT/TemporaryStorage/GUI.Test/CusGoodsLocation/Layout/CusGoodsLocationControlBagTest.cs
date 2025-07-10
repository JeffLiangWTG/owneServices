using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(CusGoodsLocationControlBag))]
sealed class CusGoodsLocationControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(CusGoodsLocationControlBag.AdditionalIdentifierDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => CusGoodsLocationControlBag.Instance;
}
