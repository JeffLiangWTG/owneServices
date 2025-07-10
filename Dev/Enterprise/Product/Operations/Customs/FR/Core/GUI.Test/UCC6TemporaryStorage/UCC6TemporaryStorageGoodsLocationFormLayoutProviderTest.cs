using System;
using Enterprise.Customs.FR.GUI.UCC6TemporaryStorage;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageGoodsLocationFormLayoutProvider))]
	sealed class UCC6TemporaryStorageGoodsLocationFormLayoutProviderTest : EU.GUI.Testing.GoodsLocationFormLayoutProviderAbstractTest<UCC6TemporaryStorageGoodsLocationFormLayoutProvider>
	{
		protected override Type ExpectedGoodsLocationLayout => typeof(UCC6TemporaryStorageCusGoodsLocationLayout);
	}
}
