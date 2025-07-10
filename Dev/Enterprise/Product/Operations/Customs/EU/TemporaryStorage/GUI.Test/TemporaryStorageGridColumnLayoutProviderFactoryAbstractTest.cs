using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

[TestsSubclassesOf(typeof(ITemporaryStorageGridColumnLayoutProviderFactory))]
public abstract class TemporaryStorageGridColumnLayoutProviderFactoryAbstractTest : TestCaseWithFactory
{
	public void TestGridColumnLayoutProviderTypes()
	{
		CombineAssertions(() =>
		{
			foreach (var testCase in GridColumnLayoutProviderTypeTestCases)
			{
				AssertType(
					$"'{testCase.GridColumnLayoutProviderName}' should return object with correct subclass of IGridColumnLayoutProvider",
					testCase.ExpectedGridColumnLayoutProviderType,
					testCase.GridColumnLayoutProviderFromFactory
				);
			}
		});
	}

	protected abstract ITemporaryStorageGridColumnLayoutProviderFactory GridColumnLayoutProviderFactory { get; }

	protected abstract Type ExpectedCreateTemporaryStorageGridColumnLayoutProviderForBillType { get; }

	protected abstract Type ExpectedCreateTemporaryStorageGridColumnLayoutProviderForPreviousDocumentsDetailsType { get; }

	protected abstract Type ExpectedCreateTemporaryStorageGridColumnLayoutProviderForPackedItemType { get; }

	IEnumerable<GridColumnLayoutProviderTypeTestCase> GridColumnLayoutProviderTypeTestCases
	{
		get
		{
			yield return new GridColumnLayoutProviderTypeTestCase("CreateTemporaryStorageGridColumnLayoutProviderForBill")
			{
				ExpectedGridColumnLayoutProviderType = ExpectedCreateTemporaryStorageGridColumnLayoutProviderForBillType,
				GridColumnLayoutProviderFromFactory = GridColumnLayoutProviderFactory.CreateTemporaryStorageGridColumnLayoutProviderForBill()
			};

			yield return new GridColumnLayoutProviderTypeTestCase("CreateTemporaryStorageGridColumnLayoutProviderForPreviousDocumentsDetails")
			{
				ExpectedGridColumnLayoutProviderType = ExpectedCreateTemporaryStorageGridColumnLayoutProviderForPreviousDocumentsDetailsType,
				GridColumnLayoutProviderFromFactory = GridColumnLayoutProviderFactory.CreateTemporaryStorageGridColumnLayoutProviderForPreviousDocumentsDetails()
			};

			yield return new GridColumnLayoutProviderTypeTestCase("CreateTemporaryStorageGridColumnLayoutProviderForPackedItem")
			{
				ExpectedGridColumnLayoutProviderType = ExpectedCreateTemporaryStorageGridColumnLayoutProviderForPackedItemType,
				GridColumnLayoutProviderFromFactory = GridColumnLayoutProviderFactory.CreateTemporaryStorageGridColumnLayoutProviderForPackedItem()
			};
		}
	}

	sealed class GridColumnLayoutProviderTypeTestCase
	{
		public GridColumnLayoutProviderTypeTestCase(string name)
		{
			GridColumnLayoutProviderName = name;
		}

		public string GridColumnLayoutProviderName { get; }
		public Type ExpectedGridColumnLayoutProviderType { get; set; }
		public IGridColumnLayoutProvider GridColumnLayoutProviderFromFactory { get; set; }
	}
}
