using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridColumnLayoutExtensionsTest : TestCaseWithFactory
	{
		public void TestApplyGridColumnLayoutGuardClauses()
		{
			using (var grid = new ZGrid())
			{
				AssertExceptionThrown<ArgumentNullException>("When grid is null", () => ZGridColumnLayoutExtensions.ApplyGridColumnLayout(grid: null, new Mock<IGridColumnLayoutProvider>().Object));
				AssertExceptionThrown<ArgumentNullException>("When gridColumnLayoutProvider is null", () => ZGridColumnLayoutExtensions.ApplyGridColumnLayout(grid, gridColumnLayoutProvider: null));
				AssertExceptionThrown<ArgumentNullException>("When gridColumnLayoutProvider.Layout is null", () => ZGridColumnLayoutExtensions.ApplyGridColumnLayout(grid, new Mock<IGridColumnLayoutProvider>().Object));
			}
		}

		public void TestApplyGridColumnLayout()
		{
			var gridColumnLayoutMock = new Mock<IGridColumnLayout>();
			gridColumnLayoutMock.Setup(m => m.Columns).Returns(new[] { new ZTextBoxColumnStyleInfo("00", 100), new ZTextBoxColumnStyleInfo("01", 123) });

			var gridColumnLayoutProviderMock = new Mock<IGridColumnLayoutProvider>();
			gridColumnLayoutProviderMock.Setup(x => x.Layout).Returns(gridColumnLayoutMock.Object);

			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "02" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "03" });

				AssertAvailableColumnNames("PRE-CONDITION: ", grid, new string[] { "02", "03" });
				grid.ApplyGridColumnLayout(gridColumnLayoutProviderMock.Object);
				AssertAvailableColumnNames("POST-CONDITION: ", grid, new string[] { "00", "01" });
			}
		}

		public void TestApplyGridColumnLayout_RemovesOldColumns()
		{
			var gridColumnLayout1Mock = Mock.Of<IGridColumnLayout>(m =>
				m.Columns == new[]
				{
					new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 100),
					new ZCalcEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_Number, 50, 3),
				});

			var gridColumnLayout2Mock = Mock.Of<IGridColumnLayout>(m =>
				m.Columns == new[]
				{
					new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_NVarCharMax, 200),
				});

			var dummyObj = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			using (var zForm = new ZForm(dummyObj))
			{
				var grid = new ZGrid();
				zForm.Controls.Add(grid);
				grid.ApplyGridColumnLayout(Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == gridColumnLayout1Mock));
				grid.SetDataBinding(dummyObj, "Collection");
				zForm.Show();

				CombineAssertions("Initial Layout", () =>
				{
					AssertEquals("Grid Columns Count", 2, grid.Columns.Count);
					AssertEquals("Grid Default Columns Count", 2, grid.DefaultColumns.Count);
					AssertAvailableColumnNames("Column Names", grid, new[] { DummyBizoSchema.Constants.Z0_Description, DummyBizoSchema.Constants.Z0_Number });
				});

				grid.ApplyGridColumnLayout(Mock.Of<IGridColumnLayoutProvider>(p => p.Layout == gridColumnLayout2Mock));
				grid.SetDataBinding(dummyObj, "Collection");

				CombineAssertions("Updated Layout with 2 removed columns and 1 new column", () =>
				{
					AssertEquals("Grid Columns Count", 1, grid.Columns.Count);
					AssertEquals("Grid Default Columns Count", 1, grid.DefaultColumns.Count);
					AssertAvailableColumnNames("Column Names", grid, new[] { DummyBizoSchema.Constants.Z0_NVarCharMax });
				});
			}
		}

		static void AssertAvailableColumnNames(string assertionMessagePrefix, ZGrid grid, string[] expectedAvailableColumnNames)
		{
			var availableColumnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertSequencesEqual(assertionMessagePrefix + "Available Column Names", expectedAvailableColumnNames, availableColumnNames);
		}
	}
}
