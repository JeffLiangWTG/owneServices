using System;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.GUI.Testing
{
	[TestedType(typeof(GoodsLocationFormLayoutProvider))]
	sealed class GoodsLocationFormLayoutProviderTest : GoodsLocationFormLayoutProviderAbstractTest<GoodsLocationFormLayoutProvider>
	{
		protected override Type ExpectedGoodsLocationLayout => typeof(CusGoodsLocationLayout);
	}
}
