using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

abstract class AdditionalInfosUserControlTest : TestCaseWithFactory
{
	public void TestAdditionalInfosUserControl()
	{
		using (var control = GetNewUserControl())
		{
			control.Show();

			var additionalInfosGrid = (ZGrid)control.Controls.Find("AdditionalInfosGrid", true).First();
			var addInfoTypeCodeDropEdit = control.Controls.Find("AddInfoTypeCodeDropEdit", true).First();
			AssertEquals(true, additionalInfosGrid.Visible);
			AssertEquals(true, addInfoTypeCodeDropEdit.Visible);
		}
	}

	public void TestAdditionalInfosGridColumns()
	{
		using (var control = GetNewUserControl())
		{
			control.Show();

			var additionalInfosGrid = control.FindSingleOrDefault<ZGrid>("AdditionalInfosGrid");
			AssertSequencesEqual("GuaranteesUserControl available Columns", ExpectedColumns, additionalInfosGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}
	}

	public void TestAdditionalInfosGroupBoxLabel()
	{
		using (var control = GetNewUserControl())
		{
			control.Show();

			var additionalInfosGroupBox = control.FindSingleOrDefault<ZGroupBox>("AdditionalInfosGroupBox");
			AssertEquals("For Additional Info tab", "[44] Additional Info", additionalInfosGroupBox.CaptionResourceString.Caption);
		}
	}

	protected virtual IEnumerable<string> ExpectedColumns => new[] { "CSI_Code", "CSI_Description" };

	protected virtual AdditionalInfosUserControl GetNewUserControl() => new AdditionalInfosUserControl();
}
