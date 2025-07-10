using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBagTest : TestCase
{
	public void TestGoodItemIdentifierCalcEditColumn()
	{
		AssertNotNull(ColumnsBag.GoodItemIdentifierCalcEditColumn);
		var columnInfo = ColumnsBag.GoodItemIdentifierCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_LineNo", columnInfo.ColumnName);
		AssertEquals("Width", 110, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
		AssertEquals("MaxValue", 99999m, columnInfo.MaxValue);
	}

	public void TestPackQtyCalcEditColumn()
	{
		AssertNotNull(ColumnsBag.PackQtyCalcEditColumn);
		var columnInfo = ColumnsBag.PackQtyCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_PackQty", columnInfo.ColumnName);
		AssertEquals("Width", 80, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
	}

	public void TestPackTypeDropEditColumn()
	{
		AssertNotNull(ColumnsBag.PackTypeDropEditColumn);
		var columnInfo = ColumnsBag.PackTypeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_PackType", columnInfo.ColumnName);
		AssertEquals("Width", 80, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
	}

	public void TestQuantityCalcEditColumn()
	{
		AssertNotNull(ColumnsBag.QuantityCalcEditColumn);
		var columnInfo = ColumnsBag.QuantityCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_Quantity", columnInfo.ColumnName);
		AssertEquals("Width", 80, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
	}

	public void TestQuantityUnitDropEditColumn()
	{
		AssertNotNull(ColumnsBag.QuantityUnitDropEditColumn);
		var columnInfo = ColumnsBag.QuantityUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_UnitOfQuantity", columnInfo.ColumnName);
		AssertEquals("Width", 80, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
	}

	public void TestReferenceNumberTextBoxColumn()
	{
		AssertNotNull(ColumnsBag.ReferenceNumberTextBoxColumn);
		var columnInfo = ColumnsBag.ReferenceNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_ReferenceNumber", columnInfo.ColumnName);
		AssertEquals("Width", 100, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
	}

	public void TestTypeCodeFindBoxColumn()
	{
		AssertNotNull(ColumnsBag.TypeCodeFindBoxColumn);
		var columnInfo = ColumnsBag.TypeCodeFindBoxColumn.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_Code", columnInfo.ColumnName);
		AssertEquals("Width", 40, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
	}

	public void TestReferenceNumber2TextBoxColumn()
	{
		AssertNotNull(ColumnsBag.ReferenceNumber2TextBoxColumn);
		var columnInfo = ColumnsBag.ReferenceNumber2TextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_ReferenceNumber2", columnInfo.ColumnName);
		AssertEquals("Width", 80, columnInfo.Width);
		AssertEquals("DefaultCollectionIndex", 0, columnInfo.DefaultCollectionIndex);
		AssertEquals("TextAlign", System.Windows.Forms.HorizontalAlignment.Right, columnInfo.TextAlign);
	}

	UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag ColumnsBag => UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnsBag.Instance;
}
