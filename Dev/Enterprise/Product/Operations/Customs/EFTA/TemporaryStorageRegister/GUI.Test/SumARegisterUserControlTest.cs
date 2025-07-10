using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(SumARegisterUserControl))]
sealed class SumARegisterUserControlTest : TestCaseWithFactory
{
	public void TestLinesGridColumnSize()
	{
		using var control = new SumARegisterUserControl();
		var linesGrid = control.FindSingle<ZGrid>("LinesGrid");
		CombineAssertions(() =>
		{
			AssertEquals("Column Count", 14, linesGrid.ColumnStyles.Count);
			AssertEquals("SRL_LineNumber", 65, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LineNumber).Width);
			AssertEquals("SRL_OwnerReferenceType", 134, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType).Width);
			AssertEquals("SRL_OwnerReference", 149, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReference).Width);
			AssertEquals("SRL_LocationOfGoods", 110, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LocationOfGoods).Width);
			AssertEquals("SRL_GoodsDescription", 150, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsDescription).Width);
			AssertEquals("SRL_LimitDate", 73, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LimitDate).Width);
			AssertEquals("SRL_PackagesRemaining", 126, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_PackagesRemaining).Width);
			AssertEquals("SRL_PackageType", 90, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_PackageType).Width);
			AssertEquals("SRL_CustomsStatus", 98, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_CustomsStatus).Width);
			AssertEquals("SRL_CustodianIdentifier", 99, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_CustodianIdentifier).Width);
			AssertEquals("SRL_CustodianIdentifierBranchNo", 108, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_CustodianIdentifierBranchNo).Width);
			AssertEquals("SRL_GoodsOwnerIdentifier", 134, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsOwnerIdentifier).Width);
			AssertEquals("SRL_GoodsOwnerIdentifierBranchNo", 142, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsOwnerIdentifierBranchNo).Width);
			AssertEquals("SRL_UnionStatus", 85, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_UnionStatus).Width);
		});
	}

	public void TestLinesGridColumnCaption()
	{
		using var control = new SumARegisterUserControl();
		var linesGrid = control.FindSingle<ZGrid>("LinesGrid");
		CombineAssertions(() =>
		{
			AssertEquals("SRL_LineNumber", "Line No.", linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LineNumber).CaptionResourceString.Caption);
		});
	}

	public void TestTransactionGridColumns()
	{
		using var control = new SumARegisterUserControl();
		var transactionGrid = control.FindSingle<ZGrid>("TransactionGrid");
		var columnNames = new[]
		{
			"SRT_BondAmount",
			"SRT_Comments",
			"SRT_GrossWeight",
			"SRT_InternalReferenceNumber",
			"SRT_PackageQty",
			"SRT_Reference",
			"SRT_ReferenceType",
			"SRT_SystemCreateTimeUtc",
			"SRT_SystemCreateUser",
			"SRT_TransactionType",
			"TransactionTypeDescription"
		};

		CombineAssertions(() =>
		{
			AssertEquals("Column Count", columnNames.Length, transactionGrid.ColumnStyles.Count);

			foreach (var columnName in columnNames)
			{
				Assert($"Grid should include {columnName} column", ColumnExists(transactionGrid, columnName));
			}
		});
	}

	static bool ColumnExists(ZGrid grid, string columnName)
	{
		foreach (var columnStyle in grid.ColumnStyles)
		{
			if (columnStyle is ZGridColumnInfo zGridColumnInfo && zGridColumnInfo.ColumnName == columnName)
			{
				return true;
			}
		}
		return false;
	}

	public void TestTransactionGridColumnCaptions()
	{
		using var control = new SumARegisterUserControl();
		var transactionGrid = control.FindSingle<ZGrid>("TransactionGrid");

		CombineAssertions(() =>
		{
			AssertEquals("SRT_TransactionType", "Transaction Type", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType).CaptionResourceString.Caption);
			AssertEquals("TransactionTypeDescription", "Transaction Type Description", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription).CaptionResourceString.Caption);
			AssertEquals("SRT_InternalReferenceNumber", "Internal Reference No.", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber).CaptionResourceString.Caption);
			AssertEquals("SRT_GrossWeight", "Gross Weight in KGs", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_GrossWeight).CaptionResourceString.Caption);
			AssertEquals("SRT_PackageQty", "Package Quantity", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_PackageQty).CaptionResourceString.Caption);
			AssertEquals("SRT_ReferenceType", "Reference Type", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType).CaptionResourceString.Caption);
			AssertEquals("SRT_Reference", "Reference Number", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Reference).CaptionResourceString.Caption);
			AssertEquals("SRT_Comments", "Comments", transactionGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Comments).CaptionResourceString.Caption);
		});
	}

	public void TestOwnerReferenceNormalCasing()
	{
		using var control = new SumARegisterUserControl();
		var linesGrid = control.FindSingle<ZGrid>("LinesGrid");

		AssertEquals("goods description", CharacterCasing.Normal, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReference).CharacterCasing);
	}

	public void TestGoodsDescriptionNormalCasing()
	{
		using var control = new SumARegisterUserControl();
		var linesGrid = control.FindSingle<ZGrid>("LinesGrid");

		AssertEquals("goods description", CharacterCasing.Normal, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsDescription).CharacterCasing);
	}

	public void TestPresentationDateFormat()
	{
		using var control = new SumARegisterUserControl();
		AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, control.FindSingle<ZDateEdit>("PresentationDateEdit").DateTimeFormat);
	}

	public void TestDetailsHeaderDynamicLayoutPanel()
	{
		using var control = new SumARegisterUserControl();
		control.AssertContainsControl<DynamicLayoutPanel>("DetailsHeaderDynamicLayoutPanel");
	}

	public void TestLinesDetailsDynamicLayoutPanel()
	{
		using var control = new SumARegisterUserControl();
		control.AssertContainsControl<DynamicLayoutPanel>("LinesDetailsDynamicLayoutPanel");
	}
}
