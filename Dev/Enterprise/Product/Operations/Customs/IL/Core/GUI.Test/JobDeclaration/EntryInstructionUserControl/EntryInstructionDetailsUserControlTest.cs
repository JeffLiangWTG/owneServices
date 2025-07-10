using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsUserControl))]
	sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var control = new EntryInstructionDetailsUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryInstructionsGrid", true).Single();
				AssertNotNull(grid);
				var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("Visible grid Column Names ", visibleColumnNames, columns.Where(x => x.IsVisible).Select(x => x.ColumnName));
				AssertContainsExactElementsInExactOrder("None visible grid Column Names ", noneVisibleColumnNames, columns.Where(x => !x.IsVisible).Select(x => x.ColumnName));
			}
		}

		static readonly string[] visibleColumnNames =
			new[]
			{
				"CEI_DateForDuty",
				"CEI_FormattedProcedure",
				"CEI_Description",
				"CEI_NumberOfPackages",
				"CEI_CustomsPackType"
			};

		static readonly string[] noneVisibleColumnNames =
			new[]
			{
				"CEI_AutonomyRegionType",
				"ToWarehouseOrgPK",
				"CEI_OA_Warehouse2",
				"FromWarehouseOrgPK",
				"CEI_OA_Warehouse",
			};

		public void TestPreviousDocumentsControlType()
		{
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>(nameof(EntryInstructionDetailsUserControl.PreviousDocumentsTabPage));
				tabPage.Show();

				var foundUserControl = control.FindSingle<ZDynamicControlCreationUserControl>(nameof(EntryInstructionDetailsUserControl.PreviousDocumentsUserControl));
				AssertEquals("Previous Documents", tabPage.CaptionResourceString.Caption);
				AssertEquals(typeof(LayoutPreviousDocumentsUserControl), foundUserControl.UserControlType);
			}
		}
	}
}
