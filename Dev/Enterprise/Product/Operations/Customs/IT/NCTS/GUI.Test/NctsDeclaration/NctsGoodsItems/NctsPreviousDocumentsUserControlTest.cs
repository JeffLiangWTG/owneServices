using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsPreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestTariffCodeFindBox()
	{
		using (var nctsPreviousDocumentsUserControl = new NctsPreviousDocumentsUserControl())
		{
			var tariffCodeFindBox = nctsPreviousDocumentsUserControl.FindSingle<TariffFindBox>("TariffCodeFindBox");
			CombineAssertions(() =>
			{
				AssertEquals("TariffCodeFindBox GetCountryCode", Core.Constants.CountryCodes.Italy, tariffCodeFindBox.GetCountryCode());
				AssertEquals("TariffCodeFindBox GetDataGrouping", Core.Constants.CountryCodes.Italy, tariffCodeFindBox.GetDataGrouping());
				AssertEquals("TariffCodeFindBox GetTariffType", Universal.Constants.TariffTypes.Import, tariffCodeFindBox.GetTariffType());
				AssertEquals("TariffCodeFindBox ShouldResize", true, tariffCodeFindBox.ShouldResize);
			});
		}
	}

	public void TestTariffColumnStyle()
	{
		using (var nctsPreviousDocumentsUserControl = new NctsPreviousDocumentsUserControl())
		{
			var previousDocumentsGrid = nctsPreviousDocumentsUserControl.FindSingle<ZGrid>("PreviousDocumentsGrid");
			var tariffColumnStyle = previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.FormattedTariff);
			AssertType<TariffColumnStyleInfo>("TariffColumnStyle type", tariffColumnStyle);
			CombineAssertions(() =>
			{
				var tariffColumnStyleInfo = (TariffColumnStyleInfo)tariffColumnStyle;
				AssertEquals("TariffColumnStyle GetCountryCode", Core.Constants.CountryCodes.Italy, tariffColumnStyleInfo.GetCountryCode());
				AssertEquals("TariffColumnStyle GetDataGrouping", Core.Constants.CountryCodes.Italy, tariffColumnStyleInfo.GetDataGrouping());
				AssertEquals("TariffColumnStyle GetTariffType", Universal.Constants.TariffTypes.Import, tariffColumnStyleInfo.GetTariffType());
			});
		}
	}

	public void TestGridColumnOrder()
	{
		using (var control = new NctsPreviousDocumentsUserControl())
		{
			var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			var previousDocumentGridColumsStyles = previousDocumentsGrid.ColumnStyles.OfType<ZGridColumnInfo>().ToArray();
			CombineAssertions(() =>
			{
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_Procedure, 0);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_Code, 1);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_SubType, 2);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_ReferenceNumber, 3);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_Status, 4);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_ReferenceNumber2, 5);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_DateOfIssue, 6);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_LineNo, 7);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_CustomsOffice, 8);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.FormattedTariff, 9);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_Quantity, 10);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_UnitOfQuantity, 11);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_Quantity2, 12);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_UnitOfQuantity2, 13);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_Quantity3, 14);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_UnitOfQuantity3, 15);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, NctsPreviousDocument.Schema.CSI_PackQty, 16);
			});
		}
	}

	void AssertColumnVisibilyAndIndex(ZGridColumnInfo[] columnsStyleList, ZString columnName, ZInt index)
	{
		var columnStyle = columnsStyleList.FirstOrDefault(x => x.ColumnName == columnName);
		AssertNotNull($"{columnName} not found", columnStyle);
		if (columnStyle != null)
		{
			Assert($"{columnName} is NOT visible", columnStyle.IsVisible);
			AssertEquals($"{columnName} index", index, Array.IndexOf(columnsStyleList, columnStyle));
		}
	}

	public void TestGridColumnSizes()
	{
		using (var control = new NctsPreviousDocumentsUserControl())
		{
			var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Procedure", 80, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Procedure).Width);
				AssertEquals("CSI_Code", 80, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Code).Width);
				AssertEquals("CSI_SubType", 80, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType).Width);
				AssertEquals("CSI_ReferenceNumber", 120, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber).Width);
				AssertEquals("CSI_ReferenceNumber2", 130, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber2).Width);
				AssertEquals("CSI_DateOfIssue", 120, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_DateOfIssue).Width);
				AssertEquals("CSI_LineNo", 80, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_LineNo).Width);
				AssertEquals("CSI_Status", 80, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Status).Width);
				AssertEquals("CSI_CustomsOffice", 100, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_CustomsOffice).Width);
				AssertEquals("FormattedTariff", 106, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.FormattedTariff).Width);
				AssertEquals("CSI_Quantity", 87, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity).Width);
				AssertEquals("CSI_UnitOfQuantity", 78, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).Width);
				AssertEquals("CSI_Quantity2", 87, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity2).Width);
				AssertEquals("CSI_UnitOfQuantity2", 58, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity2).Width);
				AssertEquals("CSI_Quantity3", 87, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity3).Width);
				AssertEquals("CSI_UnitOfQuantity3", 90, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity3).Width);
				AssertEquals("PackageQuantity", 87, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_PackQty).Width);
			});
		}
	}

	public void TestGridColumCharcterCasing()
	{
		using (var control = new NctsPreviousDocumentsUserControl())
		{
			var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Procedure", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Procedure).CharacterCasing);
				AssertEquals("CSI_Code", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Code).CharacterCasing);
				AssertEquals("CSI_SubType", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType).CharacterCasing);
				AssertEquals("CSI_ReferenceNumber", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
				AssertEquals("CSI_ReferenceNumber2", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber2).CharacterCasing);
				AssertEquals("CSI_DateOfIssue", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_DateOfIssue).CharacterCasing);
				AssertEquals("CSI_Status", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Status).CharacterCasing);
				AssertEquals("CSI_Quantity", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity).CharacterCasing);
				AssertEquals("CSI_UnitOfQuantity", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).CharacterCasing);
				AssertEquals("CSI_Quantity2", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity2).CharacterCasing);
				AssertEquals("CSI_UnitOfQuantity2", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity2).CharacterCasing);
				AssertEquals("CSI_CustomsOffice", CharacterCasing.Upper, previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_CustomsOffice).CharacterCasing);
			});
		}
	}

	public void TestQuantityGroupName()
	{
		using (var control = new NctsPreviousDocumentsUserControl())
		{
			var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Quantity", "B624CF7B-86E9-4ACB-9F91-D246ADC0C79D", previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity).GroupName.Key);
				AssertEquals("CSI_UnitOfQuantity", "B624CF7B-86E9-4ACB-9F91-D246ADC0C79D", previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).GroupName.Key);
			});
		}
	}

	public void TestSupplementaryQuantityGroupName()
	{
		using (var control = new NctsPreviousDocumentsUserControl())
		{
			var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Quantity2", "83F0A0D9-69E3-4276-9F15-F717E8CF342D", previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity2).GroupName.Key);
				AssertEquals("CSI_UnitOfQuantity2", "83F0A0D9-69E3-4276-9F15-F717E8CF342D", previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity2).GroupName.Key);
			});
		}
	}

	public void TestGrossMassQuantityGroupName()
	{
		using (var control = new NctsPreviousDocumentsUserControl())
		{
			var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Quantity3", "1B6DCDF6-7592-4ADE-A2B4-E2682FA4955D", previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity3).GroupName.Key);
				AssertEquals("CSI_UnitOfQuantity3", "1B6DCDF6-7592-4ADE-A2B4-E2682FA4955D", previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity3).GroupName.Key);
			});
		}
	}

	public void TestFormFieldLayout()
	{
		using (var control = new NctsPreviousDocumentsUserControl())
		using (var templateControl = new PreviousDocumentsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertFieldLike(templateControl, control, "ProcedureDropEdit");
				AssertFieldLike(templateControl, control, "PrevDocsTypeDropEdit");
				AssertFieldLike(templateControl, control, "PrevDocsClassDropEdit");
				AssertFieldLike(templateControl, control, "PrevDocsReferenceTextBox");
				AssertFieldLike(templateControl, control, "PrevDocsReference2TextBox");
				AssertFieldLike(templateControl, control, "SeriesTextBox");
				AssertFieldLike(templateControl, control, "DateOfIssueDateEdit");
				AssertFieldLike(templateControl, control, "LineNoTextBox");
				AssertFieldLike(templateControl, control, "CustomsOfficeFindBox");
				AssertFieldLike(templateControl, control, "TariffCodeFindBox");
				AssertFieldLike(templateControl, control, "CustomQuantityCalcDropEdit");
				AssertFieldLike(templateControl, control, "SupplementaryQuantityCalcDropEdit");
				AssertFieldLike(templateControl, control, "GrossMassCalcDropEdit");
				AssertFieldLike(templateControl, control, "PackageQuantityTextBox");
			});
		}
	}

	void AssertFieldLike(ZUserControl likeControl, ZUserControl control, string name)
	{
		var templateField = likeControl.FindSingleOrDefault<Control>(name);
		var field = control.FindSingleOrDefault<Control>(name);
		AssertNotNull($"Field not found in template: {name}", templateField);
		AssertNotNull($"Field not found in control: {name}", field);
		if (templateField != null && field != null)
		{
			AssertEquals(templateField.Anchor, field.Anchor);
			if (!templateField.Anchor.HasFlag(AnchorStyles.Right))
			{
				AssertEquals($"{name} width", templateField.Width, field.Width);
			}
		}
	}
}
