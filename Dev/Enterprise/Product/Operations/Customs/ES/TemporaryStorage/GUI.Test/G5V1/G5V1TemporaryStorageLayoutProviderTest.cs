using System;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageLayoutProvider))]
	sealed class G5V1TemporaryStorageLayoutProviderTest : EU.TemporaryStorage.GUI.Testing.TemporaryStorageLayoutProviderAbstractTest<G5V1TemporaryStorageLayoutProvider>
	{
		protected override Type ExpectedGetTemporaryStorageDetailsLayoutType => typeof(G5V1TemporaryStorageLayout);

		protected override Type ExpectedGetTemporaryStorageBillWithGridLayoutType => typeof(UCC6TemporaryStorageBillWithGridLayout);

		protected override Type ExpectedGetTemporaryStoragePackagesWithGridLayout => typeof(G5V1TemporaryStoragePackagesWithGridLayout);

		protected override Type ExpectedGetTemporaryStoragePackedItemWithGridLayoutType => typeof(G5V1TemporaryStoragePackedItemWithGridLayout);

		protected override Type ExpectedGetTemporaryStoragePreviousDocumentsDetailsLayoutWithGridType => typeof(G5V1TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid);

		protected override Type ExpectedGetTemporaryStorageBillDetailTabLayoutType => typeof(UCC6TemporaryStorageBillDetailTabLayout);

		protected override Type ExpectedGetTemporaryStorageGridColumnLayoutProviderFactoryType => typeof(G5V1TemporaryStorageGridColumnLayoutProviderFactory);
	}
}
