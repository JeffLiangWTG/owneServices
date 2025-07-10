using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.IT.GUI.Testing;

abstract class SupportingDocumentsGridInitializerBaseTest : TestCase
{
	public void TestInitializeArgumentCheck()
	{
		AssertExceptionThrown<ArgumentNullException>(() => GetNewSupportingDocumentsGridInitializer(null));
	}

	public void TestInitialize()
	{
		using (var grid = new ZGrid())
		{
			AssertEquals("PRE-CONDITION", 0, grid.ColumnStyles.Count);

			GetNewSupportingDocumentsGridInitializer(grid).Initialize();

			AssertColumns(grid);
		}
	}

	public void TestDoesNotDuplicateColumns()
	{
		using (var grid = new ZGrid())
		{
			AssertEquals("PRE-CONDITION", 0, grid.ColumnStyles.Count);

			GetNewSupportingDocumentsGridInitializer(grid).Initialize();
			AssertEquals("POST-CONDITION 1: grid initialized", ExpectedColumnNumber, grid.ColumnStyles.Count);

			GetNewSupportingDocumentsGridInitializer(grid).Initialize();
			AssertEquals("POST-CONDITION 2: columns are not duplicated", ExpectedColumnNumber, grid.ColumnStyles.Count);
		}
	}

	protected abstract void AssertColumns(ZGrid grid);

	protected abstract SupportingDocumentsGridInitializer GetNewSupportingDocumentsGridInitializer(ZGrid grid);

	protected abstract ZInt ExpectedColumnNumber { get; }
}

sealed class SupportingDocumentsGridInitializerTest : SupportingDocumentsGridInitializerBaseTest
{
	protected override void AssertColumns(ZGrid grid)
	{
		CombineAssertions("POST-CONDITION", () =>
		{
			var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			AssertEquals("ColumnStyles.Count", ExpectedColumnNumber, columnStyles.Length);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Code, 0, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_ReferenceNumber, 1, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Normal);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Quantity, 2, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_UnitOfQuantity, 3, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_YearOfIssue, 4, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_RN_NKCountryCode, 5, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Status, 6, typeof(ZDropEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_DateOfExpiry, 7, typeof(ZDateEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_ReferenceNumber2, 8, typeof(ZTextBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_Value, 9, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_RX_NKCurrency, 10, typeof(ZCodeFindBoxColumnStyle), 100, CharacterCasing.Upper);
			ColumnStyleTestHelper.AssertColumn(columnStyles, SupportingDocument.Schema.CSI_LineNo, 11, typeof(ZCalcEditColumnStyle), 100, CharacterCasing.Upper, maxLength: 4);
		});
	}

	protected override SupportingDocumentsGridInitializer GetNewSupportingDocumentsGridInitializer(ZGrid grid) => new SupportingDocumentsGridInitializer(grid);

	protected override ZInt ExpectedColumnNumber => 12;
}

public static class ColumnStyleTestHelper
{
	public static void AssertColumn(ZGridColumnInfo[] columnStyles, ZString columnName, ZInt expectedIndex, Type type, ZInt width, CharacterCasing characterCasing, int maxLength = 0)
	{
		var columnStyle = columnStyles.SingleOrDefault(x => x.ColumnName == columnName);
		AssertNotNull($"{columnName} not null", columnStyle);
		Assert($"{columnName} is visible", columnStyle.IsVisible);
		AssertEquals($"{columnName} is at specific index", expectedIndex, Array.IndexOf(columnStyles, columnStyle));
		AssertEquals($"{columnName} type", columnStyle.ColumnStyleType, type);
		AssertEquals($"{columnName} width", columnStyle.Width, width);
		AssertEquals($"{columnName} character casing", columnStyle.CharacterCasing, characterCasing);

		if (columnStyle is ZCalcEditColumnStyleInfo calcEditColumnStyleInfo)
		{
			if (maxLength != 0)
			{
				AssertEquals($"{columnName} MaxLength", calcEditColumnStyleInfo.MaxLengthOverride, maxLength);
			}
		}
	}
}
