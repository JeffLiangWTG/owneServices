using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusTempStorageRegLinesSelectionHeader))]

	sealed class CusTempStorageRegLinesSelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoad() => CombineAssertions(() =>
		{
			var header = (CusTempStorageRegLinesSelectionHeader)GetNewBusinessObject();
			header.Load([NewStorageRegLineMock("Line1", 5, 10m), NewStorageRegLineMock("Line2", 8, 20m)]);
			AssertEquals("SelectableLines", 2, header.SelectableLines.Count);
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);
			AssertEquals("SelectableLine_1 GoodsDescription", "Line1", header.SelectableLines[0].GoodsDescription);
			AssertEquals("SelectableLine_1 GrossWeightOnHand", 10m, header.SelectableLines[0].GrossWeightOnHand);
			AssertEquals("SelectableLine_1 PackagesQtyOnHand", 5, header.SelectableLines[0].PackagesQtyOnHand);
			AssertEquals("SelectableLine_2 GoodsDescription", "Line2", header.SelectableLines[1].GoodsDescription);
			AssertEquals("SelectableLine_2 GrossWeightOnHand", 20m, header.SelectableLines[1].GrossWeightOnHand);
			AssertEquals("SelectableLine_2 PackagesQtyOnHand", 8, header.SelectableLines[1].PackagesQtyOnHand);
			header.Load([]);
			AssertEquals("SelectableLines", 0, header.SelectableLines.Count);
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);
		});

		public void TestSelectLinesAndLoadAgain() => CombineAssertions(() =>
		{
			var header = (CusTempStorageRegLinesSelectionHeader)GetNewBusinessObject();
			header.Load([NewStorageRegLineMock("Line1", 5, 10m), NewStorageRegLineMock("Line2", 8, 20m)]);
			AssertEquals("SelectableLines", 2, header.SelectableLines.Count);
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);

			header.SelectableLines[0].PackagesToDraw = 3;
			AssertEquals("SelectedLines", 1, header.SelectedLines.Count);
			AssertEquals("SelectedLine[0] must be equal to SelectableLine[0]", header.SelectableLines[0], header.SelectedLines[0]);

			header.SelectableLines[1].GrossWeightToDraw = 5m;
			AssertEquals("SelectedLines", 2, header.SelectedLines.Count);
			AssertEquals("SelectedLine[0] must be equal to SelectableLine[0]", header.SelectableLines[0], header.SelectedLines[0]);
			AssertEquals("SelectedLine[1] must be equal to SelectableLine[1]", header.SelectableLines[1], header.SelectedLines[1]);

			header.SelectableLines[0].PackagesToDraw = 0;
			AssertEquals("SelectedLines", 1, header.SelectedLines.Count);
			AssertEquals("SelectedLine[0] must be equal to SelectableLine[1]", header.SelectableLines[1], header.SelectedLines[0]);

			header.SelectableLines[1].GrossWeightToDraw = 0m;
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);

			header.SelectableLines[0].PackagesToDraw = 3;
			header.SelectableLines[1].GrossWeightToDraw = 5m;
			AssertEquals("SelectedLines", 2, header.SelectedLines.Count);

			header.Load([NewStorageRegLineMock("Line3", 4, 9m)]);
			AssertEquals("SelectableLines", 1, header.SelectableLines.Count);
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);
		});

		public void TestNotAllowedSelection() => CombineAssertions(() =>
		{
			var header = (CusTempStorageRegLinesSelectionHeader)GetNewBusinessObject();
			header.Load([NewStorageRegLineMock("Line1", 5, 10m)]);
			AssertEquals("SelectableLines", 1, header.SelectableLines.Count);
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);

			header.SelectableLines[0].PackagesToDraw = 6;
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);
			AssertHasErrors("PackagesToDraw value not allowed, expected error", header.SelectableLines[0].PackagesToDrawInfo);

			header.SelectableLines[0].GrossWeightToDraw = 11m;
			AssertEquals("SelectedLines", 0, header.SelectedLines.Count);
			AssertHasErrors("GrossWeightToDraw value not allowed, expected error", header.SelectableLines[0].GrossWeightToDrawInfo);
		});

		ICusTempStorageRegLine NewStorageRegLineMock(ZString description, ZInt remainingPackages, ZDecimal remainingGrossWeight)
		{
			var mock = new Mock<ICusTempStorageRegLine>();
			mock.Setup(y => y.GoodsDescription).Returns(description);
			mock.Setup(y => y.PackagesRemainingCalculated).Returns(remainingPackages);
			mock.Setup(y => y.GrossWeightRemainingCalculated).Returns(remainingGrossWeight);
			return mock.Object;
		}

		protected override BusinessObject GetNewBusinessObject() => new CusTempStorageRegLinesSelectionHeader(Factory);
	}
}
