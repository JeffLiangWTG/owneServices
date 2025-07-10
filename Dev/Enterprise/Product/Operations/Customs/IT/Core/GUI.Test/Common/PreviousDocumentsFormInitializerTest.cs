using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Common.Testing;

sealed class PreviousDocumentsFormInitializerTest : TestCase
{
	public void TestUserControlNull()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new PreviousDocumentsFormInitializer(null));
	}

	public void TestInitializeGridLayout()
	{
		using (var grid = new ZGrid())
		{
			var form = new PreviousDocumentsFormForTest();
			var initializer = new PreviousDocumentsFormInitializer(form);
			initializer.InitializeGridLayout(grid);

			var columsStyles = grid.ColumnStyles.OfType<ZGridColumnInfo>().ToArray();
			CombineAssertions(() =>
			{
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_Procedure, 0);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_Code, 1);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_SubType, 2);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_ReferenceNumber, 3);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_Status, 4);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_ReferenceNumber2, 5);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_DateOfIssue, 6);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_LineNo, 7);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_CustomsOffice, 8);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.FormattedTariff, 9);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_Quantity, 10);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_UnitOfQuantity, 11);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_Quantity2, 12);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_UnitOfQuantity2, 13);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_Quantity3, 14);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_UnitOfQuantity3, 15);
				AssertColumnIndex(columsStyles, PreviousDocument.Schema.CSI_PackQty, 16);
			});
		}
	}

	void AssertColumnIndex(ZGridColumnInfo[] columnsStyleList, ZString columnName, ZInt index)
	{
		var columnStyle = columnsStyleList.FirstOrDefault(x => x.ColumnName == columnName);
		AssertNotNull($"{columnName} not found", columnStyle);
		if (columnStyle != null)
		{
			AssertEquals($"{columnName} index", index, Array.IndexOf(columnsStyleList, columnStyle));
		}
	}

	public void TestInitializeTariffCodeFindBox()
	{
		using (var tariffFindBox = new TariffFindBox())
		{
			var form = new PreviousDocumentsFormForTest();
			var initializer = new PreviousDocumentsFormInitializer(form);
			initializer.InitializeTariffCodeFindBox(tariffFindBox);
			AssertEquals(Universal.Constants.TariffTypes.HarmonizedSystem, tariffFindBox.GetTariffType());
			AssertEquals(new ZDateTime(2021, 4, 3), tariffFindBox.GetEffectiveDate());
		}
	}

	class PreviousDocumentsFormForTest : IPreviousDocumentsForm
	{
		ZDateTime IPreviousDocumentsForm.GetEffectiveDate() => new ZDateTime(2021, 4, 3);

		ZString IPreviousDocumentsForm.GetUniversalTariffType() => Universal.Constants.TariffTypes.HarmonizedSystem;
	}
}
