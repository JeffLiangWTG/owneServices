using System;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing;

[TestedType(typeof(Phase5GoodsLocationFormLayoutProvider))]
sealed class Phase5GoodsLocationFormLayoutProviderTest : EU.GUI.Testing.GoodsLocationFormLayoutProviderAbstractTest<Phase5GoodsLocationFormLayoutProvider>
{
	protected override Type ExpectedGoodsLocationLayout => typeof(Phase5GoodsLocationLayout);
}
