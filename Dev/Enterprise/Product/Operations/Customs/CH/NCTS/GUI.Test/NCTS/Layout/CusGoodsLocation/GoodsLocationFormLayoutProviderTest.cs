using System;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(GoodsLocationFormLayoutProvider))]
sealed class GoodsLocationFormLayoutProviderTest : GoodsLocationFormLayoutProviderAbstractTest<GoodsLocationFormLayoutProvider>
{
	protected override Type ExpectedGoodsLocationLayout => typeof(CusGoodsLocationLayout);
}
