using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestsSubclassesOf(typeof(ITemporaryStorageLayoutProvider))]
	public abstract class TemporaryStorageLayoutProviderAbstractTest<TLayoutProvider> : TestCaseWithFactory where TLayoutProvider : ITemporaryStorageLayoutProvider, new()
	{
		public void TestGetTemporaryStorageDetailsLayout()
		{
			AssertType(
				"GetTemporaryStorageDetailsLayout() should return object with correct subclass of ITemporaryStorageLayoutProvider",
				ExpectedGetTemporaryStorageDetailsLayoutType,
				new TLayoutProvider().GetTemporaryStorageDetailsLayout()
			);
		}

		public void TestGetTemporaryStorageBillWithGridLayout()
		{
			AssertType(
				"GetTemporaryStorageBillWithGridLayout() should return object with correct subclass of ITemporaryStorageLayoutProvider",
				ExpectedGetTemporaryStorageBillWithGridLayoutType,
				new TLayoutProvider().GetTemporaryStorageBillWithGridLayout()
			);
		}

		public void TestGetTemporaryStoragePackagesWithGridLayout()
		{
			AssertType(
				"GetTemporaryStoragePackagesWithGridLayout() should return object with correct subclass of ITemporaryStorageLayoutProvider",
				ExpectedGetTemporaryStoragePackagesWithGridLayout,
				new TLayoutProvider().GetTemporaryStoragePackagesWithGridLayout()
			);
		}

		public void TestGetTemporaryStoragePackedItemWithGridLayout()
		{
			AssertType(
				"GetTemporaryStoragePackedItemWithGridLayout() should return object with correct subclass of ITemporaryStorageLayoutProvider",
				ExpectedGetTemporaryStoragePackedItemWithGridLayoutType,
				new TLayoutProvider().GetTemporaryStoragePackedItemWithGridLayout()
			);
		}

		public void TestGetTemporaryStorageBillDetailTabLayout()
		{
			AssertType(
				"GetTemporaryStorageBillDetailTabLayout() should return object with correct subclass of ITemporaryStorageLayoutProvider",
				ExpectedGetTemporaryStorageBillDetailTabLayoutType,
				new TLayoutProvider().GetTemporaryStorageBillDetailTabLayout()
			);
		}

		public void TestGetTemporaryStoragePreviousDocumentsDetailsLayoutWithGridLayoutType()
		{
			AssertType(
				"GetTemporaryStoragePreviousDocumentsDetailsLayoutWithGrid() should return object with correct subclass of ITemporaryStorageLayoutProvider",
				ExpectedGetTemporaryStoragePreviousDocumentsDetailsLayoutWithGridType,
				new TLayoutProvider().GetTemporaryStoragePreviousDocumentsDetailsLayoutWithGrid()
			);
		}

		public void TestGetTemporaryStorageGridColumnLayoutProviderFactoryType()
		{
			AssertType(
				"GetTemporaryStorageGridColumnLayoutProviderFactory() should return object with correct subclass of ITemporaryStorageGridColumnLayoutProviderFactory",
				ExpectedGetTemporaryStorageGridColumnLayoutProviderFactoryType,
				new TLayoutProvider().GetTemporaryStorageGridColumnLayoutProviderFactory()
			);
		}

		protected abstract Type ExpectedGetTemporaryStorageDetailsLayoutType { get; }

		protected abstract Type ExpectedGetTemporaryStoragePackagesWithGridLayout { get; }

		protected abstract Type ExpectedGetTemporaryStorageBillWithGridLayoutType { get; }

		protected abstract Type ExpectedGetTemporaryStoragePackedItemWithGridLayoutType { get; }

		protected abstract Type ExpectedGetTemporaryStorageBillDetailTabLayoutType { get; }

		protected abstract Type ExpectedGetTemporaryStoragePreviousDocumentsDetailsLayoutWithGridType { get; }

		protected abstract Type ExpectedGetTemporaryStorageGridColumnLayoutProviderFactoryType { get; }
	}
}
