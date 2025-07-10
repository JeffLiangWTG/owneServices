using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class PreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestTariffCodeFindBoxImport()
	{
		AssertTariffCodeFindBox(Customs.Common.EU.EUJobMessageTypeList.Codes.Import);
	}

	public void TestTariffCodeFindBoxExport()
	{
		AssertTariffCodeFindBox(Customs.Common.EU.EUJobMessageTypeList.Codes.Export);
	}

	public void TestTariffColumnStyleImport()
	{
		AssertTariffColumnStyle(Customs.Common.EU.EUJobMessageTypeList.Codes.Import);
	}

	public void TestTariffColumnStyleExport()
	{
		AssertTariffColumnStyle(Customs.Common.EU.EUJobMessageTypeList.Codes.Export);
	}

	public void TestInitializeGridLayoutCore()
	{
		using (var control = new PreviousDocumentsUserControl())
		{
			var previousDocumentsGridControl = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).First();
			var previousDocumentGridColumsStyles = previousDocumentsGridControl.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			CombineAssertions(() =>
			{
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Procedure, 0);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Code, 1);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_SubType, 2);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_ReferenceNumber, 3);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Status, 4);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_ReferenceNumber2, 5);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_DateOfIssue, 6);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_LineNo, 7);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_CustomsOffice, 8);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.FormattedTariff, 9);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Quantity, 10);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_UnitOfQuantity, 11);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Quantity2, 12);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_UnitOfQuantity2, 13);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Quantity3, 14);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_UnitOfQuantity3, 15);
				AssertColumnVisibilyAndIndex(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_PackQty, 16);
			});
		}
	}

	public void TestPackageControlsVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new PreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var packageQuantityTextBox = control.FindSingle<ZCalcEdit>("PackageQuantityTextBox");
			var packageQuantityUCC6CalcDropEdit = control.FindSingle<ZCalcDropEdit>("PackageQuantityUCC6CalcDropEdit");

			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			CombineAssertions("When IsImport", () => AssertPackageControlsVisibility(expectedPackageQuantityTextBox: false, expectedPackageQuantityUCC6CalcDropEdit: true));

			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			CombineAssertions("When IsExport", () => AssertPackageControlsVisibility(expectedPackageQuantityTextBox: true, expectedPackageQuantityUCC6CalcDropEdit: false));

			void AssertPackageControlsVisibility(bool expectedPackageQuantityTextBox, bool expectedPackageQuantityUCC6CalcDropEdit)
			{
				AssertEquals("PackageQuantityTextBox Visible", expectedPackageQuantityTextBox, packageQuantityTextBox.Visible);
				AssertEquals("PackageQuantityUCC6CalcDropEdit Visible", expectedPackageQuantityUCC6CalcDropEdit, packageQuantityUCC6CalcDropEdit.Visible);
			}
		}
	}

	public void TestPackageQuantityUCC6CalcDropEdit()
	{
		using (var control = new PreviousDocumentsUserControl())
		{
			var packageQuantityUCC6CalcDropEdit = control.FindSingle<ZCalcDropEdit>("PackageQuantityUCC6CalcDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("BindToAmount", "FilteredInvoiceLines.PreviousDocuments.CSI_PackQty", packageQuantityUCC6CalcDropEdit.BindToAmount);
				AssertEquals("BindToUnit", "FilteredInvoiceLines.PreviousDocuments.CSI_PackType", packageQuantityUCC6CalcDropEdit.BindToUnit);
			});
		}
	}

	#region Implementation

	void AssertColumnVisibilyAndIndex(ZGridColumnInfo[] columnsStyleList, ZString columnName, ZInt index)
	{
		var columnStyle = columnsStyleList.FirstOrDefault(x => x.ColumnName == columnName);
		AssertNotNull($"{columnName} not null", columnStyle);
		Assert($"{columnName} is visible", columnStyle.IsVisible);
		AssertEquals($"{columnName} index", Array.IndexOf(columnsStyleList, columnStyle), index);
	}

	void AssertTariffCodeFindBox(ZString messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		using (var previousDocumentsUserControl = new PreviousDocumentsUserControl())
		{
			previousDocumentsUserControl.SetDataBinding(declaration, "");
			var tariffCodeFindBox = previousDocumentsUserControl.FindSingle<TariffFindBox>("TariffCodeFindBox");
			CombineAssertions(() =>
			{
				AssertEquals("TariffCodeFindBox GetCountryCode", Core.Constants.CountryCodes.Italy, tariffCodeFindBox.GetCountryCode());
				AssertEquals("TariffCodeFindBox GetDataGrouping", Core.Constants.CountryCodes.Italy, tariffCodeFindBox.GetDataGrouping());
				AssertEquals("TariffCodeFindBox GetTariffType", messageType, tariffCodeFindBox.GetTariffType());
				AssertEquals("TariffCodeFindBox ShouldResize", true, tariffCodeFindBox.ShouldResize);
			});
		}
	}

	void AssertTariffColumnStyle(ZString messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		using (var previousDocumentsUserControl = new PreviousDocumentsUserControl())
		{
			previousDocumentsUserControl.SetDataBinding(declaration, "");
			var previousDocumentsGrid = previousDocumentsUserControl.FindSingle<ZGrid>("PreviousDocumentsGrid");
			var tariffColumnStyle = previousDocumentsGrid.GetColumnStyle(PreviousDocument.Schema.FormattedTariff);
			AssertType<TariffColumnStyleInfo>("TariffColumnStyle type", tariffColumnStyle);
			CombineAssertions(() =>
			{
				var tariffColumnStyleInfo = (TariffColumnStyleInfo)tariffColumnStyle;
				AssertEquals("TariffColumnStyle GetCountryCode", Core.Constants.CountryCodes.Italy, tariffColumnStyleInfo.GetCountryCode());
				AssertEquals("TariffColumnStyle GetDataGrouping", Core.Constants.CountryCodes.Italy, tariffColumnStyleInfo.GetDataGrouping());
				AssertEquals("TariffColumnStyle GetTariffType", messageType, tariffColumnStyleInfo.GetTariffType());
			});
		}
	}

	#endregion
}
