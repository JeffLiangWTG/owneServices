using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePackedItemGridColumnsBagTest : TestCase
{
	public void TestLineNoCalcEditColumn() => AssertCalcEditColumn(ColumnsBag.LineNoCalcEditColumn, "API_LineNo", 80);

	public void TestGrossWeightCalcEditColumn() => AssertCalcEditColumn(ColumnsBag.GrossWeightCalcEditColumn, "API_GrossWeight", 80, "Gross Weight");

	public void TestGrossWeightUnitDropEditColumn() => AssertDropEditColumn(ColumnsBag.GrossWeightUnitDropEditColumn, "API_GrossWeightUQ", 80, "Gross Weight");

	public void TestNetWeightCalcEditColumn() => AssertCalcEditColumn(ColumnsBag.NetWeightCalcEditColumn, "API_NetWeight", 80, "Net Weight");

	public void TestNetWeightUnitDropEditColumn() => AssertDropEditColumn(ColumnsBag.NetWeightUnitDropEditColumn, "API_NetWeightUQ", 80, "Net Weight");

	public void TestCustomsQty2CalcEditColumn() => AssertCalcEditColumn(ColumnsBag.CustomsQty2CalcEditColumn, "API_CustomsQty2", 80, "Supplementary Qty", "Sup. Qty", "Supplementary Quantity");

	public void TestCustomsUnit2DropEditColumn() => AssertDropEditColumn(ColumnsBag.CustomsUnit2DropEditColumn, "API_CustomsUQ2", 80, "Supplementary Qty", "Sup. Qty", "Supplementary Quantity");

	public void TestFormattedTariffColumn()
	{
		AssertNotNull(ColumnsBag.FormattedTariffColumn);
		var columnInfo = ColumnsBag.FormattedTariffColumn.CreateGridColumnInfo() as Universal.GUI.TariffColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "API_FormattedTariff", columnInfo.ColumnName);
		AssertEquals("Width", 80, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertNull("TariffType", columnInfo.TariffType);
		AssertNull("SelectNomenclatureModes", columnInfo.SelectNomenclatureModes);
		Assert("ShowDescriptionFilterOnNonNomenclatureTariffModule", !columnInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule);
	}

	public void TestGoodsDescriptionTextBoxColumn()
	{
		AssertNotNull(ColumnsBag.GoodsDescriptionTextBoxColumn);
		var columnInfo = ColumnsBag.GoodsDescriptionTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "API_GoodsDescription", columnInfo.ColumnName);
		AssertEquals("Width", 120, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
	}

	public void TestChemicalSubstanceCodeFindBoxColumn()
	{
		AssertNotNull(ColumnsBag.ChemicalSubstanceCodeFindBoxColumn);
		var columnInfo = ColumnsBag.ChemicalSubstanceCodeFindBoxColumn.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "API_ChemicalSubstanceCode", columnInfo.ColumnName);
		AssertEquals("Width", 40, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
	}

	UCC6TemporaryStoragePackedItemGridColumnsBag ColumnsBag => UCC6TemporaryStoragePackedItemGridColumnsBag.Instance;

	void AssertCalcEditColumn(IGridColumnReference column, string columnName, int width, string groupNameCaption = null, string groupNameShortCaption = null, string groupNameDescription = null)
	{
		AssertNotNull(column);
		var columnInfo = column.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
		AssertNotNull(columnInfo);

		CombineAssertions($"CalcEditColumn '{columnName}'", () =>
		{
			AssertEquals("ColumnName", columnName, columnInfo.ColumnName);
			AssertEquals("Width", width, columnInfo.Width);
			AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
			AssertNull("BindToDecimalPlaces", columnInfo.BindToDecimalPlaces);
			AssertResourceString(columnInfo.GroupName, groupNameCaption, groupNameShortCaption, groupNameDescription);
		});
	}

	void AssertDropEditColumn(IGridColumnReference column, string columnName, int width, string groupNameCaption = null, string groupNameShortCaption = null, string groupNameDescription = null)
	{
		AssertNotNull(column);
		var columnInfo = column.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);

		CombineAssertions($"CalcEditColumn '{columnName}'", () =>
		{
			AssertEquals("ColumnName", columnName, columnInfo.ColumnName);
			AssertEquals("Width", width, columnInfo.Width);
			AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
			AssertResourceString(columnInfo.GroupName, groupNameCaption, groupNameShortCaption, groupNameDescription);
		});
	}

	void AssertResourceString(ResourceStringData res, string caption = null, string shortCaption = null, string description = null)
	{
		AssertNotNull(res);

		if (caption is not null)
		{
			AssertEquals("GroupNameCaption", caption, res.Caption);
		}

		if (shortCaption is not null)
		{
			AssertEquals("GroupNameShortCaption", shortCaption, res.ShortCaption);
		}

		if (description is not null)
		{
			AssertEquals("GroupNameDescription", description, res.FullDescription);
		}
	}
}
