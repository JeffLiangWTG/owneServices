using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageRegisterLinesSelectionForm))]
	sealed class TemporaryStorageRegisterLinesSelectionFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestModuleControlMainPanel()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var controls = form.Controls.Find("MainPanel", true);
				AssertEquals(1, controls.Length);
				controls = form.Controls.Find("LinesGrid", true);
				AssertEquals(1, controls.Length);
				controls = form.Controls.Find("SelectedLinesGrid", true);
				AssertEquals(1, controls.Length);
			}
		}

		[RequiresSTA]
		public void TestLinesGridLayout()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var controls = form.Controls.Find("LinesGrid", true);
				AssertEquals(1, controls.Length);
				var linesGrid = (ZGrid)controls[0];
				AssertArrayEqualsByElements(new[]
				{
					CusTempStorageSelectableRegLine.Schema.TSDNumber,
					CusTempStorageSelectableRegLine.Schema.ArrivalDate,
					CusTempStorageSelectableRegLine.Schema.OriginalPackagesQty,
					CusTempStorageSelectableRegLine.Schema.PackagesQtyOnHand,
					CusTempStorageSelectableRegLine.Schema.PackageType,
					CusTempStorageSelectableRegLine.Schema.PackageMarks,
					CusTempStorageSelectableRegLine.Schema.PackagesToDraw,
					CusTempStorageSelectableRegLine.Schema.GrossWeightOnHand,
					CusTempStorageSelectableRegLine.Schema.GrossWeightToDraw,
					CusTempStorageSelectableRegLine.Schema.TSDItemNumber,
					CusTempStorageSelectableRegLine.Schema.CommodityCode,
					CusTempStorageSelectableRegLine.Schema.GoodsDescription,
					CusTempStorageSelectableRegLine.Schema.PreviousReferenceType,
					CusTempStorageSelectableRegLine.Schema.PreviousReferenceNumber,
					CusTempStorageSelectableRegLine.Schema.Owner,
				}, linesGrid.Columns.Select(x => x.ColumnName).ToArray());
			}
		}

		[RequiresSTA]
		public void TestSelectedLinesGridLayout()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var controls = form.Controls.Find("SelectedLinesGrid", true);
				AssertEquals(1, controls.Length);
				var selectedLinesGrid = (ZGrid)controls[0];
				AssertArrayEqualsByElements(new[]
				{
					CusTempStorageSelectableRegLine.Schema.TSDNumber,
					CusTempStorageSelectableRegLine.Schema.ArrivalDate,
					CusTempStorageSelectableRegLine.Schema.OriginalPackagesQty,
					CusTempStorageSelectableRegLine.Schema.PackagesQtyOnHand,
					CusTempStorageSelectableRegLine.Schema.PackageType,
					CusTempStorageSelectableRegLine.Schema.PackageMarks,
					CusTempStorageSelectableRegLine.Schema.PackagesToDraw,
					CusTempStorageSelectableRegLine.Schema.GrossWeightOnHand,
					CusTempStorageSelectableRegLine.Schema.GrossWeightToDraw,
					CusTempStorageSelectableRegLine.Schema.TSDItemNumber,
					CusTempStorageSelectableRegLine.Schema.CommodityCode,
					CusTempStorageSelectableRegLine.Schema.GoodsDescription,
					CusTempStorageSelectableRegLine.Schema.PreviousReferenceType,
					CusTempStorageSelectableRegLine.Schema.PreviousReferenceNumber,
					CusTempStorageSelectableRegLine.Schema.Owner,
				}, selectedLinesGrid.Columns.Select(x => x.ColumnName).ToArray());
			}
		}

		[RequiresSTA]
		public void TestSelectButton_Click() => CombineAssertions(() =>
		{
			using (var form = (TemporaryStorageRegisterLinesSelectionForm)GetFormToBash())
			{
				form.Show();
				var header = form.BusinessEntity;

				var controls = form.Controls.Find("SelectButton", true);
				AssertEquals(1, controls.Length);
				var selectButton = (ZButton)controls[0];

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				selectButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		});

		protected override Form GetFormToBashCore()
		{
			var result = new TemporaryStorageRegisterLinesSelectionForm(new CusTempStorageRegLinesSelectionHeader(Factory));
			return result;
		}
	}
}
