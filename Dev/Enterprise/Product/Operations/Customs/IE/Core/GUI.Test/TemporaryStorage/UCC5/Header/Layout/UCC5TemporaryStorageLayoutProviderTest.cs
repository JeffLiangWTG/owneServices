using System;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.UCC5.Testing
{
	[TestedType(typeof(UCC5TemporaryStorageLayoutProvider))]
	sealed class UCC5TemporaryStorageLayoutProviderTest : EU.TemporaryStorage.GUI.Testing.TemporaryStorageLayoutProviderAbstractTest<UCC5TemporaryStorageLayoutProvider>
	{
		protected override Type ExpectedGetTemporaryStorageDetailsLayoutType => typeof(UCC5TemporaryStorageLayout);

		protected override Type ExpectedGetTemporaryStorageBillWithGridLayoutType => typeof(UCC6TemporaryStorageBillWithGridLayout);

		protected override Type ExpectedGetTemporaryStoragePackagesWithGridLayout => typeof(UCC6TemporaryStoragePackagesWithGridLayout);

		protected override Type ExpectedGetTemporaryStoragePackedItemWithGridLayoutType => typeof(UCC6TemporaryStoragePackedItemWithGridLayout);

		protected override Type ExpectedGetTemporaryStoragePreviousDocumentsDetailsLayoutWithGridType => typeof(UCC6TemporaryStoragePreviousDocumentsDetailsLayoutWithGrid);

		protected override Type ExpectedGetTemporaryStorageBillDetailTabLayoutType => typeof(UCC6TemporaryStorageBillDetailTabLayout);

		protected override Type ExpectedGetTemporaryStorageGridColumnLayoutProviderFactoryType => typeof(UCC6TemporaryStorageGridColumnLayoutProviderFactory);
	}
}
