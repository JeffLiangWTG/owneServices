using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.CusTempStorage.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(TemporaryStorageReserveTSGoodsForm))]
sealed class TemporaryStorageReserveTSGoodsFormTest : ZFormBasherTest
{
	[RequiresSTA]
	public void TestSortCollection() => CombineAssertions(() =>
	{
		CusTempStorageReserveTSGoodsTestHelper.SetUniversalReferencePackageTypesTestData(Factory);
		Factory.Save();

		var header = CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegHeader(Factory);
		using var form = new TemporaryStorageReserveTSGoodsForm(header);
		{
			form.Show();
			AssertCollectionSort(header, nameof(CusTempStorageReserveTSGoodsRegLine.PackagesToUse));
		}

		header = CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegHeader(Factory, isBulk: true);
		using var form1 = new TemporaryStorageReserveTSGoodsForm(header);
		{
			form.Show();
			AssertCollectionSort(header, nameof(CusTempStorageReserveTSGoodsRegLine.GrossWeightToUse));
		}
	});

	void AssertCollectionSort(CusTempStorageReserveTSGoodsRegHeader header, string propertyName)
	{
		var sortInfo = header.ReserveTSGoodsCollection.SortInformation;
		AssertEquals(propertyName, sortInfo.PropertyName);
		AssertEquals(System.ComponentModel.ListSortDirection.Descending, sortInfo.Direction);
	}

	[RequiresSTA]
	public void TestMakeGridLayout() => CombineAssertions(() =>
	{
		using var form = (TemporaryStorageReserveTSGoodsForm)GetFormToBash();
		{
			form.Show();
			var grid = GetAndAssertControl<ZGrid>(form, "LinesGrid");
			AssertArrayEqualsByElements(
			[
				CusTempStorageReserveTSGoodsRegLine.Schema.TSDNumber,
				CusTempStorageReserveTSGoodsRegLine.Schema.TSDItemNumber,
				CusTempStorageReserveTSGoodsRegLine.Schema.PackageType,
				CusTempStorageReserveTSGoodsRegLine.Schema.Location,
				CusTempStorageReserveTSGoodsRegLine.Schema.Reference,
				CusTempStorageReserveTSGoodsRegLine.Schema.RemainingPackageQty,
				CusTempStorageReserveTSGoodsRegLine.Schema.RemainingGrossWeight,
				CusTempStorageReserveTSGoodsRegLine.Schema.PackagesToUse,
				CusTempStorageReserveTSGoodsRegLine.Schema.GrossWeightToUse,
			], [.. grid.Columns.Select(x => x.ColumnName)]);
		}
	});

	[RequiresSTA]
	public void TestGrossWeightToUseColumnDecimals() => CombineAssertions(() =>
	{
		using var form = (TemporaryStorageReserveTSGoodsForm)GetFormToBash();
		{
			form.Show();
			var grid = GetAndAssertControl<ZGrid>(form, "LinesGrid");
			var col = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(CusTempStorageReserveTSGoodsRegLine.Schema.GrossWeightToUse);
			AssertEquals(CusTempStorageRegLineTransactionSchema.SRT_GrossWeight.Scale, col.Decimals);
		}
	});

	[RequiresSTA]
	public void TestSetGridColumnsReadOnly()
	{
		CusTempStorageReserveTSGoodsTestHelper.SetUniversalReferencePackageTypesTestData(Factory);
		Factory.Save();

		AssertSetGridColumnsReadOnly();
		AssertSetGridColumnsReadOnly(true);
	}

	void AssertSetGridColumnsReadOnly(bool isBulk = false) => CombineAssertions(() =>
	{
		var header = CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegHeader(Factory, isBulk);
		using var form = new TemporaryStorageReserveTSGoodsForm(header);
		{
			form.Show();
			var grid = GetAndAssertControl<ZGrid>(form, "LinesGrid");
			var colPackagesToUse = grid.GetColumnStyle(CusTempStorageReserveTSGoodsRegLine.Schema.PackagesToUse);
			AssertEquals(expected: isBulk, colPackagesToUse.IsReadOnly);

			var colGrossWeightToUse = grid.GetColumnStyle(CusTempStorageReserveTSGoodsRegLine.Schema.GrossWeightToUse);
			AssertEquals(expected: !isBulk, colGrossWeightToUse.IsReadOnly);
		}
	});

	[RequiresSTA]
	public void TestGetTotalQuantities() => CombineAssertions(() =>
	{
		using var form = (TemporaryStorageReserveTSGoodsForm)GetFormToBash();
		{
			form.Show();
			var lblPackages = GetAndAssertControl<ZLabel>(form, "TotalPackagesLabel");
			AssertEquals("Text", "Total Packages: 200", lblPackages.Text);
			var lblGrossWeight = GetAndAssertControl<ZLabel>(form, "TotalGrossWeightLabel");
			AssertEquals("Text", "Total Gross Weight: 1000", lblGrossWeight.Text);
		}
	});

	[RequiresSTA]
	public void TestSaveButton_Click() => CombineAssertions(() =>
	{
		using var form = (TemporaryStorageReserveTSGoodsForm)GetFormToBash();
		{
			form.Show();
			var header = form.BusinessEntity;
			var line = header.ReserveTSGoodsCollection[0];
			var btn = GetAndAssertControl<ZButton>(form, "SaveButton");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			btn.PerformClick();
#if WINZOR
			AssertEquals(DialogResult.Cancel, form.DialogResult);
#else
			AssertEquals(DialogResult.None, form.DialogResult);
#endif
			AssertEquals(2, header.ReserveTSGoodsCollection.Count);
			AssertEquals(expected: true, line.HasErrors);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			line.PackagesToUse = 100;
			btn.PerformClick();
			AssertEquals(DialogResult.OK, form.DialogResult);
			AssertEquals(2, header.ReserveTSGoodsCollection.Count);
			AssertEquals(expected: false, header.ReserveTSGoodsCollection.Any(l => l.HasErrors));
		}
	});

	[RequiresSTA]
	public void TestCancelButton_Click() => CombineAssertions(() =>
	{
		using var form = (TemporaryStorageReserveTSGoodsForm)GetFormToBash();
		{
			form.Show();
			var header = form.BusinessEntity;
			AssertEquals(2, header.ReserveTSGoodsCollection.Count);
			var btn = GetAndAssertControl<ZButton>(form, "CancelButton");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			btn.PerformClick();
			AssertEquals(DialogResult.Cancel, form.DialogResult);
			AssertEquals(0, header.ReserveTSGoodsCollection.Count);
		}
	});

	protected override Form GetFormToBashCore() => new TemporaryStorageReserveTSGoodsForm(CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegHeader(Factory));

	T GetAndAssertControl<T>(ZForm form, string key) where T : class
	{
		var controls = form.Controls.Find(key, searchAllChildren: true);
		AssertEquals(1, controls.Length);
		var ctrl = controls[0] as T;
		AssertNotNull(ctrl);
		return ctrl;
	}
}
